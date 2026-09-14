using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class StructureBuildTarget : BuildTarget
    {
        private Structure _structure;
        private EntityId _structureId;

        public StructureBuildTarget(GameplayContext context, RaftPlayerBuildTargetSettings settings, Vector3 position, EntityId structureId) : base(context, settings, position)
        {
            _context.Raft.OnStructureChanged += HandleStructureChanged;
        }

        public override void Dispose()
        {
            if (_context.Raft != null)
            {
                _context.Raft.OnStructureChanged -= HandleStructureChanged;
            }
        }

        public override void SetPosition(Vector3 position)
        {
            if (_position == position)
            {
                return;
            }

            _position = position;

            Vector2Int cell = _context.Raft.Queries.WorldPositionToStructureCell(position);

            if (_cell == cell)
            {
                return;
            }

            _cell = cell;

            _context.Raft.Structures.TryGetValue(_cell, out Structure structure);

            HandleStructureChanged(_cell, null, structure);
        }

        protected override bool CanBuild()
        {
            return false;
        }

        private void HandleStructureChanged(Vector2Int cell, Structure previous, Structure current)
        {
            if (_cell != cell)
            {
                return;
            }

            _structure = current;

            RaiseChanged();
        }
    }
}