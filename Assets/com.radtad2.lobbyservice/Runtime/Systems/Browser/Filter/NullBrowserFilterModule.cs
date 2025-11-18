namespace LobbyService
{
    /// <summary>
    /// safely no-ops all operations.
    /// </summary>
    public class NullBrowserFilterModule : IBrowserFilterAPI
    {
        public void AddNumberFilter(LobbyNumberFilter filter) { }
        public void AddStringFilter(LobbyStringFilter filter) { }
        public void RemoveNumberFilter(string key) { }
        public void RemoveStringFilter(string key) { }
        public void SetSlotsAvailableFilter(int slots) { }
        public void ClearSlotsAvailableFilter() { }
        public void SetLimitResponsesFilter(int limit) { }
        public void ClearLimitResponsesFilter() { }
        public void SetDistanceFilter(LobbyDistance filter) { }
        public void ClearDistanceFilter() { }
        public void ClearAllFilters() { }
    }
}