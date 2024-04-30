using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum ColorEnum
{
    NONE, RED, BLUE, GREEN, YELLOW, PURPLE, ORANGE, PINK, WHITE, BLACK,
}

public class PassengerStack : MonoBehaviour
{
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
        placementPoint = _placementPoint;
    }

    public LotController GetCurrentLot()
    {
        return currentVehicle.CurrentLot;
    }
    public PlacementPoint GetCurrentPoint()
    {
        return placementPoint;
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
        transform.DOMove(targetPoint.transform.position, .2f); 
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
