using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    [Serializable]
    public class DropOrientation
    {
        [SerializeField] private Vector3[] _positions;

        public Vector3[] Positions => _positions;
    }

    [CreateAssetMenu(fileName = "DroppedItemData", menuName = "Data/Entities/DroppedItemData")]
    public class DroppedItemData : EntityData
    {
        [SerializeField] private DropOrientation[] _dropOrientations;

        public DropOrientation[] DropOrientations => _dropOrientations;
    }
}