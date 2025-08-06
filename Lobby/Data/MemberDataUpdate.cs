namespace LobbyService
{
    /// <summary>
    /// Information regarding a member data update.
    /// </summary>
    public struct MemberDataUpdate
    {
        /// <summary>
        /// The member that was updated.
        /// </summary>
        public LobbyMember Member;

        /// <summary>
        /// The key that was updated.
        /// </summary>
        public string KeyUpdated;

        /// <summary>
        /// The new value.
        /// </summary>
        public string NewValue;

        /// <param name="member">The member that was updated.</param>
        /// <param name="key">The key that was updated.</param>
        /// <param name="newValue">The new value.</param>
        public MemberDataUpdate(LobbyMember member, string key, string newValue)
        {
            Member = member;
            KeyUpdated = key;
            NewValue = newValue;
        }
    }
}
