using System;
using System.Collections.Generic;

namespace LobbyService.LocalServer
{
    public interface IEvent { }

    public class OtherMemberJoinedEvent : IEvent
    {
        public LocalLobbyMember Member { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
    
    public class OtherMemberLeftEvent : IEvent
    {
        public LocalLobbyMember Member;
        
        /// <summary>
        /// 0 = user requested, 1 = kicked
        /// </summary>
        public int LeaveReason { get; set; }

        /// <summary>
        /// 0 = general, 1 = lobby closed, 2 = owner stopped responding
        /// </summary>
        public int KickReason { get; set; }
    }

    public class LocalMemberKickedEvent : IEvent
    {
        /// <summary>
        /// 0 = general, 1 = lobby closed, 2 = owner stopped responding
        /// </summary>
        public int KickReason { get; set; } 
    }

    public class ReceivedInviteEvent : IEvent
    {
        public LocalLobbyMember Sender { get; set; }
        public Guid LobbyId { get; set; }
    }

    public class LobbyDataUpdateEvent : IEvent
    {
        public Dictionary<string, string> Metadata { get; set; }
    }
    
    public class MemberDataUpdateEvent : IEvent
    {
        public LocalLobbyMember Member { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class OwnerUpdateEvent : IEvent
    {
        public LocalLobbyMember NewOwner { get; set; }
    }
}