using System;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService
{
    public struct WaitResult<T>
    {
        /// <summary>
        /// True if the timer expired before the callback happened.
        /// </summary>
        public bool Cancelled;

        /// <summary>
        /// The result.
        /// </summary>
        public T Result;
    }

    /// <summary>
    /// An event wrapper that allows for the event to be awaited on until the next time its raised with an optional timeout.
    /// </summary>
    /// <typeparam name="T">The event parameter</typeparam>
    public class AwaitableEvent
    {
        private event Action InternalEvent;

        public void Raise() => InternalEvent?.Invoke();

        /// <summary>
        /// Waits until the event occurs.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel this.</param>
        /// <param name="timeoutSeconds">The seconds to wait before timing out.</param>
        /// <returns>False if the timeout cancelled the waiting, otherwise true.</returns>
        public async Task<bool> WaitForInvoke(CancellationToken cancellationToken = default, float timeoutSeconds = 10)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            void Handler()
            {
                InternalEvent -= Handler;
                tcs.TrySetResult(true);
            }

            InternalEvent += Handler;

            cts.Token.Register(() =>
            {
                InternalEvent -= Handler;
                tcs.TrySetResult(false);
            });

            try
            {
                var value = await tcs.Task.ConfigureAwait(false);
                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            finally
            {
                InternalEvent -= Handler;
            }
        }

        public void Subscribe(Action handler) => InternalEvent += handler;
        public void Unsubscribe(Action handler) => InternalEvent -= handler;

        public static AwaitableEvent operator +(AwaitableEvent evt, Action action)
        {
            evt.Subscribe(action);
            return evt;
        }

        public static AwaitableEvent operator -(AwaitableEvent evt, Action action)
        {
            evt.Unsubscribe(action);
            return evt;
        }
    }

    /// <summary>
    /// An event wrapper that allows for the event to be awaited on until the next time its raised with an optional timeout.
    /// </summary>
    /// <typeparam name="T">The event parameter</typeparam>
    public class AwaitableEvent<T>
    {
        private event Action<T> InternalEvent;

        public void Raise(T value)
        {
            InternalEvent?.Invoke(value);
        }

        /// <summary>
        /// Waits until the event occurs.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel this.</param>
        /// <param name="timeoutSeconds">The seconds to wait before timing out.</param>
        /// <returns>False if the timeout cancelled the waiting, otherwise true.</returns>
        public async Task<WaitResult<T>> WaitForInvoke(CancellationToken cancellationToken = default, float timeoutSeconds = 10)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            void Handler(T arg)
            {
                InternalEvent -= Handler;
                tcs.TrySetResult(arg);
            }

            InternalEvent += Handler;

            cts.Token.Register(() =>
            {
                InternalEvent -= Handler;
                tcs.TrySetCanceled();
            });

            try
            {
                var value = await tcs.Task.ConfigureAwait(false);
                return new WaitResult<T>
                {
                    Cancelled = false,
                    Result = value
                };
            }
            catch (TaskCanceledException)
            {
                return new WaitResult<T>
                {
                    Cancelled = true,
                    Result = default
                };
            }
            finally
            {
                InternalEvent -= Handler;
            }
        }

        public void Subscribe(Action<T> handler) => InternalEvent += handler;
        public void Unsubscribe(Action<T> handler) => InternalEvent -= handler;

        public static AwaitableEvent<T> operator +(AwaitableEvent<T> evt, Action<T> action)
        {
            evt.Subscribe(action);
            return evt;
        }

        public static AwaitableEvent<T> operator -(AwaitableEvent<T> evt, Action<T> action)
        {
            evt.Unsubscribe(action);
            return evt;
        }
    }
}
