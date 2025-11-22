namespace LobbyService
{
    public class ChatModule : IChatAPI
    {
        private IChatView _viewBus;
        private IChatProvider _chat;
        private IReadonlyLobbyModel _model;

        public ChatModule(IChatView viewBus, IChatProvider chat, IReadonlyLobbyModel model)
        {
            _viewBus = viewBus;
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

        public void SendChatMessage(string message)
        {
            if (!_model.InLobby) return;
            _chat.SendChatMessage(_model.LobbyId, message);
        }

        public void SendDirectMessage(LobbyMember target, string message)
        {
            if (!_model.InLobby) return;
            _chat.SendDirectMessage(_model.LobbyId, target, message);
        }

        private void OnMessage(LobbyChatMessage message)
        {
            if (!_model.InLobby) return;
            _viewBus.DisplayMessage(message);
        }

        private void OnDirectMessage(LobbyChatMessage message)
        {
            if (!_model.InLobby) return;
            _viewBus.DisplayDirectMessage(message);
        }
    }
}
