using System;
using LobbyService.Heartbeat;

namespace LobbyService
{
    public interface ILobbyHeartbeatService
    {
        /// <summary>
        /// Invoked when a member whose heartbeat you are listening to times out.
        /// </summary>
        public event Action<HeartbeatTimeout> OnHeartbeatTimeout;

        /// <summary>
        /// Starts broadcasting and listening to heartbeat messages for this member.
        /// </summary>
        /// <param name="lobbyId">The lobby.</param>
        /// <param name="intervalSeconds">The seconds to wait between heartbeats.</param>
        /// <param name="othersTimeoutSeconds">How long should it take to consider another member to be timed out.</param>
        public void StartOwnHeartbeat(ProviderId lobbyId, float intervalSeconds, float othersTimeoutSeconds);

        /// <summary>
        /// Stops sending and listening for heartbeats for this member.
        /// </summary>
        public void StopHeartbeatAndClearSubscriptions();

        /// <summary>
        /// Listens to a particular lobby member's heartbeat and raises OnHeartbeatTimeout if they time out.
        /// </summary>
        /// <param name="lobbyId">The lobby.</param>
        /// <param name="member">The member to listen to.</param>
        public void SubscribeToHeartbeat(ProviderId lobbyId, LobbyMember member);

        /// <summary>
        /// Stops listening for a particular member's heart beat.
        /// </summary>
        /// <param name="lobbyId">The lobby id.</param>
        /// <param name="member">The member to stop listening to.</param>
        public void UnsubscribeFromHeartbeat(ProviderId lobbyId, LobbyMember member);
    }
}
