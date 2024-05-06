using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using GamePlay.Data.Grid;
using GamePlay.Extension;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

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

        private void Awake()
        {
            _fillController = GetComponent<FillController>();
            InitializeParkingLots();
            _fillController.FillVehicles(gridData.gridGroups, 14, 12, 12); // Variety , MatchingPassangerCount
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
            if (_lastClickedParkingLot != null)
            {
                if (parkingLot.GetCurrentVehicle() == null)
                {
                    var path = gridData.FindPath(_lastClickedParkingLot, parkingLot);

                    if (path is { Count: > 0 })
                    {
                        if (path[^1] == parkingLot)
                        {
                            var vehicle = _lastClickedParkingLot.GetCurrentVehicle();
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
                                        from.SetEmpty();
                                        pLot.Occupy(vehicle, from, true, ucs);
                                        from = pLot;
                                        await ucs.Task;
                                    }
                                }
                            }

                            SortParkingLot(parkingLot, vehicle);
                            _lastClickedParkingLot = null;
                            return;
                        }
                    }
                }

                // REMOVE HIGHLIGHT 
                _lastClickedParkingLot = null;
            }
            else
            {
                if (!parkingLot.IsEmpty())
                {
                    _lastClickedParkingLot = parkingLot;
                    // HİGHLIGHT 
                }
            }
        }

        private void SortParkingLot(object sender, Vehicle arg)
        {
            var parkingLot = (ParkingLot)sender;
            var seatsToSort = SortSeatsByColorCount(arg.GetSeats());
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

                if (arg.IsEmpty())
                {
                    return;
                }

                var parkingLotPosition = parkingLot.GetParkingLotPosition();
                var neighborParkingLots =
                    parkingLot.FindNeighbors(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()].lines);
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
                    var swappingSlot = CheckForSeatToSwap(parkingLot, arg.GetPassenger().GetColor());

                    if (swappingSlot == null)
                    {
                        break;
                    }

                    animateSwappingSeats.Add(swappingSlot);
                    animateSwappingSeats.Add(match);
                    swappingSlot.Swap(match);

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

        private List<Seat> SortSeatsByColorCount(List<Seat> seats)
        {
            List<Seat> sortedSeats = new List<Seat>();
            if (seats.FindAll(s => s.IsEmpty()).Count == 4) return sortedSeats;

            var colorCount = new Dictionary<ColorEnum, int>();
            foreach (var seat in seats)
            {
                if (seat.IsEmpty()) continue;
                var color = seat.GetPassenger().GetColor();
                if (!colorCount.TryAdd(color, 1))
                {
                    colorCount[color]++;
                }
            }

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

            if (selectedColor != ColorEnum.NONE)
            {
                colorCount.Remove(selectedColor);
                sortedSeats.Add(seats.First(s => !s.IsEmpty() && s.GetPassenger().GetColor() == selectedColor));
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