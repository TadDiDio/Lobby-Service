using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class LocalBrowserProvider : IBrowserProvider
    {
        public IBrowserFilterProvider Filter { get; } = new LocalBrowserFilterProvider();
        public async Task<List<LobbyDescriptor>> Browse(CancellationToken token)
        {
            LocalProvider.EnsureInitialized();
            
            var response = await LocalLobby.Browse(token: token);
            
            if (response.Error is not Error.Ok) return new List<LobbyDescriptor>();

            var result = response.Response.Snapshots.Select(snapshot => new LobbyDescriptor
            {
                IsJoinable = true,
                LobbyId = new ProviderId(snapshot.LobbyId.ToString()),
                Capacity = snapshot.Capacity,
                MemberCount = snapshot.Members.Count,
                Name = snapshot.LobbyData.GetValueOrDefault(LobbyKeys.NameKey, "unnamed")
            }).ToList();

            return result;
        }
        public void Dispose() { }
    }
}