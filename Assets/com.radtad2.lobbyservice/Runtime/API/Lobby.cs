namespace LobbyService
{
    public static class Lobby
    {
        private static LobbyController _controller;
        private static IPreInitStrategy _preInitStrategy = new DropPreInitStrategy();
        
        public static void Initialize(IPreInitStrategy strategy = null)
        {
            if (strategy != null) _preInitStrategy = strategy;
            
            var browser = ModuleProxyFactory.Create<IBrowserAPIInternal>(_preInitStrategy);
            browser.Sorter = ModuleProxyFactory.Create<IBrowserSorterAPI>(_preInitStrategy);
            browser.Sorter = ModuleProxyFactory.Create<IBrowserSorterAPI>(_preInitStrategy);
            Browser = browser;
            
            Friends = ModuleProxyFactory.Create<IFriendAPI>(_preInitStrategy);
            Chat =  ModuleProxyFactory.Create<IChatAPI>(_preInitStrategy);
            Procedure = ModuleProxyFactory.Create<IProcedureAPI>(_preInitStrategy);
        }
        
        public static void SetController(LobbyController controller)
        {
            _controller = controller;
            
            // ReSharper disable SuspiciousTypeConversion.Global
            ((ModuleProxy<IBrowserAPI>)Browser)?.AttachTarget(_controller.Browser);
            ((ModuleProxy<IFriendAPI>)Friends).AttachTarget(_controller.Friends);
            ((ModuleProxy<IChatAPI>)Chat).AttachTarget(_controller.Chat);
            ((ModuleProxy<IProcedureAPI>)Procedure).AttachTarget(_controller.Procedures);
            // ReSharper restore SuspiciousTypeConversion.Global
        }
        
        public static IBrowserAPI Browser { get; private set; }
        public static IFriendAPI Friends { get; private set; }
        public static IChatAPI Chat { get; private set; }
        public static IProcedureAPI Procedure { get; private set; }

        public static void RequestCreate(CreateLobbyRequest request, int numFailedAttempts = 0)
        {
            // TODO: Handle pre init
            
            _controller.Create(request, numFailedAttempts);
        }
        
        // TODO Add rest of methods.
    }
}