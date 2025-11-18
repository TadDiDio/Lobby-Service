using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService.LocalServer
{
    public class LocalFriendsProvider : IFriendProvider
    {
        public event Action<List<LobbyMember>> FriendsUpdated;
        
        private LocalProvider _provider;
        private float _pollingInterval;
        private CancellationTokenSource _friendCts;
        
        public LocalFriendsProvider(LocalProvider provider)
        {
            _provider = provider;
        }
        
        public FriendCapabilities Capabilities { get; }= new FriendCapabilities
        {
            SupportsAvatars = false
        };
        
        public void StartFriendPolling(FriendDiscoveryFilter filter, float intervalSeconds, CancellationToken token = default)
        {
            EnsureInitialized();
            
            _pollingInterval = intervalSeconds;

            _friendCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_friendCts.Token, token);

            _ = FriendLoop(cts.Token);
        }

        private async Task FriendLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var response = await LocalLobby.GetFriends(token: token);
            
                    if (response.Error is Error.Ok)
                    {
                        FriendsUpdated?.Invoke(response.Response.Friends.ToLobbyMembers());
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_pollingInterval), token);
                }
            }
            catch (OperationCanceledException) { /* Ignored */ }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public void SetFriendPollingInterval(float intervalSeconds)
        {
            _pollingInterval = intervalSeconds;
        }

        public void SetFriendPollingFilter(FriendDiscoveryFilter filter) { }

        public void StopFriendPolling()
        {
            _friendCts?.Cancel();
            _friendCts?.Dispose();
            _friendCts = null;
        }

        public async Task<Texture2D> GetFriendAvatar(LobbyMember member, CancellationToken token = default)
        {
            await Task.CompletedTask;
            return Texture2D.whiteTexture;
        }
    }
}