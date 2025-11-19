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

    public class ApplyNumberFilterRequest : IRequest
    {
        public string Key { get; set; }
        public int Value { get; set; }

        /// <summary>
        /// 0 = NotEqual
        /// 1 = LessThan
        /// 2 = LessThanOrEqual
        /// 3 = Equal
        /// 4 = GreaterThan
        /// 5 = GreaterThanOrEqual
        /// </summary>
        public int ComparisonType { get; set; }
    }
    
    public class ApplyStringFilterRequest : IRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    
    public class ApplySlotsAvailableFilterRequest : IRequest
    {
        public int Min { get; set; }
    }
    
    public class ApplyLimitResponsesFilterRequest : IRequest
    {
        public int Max { get; set; }
    }

    public class ChatMessageRequest : IRequest
    {
        public string Message { get; set; }
    }
    
    public class DirectMessageRequest : IRequest
    {
        public string TargetId { get; set; }
        public string Message { get; set; }
    }
}
