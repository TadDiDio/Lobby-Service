using System;

namespace LobbyService
{
    public interface IChatAPI : IDisposable
    {
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