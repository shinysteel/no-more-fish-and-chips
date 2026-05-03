using FishFlingers.Entities;
using FishFlingers.Items;
using FishFlingers.Localisation;
using FishFlingers.Pools;
using FishFlingers.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ShinyOwl.Common.Utils;
using FishFlingers.Inventories;
using System.Linq;
using System;
using ShinyOwl.Common;

namespace FishFlingers.UI
{
    public class BlueprintEntry : MonoBehaviour, IPoolable
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

        private List<RequirementEntry> _requirementEntries = new();

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _buildButton.onClick.AddListener(CreatePressed);
        }

        public void Setup(ICreatable creatable, Action onCreatePressed)
        {
            Log.Info($"setting up {creatable.DefinitionData.name}");
            
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
            Utils.Collections.ResizeList(_requirementEntries, _creatable.Recipe.Requirements.Length,
                createElement: () => _poolManager.GetPoolable<RequirementEntry>(new SpawnParams() { Parent = _requirementEntriesContainer }),
                removeElement: (RequirementEntry entry) => _poolManager.ReturnPoolable(entry),
                processElement: (RequirementEntry entry, int index) => entry.Setup(_creatable.Recipe.Requirements[index]));
        }

        private void CreatePressed()
        {
            _onCreatePressed?.Invoke();
        }

        public void OnReturnedToPool()
        { 
            foreach (RequirementEntry entry in _requirementEntries)
            {
                _poolManager.ReturnPoolable(entry);
            }

            _requirementEntries.Clear();
        }

        public void OnTakenFromPool()
        { }
    }
}