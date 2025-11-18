namespace LobbyService
{
    public interface ILobbyCapabilities
    {
        // Friends
        public bool SupportsFriends { get; }
        public FriendCapabilities? FriendCapabilities { get; }
        
        // Chat
        public bool SupportsChat { get; }
        public ChatCapabilities? ChatCapabilities { get; }
        
        // Browser
        public bool SupportsBrowser { get; }
        public bool SupportsBrowserSorter { get; }
        public bool SupportsBrowserFilter { get; }
        public BrowserFilterCapabilities? BrowserFilterCapabilities { get; }
        
        // Procedures
        public bool SupportsProcedures { get; }
    }
    
    public class MutableLobbyCapabilities : ILobbyCapabilities
    {
        // Friends
        public bool SupportsFriends { get; set; }
        public FriendCapabilities? FriendCapabilities { get; set; }
        
        // Chat
        public bool SupportsChat { get; set; }
        public ChatCapabilities? ChatCapabilities { get; set; }
        
        // Browser
        public bool SupportsBrowser { get; set; }
        public bool SupportsBrowserSorter { get; set; }
        public bool SupportsBrowserFilter { get; set; }
        public BrowserFilterCapabilities? BrowserFilterCapabilities { get; set; }      
        
        // Procedures
        public bool SupportsProcedures { get; set; }
    }
}