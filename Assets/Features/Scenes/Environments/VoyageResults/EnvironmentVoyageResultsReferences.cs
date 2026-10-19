using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class EnvironmentVoyageResultsReferences : MonoBehaviour
    {
        [SerializeField] private GameObject _tileGameObject;

        public GameObject TileGameObject => _tileGameObject;
    }
}