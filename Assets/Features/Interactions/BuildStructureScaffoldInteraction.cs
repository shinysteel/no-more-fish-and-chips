using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Interactions
{
    public class BuildStructureScaffoldInteraction : BuildInteraction
    {
        private EnvironmentManager _environmentManager;

        private StructureScaffold _scaffold;

        private List<Prop> _previewProps = new();

        public BuildStructureScaffoldInteraction(StructureScaffold scaffold, GameplayContext context) : base(scaffold, context)
        {
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _scaffold = scaffold;
        }

        public override void ShowPreview()
        {
            _scaffold.Shape.ForEachTrue((Vector2Int cell) =>
            {
                Vector3 position = new Vector3(cell.x, 0f, cell.y) * 0.5f + Vector3.up * 0.075f;
                Vector3 scale = new Vector3(0.51f, 0.15f, 0.51f);
                _previewProps.Add(_environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Position = position, Scale = scale, Parent = _scaffold.transform }));
            });
        }

        public override void SetPreviewColor(Color color)
        {
            foreach (Prop prop in _previewProps)
            {
                prop.SetColor(color);
            }
        }

        public override void HidePreview()
        {
            foreach (Prop prop in _previewProps)
            {
                _environmentManager.ReturnProp(prop);
            }
        }
    }
}