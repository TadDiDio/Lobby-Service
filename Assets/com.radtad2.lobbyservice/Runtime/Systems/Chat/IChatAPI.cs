using System;

namespace LobbyService
{
    public interface IChatAPI : IDisposable
    {
        /// <summary>
        /// Lists capabilities for this chat module.
        /// </summary>
        public ChatCapabilities Capabilities { get; }
        
        /// <summary>
        /// Invoked when a chat message is received.
        /// </summary>
        public event Action<LobbyChatMessage> OnChatMessageReceived;

        /// <summary>
        /// Invoked when a direct message is received.
        /// </summary>
        public event Action<LobbyChatMessage> OnDirectMessageReceived;

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
        /// <returns>False if the message is not sent.</returns>
        public void SendDirectMessage(LobbyMember member, string message);
    }
}