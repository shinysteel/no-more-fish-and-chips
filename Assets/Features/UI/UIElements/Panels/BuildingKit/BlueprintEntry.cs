using FishFlingers.Entities;
using FishFlingers.Localisation;
using FishFlingers.Pools;
using TMPro;
using UnityEngine;

namespace FishFlingers.UI
{
    public class BlueprintEntry : MonoBehaviour, IPoolable
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        
        private LocalisationManager _localisationManager;

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
        }

        public void Setup(StructureData data)
        {
            _nameText.text = _localisationManager.GetString(data.NameTerm);
            _descriptionText.text = _localisationManager.GetString(data.DescriptionTerm);
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}