using System;
using System.Collections.Generic;

namespace LobbyService
{
    public class LobbyMember : IEquatable<LobbyMember>
    {
        public static LobbyMember Unknown { get; } = new(new ProviderId(Guid.NewGuid().ToString()), "Unknown");
        
        public ProviderId Id { get; }
        public string DisplayName { get; }

        /// <param name="memberId">The id representing this member.</param>
        /// <param name="displayName">The display name for this member.</param>
        public LobbyMember(ProviderId memberId, string displayName)
        {
            Id = memberId ?? throw new ArgumentNullException(nameof(memberId), "Member id cannot be null.");
            DisplayName = displayName;
        }

        public override bool Equals(object obj) => obj is LobbyMember member && Equals(member);
        public bool Equals(LobbyMember other) => other is not null && other.Id.Equals(Id);
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;
        public static bool operator ==(LobbyMember left, LobbyMember right) => EqualityComparer<LobbyMember>.Default.Equals(left, right);
        public static bool operator !=(LobbyMember left, LobbyMember right) => !(left == right);
        public override string ToString() => DisplayName;
    }
}
