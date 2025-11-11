using System;

namespace LobbyService.LocalServer
{
    public interface IResponse { }

    public class DummyResponse : IResponse { }
    
    public class WelcomeResponse : IResponse
    {
        public LocalLobbyMember LocalMember { get; set; }
    }
    
    public class EnterResponse : IResponse
    {
        public LobbySnapshot Snapshot { get; set; }
    }
}