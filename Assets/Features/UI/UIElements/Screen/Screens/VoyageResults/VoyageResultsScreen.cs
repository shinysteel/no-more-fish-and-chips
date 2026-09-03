using UnityEngine;
using UnityEngine.UI;
using System;
using NoMoreFishAndChips.States;
using TMPro;
using NoMoreFishAndChips.Localisation;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Voyages;

namespace NoMoreFishAndChips.UI
{
    public class VoyageResultsScreen : ScreenUI
    {
        [SerializeField] private TextMeshProUGUI _outcomeText;
        [SerializeField] private Transform _stagesContainer;
        [SerializeField] private Button _returnToLobbyButton;

        private LocalisationManager _localisationManager;
        private PoolManager _poolManager;
        
        private Action _handleReturnToLobbyPressed;

        private List<StageNode> _stageNodes = new();

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _returnToLobbyButton.onClick.AddListener(HandleReturnToLobbyPressed);
        }

        private void OnDestroy()
        {
            foreach (StageNode node in _stageNodes)
            {
                _poolManager.ReturnTypedPoolable(node);
            }
        }

        public void Setup(GameplayContext context, Action handleReturnToLobbyPressed)
        {
            _handleReturnToLobbyPressed = handleReturnToLobbyPressed;

            _outcomeText.text = _localisationManager.GetString(context.VoyageRunner.VoyageResult == VoyageResult.Victory ? LocalisationTerm.VoyageResultsScreenVictory : LocalisationTerm.VoyageResultsScreenDefeat);

            for (int i = 0; i < context.VoyageRunner.StageIds.Count; i++)
            {
                StageNode node = _poolManager.GetTypedPoolable<StageNode>(new SpawnParams() { Parent = _stagesContainer });
                node.Setup(context, i);
                _stageNodes.Add(node);
            }
        }

        private void HandleReturnToLobbyPressed()
        {
            _handleReturnToLobbyPressed?.Invoke();
        }
    }
}