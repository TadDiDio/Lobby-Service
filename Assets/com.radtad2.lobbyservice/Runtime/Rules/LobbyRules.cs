using System;

namespace LobbyService
{
    /// <summary>
    /// The rules that govern how the lobby functions.
    /// </summary>
    [Serializable]
    public class LobbyRules
    {
        /// <summary>
        /// Who is allowed to invite others to the lobby.
        /// </summary>
        public bool OnlyOwnerCanInvite = false;

        /// <summary>
        /// Whether to start polling for friends automatically.
        /// </summary>
        /// <remarks>Requires ILobbyFriendService to be implemented.</remarks>
        public bool AutoStartFriendPolling = true;

        /// <summary>
        /// How should we find friends.
        /// </summary>
        /// <remarks>Requires ILobbyFriendService to be implemented.</remarks>
        public FriendDiscoveryFilter FriendDiscoveryFilter = FriendDiscoveryFilter.Online;

        /// <summary>
        /// How often should we search for new friends. Value of 5s is reasonable.
        /// </summary>
        /// <remarks>Requires ILobbyFriendService to be implemented.</remarks>
        public float FriendPollingRateSeconds = 5f;

        /// <summary>
        /// Whether to allow members to send messages secretly to other recipients.
        /// </summary>
        /// <remarks>Requires ILobbyChatService to be implemented.</remarks>
        public bool AllowDirectChatMessages;

        /// <summary>
        /// Should we detect and disconnect non responsive users?
        /// </summary>
        public bool UseHeartbeatTimeout;

        /// <summary>
        /// How long to wait between sending your heart beat to others.
        /// </summary>
        /// <remarks>Requires ILobbyHeartbeatService to be implemented.</remarks>
        public float HeartbeatIntervalSeconds = 3f;

        /// <summary>
        /// How long to go without receiving a heartbeat from another user before considering them disconnected.
        /// </summary>
        /// <remarks>Requires ILobbyHeartbeatService to be implemented. The owner will kick timed out members
        /// but if its the owner that timed out, all other members will leave.</remarks>
        public float HeartbeatTimeoutSeconds = 15f;

        /// <summary>
        /// Whether to auto start lobbies or not.
        /// </summary>
        public bool AutoStartLobbies = false;

        /// <summary>
        /// If creating a lobby automatically, should we name it after the user?
        /// </summary>
        public bool NameAutoLobbyAfterUser = true;

        /// <summary>
        /// The request to use when automatically starting lobbies.
        /// </summary>
        public CreateLobbyRequest AutoLobbyCreateRequest;

        /// <summary>
        /// The policy to use when creating a lobby fails.
        /// </summary>
        public IEnterFailedPolicy<CreateLobbyRequest> CreateFailedPolicy = new NullEnterFailurePolicies();

        /// <summary>
        /// The policy to use when joining a lobby fails.
        /// </summary>
        public IEnterFailedPolicy<JoinLobbyRequest> JoinFailedPolicy = new NullJoinFailurePolicy();

        /// <summary>
        /// The policy to use when a user attempts to create a lobby while already in one.
        /// </summary>
        public IEnterRequestedWhileInLobbyPolicy<CreateLobbyRequest> CreateWhileInLobbyPolicy = new LeaveThenCreateJoinPolicy();

        /// <summary>
        /// The policy to use when a user attempts to join a lobby while already in one.
        /// </summary>
        public IEnterRequestedWhileInLobbyPolicy<JoinLobbyRequest> JoinWhileInLobbyPolicy = new LeaveThenJoinJoinPolicy();
    }
}
