using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Data.Grid;

namespace GamePlay.Components.SortController
{
    public static class SortExtensions
    {

        public static UniTask AnimateSeatChanges(this List<Seat> seats) 
        {
            UniTaskCompletionSource mainTaskCompletionSource = new UniTaskCompletionSource();
            List<UniTask> ucsList = new List<UniTask>();
            for (int i = 0; i < seats.Count; i++)
            {
                var ucs = new UniTaskCompletionSource();
                ucsList.Add(ucs.Task);
                seats[i].TakePassengerWithAnimation(ucs);
            }

            UniTask.WhenAll(ucsList.ToArray()).ContinueWith(() =>
            {
                mainTaskCompletionSource.TrySetResult();
            });

            return mainTaskCompletionSource.Task;
        }
        
        public static void Swap(this Seat seat, Seat otherSeat)
        {
            var tempObject1 = seat.GetPassenger();
            var tempObject2 = otherSeat.GetPassenger();
            
            if (tempObject2 == null)
            {
                seat.SetEmpty();
            }
            else
                seat.Occupy(tempObject2);

            if (tempObject1 == null)
            {
                otherSeat.SetEmpty();
            }
            else
                otherSeat.Occupy(tempObject1);
        }
        
        public static List<ParkingLot> FindNeighbors(this ParkingLot parkingLot, List<GridLine> gridLines)
        {
            List<ParkingLot> neighbors = new List<ParkingLot>();
            var parkingLotPosition = parkingLot.GetParkingLotPosition();
            var x = parkingLotPosition.GetParkingLotIndex();
            var y = parkingLotPosition.GetGridLineIndex();

            if (y >= 0 && y < gridLines.Count)
            {
                var line = gridLines[y];

                if (x >= 0 && x < line.parkingLots.Count)
                {
                    // Right neighbor
                    if (x + 1 < line.parkingLots.Count)
                    {
                        if (line.parkingLots[x + 1].gameObject.activeSelf)
                            neighbors.Add(line.parkingLots[x + 1]);
                    }


                    // Left neighbor
                    if (x - 1 >= 0)
                    {
                        if (line.parkingLots[x - 1].gameObject.activeSelf)
                            neighbors.Add(line.parkingLots[x - 1]);
                    }


                    // Up neighbor
                    if (y + 1 < gridLines.Count)
                    {
                            var upLine = gridLines[y + 1];
                            if (x < upLine.parkingLots.Count)
                                neighbors.Add(upLine.parkingLots[x]);
                    }

                    // Down neighbor
                    if (y - 1 >= 0)
                    {
                            var downLine = gridLines[y - 1];
                            if (x < downLine.parkingLots.Count)
                                neighbors.Add(downLine.parkingLots[x]);
                    }
                }
            }
            
            return neighbors.Shuffle();
        }
        
        public static List<ParkingLot> ExtractUnSortableParkingLots(this List<ParkingLot> parkingLots)
        {
            parkingLots.RemoveAll(lot => lot.IsInvisible() || lot.IsEmpty());
            return parkingLots;
        }
        
        
        public static List<GridLine> GenerateVirtualGrid(this List<GridGroup> gridGroups)
        {
            List<GridLine> virtualizedLines = new List<GridLine>();
            foreach (var group in gridGroups)
            {
                virtualizedLines.Add(GenerateVirtualLine(group.lines[0].parkingLots.Count));
                foreach (var line in group.lines)
                {
                    virtualizedLines.Add(line);
                }
            }
            GridLine GenerateVirtualLine(int parkingLotCount)
            {
                var virtualLine = new GridLine
                {
                    isVirtual = true,
                    parkingLots = new List<ParkingLot>(parkingLotCount)
                };
                return virtualLine;
            }

            return virtualizedLines;

        }
    }
}