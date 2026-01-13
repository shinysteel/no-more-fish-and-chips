using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace FishFlingers.Localisation
{
    public enum LocalisationTerm
    {
        // Browse Games Panel
        BrowseGamesPanelTitle           = 1   ,
        BrowseGamesPanelLAN             = 2   ,
        BrowseGamesPanelSteam           = 3   ,

        // Common
        CommonQuit                      = 100 ,

        // Main Menu Screen
        MainMenuScreenGameTitle         = 200 ,
        MainMenuScreenBrowseGames       = 201 ,
        MainMenuScreenHostGame          = 202 ,

        // Fishing Bag Panel
        FishingBagPanelTitle            = 300 ,

        // Building Kit Panel
        BuildingKitPanelTitle           = 400 ,

        // Entities
        EntitiesRaftTileName            = 500 ,
        EntitiesRaftTileDescription     = 501 ,
        EntitiesDroppedItemName         = 502 ,
        EntitiesDroppedItemDescription  = 503 ,

        // Characters
        CharactersRaftPlayerName        = 600 ,
        CharactersRaftPlayerDescription = 601 ,
        CharactersFlyingFishName        = 602 ,
        CharactersFlyingFishDescription = 603 ,

        // Structures
        StructuresWaveSignName          = 700 ,
        StructuresWaveSignDescription   = 701 ,
    }

    [Serializable]
    public class LocalisationTable
    {
        [SerializeField] private string _name;
        [SerializeField] private string _id;
        [SerializeField] private LocalisationMapping[] _mappings;

        public string Name => _name;
        public string Id => _id;
        public LocalisationMapping[] Mappings => _mappings;
    }

    [Serializable]
    public class LocalisationMapping
    {
        [SerializeField] private LocalisationTerm _term;
        [SerializeField] private string _key;

        public LocalisationTerm Term => _term;
        public string Key => _key;
    }

    public class LocalisationLookup
    {
        public string TableId { get; private set; }
        public string EntryKey { get; private set; }

        public LocalisationLookup(string tableId, string entryKey)
        {
            TableId = tableId;
            EntryKey = entryKey;
        }
    }

    public interface ILocalisationManagerListener
    { }

    public class LocalisationManager : GameSystem<ILocalisationManagerListener>
    {
        private LocalisationManagerConfig _config;

        private Dictionary<LocalisationTerm, LocalisationLookup> _termLookupMap;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.LocalisationManagerConfig;

            _termLookupMap = new();

            foreach (LocalisationTable table in _config.GetTables())
            {
                foreach (LocalisationMapping mapping in table.Mappings)
                {
                    _termLookupMap.Add(mapping.Term, new LocalisationLookup(table.Name, $"{table.Id}.{mapping.Key}"));
                }
            }

            base.Initialise(config);
        }

        public string GetString(LocalisationTerm term)
        {
            LocalisationLookup lookup = _termLookupMap[term];
            return LocalizationSettings.StringDatabase.GetTable(lookup.TableId).GetEntry(lookup.EntryKey).GetLocalizedString();
        }
    }
}