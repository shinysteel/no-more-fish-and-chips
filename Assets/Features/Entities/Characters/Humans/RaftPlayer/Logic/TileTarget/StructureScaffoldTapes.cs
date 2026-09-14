using NoMoreFishAndChips.Pools;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class StructureScaffoldTapes : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private GameObject _forwardTape;
        [SerializeField] private GameObject _rightTape;
        [SerializeField] private GameObject _backTape;
        [SerializeField] private GameObject _leftTape;

        public void SetSides(bool forward, bool right, bool back, bool left)
        {
            _forwardTape.SetActive(forward);
            _rightTape.SetActive(right);
            _backTape.SetActive(back);
            _leftTape.SetActive(left);
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}