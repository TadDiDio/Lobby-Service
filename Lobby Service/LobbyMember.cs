namespace LobbyService
{
    public class LobbyMember : IEquatable<LobbyMember>
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

        public override bool Equals(object? obj)
        {
            if (obj is not LobbyMember other) return false;

            return other._id == _id;
        }

        public bool Equals(LobbyMember? other)
        {
            if (other is null) return false;
            return other._id == _id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool operator ==(LobbyMember? left, LobbyMember? right) =>
            EqualityComparer<LobbyMember>.Default.Equals(left, right);

        public static bool operator !=(LobbyMember? left, LobbyMember? right) =>
            !(left == right);
    }
}
