using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LotController : BaseCellBehavior
{
    public event System.Action OnVehicleLeaveEvent;

    [Header("Customized References")]
    [SerializeField] VehicleController vehiclePrefab;

    [Header("Customized Debug")]
    public bool IsInitializedEmpty;
    [SerializeField] List<LotController> neighborLots;
    public VehicleController CurrentVehicle;

    public void SpawnVehicle(int activePassengerStackCount)
    {
        SetOccupied(true);

        VehicleController cloneVehicle = Instantiate(vehiclePrefab, GetCenter(), Quaternion.identity, transform);
        cloneVehicle.Initiliaze(activePassengerStackCount);
        CurrentVehicle = cloneVehicle;

    }

    public void OnVehicleArrived()
    {
        Debug.Log("Vehicle arrived");
        SortManager.instance.Sort(this, .1f);
    }
    public void AddPassengerStack(int stackCount)
    {
        CurrentVehicle.Initiliaze(stackCount);
    }
    public void AddNeighbour(LotController neighbor)
    {
        if (neighborLots.Contains(neighbor)) return;

        neighborLots.Add(neighbor);
    }

    public List<LotController> GetLotNeighbors()
    {
        return neighborLots;
    }

    public void SetIsEmpty(bool isEmpty)
    {
        IsInitializedEmpty = isEmpty;
    }

    public List<ColorEnum> GetLotExistingColor()
    {
        return CurrentVehicle?.GetExistingColors();
    }

    public VehicleController GetVehicle()
    {
        return CurrentVehicle;
    }


    public void SetVehicle(VehicleController vehicle)
    {
        if (vehicle == null) OnVehicleLeaveEvent?.Invoke();

        CurrentVehicle = vehicle;
    }

}
