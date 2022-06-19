using System.Collections.Generic;
using UnityEngine;

namespace HeroLib.GridSystem
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private GridMap<PathNode> _gridMap;
        private List<PathNode> _openList;
        private List<PathNode> _closedList;

        public Pathfinding(int width, int height, float cellSize, Vector3 gridStartPosition, bool showDebug = false)
        {
            _gridMap = new GridMap<PathNode>(width, height, cellSize, gridStartPosition,
                (grid, x, y) => new PathNode(grid, x, y), showDebug);
            
            InitValues();
        }


        private void InitValues()
        {
            // Init Grid with default values
            for (int x = 0; x < _gridMap.Width; x++)
            {
                for (int y = 0; y < _gridMap.Height; y++)
                {
                    var pathNode = GetNode(x, y);
                    pathNode.GCost = int.MaxValue;
                    pathNode.HCost = int.MaxValue;
                    pathNode.CalculateFCost();
                    pathNode.Previous = null;
                }
            }
        }
        
        public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
        {
            var startNode = GetNode(startX, startY);
            var endNode = GetNode(endX, endY);

            if (startNode == null || endNode == null) 
                return null;

            _openList = new List<PathNode> { startNode };
            _closedList = new List<PathNode>();

            InitValues();

            // Update start node
            startNode.GCost = 0;
            startNode.HCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            // While there's paths to check...
            while (_openList.Count > 0)
            {
                // Get the best candidate to check first. Initially it will be the start node.
                var currentNode = GetLowestFCostNode(_openList);
                if (currentNode == endNode)
                {
                    // Reached final node, return list of nodes to travel this path
                    return CalculatePath(endNode);
                }

                // Checked nodes go from the open to the closed list
                _openList.Remove(currentNode);
                _closedList.Add(currentNode);

                // For each neighbour
                foreach (var neighbourNode in GetNeighbourList(currentNode))
                {
                    // Already checked? ignore
                    if (_closedList.Contains(neighbourNode)) continue;
                    
                    // Is neighbor walkable?
                    if (neighbourNode.Walkable == false)
                    {
                        _closedList.Add(neighbourNode);
                        continue;
                    }

                    // Calculate cost to reach. Initially it is float Max Value.
                    int tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighbourNode);
                    // Update with new lower cost, set as path candidate
                    if (tentativeGCost < neighbourNode.GCost)
                    {
                        neighbourNode.Previous = currentNode;
                        neighbourNode.GCost = tentativeGCost;
                        neighbourNode.HCost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.CalculateFCost();

                        // Add as candidate to check
                        if (!_openList.Contains(neighbourNode)) _openList.Add(neighbourNode);
                    }
                }
            }

            // Out of nodes on the open list
            return null;
        }

        public GridMap<PathNode> GetGrid()
        {
            return _gridMap;
        }

        public PathNode GetNode(int x, int y)
        {
            return _gridMap.GetElement(x, y);
        }

        private List<PathNode> GetNeighbourList(PathNode currentNode)
        {
            List<PathNode> neighbourList = new List<PathNode>();

            if (currentNode.X - 1 >= 0)
            {
                // Left
                neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y));

                // Left Down
                if (currentNode.Y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y - 1));

                // Left Up
                if (currentNode.Y + 1 < _gridMap.Height)
                    neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y + 1));
            }

            if (currentNode.X + 1 < _gridMap.Width)
            {
                // Right
                neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y));

                // Right Down
                if (currentNode.Y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y - 1));

                // Right Up
                if (currentNode.Y + 1 < _gridMap.Height)
                    neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y + 1));
            }

            // Down
            if (currentNode.Y - 1 > 0)
                neighbourList.Add(GetNode(currentNode.X, currentNode.Y - 1));
            // Up
            if (currentNode.Y + 1 < _gridMap.Height)
                neighbourList.Add(GetNode(currentNode.X, currentNode.Y + 1));

            return neighbourList;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();

            path.Add(endNode);
            PathNode currentNode = endNode;

            while (currentNode.Previous != null)
            {
                path.Add(currentNode.Previous);
                currentNode = currentNode.Previous;
            }

            path.Reverse();
            return path;
        }

        public int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.X - b.X);
            int yDistance = Mathf.Abs(a.Y - b.Y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].FCost < lowestFCostNode.FCost)
                {
                    lowestFCostNode = pathNodeList[i];
                }
            }

            return lowestFCostNode;
        }
    }
}