using System;
using UnityEngine;

namespace FishFlingers.Scenes
{
    /// <summary>
    /// Couldn't figure out how to serialise scenes in inspector. This is as good
    /// as it gets for now, but its not great having to manually enter and maintain
    /// the scene as a string
    /// </summary>
    /// 
    [Serializable]
    public class SceneMapping
    {
        [SerializeField] private EScene _enum;
        [SerializeField] private string _name;

        public EScene Enum => _enum;
        public string Name => _name;
    }

    [CreateAssetMenu(fileName = "SceneManagerConfig", menuName = "Configs/Managers/SceneManagerConfig")]
    public class SceneManagerConfig : ScriptableObject
    {
        [SerializeField] private SceneMapping[] _sceneMappings;

        public SceneMapping[] SceneMappings => _sceneMappings;
    }
}