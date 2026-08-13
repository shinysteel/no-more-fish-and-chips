using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.Voyages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class StageNode : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _nameText;

        private VoyageManager _voyageManager;
        private LocalisationManager _localisationManager;

        private void Awake()
        {
            _voyageManager = GameManager.Instance.Get<VoyageManager>();
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
        }

        public void Setup(GameplayContext context, int stageIndex)
        {
            StageData data = _voyageManager.GetStageData(context.VoyageRunner.StageIds[stageIndex]);

            _image.sprite = data.Sprite;
            _nameText.text = _localisationManager.GetString(data.NameTerm);
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}