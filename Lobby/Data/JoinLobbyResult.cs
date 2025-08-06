namespace LobbyService
{
    /// <summary>
    /// The result of attemping to join a lobby.
    /// </summary>
    public struct JoinLobbyResult
    {
        /// <summary>
        /// Whether the join operation was successful.
        /// </summary>
        public bool Success;

        /// <param name="success">Whether the join operation was sucecssful.</param>
        public JoinLobbyResult(bool success) { Success = success; }
    }
}
