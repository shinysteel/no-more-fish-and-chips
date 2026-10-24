using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;
using NoMoreFishAndChips.Interactions;

namespace NoMoreFishAndChips.Environments
{
    public class DockBell : MonoBehaviour, IInteractable
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        private MeshRenderer[] _meshRenderers;

        Vector3 IInteractable.Position => transform.position;
        IEnumerable<Renderer> IInteractable.Renderers => _meshRenderers;
        IInteractableSettings IInteractable.IInteractableSettings =>  _iInteractableSettings;
        private List<Interaction> _interactions;
        List<Interaction> IInteractable.Interactions => _interactions;

        private void Awake()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();

            _interactions = new List<Interaction>() { new RingDockBellInteraction(this) };
        }
    }
}