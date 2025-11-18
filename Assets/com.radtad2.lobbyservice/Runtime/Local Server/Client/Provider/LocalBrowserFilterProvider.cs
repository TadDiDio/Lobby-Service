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
            
        public void ApplyFilters()
        {
            throw new System.NotImplementedException();
        }

        public void AddNumberFilter(LobbyNumberFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public void AddStringFilter(LobbyStringFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveNumberFilter(string key)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveStringFilter(string key)
        {
            throw new System.NotImplementedException();
        }

        public void SetSlotsAvailableFilter(int slots)
        {
            throw new System.NotImplementedException();
        }

        public void ClearSlotsAvailableFilter()
        {
            throw new System.NotImplementedException();
        }

        public void SetLimitResponsesFilter(int limit)
        {
            throw new System.NotImplementedException();
        }

        public void ClearLimitResponsesFilter()
        {
            throw new System.NotImplementedException();
        }

        public void SetDistanceFilter(LobbyDistance value)
        {
            throw new System.NotImplementedException();
        }

        public void ClearDistanceFilter()
        {
            throw new System.NotImplementedException();
        }

        public void ClearAllFilters()
        {
            throw new System.NotImplementedException();
        }
    }
}