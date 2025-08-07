using LobbyService;

namespace LobbyService
{
    public interface ILobbyDataService
    {
        /// <summary>
        /// Sets the server data. Only the owner can do this.
        /// </summary>
        /// <param name="serverData">The new server data.</param>
        public void SetServerData(ServerData serverData);

        /// <summary>
        /// Gets lobby data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public string GetLobbyData(string key);

        /// <summary>
        /// Sets the lobby data.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set to.</param>
        public void SetLobbyData(string key, string value);

        /// <summary>
        /// Gets member data.
        /// </summary>
        /// <param name="member">The memer whose data to get.</param>
        /// <param name="key">The key to get.</param>
        /// <returns>The value.</returns>
        public string GetMemberData(LobbyMember member, string key);

        /// <summary>
        /// Sets member data for the local client.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set to.</param>
        public void SetMemberData(string key, string value);

        /// <summary>
        /// Invoked when the server data changes.
        /// </summary>
        public event Action<ServerData> ServerDataUpdated;

        /// <summary>
        /// Invoked when the lobby metadata is updated.
        /// </summary>
        public event Action<LobbyDataUpdate> LobbyDataUpdated;

        /// <summary>
        /// Invoked when a member's metadata is updated.
        /// </summary>
        public event Action<MemberDataUpdate> MemberDataUpdated;
    }
}
