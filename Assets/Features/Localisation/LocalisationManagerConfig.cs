using UnityEngine;

namespace FishFlingers.Localisation
{
    [CreateAssetMenu(fileName = "LocalisationManagerConfig", menuName = "Configs/Managers/LocalisationManagerConfig")]
    public class LocalisationManagerConfig : ScriptableObject
    {
        [SerializeField] private LocalisationTable _browseGamesPanelTable;
        [SerializeField] private LocalisationTable _commonTable;
        [SerializeField] private LocalisationTable _mainMenuScreenTable;
        [SerializeField] private LocalisationTable _fishingBagPanelTable;
        [SerializeField] private LocalisationTable _buildingKitPanelTable;
        [SerializeField] private LocalisationTable _entitiesTable;
        [SerializeField] private LocalisationTable _charactersTable;
        [SerializeField] private LocalisationTable _structuresTable;

        public LocalisationTable[] GetTables()
        {
            return new LocalisationTable[]
            {
                _browseGamesPanelTable,
                _commonTable,
                _mainMenuScreenTable,
                _fishingBagPanelTable,
                _buildingKitPanelTable,
                _entitiesTable,
                _charactersTable,
                _structuresTable,
            };
        }
    }
}