using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService
{
    public interface IBrowserProvider
    {
        /// <summary>
        /// Searches for lobbies matching the current filters set.
        /// </summary>
        /// <param name="token">A token to cancel the operation.</param>
        /// <returns>A sorted list of discovered lobby ids.</returns>
        public Task<List<LobbyDescriptor>> Browse(CancellationToken token);
    }
}
