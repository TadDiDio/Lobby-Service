using System.Threading.Tasks;

namespace LobbyService
{
    #region Creating
    /// <summary>
    /// Leaves the current lobby, then creates another one.
    /// </summary>
    public class LeaveThenCreateJoinPolicy : IEnterRequestedWhileInLobbyPolicy<CreateLobbyRequest>
    {
        public bool Execute(LobbyController controller, CreateLobbyRequest request, ProviderId currentLobbyId)
        {
            controller.Leave();
            return true;
        }
    }

    /// <summary>
    /// Disallows the create attempt.
    /// </summary>
    public class ProhibitCreatePolicy : IEnterRequestedWhileInLobbyPolicy<CreateLobbyRequest>
    {
        public bool Execute(LobbyController controller, CreateLobbyRequest request, ProviderId currentLobbyId) => false;
    }
    #endregion

    #region Joining
    /// <summary>
    /// Leaves the current lobby, then joins another one.
    /// </summary>
    public class LeaveThenJoinJoinPolicy : IEnterRequestedWhileInLobbyPolicy<JoinLobbyRequest>
    {
        public bool Execute(LobbyController controller, JoinLobbyRequest request, ProviderId currentLobbyId)
        {
            controller.Leave();
            return true;
        }
    }

    /// <summary>
    /// Disallows the join attempt.
    /// </summary>
    public class ProhibitJoinPolicy : IEnterRequestedWhileInLobbyPolicy<JoinLobbyRequest>
    {
        public bool Execute(LobbyController controller, JoinLobbyRequest request, ProviderId currentLobbyId) => false;
    }
    #endregion
}
