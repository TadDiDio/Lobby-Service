namespace LobbyService
{
    public struct InviteSentInfo
    {
        /// <summary>
        /// The member being invited.
        /// </summary>
        public LobbyMember Member;

        /// <summary>
        /// Whether the invitation was sent or failed. Unrelated to whether the invitee accepts it.
        /// </summary>
        public bool InviteSent;
    }

    public struct LobbyInvite
    {
        /// <summary>
        /// The member that sent you the invite.
        /// </summary>
        public LobbyMember Sender;

        /// <summary>
        /// The lobby Id.
        /// </summary>
        public ProviderId LobbyId;
    }
}
