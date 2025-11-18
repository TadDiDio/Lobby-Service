using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService
{
    public interface IBrowserProvider : IDisposable
    {
        /// <summary>
        /// A module providing filtering capabilities.
        /// </summary>
        public IBrowserFilterProvider Filter { get; }

        /// <summary>
        /// A module providing sorting capabilities.
        /// </summary>
        public IBrowserSorterProvider Sorter { get; }
        
        /// <summary>
        /// Searches for lobbies matching the current filters set.
        /// </summary>
        /// <param name="token">A token to cancel the operation.</param>
        /// <returns>A sorted list of discovered lobby ids.</returns>
        public Task<List<LobbyDescriptor>> Browse(CancellationToken token);
    }
}
