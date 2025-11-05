using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LobbyService
{
    [Flags]
    public enum LobbyBrowserCapabilities
    {
        None = 0,

        /// <summary>
        /// Generic string filter.
        /// </summary>
        StringFilter = 1 << 0,

        /// <summary>
        /// Generic number filter.
        /// </summary>
        NumberFilter = 1 << 1,

        /// <summary>
        /// How close the hosts need to be.
        /// </summary>
        DistanceFilter = 1 << 2,

        /// <summary>
        /// How many slots must be available in the lobby.
        /// </summary>
        SlotsAvailableFilter = 1 << 3,

        /// <summary>
        /// Limits the number of responses returned by a Browse call.
        /// </summary>
        LimitResponseCountFilter = 1 << 4,

        /// <summary>
        /// Sorts the returned results.
        /// </summary>
        Sorting = 1 << 5
    }

    public interface ILobbyBrowserService
    {
        /// <summary>
        /// A list of capabilities.
        /// </summary>
        public LobbyBrowserCapabilities Capabilities { get; }

        /// <summary>
        /// Searches for lobbies matching the current filters set.
        /// </summary>
        /// <returns>A sorted list of discovered lobby ids.</returns>
        public Task<List<LobbyDescriptor>> Browse();

        /// <summary>
        /// Adds a number filter. Only lobbies matching this key-value pair will be returned.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void AddNumberFilter(LobbyNumberFilter filter);

        /// <summary>
        /// Adds a string filter. Only lobbies matching this key-value pair will be returned.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void AddStringFilter(LobbyStringFilter filter);

        /// <summary>
        /// Removes a number filter by the given key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public void RemoveNumberFilter(string key);

        /// <summary>
        /// Removes a string filter by the given key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public void RemoveStringFilter(string key);

        /// <summary>
        /// Sets a distance filter. Only lobbies within this distance value will be returned.
        /// </summary>
        /// <param name="value">The max distance to allow.</param>
        public void SetDistanceFilter(LobbyDistance value);

        /// <summary>
        /// Clears the distance filter.
        /// </summary>
        public void ClearDistanceFilter();

        /// <summary>
        /// Sets a number of slots that need to be available.
        /// </summary>
        /// <param name="slots">The number of slots.</param>
        public void SetSlotsAvailableFilter(int slots);

        /// <summary>
        /// Clears the available slots filter.
        /// </summary>
        public void ClearSlotsAvailableFilter();

        /// <summary>
        /// Sets a response limiter.
        /// </summary>
        /// <param name="limit">The maximum number of responses to return.</param>
        public void SetLimitResponsesFilter(int limit);

        /// <summary>
        /// Clears the response limiter.
        /// </summary>
        public void ClearLimitResponsesFilter();

        /// <summary>
        /// Removes all filters.
        /// </summary>
        public void ClearAllFilters();

        /// <summary>
        /// Adds a sorter.
        /// </summary>
        /// <param name="sorter">The key and sorter.</param>
        /// <remarks>Sorters are applied in the order they are added.</remarks>
        public void AddSorter(LobbyKeyAndSorter sorter);

        /// <summary>
        /// Removes a sorter.
        /// </summary>
        /// <param name="key">The sorter to remove.</param>
        public void RemoveSorter(string key);

        /// <summary>
        /// Removes all sorters.
        /// </summary>
        public void ClearSorters();
    }
}
