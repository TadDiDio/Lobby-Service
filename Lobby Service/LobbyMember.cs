namespace LobbyService
{
    public class LobbyMember
    {
        private string _id;
        private string _displayName;

        /// <param name="memberId">The id representing this member.</param>
        /// <param name="displayName">The display name for this member.</param>
        public LobbyMember(string memberId, string displayName)
        {
            _id = memberId;
            _displayName = displayName;
        }
    }
}
