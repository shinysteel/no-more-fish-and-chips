using PurrNet;
using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class CharacterModel : EntityModel
    {
        [SerializeField] private Transform _itemLocator;

        public Transform ItemLocator => _itemLocator;
    }
}