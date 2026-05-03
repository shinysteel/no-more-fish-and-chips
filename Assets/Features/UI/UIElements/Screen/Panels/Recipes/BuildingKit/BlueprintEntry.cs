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

        private GameplayContext _context;
        private IBuildable _buildable;

        private List<RequirementEntry> _requirementEntries = new();

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _buildButton.onClick.AddListener(BuildPressed);
        }

        public void Setup(GameplayContext context, IBuildable buildable)
        {
            _context = context;
            _buildable = buildable;

            _image.sprite = _buildable.DefinitionData.Sprite;
            _nameText.text = _localisationManager.GetString(_buildable.DefinitionData.NameTerm);
            _descriptionText.text = _localisationManager.GetString(_buildable.DefinitionData.DescriptionTerm);

            RefreshEntries();
        }

        // Populate the recipe requirements
        private void RefreshEntries()
        {
            Utils.Collections.ResizeList(_requirementEntries, _buildable.Recipe.Requirements.Length,
                createElement: () => _poolManager.GetPoolable<RequirementEntry>(new SpawnParams() { Parent = _requirementEntriesContainer }),
                removeElement: (RequirementEntry entry) => _poolManager.ReturnPoolable(entry),
                processElement: (RequirementEntry entry, int index) => entry.Setup(_buildable.Recipe.Requirements[index]));
        }

        private void BuildPressed()
        {
            List<InventoryChangeParams> parameters = _buildable.Recipe.ToChangeParams();

            if (!_context.LocalPlayer.Inventory.CanRemoveItems(parameters, out _))
            {
                return;
            }

            if (!_buildable.TryBuild(_context, _context.LocalPlayer.TileTargetLogic.Target))
            {
                return;
            }

            _context.LocalPlayer.Inventory.TryRemoveItems(parameters);
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