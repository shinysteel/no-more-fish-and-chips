using FishFlingers.Entities;
using FishFlingers.Items;
using FishFlingers.Localisation;
using FishFlingers.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class BlueprintEntry : MonoBehaviour, IPoolable
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;

        [SerializeField] private Transform _requirementEntriesContainer;
        
        private LocalisationManager _localisationManager;
        private PoolManager _poolManager;

        private RequirementEntry[] _requirementEntries;

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Setup(StructureData data)
        {
            _image.sprite = data.Sprite;
            _nameText.text = _localisationManager.GetString(data.NameTerm);
            _descriptionText.text = _localisationManager.GetString(data.DescriptionTerm);

            RecipeRequirement[] requirements = data.Recipe.Requirements;
            _requirementEntries = new RequirementEntry[requirements.Length];

            // Populate the recipe requirements
            for (int i = 0; i < _requirementEntries.Length; i++)
            {
                RequirementEntry entry = _poolManager.Get<RequirementEntry>(new SpawnParams() { Parent = _requirementEntriesContainer });
                entry.Setup(requirements[i]);
                
                _requirementEntries[i] = entry;
            }
        }

        public void OnReturnedToPool()
        { 
            foreach (RequirementEntry entry in _requirementEntries)
            {
                _poolManager.Return(entry);
            }
        }

        public void OnTakenFromPool()
        { }
    }
}