using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    public class BrowserModule : IBrowserAPIInternal
    {
        private IBrowserView _viewBus;
        private IBrowserProvider _browser;

        private CancellationTokenSource _tokenSource;
        
        public BrowserModule(IBrowserView viewBus, IBrowserProvider browser, IBrowserFilterAPI filter, IBrowserSorterAPI sorter)
        {
            _viewBus = viewBus;
            _browser = browser;

            Filter = filter;
            Sorter = sorter;
            
            _tokenSource = new CancellationTokenSource();
        }

        public IBrowserFilterAPI Filter { get; set; }
        public IBrowserSorterAPI Sorter { get; set; }

        public void Browse() => _ = BrowseAsync();

        private async Task BrowseAsync()
        {
            try
            {
                _viewBus.DisplayStartedBrowsing();
                var result = await _browser.Browse(_tokenSource.Token);
                _viewBus.DisplayBrowsingResult(result);
            }
            catch (OperationCanceledException)
            {
                _viewBus.DisplayBrowsingResult(new List<LobbyDescriptor>());   
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _viewBus.DisplayBrowsingResult(new List<LobbyDescriptor>());
            }
        }

        public void Dispose()
        {
            _tokenSource?.Cancel();
        }
    }
}
