using System.Collections.Generic;
using System.Linq;

namespace LobbyService.LocalServer
{
    public class LocalBrowserSorterProvider : IBrowserSorterProvider
    {
        private List<(string Key, ILobbySorter Sorter)> _sorters = new();
        
        public void ApplySorters(List<LobbyDescriptor> lobbies)
        {
            var comparer = new CompositeLobbySorterComparer(_sorters.Select(s => s.Sorter));

            lobbies.Sort(comparer);
        }

        public void AddSorter(ILobbySorter sorter, string key)
        {
            _sorters.Add((key, sorter));
        }

        public void RemoveSorter(string key)
        {
            int index = _sorters.FindIndex(s => s.Key == key);
            if (index >= 0) _sorters.RemoveAt(index);
        }

        public void ClearSorters()
        {
            _sorters.Clear();
        }
    }
}