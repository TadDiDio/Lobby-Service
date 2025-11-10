using System;

namespace LobbyService.LocalServer
{
    public class CreateCommand : ICommand
    {
        public string Name;

        public void Execute()
        {
            Console.WriteLine("Executing create command");
        }
    }
}