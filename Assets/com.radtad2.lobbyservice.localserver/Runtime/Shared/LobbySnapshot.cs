using System;
using System.Collections.Generic;

namespace LobbyService.LocalServer
{
    public class LobbySnapshot
    {
        public Guid LobbyId { get; set; }
        public LocalLobbyMember Owner { get; set; }
        public int Capacity { get; set; }
        public LocalLobbyType LobbyType { get; set; }
        public IReadOnlyList<LocalLobbyMember> Members { get; set; }
        public IReadOnlyDictionary<string, string> LobbyData { get; set; }
        public IReadOnlyDictionary<LocalLobbyMember, Dictionary<string, string>> MemberData { get; set; }
    }
}