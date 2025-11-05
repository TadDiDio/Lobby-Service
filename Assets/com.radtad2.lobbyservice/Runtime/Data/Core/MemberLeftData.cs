namespace LobbyService
{
    public struct MemberLeftData
    {
        /// <summary>
        /// The id of the lobby that generated this. Useful for filtering old messages from stale lobbies if
        /// compared against the local cache.
        /// </summary>
        public ProviderId LobbyId;

        /// <summary>
        /// The member that left.
        /// </summary>
        public LobbyMember Member;
    }
}
