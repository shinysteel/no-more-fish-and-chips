using NoMoreFishAndChips.UI;
using UnityEngine;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Environments;
using System.Collections.Generic;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.Networking;
using PurrNet;

namespace NoMoreFishAndChips.Entities
{
    public class TreePlanter : Structure<TreePlanterDefinitionData>, IInteractable
    {
        private EntityManager _entityManager;

        Vector3 IInteractable.Position => transform.position;
        IEnumerable<Renderer> IInteractable.Renderers => _entityModel.MeshRenderers;
        IInteractableSettings IInteractable.IInteractableSettings => DefinitionData.IInteractableSettings;
        private List<Interaction> _interactions = new();
        List<Interaction> IInteractable.Interactions => _interactions;

        protected override void Awake()
        {
            base.Awake();

            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _interactions.Add(new TreePlanterPlantInteraction(this, _context));
        }

        [ServerRpc(requireOwnership: false)]
        public void PlantRpc()
        {
            Vector3 position = transform.position + Vector3.up * 0.1875f;
            
            _entityManager.Spawn(EntityId.PalmTree, new SpawnParams() { Position = position, Parent = transform });
        }
    }
}