using NoMoreFishAndChips.UI;
using UnityEngine;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Interactions;
using System.Collections.Generic;
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.Entities
{
    public class Planter : Structure<PlanterDefinitionData>, IInteractable
    {
        Vector3 IInteractable.Position => transform.position;
        IInteractableSettings IInteractable.IInteractableSettings => DefinitionData.IInteractableSettings;
        private List<Interaction> _interactions = new();
        List<Interaction> IInteractable.Interactions => _interactions;

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _interactions.Add(new PlantInteraction(this, _context));
        }
    }
}