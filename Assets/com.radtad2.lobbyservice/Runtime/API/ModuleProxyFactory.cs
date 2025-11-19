using System.Reflection;

namespace LobbyService
{
    public static class ModuleProxyFactory
    {
        public static T Create<T>(UnInitWrapper wrapper) where T : class
        {
            var proxy = DispatchProxy.Create<T, ModuleProxy<T>>() as ModuleProxy<T>;
            proxy.Initialize(wrapper);
            return proxy as T;
        }
    }
}