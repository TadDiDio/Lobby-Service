using System.Collections.Generic;
using System.Linq;

namespace LobbyService
{
    public class Metadata : IReadOnlyMetadata
    {
        public Metadata() { }

        public Metadata(Metadata metadata)
        {
            _data = new Dictionary<string, string>(metadata._data);
        }
        public IReadOnlyDictionary<string, string> Data => _data;
        private Dictionary<string, string> _data = new();

        public bool TryGet(string key, out string value)
        {
            var stored = _data.TryGetValue(key, out value);

            return stored && !string.IsNullOrEmpty(value);
        }

        public string GetOrDefault(string key, string defaultValue)
        {
            var value = _data.GetValueOrDefault(key, defaultValue);

            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public void Set(string key, string value)
        {
            _data[key] = value;
        }

        public List<string> GetKeys() => _data.Keys.ToList();
        public List<string> GetValues() => _data.Values.ToList();
    }
}
