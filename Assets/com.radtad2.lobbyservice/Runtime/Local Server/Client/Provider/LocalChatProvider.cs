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

        public LocalChatProvider()
        {
            LocalLobby.OnChat += OnChat;
        }
        
        public event Action<LobbyChatMessage> OnChatMessageReceived;
        public event Action<LobbyChatMessage> OnDirectMessageReceived;
        public void SendChatMessage(ProviderId lobbyId, string message)
        {
            LocalLobby.SendChatMessage(new ChatMessageRequest
            {
                LobbyId = lobbyId.ToString(),
                Message = message
            });
        }

        public void SendDirectMessage(ProviderId lobbyId, LobbyMember member, string message)
        {
            LocalLobby.SendDirectMessage(new DirectMessageRequest
            {
                LobbyId = lobbyId.ToString(),
                TargetId = member.Id.ToString(),
                Message = message
            });
        }

        private void OnChat(LobbyChatMessage message)
        {
            if (message.Direct) OnDirectMessageReceived?.Invoke(message);
            else OnChatMessageReceived?.Invoke(message);
        }
        
        public void Dispose()
        {
            LocalLobby.OnChat -= OnChat;
        }
    }
}