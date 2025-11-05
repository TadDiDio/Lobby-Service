using System;

namespace LobbyService
{
    public class ChatModule : IDisposable
    {
        private LobbyController _controller;
        private ILobbyChatService _chat;
        private IReadonlyLobbyModel _model;

        public ChatModule( LobbyController controller, ILobbyChatService chat, IReadonlyLobbyModel model)
        {
            _controller = controller;
            _chat = chat;
            _model = model;

            _chat.OnChatMessageReceived += OnMessage;
            _chat.OnDirectMessageReceived += OnDirectMessage;
        }


        public void Dispose()
        {
            _chat.OnChatMessageReceived -= OnMessage;
            _chat.OnDirectMessageReceived -= OnDirectMessage;
        }

        public void SendMessage(string message)
        {
            if (!_model.InLobby) return;

            _chat.SendChatMessage(_model.LobbyId, message);
        }

        public void SendDirectMessage(LobbyMember target, string message)
        {
            if (!_model.InLobby) return;
            if (!_controller.Rules.AllowDirectChatMessages) return;

            _chat.SendDirectMessage(_model.LobbyId, target, message);
        }

        private void OnMessage(LobbyChatMessage message)
        {
            if (!_model.InLobby) return;

            _controller.BroadcastToViews<ILobbyChatView>(v => v.DisplayMessage(message));
        }

        private void OnDirectMessage(LobbyChatMessage message)
        {
            if (!_model.InLobby) return;
            if (!_controller.Rules.AllowDirectChatMessages) return;

            _controller.BroadcastToViews<ILobbyChatView>(v => v.DisplayDirectMessage(message));
        }
    }
}
