namespace LobbyService
{
    public class BrowserSorterModule : IBrowserSorterAPI
    {
        private IBrowserView _viewBus;
        private IBrowserSorterProvider _sorter;

        public BrowserSorterModule(IBrowserView viewBus, IBrowserSorterProvider sorter)
        {
            _viewBus = viewBus;
            _sorter = sorter;
        }
        
        public void AddSorter(ILobbySorter sorter, string key)
        {
            _sorter.AddSorter(sorter, key);
            _viewBus.DisplayAddedSorter(sorter, key);
        }

        public void RemoveSorter(string key)
        {
            _sorter.RemoveSorter(key);
            _viewBus.DisplayRemovedSorter(key);
        }

        public void ClearSorters()
        {
            _sorter.ClearSorters();
            _viewBus.DisplayClearedAllSorters();
        }
    }
}