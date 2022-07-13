using System;
using UnityEngine;

namespace HeroLib.GridSystem
{
    /// <summary>
    /// Base Grid class to store any grid based data set like a tilemap, heatmap, build matrix, game map, etc.
    /// Uses generics to make use of any Type as the grid data type.
    /// </summary>
    /// <typeparam name="T">Data Type for the grid array</typeparam>
    public class GridMap<T>
    {
        // Event triggered on value changed of any grid cell
        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int X;
            public int Y;
        }

        public enum GridMapAxis
        {
            XZ,
            XY
        };

        // DATA
        private readonly int _width; // Number of cells in the X coordinate
        private readonly int _height; // Number of cells in the Z coordinate
        private readonly float _cellSize; // Cell size in 3D space
        private readonly Vector3 _originPosition; // 3D world origin position
        private GridMapAxis _gridAxis;

        private readonly T[,] _gridArray; // Contents of the grid

        // PROPERTIES
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Vector3 OriginPosition => _originPosition;
        public GridMapAxis GridAxis => _gridAxis;

        public class GridMapDebugSettings
        {
            public bool enableTextOverlay = false;
            public int fontSize = 40;
            public float textScale = 0.1f;
        }

        public GridMap(int width, int height, float cellSize, Vector3 originPosition, GridMapAxis gridAxis,
            Func<GridMap<T>, int, int, T> createGridElement,
            GridMapDebugSettings debugSettings)
        {
            this._width = width;
            this._height = height;
            this._cellSize = cellSize;
            this._originPosition = originPosition;
            this._gridAxis = gridAxis;

            _gridArray = new T[width, height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = createGridElement(this, x, y);
                }
            }

            if (debugSettings.enableTextOverlay)
            {
                var debugTextArray = new TextMesh[width, height];
                for (int x = 0; x < _gridArray.GetLength(0); x++)
                {
                    for (int y = 0; y < _gridArray.GetLength(1); y++)
                    {
                        //Debug
                        var debugTextPosition = (GridAxis == GridMapAxis.XZ)
                            ? GetWorldPosition(x, y) + new Vector3(cellSize, 0, cellSize) * .5f
                            : GetWorldPosition(x, y) + new Vector3(cellSize, cellSize, 0) * .5f;
                        debugTextArray[x, y] = UIExt.CreateWorldText(_gridArray[x, y]?.ToString(), null,
                            debugTextPosition, debugSettings.fontSize,
                            Color.white, TextAnchor.MiddleCenter);
                        debugTextArray[x, y].transform.localRotation = GridAxis == GridMapAxis.XZ
                            ? Quaternion.Euler(90, 0, 0)
                            : Quaternion.identity;
                        debugTextArray[x, y].transform.localScale = Vector3.one * debugSettings.textScale * CellSize;
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, float.MaxValue);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, float.MaxValue);
                    }

                    Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white,
                        float.MaxValue);
                    Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white,
                        float.MaxValue);
                }

                OnGridValueChanged += (sender, args) =>
                {
                    debugTextArray[args.X, args.Y].text = _gridArray[args.X, args.Y]?.ToString();
                };
            }
        }


        public Vector3 GetWorldPosition(int x, int y)
        {
            return GridAxis == GridMapAxis.XZ
                ? new Vector3(x, 0, y) * _cellSize + _originPosition
                : new Vector3(x, y, 0) * _cellSize + _originPosition;
        }

        public Vector3 GetWorldPositionXYSpace(int x, int y)
        {
            return new Vector3(x, y, 0) * _cellSize + _originPosition;
        }

        public void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            y = GridAxis == GridMapAxis.XZ
                ? Mathf.FloorToInt((worldPosition - _originPosition).z / _cellSize)
                : Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
        }

        public void SetElement(int x, int y, T value)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                _gridArray[x, y] = value;
                TriggerGridObjectChanged(x, y);
            }
        }

        public void SetElement(Vector3 worldPosition, T value)
        {
            int x = 0;
            int y = 0;
            GetXY(worldPosition, out x, out y);
            SetElement(x, y, value);
        }

        public T GetElement(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
                return _gridArray[x, y];
            else return default;
        }

        public T GetElement(Vector3 worldPosition)
        {
            int x = 0;
            int y = 0;
            GetXY(worldPosition, out x, out y);
            return GetElement(x, y);
        }

        public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
        {
            return new Vector2Int(
                Mathf.Clamp(gridPosition.x, 0, Width - 1),
                Mathf.Clamp(gridPosition.y, 0, Height - 1)
            );
        }

        public void TriggerGridObjectChanged(int x, int y)
        {
            if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { X = x, Y = y });
        }
    }
}