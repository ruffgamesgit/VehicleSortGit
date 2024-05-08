using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.Data.Grid;
using GamePlay.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace GamePlay.Components.SortController
{
    public class SortController : MonoBehaviour
    {
        [SerializeField] private GridData gridData;
        [SerializeField] private LevelData _levelData; // Removed Later
        [SerializeField] private int _colorVariety; // Removed Later

        private FillController _fillController;
        private ParkingLot _lastClickedParkingLot;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ConcurrentQueue<(ParkingLot, Seat)> _affectedSortQueue = new();
        private readonly object _lock = new();
        private void Awake()
        {
            _fillController = GetComponent<FillController>();
            InitializeParkingLots();

            if (_colorVariety == 0)
            {
                _colorVariety = 15;
                Debug.LogError(
                    "Color variety count is assigned manually, this should be pre-assigned from the hierarchy");
            }

            _fillController.FillVehicles(gridData.gridGroups, 15, 12,
                12); // Variety , MatchingPassangerCount
        }
 
        private void Start()
        {
            //GameManager.instance.SetColorVarietyCount(_colorVariety);
            //>>>>>>> Stashed changes
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
                            .parkingLotStatus[parkingLotIndex];
                        var parkingLotPosition = new ParkingLotPosition(gridGroupIndex, gridLineIndex, parkingLotIndex);
                        parkingLot.Initialize(isParkingLotInvisible, parkingLotPosition);
                        parkingLot.OnParkingLotClicked += OnParkingLotClicked;

                        parkingLotIndex++;
                    }

                    gridLineIndex++;
                }

                gridGroupIndex++;
            }
        }

        private async void OnParkingLotClicked(object sender, Vehicle arg)
        {
            var parkingLot = (ParkingLot)sender;
            if (parkingLot == null)
            {
                if (_lastClickedParkingLot != null)
                {
                    _lastClickedParkingLot.GetCurrentVehicle()?.SetHighlight(false);
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
                            parkingLot.SetWillOccupied();
                            var vehicle = _lastClickedParkingLot.GetCurrentVehicle();
                            _lastClickedParkingLot.GetCurrentVehicle()?.SetHighlight(false);
                            _lastClickedParkingLot.SetEmpty();
                            _lastClickedParkingLot = null;
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
                                        pLot.MoveAnimation(vehicle, ucs, from, counter == 0, pLot == parkingLot);
                                        counter++;
                                        from = pLot;
                                        await ucs.Task;
                                    }
                                }
                               
                            }

                            parkingLot.Occupy(vehicle, false);
                            var neighbors =
                                parkingLot.FindNeighbors(
                                    gridData.gridGroups[parkingLot.GetParkingLotPosition().GetGridGroupIndex()].lines);
                            neighbors = neighbors.ExtractUnSortableParkingLots();
                            foreach (var neighbor in neighbors)
                            {
                                if (_lastClickedParkingLot == neighbor)
                                {
                                    neighbor.GetCurrentVehicle().SetHighlight(false);
                                    _lastClickedParkingLot = null;
                                }
                            }
                          
                            SortParkingLot(parkingLot, vehicle);
                            return;
                        }
                    }
                }

                _lastClickedParkingLot.GetCurrentVehicle()?.SetHighlight(false);
                _lastClickedParkingLot = null;
            }
            else
            {
                if (!parkingLot.IsEmpty())
                {
                    _lastClickedParkingLot = parkingLot;
                    _lastClickedParkingLot.GetCurrentVehicle()?.SetHighlight(true);
                }
            }
        }

        private void SortParkingLot(object sender, Vehicle arg)
        {
            var parkingLot = (ParkingLot)sender;
            var seats = arg.GetSeats();
            var colorCounts = arg.GetExistingColors();
            if (colorCounts.Count == 0) return;
            
            List<Seat> seatsToQueue = new List<Seat>();
            iterate:
            
 
            var maxColor = colorCounts.GetMaxValue();
            colorCounts.Remove(maxColor);
            
            var seatsToSort = seats.FirstOrDefault(s => !s.IsEmpty() && s.GetPassenger().GetColor() == maxColor);
            if (seatsToSort == null)
            {
                return;
            }
            seatsToQueue.Add(seatsToSort);
            
            if(colorCounts.Count > 0) goto iterate;

            for (int i = seatsToQueue.Count - 1; i >= 0; i--)
            {
                InsertItemToQueue(parkingLot, seatsToQueue[i]);
            }
            
            if(!Monitor.IsEntered(_lock)) SortAffectedParkingLots();
        }


        private async void SortAffectedParkingLots()
        {
            Monitor.Enter(_lock);
            while (_affectedSortQueue.Count > 0)
            {
                await _semaphore.WaitAsync();
                if (_affectedSortQueue.TryDequeue(out var s))
                {
                    if (this == null) break;
                    SortParkingLotAlgorithm(s.Item1, s.Item2);
                }
                else
                {
                    _semaphore.Release();
                }
            }
            Monitor.Exit(_lock);
            await _semaphore.WaitAsync();
            try
            {
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
                                var task = vehicle.SortByType();
                                sortTasks.Add(task);
                            }
                        }
                    }
                }
                await UniTask.WhenAll(sortTasks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async void SortParkingLotAlgorithm(object sender, Seat arg)
        {
            try
            {
                ParkingLot parkingLot = (ParkingLot)sender;


                if (parkingLot == null || arg.IsEmpty())
                {
                    return;
                }

                var parkingLotPosition = parkingLot.GetParkingLotPosition();
                var neighborParkingLots =
                    parkingLot.FindNeighbors(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()].lines);
                neighborParkingLots = neighborParkingLots.ExtractUnSortableParkingLots();

                if (neighborParkingLots.Count == 0)
                {
                    return;
                }

                var countToLookFor = 4 - parkingLot.GetCurrentVehicle().GetSeats().FindAll(seat => !seat.IsEmpty()
                    && seat.GetPassenger().GetColor()
                    == arg.GetPassenger().GetColor()).Count;

                List<Seat> swappingSeats = new List<Seat>();
                List<ParkingLot> swappingNeighbors = new List<ParkingLot>();
                foreach (var neighbor in neighborParkingLots)
                {
                    var matchingSeats = LookForMatchingTypes(neighbor, arg.GetPassenger().GetColor());
                    if (matchingSeats.Count == 0) continue;

                    foreach (var seat in matchingSeats)
                    {
                        if (swappingSeats.Count < countToLookFor)
                        {
                            swappingSeats.Add(seat);
                            swappingNeighbors.Add(neighbor);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (swappingSeats.Count == 0) return;
                if (swappingSeats.Count == 1)
                {
                    bool IsMatchedBySecondNeighbor()
                    {
                        var secondNeighbors = swappingNeighbors[0]
                            .FindNeighbors(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()].lines);
                        secondNeighbors.Remove(parkingLot);
                        if (secondNeighbors.Count != 0)
                        {
                            foreach (var secondNeighborParkingLot in secondNeighbors)
                            {
                                var matchingSlots = LookForMatchingTypes(secondNeighborParkingLot,
                                    arg.GetPassenger().GetColor());
                                if (matchingSlots.Count == 0) continue;

                                EnqueueItem(secondNeighborParkingLot, matchingSlots[0]);
                                return true;
                            }
                        }

                        return false;
                    }

                    if (DoesParkingLotHasAnotherMatchingItem(parkingLot, arg.GetPassenger().GetColor()) &&
                        DoesParkingLotHasAnotherMatchingItem(swappingNeighbors[0],
                            swappingSeats[0].GetPassenger().GetColor()))
                    {
                        IsMatchedBySecondNeighbor();
                        return;
                    }

                    if (IsMatchedBySecondNeighbor())
                        return;
                }

                List<Seat> animateSwappingSeats = new List<Seat>();
                for (var i = 0; i < swappingSeats.Count; i++)
                {
                    var match = swappingSeats[i];
                    var swappingSeat = CheckForSeatToSwap(parkingLot, arg.GetPassenger().GetColor());

                    if (swappingSeat == null)
                    {
                        break;
                    }

                    animateSwappingSeats.Add(swappingSeat);
                    animateSwappingSeats.Add(match);
                    swappingSeat.Swap(match);

                    if (!swappingNeighbors[i].CheckIfCompleted()) // LATER AWAIT ANIMATION
                    {
                        EnqueueItem(swappingNeighbors[i], swappingSeats[i]);
                    }
                }

                await animateSwappingSeats.AnimateSeatChanges();

                parkingLot.CheckIfCompleted(); // LATER ASYNC 
                if(!Monitor.IsEntered(_lock)) SortAffectedParkingLots();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                _semaphore.Release();
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

        private List<Seat> SortSeatsByColorCount(Vehicle vehicle)
        {
            List<Seat> sortedSeats = new List<Seat>();
            if (vehicle.IsAllEmpty()) return sortedSeats;

            var colorCount = vehicle.GetExistingColors();

        Iterate:
            ColorEnum selectedColor = ColorEnum.NONE;
            int selectedColorCount = 0;
            foreach (var color in colorCount)
            {
                if (selectedColor == ColorEnum.NONE)
                {
                    selectedColor = color.Key;
                    selectedColorCount = color.Value;
                    continue;
                }

                if (selectedColorCount < color.Value)
                {
                    selectedColor = color.Key;
                    selectedColorCount = color.Value;
                }
            }

            var vehicleSeats = vehicle.GetSeats();
            if (selectedColor != ColorEnum.NONE)
            {
                colorCount.Remove(selectedColor);
                sortedSeats.AddRange(vehicleSeats.FindAll(s =>
                    !s.IsEmpty() && s.GetPassenger().GetColor() == selectedColor));
                if (colorCount.Count > 0)
                {
                    goto Iterate;
                }
            }


            return sortedSeats;
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
            foreach (var seat in parkingLot.GetCurrentVehicle().GetSeats())
            {
                _affectedSortQueue.Enqueue((parkingLot, seat));
            }
        }

        private void EnqueueItem(ParkingLot parkingLot, Seat seat)
        {
            _affectedSortQueue.Enqueue((parkingLot, seat));
        }

        private void InsertItemToQueue(ParkingLot parkingLot, Seat seat)
        {
            var items = _affectedSortQueue.ToArray();
            _affectedSortQueue.Clear();
            _affectedSortQueue.Enqueue((parkingLot, seat));
            foreach (var item in items)
                _affectedSortQueue.Enqueue(item);
        }
    }
}