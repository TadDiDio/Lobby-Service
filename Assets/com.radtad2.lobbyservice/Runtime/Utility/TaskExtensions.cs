using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    public static class TaskExtensions
    {
        public static void LogExceptions(this Task task)
        {
            _ = task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogException(t.Exception);
            }, TaskScheduler.Default);
        }
    }
}