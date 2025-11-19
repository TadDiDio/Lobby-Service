using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;

namespace LobbyService.Example.Steam
{
    public class SteamBrowserProvider : IBrowserProvider
    {
        public IBrowserFilterProvider Filter { get; } = new SteamBrowserFilterProvider();
        public IBrowserSorterProvider Sorter { get; } = new SteamBrowserSorterProvider();
        
        public async Task<List<LobbyDescriptor>> Browse(CancellationToken token)
        {
            if (!SteamProvider.EnsureInitialized()) return new List<LobbyDescriptor>();
            
            Filter.ApplyFilters();

            var tcs = new TaskCompletionSource<uint>();
            using var callResult = CallResult<LobbyMatchList_t>.Create();
            var handle = SteamMatchmaking.RequestLobbyList();

            callResult.Set(handle, (result, error) =>
            {
                tcs.TrySetResult(error ? 0 : result.m_nLobbiesMatching);
            });

            var numLobbies = await tcs.Task;
            var result = new List<LobbyDescriptor>();

            for (int i = 0; i < numLobbies; i++)
            {
                var steamId = SteamMatchmaking.GetLobbyByIndex(i);

                result.Add(new LobbyDescriptor
                {
                    LobbyId = new ProviderId(steamId.ToString()),
                    Name = SteamMatchmaking.GetLobbyData(steamId, SteamLobbyKeys.Name),
                    MemberCount = SteamMatchmaking.GetNumLobbyMembers(steamId),
                    MaxMembers = SteamMatchmaking.GetLobbyMemberLimit(steamId),

                    // Steam only returns lobbies that are public or invisible, and also joinable.
                    IsJoinable = true
                });
            }

            Sorter.ApplySorters(result);

            return result;
        }
        
        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}