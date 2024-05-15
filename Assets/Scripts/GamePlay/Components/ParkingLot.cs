using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using GamePlay.Data.Grid;
using GamePlay.Extension;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GamePlay.Components
{
    public class ParkingLot : MonoBehaviour
    {
        private const float SecondsPerMeter = 0.085f;
        private const float MınTweenDuration = 0.25f;
        
        public EventHandler<Vehicle> OnParkingLotClicked;
        public EventHandler OnEmptied;
        [SerializeField] private ImageColorModifier imageColorModifier;
        
        private ParkingLotPosition _parkingLotPosition;
        private Vehicle _currentVehicle;
        private Sequence _sequence;
        
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
                    });
                    _isInvisible = false;
                }).SetDelay(0.15f);
                
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

        public Sequence OccupyAnimation(GridData gridData, Vehicle vehicle, UniTaskCompletionSource ucs, ParkingLot from,
            bool isFirstMove, bool isLastMove)
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
            var vehiclePos = vehicle.transform.position;

            if (targetGridGroupIndex == fromGridGroupIndex)
            {
                if (targetGridLineIndex == fromGridLineIndex)
                {
                    bool isNextToEachOther = Mathf.Abs(targetParkingLotIndex - fromParkingLotIndex) == 1;
                    if (isNextToEachOther)
                    {
                        float duration = GetDurationByDistance(transform.position, vehicle.transform.position);
                        _sequence.Append(vehicle.transform.DOMove(transform.position, duration)
                            .SetEase(isLastMove ? Ease.OutBack : Ease.Linear));
                    }
                    else
                    {
                        var targetParkingLotPosition = transform.position;
                        var gridGroup = gridData.gridGroups[targetGridGroupIndex];
                        float zOffsetOfMovement = CalculateZOffset(gridGroup, targetGridLineIndex);

                        List<Vector3> targetPositions = new List<Vector3>();
                        if (zOffsetOfMovement != 0)
                        {
                            targetPositions.Add(new Vector3(vehiclePos.x, vehiclePos.y, vehiclePos.z + zOffsetOfMovement));
                            targetPositions.Add(new Vector3(targetParkingLotPosition.x, vehiclePos.y,
                                vehiclePos.z + zOffsetOfMovement));
                            vehicle.ReverseSeats();
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
                            _sequence.Append(subSequence);

                            if (i + 2 <= targetPositions.Count)
                            {
                                var rotation = CalculateRotation(i == 0 ? vehiclePos : targetPositions[i - 1],
                                    targetPositions[i],
                                    targetPositions[i + 1]);

                                _sequence.Insert(totalTime + targetDurations[i] * 0.75f, vehicle.transform.DORotate(
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
                    _sequence.Append(vehicle.transform.DOMove(transform.position, duration)
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
                    _sequence.Append(subSequence);

                    if (i + 2 <= targetPositions.Count)
                    {
                        var rotation = CalculateRotation(i == 0 ? vehiclePos : targetPositions[i - 1],
                            targetPositions[i],
                            targetPositions[i + 1]);

                        _sequence.Insert(totalTime + targetDurations[i] * 0.75f, vehicle.transform.DORotate(
                            new Vector3(0, rotation, 0)
                            , targetDurations[i] * 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
                    }

                    totalTime += targetDurations[i];
                }
            }

            _sequence.OnComplete(() => { ucs?.TrySetResult(); });
            return _sequence;
        }

        float GetDurationByDistance(Vector3 pos1, Vector3 pos2)
        {
            float distance = Vector3.Distance(pos1, pos2);
            float duration = distance * SecondsPerMeter;
            float finalValue = Mathf.Max(duration, MınTweenDuration);

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
            OnEmptied?.Invoke(this, null);
        }

        public bool IsEmpty()
        {
            return _currentVehicle == null;
        }

        public bool IsWalkable()
        {
            return !_isObstacle && !_willOccupied & (_isInvisible || IsEmpty());
        }

        public bool IsInvisible()
        {
            return _isInvisible;
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

            var offset = CalculateZOffset(gridData.gridGroups[_parkingLotPosition.GetGridGroupIndex()],
                _parkingLotPosition.GetGridLineIndex());


            var vehicle = _currentVehicle;
            var gridGroup = gridData.gridGroups[_parkingLotPosition.GetGridGroupIndex()];
            var parkingLotGridLineIndex = _parkingLotPosition.GetGridLineIndex();
            var parkingLotIndex = _parkingLotPosition.GetParkingLotIndex();
            _sequence?.Kill(true);
            _sequence = DOTween.Sequence();
            if (offset != 0)
            {
                _sequence.Append(vehicle.transform.DOMoveZ(vehicle.transform.position.z + offset, 0.35f)
                    .SetEase(Ease.InBack));
                var parkingLotCount = gridGroup.lines[parkingLotGridLineIndex].parkingLots.Count - 1;
                bool isRightTurn = parkingLotIndex > parkingLotCount / 2;
                _sequence.Append(vehicle.transform.DOMoveX(isRightTurn ? 10 : -10, 0.75f).SetEase(Ease.Linear));
                _sequence.Insert(0.2f,
                    vehicle.transform
                        .DORotate(new Vector3(0, isRightTurn ? offset > 0 ? 90 : -90 : offset > 0 ? -90 : 90, 0), 0.3f,
                            RotateMode.LocalAxisAdd)
                        .SetEase(Ease.InOutQuad));
            }
            else
            {
                var hasUpperRoad = gridGroup.hasUpperRoad;
                var hasLowerRoad = gridGroup.hasLowerRoad;
                bool isCloseToUpperRoad = parkingLotGridLineIndex >= (gridGroup.lines.Count - 1) / 2;

                bool hasDirectPathToUp = HasDirectRoadAvailable(gridGroup, parkingLotGridLineIndex, parkingLotIndex,
                    true);
                bool hasDirectPathToDown = HasDirectRoadAvailable(gridGroup, parkingLotGridLineIndex, parkingLotIndex,
                    false);


                if (hasDirectPathToUp || hasDirectPathToDown)
                {
                    var lineIndex = hasDirectPathToUp && isCloseToUpperRoad ? gridGroup.lines.Count - 1
                        : hasDirectPathToDown && !isCloseToUpperRoad ? 0
                        : hasDirectPathToUp ? gridGroup.lines.Count - 1 : 0;

                    var newOffset =
                        CalculateZOffset(gridGroup, lineIndex);
                    var newTargetPosition = gridGroup.lines[lineIndex].parkingLots[parkingLotIndex].transform
                        .position.z + newOffset;
                    _sequence.Append(vehicle.transform.DOMoveZ(newTargetPosition, 0.35f)
                        .SetEase(Ease.InBack));
                    var parkingLotCount = gridGroup.lines[lineIndex].parkingLots.Count - 1;
                    bool isRightTurn = parkingLotIndex > parkingLotCount / 2;
                    _sequence.Append(vehicle.transform.DOMoveX(isRightTurn ? 10 : -10, 0.75f).SetEase(Ease.Linear));
                    _sequence.Insert(0.2f,
                        vehicle.transform.DORotate(
                                new Vector3(0, isRightTurn ? newOffset > 0 ? 90 : -90 : newOffset > 0 ? -90 : 90, 0),
                                0.3f,
                                RotateMode.LocalAxisAdd)
                            .SetEase(Ease.InOutQuad));
                }
                else
                {
           
                        List<ParkingLot> availableFrontParkingLots = new List<ParkingLot>();
                        List<GridLine> frontLines = new List<GridLine>();
                        if (hasUpperRoad)
                            frontLines.Add(gridGroup.lines[^1]);
                        if (hasLowerRoad)
                            frontLines.Add(gridGroup.lines[0]);

                        foreach (var line in frontLines)
                        {
                            for (int i = 0; i < line.parkingLots.Count; i++)
                            {
                                if (line.parkingLots[i].IsWalkable())
                                {
                                    availableFrontParkingLots.Add(line.parkingLots[i]);
                                }
                            }
                        }

                        List<List<ParkingLot>> paths = new List<List<ParkingLot>>();
                        foreach (var parkingLot in availableFrontParkingLots)
                        {
                            var path = gridData.FindPath(this, parkingLot);
                            if (path == null) continue;
                            path.RemoveAll(lot => lot == null);
                            if (path.Count > 0)
                                paths.Add(path);
                        }

                        if (paths.Count == 0)
                        {
                            _currentVehicle.SetCompleted();
                            return true; 
                        }

                        List<ParkingLot> shortestPath = null;
                        foreach (var path in paths)
                        {
                            if (shortestPath == null || path.Count < shortestPath.Count)
                            {
                                shortestPath = path;
                            }
                        }

                        if (shortestPath == null)
                        {
                            _currentVehicle.SetCompleted();
                            return true; 
                        }

                        ParkingLot lastMovedParkingLot = this;
                        var subSequence = DOTween.Sequence();
                        foreach (var pLot in shortestPath)
                        {
                            if (pLot == this)
                            {
                                continue;
                            }

                            subSequence.Append(pLot.OccupyAnimation(gridData, vehicle, null, lastMovedParkingLot,
                                pLot == shortestPath[0], pLot == shortestPath[^1]));
                            lastMovedParkingLot = pLot;
                        }

                        _sequence.Append(subSequence);
                        var lastParkingLot = shortestPath[^1];
                        var newOffset =
                            CalculateZOffset(gridGroup, lastParkingLot.GetParkingLotPosition().GetGridLineIndex());
                        var secondSequence = DOTween.Sequence();
                        secondSequence.Append(vehicle.transform
                            .DOMoveZ(lastParkingLot.transform.position.z + newOffset, 0.35f)
                            .SetEase(Ease.InBack));
                        var parkingLotCount = gridGroup.lines[lastParkingLot.GetParkingLotPosition().GetGridLineIndex()]
                            .parkingLots.Count - 1;
                        bool isRightTurn = lastParkingLot.GetParkingLotPosition().GetParkingLotIndex() >
                                           parkingLotCount / 2;
                        secondSequence.Append(vehicle.transform.DOMoveX(isRightTurn ? 10 : -10, 0.75f)
                            .SetEase(Ease.Linear));
                        secondSequence.Insert(0.2f,
                            vehicle.transform.DORotate(
                                    new Vector3(0, isRightTurn ? newOffset > 0 ? 90 : -90 : newOffset > 0 ? -90 : 90,
                                        0), 0.3f,
                                    RotateMode.LocalAxisAdd)
                                .SetEase(Ease.InOutQuad));
                        _sequence.Append(secondSequence);
                }
            }

            _currentVehicle.SetCompleted();
            _sequence.OnComplete(() => { vehicle.Destroy(); });
            SetEmpty();
            return true;
            // COMPLETE
        }
        
        private bool HasDirectRoadAvailable(GridGroup gridGroup, int parkingLotGridLineIndex, int parkingLotIndex,
            bool isUpper)
        {
            if (isUpper && !gridGroup.hasUpperRoad)
                return false;
            if (!isUpper && !gridGroup.hasLowerRoad)
                return false;

            for (int i = 0; i < gridGroup.lines.Count; i++)
            {
                if (isUpper ? i > parkingLotGridLineIndex : i < parkingLotGridLineIndex)
                {
                    if (!gridGroup.lines[i].parkingLots[parkingLotIndex].IsWalkable())
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}