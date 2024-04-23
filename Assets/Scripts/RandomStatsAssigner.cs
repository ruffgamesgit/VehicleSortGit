using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStatsAssigner : MonoSingleton<RandomStatsAssigner>
{
    [Header("Config")]
    public int emptyLotCount;
    [Range(1, 10)]
    [SerializeField] int occupancyRateOfVehicles;

    [Header("Debug")]
    [SerializeField] int totalPassengerStackCount;
    [SerializeField] int desiredPassengerStackCount;
    [SerializeField] List<LotController> spawnedLots;
    [SerializeField] List<LotController> lotsWithVehicle;
    [SerializeField] List<LotController> emptyLots;
    List<ParkingLotsHolder> parkingLotsHolders;
    int totalLotCount;
    int totalVehiclesCount;


    protected override void Awake()
    {
        base.Awake();
        parkingLotsHolders = new();
        emptyLots = new List<LotController>(); // Initialize emptyLots
        spawnedLots = new List<LotController>(); // Initialize spawnedLots
    }

    IEnumerator Start()
    {
        yield return null;

        SetEmptyLots();
        SetLotsWithVehicleList();

        HandleRandomization();
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


    void HandleRandomization()
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
                int availablePointCount = lot.spawnedVehicle.GetAvailablePointCount();

                if (availablePointCount < 1) continue;

                int randomStackCount = Random.Range(0, leftStackCount + 1);
                randomStackCount = Mathf.Min(randomStackCount, 3);
                randomStackCount = Mathf.Min(randomStackCount, availablePointCount);

                leftStackCount -= randomStackCount;
                lot.AddPassengerStack(randomStackCount);
            }
        }

    }

}

