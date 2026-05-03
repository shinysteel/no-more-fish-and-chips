using PrimeTween;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerTileTargetVisual : MonoBehaviour
    {
        [SerializeField] private GameObject _repairGameObject;
        [SerializeField] private GameObject _tileScaffoldGameObject;
        [SerializeField] private GameObject _structureScaffoldGameObject;

        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        private Material _repairMaterial;
        private Material _tileScaffoldMaterial;
        private Material _structureScaffoldMaterial;

        private EVisual _visual;

        public enum EVisual
        {
            None,
            Repair,
            TileScaffold,
            StructureScaffold
        }

        public enum EColor
        {
            Valid,
            Invalid,
        }

        private void Awake()
        {
            _repairMaterial = GetMaterial(_repairGameObject);
            _tileScaffoldMaterial = GetMaterial(_tileScaffoldGameObject);
            _structureScaffoldMaterial = GetMaterial(_structureScaffoldGameObject);
        }

        private Material GetMaterial(GameObject obj)
        {
            Material material = null;

            foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                if (material == null)
                {
                    material = renderer.material;
                }
                else
                {
                    renderer.material = material;
                }
            }

            return material;
        }

        public void SetVisual(EVisual visual)
        {
            if (_visual == visual)
            {
                return;
            }

            _repairGameObject.SetActive(false);
            _tileScaffoldGameObject.SetActive(false);
            _structureScaffoldGameObject.SetActive(false);

            (visual switch
            {
                EVisual.Repair => _repairGameObject,
                EVisual.TileScaffold => _tileScaffoldGameObject,
                EVisual.StructureScaffold => _structureScaffoldGameObject,
                _ => null
            })?.SetActive(true);

            _visual = visual;
        }

        public void SetColor(EColor colorEnum)
        {
            Color color = colorEnum switch
            {
                EColor.Valid => _validColor,
                EColor.Invalid => _invalidColor,
                _ => Color.white
            };

            _repairMaterial.color = new Color(color.r, color.g, color.b, color.a * 0.5f);
            _tileScaffoldMaterial.color = color;
            _structureScaffoldMaterial.color = color;
        }
    }
}