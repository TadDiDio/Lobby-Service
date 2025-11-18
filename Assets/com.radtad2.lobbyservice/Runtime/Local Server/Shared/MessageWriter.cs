using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class MessageWriter : IDisposable
    {
        private ConcurrentQueue<Message> _queue = new();
        private CancellationTokenSource _tokenSource;

        public MessageWriter(StreamWriter writer)
        {
            _tokenSource = new CancellationTokenSource();
            
            _ = SendLoopAsync(writer);
        }
        
        public void Send(Message message)
        {
            _queue.Enqueue(message);
        }
        
        private async Task SendLoopAsync(StreamWriter writer)
        {
            try
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        if (_queue.TryDequeue(out var msg))
                        {
                            var json = MessageSerializer.Serialize(msg);
                            await writer.WriteLineAsync(json).AsCancellable(_tokenSource.Token);
                        }
                        else await Task.Delay(10, _tokenSource.Token);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (IOException) { break; }
                }
            }
            catch (Exception e)
            {
                SharedLogger.WriteLine(e);
            }
        }

        public void Dispose()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
    }
}