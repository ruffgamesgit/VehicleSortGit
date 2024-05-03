using DG.Tweening;
using System.Collections.Generic;
using GamePlay.Data;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    public ColorEnum passengerColor = ColorEnum.GREEN;
    [SerializeField] private List<Material> materials = new List<Material>();
    [SerializeField] private MeshRenderer meshRenderer;
    VehicleController currentVehicle;

    public void SetColorEnumAndMat(ColorEnum color)
    {
        passengerColor = color;
        SetMat();
    }

    private void Start()
    {
        PassengerStack passengerStack = transform.GetComponentInParent<PassengerStack>();
        passengerStack.UpdateCurrentVehicleEvent += OnCurrentVehicleUpdated;
        passengerStack.StackMovedNewVehicleEvent += OnVehicleRotationPossiblyChanged;

        currentVehicle = passengerStack.GetCurrentVehicle();
        currentVehicle.VehicleArrivedAtNewLotEvent += OnVehicleRotationPossiblyChanged;

        SetRotation();
    }

    private void OnCurrentVehicleUpdated(VehicleController newVehicle)
    {
        if (currentVehicle is not null)
            currentVehicle.VehicleArrivedAtNewLotEvent -= OnVehicleRotationPossiblyChanged;

        currentVehicle = newVehicle;
        currentVehicle.VehicleArrivedAtNewLotEvent += OnVehicleRotationPossiblyChanged;
    }

    private void OnVehicleRotationPossiblyChanged()
    {
        SetRotation();
    }


    public void SetRotation()
    {
        transform.forward = Vector3.back;
    }

    void SetMat()
    {
        int colorIndex = (int)passengerColor;
        if (colorIndex >= 0 && colorIndex < materials.Count)
        {
            meshRenderer.material = materials[colorIndex];
        }
    }



    private void OnValidate()
    {
        SetMat();
    }
}
