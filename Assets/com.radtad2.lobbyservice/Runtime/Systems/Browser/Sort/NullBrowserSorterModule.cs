namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all operations.
    /// </summary>
    public class NullBrowserSorterModule : IBrowserSorterAPI
    {
        public void AddSorter(ILobbySorter sorter, string key) { }
        public void RemoveSorter(string key) { }
        public void ClearSorters() { }
        public void Dispose() { }
    }
}