namespace LobbyService
{
    public struct MemberJoinedInfo
    {
        /// <summary>
        /// The member that joined.
        /// </summary>
        public LobbyMember Member;

        /// <summary>
        /// The data associated with this player.
        /// </summary>
        public Metadata Data;
    }
}
