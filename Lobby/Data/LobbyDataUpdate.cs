namespace LobbyService
{
    /// <summary>
    /// Information on a lobby data update.
    /// </summary>
    public struct LobbyDataUpdate
    {
        /// <summary>
        /// The key that was updated.
        /// </summary>
        public string KeyUpdated;

        /// <summary>
        /// The new value for the key.
        /// </summary>
        public string NewValue;

        /// <param name="key">The key that was updated.</param>
        /// <param name="newValue">The new value.</param>
        public LobbyDataUpdate(string key, string newValue) 
        { 
            KeyUpdated = key;
            NewValue = newValue;
        }
    }
}
