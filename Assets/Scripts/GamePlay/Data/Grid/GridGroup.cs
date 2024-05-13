using System.Collections.Generic;

namespace GamePlay.Data.Grid
{
    [System.Serializable]
    public class GridGroup
    {
         public List<GridLine> lines;
         public bool hasUpperRoad = false;
         public bool hasLowerRoad = true;
    }
}