namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all operations.
    /// </summary>
    public class NullBrowserModule : IBrowserAPI
    {
        public NullBrowserModule(IBrowserFilterAPI filter, IBrowserSorterAPI sorter)
        {
            Filter = filter;
            Sorter = sorter;
        }
        
        public IBrowserFilterAPI Filter { get; } 
        public IBrowserSorterAPI Sorter { get; }
        public void Browse() { }
        public void Dispose() { }
    }
}