using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Components.SortController;
using GamePlay.Data;
using GamePlay.Data.Grid;
using GamePlay.Extension;
using UnityEngine;

namespace GamePlay.Components
{
    public class Vehicle : MonoBehaviour
    {
        private const float SecondsPerMeter = 0.085f;
        private const float MınTweenDuration = 0.25f;

        [SerializeField] private List<Seat> seats = new List<Seat>(4);
        [SerializeField] private Outline outline;
        [SerializeField] private Transform busTopObject;
        private bool _isCompleted = false;
        private Sequence _moveSequence;
        private Sequence _goOutSequence;
        private Sequence _vehicleCompleteIdleSequence;

        public Dictionary<ColorEnum, int> GetExistingColors()
        {
            var colorCount = new Dictionary<ColorEnum, int>();
            foreach (var seat in seats)
            {
                if (seat.IsEmpty()) continue;
                var color = seat.GetPassenger().GetColor();
                if (!colorCount.TryAdd(color, 1))
                {
                    colorCount[color]++;
                }
            }

            return colorCount;
        }

        public void SetHighlight(bool active)
        {
            outline.HandleOutline(active ? 2.7f : 0f);
        }

        public bool IsCompleted()
        {
            return _isCompleted;
        }

        public void SetCompleted()
        {
            _isCompleted = true;
        }

        public Sequence OnBusIsCompleted()
        {
            if (busTopObject.gameObject.activeInHierarchy) return null;

            var sequence = DOTween.Sequence();
            sequence.Join(transform.DOScaleY(1.015f, .43f)).SetLoops(-1, LoopType.Yoyo);
            busTopObject.gameObject.SetActive(true);
            busTopObject.DOScaleZ(1, .2f);
            return sequence;
        }

        public List<Seat> GetSeats()
        {
            return seats;
        }

        public bool IsAllEmpty()
        {
            return seats.FindAll(s => s.IsEmpty()).Count == 4;
        }

        public bool HasEmptySeat()
        {
            foreach (var seat in seats)
            {
                if (seat.IsEmpty())
                    return true;
            }

            return false;
        }

        public bool IsAnimationOn()
        {
            foreach (var seat in seats)
            {
                if (seat.IsAnimating())
                    return true;
            }

            return false;
        }

        public async UniTask SortByType(bool instant)
        {
            if (IsAllEmpty()) return;

            await UniTask.WaitUntil(() =>!IsAnimationOn());
            HashSet<Seat> swappingAnimationList = new HashSet<Seat>();
            if (HasEmptySeat())
            {
                var emptySeats = seats.FindAll(seat => seat.IsEmpty());
                for (int i = seats.Count - 1; i > seats.Count - emptySeats.Count - 1; i--)
                {
                    if (!seats[i].IsEmpty())
                    {
                        var swappingTarget = seats.First(s => s.IsEmpty());
                        seats[i].Swap(swappingTarget);
                        swappingAnimationList.Add(seats[i]);
                        swappingAnimationList.Add(swappingTarget);
                    }
                }
            }

            var colorCount = GetExistingColors();
            if (colorCount.Count == 1) goto finalize;
            iterate:
            if (colorCount.AreAllValuesEqual())
            {
                if (colorCount.Values.ToArray()[0] == 1)
                {
                    goto finalize;
                }

                if (seats[0].GetPassenger().GetColor() == seats[1].GetPassenger().GetColor())
                {
                    goto finalize;
                }
            }

            var highestValueColor = colorCount.GetMaxValue();
            var highestValueCount = colorCount[highestValueColor];
            colorCount.Remove(highestValueColor);

            for (int i = 0; i < highestValueCount; i++)
            {
                if (seats[i].GetPassenger().GetColor() != highestValueColor)
                {
                    var swappingTarget = seats.Last(s =>
                        s.GetPassenger() != null && s.GetPassenger().GetColor() == highestValueColor);
                    if (swappingTarget == seats[i]) continue;
                    swappingAnimationList.Add(seats[i]);
                    swappingAnimationList.Add(swappingTarget);
                    seats[i].Swap(swappingTarget);
                }
            }

            if (colorCount.Count > 0)
                goto iterate;

            finalize:

            if (swappingAnimationList.Count > 0)
            {
                await swappingAnimationList.ToList().AnimateSeatChanges(instant);
            }
        }

        private void ReverseSeats()
        {
            if (IsAllEmpty()) return;
            if (!HasEmptySeat() && GetExistingColors().AreAllValuesEqual())
            {
                goto rotatePassengers;
            }

            seats.Reverse(0, seats.Count);
            seats[^1].Swap(seats[0]);
            seats[^2].Swap(seats[1]);
            seats.AnimateSeatChanges(false);

            rotatePassengers:
            foreach (var seat in seats)
            {
                seat.RotatePassengers();
            }
        }

