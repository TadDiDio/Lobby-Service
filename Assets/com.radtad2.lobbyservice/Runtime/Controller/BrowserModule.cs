using System;
using System.Threading.Tasks;

namespace LobbyService
{
    public class BrowserModule : IDisposable
    {
        private LobbyController _controller;
        private ILobbyBrowserService _browser;

        public BrowserModule(LobbyController controller, ILobbyBrowserService browser)
        {
            _controller = controller;
            _browser = browser;
        }

        public bool SupportsCapability(LobbyBrowserCapabilities capabilities) => (_browser.Capabilities & capabilities) == capabilities;

        public async Task Browse()
        {
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayStartedBrowsing());

            var result = await _browser.Browse();

            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayBrowsingResult(result));
        }

        public void AddNumberFilter(LobbyNumberFilter filter)
        {
            _browser.AddNumberFilter(filter);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayAddedNumberFilter(filter));
        }

        public void AddStringFilter(LobbyStringFilter filter)
        {
            _browser.AddStringFilter(filter);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayAddedStringFilter(filter));
        }

        public void RemoveNumberFilter(string key)
        {
            _browser.RemoveNumberFilter(key);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayRemovedNumberFilter(key));
        }

        public void RemoveStringFilter(string key)
        {
            _browser.RemoveStringFilter(key);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayRemovedStringFilter(key));
        }

        public void AddDistanceFilter(LobbyDistance value)
        {
            _browser.SetDistanceFilter(value);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayAddedDistanceFilter(value));
        }
        public void ClearDistanceFilter()
        {
            _browser.ClearDistanceFilter();
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayClearedDistanceFilter());
        }

        public void SetSlotsAvailableFilter(int available)
        {
            _browser.SetSlotsAvailableFilter(available);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplaySetSlotsAvailableFilter(available));
        }

        public void ClearSlotsAvailableFilter()
        {
            _browser.ClearSlotsAvailableFilter();
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayClearedSlotsAvailableFilter());
        }

        public void SetLimitResponsesFilter(int limit)
        {
            _browser.SetLimitResponsesFilter(limit);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplaySetLimitResponsesFilter(limit));
        }

        public void ClearLimitResponsesFilter()
        {
            _browser.ClearLimitResponsesFilter();
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayClearLimitResponsesFilter());
        }

        public void ClearAllFilters()
        {
            _browser.ClearAllFilters();
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayClearedAllFilters());
        }

        public void AddSorter(LobbyKeyAndSorter keyAndSorter)
        {
            _browser.AddSorter(keyAndSorter);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayAddedSorter(keyAndSorter));
        }

        public void RemoveSorter(string key)
        {
            _browser.RemoveSorter(key);
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayRemovedSorter(key));
        }
        public void ClearSorters()
        {
            _browser.ClearSorters();
            _controller.BroadcastToViews<ILobbyBrowserView>(v => v.DisplayClearedAllSorters());
        }
        public void Dispose() { }
    }
}
