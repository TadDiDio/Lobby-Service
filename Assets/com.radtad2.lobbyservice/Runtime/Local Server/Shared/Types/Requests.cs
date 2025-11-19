namespace LobbyService.LocalServer
{
    public interface IRequest { }

    public class ConnectRequest : IRequest { }
    public class DisconnectRequest : IRequest { }
    
    public class CreateLobbyRequest : IRequest
    {
        public int Capacity { get; set; }
        public string Name { get; set; }
        public LocalLobbyType LobbyType { get; set; }
    }
    
    public class JoinLobbyRequest : IRequest
    {
        public string LobbyId { get; set; }
    }

    public class LeaveLobbyRequest : IRequest
    {
        public string LobbyId { get; set; }
    }
    
    public class CloseLobbyRequest : IRequest
    {
        public string LobbyId { get; set; }
    }

    public class InviteMemberRequest : IRequest
    {
        public string LobbyId { get; set; }
        public string InviteeId { get; set; }
    }
    
    public class KickMemberRequest : IRequest
    {
        public string LobbyId { get; set; }
        public string KickeeId { get; set; }
    }
    
    public class SetOwnerRequest : IRequest
    {
        public string LobbyId { get; set; }
        public string NewOwnerId { get; set; }
    }

    public class LobbyDataRequest : IRequest
    {
        public string LobbyId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
    
    public class MemberDataRequest : IRequest
    {
        public string LobbyId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class QueryFriendsRequest : IRequest { }

    public class BrowseRequest : IRequest { }
}