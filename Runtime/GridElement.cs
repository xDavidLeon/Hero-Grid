using UnityEngine;

namespace HeroLib.GridSystem
{
    public class StringGridElement
    {
        private readonly int _x;
        private readonly int _y;
        private readonly GridMap<StringGridElement> _grid;

        private string _letters;
        private string _numbers;

        public StringGridElement(GridMap<StringGridElement> grid, int x, int y)
        {
            this._grid = grid;
            this._x = x;
            this._y = y;
        }
        
        public void AddLetter(string letter)
        {
            _letters += letter;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public void AddNumber(string number)
        {
            _numbers += number;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public override string ToString()
        {
            return _letters + "\n" + _numbers;
        }
    }

    public class HeatmapGridElement
    {
        private const int Min = 0;
        private const int Max = 100;

        private readonly int _x;
        private readonly int _y;
        private readonly GridMap<HeatmapGridElement> _grid;

        private int value;

        public HeatmapGridElement(GridMap<HeatmapGridElement> grid, int x, int y)
        {
            this._grid = grid;
            this._x = x;
            this._y = y;
        }

        public void SetValue(int newValue)
        {
            value = newValue;
            value = Mathf.Clamp(value, Min, Max);
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public void AddValue(int addValue)
        {
            value += addValue;
            value = Mathf.Clamp(value, Min, Max);
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        /// <summary>
        /// Add value in a diamond pattern
        /// </summary>
        /// <param name="addValue">Added value at center center of effect</param>
        /// <param name="fullValueRange">Number of cells that will be given the full value</param>
        /// <param name="totalRange">Total range of effect</param>
        public void AddValue(int addValue, int fullValueRange, int totalRange)
        {
            int lowerValueAmount = Mathf.RoundToInt((float)addValue / (totalRange - fullValueRange));

            for (int x = 0; x < totalRange; x++)
            {
                for (int y = 0; y < totalRange - x; y++)
                {
                    int radius = x + y;
                    int addValueAmount = addValue;
                    if (radius > fullValueRange)
                    {
                        addValueAmount -= lowerValueAmount * (radius - fullValueRange);
                    }

                    _grid.GetElement(this._x + x, this._y + y)?.AddValue(addValueAmount);

                    if (x != 0)
                        _grid.GetElement(this._x - x, this._y + y)?.AddValue(addValueAmount);

                    if (y != 0)
                    {
                        _grid.GetElement(this._x + x, this._y - y)?.AddValue(addValueAmount);

                        if (x != 0)
                            _grid.GetElement(this._x - x, this._y - y)?.AddValue(addValueAmount);
                    }
                }
            }
        }

        public float Normalized()
        {
            return (float)value / Max;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }


    public class HeatmapGridElementBool
    {
        private readonly int _x;
        private readonly int _y;
        private readonly GridMap<HeatmapGridElementBool> _grid;
        
        private bool _value;

        public HeatmapGridElementBool(GridMap<HeatmapGridElementBool> grid, int x, int y)
        {
            this._grid = grid;
            this._x = x;
            this._y = y;
        }

        public void SetValue(bool newValue)
        {
            _value = newValue;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        public void AddValue(bool addValue)
        {
            _value = addValue;
            _grid.TriggerGridObjectChanged(_x, _y);
        }

        /// <summary>
        /// Add value in a diamond pattern
        /// </summary>
        /// <param name="addValue">Added value at center center of effect</param>
        /// <param name="fullValueRange">Number of cells that will be given the full value</param>
        /// <param name="totalRange">Total range of effect</param>
        public void AddValue(bool addValue, int fullValueRange, int totalRange)
        {
            for (int x = 0; x < totalRange; x++)
            {
                for (int y = 0; y < totalRange - x; y++)
                {
                    int radius = x + y;
                    bool addValueAmount = addValue;
                    if (radius > fullValueRange)
                    {
                        addValueAmount = false;
                    }

                    _grid.GetElement(this._x + x, this._y + y)?.AddValue(addValueAmount);

                    if (x != 0)
                        _grid.GetElement(this._x - x, this._y + y)?.AddValue(addValueAmount);

                    if (y != 0)
                    {
                        _grid.GetElement(this._x + x, this._y - y)?.AddValue(addValueAmount);

                        if (x != 0)
                            _grid.GetElement(this._x - x, this._y - y)?.AddValue(addValueAmount);
                    }
                }
            }
        }

        public float Normalized()
        {
            return _value ? 1.0f : 0.0f;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}