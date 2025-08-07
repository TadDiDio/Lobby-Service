using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace LocalLobby;

public class Server
{
    private TcpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentBag<ClientHandler> _handlers = new();

    public Server(int port)
    {
        _cancellationTokenSource = new();
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Shutting down...");
            _cancellationTokenSource.Cancel();
            e.Cancel = true; // Prevents immediate process kill
        };

        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();

        Console.WriteLine($"Server started on port {port}");
    }

    public async Task RunAsync(bool createConsoleClient)
    {
        await ReceiveClients(createConsoleClient);
        Dispose();
    }

    private void HandleClient(IClientAdapter adapter, CancellationToken token)
    {
        var handler = new ClientHandler(adapter);
        _handlers.Add(handler);
        _ = handler.RunAsync(token);
    }

    private async Task ReceiveClients(bool createConsoleClient)
    {
        CancellationToken token = _cancellationTokenSource.Token;

        if (createConsoleClient)
        {
            HandleClient(new ConsoleAdapter(), token);
        }

        while (!token.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(token);
                var socketClient = new SocketAdapter(tcpClient);
                HandleClient(socketClient, token);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        _listener?.Stop();
        _listener?.Dispose();
        _cancellationTokenSource?.Dispose();

        foreach (var handler in _handlers)
        {
            handler.Dispose();
        }
    }
}