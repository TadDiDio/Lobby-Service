using System;
using System.Collections.Generic;

namespace LobbyService.LocalServer
{
    public class LocalLobbyMember : IEquatable<LocalLobbyMember>
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }

        
        public LocalLobbyMember(Guid id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public override bool Equals(object obj) => obj is LocalLobbyMember member && Equals(member);
        public bool Equals(LocalLobbyMember other) => other is not null && other.Id.Equals(Id);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(LocalLobbyMember left, LocalLobbyMember right) => EqualityComparer<LocalLobbyMember>.Default.Equals(left, right);
        public static bool operator !=(LocalLobbyMember left, LocalLobbyMember right) => !(left == right);

        public override string ToString() => DisplayName;

        public LocalLobbyMember Copy() => new(Id, DisplayName);

    }
}