        public Sequence MoveAnimation(GridData gridData, UniTaskCompletionSource ucs, ParkingLot from, ParkingLot to,
            bool isFirstMove, bool isLastMove)
        {
            _moveSequence?.Kill(true);

            _moveSequence = DOTween.Sequence();
            var parkingLotPosition = to.GetParkingLotPosition();
            var fromPosition = from.GetParkingLotPosition();
            var targetGridGroupIndex = parkingLotPosition.GetGridGroupIndex();
            var fromGridGroupIndex = fromPosition.GetGridGroupIndex();
            var targetGridLineIndex = parkingLotPosition.GetGridLineIndex();
            var fromGridLineIndex = fromPosition.GetGridLineIndex();
            var targetParkingLotIndex = parkingLotPosition.GetParkingLotIndex();
            var fromParkingLotIndex = fromPosition.GetParkingLotIndex();
            var vehiclePos = transform.position;

            if (targetGridGroupIndex == fromGridGroupIndex)
            {
                if (targetGridLineIndex == fromGridLineIndex)
                {
                    bool isNextToEachOther = Mathf.Abs(targetParkingLotIndex - fromParkingLotIndex) == 1;
                    if (isNextToEachOther)
                    {
                        float duration = GetDurationByDistance(transform.position, to.transform.position);
                        _moveSequence.Append(transform.DOMove(to.transform.position, duration)
                            .SetEase(isLastMove ? Ease.OutBack : Ease.Linear));
                    }
                    else
                    {
                        var targetParkingLotPosition = to.transform.position;
                        var gridGroup = gridData.gridGroups[targetGridGroupIndex];
                        float zOffsetOfMovement = CalculateZOffset(gridGroup, targetGridLineIndex);

                        List<Vector3> targetPositions = new List<Vector3>();
                        if (zOffsetOfMovement != 0)
                        {
                            targetPositions.Add(new Vector3(vehiclePos.x, vehiclePos.y,
                                vehiclePos.z + zOffsetOfMovement));
                            targetPositions.Add(new Vector3(targetParkingLotPosition.x, vehiclePos.y,
                                vehiclePos.z + zOffsetOfMovement));
                            ReverseSeats();
                        }

                        targetPositions.Add(new Vector3(targetParkingLotPosition.x, transform.position.y,
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

                            subSequence.Append(transform.DOMove(targetPositions[i], targetDurations[i])
                                .SetEase(isFirstMove && i == 0 ? Ease.InBack :
                                    isLastMove && i == targetPositions.Count - 1 ? Ease.OutBack : Ease.Linear));
                            _moveSequence.Append(subSequence);

                            if (i + 2 <= targetPositions.Count)
                            {
                                var rotation = CalculateRotation(i == 0 ? vehiclePos : targetPositions[i - 1],
                                    targetPositions[i],
                                    targetPositions[i + 1]);

                                _moveSequence.Insert(totalTime + targetDurations[i] * 0.75f, transform.DORotate(
                                    new Vector3(0, rotation, 0)
                                    , targetDurations[i] * 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
                            }

                            totalTime += targetDurations[i];
                        }
                    }
                }
                else
                {
                    float duration = GetDurationByDistance(from.transform.position, to.transform.position);
                    _moveSequence.Append(transform.DOMove(to.transform.position, duration)
                        .SetEase(isFirstMove ? Ease.InBack : Ease.Linear));
                }
            }
            else
            {
                var targetParkingLotPosition = to.transform.position;
                var midPointVector3 = (from.transform.position + targetParkingLotPosition) / 2;

                List<Vector3> targetPositions = new List<Vector3>();

                if (Mathf.Abs(vehiclePos.x - targetParkingLotPosition.x) > 0.05)
                {
                    targetPositions.Add(new Vector3(vehiclePos.x, vehiclePos.y, midPointVector3.z));
                    targetPositions.Add(new Vector3(targetParkingLotPosition.x, vehiclePos.y,
                        midPointVector3.z));
                }

                targetPositions.Add(new Vector3(targetParkingLotPosition.x, transform.position.y,
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

                    subSequence.Append(transform.DOMove(targetPositions[i], targetDurations[i])
                        .SetEase(isFirstMove && i == 0 ? Ease.InBack :
                            isLastMove && i == targetPositions.Count - 1 ? Ease.OutBack : Ease.Linear));
                    _moveSequence.Append(subSequence);

                    if (i + 2 <= targetPositions.Count)
                    {
                        var rotation = CalculateRotation(i == 0 ? vehiclePos : targetPositions[i - 1],
                            targetPositions[i],
                            targetPositions[i + 1]);

                        _moveSequence.Insert(totalTime + targetDurations[i] * 0.75f, transform.DORotate(
                            new Vector3(0, rotation, 0)
                            , targetDurations[i] * 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
                    }

                    totalTime += targetDurations[i];
                }
            }

            _moveSequence.OnComplete(() => { ucs?.TrySetResult(); });
            return _moveSequence;
        }

        public bool CompletedAnimation(GridData gridData, ParkingLot parkingLot)
        {
            var parkingLotPosition = parkingLot.GetParkingLotPosition();
            var offset = CalculateZOffset(gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()],
                parkingLotPosition.GetGridLineIndex());


            var gridGroup = gridData.gridGroups[parkingLotPosition.GetGridGroupIndex()];
            var parkingLotGridLineIndex = parkingLotPosition.GetGridLineIndex();
            var parkingLotIndex = parkingLotPosition.GetParkingLotIndex();
            _goOutSequence?.Kill();
            _goOutSequence = DOTween.Sequence();

            var completeAnimationSequence = OnBusIsCompleted();
            _vehicleCompleteIdleSequence = completeAnimationSequence ?? _vehicleCompleteIdleSequence;

            if (offset != 0)
            {
                _goOutSequence.Append(transform.DOMoveZ(transform.position.z + offset, 0.35f)
                    .SetEase(Ease.InBack));
                var parkingLotCount = gridGroup.lines[parkingLotGridLineIndex].parkingLots.Count - 1;
                bool isRightTurn = parkingLotIndex > parkingLotCount / 2;
                _goOutSequence.Append(transform.DOMoveX(isRightTurn ? 10 : -10, 0.75f).SetEase(Ease.Linear));
                _goOutSequence.Insert(0.2f,
                    transform
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
                    _goOutSequence.Append(transform.DOMoveZ(newTargetPosition, 0.35f)
                        .SetEase(Ease.InBack));
                    var parkingLotCount = gridGroup.lines[lineIndex].parkingLots.Count - 1;
                    bool isRightTurn = parkingLotIndex > parkingLotCount / 2;
                    _goOutSequence.Append(transform.DOMoveX(isRightTurn ? 10 : -10, 0.75f).SetEase(Ease.Linear));
                    _goOutSequence.Insert(0.2f,
                        transform.DORotate(
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
                    foreach (var availableParkingLot in availableFrontParkingLots)
                    {
                        var path = gridData.FindPath(parkingLot, availableParkingLot);
                        if (path == null) continue;
                        path.RemoveAll(lot => lot == null);
                        if (path.Count > 0)
                            paths.Add(path);
                    }

                    if (paths.Count == 0)
                    {
                        SetCompleted();
                        return false;
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
                        SetCompleted();
                        return false;
                    }

                    var lastMovedParkingLot = parkingLot;
                    var subSequence = DOTween.Sequence();
                    foreach (var pLot in shortestPath)
                    {
                        if (pLot == parkingLot)
                        {
                            continue;
                        }

                        subSequence.Append(pLot.OccupyAnimation(gridData, this, null, lastMovedParkingLot,
                            pLot == shortestPath[0], pLot == shortestPath[^1]));
                        lastMovedParkingLot = pLot;
                    }

                    _goOutSequence.Append(subSequence);
                    var lastParkingLot = shortestPath[^1];
                    var newOffset =
                        CalculateZOffset(gridGroup, lastParkingLot.GetParkingLotPosition().GetGridLineIndex());
                    var secondSequence = DOTween.Sequence();
                    secondSequence.Append(transform
                        .DOMoveZ(lastParkingLot.transform.position.z + newOffset, 0.35f)
                        .SetEase(Ease.InBack));
                    var parkingLotCount = gridGroup.lines[lastParkingLot.GetParkingLotPosition().GetGridLineIndex()]
                        .parkingLots.Count - 1;
                    bool isRightTurn = lastParkingLot.GetParkingLotPosition().GetParkingLotIndex() >
                                       parkingLotCount / 2;
                    secondSequence.Append(transform.DOMoveX(isRightTurn ? 10 : -10, 0.75f)
                        .SetEase(Ease.Linear));
                    secondSequence.Insert(0.2f,
                        transform.DORotate(
                                new Vector3(0, isRightTurn ? newOffset > 0 ? 90 : -90 : newOffset > 0 ? -90 : 90,
                                    0), 0.3f,
                                RotateMode.LocalAxisAdd)
                            .SetEase(Ease.InOutQuad));
                    _goOutSequence.Append(secondSequence);
                }
            }

            SetCompleted();
            _goOutSequence.OnComplete(Destroy);
            return true;
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

        float GetDurationByDistance(Vector3 pos1, Vector3 pos2)
        {
            float distance = Vector3.Distance(pos1, pos2);
            float duration = distance * SecondsPerMeter;
            float finalValue = Mathf.Max(duration, MınTweenDuration);

            return finalValue * 1;
        }

        public void Destroy()
        {
            Destroy(gameObject);
            _vehicleCompleteIdleSequence?.Kill();
            _goOutSequence?.Kill();
            _moveSequence?.Kill();
        }
    }
}