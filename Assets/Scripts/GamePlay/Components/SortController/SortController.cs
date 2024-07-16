using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.Data.Grid;
using GamePlay.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Locator;
using Core.Services;
using Core.Services.GamePlay;
using Services.Sound;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Components.SortController
{
    public class SortController : MonoBehaviour
    {
        [SerializeField] private GridData gridData;
        [SerializeField] private List<GarageController> garages;
        private FillController _fillController;
        private ParkingLot _lastClickedParkingLot;
        private IGamePlayService _gamePlayService;
        private ISoundService _soundService;
        private LevelData _levelData;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly SemaphoreSlim _mainSemaphore = new(1, 1);
        private readonly ConcurrentQueue<ParkingLot> _affectedSortQueue = new();


        private void Awake()
        {
            if (ServiceLocator.Instance == null) return;
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _fillController = GetComponent<FillController>();
            _levelData = _gamePlayService.GetCurrentLevelData();
            InitializeParkingLots();
            InitializeFeeders();
            _fillController.FillVehicles(gridData.gridGroups, _levelData.vehicleCount, _levelData.colorVariety,
                _levelData.matchingPassengerCount, garages);
            ActivateFeeders();
        }

        private void ActivateFeeders()
        {
            if (garages.Count == 0) return;
            for (int i = 0; i < garages.Count; i++)
            {
                garages[i].OnVehicleReleased += (obj, pLot) => SortParkingLot(pLot);
                garages[i].Initialize();
            }
        }

        private void InitializeFeeders()
        {
            if (garages.Count == 0) return;

            for (int i = 0; i < garages.Count; i++)
            {
                garages[i].vehicleNeed = _levelData.garageVehicleCounts[i];
            }
        }

        private void InitializeParkingLots()
        {
            int gridGroupIndex = 0;
            foreach (var gridGroup in gridData.gridGroups)
            {
                int gridLineIndex = 0;
                foreach (var line in gridGroup.lines)
                {
                    int parkingLotIndex = 0;
                    foreach (var parkingLot in line.parkingLots)
                    {
                        var isParkingLotInvisible = _levelData.levelDataGridGroups[gridGroupIndex].lines[gridLineIndex]
                            .ParkingLots[parkingLotIndex]
                            .IsInvisible;
                        var isParkingLotObstacle = _levelData.levelDataGridGroups[gridGroupIndex].lines[gridLineIndex]
                            .ParkingLots[parkingLotIndex]
                            .IsObstacle;
                        var isEmptyAtStart = _levelData.levelDataGridGroups[gridGroupIndex].lines[gridLineIndex]
                            .ParkingLots[parkingLotIndex]
                            .IsEmpty;
                        var parkingLotPosition = new ParkingLotPosition(gridGroupIndex, gridLineIndex, parkingLotIndex);
                        parkingLot.Initialize(isParkingLotInvisible, isParkingLotObstacle, isEmptyAtStart,
                            parkingLotPosition);
                        parkingLot.OnParkingLotClicked += OnParkingLotClicked;
                        parkingLot.OnEmptied += (obj, arg) => CheckWaitingVehiclesThatCompleted();

                        parkingLotIndex++;
                    }

                    gridLineIndex++;
                }

                gridGroupIndex++;
            }
        }

        private void HighlightPossibleParkingLots(bool isActivate, ParkingLot selectedParkingLot)
        {
            foreach (var group in gridData.gridGroups)
            {
                foreach (var line in group.lines)
                {
                    foreach (var parkingLot in line.parkingLots)
                    {
                        if (isActivate)
                        {
                            if (parkingLot.IsWalkable())
                            {
                                var path = gridData.FindPath(selectedParkingLot, parkingLot);
                                if (path is { Count: > 0 })
                                {
                                    if (path[^1] == parkingLot)
                                    {
                                        parkingLot.SetPossibleTargetHighLight(true, true);
                                        continue;
                                    }
                                }
                                //parkingLot.SetPossibleTargetHighLight(true,false);
                            }
                        }
                        else
                        {
                            parkingLot.SetPossibleTargetHighLight(false, false);
                        }
                    }
                }
            }
        }

        private async void OnParkingLotClicked(object sender, Vehicle arg)
        {
            if(!_gamePlayService.IsInteractable())return;
            if (_gamePlayService.IsSettingEnabled()) return;
            var parkingLot = (ParkingLot)sender;
            if (parkingLot == null)
            {
                if (_lastClickedParkingLot != null)
                {
                    _lastClickedParkingLot.GetCurrentVehicle()?.SetHighlight(false);
                    HighlightPossibleParkingLots(false, null);
                    _lastClickedParkingLot = null;
                }

                return;
            }

            if (_lastClickedParkingLot != null)
            {
                if (parkingLot.GetCurrentVehicle() == null)
                {
                    var path = gridData.FindPath(_lastClickedParkingLot, parkingLot);
                    if (path is { Count: > 0 })
                    {
                        if (path[^1] == parkingLot)
                        {
                            Debug.Log("Selected lot is EMPTY, can move towards to the lot");
                            Taptic.Medium();
                            _gamePlayService.TriggerOnVehicleMove();
                            _soundService.PlaySound(SoundTypeEnum.ButtonClickedSound);
                            var fromParkingLot = _lastClickedParkingLot;
                            _lastClickedParkingLot = null;
                            parkingLot.SetWillOccupied();
                            var vehicle = fromParkingLot.GetCurrentVehicle();
                            vehicle.StopIdleAnimationSequence();
                            fromParkingLot.GetCurrentVehicle()?.SetHighlight(false);
                            fromParkingLot.GetCurrentVehicle()?.StopIdleAnimationSequence();
                            HighlightPossibleParkingLots(false, null);
                            fromParkingLot.SetEmpty();

                            ParkingLot from = null;

                            int counter = 0;
                            foreach (var pLot in path)
                            {
                                if (pLot != null)
                                {
                                    if (from == null)
                                    {
                                        from = pLot;
                                    }
                                    else
                                    {
                                        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
                                        pLot.OccupyAnimation(gridData, vehicle, ucs, from, counter == 0,
                                            pLot == parkingLot);
                                        counter++;
                                        from = pLot;
                                        await ucs.Task;
                                    }
                                }
                            }

                            parkingLot.Occupy(vehicle, false);
                            var targetNeighbors =
                                parkingLot.FindNeighbors(
                                    gridData.gridGroups[parkingLot.GetParkingLotPosition().GetGridGroupIndex()].lines);
                            targetNeighbors = targetNeighbors.ExtractUnSortableParkingLots(); // WRONG CHANGE LATER 
                            foreach (var neighbor in targetNeighbors)
                            {
                                if (_lastClickedParkingLot == neighbor)
                                {
                                    neighbor.GetCurrentVehicle().SetHighlight(false);
                                    HighlightPossibleParkingLots(false, null);
                                    _lastClickedParkingLot = null;
                                }
                            }

                            SortParkingLot(parkingLot);
                            return;
                        }
                    }
                }

                Taptic.Heavy();
                Debug.Log("Selected lot has another vehicle, cannot move there");
                HighlightPossibleParkingLots(false, null);
                _lastClickedParkingLot.GetCurrentVehicle()?.SetHighlight(false);
                _lastClickedParkingLot.GetCurrentVehicle()?.StopIdleAnimationSequence();
                _lastClickedParkingLot.GetCurrentVehicle()?.OnMovementBlocked();
                _lastClickedParkingLot = null;
            }
            else
            {
                if (parkingLot.IsEmpty()) return;
                Debug.Log("NEW lot selected, and it has vehicle");
                Taptic.Medium();
                _soundService.PlaySound(SoundTypeEnum.ButtonClickedSound);
                _lastClickedParkingLot = parkingLot;
                HighlightPossibleParkingLots(true, _lastClickedParkingLot);
                    
                _lastClickedParkingLot.GetCurrentVehicle()?.SetHighlight(true);
                _lastClickedParkingLot.GetCurrentVehicle()?.StartIdleAnimation();
            }
        }

        private void SortParkingLot(ParkingLot parkingLot)
        {
            var vehicle = parkingLot.GetCurrentVehicle();
            if (vehicle == null) return;
            InsertItemToQueue(parkingLot);
            SortAffectedParkingLots();
        }

        private object _lock = new();

        private async void SortAffectedParkingLots()
        {
            if (!Monitor.TryEnter(_lock))
            {
                return;
            }

            while (_affectedSortQueue.Count > 0)
            {
                await _semaphore.WaitAsync();
                if (_affectedSortQueue.TryDequeue(out var parkingLot))
                {
                    if (this == null)
                    {
                        break;
                    }

                    SortParkingLotAlgorithmNew(parkingLot);
                }
                else
                {
                    _semaphore.Release();
                }
            }

            Monitor.Exit(_lock);

            List<UniTask> sortTasks = new List<UniTask>();
            foreach (var group in gridData.gridGroups)
            {
                foreach (var line in group.lines)
                {
                    foreach (var parkingLot in line.parkingLots)
                    {
                        if (!parkingLot.IsInvisible() && !parkingLot.IsEmpty())
                        {
                            var vehicle = parkingLot.GetCurrentVehicle();
                            var task = vehicle.SortByType(false);
                            sortTasks.Add(task);
                        }
                    }
                }
            }
            await UniTask.WhenAll(sortTasks);
            _gamePlayService.SortCompletedTrigger();
        }

        private bool IsGameFailed()
        {
            foreach (var group in gridData.gridGroups)
            {
                foreach (var line in group.lines)
                {
                    foreach (var parkingLot in line.parkingLots)
                    {
                        if (parkingLot.IsObstacle() || parkingLot.IsInvisible()) continue;
                        if (parkingLot.IsEmpty()) return false;

                        var vehicle = parkingLot.GetCurrentVehicle();
                        foreach (var seat in vehicle.GetSeats())
                        {
                            if (seat.GetPassenger() == null || seat.GetPassenger().GetColor() == ColorEnum.NONE)
                                continue;
                            if (CalculateSortingOptions(parkingLot, seat, parkingLot.FindNeighbors(group.lines)) !=
                                null)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool IsGameCompleted()
        {
            foreach (var group in gridData.gridGroups)
            {
                foreach (var line in group.lines)
                {
                    foreach (var parkingLot in line.parkingLots)
                    {
                        if (parkingLot.IsObstacle() || parkingLot.IsInvisible()) continue;
                        if (parkingLot.IsEmpty()) continue;

                        var vehicle = parkingLot.GetCurrentVehicle();
                        if (!vehicle.IsAllEmpty())
                        {
                            return false;
                        }
                    }
                }
            }

            foreach (var garage in garages)
            {
                if (!garage.IsEmpty())
                {
                    return false;
                }
            }

            return true;
        }

        private async void SortParkingLotAlgorithmNew(ParkingLot parkingLot)
        {
            try
            {
                if (parkingLot == null) return;
                var currentVehicle = parkingLot.GetCurrentVehicle();
                if (currentVehicle == null) return;
                if (currentVehicle.IsCompleted()) return;


                var parkingLotPosition = parkingLot.GetParkingLotPosition();
                var neighborParkingLots =
                    parkingLot.FindNeighbors(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()].lines);
                if (!currentVehicle.IsAllEmpty())
                    foreach (var neighbor in neighborParkingLots)
                    {
                        if (neighbor.GetCurrentVehicle() == null) continue;
                        if (neighbor.GetCurrentVehicle().IsAllEmpty())
                        {
                            EnqueueItem(neighbor);
                        }
                    }

                neighborParkingLots = neighborParkingLots.ExtractUnSortableParkingLots();

                if (neighborParkingLots.Count == 0)
                {
                    return;
                }

                var sortPriorityList = GetSortPriorityList(parkingLot);
                ParkingLotSortData targetSort = null;
                if (sortPriorityList.Count == 0)
                {
                    var oneTypeFromNeighbor = GetOneTypeFromNeighbor(parkingLot, neighborParkingLots);
                    if (oneTypeFromNeighbor == null) return;
                    targetSort = oneTypeFromNeighbor;
                    goto targetIsSet;
                }

                List<ParkingLotSortData> sortOptions = new List<ParkingLotSortData>();

                foreach (var seat in sortPriorityList)
                {
                    var sortOptionData = CalculateSortingOptions(parkingLot, seat, neighborParkingLots);
                    if (sortOptionData != null)
                        sortOptions.Add(sortOptionData);
                }

                if (sortOptions.Count == 0) return;


                foreach (var current in sortOptions)
                {
                    if (targetSort == null)
                    {
                        targetSort = current;
                    }
                    else
                    {
                        var targetSortMatchedCount = 4 - targetSort.CountToLookFor + targetSort.MatchedSeats.Count;
                        var currentMatchedCount = 4 - current.CountToLookFor + current.MatchedSeats.Count;
                        if (currentMatchedCount == 4)
                        {
                            targetSort = current;
                        }
                        else
                        {
                            if (current.SecondNeighborMatch != null && targetSort.SecondNeighborMatch == null)
                            {
                                targetSort = current;
                            }
                            else
                            {
                                if (targetSortMatchedCount < currentMatchedCount)
                                {
                                    targetSort = current;
                                }
                            }
                        }
                    }

                    if (targetSort.CountToLookFor - targetSort.MatchedSeats.Count == 0)
                    {
                        break;
                    }
                }

                targetIsSet:

                if (targetSort != null)
                {
                    if (targetSort.SecondNeighborMatch == null)
                    {
                        await SwapSeats(targetSort);
                    }
                    else
                    {
                        InsertItemToQueue(targetSort.SecondNeighborMatch);
                        SortAffectedParkingLots();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (!isGameFinished && _affectedSortQueue.Count == 0)
                {
                    if (IsGameCompleted())
                    {
                        MoveAllVehiclesOnLevelComplete();
                        isGameFinished = true;
                        _gamePlayService.LevelFinished(LevelFinishedType.Complete);
                    }

                    if (IsGameFailed())
                    {
                        isGameFinished = true;
                        _gamePlayService.LevelFinished(LevelFinishedType.Fail);
                    }
                }

                _semaphore.Release();
            }
        }

        private void MoveAllVehiclesOnLevelComplete()
        {
            foreach (var group in gridData.gridGroups)
            {
                foreach (var line in group.lines)
                {
                    foreach (var parkingLot in line.parkingLots)
                    {
                        var currentVehicle = parkingLot.GetCurrentVehicle();
                        if(currentVehicle != null)
                            currentVehicle.CompletedAnimation(gridData, parkingLot);
                    }
                }
            }
        }
        
        private bool isGameFinished;

        private ParkingLotSortData GetOneTypeFromNeighbor(ParkingLot parkingLot, List<ParkingLot> neighbors)
        {
            var neighborsWithMultipleVariety =
                neighbors.FindAll(pLot => pLot.GetCurrentVehicle().GetExistingColors().Count > 1);

            if (neighborsWithMultipleVariety.Count == 0) return null;

            neighborsWithMultipleVariety.Shuffle();
            ParkingLot targetParkingLot = null;
            List<Seat> targetSeats = null;
            foreach (var neighbor in neighborsWithMultipleVariety)
            {
                var neighborVehicle = neighbor.GetCurrentVehicle();
                var colorCounts = neighborVehicle.GetExistingColors();
                var minColor = colorCounts.GetMinValue();
                if (targetSeats == null || targetParkingLot == null)
                {
                    targetParkingLot = neighbor;
                    targetSeats = neighborVehicle.GetSeats()
                        .FindAll(seat => !seat.IsEmpty() && seat.GetPassenger().GetColor() == minColor);
                    continue;
                }

                if (colorCounts[minColor] == 1)
                {
                    targetParkingLot = neighbor;
                    targetSeats = neighborVehicle.GetSeats()
                        .FindAll(seat => !seat.IsEmpty() && seat.GetPassenger().GetColor() == minColor);
                    break;
                }
            }


            if (targetSeats != null && targetParkingLot != null)
            {
                List<ParkingLot> matchedNeighbors = new();
                foreach (var seat in targetSeats)
                {
                    matchedNeighbors.Add(targetParkingLot);
                }

                var sortData = new ParkingLotSortData()
                {
                    ParkingLot = parkingLot,
                    TargetColor = targetSeats[0].GetPassenger().GetColor(),
                    MatchedNeighbors = matchedNeighbors,
                    MatchedSeats = targetSeats,
                    CountToLookFor = 4 - targetSeats.Count
                };
                return sortData;
            }


            return null;
        }

        private ParkingLotSortData CalculateSortingOptions(ParkingLot parkingLot, Seat arg,
            List<ParkingLot> neighborParkingLots)
        {
            ParkingLotSortData sortOptionData = new()
            {
                TargetColor = arg.GetPassenger() != null ? arg.GetPassenger().GetColor() : ColorEnum.NONE,
                ParkingLot = parkingLot
            };

            var countToLookFor = 4 - parkingLot.GetCurrentVehicle().GetSeats().FindAll(seat => !seat.IsEmpty()
                && seat.GetPassenger().GetColor()
                == arg.GetPassenger().GetColor()).Count;

            sortOptionData.CountToLookFor = countToLookFor;

            List<Seat> matchedSeats = new List<Seat>();
            List<ParkingLot> matchedNeighbors = new List<ParkingLot>();
            foreach (var neighbor in neighborParkingLots)
            {
                var matchingSeats = LookForMatchingTypes(neighbor, arg.GetPassenger().GetColor());
                if (matchingSeats.Count == 0) continue;
                if(matchingSeats.Count == 4) continue;

                foreach (var seat in matchingSeats)
                {
                    if (matchedSeats.Count < countToLookFor)
                    {
                        matchedSeats.Add(seat);
                        matchedNeighbors.Add(neighbor);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            sortOptionData.MatchedSeats = matchedSeats;
            sortOptionData.MatchedNeighbors = matchedNeighbors;

            if (matchedSeats.Count == 0) return null;
            if (matchedSeats.Count < countToLookFor)
            {
                (bool, ParkingLot) IsMatchedBySecondNeighbor()
                {
                    ParkingLot selectedSecondNeighbor = null;
                    int remaining = 4;
                    for (int i = 0; i < matchedNeighbors.Count; i++)
                    {
                        var parkingLotPosition = matchedNeighbors[i].GetParkingLotPosition();
                        var secondNeighbors = matchedNeighbors[i]
                            .FindNeighbors(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()].lines);
                        secondNeighbors = secondNeighbors.ExtractUnSortableParkingLots();
                        var neighborCountToLookFor = 4 - matchedNeighbors[i].GetCurrentVehicle().GetSeats().FindAll(
                            seat => !seat.IsEmpty()
                                    && seat.GetPassenger().GetColor()
                                    == arg.GetPassenger().GetColor()).Count;
                        int remainingCount = neighborCountToLookFor;
                        if (secondNeighbors.Count != 0)
                        {
                            foreach (var secondNeighborParkingLot in secondNeighbors)
                            {
                                var matchingSlots = LookForMatchingTypes(secondNeighborParkingLot,
                                    arg.GetPassenger().GetColor());
                                if (matchingSlots.Count == 0) continue;
                                remainingCount--;
                            }

                            if (remainingCount <= 0)
                            {
                                return (true, matchedNeighbors[i]);
                            }

                            if (remainingCount < countToLookFor)
                            {
                                if (remainingCount < remaining)
                                {
                                    remaining = remainingCount;
                                    selectedSecondNeighbor = matchedNeighbors[i];
                                }
                            }
                        }
                    }

                    if (selectedSecondNeighbor != null)
                    {
                        return (false, selectedSecondNeighbor);
                    }

                    return (false, null);
                }

                var secondNeighborMatch = IsMatchedBySecondNeighbor();

                if (secondNeighborMatch.Item2 != null && secondNeighborMatch.Item1)
                {
                    sortOptionData.SecondNeighborMatch = secondNeighborMatch.Item2;
                }
                else
                {
                    if (DoesParkingLotHasAnotherMatchingItem(parkingLot, arg.GetPassenger().GetColor()))
                    {
                        if (secondNeighborMatch.Item2 != null)
                        {
                            if (DoesParkingLotHasAnotherMatchingItem(secondNeighborMatch.Item2,
                                    arg.GetPassenger().GetColor()))
                                return null;

                            sortOptionData.SecondNeighborMatch = secondNeighborMatch.Item2;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return sortOptionData;
        }

        private async UniTask SwapSeats(ParkingLotSortData sortData)
        {
            List<Seat> animateSwappingSeats = new List<Seat>();
            List<ParkingLot> checkAfterAnimationNeighbors = new List<ParkingLot>();
            for (var i = 0; i < sortData.MatchedSeats.Count; i++)
            {
                var match = sortData.MatchedSeats[i];
                var swappingSeat = CheckForSeatToSwap(sortData.ParkingLot, sortData.TargetColor);

                if (swappingSeat == null)
                {
                    EnqueueItem(sortData.MatchedNeighbors[i]);
                    checkAfterAnimationNeighbors.Clear();
                    break;
                }

                animateSwappingSeats.Add(swappingSeat);
                animateSwappingSeats.Add(match);
                swappingSeat.Swap(match);

                checkAfterAnimationNeighbors.Add(sortData.MatchedNeighbors[i]);
            }

            await animateSwappingSeats.AnimateSeatChanges(false);

            if (!sortData.ParkingLot.CheckIfCompleted(gridData))
            {
                InsertItemToQueue(sortData.ParkingLot);
            }

            foreach (var neighbor in checkAfterAnimationNeighbors)
            {
                if (!neighbor.CheckIfCompleted(gridData))
                {
                    EnqueueItem(neighbor);
                }
            }


            SortAffectedParkingLots();
        }

        private void CheckWaitingVehiclesThatCompleted()
        {
            foreach (var group in gridData.gridGroups)
            {
                foreach (var line in group.lines)
                {
                    foreach (var parkingLot in line.parkingLots)
                    {
                        if (parkingLot.IsWalkable()) continue;
                        var vehicle = parkingLot.GetCurrentVehicle();
                        if (vehicle == null) continue;
                        if (vehicle.HasEmptySeat()) continue;

                        parkingLot.CheckIfCompleted(gridData);
                    }
                }
            }

            if (_lastClickedParkingLot != null)
            {
                HighlightPossibleParkingLots(true, _lastClickedParkingLot);
            }
        }

        private List<Seat> LookForMatchingTypes(ParkingLot neighbor, ColorEnum color)
        {
            var seats = new List<Seat>();
            var vehicle = neighbor.GetCurrentVehicle();
            if (vehicle != null)
            {
                foreach (var seat in vehicle.GetSeats())
                {
                    if (!seat.IsEmpty() && seat.GetPassenger().GetColor() == color)
                        seats.Add(seat);
                }
            }


            return seats;
        }

        private bool DoesParkingLotHasAnotherMatchingItem(ParkingLot parkingLot, ColorEnum color)
        {
            var otherItems = parkingLot.GetCurrentVehicle().GetSeats()
                .FindAll(seat => !seat.IsEmpty() && seat.GetPassenger().GetColor() != color);

            if (otherItems.Count >= 3)
            {
                foreach (var item in otherItems)
                {
                    var matchingItems = parkingLot.GetCurrentVehicle().GetSeats()
                        .FindAll(seat => !seat.IsEmpty() && seat.GetPassenger().GetColor()
                            == item.GetPassenger().GetColor());
                    if (matchingItems.Count >= 3)
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        private Seat CheckForSeatToSwap(ParkingLot parkingLot, ColorEnum color)
        {
            var emptySeats = parkingLot.GetCurrentVehicle().GetSeats()
                .FindAll(seat => seat.IsEmpty());

            if (emptySeats.Count > 0)
            {
                return emptySeats[0];
            }

            var otherItems = parkingLot.GetCurrentVehicle().GetSeats()
                .FindAll(seat => !seat.IsEmpty() && seat.GetPassenger().GetColor() != color);

            if (otherItems.Count == 1)
            {
                return otherItems[0];
            }

            if (otherItems.Count >= 2)
            {
                foreach (var item in otherItems)
                {
                    var matchingItems = parkingLot.GetCurrentVehicle().GetSeats()
                        .FindAll(seat =>
                            !seat.IsEmpty() && seat.GetPassenger().GetColor() == item.GetPassenger().GetColor());
                    if (matchingItems.Count == 1)
                    {
                        return item;
                    }
                }
            }

            return otherItems[0];
        }

        private void EnqueueItem(ParkingLot parkingLot)
        {
            _affectedSortQueue.Enqueue((parkingLot));
        }

        private void InsertItemToQueue(ParkingLot parkingLot)
        {
            var items = _affectedSortQueue.ToArray();
            _affectedSortQueue.Clear();
            _affectedSortQueue.Enqueue(parkingLot);
            foreach (var item in items)
                _affectedSortQueue.Enqueue(item);
        }

        private List<Seat> GetSortPriorityList(ParkingLot parkingLot)
        {
            List<Seat> seatsToQueue = new List<Seat>();
            var vehicle = parkingLot.GetCurrentVehicle();
            if (vehicle == null) return seatsToQueue;

            var colorCounts = vehicle.GetExistingColors();
            if (colorCounts.Count == 0) return seatsToQueue;

            var seats = vehicle.GetSeats();


            iterate:

            var maxColor = colorCounts.GetMaxValue();
            colorCounts.Remove(maxColor);

            var seatsToSort = seats.FirstOrDefault(s => !s.IsEmpty() && s.GetPassenger().GetColor() == maxColor);
            if (seatsToSort == null)
            {
                return seatsToQueue;
            }

            seatsToQueue.Add(seatsToSort);

            if (colorCounts.Count > 0) goto iterate;

            return seatsToQueue;
        }

        private void OnDestroy()
        {
            for (int i = 0; i < garages.Count; i++)
            {
                garages[i].OnVehicleReleased -= (obj, pLot) => SortParkingLot(pLot);
            }

            foreach (var gridGroup in gridData.gridGroups)
            {
                foreach (var line in gridGroup.lines)
                {
                    foreach (var parkingLot in line.parkingLots)
                    {
                        parkingLot.OnParkingLotClicked -= OnParkingLotClicked;
                        parkingLot.OnEmptied -= (obj, arg) => CheckWaitingVehiclesThatCompleted();
                    }
                }
            }
        }

        private class ParkingLotSortData
        {
            public ParkingLot ParkingLot = null;
            public ColorEnum TargetColor = default;
            public int CountToLookFor = 4;
            public List<Seat> MatchedSeats = null;
            public List<ParkingLot> MatchedNeighbors = null;
            public ParkingLot SecondNeighborMatch;
        }
    }
}