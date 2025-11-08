using System;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public static class Extensions
    {
        public static void LogExceptions(this Task task)
        {
            _ = task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Console.WriteLine(t.Exception);
            }, TaskScheduler.Default);
        }
    }
}