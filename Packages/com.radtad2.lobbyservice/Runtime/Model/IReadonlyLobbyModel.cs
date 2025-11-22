using System.Collections.Generic;

namespace LobbyService
{
    public interface IReadonlyLobbyModel
    {
        public bool InLobby { get; }
        ProviderId LobbyId { get; }
        LobbyMember Owner { get; }
        int Capacity { get; }
        LobbyType Type { get; }

        IReadOnlyList<LobbyMember> Members { get; }
        IReadOnlyMetadata LobbyData { get; }
        IReadOnlyDictionary<LobbyMember, Metadata> MemberData { get; }
    }
}
