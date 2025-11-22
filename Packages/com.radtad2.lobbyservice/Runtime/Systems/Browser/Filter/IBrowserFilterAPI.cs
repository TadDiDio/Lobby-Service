namespace LobbyService
{
    public interface IBrowserFilterAPI
    {
        /// <summary>
        /// Applies all cached filters.
        /// </summary>
        public void ApplyFilters();
        
        /// <summary>
        /// Stores a number filter for use during ApplyFilters. Only lobbies matching this key-value pair will be returned.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <remarks>It is expected that filters added by this are applied during every ApplyFilters until removed.</remarks>
        public void AddNumberFilter(LobbyNumberFilter filter);

        /// <summary>
        /// Adds a string filter for use during ApplyFilters. Only lobbies matching this key-value pair will be returned.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <remarks>It is expected that filters added by this are applied during every ApplyFilters until removed.</remarks>
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
        /// Sets a number of slots that need to be available for use during ApplyFilters.
        /// </summary>
        /// <param name="slots">The number of slots.</param>
        /// <remarks>It is expected that filters added by this are applied during every ApplyFilters until removed.</remarks>
        public void AddSlotsAvailableFilter(int slots);

        /// <summary>
        /// Clears the available slots filter.
        /// </summary>
        public void ClearSlotsAvailableFilter();

        /// <summary>
        /// Sets a response limiter for use during ApplyFilters.
        /// </summary>
        /// <param name="limit">The maximum number of responses to return.</param>
        /// <remarks>It is expected that filters added by this are applied during every ApplyFilters until removed.</remarks>
        public void SetLimitResponsesFilter(int limit);

        /// <summary>
        /// Clears the response limiter.
        /// </summary>
        public void ClearLimitResponsesFilter();

        /// <summary>
        /// Sets a distance filter for use during ApplyFilters. Only lobbies within this distance value will be returned.
        /// </summary>
        /// <param name="filter">The max distance to allow.</param>
        /// <remarks>It is expected that filters added by this are applied during every ApplyFilters until removed.</remarks>
        public void AddDistanceFilter(LobbyDistance filter);

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