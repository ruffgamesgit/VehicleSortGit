using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingLotsHolder : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] LotController lotPrefab;
    [SerializeField] float horizontalGap;
    [SerializeField] int desiredLotAmount;
    [SerializeField] List<LotStatsWrapper> lotStats;

    [Header("Debug")]
    List<LotController> spawnedLots = new List<LotController>();

    private void Awake()
    {
        SpawnLots();
    }
    private void Start()
    {
        SetLotsNeighbors();
    }
    private void SpawnLots()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < desiredLotAmount; i++)
        {
            float xPos = i * horizontalGap;
            Vector3 spawnPos = new Vector3(xPos, 1, transform.position.z);
            LotController cloneLot = Instantiate(lotPrefab, spawnPos, Quaternion.identity, transform);
            cloneLot.gameObject.name = "Lot (" + i + ")";
            cloneLot.Initiliaze(GetStatsByIndex(i));
            spawnedLots.Add(cloneLot);
        }
    }

    private void SetLotsNeighbors()
    {
        for (int i = 0; i < spawnedLots.Count; i++)
        {
            List<LotController> neigbourLots = new List<LotController>();
            LotController currentLot = spawnedLots[i];

            if (i > 0 && i < spawnedLots.Count - 1)
            {
                neigbourLots.Add(spawnedLots[i - 1]);
                neigbourLots.Add(spawnedLots[i + 1]);


            }
            if (i == 0) neigbourLots.Add(spawnedLots[i + 1]);
            if (i == spawnedLots.Count - 1) neigbourLots.Add(spawnedLots[i - 1]);

            for (int a = 0; a < neigbourLots.Count; a++)
            {
                currentLot.AddNeighbour(neigbourLots[a]);
            }
        }
    }

    public LotStatsWrapper GetStatsByIndex(int index)
    {
        return lotStats[index];
    }
}

[System.Serializable]
public class LotStatsWrapper
{
    public bool HasVehicle;
    public List<StackStatsWrapper> stackStats;
}

[System.Serializable]
public class StackStatsWrapper
{
    public ColorEnum stackColor;
}

