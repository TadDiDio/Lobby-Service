namespace LobbyService
{
    /// <summary>
    /// A request to join a lobby.
    /// </summary>
    public struct JoinLobbyRequest
    {
        /// <summary>
        /// The lobby to join.
        /// </summary>
        public ProviderId LobbyId;
    }
}
