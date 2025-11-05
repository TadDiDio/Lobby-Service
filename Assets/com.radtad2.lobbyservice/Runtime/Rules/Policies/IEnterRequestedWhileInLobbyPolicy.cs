using System.Threading.Tasks;

namespace LobbyService
{
    public interface IEnterRequestedWhileInLobbyPolicy<in TRequest>
    {
        /// <summary>
        /// Executes a policy handling the case that the user attempts to join a lobby while in another.
        /// </summary>
        /// <param name="core">A module used to invoke actions.</param>
        /// <param name="request">The request.</param>
        /// <param name="currentLobbyId">The current lobby Id.</param>
        /// <remarks>Use the core to call methods to execute the policy.</remarks>
        public Task Execute(CoreModule core, TRequest request, ProviderId currentLobbyId);
    }
}
