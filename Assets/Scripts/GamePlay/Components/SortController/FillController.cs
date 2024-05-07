
using System.Collections.Generic;
using System.Linq;
using GamePlay.Data;
using GamePlay.Data.Grid;
using UnityEngine;
using Random = System.Random;

namespace GamePlay.Components.SortController
{
    public class FillController : MonoBehaviour
    {
        [SerializeField] private Vehicle vehiclePrefab;
        [SerializeField] private Passenger passengerPrefab;
        private readonly Random _rng = new Random();

        public void GenerateVehicles(List<GridGroup> gridGroups, int vehicleCount)
        {
            List<ParkingLot> parkingLots = GetParkingLotsShuffled(gridGroups);
            Queue<ParkingLot> parkingLotQueue = new Queue<ParkingLot>(parkingLots);

            int vehicleCounter = 0;
            while (vehicleCounter < vehicleCount && parkingLotQueue.Count > 0)
            {
                var parkingLot = parkingLotQueue.Dequeue();
                var vehicle = Instantiate(vehiclePrefab, parkingLot.transform.position, Quaternion.identity);
                parkingLot.Occupy(vehicle,true);
                vehicle.transform.position = parkingLot.transform.position;
                vehicleCounter++;
            }
        }

        public void FillVehicles(List<GridGroup> gridGroups, int vehicleCount , int colorVariety, int matchingPassengerCount)
        {
            if (vehicleCount < matchingPassengerCount)
            {
                Debug.LogError("Vehicle count should be at least 4 times the matching passenger count");
                return;
            }
            GenerateVehicles(gridGroups, vehicleCount);
            startOver:
            List<ColorEnum> colorVarietyList = ColorEnumExtension.GetAsList();
            colorVarietyList = colorVarietyList.Take(colorVariety).ToList();
            var colorCounts = GetRandomColorCounts(colorVarietyList, matchingPassengerCount);

            List<ParkingLot> parkingLots = GetParkingLotsShuffled(gridGroups);
            while (colorCounts.Count > 0)
            {
                var color = GetRandomColorFromDictionary(colorCounts);
                parkingLots.Shuffle();
                foreach (var parkingLot in parkingLots)
                {
                    var availableSeat = CheckIfPlaceable(gridGroups, parkingLot, color);
                    if (availableSeat == null) continue;
                    availableSeat.SetPreColor(color);
                    goto placementFound;
                    
                }
                
                ResetAllSeats(gridGroups);
                goto startOver;
                
                placementFound:
                
                colorCounts[color]--;
                if(colorCounts[color] == 0)
                    colorCounts.Remove(color);
            }
            InitializeAllSeats(gridGroups);
        }

        private void InitializeAllSeats(List<GridGroup> gridGroups)
        {
            foreach (var gridGroup in gridGroups)
            {
                foreach (var gridLine in gridGroup.lines)
                {
                    foreach (var parkingLot in gridLine.parkingLots)
                    {
                        if (!parkingLot.IsInvisible())
                        {
                            var vehicle = parkingLot.GetCurrentVehicle();
                            if(vehicle == null) continue;
                            foreach (var seat in vehicle.GetSeats())
                            {
                                seat.InstantiatePreColor(passengerPrefab);
                            }
                        }
                    }
                }
            }
        }
        
        private void ResetAllSeats(List<GridGroup> gridGroups)
        {
            foreach (var gridGroup in gridGroups)
            {
                foreach (var gridLine in gridGroup.lines)
                {
                    foreach (var parkingLot in gridLine.parkingLots)
                    {
                        if (!parkingLot.IsInvisible())
                        {
                            var vehicle = parkingLot.GetCurrentVehicle();
                            if(vehicle == null) continue;
                            foreach (var seat in vehicle.GetSeats())
                            {
                                seat.ResetPreColor();
                            }
                        }
                    }
                }
            }
        }
        
        private Seat CheckIfPlaceable(List<GridGroup> gridGroups, ParkingLot parkingLot, ColorEnum color)
        {
            if (parkingLot.IsInvisible())
            {
                return null;
            }
            var vehicle = parkingLot.GetCurrentVehicle();
            if (vehicle == null)
            {
                return null;
            }
            
            var availableSeat = CheckAvailabilityByPreColor(vehicle);

            if (availableSeat == null)
            {
                return null;
            }

            var seats = vehicle.GetSeats();
            var filledSeats = GetFilledSeats(seats);
            if (filledSeats.Count == seats.Count -1)
            {
                bool isAllMatching = true;
                foreach (var filledSeat in filledSeats)
                {
                    if (filledSeat.GetPreColor() != color)
                    {
                        isAllMatching = false;
                        break;
                    }
                }
                if(isAllMatching) return null;
            }

            var neighbors =
                parkingLot.FindNeighbors(gridGroups[parkingLot.GetParkingLotPosition().GetGridGroupIndex()].lines);

            foreach (var neighbor in neighbors)
            {
                var neighborVehicle = neighbor.GetCurrentVehicle();
                if(neighborVehicle == null) continue;

                var neighborSeats = neighborVehicle.GetSeats();

                foreach (var seat in neighborSeats)
                {
                    if (seat.GetPreColor() == color)
                    {
                        return null;
                    }
                }
            }

            return availableSeat;
        }

        private Seat CheckAvailabilityByPreColor(Vehicle vehicle)
        {
            var seats = vehicle.GetSeats();

            foreach (var seat in seats)
            {
                if(seat.GetPreColor() == ColorEnum.NONE)
                    return seat;
            }

            return null;
        }
        
        private List<Seat> GetFilledSeats(List<Seat> seats)
        {
            return seats.FindAll(seat => seat.GetPreColor() != ColorEnum.NONE);
        }
        
        private ColorEnum GetRandomColorFromDictionary(Dictionary<ColorEnum,int> colorCounts)
        {
            var colorIndex = _rng.Next(0, colorCounts.Count);
            var colorCountKeys = colorCounts.Keys.ToList();
            return colorCountKeys[colorIndex];
        }

        private Dictionary<ColorEnum, int> GetRandomColorCounts(List<ColorEnum> colors, int matchingPassengerCount)
        {
            Dictionary<ColorEnum, int> colorCounts = new Dictionary<ColorEnum, int>();
            foreach (var color in colors)
            {
                colorCounts.TryAdd(color, 1);
                matchingPassengerCount--;
            }

            while (matchingPassengerCount > 0)
            {
                var color = (ColorEnum)_rng.Next(1, colorCounts.Count);
                colorCounts[color]++;
                matchingPassengerCount--;
            }

            var colorCountKeys = colorCounts.Keys.ToList();
            for(int i = 0; i  < colorCountKeys.Count; i++)
            {
                colorCounts[colorCountKeys[i]] *= 4;
            }
   
            return colorCounts;
        }

        private List<ParkingLot> GetParkingLotsShuffled(List<GridGroup> gridGroups)
        {
            List<ParkingLot> parkingLots = new List<ParkingLot>();
            foreach (var gridGroup in gridGroups)
            {
                foreach (var gridLine in gridGroup.lines)
                {
                    foreach (var parkingLot in gridLine.parkingLots)
                    {
                        if (!parkingLot.IsInvisible())
                        {
                            parkingLots.Add(parkingLot);
                        }
                    }
                }
            }

            return parkingLots.Shuffle();
        }
    }
}