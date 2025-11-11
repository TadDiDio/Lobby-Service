using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyService.LocalServer
{
    public static class ClientExtensions
    {
        public static LobbyMember ToLobbyMember(this LocalLobbyMember member)
        {
            return new LobbyMember(new ProviderId(member.Id.ToString()), member.DisplayName);
        }

        public static LobbyType ToLobbyType(this LocalLobbyType type)
        {
            return type switch
            {
                LocalLobbyType.Public     => LobbyType.Public,
                LocalLobbyType.InviteOnly => LobbyType.InviteOnly,
                _                         => throw new ArgumentOutOfRangeException()
            };
        }

        public static List<LobbyMember> ToLobbyMembers(this IReadOnlyList<LocalLobbyMember> members)
        {
            return members.Select(ToLobbyMember).ToList();
        }
        
        public static Metadata ToMeta(this IReadOnlyDictionary<string, string> data)
        {
            var meta = new Metadata();

            foreach (var kvp in data)
            {
                meta.Set(kvp.Key, kvp.Value);
            }
            
            return meta;
        }
        public static Dictionary<LobbyMember, Metadata> ToMemberData(this IReadOnlyDictionary<LocalLobbyMember, Dictionary<string, string>> data)
        {
            var result = new Dictionary<LobbyMember, Metadata>();
            
            foreach (var kvp in data)
            {
                var meta = new Metadata();
                
                foreach (var kvp2 in kvp.Value)
                {
                    meta.Set(kvp2.Key, kvp2.Value);
                }
                
                result[kvp.Key.ToLobbyMember()] = meta;
            }
            
            return result;
        }
    }
}