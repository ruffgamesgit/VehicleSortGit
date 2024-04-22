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

    public void Initiliaze(ColorEnum _color, int stackPlacementIndex)
    {
        currentLot = transform.parent.GetComponent<LotController>();

        PlacementPoint targetPoint = placementPoints[stackPlacementIndex];
        Vector3 spawnPos;
        if (!targetPoint.IsOccupied)
            spawnPos = targetPoint.transform.position;
        else
        {
            Debug.LogError("Spawn placement point is mistaken");

            return;
        }

        PassengerStack cloneStack = Instantiate(stackPrefab, spawnPos, Quaternion.identity, targetPoint.transform);
        cloneStack.Initialize(_color);
        targetPoint.SetOccupied(true);
    }
}
