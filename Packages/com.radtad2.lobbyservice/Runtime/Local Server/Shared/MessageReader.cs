using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class MessageReader : IDisposable
    {
        public event Action<Message> OnMessage;
        public event Action OnDisconnected;
        
        private CancellationTokenSource _tokenSource;
        private bool _disposed;
        
        
        public MessageReader(StreamReader reader)
        {
            _tokenSource = new CancellationTokenSource();
            _ = ReceiveLoopAsync(reader);
        }
        
        public async Task<Message> WaitForMessageAsync(Guid messageId, float timeoutSeconds, CancellationToken token)
        {
            if (_disposed) return Message.CreateFailure(Error.Cancelled, messageId);
            
            var combined = CancellationTokenSource.CreateLinkedTokenSource(token, _tokenSource.Token);
            var tcs = new TaskCompletionSource<Message>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Listener(Message msg)
            {
                if (msg.RequestId != messageId) return;
                
                OnMessage -= Listener;
                tcs.TrySetResult(msg);
            }

            OnMessage += Listener;

            try
            {
                var delayTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), combined.Token);
                var completed = await Task.WhenAny(tcs.Task, delayTask);

                if (completed == tcs.Task) return await tcs.Task;

                OnMessage -= Listener;
                return null;
            }
            catch (OperationCanceledException) { return null; }
            catch (Exception e)
            {
                SharedLogger.WriteLine(e);
                throw;
            }
            finally { OnMessage -= Listener; }
        }
        
        private async Task ReceiveLoopAsync(StreamReader reader)
        {
            try
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    string line;
                    try
                    {
                        line = await reader.ReadLineAsync().AsCancellable(_tokenSource.Token);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (IOException) { break; } 

                    if (line == null) break;

                    if (!MessageSerializer.Deserialize(line, out var message))
                    {
                        SharedLogger.WriteLine($"Received badly formatted message: {Environment.NewLine}{line}.");
                        continue;
                    }
                
                    OnMessage?.Invoke(message);
                }

                OnDisconnected?.Invoke();
            }
            catch (Exception e)
            {
                SharedLogger.WriteLine(e);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
    }
}