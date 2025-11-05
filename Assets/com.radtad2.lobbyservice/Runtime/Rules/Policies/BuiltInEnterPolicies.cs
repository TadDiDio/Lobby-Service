using System.Threading.Tasks;

namespace LobbyService
{
    #region Creating
    /// <summary>
    /// Leaves the current lobby, then creates another one.
    /// </summary>
    public class LeaveThenCreateJoinPolicy : IEnterRequestedWhileInLobbyPolicy<CreateLobbyRequest>
    {
        public async Task Execute(CoreModule core, CreateLobbyRequest request, ProviderId currentLobbyId)
        {
            core.Leave();
            await core.CreateLobbyAsync(request, 0);
        }
    }

    /// <summary>
    /// Disallows the create attempt.
    /// </summary>
    public class ProhibitCreatePolicy : IEnterRequestedWhileInLobbyPolicy<CreateLobbyRequest>
    {
        public async Task Execute(CoreModule core, CreateLobbyRequest request, ProviderId currentLobbyId)
        {
            await Task.CompletedTask;
        }
    }
    #endregion

    #region Joining
    /// <summary>
    /// Leaves the current lobby, then joins another one.
    /// </summary>
    public class LeaveThenJoinJoinPolicy : IEnterRequestedWhileInLobbyPolicy<JoinLobbyRequest>
    {
        public async Task Execute(CoreModule core, JoinLobbyRequest request, ProviderId currentLobbyId)
        {
            core.Leave();
            await core.JoinLobbyAsync(request, 0);
        }
    }

    /// <summary>
    /// Disallows the join attempt.
    /// </summary>
    public class ProhibitJoinPolicy : IEnterRequestedWhileInLobbyPolicy<JoinLobbyRequest>
    {
        public async Task Execute(CoreModule core, JoinLobbyRequest request, ProviderId currentLobbyId)
        {
            await Task.CompletedTask;
        }
    }
    #endregion
}
