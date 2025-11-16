using System.Collections.Generic;

namespace LobbyService
{
    /// <summary>
    /// A view layer that reacts to browsing events.
    /// </summary>
    public interface IBrowserView : IView
    {
        /// <summary>
        /// Called when browsing for lobbies starts.
        /// </summary>
        public void DisplayStartedBrowsing();

        /// <summary>
        /// Called when the browsing completes.
        /// </summary>
        public void DisplayBrowsingResult(List<LobbyDescriptor> lobbies);

        /// <summary>
        /// Called when a number filter is added.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void DisplayAddedNumberFilter(LobbyNumberFilter filter);

        /// <summary>
        /// Called when a string filter is added.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void DisplayAddedStringFilter(LobbyStringFilter filter);

        /// <summary>
        /// Called when a number filter is removed.
        /// </summary>
        /// <param name="key">The filter key.</param>
        public void DisplayRemovedNumberFilter(string key);

        /// <summary>
        /// Called when a string filter is removed.
        /// </summary>
        /// <param name="key">The filter key.</param>
        public void DisplayRemovedStringFilter(string key);

        /// <summary>
        /// Called when a slots available filter is added.
        /// </summary>
        /// <param name="numAvailable">The number of slots that must be available.</param>
        public void DisplaySetSlotsAvailableFilter(int numAvailable);

        /// <summary>
        /// Called when the slots available filter is cleared.
        /// </summary>
        public void DisplayClearedSlotsAvailableFilter();

        /// <summary>
        /// Called when the limit responses filter is set.
        /// </summary>
        /// <param name="limit">The maximum responses to return.</param>
        public void DisplaySetLimitResponsesFilter(int limit);

        /// <summary>
        /// Called when the slots available filter is cleared.
        /// </summary>
        public void DisplayClearLimitResponsesFilter();

        /// <summary>
        /// Called when a distance filter is added.
        /// </summary>
        /// <param name="filter">The maximum distance to search.</param>
        public void DisplayAddedDistanceFilter(LobbyDistance filter);

        /// <summary>
        /// Called when the distance filter is cleared.
        /// </summary>
        public void DisplayClearedDistanceFilter();

        /// <summary>
        /// Called when all filters are removed.
        /// </summary>
        public void DisplayClearedAllFilters();

        /// <summary>
        /// Called when a sorter is added.
        /// </summary>
        /// <param name="sorter">The sorter.</param>
        /// <param name="key">The key.</param>
        public void DisplayAddedSorter(ILobbySorter sorter, string key);

        /// <summary>
        /// Called when a sorter is removed.
        /// </summary>
        /// <param name="key">The sorter key.</param>
        public void DisplayRemovedSorter(string key);

        /// <summary>
        /// Called when all sorters are removed.
        /// </summary>
        public void DisplayClearedAllSorters();
    }
}
