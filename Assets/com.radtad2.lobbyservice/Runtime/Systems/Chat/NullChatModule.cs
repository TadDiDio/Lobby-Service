using System;

namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all operations.
    /// </summary>
    public class NullChatModule : IChatAPI
    {
        public ChatCapabilities Capabilities { get; } =  new();
        public event Action<LobbyChatMessage> OnChatMessageReceived;
        public event Action<LobbyChatMessage> OnDirectMessageReceived;
        public void SendChatMessage(string message) { }
        public void SendDirectMessage(LobbyMember member, string message) { }
        public void Dispose() { }
    }
}