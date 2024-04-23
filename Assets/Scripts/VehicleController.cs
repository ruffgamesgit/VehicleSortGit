using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public bool isPicked;
    LotController currentLot;

    [Header("References")]
    [SerializeField] PassengerStack stackPrefab;
    [SerializeField] List<PlacementPoint> placementPoints;

    public void Initiliaze(ColorEnum _color, int passengerStackCount)
    {
        currentLot = transform.parent.GetComponent<LotController>();

        for (int i = 0; i < passengerStackCount; i++)
        {
            PlacementPoint targetPoint = GetAvailablePoint();

            if (targetPoint == null) break; // If there is no placeable point that
                                            // indicates the vehicle is full

            Vector3 spawnPos;
            spawnPos = targetPoint.transform.position;
            PassengerStack cloneStack = Instantiate(stackPrefab, spawnPos, Quaternion.identity, targetPoint.transform);
            cloneStack.Initialize(_color);
            targetPoint.SetOccupied(true);
        }
    }

    public void GetPicked()
    {
        isPicked = true;
        transform.DOMoveY(transform.position.y + 1, 0.25f);
    }

    public void GetReleased()
    {
        isPicked = false;
        transform.DOMoveY(transform.position.y - 1, 0.25f);
    }

    public void GoOtherLot(LotController targetLot)
    {
        if (currentLot)
            currentLot.SetOccupied(false);

        currentLot = targetLot;
        transform.DOMove(targetLot.GetCenter(), .5f).OnComplete(() =>
        {
            GetReleased();
        });
    }

    public int GetAvailablePointCount()
    {
        int num = 0;
        for (int i = 0; i < placementPoints.Count; i++)
        {
            if (!placementPoints[i].IsOccupied)
                num++;
        }

        return num;
    }


    PlacementPoint GetAvailablePoint()
    {
        for (int i = 0; i < placementPoints.Count; i++)
        {
            PlacementPoint point = placementPoints[i];  

            if (!point.IsOccupied)
                return point;
            else
            {
                if(i == placementPoints.Count - 1)
                    break;
            }
        }

        return null;
    }
}
