namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all operations.
    /// </summary>
    public class NullHeartbeatModule : IHeartbeatAPI
    {
        public void StartOwnHeartbeat(float intervalSeconds, float othersTimeoutSeconds) { }
        public void StopOwnHeartbeat() { }
        public void ClearSubscriptions() { }
        public void SubscribeToHeartbeat(LobbyMember member) { }
        public void UnsubscribeFromHeartbeat(LobbyMember member) { }
        public void Dispose() { }
    }
}