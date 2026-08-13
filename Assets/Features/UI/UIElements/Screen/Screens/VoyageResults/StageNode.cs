using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.Voyages;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.UI
{
    public class StageNode : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Transform _wavesContainer;

        private PoolManager _poolManager;
        private VoyageManager _voyageManager;
        private LocalisationManager _localisationManager;

        private List<WaveNode> _wavesNodes = new();

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _voyageManager = GameManager.Instance.Get<VoyageManager>();
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
        }

        public void OnReturnedToPool()
        {
            foreach (WaveNode node in _wavesNodes)
            {
                _poolManager.ReturnTypedPoolable(node);
            }   

            _wavesNodes.Clear();
        }

        public void Setup(GameplayContext context, int stageIndex)
        {
            StageData data = _voyageManager.GetStageData(context.VoyageRunner.StageIds[stageIndex]);

            _image.sprite = data.Sprite;
            _nameText.text = _localisationManager.GetString(data.NameTerm);

            for (int i = 0; i < data.Waves.Length; i++)
            {
                WaveNode node = _poolManager.GetTypedPoolable<WaveNode>(new SpawnParams() { Parent = _wavesContainer });

                bool complete = context.VoyageRunner.VoyageResult == VoyageResult.Victory
                    || stageIndex < context.VoyageRunner.StageIndex
                    || (stageIndex == context.VoyageRunner.StageIndex && i < context.VoyageRunner.WaveIndex);

                node.Setup(complete);

                _wavesNodes.Add(node);
            }
        }

        public void OnTakenFromPool()
        { }
    }
}