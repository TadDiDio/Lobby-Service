namespace LobbyService
{
    /// <summary>
    /// Information on the member kicked.
    /// </summary>
    public struct KickInfo
    {
        /// <summary>
        /// The member that was kicked.
        /// </summary>
        public LobbyMember Member;

        /// <summary>
        /// The reason for being kicked.
        /// </summary>
        public KickReason Reason;

        /// <param name="member">The member that was kicked.</param>
        /// <param name="reason">The reason for being kicked.</param>
        public KickInfo(LobbyMember member, KickReason reason)
        {
            Member = member;
            Reason = reason;
        }
    }
}
