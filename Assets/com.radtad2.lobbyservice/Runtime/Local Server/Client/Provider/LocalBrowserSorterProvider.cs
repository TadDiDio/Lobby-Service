using System.Collections.Generic;

namespace LobbyService.LocalServer
{
    public class LocalBrowserSorterProvider : IBrowserSorterProvider
    {
        public void ApplySorters(List<LobbyDescriptor> lobbies)
        {
            throw new System.NotImplementedException();
        }

        public void AddSorter(ILobbySorter sorter, string key)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveSorter(string key)
        {
            throw new System.NotImplementedException();
        }

        public void ClearSorters()
        {
            throw new System.NotImplementedException();
        }
    }
}