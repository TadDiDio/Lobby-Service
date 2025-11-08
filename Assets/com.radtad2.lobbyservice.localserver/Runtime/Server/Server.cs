using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class Server
    {
        private int _port;
        private TcpListener _listener;
        
        public Server(int port)
        {
            _port = port;
        }

        public void Start(CancellationToken token)
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();

            RunAsync(token).LogExceptions();
        }

        private async Task RunAsync(CancellationToken token)
        {
            var client = await _listener.AcceptTcpClientAsync();
            HandleClient(client, token).LogExceptions();
        }

        private async Task HandleClient(TcpClient client, CancellationToken token)
        {
            await using var stream = client.GetStream();
            
            var buffer = new byte[2048];
            var stringBuilder = new StringBuilder();

            while (!token.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
                if (bytesRead == 0) break;

                stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                int newlineIndex;
                while ((newlineIndex = stringBuilder.ToString().IndexOf('\n')) >= 0)
                {
                    string message = stringBuilder.ToString(0, newlineIndex).Trim();
                    stringBuilder.Remove(0, newlineIndex + 1);

                    if (message.Length > 0) HandleMessage(message);
                }
            }
        }

        private void HandleMessage(string json)
        {
            if (!Serializer.Deserialize(json, out var command))
            {
                Console.WriteLine($"[Local Lobby Server] Received unknown command: {'\n'}{json}");
                return;
            }
        }
    }
}