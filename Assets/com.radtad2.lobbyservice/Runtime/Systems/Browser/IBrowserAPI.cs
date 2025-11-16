namespace LobbyService
{
    public interface IBrowserAPIInternal : IBrowserAPI
    {
        public new IBrowserFilterAPI Filter { get; set; }
        public new IBrowserSorterAPI Sorter { get; set; }  
    }
    
    public interface IBrowserAPI
    {
        /// <summary>
        /// Filtering functionality.
        /// </summary>
        public IBrowserFilterAPI Filter { get; }
        
        /// <summary>
        /// Sorting functionality.
        /// </summary>
        public IBrowserSorterAPI Sorter { get; }
        
        /// <summary>
        /// Searches for lobbies matching the current filters.
        /// </summary>
        public void Browse();
    }
}