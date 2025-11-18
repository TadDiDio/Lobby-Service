using System;

namespace LobbyService.LocalServer
{
    public class LocalChatProvider : IChatProvider
    {
        public ChatCapabilities Capabilities { get; } = new ChatCapabilities
        {
            SupportsGeneralMessages = true,
            SupportsDirectMessages = true
        };
        
        public event Action<LobbyChatMessage> OnChatMessageReceived;
        public event Action<LobbyChatMessage> OnDirectMessageReceived;
        public void SendChatMessage(ProviderId lobbyId, string message)
        {
            throw new NotImplementedException();
        }

        public void SendDirectMessage(ProviderId lobbyId, LobbyMember member, string message)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose() { }
    }
}