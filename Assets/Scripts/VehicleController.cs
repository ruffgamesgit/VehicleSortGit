using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{

    [Header("Debug")]
    public bool isPicked;
    public bool IsPerformingTransfer;
    public LotController CurrentLot;
    [HideInInspector] public bool Disappearing = false;

    [Header("References")]
    [SerializeField] PassengerStack stackPrefab;
    [SerializeField] List<PlacementPoint> placementPoints;
    public List<PassengerStack> CurrentPassengerStacks = new();
    [SerializeField] List<VehicleController> TargetVehiclesToMove = new();
    [SerializeField] List<ColorEnum> existingColorList = new();


    IEnumerator Start()
    {
        yield return new WaitForSeconds(.25f);

        SortInnerPlacement();

    }
    public void Initiliaze(int passengerStackCount)
    {
        CurrentLot = transform.parent.GetComponent<LotController>();

        for (int i = 0; i < passengerStackCount; i++)
        {
            PlacementPoint targetPoint = GetFirstAvailablePoint();

            if (targetPoint == null) break; // If there is no placeable point that
                                            // indicates the vehicle is full

            Vector3 spawnPos;
            spawnPos = targetPoint.transform.position;
            PassengerStack cloneStack = Instantiate(stackPrefab, spawnPos, Quaternion.identity, targetPoint.transform);
            CurrentPassengerStacks.Add(cloneStack);
            cloneStack.SetCurrentVehicleAndPlacementPoint(this, targetPoint);

            targetPoint.SetOccupied(true);
        }
    }
    #region GETTERS
    public void GetPicked()
    {
        isPicked = true;
        transform.DOMoveY(transform.position.y + 1, 0.25f);
    }
    public List<PassengerStack> GetPassengerStacksBySpecificColor(ColorEnum _color)
    {
        List<PassengerStack> stacks = new List<PassengerStack>();


        for (int i = 0; i < CurrentPassengerStacks.Count; i++)
        {
            if (CurrentPassengerStacks[i].stackColor == _color && !stacks.Contains(CurrentPassengerStacks[i]))
                stacks.Add(CurrentPassengerStacks[i]);
        }

        return stacks;
    }
    public List<PlacementPoint> GetAllAvailablePoints()
    {
        List<PlacementPoint> points = new List<PlacementPoint>();

        for (int i = 0; i < placementPoints.Count; i++)
        {
            if (!placementPoints[i].IsOccupied)
                points.Add(placementPoints[i]);
        }

        return points;
    }
    public List<ColorEnum> GetExistingColors()
    {

        return existingColorList;

    }
    PlacementPoint GetFirstAvailablePoint()
    {
        for (int i = 0; i < placementPoints.Count; i++)
        {
            PlacementPoint point = placementPoints[i];

            if (!point.IsOccupied)
                return point;
            else
            {
                if (i == placementPoints.Count - 1)
                    break;
            }
        }

        return null;
    }
    public List<PassengerStack> GetPassengerStacks()
    {
        return CurrentPassengerStacks;
    }

    #endregion

    public void Disappear(float duration = .5f)
    {
        Disappearing = true;
        CurrentPassengerStacks.Clear();
        transform.DOScale(Vector3.zero, duration);
    }
    public void GetReleased()
    {
        isPicked = false;
        transform.DOMoveY(transform.position.y - 1, 0.25f);
    }
    public void GoOtherLot(LotController targetLot, float tweenDuration = .5f)
    {
        if (CurrentLot)
        {
            CurrentLot.SetOccupied(false);
            CurrentLot.SetVehicle(null);
        }

        CurrentLot = targetLot;
        CurrentLot.SetVehicle(this);
        transform.DOMove(targetLot.GetCenter(), tweenDuration).OnComplete(() =>
        {
            transform.SetParent(CurrentLot.transform);
            GetReleased();
            CurrentLot.OnVehicleArrived();
        });
    }
    public bool IsVehicleSortedFully()
    {
        int sameColoredStackCount = 0;
        if (CurrentPassengerStacks.Count == 4)
        {
            ColorEnum firstColor = CurrentPassengerStacks[0].stackColor;
            for (int i = 0; i < CurrentPassengerStacks.Count; i++)
            {
                if (firstColor == CurrentPassengerStacks[i].stackColor)
                {
                    sameColoredStackCount++;
                }
            }

        }
        return sameColoredStackCount == 4;
    }
    public bool HasMajorityOfOneColor(out ColorEnum colorEnum, out int countOfTheStack)
    {
        Dictionary<ColorEnum, int> colorCounts = new Dictionary<ColorEnum, int>();

        if (CurrentPassengerStacks.Count == 1)
        {
            colorEnum = CurrentPassengerStacks[0].stackColor;
            countOfTheStack = 1;
            return true;
        }
        else
        {
            // Count occurrences of each color
            foreach (PassengerStack stack in CurrentPassengerStacks)
            {
                ColorEnum color = stack.stackColor;
                if (colorCounts.ContainsKey(color))
                {
                    colorCounts[color]++;
                }
                else
                {
                    colorCounts[color] = 1;
                }
            }

            // Find the color with the highest count
            int maxCount = 0;
            colorEnum = ColorEnum.NONE;

            foreach (KeyValuePair<ColorEnum, int> pair in colorCounts)
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value;
                    colorEnum = pair.Key;
                }
            }

            // Check if the majority is at least countOfTheStack
            if (maxCount >= 2)
            {
                countOfTheStack = maxCount;
                return true;
            }
            else
            {
                colorEnum = ColorEnum.NONE;
                countOfTheStack = 0;
                return false;
            }
        }
    }
    public void AddStack(PassengerStack stack)
    {
        if (CurrentPassengerStacks.Contains(stack)) return;

        CurrentPassengerStacks.Add(stack);

        if (IsVehicleSortedFully())
        {
            Disappearing = true;
            float tweenDuration = .5f;

            CurrentLot.SetOccupied(false);
            transform.DOScale(Vector3.zero, tweenDuration);
            CurrentPassengerStacks.Clear();

            CurrentLot.SetOccupied(false);
            CurrentLot.SetVehicle(null);

            // Debug.LogWarning("Vehicle is sorted fully and disappearing, BUT this is not the best practise this function should not be called here.");
        }
        else
        {

            IEnumerator Routine()
            {
                yield return new WaitForSeconds(0.05f);
                SortInnerPlacement();

            }
            StartCoroutine(Routine());
        }
    }

    public void RemoveStack(PassengerStack stack)
    {
        if (!CurrentPassengerStacks.Contains(stack)) { return; }

        CurrentPassengerStacks.Remove(stack);

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.05f);
            SortInnerPlacement();

        }

        StartCoroutine(Routine());
    }
    public void SortInnerPlacement()
    {
        Debug.Log("Sort inner placement");

        if(CurrentPassengerStacks.Count == 0) return;

        Dictionary<ColorEnum, List<PassengerStack>> colorGroupsDict = new Dictionary<ColorEnum, List<PassengerStack>>();

        foreach (PassengerStack targetStack in CurrentPassengerStacks)
        {
            if (!colorGroupsDict.ContainsKey(targetStack.stackColor))
            {
                colorGroupsDict[targetStack.stackColor] = new List<PassengerStack>();
            }
            colorGroupsDict[targetStack.stackColor].Add(targetStack);
        }

        int usedPointCount = 0;

        for (int i = 0; i < GetExistingColors().Count; i++)
        {
            ColorEnum selectedColor = GetExistingColors()[i];

            for (int cc = 0; cc < colorGroupsDict[selectedColor].Count; cc++)
            {
                PlacementPoint newPoint = placementPoints[usedPointCount];
                colorGroupsDict[selectedColor][cc].SetPlacementPoint(newPoint);
                usedPointCount++;
            }
        }
    }
    public void AddExistingStackColors(ColorEnum _color)
    {
        if (!existingColorList.Contains(_color))
            existingColorList.Add(_color);
    }
    public void RefreshExistingColorList()
    {
        existingColorList.Clear();
        for (int i = 0; i < CurrentPassengerStacks.Count; i++)
        {
            ColorEnum stackColor = CurrentPassengerStacks[i].stackColor;

            if (!existingColorList.Contains(stackColor))
                existingColorList.Add(stackColor);
        }
    }
}