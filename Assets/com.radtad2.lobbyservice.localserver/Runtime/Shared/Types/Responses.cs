namespace LobbyService.LocalServer
{
    public interface IResponse { }
    
    public class EnterResponse : IResponse
    {
        public string LobbyId;
    }
}