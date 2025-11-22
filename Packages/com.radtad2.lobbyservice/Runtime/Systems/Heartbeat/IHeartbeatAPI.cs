using System;

namespace LobbyService
{
    public interface IHeartbeatAPI : IDisposable
    {
        /// <summary>
        /// Starts broadcasting and listening to heartbeat messages for this member.
        /// </summary>
        /// <param name="intervalSeconds">The seconds to wait between heartbeats.</param>
        /// <param name="othersTimeoutSeconds">How long should it take to consider another member to be timed out.</param>
        public void StartOwnHeartbeat(float intervalSeconds, float othersTimeoutSeconds);

        /// <summary>
        /// Stops sending heartbeat updates to others.
        /// </summary>
        public void StopOwnHeartbeat();
        
        /// <summary>
        /// Stops listening to all heartbeats for this member.
        /// </summary>
        public void ClearSubscriptions();

        /// <summary>
        /// Listens to a particular lobby member's heartbeat and raises OnHeartbeatTimeout if they time out.
        /// </summary>
        /// <param name="member">The member to listen to.</param>
        public void SubscribeToHeartbeat(LobbyMember member);

        /// <summary>
        /// Stops listening for a particular member's heart beat.
        /// </summary>
        /// <param name="member">The member to stop listening to.</param>
        public void UnsubscribeFromHeartbeat(LobbyMember member);
    }
}