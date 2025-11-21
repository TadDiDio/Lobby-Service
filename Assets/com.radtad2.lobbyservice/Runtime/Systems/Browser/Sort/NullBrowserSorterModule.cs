using System.Collections.Generic;

namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all operations.
    /// </summary>
    public class NullBrowserSorterModule : IBrowserSorterAPI
    {
        public void ApplySorters(List<LobbyDescriptor> descriptors) { }
        public void AddSorter(ILobbySorter sorter, string key) { }
        public void RemoveSorter(string key) { }
        public void ClearSorters() { }
    }
}