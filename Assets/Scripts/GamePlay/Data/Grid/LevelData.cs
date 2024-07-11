using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace GamePlay.Data.Grid
{
    [System.Serializable]
    public class LevelData
    {
        public string scene;
        public int colorVariety;
        public int matchingPassengerCount;
        public int vehicleCount;
        public int moveCount;
        public List<int> garageVehicleCounts;
        public List<LevelDataGridGroup> levelDataGridGroups;
    }
    
    [System.Serializable]
    public class LevelDataGridGroup
    {
        public List<LevelDataGridLine> lines;
    }
    
    [System.Serializable]
    public class LevelDataGridLine
    {
        public List<LevelDataParkingLot> ParkingLots;
 
    }
    
    [System.Serializable]
    public class LevelDataParkingLot
    {
        public bool IsEmpty;
        public bool IsInvisible;
        public bool IsObstacle;
    }
}