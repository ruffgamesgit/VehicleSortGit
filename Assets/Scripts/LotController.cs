using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LotController : BaseCellBehavior
{
    [Header("Customized References")]
    [SerializeField] VehicleController vehiclePrefab;

    [Header("Customized Debug")]
    public bool IsInitializedEmpty;
    [SerializeField] List<LotController> neighborLots;
    public int receivedStackCount = 0;
    [HideInInspector] public VehicleController spawnedVehicle;



    public void SpawnVehicle(int activePassengerStackCount)
    {
        SetOccupied(true);

        VehicleController cloneVehicle = Instantiate(vehiclePrefab, GetCenter(), Quaternion.identity, transform);
        cloneVehicle.Initiliaze(ColorEnum.RED, activePassengerStackCount);
        spawnedVehicle = cloneVehicle;

        receivedStackCount += activePassengerStackCount;

    }

    public void AddPassengerStack(int stackCount)
    {
        spawnedVehicle.Initiliaze(ColorEnum.RED, stackCount);

        receivedStackCount += stackCount;
    }
    public void AddNeighbour(LotController neighbor)
    {
        if (neighborLots.Contains(neighbor)) return;

        neighborLots.Add(neighbor);
    }

    public void SetIsEmpty(bool isEmpty)
    {
        IsInitializedEmpty = isEmpty;
    }
}
