using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace GamePlay.Components
{
    public class GarageController : MonoBehaviour
    {
        public EventHandler<ParkingLot> OnVehicleReleased; 
        public ParkingLot neighborParkingLot;
        [SerializeField] private TextMeshPro vehicleCountTxt;
        [HideInInspector] public int vehicleNeed;
        private List<Vehicle> _vehicles = new List<Vehicle>();

        public void Initialize()
        {
            neighborParkingLot.OnEmptied += OnNeighborEmptied;
        }

        public void PushVehicle(Vehicle vehicle)
        {
            if (_vehicles.Count != 0)
            {
                vehicle.transform.DOScale(Vector3.zero, 0f);
            }

            _vehicles.Add(vehicle);
            vehicleCountTxt.text = _vehicles.Count.ToString();
        }

        private void OnNeighborEmptied(object sender, EventArgs e)
        {
            if (_vehicles.Count == 0) return;
            var vehicle = _vehicles[0];
            _vehicles.RemoveAt(0);
            neighborParkingLot.Occupy(vehicle, true, () =>
            {
                OnVehicleReleased?.Invoke(this, neighborParkingLot);
            });
            if (_vehicles.Count > 0)
            {
                _vehicles[0].transform.DOScale(Vector3.one * 0.9f, 0.35f).SetEase(Ease.OutQuad).SetDelay(0.25f);
            }
            vehicleCountTxt.text = _vehicles.Count.ToString();
        }

        public List<Vehicle> Clear()
        {
            if (_vehicles.Count != 0)
                foreach (var vehicle in _vehicles)
                {
                    var seats = vehicle.GetSeats();
                    foreach (var seat in seats)
                    {
                        seat.ResetPreColor();
                    }
                }
            return _vehicles;
        }
    }
}