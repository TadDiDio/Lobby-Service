using System;

namespace LobbyService
{
    public struct ChatCapabilities
    {
        public bool SupportsGeneralMessages;
        public bool SupportsDirectMessages;
    }
    
    /// <summary>
    /// This interface provides a set of services for sending and recieving chat messages.
    /// </summary>
    public interface IChatProvider : IDisposable
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
        /// <param name="lobbyId">The lobby.</param>
        /// <param name="message">The message.</param>
        public void SendChatMessage(ProviderId lobbyId, string message);

        /// <summary>
        /// Sends a chat message to a single member.
        /// </summary>
        /// <param name="lobbyId">The lobby.</param>
        /// <param name="member">The target member.</param>
        /// <param name="message">The message.</param>
        /// <returns>False if the message is not sent.</returns>
        public void SendDirectMessage(ProviderId lobbyId, LobbyMember member, string message);
    }
}
