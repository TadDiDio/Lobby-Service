using System.Collections.Generic;

namespace LobbyService
{
    /// <summary>
    /// A reason that joining a lobby failed.
    /// </summary>
    public enum EnterFailedReason
    {
        /// <summary>
        /// There was not a failure.
        /// </summary>
        None,

        /// <summary>
        /// General failure such as timeout.
        /// </summary>
        General,

        /// <summary>
        /// Stale request from a previous provider was caught and cancelled.
        /// </summary>
        StaleRequest,

        /// <summary>
        /// The request was not sent because the ProviderId could not be resolved to a
        /// valid Id for the current provider platform.
        /// </summary>
        InvalidId,

        /// <summary>
        /// The backend for a provider was not initialized.
        /// </summary>
        BackendNotInitialized
    }

    public struct EnterFailedResult<TRequest>
    {
        /// <summary>
        /// The failure reason.
        /// </summary>
        public EnterFailedReason Reason;

        /// <summary>
        /// The failed request.
        /// </summary>
        public TRequest Request;
    }

    /// <summary>
    /// The result of attempting to enter a lobby.
    /// </summary>
    public class EnterLobbyResult
    {
        public static EnterLobbyResult Succeeded
        (
            ProviderId lobbyId,
            LobbyMember owner,
            LobbyMember localMember,
            int capacity,
            LobbyType type,
            List<LobbyMember> members,
            Metadata lobbyMetadata,
            Dictionary<LobbyMember, Metadata> memberData
        )
        {
            return new EnterLobbyResult
            {
                Success = true,
                FailureReason = EnterFailedReason.None,
                LobbyId = lobbyId,
                Owner = owner,
                LobbyData = lobbyMetadata,
                LocalMember = localMember,
                Capacity = capacity,
                Type = type,
                MemberData = memberData,
                Members = members
            };
        }

        public static EnterLobbyResult Failed(EnterFailedReason reason)
        {
            return new EnterLobbyResult { Success = false, FailureReason = reason };
        }

        private EnterLobbyResult() { }

        /// <summary>
        /// Whether the join operation was successful.
        /// </summary>
        public bool Success;

        /// <summary>
        /// A reason for failure.
        /// </summary>
        public EnterFailedReason FailureReason;

        /// <summary>
        /// The lobby id. Only valid if successful.
        /// </summary>
        public ProviderId LobbyId;

        /// <summary>
        /// The current owner of the lobby. Only valid if successful.
        /// </summary>
        public LobbyMember Owner;

        /// <summary>
        /// The local member joining the lobby. Only valid if successful.
        /// </summary>
        public LobbyMember LocalMember;

        /// <summary>
        /// Maximum players allowed in lobby.
        /// </summary>
        public int Capacity;

        /// <summary>
        /// The lobby type.
        /// </summary>
        public LobbyType Type;

        /// <summary>
        /// Gets a list of all members in the lobby including you. Only valid if successful.
        /// </summary>
        public List<LobbyMember> Members;

        /// <summary>
        /// The current lobby data. Only valid if successful.
        /// </summary>
        public Metadata LobbyData;

        /// <summary>
        /// The current member data. Only valid if successful.
        /// </summary>
        public Dictionary<LobbyMember, Metadata> MemberData;
    }
}
