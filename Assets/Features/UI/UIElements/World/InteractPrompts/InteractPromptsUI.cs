using UnityEngine;
using System.Collections.Generic;
using NoMoreFishAndChips.Interactions;
using UnityEngine.Pool;
using NoMoreFishAndChips.Pools;

namespace NoMoreFishAndChips.UI
{
    public class InteractPromptsUI : WorldUI
    {
        private PoolManager _poolManager;

        private Dictionary<Interaction, InteractPrompt> _interactionPromptMap = new();

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void SetInteractionsNonAlloc(List<Interaction> interactions)
        {
            List<Interaction> removes = ListPool<Interaction>.Get();

            foreach (Interaction interaction in _interactionPromptMap.Keys)
            {
                if (!interactions.Contains(interaction))
                {
                    removes.Add(interaction);
                }
            }

            foreach (Interaction remove in removes)
            {
                _poolManager.ReturnTypedPoolable(_interactionPromptMap[remove]);
                _interactionPromptMap.Remove(remove);
            }

            ListPool<Interaction>.Release(removes);

            foreach (Interaction interaction in interactions)
            {
                if (!_interactionPromptMap.ContainsKey(interaction))
                {
                    InteractPrompt prompt = interaction.CreatePrompt(new SpawnParams() { Parent = transform });
                    _interactionPromptMap.Add(interaction, prompt);
                }
            }

            for (int i = 0; i < interactions.Count; i++)
            {
                _interactionPromptMap[interactions[i]].transform.SetSiblingIndex(i);
            }
        }
    }
}