using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.Data.Grid;
using GamePlay.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//<<<<<<< Updated upstream
//=======
//>>>>>>> Stashed changes
using UnityEngine;

namespace GamePlay.Components.SortController
{
    public class SortController : MonoBehaviour
    {
        [SerializeField] private GridData gridData;
        private FillController _fillController;
        [SerializeField] private LevelData _levelData;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ConcurrentQueue<(ParkingLot, Seat)> _affectedSortQueue = new();
        private ParkingLot _lastClickedParkingLot;
        [SerializeField] int _colorVariety;
        private void Awake()
        {
            _fillController = GetComponent<FillController>();
            InitializeParkingLots();
            //<<<<<<< Updated upstream

            //    _fillController.FillVehicles(gridData.gridGroups, 14, _colorVariety, 12); // Variety , MatchingPassangerCount
            //=======

            if (_colorVariety == 0)
            {
                _colorVariety = 15;
                Debug.LogError("Color variety count is assigned manually, this should be pre-assigned from the hierarchy");
            }

            _fillController.FillVehicles(gridData.gridGroups, 23, _colorVariety, 12); // Variety , MatchingPassangerCount

        }

        private void Start()
        {
            GameManager.instance.SetColorVarietyCount(_colorVariety);
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
            if(parkingLot == null)
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
                                        pLot.MoveAnimation(vehicle,ucs, from);
                                        from = pLot;
                                        await ucs.Task;
                                    }
                                }
                            }
                            
                            parkingLot.Occupy(vehicle,false);
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
            var seatsToSort = SortSeatsByColorCount(arg);
            if (seatsToSort.Count == 0) return;
            foreach (var seat in seatsToSort)
            {
                InsertItemToQueue(parkingLot, seat);
            }

            SortAffectedParkingLots();
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

                if (parkingLot.CheckIfCompleted()) // LATER AWAIT ANIMATION
                {
                    EnqueueItem(parkingLot);
                }

                SortAffectedParkingLots();
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

        private async void SortAffectedParkingLots()
        {
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
                //<<<<<<< Updated upstream

                //=======
                //>>>>>>> Stashed changes
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
                sortedSeats.Add(vehicleSeats.First(s => !s.IsEmpty() && s.GetPassenger().GetColor() == selectedColor));
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