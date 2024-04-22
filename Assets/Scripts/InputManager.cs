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
                    if (selectedVehicle != null) selectedVehicle.GetReleased();
                    if(selectedVehicle == vehicle)
                    {
                        selectedVehicle.GetReleased();
                        ResetParams();
                        
                        return;
                    }
                    ResetParams();

                    if (vehicle.isPicked)
                    {
                        return;
                    }


                    selectedVehicle = vehicle;
                    vehicle.GetPicked();
                    blockVehiclePicking = true;
                }
            }
            else if (Physics.Raycast(ray, out hit, 300, LotLayer))
            {
                if (hit.collider.TryGetComponent(out LotController lot))
                {
                    if (!lot.IsOccupied)
                    {
                        if (selectedVehicle == null) return;
                        lot.SetOccupied(true);
                        selectedVehicle.GoOtherLot(lot);
                    }
                    else
                    {
                        if (selectedVehicle == null) return;
                        selectedVehicle.GetReleased();

                    }
                    ResetParams();

                }
            }
        }
    }
    void ResetParams()
    {
        selectedVehicle = null;
        blockVehiclePicking = false;
    }
}
