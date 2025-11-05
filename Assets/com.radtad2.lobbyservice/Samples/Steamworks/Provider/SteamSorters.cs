using LobbyService;
using System.Collections.Generic;

namespace LobbyService.Samples.Steam
{
    public class CurrentMemberCountSteamSorter : ILobbySorter
    {
        public int CompareTo(LobbyDescriptor a, LobbyDescriptor b)
        {
            return -Comparer<int>.Default.Compare(a.MemberCount, b.MemberCount);
        }
    }

    public class CapacitySteamSorter : ILobbySorter
    {
        public int CompareTo(LobbyDescriptor a, LobbyDescriptor b)
        {
            return -Comparer<int>.Default.Compare(a.MaxMembers, b.MaxMembers);
        }
    }
}
