using System.Collections.Generic;

namespace LobbyService
{
    public interface IReadOnlyMetadata
    {
        /// <summary>
        /// The underlying data.
        /// </summary>
        IReadOnlyDictionary<string, string> Data { get; }

        /// <summary>
        /// Gets the value if it exists, otherwise returns the default.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist.</param>
        /// <returns>The value or default if none.</returns>
        public string GetOrDefault(string key, string defaultValue);

        /// <summary>
        /// Tries to get data.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="value">The value if there is one.</param>
        /// <returns>True if the key exists.</returns>
        public bool TryGet(string key, out string value);

        /// <summary>
        /// Gets a copy of the keys.
        /// </summary>
        /// <returns>The keys.</returns>
        public List<string> GetKeys();

        /// <summary>
        /// Gets a copy of the values.
        /// </summary>
        /// <returns>The copy.</returns>
        public List<string> GetValues();
    }
}
