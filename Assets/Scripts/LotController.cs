using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LotController : BaseCellBehavior
{

    [Header("Customized References")]
    [SerializeField] VehicleController vehiclePrefab;

    [Header("Customized Debug")]
    [SerializeField] List<LotController> neighborLots;

    public void Initiliaze(LotStatsWrapper lotStatsWrapper)
    {
        if (!lotStatsWrapper.HasVehicle) return;

        SetOccupied(true);
        VehicleController cloneVehicle = Instantiate(vehiclePrefab, GetCenter(), Quaternion.identity, transform);

        List<StackStatsWrapper> stackStats = lotStatsWrapper.stackStats;

        for (int i = 0; i < stackStats.Count; i++)
        {
            cloneVehicle.Initiliaze(stackStats[i].stackColor, i);
        }

    }

    public void AddNeighbour(LotController neighbor)
    {
        if (neighborLots.Contains(neighbor)) return;

        neighborLots.Add(neighbor);
    }


}
