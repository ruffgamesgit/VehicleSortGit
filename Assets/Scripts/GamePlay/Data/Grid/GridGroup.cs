using System.Collections.Generic;

namespace GamePlay.Data.Grid
{
    [System.Serializable]
    public class GridGroup
    {
         public List<GridLine> lines;
         public bool hasLowerRoad = true;
         public bool hasUpperRoad = false;
    }
}