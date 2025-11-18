using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    public class FriendModule : IFriendAPI
    {
        private IFriendView _viewBus;
        private IFriendProvider _provider;
        private List<LobbyMember> _currentFriends = new();

        private CancellationTokenSource _tokenSource = new();

        public FriendModule(IFriendView viewBus, IFriendProvider friends)
        {
            _viewBus = viewBus;
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
            _viewBus.DisplayUpdatedFriendList(_currentFriends);
        }
        
        public List<LobbyMember> GetFriends() => _currentFriends;
        
        public void StartPolling(FriendDiscoveryFilter filter, float intervalSeconds)
            => _provider.StartFriendPolling(filter, intervalSeconds, _tokenSource.Token);
        
        public void StopPolling() => _provider.StopFriendPolling();
        
        public void SetFilter(FriendDiscoveryFilter filter) => _provider.SetFriendPollingFilter(filter);
       
        public void SetInterval(float intervalSeconds) => _provider.SetFriendPollingInterval(intervalSeconds);
        
        public void RequestAvatar(LobbyMember member)
        {
            _ = GetAvatar(member);
        }

        private async Task GetAvatar(LobbyMember member)
        {
            try
            {
                var avatar = await _provider.GetFriendAvatar(member, _tokenSource.Token);
                _viewBus.DisplayFriendAvatar(member, avatar);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
