using System.Collections.Generic;

namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all requests.
    /// </summary>
    public class NullFriendModule : IFriendAPI
    {
        public void StartPolling(FriendDiscoveryFilter filter, float intervalSeconds) { }
        public void SetInterval(float intervalSeconds) { }
        public void SetFilter(FriendDiscoveryFilter filter) { }
        public void StopPolling() { }
        public void RequestAvatar(LobbyMember member) { }
        public List<LobbyMember> GetFriends() => new();
        public void Dispose() { }
    }
}