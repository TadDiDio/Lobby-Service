namespace LobbyService
{
    public static class Lobby
    {
        private static LobbyController _controller;
        private static IPreInitStrategy _preInitStrategy = new DropPreInitStrategy();
        
        public static void Initialize(IPreInitStrategy strategy = null)
        {
            if (strategy != null) _preInitStrategy = strategy;
            
            Core = ModuleProxyFactory.Create<ICoreModule>(_preInitStrategy);
        }
        
        public static void SetController(LobbyController controller)
        {
            _controller = controller;
            
            // Castings are safe due to runtime proxy type generation
            ((ModuleProxy<ICoreModule>)Core).AttachTarget(_controller.CoreModule);
        }
        
        public static ICoreModule Core { get; private set; }
    }
}