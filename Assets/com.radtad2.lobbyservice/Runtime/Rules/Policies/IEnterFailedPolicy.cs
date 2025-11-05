namespace LobbyService
{
    public interface IEnterFailedPolicy<TRequest>
    {
        public void Handle(LobbyController controller, EnterFailedResult<TRequest> failure);
    }
}
