using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParkingLotsHolder : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] LotController lotPrefab;
    [SerializeField] float horizontalGap;
    [SerializeField] int desiredLotCount;
    public int placementOffset;
    [SerializeField] List<ParkingLotsHolder> neighborParkingLots = new();

    [Header("Debug")]
    [HideInInspector] public List<LotController> SpawnedLots = new List<LotController>();

    private void Awake()
    {
        SpawnLots();
    }
    private void Start()
    {
        SetLotsHorizontalNeighbors();
        SetLotsVerticalNeighbors();
        RandomStatsAssigner.instance.ModifyTotalLotCount(desiredLotCount);
        RandomStatsAssigner.instance.AddParkingLotsHolder(this);
    }
    private void SpawnLots()
    {
        float initXPos = transform.position.x;

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < desiredLotCount; i++)
        {
            float xPos = i * horizontalGap;
            Vector3 spawnPos = new Vector3(xPos, 1, transform.position.z);
            LotController cloneLot = Instantiate(lotPrefab, spawnPos, Quaternion.identity, transform);
            cloneLot.gameObject.name = "Lot (" + i + ")";

            SpawnedLots.Add(cloneLot);
        }


        transform.position = new Vector3(transform.position.x + initXPos, transform.position.y, transform.position.z);
    }


    void SetLotsVerticalNeighbors()
    {
        if (neighborParkingLots.Count == 0)
            return;

        for (var x = 0; x < SpawnedLots.Count; x++)
        {
            for (int i = 0; i < neighborParkingLots.Count; i++)
            {
                ParkingLotsHolder parkingHolder = neighborParkingLots[i];
                var itemCount = parkingHolder.desiredLotCount;


                LotController myLot = SpawnedLots[x];
                var placementIndex = x - parkingHolder.placementOffset + placementOffset;
                if (placementIndex < 0) break;

                if (placementIndex < parkingHolder.SpawnedLots.Count)
                {
                    if (parkingHolder.SpawnedLots[placementIndex] != null)
                    {
                        myLot.AddNeighbour(parkingHolder.SpawnedLots[placementIndex]);
                    }
                }
            }
        }
    }


    private void SetLotsHorizontalNeighbors()
    {
        if (SpawnedLots.Count <= 1)
        {
            Debug.LogWarning("No HORIZONTAL neighbor exists");
            return;
        }

        for (int i = 0; i < SpawnedLots.Count; i++)
        {
            List<LotController> neigbourLots = new List<LotController>();
            LotController currentLot = SpawnedLots[i];


            if (i > 0 && i < SpawnedLots.Count - 1)
            {
                neigbourLots.Add(SpawnedLots[i - 1]);
                neigbourLots.Add(SpawnedLots[i + 1]);
            }

            if (i == 0) neigbourLots.Add(SpawnedLots[i + 1]);
            if (i == SpawnedLots.Count - 1) neigbourLots.Add(SpawnedLots[i - 1]);

            for (int a = 0; a < neigbourLots.Count; a++)
            {
                currentLot.AddNeighbour(neigbourLots[a]);
            }

        }
    }

}

