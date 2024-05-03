using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum ColorEnum
{
    NONE, RED, BLUE, GREEN, YELLOW, PURPLE, ORANGE, PINK, WHITE, BLACK,
}

public class PassengerStack : MonoBehaviour
{
    public event System.Action<VehicleController> UpdateCurrentVehicleEvent;
    public event System.Action StackMovedNewVehicleEvent;

    [Header("Debug")]
    public ColorEnum stackColor;
    [SerializeField] VehicleController currentVehicle;
    [SerializeField] PlacementPoint placementPoint;
    [SerializeField] List<Passenger> passengers = new();

    public void Initialize(ColorEnum _color)
    {
        stackColor = _color;
        gameObject.name = gameObject.name + "_" + _color.ToString();
        currentVehicle.AddExistingStackColors(stackColor);


        for (int i = 0; i < transform.childCount; i++)
        {
            Passenger pass = transform.GetChild(i).GetComponent<Passenger>();
            if (pass != null)
            {
                passengers.Add(pass);
                pass.SetColorEnumAndMat(stackColor);
            }
        }
    }


    public void SetCurrentVehicleAndPlacementPoint(VehicleController vehicle, PlacementPoint _placementPoint)
    {
        currentVehicle = vehicle;
        UpdateCurrentVehicleEvent?.Invoke(vehicle);

        placementPoint = _placementPoint;
    }

    public void SetPlacementPoint(PlacementPoint placementPoint)
    {
        this.placementPoint = placementPoint;
        transform.SetParent(placementPoint.transform);
        transform.DOMove(placementPoint.transform.position, .2f);
    }

    public LotController GetCurrentLot()
    {
        return currentVehicle.CurrentLot;
    }
    public PlacementPoint GetCurrentPoint()
    {
        return placementPoint;
    }
    public VehicleController GetCurrentVehicle()
    {
        return currentVehicle;
    }

    public void GoOtherVehicle(VehicleController vehicle, PlacementPoint targetPoint)
    {
        // before leaving the previous vehicle remove your data from it
        // color unu çıkartman gerekli
        currentVehicle.RemoveStack(this);
        placementPoint.SetOccupied(false);
        currentVehicle.RefreshExistingColorList();

        //////////////////////////////////////////////////////////

        transform.SetParent(targetPoint.transform);
        transform.DOMove(targetPoint.transform.position, .2f).OnComplete(() =>
        {
            StackMovedNewVehicleEvent?.Invoke();
        });


        placementPoint = targetPoint;
        placementPoint.SetOccupied(true);

        //////////////////////////////////////////////////////////
        currentVehicle = vehicle;
        currentVehicle.AddExistingStackColors(stackColor);
        currentVehicle.AddStack(this);
    }
}


[System.Serializable]
public class PaassengerStackInfo
{
    public ColorEnum Color;
}
