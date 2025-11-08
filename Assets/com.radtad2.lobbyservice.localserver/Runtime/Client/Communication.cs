using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class Communication
    {
        private TcpClient _client;
        
        public async Task InitAsync(CancellationToken token)
        {
            await using (token.Register(() => _client.Close()))
            {
                var connectTask = _client.ConnectAsync(IPAddress.Loopback, ServerDetails.Port);
                var monitorTask = Task.Delay(Timeout.Infinite, token);
                
                await Task.WhenAny(connectTask, monitorTask);
            } 
        }
    }
}