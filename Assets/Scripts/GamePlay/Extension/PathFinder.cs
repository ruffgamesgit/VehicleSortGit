using System;
using System.Collections.Generic;
using GamePlay.Components;
using GamePlay.Components.SortController;
using GamePlay.Data;
using GamePlay.Data.Grid;

namespace GamePlay.Extension
{
    public static class PathFinder
    {
        
        public static List<ParkingLot> FindPath(this GridData gridData,ParkingLot from, ParkingLot to)
        {
            int GetGridLineOffset(ParkingLotPosition position)
            {
                int offset = 0;
                for (int i = 0; i < position.GetGridGroupIndex(); i++)
                {
                    offset += gridData.gridGroups[i].lines.Count + 1;
                }

                offset += 1;
                return offset;
            }
            List<GridLine> virtualizedLines = gridData.gridGroups.GenerateVirtualGrid();
            var fromPosition = from.GetParkingLotPosition();
            var toPosition = to.GetParkingLotPosition();
            var startGridLineIndex = fromPosition.GetGridLineIndex() + GetGridLineOffset(fromPosition);
            var targetGridLineIndex = toPosition.GetGridLineIndex() + GetGridLineOffset(toPosition);
            var path =  virtualizedLines.CalculatePath(startGridLineIndex, fromPosition.GetParkingLotIndex(), 
                targetGridLineIndex, toPosition.GetParkingLotIndex());

            if (path is { Count: >= 3 })
            {
                var indexOfLastNullElement = path.FindLastIndex(x => x == null);

                if (indexOfLastNullElement != -1)
                {
                    var lastElementLine = path[^1].GetParkingLotPosition().GetGridLineIndex();
                    var elementsAfterNull = path.GetRange(indexOfLastNullElement + 1, path.Count - 1 - indexOfLastNullElement);

                    foreach (var element in elementsAfterNull)
                    {
                        if (element.GetParkingLotPosition().GetGridLineIndex() != lastElementLine)
                        {
                            return path;
                        }
                    }
                    
                    for(int i = path.Count -2 ; i > indexOfLastNullElement; i--)
                    {
                        if (path[i].GetParkingLotPosition().GetGridLineIndex() == lastElementLine)
                        {
                            path[i] = null;
                        }
                    }

                }
                   
            }

            return path;
        }
        private static List<ParkingLot> CalculatePath(this List<GridLine> gridLines, int startX, int startY, int targetX, int targetY)
        {
            var grid = GenerateArray(gridLines);
            var nodes = ConvertGridToNode(grid);
            var calculatedPath = CalculatePath(nodes, nodes[startX, startY], nodes[targetX, targetY]);
            if (calculatedPath == null)
            {
                return null;
            }

            List<ParkingLot> result = new List<ParkingLot>();
            foreach (var node in calculatedPath)
            {
                result.Add(grid[node.X,node.Y]);
            }
            return result;
        }

        private static ParkingLot[,] GenerateArray(List<GridLine> lines)
        {
            var dimension1 = lines.Count;
            var dimension2 = lines[1].parkingLots.Count;
            ParkingLot[,] array = new ParkingLot[dimension1, dimension2];

            int x = 0;
            foreach (var line in lines)
            {
                int y = 0;
                foreach (var parkingLot in line.parkingLots)
                {
                    array[x, y] = parkingLot;
                    y++;
                }

                x++;
            }

            return array;
        }
        
        private static Node[,] ConvertGridToNode(ParkingLot[,] grid)
        {
            Node[,] nodes = new Node[grid.GetLength(0), grid.GetLength(1)];
            for (int i = 0; i < nodes.GetLength(0); i++)
            {
                for (int j = 0; j < nodes.GetLength(1); j++)
                {
                    bool isGridNull = grid[i, j] == null;
                    bool isGridWalkable = isGridNull || grid[i, j].IsWalkable();
                    nodes[i, j] = new Node(i, j,  isGridWalkable);
                }
            }
            return nodes;
        }

        private static List<Node> CalculatePath(Node[,] nodes, Node start, Node goal)
        {
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Node current = GetLowestFScoreNode(openSet);
                if (current == goal)
                {
                    return ReconstructPath(current);
                }
                openSet.Remove(current);
                closedSet.Add(current);

                foreach (Node neighbor in GetNeighbors(nodes, current))
                {
                    if (closedSet.Contains(neighbor) || !IsNodeWalkable(neighbor))
                        continue;

                    int tentativeGScore = current.G + 1;

                    if (!openSet.Contains(neighbor) || tentativeGScore < neighbor.G)
                    {
                        neighbor.Parent = current;
                        neighbor.G = tentativeGScore;
                        neighbor.H = CalculateHeuristic(neighbor, goal);
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
            return null;
        }

        private static bool IsNodeWalkable(Node node)
        {
            return node.IsWalkable;
        }

        private static IEnumerable<Node> GetNeighbors(Node[,] grid, Node node)
        {
            int maxX = grid.GetLength(0);
            int maxY = grid.GetLength(1);

            int x = node.X;
            int y = node.Y;

            if (x > 0) yield return grid[x - 1, y];
            if (x < maxX - 1) yield return grid[x + 1, y];
            if (y > 0) yield return grid[x, y - 1];
            if (y < maxY - 1) yield return grid[x, y + 1];
        }

        private static Node GetLowestFScoreNode(List<Node> nodes)
        {
            Node lowest = nodes[0];

            foreach (Node node in nodes)
            {
                if (node.F < lowest.F)
                    lowest = node;
            }

            return lowest;
        }

        private static int CalculateHeuristic(Node a, Node b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static List<Node> ReconstructPath(Node node)
        {
            List<Node> path = new List<Node>();
            while (node != null)
            {
                path.Insert(0, node);
                node = node.Parent;
            }
            return path;
        }
    }

    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsWalkable { get; set; }
        public List<Node> Neighbors { get; set; }
        public Node Parent { get; set; }
        public int G { get; set; }
        public int H { get; set; } 
        public int F => G + H;

        public Node(int x, int y, bool isWalkable)
        {
            X = x;
            Y = y;
            IsWalkable = isWalkable;
            Neighbors = new List<Node>();
        }
    }
}