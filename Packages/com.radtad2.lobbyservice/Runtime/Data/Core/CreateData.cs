using System;

namespace LobbyService
{
    /// <summary>
    /// A request to create a lobby.
    /// </summary>
    [Serializable]
    public struct CreateLobbyRequest
    {
        /// <summary>
        /// The name of the lobby.
        /// </summary>
        public string Name;

        /// <summary>
        /// The maximum number of allowed clients.
        /// </summary>
        public int Capacity;

        /// <summary>
        /// The type of lobby.
        /// </summary>
        public LobbyType LobbyType;

        // TODO: Add meta kvps to add on startup.
    }
}
