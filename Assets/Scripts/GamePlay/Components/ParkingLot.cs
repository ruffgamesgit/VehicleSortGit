using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using GamePlay.Data.Grid;
using UnityEngine;

namespace GamePlay.Components
{
    public class ParkingLot : MonoBehaviour
    {
        public EventHandler<Vehicle> OnParkingLotClicked;
        public EventHandler OnEmptied;
        [SerializeField] private ImageColorModifier imageColorModifier;
        
        private ParkingLotPosition _parkingLotPosition;
        [SerializeField] private Vehicle currentVehicle;
        [SerializeField] private List<ParkingLot> manuallyAssignedNeighbours = new List<ParkingLot>();
        private bool _isInvisible;
        private bool _isObstacle;
        private bool _isEmptyAtStart;
        private bool _willOccupied;

        public void Initialize(bool isInvisible, bool isObstacle, bool isEmptyAtStart,
            ParkingLotPosition parkingLotPosition)
        {
            _isInvisible = isInvisible;
            _isObstacle = isObstacle;
            _isEmptyAtStart = isEmptyAtStart;
            if (_isInvisible)
            {
                gameObject.SetActive(false);
            }

            _parkingLotPosition = parkingLotPosition;
        }

        public void Occupy(Vehicle vehicle, bool moveTransform, Action onComplete = null)
        {
            _willOccupied = false;
            currentVehicle = vehicle;
            currentVehicle.transform.parent = this.transform;
            if (moveTransform)
            {
                _isInvisible = true;
                currentVehicle.transform.DOMove(transform.position, 0.45f).SetEase(Ease.InOutBack).OnComplete(() =>
                {
                    DOVirtual.DelayedCall(0.2f, () =>
                    {
                        onComplete?.Invoke();
                        _isInvisible = false;
                    });
                }).SetDelay(0.15f);
            }
        }


        public void SetPossibleTargetHighLight(bool activate, bool isPossibleMove)
        {
            imageColorModifier.SetHighlight(activate);
        }


        public Sequence OccupyAnimation(GridData gridData, Vehicle vehicle, UniTaskCompletionSource ucs,
            ParkingLot from,
            bool isFirstMove, bool isLastMove)
        {
            return vehicle.MoveAnimation(gridData, ucs, from, this, isFirstMove, isLastMove);
        }


        private void OnMouseDown()
        {
            if (_isInvisible) return;
            if (_parkingLotPosition.GetGridGroupIndex() == 1) return; // FOR MOCK UP

            if (IsAnimationOn() || _willOccupied)
            {
                OnParkingLotClicked?.Invoke(null, null);
                return;
            }

            OnParkingLotClicked?.Invoke(this, currentVehicle);
        }

        private bool IsAnimationOn()
        {
            if (currentVehicle == null) return false;
            return currentVehicle.IsAnimationOn();
        }

        public Vehicle GetCurrentVehicle()
        {
            return currentVehicle;
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
            currentVehicle = null;
            OnEmptied?.Invoke(this, null);
        }

        public bool IsEmpty()
        {
            return currentVehicle == null;
        }

        public bool IsWalkable()
        {
            return !IsObstacle() && !_willOccupied & (_isInvisible || IsEmpty());
        }

        public bool IsInvisible()
        {
            return _isInvisible;
        }

        public bool IsObstacle()
        {
            return _isObstacle;
        }


        public bool IsEmptyAtStart()
        {
            return _isEmptyAtStart;
        }

        public bool CheckIfCompleted(GridData gridData)
        {
            if (currentVehicle == null) return false;
            var seats = currentVehicle.GetSeats();

            foreach (var seat in seats)
            {
                if (seat.IsEmpty())
                    return false;

                if (seat.GetPassenger().GetColor() != seats[0].GetPassenger().GetColor())
                    return false;
            }

            if (currentVehicle.CompletedAnimation(gridData, this))
            {
                SetEmpty();
            }

            return true;
        }
    }
}