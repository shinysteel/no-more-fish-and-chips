using NoMoreFishAndChips.States;
using UnityEngine;
using System.Collections.Generic;

namespace NoMoreFishAndChips.Entities
{
    public class Cannon : Structure<CannonDefinitionData>, IInteractable
    {
        Vector3 IInteractable.Position => transform.position;
        IEnumerable<Renderer> IInteractable.Renderers => _entityModel.MeshRenderers;
        IInteractableSettings IInteractable.IInteractableSettings => DefinitionData.IInteractableSettings;
        private List<Interaction> _interactions = new();
        List<Interaction> IInteractable.Interactions => _interactions;

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _interactions.Add(new CannonManInteraction(this, _context));
            _interactions.Add(new CannonLoadInteraction(this, _context));
        }
    }
}