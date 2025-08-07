namespace LobbyService
{
    /// <summary>
    /// The result of attempting to create a lobby.
    /// </summary>
    public struct CreateLobbyResult
    {
        /// <summary>
        /// Whether the create operation was successful.
        /// </summary>
        public bool Success;

        /// <param name="success">Whether the create operation was successful.</param>
        public CreateLobbyResult(bool success) { Success = success; }
    }
}
