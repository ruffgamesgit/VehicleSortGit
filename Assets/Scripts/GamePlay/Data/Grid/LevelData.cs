using System.Collections.Generic;

namespace GamePlay.Data.Grid
{
    [System.Serializable]
    public class LevelData
    {
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
        public List<bool> parkingLotStatus;
    }
}