using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    /// <summary>
    /// Provides services for discovering and inviting friends.
    /// </summary>
    public interface IFriendProvider : IDisposable
    {
        /// <summary>
        /// Capabilities of this friends provider.
        /// </summary>
        public FriendCapabilities Capabilities { get; }
        
        /// <summary>
        /// Invoked when a refreshed list of friends is found.
        /// </summary>
        public event Action<List<LobbyMember>> FriendsUpdated;

        /// <summary>
        /// Starts polling for friends on an interval.
        /// </summary>
        /// <returns>List of friends as lobby members - they may or may not actually be in a lobby.</returns>
        public void StartFriendPolling(FriendDiscoveryFilter filter, float intervalSeconds, CancellationToken token = default);

        /// <summary>
        /// Sets the interval at which the polling happens.
        /// </summary>
        /// <param name="intervalSeconds">The seconds to wait between polling.</param>
        public void SetFriendPollingInterval(float intervalSeconds);

        /// <summary>
        /// Sets the filter to use when finding friends.
        /// </summary>
        /// <param name="filter"></param>
        public void SetFriendPollingFilter(FriendDiscoveryFilter filter);

        /// <summary>
        /// Stops polling for friends.
        /// </summary>
        public void StopFriendPolling();

        /// <summary>
        /// Gets the avatar associated with a lobby member.
        /// </summary>
        /// <param name="member">The member to get for.</param>
        /// <param name="token">A token to cancel the operation.</param>
        /// <returns>The avatar.</returns>
        public Task<Texture2D> GetFriendAvatar(LobbyMember member, CancellationToken token);
    }
}
