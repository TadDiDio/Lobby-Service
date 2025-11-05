using System.Collections.Generic;

namespace LobbyService
{
    public interface ILobbyFriendView : ILobbyView
    {
        /// <summary>
        /// Called when the friend list is updated.
        /// </summary>
        /// <param name="friends"></param>
        public void DisplayUpdatedFriendList(IReadOnlyList<LobbyMember> friends);
    }
}
