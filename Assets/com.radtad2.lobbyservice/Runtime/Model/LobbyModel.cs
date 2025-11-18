using System.Collections.Generic;

namespace LobbyService
{
    /// <summary>
    /// A cache of the current lobby state.
    /// </summary>
    public class LobbyModel : IReadonlyLobbyModel
    {
        public bool InLobby;
        public ProviderId LobbyId;
        public LobbyMember Owner;
        public int Capacity;
        public LobbyType Type;
        public List<LobbyMember> Members = new();
        public Metadata LobbyData = new();
        public Dictionary<LobbyMember, Metadata> MemberData = new();

        bool IReadonlyLobbyModel.InLobby => InLobby;
        ProviderId IReadonlyLobbyModel.LobbyId => LobbyId;
        LobbyMember IReadonlyLobbyModel.Owner => Owner;
        int IReadonlyLobbyModel.Capacity => Capacity;
        LobbyType IReadonlyLobbyModel.Type => Type;

        IReadOnlyList<LobbyMember> IReadonlyLobbyModel.Members => Members;
        IReadOnlyMetadata IReadonlyLobbyModel.LobbyData => LobbyData;
        IReadOnlyDictionary<LobbyMember, Metadata> IReadonlyLobbyModel.MemberData => MemberData;

        public void RemoveMember(LobbyMember member)
        {
            Members.Remove(member);
            MemberData.Remove(member);
        }

        public void AddMember(MemberJoinedInfo info)
        {
            Members.Add(info.Member);
            MemberData[info.Member] = info.Data;
        }

        public void Initialize(EnterLobbyResult result)
        {
            if (!result.Success) return;

            Reset();
            
            InLobby = true;
            LobbyId = result.LobbyId;
            Owner = result.Owner;
            Capacity = result.Capacity;
            Type = result.Type;
            Members = new List<LobbyMember>(result.Members);
            LobbyData = new Metadata(result.LobbyData);
            MemberData = new Dictionary<LobbyMember, Metadata>(result.MemberData);
        }

        public void Reset()
        {
            InLobby = false;
            LobbyId = null;
            Owner = null;
            Capacity = 0;
            Type = default;
            Members = new List<LobbyMember>();
            LobbyData = new Metadata();
            MemberData = new Dictionary<LobbyMember, Metadata>();
        }
    }
}
