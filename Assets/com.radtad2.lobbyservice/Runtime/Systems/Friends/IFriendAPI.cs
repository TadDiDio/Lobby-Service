namespace LobbyService
{
    public interface IFriendAPI
    {
        /// <summary>
        /// Starts polling for friends on an interval.
        /// </summary>
        /// <returns>List of friends as lobby members - they may or may not actually be in a lobby.</returns>
        public void StartPolling(FriendDiscoveryFilter filter, float intervalSeconds);

        /// <summary>
        /// Sets the interval at which the polling happens.
        /// </summary>
        /// <param name="intervalSeconds">The seconds to wait between polling.</param>
        public void SetInterval(float intervalSeconds);

        /// <summary>
        /// Sets the filter to use when finding friends.
        /// </summary>
        /// <param name="filter"></param>
        public void SetFilter(FriendDiscoveryFilter filter);

        /// <summary>
        /// Stops polling for friends.
        /// </summary>
        public void StopPolling();

        /// <summary>
        /// Gets the avatar associated with a lobby member.
        /// </summary>
        /// <param name="member">The member to get for.</param>
        public void RequestAvatar(LobbyMember member);
    }
}