using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LobbyService
{
    public class StaleLobbyManager
    {
        private static string _filePath = Path.Combine(Application.persistentDataPath, "stale_lobby_map.json");

        [Serializable]
        private class Entry
        {
            public string ProviderType;
            public string ProviderId;
        }

        [Serializable]
        private class EntryCollection
        {
            public List<Entry> Entries = new();
        }

        private readonly Dictionary<string, string> _cache = Load();

        /// <summary>
        /// Gets a stale id if there is one.
        /// </summary>
        /// <param name="providerType"></param>
        /// <param name="staleId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryGetStaleId(Type providerType, out ProviderId staleId)
        {
            if (providerType == null) throw new ArgumentNullException(nameof(providerType));

            if (_cache.TryGetValue(providerType.AssemblyQualifiedName!, out var idString))
            {
                staleId = new ProviderId(idString);
                return true;
            }

            staleId = null;
            return false;
        }

        /// <summary>
        /// Records a provider id in persistent storage.
        /// </summary>
        /// <param name="providerType">The provider type associated with the id.</param>
        /// <param name="id">The lobby id.</param>
        /// <exception cref="ArgumentNullException">Throws if the provider type is null.</exception>
        public void RecordId(Type providerType, ProviderId id)
        {
            if (providerType == null) throw new ArgumentNullException(nameof(providerType));

            var key = providerType.AssemblyQualifiedName!;
            _cache[key] = id.ToString();

            Save(_cache);
        }

        /// <summary>
        /// Erases a provider id from storage.
        /// </summary>
        /// <param name="providerType">The provider type associated with the id.</param>
        /// <exception cref="ArgumentNullException">Throws if the provider type is null.</exception>
        public void EraseId(Type providerType)
        {
            if (providerType == null) throw new ArgumentNullException(nameof(providerType));

            var key = providerType.AssemblyQualifiedName!;
            _cache.Remove(key);

            Save(_cache);
        }

        private static Dictionary<string, string> Load()
        {
            try
            {
                if (!File.Exists(_filePath)) return new Dictionary<string, string>();

                var json = File.ReadAllText(_filePath);
                var data = JsonUtility.FromJson<EntryCollection>(json);

                return data?.Entries?.ToDictionary(e => e.ProviderType, e => e.ProviderId) ?? new Dictionary<string, string>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load stale lobby map: {e}");
                return new Dictionary<string, string>();
            }
        }

        private static void Save(Dictionary<string, string> map)
        {
            try
            {
                var data = new EntryCollection
                {
                    Entries = map.Select(kvp => new Entry
                    {
                        ProviderType = kvp.Key,
                        ProviderId = kvp.Value
                    }).ToList()
                };

                var json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to save stale lobby map: {e}");
            }
        }
    }
}
