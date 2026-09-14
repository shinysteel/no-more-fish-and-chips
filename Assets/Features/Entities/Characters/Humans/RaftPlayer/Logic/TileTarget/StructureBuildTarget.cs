using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class StructureBuildTarget : BuildTarget
    {
        private EntityManager _entityManager;
        private PoolManager _poolManager;

        private EntityId _structureId;

        private Structure _structure;

        private GameObject _targetGameObject;

        public StructureBuildTarget(GameplayContext context, BuildTargetSettings settings, Vector3 position, EntityId structureId) : base(context, settings, position)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _structureId = structureId;

            _structure = (Structure)_entityManager.GetPrefab(_structureId);

            _targetGameObject = new GameObject(nameof(StructureBuildTarget));
            
            _targetGameObject.transform.position = _context.Raft.Queries.StructureCellToWorldPosition(_cell) + Vector3.up * 0.125f;

            _structure.StructureDefinitionData.Shape.ForEachTrue((Vector2Int cell) =>
            {
                StructureScaffoldTapes tapes = _poolManager.GetTypedPoolable<StructureScaffoldTapes>(new SpawnParams() { Position = new Vector3(cell.x, 0f, cell.y) * 0.5f, Parent = _targetGameObject.transform });
            });

            _context.Raft.OnStructureChanged += HandleStructureChanged;
        }

        public override void Dispose()
        {
            Object.Destroy(_targetGameObject);

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

        }
    }
}