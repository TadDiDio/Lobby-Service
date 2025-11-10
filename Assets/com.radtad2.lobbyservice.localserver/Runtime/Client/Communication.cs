using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using NUnit.Framework.Internal;

namespace LobbyService.LocalServer
{
    public class Communication
    {
        private TcpClient _client;
        private Queue<ICommand> _queue;
        
        public async Task InitAsync(CancellationToken token)
        {
            _client = new TcpClient();
            _queue = new Queue<ICommand>();

            SendCommands(token).LogExceptions();

            await using (token.Register(() => _client.Close()))
            {
                while (true)
                {
                    var connectTask = _client.ConnectAsync(IPAddress.Loopback, ServerDetails.Port);
                    var monitorTask = Task.Delay(Timeout.Infinite, token);

                    var completed = await Task.WhenAny(connectTask, monitorTask);

                    if (completed != connectTask)
                    {
                        if (token.IsCancellationRequested) return;
                        continue;
                    }

                    if (_client.Connected)
                    {
                        Debug.Log("Client connected to local server");
                        return;
                    }

                    await Task.Delay(100, token);
                }
            } 
        }

        public void SendCommand(ICommand command)
        {
            _queue.Enqueue(command);
        }

        private async Task SendCommands(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                while (_queue.Count == 0) await Task.Delay(50);

                var command = _queue.Dequeue();

                var json = Serializer.Serialize(command);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

                await _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}