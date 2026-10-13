using ShinyOwl.Common;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterNavigateLogic : CharacterLogic
    {
        private CharacterNavigateSettings _settings;

        private PathNavigator _pathNavigator;
        private Vector2 _targetPosition;
        private Action<bool> _onTrySetPath;
        private Action _onPathStale;
        private Action _onDestinationReached;

        public CharacterNavigateLogic(Character character) : base(character)
        {
            _settings = character.CharacterDefinitionData.NavigateSettings;
        }

        public void StartNavigation(IPathFindable findable, Vector3 targetPosition, Action<bool> onTrySetPath, Action onPathStale, Action onDestinationReached)
        {
            _pathNavigator = new PathNavigator(findable, _character.CharacterPhysicsLogic.CapsuleCollider.radius);
            _targetPosition = targetPosition;
            _onTrySetPath = onTrySetPath;
            _onPathStale = onPathStale;
            _onDestinationReached = onDestinationReached;

            _context.Raft.OnTileChanged += HandleTileChanged;
            _context.Raft.OnStructureChanged += HandleStructureChanged;
        }

        public void StopNavigation()
        {
            _pathNavigator = null;
            _targetPosition = Vector3.zero;
            _onTrySetPath = null;
            _onPathStale = null;
            _onDestinationReached = null;

            _context.Raft.OnTileChanged -= HandleTileChanged;
            _context.Raft.OnStructureChanged -= HandleStructureChanged;
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            ClearPath();
        }

        private void HandleStructureChanged(Vector2Int cell, Structure previous, Structure current)
        {
            ClearPath();
        }

        private void ClearPath()
        {
            _pathNavigator.ClearPath();

            _onPathStale?.Invoke();
        }

        public override void Tick()
        {
            base.Tick();

            if (_pathNavigator == null)
            {
                return;
            }

            Vector2 position = _context.Raft.Queries.WorldPositionToStructurePosition(_character.transform.position);

            if (!_pathNavigator.HasPath())
            {
                bool set = _pathNavigator.TrySetPath(position, _targetPosition);
                _onTrySetPath?.Invoke(set);

                if (!set)
                {
                    return;
                }
            }

            _pathNavigator.Tick(position);

            if (_pathNavigator.AtDestination())
            {
                _onDestinationReached?.Invoke();
                StopNavigation();
            }
        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (_pathNavigator == null)
            {
                return;
            }

            if (_pathNavigator.HasPath())
            {
                Vector2 cellPosition = _pathNavigator.GetNextPosition();
                Vector3 worldPosition = _context.Raft.Queries.StructurePositionToWorldPosition(cellPosition);

                Vector3 direction = (worldPosition - _character.transform.position);
                direction.y = 0f;
                direction.Normalize();

                _character.CharacterPhysicsLogic.Move(direction, _settings.MoveSpeed, _settings.MoveAcceleration);
                _character.CharacterPhysicsLogic.Rotate(direction, _settings.RotateSpeed);
            }
        }
    }
}