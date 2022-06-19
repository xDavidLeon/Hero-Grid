using System.Collections.Generic;
using UnityEngine;

namespace HeroLib.GridSystem
{
    public class PlacedObject : MonoBehaviour
    {
        private PlacedObjectTypeSO _placedObjectTypeSO;
        private Vector2Int _origin;
        private PlacedObjectTypeSO.Dir _dir;

        public PlacedObjectTypeSO PlacedObjectTypeSo => _placedObjectTypeSO;
        public Vector2Int Origin => _origin;
        public PlacedObjectTypeSO.Dir RotationDirection => _dir;
        
        public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, PlacedObjectTypeSO.Dir dir,
            PlacedObjectTypeSO placedObjectTypeSO, Transform parent = null)
        {
            Transform placedObjectTransform = Instantiate(placedObjectTypeSO.prefab, worldPosition,
                Quaternion.Euler(0, PlacedObjectTypeSO.GetRotationAngle(dir), 0), parent);

            PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();
            placedObject.Setup(placedObjectTypeSO, origin, dir);

            return placedObject;
        }

        private void Setup(PlacedObjectTypeSO placedObjectTypeSO, Vector2Int origin, PlacedObjectTypeSO.Dir dir)
        {
            this._placedObjectTypeSO = placedObjectTypeSO;
            this._origin = origin;
            this._dir = dir;
        }

        public List<Vector2Int> GetGridPositionList()
        {
            return _placedObjectTypeSO.GetGridPositionList(_origin, _dir);
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        public override string ToString()
        {
            return _placedObjectTypeSO.nameString;
        }
    }
}