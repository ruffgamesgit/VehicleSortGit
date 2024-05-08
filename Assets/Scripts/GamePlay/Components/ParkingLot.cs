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

        public void MoveAnimation(Vehicle vehicle, UniTaskCompletionSource ucs, ParkingLot from, bool isFirstMove)
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
                        sequence.Append(vehicle.transform.DOMove(transform.position, 0.25f).SetEase(Ease.Linear));
                    }
                    else
                    {
                        var targetVector3 = transform.position;
                        Vector3 vehiclePos = vehicle.transform.position;
                        float tweenDuration = .35f;


                        Ease ease1 = Ease.InBack;
                        Vector3 pos1 = new Vector3(vehiclePos.x, vehiclePos.y, targetVector3.z + (targetGridLineIndex == 0 ? -2 : 2));
                        goingDown = vehicle.transform.position.z > pos1.z ? false : true;
                        sequence.Append(vehicle.transform.DOMove(pos1, tweenDuration).SetEase(ease1));

                        float additionalRot = goingDown == true ? -15 : 15;
                        //  sequence.Join(vehicle.transform.DORotate(new Vector3(vehicle.transform.rotation.x + additionalRot, vehicle.transform.rotation.y, vehicle.transform.rotation.z), .25f));

                        Vector3 pos2 = new Vector3(targetVector3.x, vehiclePos.y, targetVector3.z + (targetGridLineIndex == 0 ? -2 : 2));
                        sequence.Append(vehicle.transform.DOMoveX(targetVector3.x, tweenDuration).SetEase(Ease.Linear));
                        DOTween.To((xx) => { }, 0, 1, tweenDuration).OnComplete(() =>
                         {
                             vehicle.transform.forward = -(pos2 - pos1);
                         });

                        Ease ease2 = Ease.OutBack;
                        Vector3 pos3 = new Vector3(targetVector3.x, vehiclePos.y, targetVector3.z);
                        sequence.Append(vehicle.transform.DOMove(pos3, tweenDuration).SetEase(ease2));
                        //  sequence.Join(vehicle.transform.DORotate(new Vector3(vehicle.transform.rotation.x + (-additionalRot), vehicle.transform.rotation.y, vehicle.transform.rotation.z), .25f));
                        DOTween.To((xx) => { }, 0, 1, tweenDuration * 2).OnComplete(() =>
                        {
                            vehicle.transform.forward = -(pos3 - pos2);
                        });
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
                Vector3 vehiclePos = vehicle.transform.position;
                float tweenDuration = .35f;
                goingDown = vehicle.transform.position.z > targetVector3.z ? true : false;

                Vector3 pos1 = new Vector3(vehiclePos.x, vehiclePos.y, midPointVector3.z);
                sequence.Append(vehicle.transform.DOMove(pos1, tweenDuration).SetEase(Ease.InBack))
                    .OnStart(() => vehicle.transform.forward = -(pos1 - vehiclePos));

                Vector3 pos2 = Vector3.zero;
                if (Mathf.Abs(targetVector3.x - from.transform.position.x) > 0.01f)
                {
                    pos2 = new Vector3(targetVector3.x, vehiclePos.y, midPointVector3.z);
                    sequence.Append(vehicle.transform.DOMoveX(targetVector3.x, tweenDuration).SetEase(Ease.Linear));
                    DOTween.To((xx) => { }, 0, 1, tweenDuration).OnComplete(() =>
                        {
                            vehicle.transform.forward = -(pos2 - pos1);
                        });
                }

                Vector3 pos3 = new Vector3(targetVector3.x, vehicle.transform.position.y, targetVector3.z);
                Ease ease = pos2 == Vector3.zero ? Ease.Linear : Ease.OutBack;
                tweenDuration = Mathf.Approximately(Vector3.Distance(pos2, Vector3.zero), 0) ? tweenDuration : (tweenDuration * 2);
                sequence.Append(vehicle.transform.DOMove(pos3, tweenDuration).SetEase(ease));
                DOTween.To((xx) => { }, 0, 1, tweenDuration).OnComplete(() =>
               {
                   vehicle.transform.forward = -(pos3 - (pos2 == Vector3.zero ? pos1 : pos2));
               });
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