using System.Collections.Generic;
using UnityEngine;

public enum ColorEnum
{
    RED, BLUE, GREEN, YELLOW, PURPLE, ORANGE, PINK, WHITE, BLACK
}

public class PassengerStack : MonoBehaviour
{
    [Header("Debug")]
    public ColorEnum stackColor;
    [SerializeField] VehicleController currentVehicle;
    [SerializeField] List<Passenger> passengers = new();

    public void Initialize(ColorEnum _color)
    {
        stackColor = _color;

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


    public void SetCurrentVehicle(VehicleController vehicle)
    {
        currentVehicle = vehicle;
    }

    public LotController GetCurrentLot()
    {
        return currentVehicle.CurrentLot;
    }
}


[System.Serializable]
public class PaassengerStackInfo
{
    public ColorEnum Color;
}
