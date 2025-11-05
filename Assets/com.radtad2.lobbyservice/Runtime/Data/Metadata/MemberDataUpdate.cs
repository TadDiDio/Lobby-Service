namespace LobbyService
{
    public struct MemberDataUpdate
    {
        /// <summary>
        /// The member that was updated.
        /// </summary>
        public LobbyMember Member;

        /// <summary>
        /// The new data.
        /// </summary>
        public Metadata Data;
    }
}
