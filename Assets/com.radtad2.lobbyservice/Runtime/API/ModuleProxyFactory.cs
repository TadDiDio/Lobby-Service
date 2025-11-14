using System.Reflection;

namespace LobbyService
{
    public static class ModuleProxyFactory
    {
        public static T Create<T>(IPreInitStrategy strategy) where T : class
        {
            var proxy = DispatchProxy.Create<T, ModuleProxy<T>>() as ModuleProxy<T>;
            proxy.Initialize(strategy);
            return proxy as T;
        }
    }
}