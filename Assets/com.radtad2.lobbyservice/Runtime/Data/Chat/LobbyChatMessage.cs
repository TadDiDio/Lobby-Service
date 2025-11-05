using JetBrains.Annotations;

namespace LobbyService
{
    public enum LobbyMessageType
    {
        /// <summary>
        /// A message sent from a member to the entire chat.
        /// </summary>
        General,

        /// <summary>
        /// A message sent from a member to just one member.
        /// </summary>
        Direct,

        /// <summary>
        /// A lobby update such as member joining or being promoted to owner.
        /// </summary>
        Update
    }

    /// <summary>
    /// A message in chat.
    /// </summary>
    public struct LobbyChatMessage
    {
        /// <summary>
        /// The message sender.
        /// </summary>
        public LobbyMember Sender;

        /// <summary>
        /// The message type.
        /// </summary>
        public LobbyMessageType Type;

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Content;

        /// <summary>
        /// Optional metadata if needed, i.e. joined, left, kicked, promoted, etc.
        /// </summary>
        /// <remarks>Only valid if Type is Update.</remarks>
        [CanBeNull] public string Meta;
    }
}
