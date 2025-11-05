namespace LobbyService
{
    public enum LeaveReason
    {
        UserRequested,
        Kicked,
    }
    public struct LeaveInfo
    {
        /// <summary>
        /// The member that left.
        /// </summary>
        public LobbyMember Member;

        /// <summary>
        /// The reason you left.
        /// </summary>
        public LeaveReason LeaveReason;

        /// <summary>
        /// Information about why you were kicked. Only valid if LeaveReason is Kicked.
        /// </summary>
        public KickInfo? KickInfo;
    }
}
