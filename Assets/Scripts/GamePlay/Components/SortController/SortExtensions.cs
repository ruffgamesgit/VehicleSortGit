using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Data.Grid;

namespace GamePlay.Components.SortController
{
    public static class SortExtensions
    {
        
        public static UniTask Swap(this Seat seat, Seat otherSeat)
        {
            var tempObject1 = seat.GetPassenger();
            var tempObject2 = otherSeat.GetPassenger();
            
            UniTaskCompletionSource mainTaskCompletionSource = new UniTaskCompletionSource();
            UniTaskCompletionSource taskCompletionSource1 = new UniTaskCompletionSource();
            UniTaskCompletionSource taskCompletionSource2 = new UniTaskCompletionSource();

            if (tempObject2 == null)
            {
                seat.SetEmpty();
                taskCompletionSource1.TrySetResult();
            }
              
            else
                seat.Occupy(tempObject2,true, taskCompletionSource1);

            if (tempObject1 == null)
            {
                otherSeat.SetEmpty();
                taskCompletionSource2.TrySetResult();
            }
            else
                otherSeat.Occupy(tempObject1, true, taskCompletionSource2);


            UniTask.WhenAll(taskCompletionSource1.Task,taskCompletionSource2.Task).ContinueWith(() =>
            {
                mainTaskCompletionSource.TrySetResult();
            });

            return mainTaskCompletionSource.Task;
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
            
            //neighbors = neighbors.ExtractEmptyParkingLots();
            return neighbors.Shuffle();
        }
        
        private static List<ParkingLot> ExtractEmptyParkingLots(this List<ParkingLot> parkingLots)
        {
            var parkingLotsExtracted = new List<ParkingLot>();
            for (int i = 0; i < parkingLots.Count; i++)
            {
                if (!parkingLots[i].IsInvisible() && !parkingLots[i].IsEmpty())
                {
                    parkingLotsExtracted.Add(parkingLots[i]);
                }
            }

            return parkingLotsExtracted;
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