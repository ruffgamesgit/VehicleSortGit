using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCellBehavior : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] protected float verticalCenterOffset;

    [Header("Debug")]
    public bool IsOccupied;

    public Vector3 GetCenter()
    {
        Vector3 centerPos = new Vector3(transform.position.x, transform.position.y + verticalCenterOffset, transform.position.z);
        return centerPos;
    }

    public void SetOccupied(bool state)
    {
        IsOccupied = state;
    }

}
