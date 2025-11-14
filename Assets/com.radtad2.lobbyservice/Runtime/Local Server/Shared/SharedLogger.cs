using System;

namespace LobbyService.LocalServer
{
    public static class SharedLogger
    {
        public const string Header = "[Local Server]";

        public static void WriteLine(object value)
        {
            Console.WriteLine($"{Header} {value}");
        }
    }
}