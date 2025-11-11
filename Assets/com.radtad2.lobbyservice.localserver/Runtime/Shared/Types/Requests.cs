namespace LobbyService.LocalServer
{
    public interface IRequest { }

    public class ConnectRequest : IRequest { }
    public class DisconnectRequest : IRequest { }
    
    public class CreateLobbyRequest : IRequest
    {
        public int Capacity { get; set; }
    }
    
    public class JoinLobbyRequest : IRequest
    {
        public string LobbyId { get; set; }
    }

    public class LeaveLobbyRequest : IRequest
    {
        public string LobbyId { get; set; }
    }
}