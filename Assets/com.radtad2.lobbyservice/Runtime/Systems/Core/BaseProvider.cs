using System;
using System.Threading.Tasks;

namespace LobbyService
{
    /// <summary>
    /// This interface provides a core set of services for the lobby lifecycle. All events represent
    /// externally driven updates while methods represent user driven requests.
    /// </summary>
    public abstract class BaseProvider : IDisposable
    {
        private bool _obsolete;

        /// <summary>
        /// Tells if this provider is obsolete. This occurs when a provider is swapped.
        /// </summary>
        public bool IsObsolete() => _obsolete;

        /// <summary>
        /// Marks this provider as obsolete.
        /// </summary>
        public void MarkObsolete() => _obsolete = true;

        /// <summary>
        /// Whether this provider should automatically attempt to leave a stale lobby if one exists when being created.
        /// </summary>
        public virtual bool ShouldFlushStaleLobbies() => true;

        #region Extra Modules

        public abstract IHeartbeatProvider Heartbeat { get; }
        public abstract IBrowserProvider Browser { get; }
        public abstract IFriendProvider  Friends { get; }
        public abstract IChatProvider Chat { get; }
        public abstract IProcedureProvider Procedures { get; }
        
        #endregion
        
        #region Events
        /// <summary>
        /// Invoked when a member other than the local client joins.
        /// </summary>
        public abstract event Action<MemberJoinedInfo> OnOtherMemberJoined;

        /// <summary>
        /// Invoked when a member other than the local client leaves.
        /// </summary>
        public abstract event Action<LeaveInfo> OnOtherMemberLeft;

        /// <summary>
        /// Invoked when you are kicked from the lobby.
        /// </summary>
        public abstract event Action<KickInfo> OnLocalMemberKicked;

        /// <summary>
        /// Invoked when an invitation is received.
        /// </summary>
        public abstract event Action<LobbyInvite> OnReceivedInvitation;

        /// <summary>
        /// Invoked when the lobby's data updates.
        /// </summary>
        public abstract event Action<LobbyDataUpdate> OnLobbyDataUpdated;

        /// <summary>
        /// Invoked when any member's data updates, including the local member.
        /// </summary>
        public abstract event Action<MemberDataUpdate> OnMemberDataUpdated;

        /// <summary>
        /// Invoked when the owner changes.
        /// </summary>
        public abstract event Action<LobbyMember> OnOwnerUpdated;
        #endregion

        #region User Initiated
        /// <summary>
        /// Initializes the lobby provider.
        /// </summary>
        public abstract void Initialize(LobbyController controller);

        /// <summary>
        /// Gets the local user whether or not they are in a lobby.
        /// </summary>
        /// <returns>The local user.</returns>
        public abstract LobbyMember GetLocalUser();

        /// <summary>
        /// Creates a lobby and joins the local client to it.
        /// </summary>
        /// <param name="request">The request details.</param>
        /// <returns>Information regarding the success of the operation.</returns>
        public abstract Task<EnterLobbyResult> CreateAsync(CreateLobbyRequest request);

        /// <summary>
        /// Joins a created lobby.
        /// </summary>
        /// <param name="request">The request details.</param>
        /// <returns>Information regarding the success of the operation.</returns>
        public abstract Task<EnterLobbyResult> JoinAsync(JoinLobbyRequest request);

        /// <summary>
        /// Invites another member to the lobby.
        /// </summary>
        /// <param name="member">The member to invite.</param>
        /// <param name="lobbyId">The current lobbyId, or null if there is none.</param>
        /// <returns>True if the invite was sent.</returns>
        public abstract bool SendInvite(ProviderId lobbyId, LobbyMember member);

        /// <summary>
        /// Leaves a lobby.
        /// <param name="lobbyId">The current lobbyId, or null if there is none.</param>
        /// </summary>
        public abstract void Leave(ProviderId lobbyId);

        /// <summary>
        /// Closes the lobby for all members and leaves. Only the owner can do this.
        /// <param name="lobbyId">The current lobbyId, or null if there is none.</param>
        /// </summary>
        /// <returns>True if successful. Can fail if you are not the owner.</returns>
        public abstract bool Close(ProviderId lobbyId);

        /// <summary>
        /// Promotes another member to owner. Only the owner can do this.
        /// </summary>
        /// <param name="lobbyId">The lobby.</param>
        /// <param name="newOwner">The new owner.</param>
        ///<returns>True if the action was successful.</returns>
        public abstract bool SetOwner(ProviderId lobbyId, LobbyMember newOwner);

        /// <summary>
        /// Kicks the user from the lobby. Only the owner can do this.
        /// </summary>
        /// <param name="lobbyId">The lobby.</param>
        /// <param name="member">The member to kick and ban.</param>
        /// <returns>True if you have permission to kick the user.</returns>
        /// <remarks>Note that this method is best effort since a kicked client may not be responding.
        /// Everyone should be made aware of the intent and update local caches but the user may or may
        /// not be removed from the backend provider until TTL expires. This is why you should use the local
        /// cache for gameplay decisions (i.e. send start game messages) as it represents intent rather than reality.</remarks>
        public abstract bool KickMember(ProviderId lobbyId, LobbyMember member);

        /// <summary>
        /// Sets lobby data. Only allowed if you are the owner.
        /// </summary>
        /// <param name="lobbyId">The lobby id.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value.</param>
        /// <returns>Whether setting the data was successful, fails if you are not the owner.</returns>
        public abstract bool SetLobbyData(ProviderId lobbyId, string key, string value);

        /// <summary>
        /// Sets member data.
        /// </summary>
        /// <param name="lobbyId">The lobby id.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value.</param>
        public abstract void SetLocalMemberData(ProviderId lobbyId, string key, string value);

        /// <summary>
        /// Gets data from a lobby.
        /// </summary>
        /// <param name="lobbyId">The lobby.</param>
        /// <param name="key">The key to get for.</param>
        /// <param name="defaultValue">The value to return if there is no data set for this key.</param>
        /// <returns>The value at the key or defaultValue if the key doesn't exist.</returns>
        public abstract string GetLobbyData(ProviderId lobbyId, string key, string defaultValue);

        /// <summary>
        /// Gets data from a member.
        /// </summary>
        /// <param name="lobbyId">The lobby the member is in.</param>
        /// <param name="member">The member to get for.</param>
        /// <param name="key">The key to get for.</param>
        /// <param name="defaultValue">The value to return if there is no data set for this key.</param>
        /// <returns>The value at the key or defaultValue if the key doesn't exist.</returns>
        public abstract string GetMemberData(ProviderId lobbyId, LobbyMember member, string key, string defaultValue);
        #endregion

        public abstract void Dispose();
    }
}
