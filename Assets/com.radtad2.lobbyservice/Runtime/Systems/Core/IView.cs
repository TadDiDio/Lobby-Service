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
        public void Reset();
    }
}
