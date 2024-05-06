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
        private bool _isInvisible = false;
        private Vehicle _currentVehicle;
        private Sequence _sequence;

        public void Initialize(bool isInvisible, ParkingLotPosition parkingLotPosition)
        {
            _isInvisible = isInvisible;
            if (_isInvisible)
            {
                gameObject.SetActive(false);
            }
            _parkingLotPosition = parkingLotPosition;
        }
        
        public void Occupy(Vehicle vehicle, ParkingLot from, bool withAnimation, UniTaskCompletionSource ucs)
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
                OccupyAnimation(ucs, from);
            }
        }


        private void OccupyAnimation(UniTaskCompletionSource ucs, ParkingLot from)
        {
            _sequence?.Kill(true);
            _sequence = DOTween.Sequence();
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
                        _sequence.Append(_currentVehicle.transform.DOMove(transform.position, 0.25f).SetEase(Ease.Linear));
                    }
                    else
                    {
                        var targetVector3 = transform.position;
                        _sequence.Append(_currentVehicle.transform
                            .DOMoveZ(targetVector3.z + (targetGridLineIndex == 0 ? -2 : 2), 0.25f)
                            .SetEase(Ease.Linear));
                     
                        _sequence.Append(_currentVehicle.transform.DOMoveX(targetVector3.x, 0.25f).SetEase(Ease.Linear));
                        _sequence.Append(_currentVehicle.transform.DOMoveZ(targetVector3.z, 0.25f)
                            .SetEase(Ease.Linear));
                    }
                }
                else
                {
                    _sequence.Append(_currentVehicle.transform.DOMove(transform.position, 0.25f).SetEase(Ease.Linear));
                }
            }
            else
            {
                var targetVector3 = transform.position;
                var midPointVector3 = (from.transform.position + targetVector3) / 2;
                
                _sequence.Append(_currentVehicle.transform
                    .DOMoveZ(midPointVector3.z, 0.25f)
                    .SetEase(Ease.Linear));
                if (Math.Abs(targetVector3.x - from.transform.position.x) > 0.01f)
                {
                    _sequence.Append(_currentVehicle.transform.DOMoveX(targetVector3.x, 0.25f).SetEase(Ease.Linear));
                }
                
                _sequence.Append(_currentVehicle.transform.DOMoveZ(targetVector3.z, 0.25f)
                    .SetEase(Ease.Linear));
            }

            _sequence.OnComplete(() =>
            {
                ucs.TrySetResult();
            });
        }

        private void OnMouseDown()
        {
            if (_isInvisible) return;
            OnParkingLotClicked?.Invoke(this, _currentVehicle);
        }

        public void SetEmpty()
        {
            _currentVehicle = null;
        }

        public bool IsEmpty()
        {
            return _currentVehicle == null;
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
            return _isInvisible || IsEmpty();
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