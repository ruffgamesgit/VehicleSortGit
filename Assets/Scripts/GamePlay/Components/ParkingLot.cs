using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using GamePlay.Data.Grid;
using UnityEditor;
using UnityEngine;

namespace GamePlay.Components
{
    public class ParkingLot : MonoBehaviour
    {
        public EventHandler<Vehicle> OnParkingLotClicked;
        public EventHandler OnEmptied;

        private ParkingLotPosition _parkingLotPosition;
        private Vehicle _currentVehicle;
        private bool _isInvisible;
        private bool _isObstacle;
        private bool _isEmptyAtStart;
        private bool _willOccupied;
        private MeshRenderer _modelMeshRenderer;
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

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
            _modelMeshRenderer = GetComponentInChildren<MeshRenderer>();
            //  SetPossibleTargetHighLight(false, false);
        }

        public void Occupy(Vehicle vehicle, bool moveTransform, Action onComplete = null)
        {
            _willOccupied = false;
            _currentVehicle = vehicle;
            _currentVehicle.transform.parent = this.transform;
            if (moveTransform)
            {
                _isInvisible = true;
                _currentVehicle.transform.DOMove(transform.position, 0.45f).SetEase(Ease.InOutBack).OnComplete(() =>
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
            Material material = _modelMeshRenderer.materials[1];

            Color color = material.GetColor(BaseColorID);
            color.a = activate ? 100 / 255f : 0; // Alpha values are between 0 and 1, so divide by 255.
            material.SetColor(BaseColorID, color);
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
            OnEmptied?.Invoke(this, null);
        }

        public bool IsEmpty()
        {
            return _currentVehicle == null;
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
            if (_currentVehicle == null) return false;
            var seats = _currentVehicle.GetSeats();

            foreach (var seat in seats)
            {
                if (seat.IsEmpty())
                    return false;

                if (seat.GetPassenger().GetColor() != seats[0].GetPassenger().GetColor())
                    return false;
            }

            if (_currentVehicle.CompletedAnimation(gridData, this))
            {
                SetEmpty();
            }

            return true;
        }
    }
}