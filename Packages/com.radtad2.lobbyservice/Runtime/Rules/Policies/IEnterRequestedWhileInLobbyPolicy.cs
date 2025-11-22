namespace LobbyService
{
    public interface IEnterRequestedWhileInLobbyPolicy<in TRequest>
    {
        /// <summary>
        /// Executes a policy handling the case that the user attempts to join a lobby while in another.
        /// </summary>
        /// <param name="controller">A controller used to invoke raw actions. Prefer ForceXXX methods here since they
        /// bypass policy handlers like this one.</param>
        /// <param name="request">The request.</param>
        /// <param name="currentLobbyId">The current lobby Id.</param>
        /// <returns>Whether to proceed with the enter or not.</returns>
        /// <remarks>Use the core to call methods to execute the policy.</remarks>
        public bool Execute(LobbyController controller, TRequest request, ProviderId currentLobbyId);
    }
}
