using System.Collections.Generic;

namespace LobbyService.LocalServer
{
    public interface IResponse { }

    public class NullResponse : IResponse { }
    
    public class WelcomeResponse : IResponse
    {
        public LocalLobbyMember LocalMember { get; set; }
    }
    
    public class EnterResponse : IResponse
    {
        public LobbySnapshot Snapshot { get; set; }
    }

    
    
    
    
    
    
    public class QueryFriendsResponse : IResponse
    {
        public List<LocalLobbyMember> Friends { get; set; }
    }
}