using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroLib.GridSystem
{
    public class PathNode
    {
        // For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
        private int _gCost;

        // h(n) estimates the cost to reach goal from node n.
        private int _hCost;

        // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
        // how cheap a path could be from start to finish if it goes through n.
        private int _fCost;

        private PathNode _prevNode;

        public int X => _x;
        public int Y => _y;

        public bool Walkable
        {
            get => _isWalkable;
            set
            {
                _isWalkable = value;
                _grid.TriggerGridObjectChanged(X, Y);
            }
        }

        public int GCost
        {
            get => _gCost;
            set
            {
                _gCost = value;
                _grid.TriggerGridObjectChanged(X, Y);
            }
        }

        public int HCost
        {
            get => _hCost;
            set
            {
                _hCost = value;
                _grid.TriggerGridObjectChanged(X, Y);
            }
        }

        public int FCost
        {
            get => _fCost;
            private set
            {
                _fCost = value;
                _grid.TriggerGridObjectChanged(X, Y);
            }
        }

        public bool IsValid => HCost < int.MaxValue;

        public PathNode Previous
        {
            get => _prevNode;
            set
            {
                _prevNode = value;
                _grid.TriggerGridObjectChanged(X, Y);
            }
        }

        private GridMap<PathNode> _grid;
        private int _x;
        private int _y;

        private bool _isWalkable;

        public PathNode(GridMap<PathNode> grid, int x, int y)
        {
            this._grid = grid;
            this._x = x;
            this._y = y;
            _isWalkable = true;
        }

        public override string ToString()
        {
            if (!_isWalkable)
                return "*";
            
            if (!IsValid)
                return string.Empty;
            
            return $"{GCost} + {HCost} = {FCost}";
        }

        public void CalculateFCost()
        {
            FCost = _gCost + _hCost;
        }
    }
}