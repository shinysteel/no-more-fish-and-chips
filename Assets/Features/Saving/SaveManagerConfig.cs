using UnityEngine;

namespace FishFlingers.Saving
{
    [CreateAssetMenu(fileName = "SaveManagerConfig", menuName = "Configs/Managers/SaveManagerConfig")]
    public class SaveManagerConfig : ScriptableObject
    {
        [SerializeField] private string _userSaveFileName;
        [SerializeField] private string _gameSaveFileName;

        public string UserSaveFileName => _userSaveFileName;
        public string GameSaveFileName => _gameSaveFileName;
    }
}