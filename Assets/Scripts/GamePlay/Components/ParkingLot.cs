using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using GamePlay.Data.Grid;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        private bool _isObstacle;
        private bool _isEmptyAtStart;
        private bool _willOccupied;
        const float SECONDS_FOR_PER_METER = 0.085f;
        const float MIN_TWEEN_DURATION = 0.25f;
        const float TWEEN_DURATION_DIVIDER = 10;

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

        private int CalculateRotation(Vector3 current, Vector3 target, Vector3 next)
        {
            bool isFirstMoveVertical = Mathf.Approximately(target.x, current.x);
            bool isVerticalMoveGoingUp = isFirstMoveVertical ? target.z - current.z > 0 : next.z - target.z > 0;
            bool isHorizontalMoveGoingRight = isFirstMoveVertical ? next.x - target.x > 0 : target.x - current.x > 0;

            return isFirstMoveVertical ? isVerticalMoveGoingUp ? isHorizontalMoveGoingRight ? 90 : -90 :
                isHorizontalMoveGoingRight ? -90 : 90
                : isHorizontalMoveGoingRight ? isVerticalMoveGoingUp ? -90 : 90
                : isVerticalMoveGoingUp ? 90 : -90;
        }

        private float CalculateZOffset(GridGroup gridGroup, int lineIndex)
        {
            if (lineIndex == 0)
            {
                if (gridGroup.lines.Count == 1)
                {
                    return gridGroup.hasUpperRoad ? 1.65f : gridGroup.hasLowerRoad ? -1.65f : 0f;
                }

                return gridGroup.hasLowerRoad ? -1.65f : 0f;
            }

            if (lineIndex == gridGroup.lines.Count - 1)
            {
                return gridGroup.hasUpperRoad ? 1.65f : 0f;
            }

            return 0;
        }

        public void MoveAnimation(GridData gridData, Vehicle vehicle, UniTaskCompletionSource ucs, ParkingLot from,
            bool isFirstMove, bool isLastMove)
        {
            var sequence = DOTween.Sequence();
            var fromPosition = from.GetParkingLotPosition();
            var targetGridGroupIndex = _parkingLotPosition.GetGridGroupIndex();
            var fromGridGroupIndex = fromPosition.GetGridGroupIndex();
            var targetGridLineIndex = _parkingLotPosition.GetGridLineIndex();
            var fromGridLineIndex = fromPosition.GetGridLineIndex();
            var targetParkingLotIndex = _parkingLotPosition.GetParkingLotIndex();
            var fromParkingLotIndex = fromPosition.GetParkingLotIndex();
            var vehiclePos = vehicle.transform.position;

            if (targetGridGroupIndex == fromGridGroupIndex)
            {
                if (targetGridLineIndex == fromGridLineIndex)
                {
                    bool isNextToEachOther = Mathf.Abs(targetParkingLotIndex - fromParkingLotIndex) == 1;
                    if (isNextToEachOther)
                    {
                        float duration = GetDurationByDistance(transform.position, vehicle.transform.position);
                        sequence.Append(vehicle.transform.DOMove(transform.position, duration)
                            .SetEase(isLastMove ? Ease.OutBack : Ease.Linear));
                    }
                    else
                    {
                        var targetParkingLotPosition = transform.position;
                        var gridGroup = gridData.gridGroups[targetGridGroupIndex];
                        float zOffsetOfMovement = CalculateZOffset(gridGroup, targetGridLineIndex);

                        List<Vector3> targetPositions = new List<Vector3>();
                        targetPositions.Add(new Vector3(vehiclePos.x, vehiclePos.y, vehiclePos.z + zOffsetOfMovement));
                        targetPositions.Add(new Vector3(targetParkingLotPosition.x, vehiclePos.y,
                            vehiclePos.z + zOffsetOfMovement));
                        targetPositions.Add(new Vector3(targetParkingLotPosition.x, vehicle.transform.position.y,
                            targetParkingLotPosition.z));

                        List<float> targetDurations = new List<float>();
                        targetDurations.Add(GetDurationByDistance(vehiclePos, targetPositions[0]));
                        for (int i = 1; i < targetPositions.Count; i++)
                        {
                            targetDurations.Add(GetDurationByDistance(targetPositions[i - 1], targetPositions[i]));
                        }

                        float totalTime = 0;
                        for (int i = 0; i < targetPositions.Count; i++)
                        {
                            var subSequence = DOTween.Sequence();

                            subSequence.Append(vehicle.transform.DOMove(targetPositions[i], targetDurations[i])
                                .SetEase(isFirstMove && i == 0 ? Ease.InBack :
                                    isLastMove && i == targetPositions.Count - 1 ? Ease.OutBack : Ease.Linear));
                            sequence.Append(subSequence);

                            if (i + 2 <= targetPositions.Count)
                            {
                                var rotation = CalculateRotation(i == 0 ? vehiclePos : targetPositions[i - 1],
                                    targetPositions[i],
                                    targetPositions[i + 1]);

                                sequence.Insert(totalTime + targetDurations[i] * 0.75f, vehicle.transform.DORotate(
                                    new Vector3(0, rotation, 0)
                                    , targetDurations[i] * 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
                            }

                            totalTime += targetDurations[i];
                        }
                    }
                }
                else
                {
                    float duration = GetDurationByDistance(transform.position, vehicle.transform.position);
                    sequence.Append(vehicle.transform.DOMove(transform.position, duration)
                        .SetEase(isFirstMove ? Ease.InBack : Ease.Linear));
                }
            }
            else
            {
                var targetParkingLotPosition = transform.position;
                var midPointVector3 = (from.transform.position + targetParkingLotPosition) / 2;

                List<Vector3> targetPositions = new List<Vector3>();

                if (Mathf.Abs(vehiclePos.x - targetParkingLotPosition.x) > 0.05)
                {
                    targetPositions.Add(new Vector3(vehiclePos.x, vehiclePos.y, midPointVector3.z));
                    targetPositions.Add(new Vector3(targetParkingLotPosition.x, vehiclePos.y,
                        midPointVector3.z));
                }

                targetPositions.Add(new Vector3(targetParkingLotPosition.x, vehicle.transform.position.y,
                    targetParkingLotPosition.z));

                List<float> targetDurations = new List<float>();
                targetDurations.Add(GetDurationByDistance(vehiclePos, targetPositions[0]));
                for (int i = 1; i < targetPositions.Count; i++)
                {
                    targetDurations.Add(GetDurationByDistance(targetPositions[i - 1], targetPositions[i]));
                }

                float totalTime = 0;
                for (int i = 0; i < targetPositions.Count; i++)
                {
                    var subSequence = DOTween.Sequence();

                    subSequence.Append(vehicle.transform.DOMove(targetPositions[i], targetDurations[i])
                        .SetEase(isFirstMove && i == 0 ? Ease.InBack :
                            isLastMove && i == targetPositions.Count - 1 ? Ease.OutBack : Ease.Linear));
                    sequence.Append(subSequence);

                    if (i + 2 <= targetPositions.Count)
                    {
                        var rotation = CalculateRotation(i == 0 ? vehiclePos : targetPositions[i - 1],
                            targetPositions[i],
                            targetPositions[i + 1]);

                        sequence.Insert(totalTime + targetDurations[i] * 0.75f, vehicle.transform.DORotate(
                            new Vector3(0, rotation, 0)
                            , targetDurations[i] * 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
                    }

                    totalTime += targetDurations[i];
                }
            }

            sequence.OnComplete(() => { ucs.TrySetResult(); });
        }

        float GetDurationByDistance(Vector3 pos1, Vector3 pos2)
        {
            float distance = Vector3.Distance(pos1, pos2);
            float duration = distance * SECONDS_FOR_PER_METER;
            float finalValue = Mathf.Max(duration, MIN_TWEEN_DURATION);

            return finalValue * 1;
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
            return !_isObstacle && (_isInvisible || IsEmpty());
        }

        public bool IsInvisible()
        {
            return _isInvisible;
        }

        public bool IsEmptyAtStart()
        {
            return _isEmptyAtStart;
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