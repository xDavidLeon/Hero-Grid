using UnityEngine;

namespace HeroLib.GridSystem
{
    /// <summary>
    /// Generates a mesh and paints it with data from the Grid in a heatmap fashion.
    /// </summary>
    public class HeatmapPathfindingVisual : MonoBehaviour
    {
        [Tooltip("Invert uv coordinates used for the visual representation.")]
        public bool invertColor = false;

        private GridMap<PathNode> _grid;
        private Mesh _mesh;
        private bool _updateMesh;

        private void Awake()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void LateUpdate()
        {
            if (_updateMesh)
            {
                _updateMesh = false;
                Redraw();
            }
        }

        public void SetGrid(GridMap<PathNode> grid)
        {
            _grid = grid;
            UpdateHeadmap();

            _grid.OnGridValueChanged += GridOnOnGridValueChanged;
        }

        public void UpdateHeadmap()
        {
            _updateMesh = true;
        }

        private void GridOnOnGridValueChanged(object sender, GridMap<PathNode>.OnGridValueChangedEventArgs e)
        {
            UpdateHeadmap();
        }

        private void Redraw()
        {
            MeshUtils.CreateEmptyMeshArrays(_grid.Width * _grid.Height, out Vector3[] vertices, out Vector2[] uv,
                out int[] triangles);

            float maxCost = 0;

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    var elem = _grid.GetElement(x, y);
                    if (elem.IsValid == false) continue;
                    
                    if (elem.HCost > maxCost ) maxCost = elem.HCost;
                }
            }
            
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    int index = x * _grid.Width + y;
                    Vector3 quadSize = new Vector3(1, 1) * _grid.CellSize;

                    var value = _grid.GetElement(x, y);

                    if (value.IsValid == false)
                    {
                        quadSize = Vector3.zero;
                    }

                    float valueFloat = value.HCost / maxCost;
                    float gridValueNormalized = invertColor ? 1.0f - valueFloat : valueFloat;
                    Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                    
                    MeshUtils.AddToMeshArrays(vertices, uv, triangles, index,
                        _grid.GetWorldPosition(x, y) + quadSize * 0.5f, 0f,
                        quadSize, gridValueUV, gridValueUV);
                }
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }
    }
}