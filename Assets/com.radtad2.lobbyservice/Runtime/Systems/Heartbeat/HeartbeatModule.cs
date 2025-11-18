using LobbyService.Heartbeat;

namespace LobbyService
{
    public class HeartbeatModule : IHeartbeatAPI
    {
        private LobbyController _controller;
        private IHeartbeatProvider _heartbeat;
        private IReadonlyLobbyModel _model;

        public HeartbeatModule(LobbyController controller, IHeartbeatProvider heartbeat, IReadonlyLobbyModel model)
        {
            _controller = controller;
            _heartbeat = heartbeat;
            _model = model;

            _heartbeat.OnHeartbeatTimeout += OnTimeout;
        }

        public void Dispose()
        {
            _heartbeat.OnHeartbeatTimeout -= OnTimeout;
        }

        public void StartOwnHeartbeat(float intervalSeconds, float otherTimeoutSeconds)
        {
            if (!_model.InLobby) return;

            _heartbeat.StartOwnHeartbeat(_model.LobbyId, intervalSeconds, otherTimeoutSeconds);
        }

        public void StopOwnHeartbeat()
        {
            _heartbeat.StopOwnHeartbeat();
        }

        public void ClearSubscriptions()
        {
            _heartbeat.ClearSubscriptions();
        }

        public void SubscribeToHeartbeat(LobbyMember member)
        {
            if (!_model.InLobby) return;

            _heartbeat.SubscribeToHeartbeat(_model.LobbyId, member);
        }

        public void UnsubscribeFromHeartbeat(LobbyMember member)
        {
            if (!_model.InLobby) return;

            _heartbeat.UnsubscribeFromHeartbeat(_model.LobbyId, member);
        }

        private void OnTimeout(HeartbeatTimeout timeout)
        {
            if (!_model.InLobby) return;

            if (_controller.IsOwner)
            {
                _controller.KickMember(timeout.Member);
            }
            else if (_model.Owner == timeout.Member)
            {
                _controller.Leave(true);
            }
        }
    }
}
