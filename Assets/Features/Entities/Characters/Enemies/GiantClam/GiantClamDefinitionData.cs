using NoMoreFishAndChips.Hitboxes;
using ShinyOwl.Common.Structures;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "GiantClamDefinitionData", menuName = "Data/Entities/Characters/GiantClamDefinitionData")]
    public class GiantClamDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private BoolGrid _inventoryLayout;
        [SerializeField] private GiantClamAwaitItemsSettings _awaitItemsSettings;
        [SerializeField] private GiantClamEmptyRageSettings _emptyRageSettings;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public BoolGrid InventoryLayout => _inventoryLayout;
        public GiantClamAwaitItemsSettings AwaitItemsSettings => _awaitItemsSettings;
        public GiantClamEmptyRageSettings EmptyRageSettings => _emptyRageSettings;
    }

    [Serializable]
    public class GiantClamAwaitItemsSettings
    {
        [SerializeField] private float _duration = 20f;

        public float Duration => _duration;
    }

    [Serializable]
    public class GiantClamEmptyRageSettings
    {
        [SerializeField] private HitboxData _hitboxData;

        public HitboxData HitboxData => _hitboxData;
    }
}