using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class ParkingLot : MonoBehaviour
    {
        public EventHandler<Vehicle> OnParkingLotClicked;
        
        private ParkingLotPosition _parkingLotPosition;
        private Vehicle _currentVehicle;

        private bool _isInvisible;
        private bool _willOccupied;
        public void Initialize(bool isInvisible, ParkingLotPosition parkingLotPosition)
        {
            _isInvisible = isInvisible;
            if (_isInvisible)
            {
                gameObject.SetActive(false);
            }
            _parkingLotPosition = parkingLotPosition;
        }
        
        public void Occupy(Vehicle vehicle, bool moveTransform)
        {
            _willOccupied = false;
            _currentVehicle = vehicle;
            _currentVehicle.transform.parent = this.transform;
            if (!moveTransform)
            {
                _currentVehicle.transform.position = this.transform.position;
            }
        }


        public void MoveAnimation(Vehicle vehicle,UniTaskCompletionSource ucs, ParkingLot from)
        {

            var sequence = DOTween.Sequence();
            var fromPosition = from.GetParkingLotPosition();
            var targetGridGroupIndex = _parkingLotPosition.GetGridGroupIndex();
            var fromGridGroupIndex = fromPosition.GetGridGroupIndex();
            var targetGridLineIndex = _parkingLotPosition.GetGridLineIndex();
            var fromGridLineIndex = fromPosition.GetGridLineIndex();
            var targetParkingLotIndex = _parkingLotPosition.GetParkingLotIndex();
            var fromParkingLotIndex = fromPosition.GetParkingLotIndex();

            if (targetGridGroupIndex == fromGridGroupIndex)
            {
                if (targetGridLineIndex == fromGridLineIndex)
                {
                    bool isNext = Mathf.Abs(targetParkingLotIndex - fromParkingLotIndex) == 1;
                    if(isNext)
                    {
                        sequence.Append(vehicle.transform.DOMove(transform.position, 0.25f).SetEase(Ease.Linear));
                    }
                    else
                    {
                        var targetVector3 = transform.position;
                        sequence.Append(vehicle.transform
                            .DOMoveZ(targetVector3.z + (targetGridLineIndex == 0 ? -2 : 2), 0.25f)
                            .SetEase(Ease.Linear));
                     
                        sequence.Append(vehicle.transform.DOMoveX(targetVector3.x, 0.25f).SetEase(Ease.Linear));
                        sequence.Append(vehicle.transform.DOMoveZ(targetVector3.z, 0.25f)
                            .SetEase(Ease.Linear));
                    }
                }
                else
                {
                    sequence.Append(vehicle.transform.DOMove(transform.position, 0.25f).SetEase(Ease.Linear));
                }
            }
            else
            {
                var targetVector3 = transform.position;
                var midPointVector3 = (from.transform.position + targetVector3) / 2;
                
                sequence.Append(vehicle.transform
                    .DOMoveZ(midPointVector3.z, 0.25f)
                    .SetEase(Ease.Linear));
                if (Math.Abs(targetVector3.x - from.transform.position.x) > 0.01f)
                {
                    sequence.Append(vehicle.transform.DOMoveX(targetVector3.x, 0.25f).SetEase(Ease.Linear));
                }
                
                sequence.Append(vehicle.transform.DOMoveZ(targetVector3.z, 0.25f)
                    .SetEase(Ease.Linear));
            }

            sequence.OnComplete(() =>
            {
                ucs.TrySetResult();
            });
        }

        private void OnMouseDown()
        {
            if (_isInvisible) return;
            if (IsAnimationOn() || _willOccupied)
            {
                OnParkingLotClicked?.Invoke(null, null);
                return;
            }
            OnParkingLotClicked?.Invoke(this, _currentVehicle);
        }
        
        private bool IsAnimationOn()
        {
            if (_currentVehicle == null) return false;
            return _currentVehicle.IsAnimationOn();
        }
        
        public Vehicle GetCurrentVehicle()
        {
            return _currentVehicle;
        }
        
        public ParkingLotPosition GetParkingLotPosition()
        {
            return _parkingLotPosition;
        }
        
        public void SetWillOccupied()
        {
            _willOccupied = true;
        }
        
        public void SetEmpty()
        {
            _currentVehicle = null;
        }

        public bool IsEmpty()
        {
            return _currentVehicle == null;
        }

        public bool IsWalkable()
        {
            return  _isInvisible || IsEmpty();
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