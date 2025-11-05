namespace LobbyService
{
    public interface ILobbyChatView : ILobbyView
    {
        /// <summary>
        /// Called when a message is received. Note, this is also called for messages sent by
        /// the local user.
        /// </summary>
        /// <param name="message"></param>
        public void DisplayMessage(LobbyChatMessage message);
        public void DisplayDirectMessage(LobbyChatMessage message);
    }
}
