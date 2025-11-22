namespace LobbyService.Heartbeat
{
    public struct HeartbeatTimeout
    {
        /// <summary>
        /// The lobby id associated with the timed out member.
        /// </summary>
        public ProviderId LobbyId;

        /// <summary>
        /// The member that timed out.
        /// </summary>
        public LobbyMember Member;
    }
}
