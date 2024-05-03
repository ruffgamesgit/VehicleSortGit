using System.Collections.Generic;
using GamePlay.Components;
using GamePlay.Components.SortController;
using GamePlay.Extension;

namespace GamePlay.Data.Grid
{
    [System.Serializable]
    public class GridData
    {
        public List<GridGroup> gridGroups;
        
        public List<ParkingLot> FindPath(ParkingLot from, ParkingLot to)
        {
            List<GridLine> virtualizedLines = gridGroups.GenerateVirtualGrid();
            var fromPosition = from.GetParkingLotPosition();
            var toPosition = to.GetParkingLotPosition();
            
            return virtualizedLines.FindPath(fromPosition.GetGridLineIndex(), fromPosition.GetParkingLotIndex(), 
                toPosition.GetGridLineIndex(), toPosition.GetParkingLotIndex());
        }
        
    }
}