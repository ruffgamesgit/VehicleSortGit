using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.Data.Grid;
using Unity.VisualScripting;
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


        private void Awake()
        {
            _fillController = GetComponent<FillController>();
            InitializeParkingLots();
            _fillController.FillVehicles(gridData.gridGroups, 20,5 , 16); // Variety , MatchingPassangerCount
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
                        parkingLot.Initialize(isParkingLotInvisible,parkingLotPosition);
                        parkingLot.OnVehiclePlaced += SortParkingLot;
                        
                        parkingLotIndex++;
                    }

                    gridLineIndex++;
                }

                gridGroupIndex++;
            }
        }

        private void SortParkingLot(object sender, Vehicle arg)
        {
            var parkingLot = (ParkingLot)sender;
            UniTaskCompletionSource uc = new UniTaskCompletionSource();
            parkingLot.Occupy(arg, true, uc);
            uc.Task.ContinueWith(() =>
            {
                foreach (var seat in arg.GetSeats())
                {
                    if(!seat.IsEmpty())
                        InsertItemToQueue(parkingLot,seat);
                }
                SortAffectedParkingLots();
            });
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
                var neighborParkingLots = parkingLot.FindNeighbors(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()].lines);
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
                        var secondNeighbors = swappingNeighbors[0].FindNeighbors(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()].lines);
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

                    if (DoesParkingLotHasMatchingItem(parkingLot, arg.GetPassenger().GetColor()) &&
                        DoesParkingLotHasMatchingItem(swappingNeighbors[0], swappingSeats[0].GetPassenger().GetColor()))
                    {
                        IsMatchedBySecondNeighbor();
                        return;
                    }

                    if (IsMatchedBySecondNeighbor())
                        return;
                }

                for (var i = 0; i < swappingSeats.Count; i++)
                {
                    var match = swappingSeats[i];
                    var swappingSlot = parkingLot.GetCurrentVehicle().GetSeats().Find(seat =>
                        seat.IsEmpty());
                    if (swappingSlot == null)
                    {
                        swappingSlot = parkingLot.GetCurrentVehicle().GetSeats().Find(seat =>
                            seat.GetPassenger().GetColor() != arg.GetPassenger().GetColor());
                    }

                    if (swappingSlot == null)
                    {
                        break;
                    }
                    
                    await swappingSlot.Swap(match);

                    if (swappingNeighbors[i].CheckIfCompleted()) // LATER AWAIT ANIMATION
                    {
                        EnqueueItem(swappingNeighbors[i]);
                    }
                    else
                    {
                        EnqueueItem(swappingNeighbors[i], swappingSeats[i]);
                    }
                }
                
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
            foreach (var seat in neighbor.GetCurrentVehicle().GetSeats())
            {
                if (!seat.IsEmpty() &&  seat.GetPassenger().GetColor() == color)
                    seats.Add(seat);
            }
            return seats;
        }
        
        private bool DoesParkingLotHasMatchingItem(ParkingLot parkingLot, ColorEnum color)
        {
            var matchingItems = parkingLot.GetCurrentVehicle().GetSeats()
                .FindAll(seat => !seat.IsEmpty() && seat.GetPassenger().GetColor() != color);

            if (matchingItems.Count >= 2)
            {
                var firstMatchingItemColor = matchingItems[0].GetPassenger().GetColor();

                foreach (var item in matchingItems)
                {
                    if (item.GetPassenger().GetColor() != firstMatchingItemColor)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
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