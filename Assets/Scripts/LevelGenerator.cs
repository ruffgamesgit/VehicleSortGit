using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoSingleton<LevelGenerator>
{
    [Header("Config")]   
    [Range(1, 10)]
    [SerializeField] int occupancyRateOfVehicles;
    [SerializeField] List<ColorEnum> desiredColorsForLevel = new List<ColorEnum>();
    [SerializeField] List<int> multipliers = new List<int>();
    [SerializeField] Dictionary<ColorEnum, int> baseColorPool = new Dictionary<ColorEnum, int>();

    [Header("Debug")]
    [SerializeField] int totalPassengerStackCount;
    [SerializeField] int desiredPassengerStackCount;
    int totalLotCount;
    int totalVehiclesCount;
    List<LotController> spawnedLots = new();
    List<LotController> lotsWithVehicle = new List<LotController>();
    List<LotController> emptyLots = new();
    List<ParkingLotsHolder> parkingLotsHolders = new();
    private System.Random _rng = new System.Random();
    IEnumerator Start()
    {
        yield return null;

        SetInitialParameters();
        SetLotsWithVehicleList();
        GenerateLevelRandomly();
    }
    #region List Modify Region 

    public void ModifyTotalLotCount(int add)
    {
        totalLotCount += add;
    }
    public void AddParkingLotsHolder(ParkingLotsHolder lotsHolder)
    {
        if (parkingLotsHolders.Contains(lotsHolder)) return;

        parkingLotsHolders.Add(lotsHolder);
        AddSpawnedLots(lotsHolder);
    }
    private void AddSpawnedLots(ParkingLotsHolder lotsHolder)
    {
        for (int i = 0; i < lotsHolder.SpawnedLots.Count; i++)
        {
            LotController lot = lotsHolder.SpawnedLots[i];
            if (!spawnedLots.Contains(lot))
            {
                spawnedLots.Add(lot);
            }
        }
    }
    void SetInitialParameters()
    {
        totalVehiclesCount = (spawnedLots.Count - emptyLots.Count);
        totalPassengerStackCount = totalVehiclesCount * 4;
        desiredPassengerStackCount = (totalPassengerStackCount * occupancyRateOfVehicles) / 10;

        if (desiredPassengerStackCount % 4 != 0)
        {
            desiredPassengerStackCount += 4 - (desiredPassengerStackCount % 4);
            Debug.LogWarning(desiredPassengerStackCount);
        }
    }
    void SetLotsWithVehicleList()
    {
        for (int i = 0; i < spawnedLots.Count; i++)
        {
            if (!spawnedLots[i].IsInitializedEmpty)
            {
                lotsWithVehicle.Add(spawnedLots[i]);
            }
        }
    }

    #endregion

    void GenerateLevelRandomly()
    {
        for (int i = 0; i < desiredColorsForLevel.Count; i++)
        {
            multipliers.Add(1);
            var color = desiredColorsForLevel[i];
            if (!baseColorPool.TryAdd(color, 4))
            {
                Debug.Log("Something wrong with the baseColorPool dictionary, operation add failed.");
            }
        }

        int totalMultiplier = (desiredPassengerStackCount / 4) - desiredColorsForLevel.Count;
        while (totalMultiplier > 0)
        {
            if (totalMultiplier <= 0) break;

            for (int i = 0; i < multipliers.Count; i++)
            {
                if (totalMultiplier <= 0) break;

                int randomValue = _rng.Next(0, totalMultiplier + 1);
                multipliers[i] += randomValue;
                totalMultiplier -= randomValue;

                var color = desiredColorsForLevel[i];
                baseColorPool[color] += randomValue * 4;

            }
        }

        List<LotController> lotsWithVehicleShuffled = new List<LotController>(lotsWithVehicle);
        lotsWithVehicleShuffled.Shuffle();
        foreach (LotController lot in lotsWithVehicleShuffled)
        {
            lot.SpawnVehicle(1);
            VehicleController vehicle = lot.GetVehicle();
            var neighbors = lot.GetLotNeighbors();
            var uniqueNeighborColors = GetUniqueColorListFromNeighbors(neighbors);
            List<ColorEnum> colorsAvailable = new List<ColorEnum>(baseColorPool.Keys);
            foreach (var color in uniqueNeighborColors)
            {
                colorsAvailable.Remove(color);
            }

            if (colorsAvailable.Count > 0)
            {
                var randomColorIndex = _rng.Next(0, colorsAvailable.Count - 1);
                var colorToUse = colorsAvailable[randomColorIndex];
                vehicle.CurrentPassengerStacks[0].Initialize(colorToUse);
                baseColorPool[colorToUse] -= 1;
                if (baseColorPool[colorToUse] <= 0)
                {
                    baseColorPool.Remove(colorToUse);
                }
            }

        }

        int fillCount = 0;
    fillPassengers:
        lotsWithVehicle.Shuffle();



        foreach (LotController lot in lotsWithVehicleShuffled)
        {
            VehicleController vehicle = lot.GetVehicle();
            var neighbors = lot.GetLotNeighbors();
            var uniqueNeighborColors = GetUniqueColorListFromNeighbors(neighbors);
            List<ColorEnum> colorsAvailable = new List<ColorEnum>(baseColorPool.Keys);
            foreach (ColorEnum color in uniqueNeighborColors)
            {
                colorsAvailable.Remove(color);
            }

            if (colorsAvailable.Count > 0)
            {
                var randomColorIndex = _rng.Next(0, colorsAvailable.Count - 1);
                var colorToUse = colorsAvailable[randomColorIndex];

                var existingColorList = vehicle.GetExistingColors();
                if (vehicle.CurrentPassengerStacks.Count == 3 && existingColorList.Count == 1 && existingColorList[0] == colorToUse)
                {
                    continue;
                }
                var randomValue = _rng.Next(0, 2);
                if (randomValue == 1)
                {
                    lot.AddPassengerStack(1);
                }
                else
                {
                    continue;
                }

                vehicle.CurrentPassengerStacks[vehicle.CurrentPassengerStacks.Count - 1].Initialize(colorToUse);
                baseColorPool[colorToUse] -= 1;
                if (baseColorPool[colorToUse] <= 0)
                {
                    baseColorPool.Remove(colorToUse);
                }
                if (baseColorPool.Count == 0)
                {
                    return;
                }
            }
            else
            {
                Debug.Log("");
            }

        }

        fillCount += 1;
        if (baseColorPool.Count > 0)
        {
            if (fillCount < 30)
                goto fillPassengers;
        }

        if (baseColorPool.Count > 0)
        {
            // Reload Scene
            Debug.Log("Scene reloaded");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private List<ColorEnum> GetUniqueColorListFromNeighbors(List<LotController> neighbors)
    {
        HashSet<ColorEnum> colorEnums = new HashSet<ColorEnum>();

        foreach (var neighbor in neighbors)
        {
            var existingColor = neighbor.GetLotExistingColor();
            if (existingColor == null) continue;
            foreach (ColorEnum colorEnum in existingColor)
            {
                colorEnums.Add(colorEnum);
            }
        }
        return colorEnums.ToList();
    }


    public void AddEmptyLot(LotController lot)
    {
        emptyLots.Add(lot);
    }
}

