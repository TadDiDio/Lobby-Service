namespace LobbyService
{
    /// <summary>
    /// Information on the member kicked.
    /// </summary>
    public struct KickInfo
    {
        /// <summary>
        /// The reason for being kicked.
        /// </summary>
        public KickReason Reason;
    }

    /// <summary>
    /// Possible reasons for being kicked.
    /// </summary>
    public enum KickReason
    {
        /// <summary>
        /// Kicked by the owner of the lobby.
        /// </summary>
        General,

        /// <summary>
        /// Lobby was closed by the host.
        /// </summary>
        LobbyClosed,

        /// <summary>
        /// The owner stopped responding.
        /// </summary>
        OwnerStoppedResponding
    }
}
