using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class TreeModel : EntityModel
    {
        [SerializeField] private GameObject[] _stageGameObjects;

        public GameObject[] StageGameObjects => _stageGameObjects;
    }
}