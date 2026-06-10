using UnityEngine;

namespace NoMoreFishAndChips.Saving
{
    [CreateAssetMenu(fileName = "SaveManagerConfig", menuName = "Configs/Managers/SaveManagerConfig")]
    public class SaveManagerConfig : ScriptableObject
    {
        [SerializeField] private string _userSaveFileName;
        [SerializeField] private string _gameSaveFileName;
        [SerializeField] private string _thumbnailFileName;

        public string UserSaveFileName => _userSaveFileName;
        public string GameSaveFileName => _gameSaveFileName;
        public string ThumbnailFileName => _thumbnailFileName;

        [SerializeField] private string _gameSavesFolderName;
        [SerializeField] private string _defaultGameSaveFolderName;

        public string GameSavesFolderName => _gameSavesFolderName;
        public string DefaultGameSaveFolderName => _defaultGameSaveFolderName;
    }
}