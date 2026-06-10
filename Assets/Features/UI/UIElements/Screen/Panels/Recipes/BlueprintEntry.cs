using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.Inventories;
using System.Linq;
using System;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.UI
{
    public class BlueprintEntry : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Transform _requirementEntriesContainer;
        [SerializeField] private Button _buildButton;
        
        private LocalisationManager _localisationManager;
        private PoolManager _poolManager;

        private ICreatable _creatable;
        private Action _onCreatePressed;

        private List<BlueprintRequirementEntry> _requirementEntries = new();

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _buildButton.onClick.AddListener(CreatePressed);
        }

        public void Setup(ICreatable creatable, Action onCreatePressed)
        {
            _creatable = creatable;
            _onCreatePressed = onCreatePressed;

            _image.sprite = _creatable.DefinitionData.Sprite;
            _nameText.text = _localisationManager.GetString(_creatable.DefinitionData.NameTerm);
            _descriptionText.text = _localisationManager.GetString(_creatable.DefinitionData.DescriptionTerm);

            RefreshEntries();
        }

        // Populate the recipe requirements
        private void RefreshEntries()
        {
            Utils.Collections.ResizeList(_requirementEntries, _creatable.BuildRecipe.Requirements.Length,
                createElement: () => _poolManager.GetTypedPoolable<BlueprintRequirementEntry>(new SpawnParams() { Parent = _requirementEntriesContainer }),
                removeElement: (BlueprintRequirementEntry entry) => _poolManager.ReturnTypedPoolable(entry),
                processElement: (BlueprintRequirementEntry entry, int index) => entry.Setup(_creatable.BuildRecipe.Requirements[index]));
        }

        private void CreatePressed()
        {
            _onCreatePressed?.Invoke();
        }

        public void OnReturnedToPool()
        { 
            foreach (BlueprintRequirementEntry entry in _requirementEntries)
            {
                _poolManager.ReturnTypedPoolable(entry);
            }

            _requirementEntries.Clear();
        }

        public void OnTakenFromPool()
        { }
    }
}