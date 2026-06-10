using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    // The orientation that n models of the item will take
    [Serializable]
    public class DropOrientation
    {
        [SerializeField] private Vector3[] _positions;

        public Vector3[] Positions => _positions;
    }

    [CreateAssetMenu(fileName = "DroppedItemDefinitionData", menuName = "Data/Entities/DroppedItemDefinitionData")]
    public class DroppedItemDefinitionData : EntityDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private DropOrientation[] _modelOrientations;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public DropOrientation[] ModelOrientations => _modelOrientations;
    }
}