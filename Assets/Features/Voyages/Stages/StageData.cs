using UnityEngine;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.Localisation;

namespace NoMoreFishAndChips.Voyages
{
    [CreateAssetMenu(fileName = "StageData", menuName = "Data/Voyages/StageData")]
    public class StageData : ScriptableObject
    {
        [SerializeField] private StageId _id;
        [SerializeField] private LocalisationTerm _nameTerm;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Wave[] _waves;

        public StageId Id => _id;
        public LocalisationTerm NameTerm => _nameTerm;
        public Sprite Sprite => _sprite;
        public Wave[] Waves => _waves;
    }
}