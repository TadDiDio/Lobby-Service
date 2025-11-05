using System;

namespace LobbyService
{
    /// <summary>
    /// The various possible types of lobby.
    /// </summary>
    [Serializable]
    public enum LobbyType
    {
        /// <summary>
        /// Anyone can join.
        /// </summary>
        Public,

        /// <summary>
        /// Only invited clients can join.
        /// </summary>
        InviteOnly,
    }
}
