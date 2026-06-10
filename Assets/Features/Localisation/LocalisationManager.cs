using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NoMoreFishAndChips.Localisation
{
    public interface ILocalisationManagerListener
    { }

    public class LocalisedStringLookup
    {
        private string _table;
        private string _key;

        public LocalisedStringLookup(string table, string key)
        {
            _table = table;
            _key = key;
        }

        public string GetString()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(_table, _key);
        }
    }

    public class LocalisationManager : GameSystem<ILocalisationManagerListener>
    {
        private LocalisationManagerConfig _config;

        private Dictionary<LocalisationTerm, LocalisedStringLookup> _termLookupMap = new();

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.LocalisationManagerConfig;

            AsyncOperationHandle<IList<StringTable>> op = LocalizationSettings.StringDatabase.GetAllTables();

            op.Completed += completed =>
            {
                foreach (StringTable table in completed.Result)
                {
                    foreach (StringTableEntry entry in table.Values)
                    {
                        _termLookupMap.Add((LocalisationTerm)Utils.Math.HashLongToInt(entry.KeyId), new LocalisedStringLookup(table.TableCollectionName, entry.Key));
                    }
                }

                State = ManagerState.Ready;
            };
        }

        public string GetString(LocalisationTerm term)
        {
            return _termLookupMap[term].GetString();
        }
    }
}