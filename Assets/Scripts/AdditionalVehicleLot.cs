using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditionalVehicleLot : MonoBehaviour
{

    [Header("References")]
    [SerializeField] VehicleController vehiclePrefab;

    [Header("Config")]
    public int additionalVehicleCount;
    [SerializeField] TargetLotInfo targetLotInfo;


    [Header("Debug")]
    [SerializeField] LotController targetLotToPlace;
    [HideInInspector] public VehicleController CurrentVehicle;
    [SerializeField] List<VehicleController> spawnedVehicles = new List<VehicleController>();


    private void Start()
    {
        LevelGenerator.instance.AddAdditionalVehicleLotList(this);
        LevelGenerator.instance.IncrementAdditionalVehicleCount(additionalVehicleCount);

        targetLotToPlace = targetLotInfo.ParkingLotsHolder.SpawnedLots[targetLotInfo.TargetLotIndex];
        targetLotToPlace.OnVehicleLeaveEvent += OnTargetLotIsEmpty;
    }

    private void OnTargetLotIsEmpty()
    {
        if (spawnedVehicles.Count <= 0) return;

        VehicleController vehicle = spawnedVehicles[0];
        targetLotToPlace.SetOccupied(true);
     
        vehicle.GoOtherLot(targetLotToPlace, tweenDuration: .15f);
        spawnedVehicles.Remove(vehicle);

        SortManager.instance.Sort(targetLotToPlace, 0.25f);

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(.4f);

            targetLotToPlace.SetVehicle(vehicle);
        }

        StartCoroutine(Routine());
    }

    public void SpawnVehicle(int activePassengerStackCount)
    {
        VehicleController cloneVehicle = Instantiate(vehiclePrefab, transform.position + Vector3.down, Quaternion.identity, transform);
        cloneVehicle.Initiliaze(activePassengerStackCount);
        CurrentVehicle = cloneVehicle;
        spawnedVehicles.Add(cloneVehicle);
    }


    public void AddPassengerStack(int stackCount)
    {
        CurrentVehicle.Initiliaze(stackCount);
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
        CurrentVehicle = vehicle;
    }


}

[System.Serializable]
public class TargetLotInfo
{
    public ParkingLotsHolder ParkingLotsHolder;
    public int TargetLotIndex;
}
