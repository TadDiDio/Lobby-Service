using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LobbyService.LocalServer
{
    public static class MessageTypeRegistry
    {
        private static Dictionary<string, Type> _types;
        
        public static void RegisterMessageTypes()
        {
            if (_types != null) return;
            
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => (typeof(IRequest).IsAssignableFrom(t) || typeof(IResponse).IsAssignableFrom(t))
                            && !t.IsInterface
                            && !t.IsAbstract);

            _types = new Dictionary<string, Type>();
            
            foreach (var type in commandTypes)
            {
                _types[type.FullName] = type;
            }
        }
        
        public static bool TryGetType(string typeName, out Type type)
        {
            if (_types == null) RegisterMessageTypes();

            type = null;

            if (!_types.TryGetValue(typeName, out var storedType)) return false;
            
            type = storedType;
            return true;
        }
    }
}