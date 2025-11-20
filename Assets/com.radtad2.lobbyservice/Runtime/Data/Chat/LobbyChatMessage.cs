namespace LobbyService
{
    public enum LobbyMessageType
    {
        /// <summary>
        /// A message sent from a member to the entire chat.
        /// </summary>
        General = 0,

        /// <summary>
        /// A message sent from a member to just one member.
        /// </summary>
        Direct = 1,
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
        /// The content of the message.
        /// </summary>
        public string Content;

        /// <summary>
        /// Tells if the message was direct or general.
        /// </summary>
        public bool Direct;
    }
}
