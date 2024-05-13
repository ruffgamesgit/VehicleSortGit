using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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

        private List<Vehicle> GenerateVehiclesForFeeders(List<GarageController> garages)
        {
            if(garages == null || garages.Count == 0) return new List<Vehicle>();
            List<Vehicle> vehiclesOnFeeder = new List<Vehicle>();
            for (int i = 0; i < garages.Count; i++)
            {
                garages[i].Clear();
                for(int j = 0; j < garages[i].vehicleNeed; j++)
                {
                    var vehicle = Instantiate(vehiclePrefab, garages[i].transform.position, Quaternion.identity);
                    garages[i].PushVehicle(vehicle);
                    vehiclesOnFeeder.Add(vehicle);
                }
            }
            return vehiclesOnFeeder;
        }
        
        
        private void GenerateVehicles(List<GridGroup> gridGroups, int vehicleCount, List<GarageController> garages)
        {
            List<ParkingLot> parkingLots = GetParkingLotsShuffled(gridGroups);

            foreach (var parkingLot in parkingLots)
            {
                if (!parkingLot.IsEmpty())
                {
                    var vehicle = parkingLot.GetCurrentVehicle();
                    if(vehicle != null)
                        vehicle.Destroy();
                    
                }
                parkingLot.SetEmpty();
            }
            
            int vehicleCounter = 0;
            if(garages != null && garages.Count > 0)
            {
                foreach (var garage in garages)
                {
                    var parkingLot = garage.neighborParkingLot;
                    parkingLots.Remove(parkingLot);
                    var vehicle = Instantiate(vehiclePrefab, parkingLot.transform.position, Quaternion.identity);
                    parkingLot.Occupy(vehicle, true);
                    vehicle.transform.position = parkingLot.transform.position;
                    vehicleCounter++;
                }
            }
         
            Queue<ParkingLot> parkingLotQueue = new Queue<ParkingLot>(parkingLots);
            while (vehicleCounter < vehicleCount && parkingLotQueue.Count > 0)
            {
                var parkingLot = parkingLotQueue.Dequeue();
                var vehicle = Instantiate(vehiclePrefab, parkingLot.transform.position, Quaternion.identity);
                parkingLot.Occupy(vehicle, true);
                vehicle.transform.position = parkingLot.transform.position;
                vehicleCounter++;
            }
        }

        public void FillVehicles(List<GridGroup> gridGroups, int vehicleCount, int colorVariety,
            int matchingPassengerCount, List<GarageController> garages)
        {
            if (vehicleCount < matchingPassengerCount)
            {
                Debug.LogError("Vehicle count should be at least 4 times the matching passenger count");
                return;
            }

            startOver:
            var vehiclesOnFeeders = GenerateVehiclesForFeeders(garages);
            GenerateVehicles(gridGroups, vehicleCount - vehiclesOnFeeders.Count, garages);
            List<ColorEnum> colorVarietyList = ColorEnumExtension.GetAsList();
            colorVarietyList = colorVarietyList.Take(colorVariety).ToList();
            var colorCounts = GetRandomColorCounts(colorVarietyList, matchingPassengerCount);

            List<ParkingLot> parkingLots = GetParkingLotsShuffled(gridGroups);
            parkingLots.RemoveAll(p => p.GetCurrentVehicle() == null);
            List<object> allVehicles = new List<object>(parkingLots);
            allVehicles.AddRange(vehiclesOnFeeders);
            allVehicles.Shuffle();

            foreach (var item in allVehicles)
            {
                if(item == null) return;
                List<ColorEnum> availableColors = new List<ColorEnum>();
                foreach (var color in colorCounts)
                {
                    bool canPlaceable = item.GetType() == typeof(ParkingLot) ? CheckIfPlaceable(gridGroups, item as ParkingLot, color.Key) != null : CheckIfPlaceable(item as Vehicle, color.Key) != null;
                    if (canPlaceable)
                        availableColors.Add(color.Key);
                }
                
                if (availableColors.Count == 0)
                {
                    goto startOver;
                }
                
                var colorToPlace = availableColors.GetRandomObjectType();
                var seat =  item.GetType() == typeof(ParkingLot) ? CheckIfPlaceable(gridGroups, item as ParkingLot, colorToPlace) : CheckIfPlaceable(item as Vehicle,colorToPlace);
                if (seat == null)
                {
                    goto startOver;
                }
                seat.SetPreColor(colorToPlace);

                colorCounts[colorToPlace]--;
                if (colorCounts[colorToPlace] == 0)
                    colorCounts.Remove(colorToPlace);
            }
            
            while (colorCounts.Count > 0)
            {
                var color = GetRandomColorFromDictionary(colorCounts);
                allVehicles.Shuffle();
                foreach (var item in allVehicles)
                {
                    var availableSeat = item.GetType() == typeof(ParkingLot) ? CheckIfPlaceable(gridGroups, item as ParkingLot, color) : CheckIfPlaceable(item as Vehicle, color);
                    if (availableSeat == null) continue;
                    availableSeat.SetPreColor(color);
                    goto placementFound;
                }
                
                goto startOver;

                placementFound:

                colorCounts[color]--;
                if (colorCounts[color] == 0)
                    colorCounts.Remove(color);
            }

            InitializeAllSeats(allVehicles);
        }

        private void InitializeAllSeats(List<object> items)
        {

            foreach (var item in items)
            {
                if (item.GetType() == typeof(ParkingLot))
                {
                    var parkingLot = item as ParkingLot;
                    if(parkingLot == null)continue;
                    var vehicle = parkingLot.GetCurrentVehicle();
                    if(vehicle == null)continue;
                    foreach (var seat in vehicle.GetSeats())
                    {
                        seat.InstantiatePreColor(passengerPrefab);
                    }
                    vehicle.SortByType(true).Forget();
                }
                else
                {
                    var vehicle = item as Vehicle;
                    if(vehicle == null) continue;
                    foreach (var seat in vehicle.GetSeats())
                    {
                        seat.InstantiatePreColor(passengerPrefab);
                    }
                    vehicle.SortByType(true).Forget();
                }
            }
        }
        
        private Seat CheckIfPlaceable(Vehicle vehicle, ColorEnum color)
        {
            var availableSeat = CheckAvailabilityByPreColor(vehicle);

            if (availableSeat == null)
            {
                return null;
            }
            
            var seats = vehicle.GetSeats();
            var filledSeats = GetFilledSeats(seats);
            
            if (filledSeats.Count == seats.Count - 1)
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

                if (isAllMatching) return null;
            }
            
            return availableSeat;
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
            if (filledSeats.Count == seats.Count - 1)
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

                if (isAllMatching) return null;
            }

            var neighbors =
                parkingLot.FindNeighbors(gridGroups[parkingLot.GetParkingLotPosition().GetGridGroupIndex()].lines);

            foreach (var neighbor in neighbors)
            {
                var neighborVehicle = neighbor.GetCurrentVehicle();
                if (neighborVehicle == null) continue;

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
                if (seat.GetPreColor() == ColorEnum.NONE)
                    return seat;
            }

            return null;
        }

        private List<Seat> GetFilledSeats(List<Seat> seats)
        {
            return seats.FindAll(seat => seat.GetPreColor() != ColorEnum.NONE);
        }

        private ColorEnum GetRandomColorFromDictionary(Dictionary<ColorEnum, int> colorCounts)
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
            for (int i = 0; i < colorCountKeys.Count; i++)
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
                        if (!parkingLot.IsInvisible() && !parkingLot.IsEmptyAtStart())
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