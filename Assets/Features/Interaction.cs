using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips
{
    public abstract class Interaction
    {
        private PoolManager _poolManager;

        public abstract ActionHotkey Hotkey { get; }
        public abstract LocalisationTerm Term { get; }

        public Interaction()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public virtual bool CanPrompt()
        {
            return true;
        }

        public virtual InteractPrompt CreatePrompt(SpawnParams parameters)
        {
            InteractPrompt prompt = _poolManager.GetTypedPoolable<InteractPrompt>(parameters);
            prompt.SetupInteract(Hotkey, Term);
            return prompt;
        }

        public virtual bool CanInteract()
        {
            return true;
        }

        public abstract void Interact();

        public virtual void ShowPreview()
        { }

        public virtual void SetPreviewColor(Color color)
        { }

        public virtual void HidePreview()
        { }
    }
}