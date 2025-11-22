using System.Collections.Generic;
using System.Linq;

namespace LobbyService
{
    public class BrowserSorterModule : IBrowserSorterAPI
    {
        private IBrowserView _viewBus;
        private List<(string Key, ILobbySorter Sorter)> _sorters = new();
        
        public BrowserSorterModule(IBrowserView viewBus)
        {
            _viewBus = viewBus;
        }
        
        public void AddSorter(ILobbySorter sorter, string key)
        {
            _sorters.Add((key, sorter));
            _viewBus.DisplayAddedSorter(sorter, key);
        }

        public void RemoveSorter(string key)
        {
            int index = _sorters.FindIndex(s => s.Key == key);
            if (index >= 0) 
            {
                _sorters.RemoveAt(index);
                _viewBus.DisplayRemovedSorter(key);
            }
        }

        public void ClearSorters()
        {
            _sorters.Clear();
            _viewBus.DisplayClearedAllSorters();
        }
                
        public void ApplySorters(List<LobbyDescriptor> lobbies)
        {
            var comparer = new CompositeLobbySorterComparer(_sorters.Select(s => s.Sorter));

            lobbies.Sort(comparer);
        }
    }
}