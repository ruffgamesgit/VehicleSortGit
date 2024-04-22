using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("References")]    
    public LayerMask VehicleLayer;
    public LayerMask LotLayer;

    [Header("Debug")]
    [SerializeField] bool blockVehiclePicking;
    [SerializeField] VehicleController selectedVehicle;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 300, VehicleLayer))
            {
                if (hit.collider.TryGetComponent(out VehicleController vehicle))
                {
                    if (blockVehiclePicking) return;

                    selectedVehicle = vehicle;
                   // vehicle.GetPicked();

                    blockVehiclePicking = true;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedVehicle != null)
            {
                selectedVehicle = null;
            }
        }
    }
}
