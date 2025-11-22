using System.Collections.Generic;
using UnityEngine;

namespace LobbyService
{
    public interface IFriendView : IView
    {
        /// <summary>
        /// Called when the friend list is updated.
        /// </summary>
        /// <param name="friends"></param>
        public void DisplayUpdatedFriendList(IReadOnlyList<LobbyMember> friends);

        /// <summary>
        /// Called when an avatar is available for a lobby member or friend.
        /// </summary>
        /// <param name="member">The member whose avatar it is.</param>
        /// <param name="avatar">The avatar.</param>
        public void DisplayFriendAvatar(LobbyMember member, Texture2D avatar);
    }
}
