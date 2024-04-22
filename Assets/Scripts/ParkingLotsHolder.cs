using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingLotsHolder : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] LotController lotPrefab;
    [SerializeField] float horizontalGap;

    [Header("References")]
    [SerializeField] int desiredLotAmount;


    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }


        for (int i = 0; i < desiredLotAmount; i++)
        {
            float xPos = i * horizontalGap;
            Vector3 spawnPos = new Vector3(xPos, 1, transform.position.z);
            LotController cloneLot = Instantiate(lotPrefab, spawnPos, Quaternion.identity, transform);
        }
    }

}
