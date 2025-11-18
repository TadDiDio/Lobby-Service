using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class LocalBrowserProvider : IBrowserProvider
    {
        public IBrowserFilterProvider Filter { get; } = new LocalBrowserFilterProvider();
        public IBrowserSorterProvider Sorter { get; } = new LocalBrowserSorterProvider();
        public Task<List<LobbyDescriptor>> Browse(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
        public void Dispose() { }
    }
}