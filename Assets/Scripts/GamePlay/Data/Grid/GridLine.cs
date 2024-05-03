using System.Collections.Generic;
using GamePlay.Components;
using UnityEngine;

namespace GamePlay.Data.Grid
{
    [System.Serializable]
    public class GridLine
    {
        public List<ParkingLot> parkingLots;
        [HideInInspector]public bool isVirtual;
    }
}