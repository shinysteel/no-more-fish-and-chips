using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Interactions
{
    public class BuildScaffoldRaftTileInteraction : BuildInteraction
    {
        private EnvironmentManager _environmentManager;

        private ScaffoldRaftTile _tile;

        private Prop _previewProp;

        public BuildScaffoldRaftTileInteraction(ScaffoldRaftTile tile, GameplayContext context) : base(tile, context)
        {
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _tile = tile;
        }

        public override void ShowPreview()
        {
            _previewProp = _environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Position = new Vector3(0f, -0.125f, 0f), Scale = new Vector3(1.01f, 0.26f, 1.01f), Parent = _tile.transform });
        }

        public override void SetPreviewColor(Color color)
        {
            _previewProp.SetColor(color);
        }

        public override void HidePreview()
        {
            if (_previewProp == null)
            {
                return;
            }

            _environmentManager.ReturnProp(_previewProp);
            _previewProp = null;
        }
    }
}