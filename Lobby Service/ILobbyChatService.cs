namespace LobbyService
{
    public interface ILobbyChatService
    {
        /// <summary>
        /// Broadcasts a chat message to all members.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendChatMessage(string message);

        /// <summary>
        /// Sends a chat message to a single member.
        /// </summary>
        /// <param name="member">The target member.</param>
        /// <param name="message">The message.</param>
        /// <returns>False if the member does not exist in the lobby.</returns>
        public bool SendDirectMessage(LobbyMember member, string message);

        /// <summary>
        /// Invoked when a chat message is receieved.
        /// </summary>
        public event Action<string> ChatMessageReceived;

        /// <summary>
        /// Invoked when a direct message is received.
        /// </summary>
        public event Action<string> DirectMessageReceived;
    }
}
