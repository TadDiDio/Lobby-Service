namespace LobbyService.LocalServer
{
    public interface IRequest { }

    public class CreateLobbyRequest : IRequest
    {
        public int Capacity;
    }
}