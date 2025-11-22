namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all operations.
    /// </summary>
    public class NullBrowserModule : IBrowserAPIInternal
    {
        public NullBrowserModule(IBrowserFilterAPI filter, IBrowserSorterAPI sorter)
        {
            Filter = filter;
            Sorter = sorter;
        }
        
        public IBrowserFilterAPI Filter { get; set; }
        public IBrowserSorterAPI Sorter { get; set; }
        public void Browse() { }
        public void Dispose() { }
    }
}