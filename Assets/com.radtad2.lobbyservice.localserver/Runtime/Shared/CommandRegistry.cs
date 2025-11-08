using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LobbyService.LocalServer
{
    public static class CommandRegistry
    {
        private static Dictionary<string, ICommand> _commands;
        
        public static void RegisterCommands()
        {
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t)
                            && !t.IsInterface
                            && !t.IsAbstract);

            _commands = new Dictionary<string, ICommand>();
            
            foreach (var type in commandTypes)
            {
                var instance = (ICommand)Activator.CreateInstance(type);
                _commands[instance.GetType().Name] = instance;
            }
        }
        
        public static ICommand Get(string type)
        {
            if (_commands == null) RegisterCommands();
            
            return _commands.GetValueOrDefault(type.ToLower());
        }
    }
}