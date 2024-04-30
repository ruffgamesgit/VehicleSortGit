using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoSingleton<InputManager>
{
    [Header("References")]
    public LayerMask VehicleLayer;
    public LayerMask LotLayer;

    [Header("Debug")]
    [SerializeField] bool blockVehiclePicking;
    [SerializeField] VehicleController selectedVehicle;
    public List<ParkingLotsHolder> ParkingLotsHolders = new List<ParkingLotsHolder>();
    ParkingLotsHolder startHolder;

    // Update is called once per frame
    IEnumerator Start()
    {
        yield return null;
        SortList();
    }
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
                    if (selectedVehicle == vehicle)
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
                    startHolder = selectedVehicle.CurrentLot.GetComponentInParent<ParkingLotsHolder>();
                    vehicle.GetPicked();
                    blockVehiclePicking = true;
                }
            }
            else if (Physics.Raycast(ray, out hit, 300, LotLayer))
            {
                if (hit.collider.TryGetComponent(out LotController lot))
                {
                    int targetHolderIndex = ParkingLotsHolders.IndexOf(lot.GetComponentInParent<ParkingLotsHolder>());
                    if (startHolder.CheckCanMove(targetHolderIndex) && !lot.IsOccupied)
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

    public void AddParkingLotHolder(ParkingLotsHolder parkingLotsHolder)
    {
        if (ParkingLotsHolders.Contains(parkingLotsHolder)) return;

        ParkingLotsHolders.Add(parkingLotsHolder);
    }

    void SortList()
    {
        List<ParkingLotsHolder> sortedList = new List<ParkingLotsHolder>();

        foreach (var holder in ParkingLotsHolders)
        {
            // Find the index to insert at
            int index = 0;
            foreach (var sortedHolder in sortedList)
            {
                if (holder.gameObject.transform.position.z > sortedHolder.gameObject.transform.position.z)
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            // Insert the holder at the correct index
            sortedList.Insert(index, holder);
        }

        // Update the original list
        ParkingLotsHolders = sortedList;
    }
}
