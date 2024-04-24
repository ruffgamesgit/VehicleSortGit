using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomStatsAssigner : MonoSingleton<RandomStatsAssigner>
{
    [Header("Config")]
    public int emptyLotCount;
    [Range(1, 10)]
    [SerializeField] int occupancyRateOfVehicles;
    [SerializeField] List<ColorEnum> desiredColorsForLevel = new List<ColorEnum>();
    [SerializeField] List<int> multipliers = new List<int>();
    [SerializeField] List<ColorEnum> baseColorPool = new List<ColorEnum>();

    [Header("Debug")]
    [SerializeField] int totalPassengerStackCount;
    [SerializeField] int desiredPassengerStackCount;
    int totalLotCount;
    int totalVehiclesCount;
    List<LotController> spawnedLots = new();
    List<LotController> lotsWithVehicle = new List<LotController>();
    List<LotController> emptyLots = new();
    List<ParkingLotsHolder> parkingLotsHolders = new();

    IEnumerator Start()
    {
        yield return null;

        SetEmptyLots();
        SetLotsWithVehicleList();

        HandleRandomizationPlacement();
        ColorAssign();
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
    void SetEmptyLots()
    {
        for (int i = 0; i < spawnedLots.Count; i++)
        {
            if (emptyLotCount == 0) break;

            int randomIndex = Random.Range(0, spawnedLots.Count - 1);
            LotController lot = spawnedLots[randomIndex];

            if (lot != null && !lot.IsInitializedEmpty)
            {
                lot.SetIsEmpty(true);
                emptyLots.Add(lot);
                emptyLotCount--;
            }
            else
            {
                continue;
            }

        }

        totalVehiclesCount = (spawnedLots.Count - emptyLots.Count);
        totalPassengerStackCount = totalVehiclesCount * 4;
        desiredPassengerStackCount = (totalPassengerStackCount * occupancyRateOfVehicles) / 10;
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
    void HandleRandomizationPlacement()
    {
        int desiredStacks = desiredPassengerStackCount; // X
        int lotListCount = lotsWithVehicle.Count; // Y
        int leftStackCount = desiredStacks - lotsWithVehicle.Count; // Z


        for (int i = 0; i < lotListCount; i++)
        {
            LotController lot = lotsWithVehicle[i];
            lot.SpawnVehicle(1);
        }

        while (leftStackCount != 0)
        {
            if (leftStackCount == 0) break;

            for (int i = 0; i < lotListCount; i++)
            {
                if (leftStackCount == 0) break;

                LotController lot = lotsWithVehicle[i];
                int availablePointCount = lot.CurrentVehicle.GetAvailablePointCount();
                if (availablePointCount < 1) continue;

                int randomStackCount = Random.Range(0, leftStackCount + 1);
                randomStackCount = Mathf.Min(randomStackCount, 3);
                randomStackCount = Mathf.Min(randomStackCount, availablePointCount);

                leftStackCount -= randomStackCount;
                lot.AddPassengerStack(randomStackCount);

            }
        }
    }

    void ColorAssign()
    {
        for (int i = 0; i < desiredColorsForLevel.Count; i++)
        {
            multipliers.Add(1);

        }

        // SETTING THE MULTIPLIERS LIST BY RANDOM
        int totalMultiplier = (desiredPassengerStackCount / 4) - desiredColorsForLevel.Count;
        while (totalMultiplier > 0)
        {
            if (totalMultiplier <= 0) break;

            for (int i = 0; i < multipliers.Count; i++)
            {
                if (totalMultiplier <= 0) break;

                int randomValue = Random.Range(0, totalMultiplier + 1);
                multipliers[i] += randomValue;
                totalMultiplier -= randomValue;
            }
        }


        if (desiredColorsForLevel.Count != desiredPassengerStackCount / 4)
        {
            // COMPOSING BASE COLOR POOL LIST USING THE MULTIPLIERS
            for (int j = 0; j < desiredColorsForLevel.Count; j++)
            {
                ColorEnum color = desiredColorsForLevel[j];
                int multiplier = multipliers[j];
                int totalIterateCount = multiplier * 4;

                for (int i = 0; i < totalIterateCount; i++)
                {
                    baseColorPool.Add(color);

                }
            }
        }
        else
        {
            Debug.Log("Every color can only have one stack");
            for (int i = 0; i < desiredColorsForLevel.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    baseColorPool.Add(desiredColorsForLevel[i]);

                }
            }
        }

        Shuffle(baseColorPool);

        // ASSIGNING PASSENGERS' COLOR
        int iterateCount = 0;
        List<PassengerStack> stacks = new(GetTotalPassengerStacks());
        for (int i = 0; i < stacks.Count; i++)
        {
            PassengerStack stack = stacks[i];
            LotController currentLot = stack.GetCurrentLot();
            ColorEnum assignableColor = baseColorPool[i];
            List<ColorEnum> neighborStackColorsList = new List<ColorEnum>();

            for (int p = 0; p < currentLot.GetLotNeighbors().Count; p++)
            {
                if (currentLot.GetLotNeighbors()[p] == null) continue;
                LotController neighborLot = currentLot.GetLotNeighbors()[p];


                if (neighborLot.CurrentVehicle == null) continue;
                List<ColorEnum> existingColor = neighborLot.CurrentVehicle.GetExistingColor();
                neighborStackColorsList.AddRange(existingColor);
            }

            if (neighborStackColorsList.Count > 0 && neighborStackColorsList.Contains(assignableColor))
            {
                while (neighborStackColorsList.Contains(assignableColor))
                {
                    if (iterateCount > 20)
                    {
                        Debug.LogError("Too many iteration is attempted, this message should never displayed normally");
                        break;
                    }
                    int randomIndex = Random.Range(0, desiredColorsForLevel.Count);

                    assignableColor = desiredColorsForLevel[randomIndex];

                    iterateCount++;
                }
            }

            stack.Initialize(assignableColor);
        }
    }
    private List<PassengerStack> GetTotalPassengerStacks()
    {
        List<PassengerStack> stacks = new List<PassengerStack>();

        for (int i = 0; i < lotsWithVehicle.Count; i++)
        {
            LotController lotWithVehicle = lotsWithVehicle[i];

            stacks.AddRange(lotWithVehicle.GetVehicle().GetSpawnedStacks());

        }

        return stacks;
    }
    void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        System.Random rng = new System.Random();

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}

