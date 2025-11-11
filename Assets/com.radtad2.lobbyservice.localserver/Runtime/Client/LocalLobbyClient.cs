using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class LocalLobbyClient : IDisposable
    {
        private int _port;
        private IPAddress _ip;

        private TcpClient _client = new();
        private MessageReader _reader;
        private MessageWriter _writer;
        
        public LocalLobbyClient(IPAddress ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public async Task<bool> ConnectAsync(CancellationToken token)
        {
            while (!_client.Connected && !token.IsCancellationRequested)
            {
                try
                {
                    await _client.ConnectAsync(_ip, _port).AsCancellable(token);
                }
                catch (OperationCanceledException) { break; }
                catch (SocketException) { await Task.Delay(500, token); }
            }

            if (!_client.Connected) return false;

            var stream = _client.GetStream();
            
            _reader = new MessageReader(new StreamReader(stream, Encoding.UTF8));
            _writer = new MessageWriter(new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true});
            return true;
        }

        public void Send(Message message)
        {
            _writer.Send(message);
        }

        public async Task<Message> WaitForResponse(Guid id, float timeoutSeconds, CancellationToken token)
        {
            return await _reader.WaitForMessageAsync(id, timeoutSeconds, token);
        }
        
        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _client?.Close();
            _client?.Dispose();
        }
    }
}