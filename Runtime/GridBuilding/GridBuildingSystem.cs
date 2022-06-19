using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroLib.GridSystem
{
    public class GridBuildingSystem : MonoBehaviour
    {
        public class GridObject
        {
            private readonly GridMap<GridObject> _grid;
            private readonly int _x;
            private readonly int _y;

            private PlacedObject _placedObject;

            public PlacedObject PlacedObject => _placedObject;

            public int X => _x;
            public int Y => _y;

            public GridObject(GridMap<GridObject> grid, int x, int y)
            {
                this._grid = grid;
                this._x = x;
                this._y = y;
            }

            public void SetPlacedObject(PlacedObject placedObject)
            {
                this._placedObject = placedObject;
                _grid.TriggerGridObjectChanged(X, Y);
            }

            public void ClearPlacedObject()
            {
                _placedObject = null;
                _grid.TriggerGridObjectChanged(X, Y);
            }

            public bool CanBuild()
            {
                return PlacedObject == null;
            }

            public override string ToString()
            {
                return X + "," + Y + "\n" + PlacedObject;
            }
        }

        public event EventHandler OnObjectPlaced;

        private GridMap<GridObject> _grid;
        private PlacedObjectTypeSO.Dir _rotationDirection;

        public PlacedObjectTypeSO.Dir RotationDirection
        {
            get => _rotationDirection;
            set => _rotationDirection = value;
        }

        public void Init(int gridWidth, int gridHeight, int cellSize, bool debug = false)
        {
            _grid = new GridMap<GridObject>(gridWidth, gridHeight, cellSize, Vector3.zero, (g, x, y) =>
                new GridObject(_grid, x,
                    y), debug);
        }

        public bool CanBuildAtPosition(PlacedObjectTypeSO placedObjectTypeSO, Vector3 worldPosition,
            out Vector2Int placedObjectOrigin, out List<Vector2Int> gridPositionList)
        {
            // Origin of object placement
            placedObjectOrigin = GetGridPosition(worldPosition);
            placedObjectOrigin = _grid.ValidateGridPosition(placedObjectOrigin);

            // Get list of positions in grid that object will occupy
            gridPositionList = placedObjectTypeSO.GetGridPositionList(placedObjectOrigin, _rotationDirection);
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                if (!_grid.GetElement(gridPosition.x, gridPosition.y).CanBuild())
                {
                    return false;
                }
            }

            return true;
        }

        public PlacedObject Build(PlacedObjectTypeSO placedObjectTypeSO, Vector3 worldPosition, Transform parent = null)
        {
            if (!CanBuildAtPosition(placedObjectTypeSO, worldPosition, out var placedObjectOrigin,
                    out var gridPositionList))
                return null;

            Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(_rotationDirection);
            Vector3 placedObjectWorldPosition = _grid.GetWorldPosition(placedObjectOrigin.x, placedObjectOrigin.y) +
                                                new Vector3(rotationOffset.x, 0, rotationOffset.y) * _grid.CellSize;

            PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, placedObjectOrigin,
                _rotationDirection, placedObjectTypeSO, parent);

            foreach (Vector2Int gridPosition in gridPositionList)
            {
                _grid.GetElement(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
            }

            OnObjectPlaced?.Invoke(this, EventArgs.Empty);

            return placedObject;
        }

        public bool Remove(Vector3 worldPosition)
        {
            if (_grid.GetElement(worldPosition) == null) return false;

            // Valid Grid Position
            PlacedObject placedObject = _grid.GetElement(worldPosition).PlacedObject;
            if (placedObject == null) return false;

            placedObject.DestroySelf();

            List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();
            foreach (Vector2Int gridPosition in gridPositionList)
                _grid.GetElement(gridPosition.x, gridPosition.y).ClearPlacedObject();

            return true;
        }

        public void NextRotationDirection()
        {
            RotationDirection = PlacedObjectTypeSO.GetNextDir(RotationDirection);
        }

        public void PreviousRotationDirection()
        {
            RotationDirection = PlacedObjectTypeSO.GetPreviousDir(RotationDirection);
        }
        
        public Quaternion GetPlacedObjectRotation(Vector3 worldPosition)
        {
            var element = _grid.GetElement(worldPosition);
            if (element == null) return Quaternion.identity;
            return GetPlacedObjectRotation(element.PlacedObject);
        }
        
        public Quaternion GetPlacedObjectRotation(PlacedObject placedObject)
        {
            if (placedObject == null) return Quaternion.identity;
            return Quaternion.Euler(0, PlacedObjectTypeSO.GetRotationAngle(placedObject.RotationDirection), 0);
        }

        public Vector2Int GetGridPosition(Vector3 worldPosition) {
            _grid.GetXY(worldPosition, out int x, out int z);
            return new Vector2Int(x, z);
        }

    }
}