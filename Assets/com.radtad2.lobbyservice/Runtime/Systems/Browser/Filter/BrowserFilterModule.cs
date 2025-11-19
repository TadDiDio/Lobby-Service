using System;

namespace LobbyService
{
    public class BrowserFilterModule : IBrowserFilterAPI
    {
        private IBrowserView _viewBus;
        private IBrowserFilterProvider _filter;
        
        public BrowserFilterModule(IBrowserView viewBus, IBrowserFilterProvider filter)
        {
            _viewBus = viewBus;
            _filter = filter;
        }
        
        public void AddNumberFilter(LobbyNumberFilter filter)
        {
            _filter.AddNumberFilter(filter);
            _viewBus.DisplayAddedNumberFilter(filter);
        }

        public void AddStringFilter(LobbyStringFilter filter)
        {
            _filter.AddStringFilter(filter);
            _viewBus.DisplayAddedStringFilter(filter);
        }

        public void RemoveNumberFilter(string key)
        {
            _filter.RemoveNumberFilter(key);
            _viewBus.DisplayRemovedNumberFilter(key);
        }

        public void RemoveStringFilter(string key)
        {
            _filter.RemoveStringFilter(key);
            _viewBus.DisplayRemovedStringFilter(key);
        }

        public void AddSlotsAvailableFilter(int slots)
        {
            if (slots <= 0) throw new ArgumentException("Slots must be greater than zero");
            _filter.SetSlotsAvailableFilter(slots);
            _viewBus.DisplaySetSlotsAvailableFilter(slots);
        }

        public void ClearSlotsAvailableFilter()
        {
            _filter.ClearSlotsAvailableFilter();
            _viewBus.DisplayClearedSlotsAvailableFilter();
        }

        public void SetLimitResponsesFilter(int limit)
        {
            if (limit <= 0) throw new ArgumentException("Limit must be greater than zero");
            _filter.SetLimitResponsesFilter(limit);
            _viewBus.DisplaySetLimitResponsesFilter(limit);        
        }

        public void ClearLimitResponsesFilter()
        {
            _filter.ClearLimitResponsesFilter();
            _viewBus.DisplayClearLimitResponsesFilter();
        }

        public void AddDistanceFilter(LobbyDistance filter)
        {
            _filter.SetDistanceFilter(filter);
            _viewBus.DisplayAddedDistanceFilter(filter);
        }

        public void ClearDistanceFilter()
        {
            _filter.ClearDistanceFilter();
            _viewBus.DisplayClearedDistanceFilter();
        }

        public void ClearAllFilters()
        {
            _filter.ClearAllFilters();
            _viewBus.DisplayClearedAllFilters();
        }
    }
}