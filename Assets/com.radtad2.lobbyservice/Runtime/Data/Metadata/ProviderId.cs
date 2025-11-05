using System;

namespace LobbyService
{
    /// <summary>
    /// Represents a general id for lobbies and members.
    /// </summary>
    public class ProviderId : IEquatable<ProviderId>
    {
        private readonly string _id;

        public ProviderId(string id) => _id = id ?? throw new ArgumentNullException(nameof(id));

        public override string ToString() => _id;

        public bool Equals(ProviderId other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProviderId)obj);
        }

        public override int GetHashCode()
        {
            return (_id != null ? _id.GetHashCode() : 0);
        }
    }
}
