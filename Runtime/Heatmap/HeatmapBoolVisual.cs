using UnityEngine;

namespace HeroLib.GridSystem
{
    /// <summary>
    /// Generates a mesh and paints it with bool data from the Grid in a heatmap fashion.
    /// </summary>
    public class HeatmapBoolVisual : MonoBehaviour
    {
        [Tooltip("Invert uv coordinates used for the visual representation.")]
        public bool invertColor = false;

        private GridMap<HeatmapGridElementBool> _grid;
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

        public void SetGrid(GridMap<HeatmapGridElementBool> grid)
        {
            _grid = grid;
            UpdateHeadmap();
            
            _grid.OnGridValueChanged += GridOnOnGridValueChanged;
        }

        public void UpdateHeadmap()
        {
            _updateMesh = true;
        }
        
        private void GridOnOnGridValueChanged(object sender, GridMap<HeatmapGridElementBool>.OnGridValueChangedEventArgs e)
        {
            UpdateHeadmap();
        }

        private void Redraw()
        {
            MeshUtils.CreateEmptyMeshArrays(_grid.Width * _grid.Height, out Vector3[] vertices, out Vector2[] uv,
                out int[] triangles);

            Vector3 quadSize = new Vector3(1, 1) * _grid.CellSize;

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    int index = x * _grid.Width + y;

                    var value = _grid.GetElement(x, y);
                    var normalizedValue = value.Normalized();
                    float gridValueNormalized = invertColor? 1.0f - normalizedValue : normalizedValue;
                    Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                    
                    MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, _grid.GetWorldPosition(x, y) + quadSize*0.5f, 0f,
                        quadSize, gridValueUV, gridValueUV);
                }
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }
    }
}