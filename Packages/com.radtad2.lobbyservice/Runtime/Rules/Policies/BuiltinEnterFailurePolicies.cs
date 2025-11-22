namespace LobbyService
{
    public class NullEnterFailurePolicies : IEnterFailedPolicy<CreateLobbyRequest>
    {
        public void Handle(LobbyController controller, EnterFailedResult<CreateLobbyRequest> failure)
        {
            // No-op
        }
    }

    public class NullJoinFailurePolicy : IEnterFailedPolicy<JoinLobbyRequest>
    {
        public void Handle(LobbyController controller, EnterFailedResult<JoinLobbyRequest> failure)
        {
            // No-op
        }
    }
}
