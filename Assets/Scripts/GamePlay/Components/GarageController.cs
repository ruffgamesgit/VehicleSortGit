using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace GamePlay.Components
{
    public class GarageController : MonoBehaviour
    {
        public ParkingLot neighborParkingLot;
        [HideInInspector]public int vehicleNeed;
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
        }

        private void OnNeighborEmptied(object sender, EventArgs e)
        {
            if(_vehicles.Count == 0) return;
            var vehicle = _vehicles[0];
            _vehicles.RemoveAt(0);
            neighborParkingLot.Occupy(vehicle,true);
            if (_vehicles.Count > 0)
            {
                _vehicles[0].transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutQuad);
            }
        }

        public void Clear()
        {
            if(_vehicles.Count == 0) return;
            foreach (var vehicle in _vehicles)
            {
                Destroy(vehicle.gameObject);
            }
        }
    }
}