using System;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public static class Extensions
    {
        public static async Task AsCancellable(this Task task, CancellationToken token)
        {
            var cancelTask = Task.Delay(Timeout.Infinite, token);
            var completed = await Task.WhenAny(task, cancelTask);

            if (completed == cancelTask)
                throw new OperationCanceledException(token);

            await task;
        }

        public static async Task<T> AsCancellable<T>(this Task<T> task, CancellationToken token)
        {
            var cancelTask = Task.Delay(Timeout.Infinite, token);
            var completed = await Task.WhenAny(task, cancelTask);

            if (completed == cancelTask) throw new OperationCanceledException(token);

            return await task;
        }
    }
}