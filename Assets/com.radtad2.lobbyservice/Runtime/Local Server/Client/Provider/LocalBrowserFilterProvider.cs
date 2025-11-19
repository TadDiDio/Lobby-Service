using System.Collections.Generic;

namespace LobbyService.LocalServer
{
    public class LocalBrowserFilterProvider : IBrowserFilterProvider
    {
        public BrowserFilterCapabilities FilterCapabilities { get; } = new BrowserFilterCapabilities
        {
            SupportsDistanceFiltering = false,
            SupportsResponseLimit =  true,
            SupportsSlotsAvailable = true,
            SupportsStringFiltering = true,
            SupportsNumberFiltering = true
        };

        private Dictionary<string, LobbyNumberFilter> _numberFilters = new();
        private Dictionary<string, LobbyStringFilter> _stringFilters = new();
        
        private int _maxSearchResults;
        private int _slotsAvailableFilter;
        
        public void ApplyFilters()
        {
            foreach (var filter in _numberFilters.Values)
            {
                LocalLobby.ApplyNumberFilter(new ApplyNumberFilterRequest
                {
                    Key = filter.Key,
                    Value = filter.Value,
                    ComparisonType = (int)filter.ComparisonType
                });
            }

            foreach (var filter in _stringFilters.Values)
            {
                LocalLobby.ApplyStringFilter(new ApplyStringFilterRequest
                {
                    Key = filter.Key,
                    Value = filter.Value,
                });
            }
            
            if (_maxSearchResults > 0) LocalLobby.ApplyLimitResponsesFilter(new ApplyLimitResponsesFilterRequest
            {
                Max = _maxSearchResults,
            });
            if (_slotsAvailableFilter > 0) LocalLobby.ApplySlotsAvailableFilter(new ApplySlotsAvailableFilterRequest
            {
                Min = _slotsAvailableFilter
            });
        }

        public void AddNumberFilter(LobbyNumberFilter filter)
        {
            _numberFilters[filter.Key] = filter;
        }

        public void AddStringFilter(LobbyStringFilter filter)
        {
            _stringFilters[filter.Key] = filter;
        }

        public void RemoveNumberFilter(string key)
        {
            _numberFilters.Remove(key);
        }

        public void RemoveStringFilter(string key)
        {
            _stringFilters.Remove(key);
        }

        public void SetSlotsAvailableFilter(int slots)
        {
            _slotsAvailableFilter = slots;
        }

        public void ClearSlotsAvailableFilter()
        {
            _slotsAvailableFilter = 0;
        }

        public void SetLimitResponsesFilter(int limit)
        {
            _maxSearchResults = limit;
        }

        public void ClearLimitResponsesFilter()
        {
            _maxSearchResults = 0;
        }

        public void SetDistanceFilter(LobbyDistance value)
        {
            
        }

        public void ClearDistanceFilter()
        {
            
        }

        public void ClearAllFilters()
        {
            _numberFilters.Clear();
            _stringFilters.Clear();
            _maxSearchResults = 0;
            _slotsAvailableFilter = 0;
        }
    }
}