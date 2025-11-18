namespace LobbyService
{
    /// <summary>
    /// Provides view functionality for core lobby actions
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Called when the lobby system is reset and all state should be cleared
        /// </summary>
        /// <param name="capabilities">The capabilities of the currently configured lobby.</param>
        public void ResetView(ILobbyCapabilities capabilities);
    }
}
