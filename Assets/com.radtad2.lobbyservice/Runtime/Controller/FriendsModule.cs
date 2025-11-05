using System;
using System.Collections.Generic;
using System.Threading;

namespace LobbyService
{
    public class FriendsModule : IDisposable
    {
        private ILobbyFriendService _provider;
        private List<LobbyMember> _currentFriends = new();

        private CancellationTokenSource _tokenSource = new();

        public FriendsModule(ILobbyFriendService friends)
        {
            _provider = friends;
            _provider.FriendsUpdated += OnUpdate;
        }

        public void Dispose()
        {
            StopPolling();
            if (_provider != null)
            {
                _provider.FriendsUpdated -= OnUpdate;
            }

            if (_tokenSource is { IsCancellationRequested: false })
            {
                _tokenSource?.Cancel();
            }

            _tokenSource?.Dispose();
            _currentFriends = null;
        }

        private void OnUpdate(List<LobbyMember> friends)
        {
            _currentFriends = friends;
        }

        /// <summary>
        /// Gets the most up to date list of friends. Will never return null.
        /// </summary>
        public List<LobbyMember> GetFriends() => _currentFriends;

        /// <summary>
        /// Starts automatic polling for friends.
        /// </summary>
        public void StartPolling(FriendDiscoveryFilter filter, float intervalSeconds)
            => _provider.StartFriendPolling(filter, intervalSeconds, _tokenSource.Token);

        /// <summary>
        /// Stops automatically polling for friends
        /// </summary>
        public void StopPolling() => _provider.StopFriendPolling();

        /// <summary>
        /// Sets the filter to use when looking for friends.
        /// </summary>
        public void SetFilter(FriendDiscoveryFilter filter) => _provider.SetFriendPollingFilter(filter);

        /// <summary>
        /// Sets the interval in seconds to wait before periodically refreshing the friend list.
        /// </summary>
        public void SetInterval(float intervalSeconds) => _provider.SetFriendPollingInterval(intervalSeconds);
    }
}
