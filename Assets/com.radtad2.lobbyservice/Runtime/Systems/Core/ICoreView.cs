namespace LobbyService
{
    public interface ICoreView : IView
    {
        /// <summary>
        /// Called to seed a new view with existing lobby data if there is any.
        /// </summary>
        /// <param name="snapshot">A readonly copy of the current state.</param>
        /// <remarks>Only called as a direct response to LobbyController.Connect(ILobbyView) so that
        /// a new view can initialize. The view should use DisplayXXX methods to stay up to date afterward.</remarks>
        public void DisplayExistingLobby(IReadonlyLobbyModel snapshot);

        /// <summary>
        /// Called when a lobby creation request has been issued.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        public void DisplayCreateRequested(CreateLobbyRequest request);

        /// <summary>
        /// Called when the result of creating a lobby is available.
        /// </summary>
        /// <param name="result">The result.</param>
        public void DisplayCreateResult(EnterLobbyResult result);

        /// <summary>
        /// Called any time a user attempts to join a lobby whether by browsing or invitation.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        public void DisplayJoinRequested(JoinLobbyRequest request);

        /// <summary>
        /// Called when the result of joining a lobby is available.
        /// </summary>
        /// <param name="result">The result.</param>
        public void DisplayJoinResult(EnterLobbyResult result);

        /// <summary>
        /// Called when the local member leaves the lobby for any reason such as user intent or being kicked.
        /// </summary>
        public void DisplayLocalMemberLeft(LeaveInfo info);

        /// <summary>
        /// Called when an invite is issued.
        /// </summary>
        /// <param name="info">Info about the invite.</param>
        public void DisplaySendInvite(InviteSentInfo info);

        /// <summary>
        /// Called when you receive an invite.
        /// </summary>
        /// <param name="invite">The invite.</param>
        public void DisplayReceivedInvite(LobbyInvite invite);

        /// <summary>
        /// Called when a member other than yourself joins the lobby.
        /// </summary>
        /// <param name="info">The member and their associated metadata.</param>
        public void DisplayOtherMemberJoined(MemberJoinedInfo info);

        /// <summary>
        /// Called when a member other than yourself leaves for any reason such as intent or being kicked.
        /// </summary>
        /// <param name="info">Info about who left and why.</param>
        public void DisplayOtherMemberLeft(LeaveInfo info);

        /// <summary>
        /// Called when the owner has updated.
        /// </summary>
        /// <param name="newOwner">The new owner.</param>
        public void DisplayUpdateOwner(LobbyMember newOwner);

        /// <summary>
        /// Called when lobby data updates.
        /// </summary>
        /// <param name="update">The update information.</param>
        public void DisplayUpdateLobbyData(LobbyDataUpdate update);

        /// <summary>
        /// Called when a member's metadata is updated.
        /// </summary>
        /// <param name="update">The update information.</param>
        public void DisplayUpdateMemberData(MemberDataUpdate update);
    }
}
