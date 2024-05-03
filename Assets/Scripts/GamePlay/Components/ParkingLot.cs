using System;
using Cysharp.Threading.Tasks;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class ParkingLot : MonoBehaviour
    {
        public EventHandler<Vehicle> OnVehiclePlaced;
        private ParkingLotPosition _parkingLotPosition;
        private bool _isInvisible = false;
        private Vehicle _currentVehicle;


        public void Initialize(bool isInvisible, ParkingLotPosition parkingLotPosition)
        {
            _isInvisible = isInvisible;
            _parkingLotPosition = parkingLotPosition;
        }
        
        public void Occupy(Vehicle vehicle, bool withAnimation, UniTaskCompletionSource ucs)
        {
            _currentVehicle = vehicle;
            _currentVehicle.transform.parent = this.transform;
            if (!withAnimation)
            {
                _currentVehicle.transform.position = this.transform.position;
                ucs?.TrySetResult();
            }
            else
            {
                _currentVehicle.transform.position = this.transform.position;
                ucs?.TrySetResult();
                //ANIMATION
            }
           
        }

        public void SetEmpty()
        {
            _currentVehicle = null;
        }

        public bool IsEmpty()
        {
            return _currentVehicle != null;
        }
        
        public Vehicle GetCurrentVehicle()
        {
            return _currentVehicle;
        }
        
        public ParkingLotPosition GetParkingLotPosition()
        {
            return _parkingLotPosition;
        }
        
        public bool IsWalkable()
        {
            return _isInvisible || !IsEmpty();
        }
        
        public bool IsInvisible()
        {
            return _isInvisible;
        }

        public bool CheckIfCompleted()
        {
            var seats = _currentVehicle.GetSeats();
            
            foreach (var seat in seats)
            {
                if (seat.IsEmpty())
                    return false;

                if (seat.GetPassenger().GetColor() != seats[0].GetPassenger().GetColor())
                    return false;
            }

            _currentVehicle.Destroy(); // LATER ANİMATON
            return true;
            // COMPLETE
        }
     
    }
}