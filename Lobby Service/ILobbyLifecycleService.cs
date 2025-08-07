namespace LobbyService
{
    public interface ILobbyLifecycleService
    {
        /// <summary>
        /// Creates a lobby and joins the local client to it.
        /// </summary>
        /// <returns>Information regarding the success of the operation.</returns>
        public Task<CreateLobbyResult> CreateLobbyAsync();

        /// <summary>
        /// Joins a created lobby.
        /// </summary>
        /// <returns>Information regarding the success of the operation.</returns>
        public Task<JoinLobbyResult> JoinLobbyAsync();

        /// <summary>
        /// Cancel joining a lobby.
        /// </summary>
        public Task CancelJoiningLobbyAsync();

        /// <summary>
        /// Leaves a lobby.
        /// </summary>
        public Task LeaveLobbyAsync();

        /// <summary>
        /// Closes the lobby for all members. Only the owner can do this.
        /// </summary>
        public Task CloseLobbyAsync();

        /// <summary>
        /// Invites another member to the lobby.
        /// </summary>
        /// <param name="memberId">The id of the member to invite.</param>
        public void Invite(string memberId);

        /// <summary>
        /// Promotes another member to owner. Only the owner can do this.
        /// </summary>
        /// <param name="newOwner">The new owner.</param>
        public void ChangeOwner(LobbyMember newOwner);

        /// <summary>
        /// Kicks a member from the lobby. Only the owner can do this.
        /// </summary>
        /// <param name="member">The member to kick.</param>
        public void KickMember(LobbyMember member);

        /// <summary>
        /// Kicks and bans the memeber from joining this lobby again. Only the owner can do this.
        /// </summary>
        /// <param name="member">The member to kick and ban.</param>
        /// <param name="minutes">The time that the ban lasts. Infinite if 0.</param>
        public void KickAndBanMember(LobbyMember member, int minutes = 0);

        /// <summary>
        /// Invoked when a the local client creates a lobby.
        /// </summary>
        public event Action CreatedLobby;

        /// <summary>
        /// Invoked when the local client joins a lobby.
        /// </summary>
        public event Action JoinedLobby;

        /// <summary>
        /// Invoked when the local client cancels joining a lobby.
        /// </summary>
        public event Action CancelledJoiningLobby;

        /// <summary>
        /// Invoked when the local client leaves a lobby.
        /// </summary>
        public event Action LeftLobby;

        /// <summary>
        /// Invoked when the local client was kicked from a lobby,
        /// </summary>
        public event Action KickedFromLobby;

        /// <summary>
        /// Invoked when the lobby is closed for all members.
        /// </summary>
        public event Action LobbyClosed;

        /// <summary>
        /// Invoked when a client other than the local one joins the lobby.
        /// </summary>
        public event Action<LobbyMember> MemberJoined;

        /// <summary>
        /// Invoked when a client other than the local one leaves the lobby.
        /// </summary>
        public event Action<LobbyMember> MemberLeft;

        /// <summary>
        /// Invoked when a client other than the local one is kicked.
        /// </summary>
        public event Action<LobbyMember> MemberKicked;

        /// <summary>
        /// Invoked for all members when the lobby owner changes.
        /// </summary>
        public event Action<OwnerChangedInfo> OwnerChanged;
    }
}
