using System;

namespace LobbyService
{
    public class LobbyDescriptor
    {
        /// <summary>
        /// This id.
        /// </summary>
        public ProviderId LobbyId;

        /// <summary>
        /// The name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// How many members are currently in the lobby.
        /// </summary>
        public int MemberCount;

        /// <summary>
        /// The max allowed members.
        /// </summary>
        public int MaxMembers;

        /// <summary>
        /// True if this lobby is joinable.
        /// </summary>
        public bool IsJoinable;

        public override string ToString()
        {
            return $"Id: {LobbyId}{Environment.NewLine}" +
                   $"Name: {Name}{Environment.NewLine}" +
                   $"Member Count: {MemberCount}{Environment.NewLine}" +
                   $"Capacity: {MaxMembers}{Environment.NewLine}" +
                   $"IsJoinable: {IsJoinable}";
        }
    }
}
