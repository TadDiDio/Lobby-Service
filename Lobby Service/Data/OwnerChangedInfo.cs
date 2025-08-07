namespace LobbyService
{
    /// <summary>
    /// Information regarding an ownership change.
    /// </summary>
    public struct OwnerChangedInfo
    {
        /// <summary>
        /// The previous owner.
        /// </summary>
        public LobbyMember OldOwner;

        /// <summary>
        /// The new owner.
        /// </summary>
        public LobbyMember NewOwner;

        /// <param name="oldOwner">The previous owner.</param>
        /// <param name="newOwner">The new owner</param>
        public OwnerChangedInfo(LobbyMember oldOwner, LobbyMember newOwner)
        {
            OldOwner = oldOwner;
            NewOwner = newOwner;
        }
    }
}
