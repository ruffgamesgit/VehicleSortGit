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
        [SerializeField] private ImageColorModifier imageColorModifier;
        private ParkingLotPosition _parkingLotPosition;
        private Vehicle _currentVehicle;
        private Sequence _sequence;
        private bool _isInvisible;
        private bool _willOccupied;
        const float DURATION_FOR_PER_METER = 0.085f;
        const float TWEEN_DURATION_DIVIDER = 10;
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

        public void SetPossibleTargetHighLight(bool activate)
        {
            imageColorModifier.SetHighlight(activate);
        }

        public void MoveAnimation(Vehicle vehicle, UniTaskCompletionSource ucs, ParkingLot from, bool isFirstMove, bool isLastMove)
        {

            var sequence = DOTween.Sequence();
            var fromPosition = from.GetParkingLotPosition();
            var targetGridGroupIndex = _parkingLotPosition.GetGridGroupIndex();
            var fromGridGroupIndex = fromPosition.GetGridGroupIndex();
            var targetGridLineIndex = _parkingLotPosition.GetGridLineIndex();
            var fromGridLineIndex = fromPosition.GetGridLineIndex();
            var targetParkingLotIndex = _parkingLotPosition.GetParkingLotIndex();
            var fromParkingLotIndex = fromPosition.GetParkingLotIndex();
            bool goingDown;

            if (targetGridGroupIndex == fromGridGroupIndex)
            {
                if (targetGridLineIndex == fromGridLineIndex)
                {
                    bool isNext = Mathf.Abs(targetParkingLotIndex - fromParkingLotIndex) == 1;
                    if (isNext)
                    {
                        float duration = GetDurationByDistance(transform.position, vehicle.transform.position);
                        sequence.Append(vehicle.transform.DOMove(transform.position, duration).SetEase(isLastMove ? Ease.OutBack : Ease.Linear));
                    }
                    else
                    {
                        var targetVector3 = transform.position;
                        Vector3 vehiclePos = vehicle.transform.position;
                        float tweenDuration = .35f;
                        #region First Movement

                        Vector3 pos1 = new Vector3(vehicle.transform.position.x, vehicle.transform.position.y, targetVector3.z + (targetGridLineIndex == 0 ? -2 : 2));
                        tweenDuration = GetDurationByDistance(pos1, vehiclePos);
                        sequence.Append(vehicle.transform.DOMove(pos1, tweenDuration).SetEase(isFirstMove ? Ease.InBack : Ease.Linear)
                            .OnStart(() => vehicle.transform.forward = -(pos1 - vehiclePos)));

                        #endregion

                        goingDown = vehicle.transform.position.z > pos1.z ? false : true;
                        #region Second Movement

                        //  Debug.Log("Going down: " + goingDown);
                        Vector3 pos2 = new Vector3(targetVector3.x, vehicle.transform.position.y, targetVector3.z + (targetGridLineIndex == 0 ? -2 : 2));
                        tweenDuration = GetDurationByDistance(pos1, pos2);
                        sequence.Append(vehicle.transform.DOMoveX(targetVector3.x, tweenDuration).SetEase(Ease.Linear));
                        sequence.Join(DOTween.To((xx) => { }, 0, 1, tweenDuration / TWEEN_DURATION_DIVIDER).OnComplete(() =>
                        {
                            vehicle.transform.forward = -(pos2 - pos1);
                        }));

                        #endregion

                        #region Third Movement

                        Vector3 pos3 = new Vector3(targetVector3.x, vehicle.transform.position.y, targetVector3.z);
                        tweenDuration = GetDurationByDistance(pos3, pos2);
                        sequence.Append(vehicle.transform.DOMove(pos3, tweenDuration * 2).SetEase(isLastMove ? Ease.OutBack : Ease.Linear));
                        sequence.Join(DOTween.To((xx) => { }, 0, 1, tweenDuration / TWEEN_DURATION_DIVIDER).OnComplete(() =>
                        {
                            vehicle.transform.forward = -(pos3 - (pos2 == Vector3.zero ? pos1 : pos2));
                        }));

                        #endregion
                    }
                }
                else
                {
                    float duration = GetDurationByDistance(transform.position, vehicle.transform.position);
                    sequence.Append(vehicle.transform.DOMove(transform.position, duration).SetEase(isFirstMove ? Ease.InBack : Ease.Linear));
                    sequence.Join(DOTween.To((xx) => { }, 0, 1, duration).OnComplete(() =>
                    {
                        vehicle.transform.forward = (transform.position - vehicle.transform.position);
                    }));

                }
            }
            else
            {

                var targetVector3 = transform.position;
                var midPointVector3 = (from.transform.position + targetVector3) / 2;
                Vector3 vehiclePos = vehicle.transform.position;
                float tweenDuration = 5f;
                goingDown = vehicle.transform.position.z > targetVector3.z ? true : false;

                #region First Movement

                Vector3 pos1 = new Vector3(vehiclePos.x, vehiclePos.y, midPointVector3.z);
                tweenDuration = GetDurationByDistance(pos1, vehiclePos);

                sequence.Append(vehicle.transform.DOMove(pos1, tweenDuration).SetEase(isFirstMove ? Ease.InBack : Ease.Linear))
                    .OnStart(() => vehicle.transform.forward = -(pos1 - vehiclePos));

                #endregion


                #region Second Movement

                Vector3 pos2 = Vector3.zero;
                if (Mathf.Abs(targetVector3.x - from.transform.position.x) > 0.01f)
                {
                    pos2 = new Vector3(targetVector3.x, vehiclePos.y, midPointVector3.z);
                    tweenDuration = GetDurationByDistance(pos1, pos2);
                    sequence.Append(vehicle.transform.DOMoveX(targetVector3.x, tweenDuration).SetEase(Ease.Linear));
                    sequence.Join(DOTween.To((xx) => { }, 0, 1, tweenDuration / TWEEN_DURATION_DIVIDER).OnComplete(() =>
                    {
                        vehicle.transform.forward = -(pos2 - pos1);
                    }));

                }
                #endregion


                #region Third Movement
                Vector3 pos3 = new Vector3(targetVector3.x, vehicle.transform.position.y, targetVector3.z);

                tweenDuration = GetDurationByDistance(pos2 == Vector3.zero ? pos1 : pos2, vehiclePos);
                //  tweenDuration = Mathf.Approximately(Vector3.Distance(pos2, Vector3.zero), 0) ? tweenDuration : (tweenDuration * 2);
                sequence.Append(vehicle.transform.DOMove(pos3, tweenDuration).SetEase(isLastMove ? Ease.OutBack : Ease.Linear));
                sequence.Join(DOTween.To((xx) => { }, 0, 1, tweenDuration / TWEEN_DURATION_DIVIDER).OnComplete(() =>
                {
                    vehicle.transform.forward = -(pos3 - (pos2 == Vector3.zero ? pos1 : pos2));
                }));

                #endregion
            }

            sequence.OnComplete(() =>
            {
                ucs.TrySetResult();
            });
        }

        float GetDurationByDistance(Vector3 pos1, Vector3 pos2)
        {
            float distance = Vector3.Distance(pos1, pos2);

            return distance * DURATION_FOR_PER_METER;
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