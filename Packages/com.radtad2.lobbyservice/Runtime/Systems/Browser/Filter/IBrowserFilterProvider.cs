namespace LobbyService
{
    public interface IBrowserFilterProvider
    {
        /// <summary>
        /// The capabilities listing what this browser filter supports.
        /// </summary>
        public BrowserFilterCapabilities FilterCapabilities { get; }

        /// <summary>
        /// Applies all set filters.
        /// </summary>
        public void ApplyFilters();
        
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
        /// Sets a distance filter. Only lobbies within this distance value will be returned.
        /// </summary>
        /// <param name="value">The max distance to allow.</param>
        public void SetDistanceFilter(LobbyDistance value);

        /// <summary>
        /// Clears the distance filter.
        /// </summary>
        public void ClearDistanceFilter();
        
        /// <summary>
        /// Removes all filters.
        /// </summary>
        public void ClearAllFilters();
    }
}