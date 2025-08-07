using System.Text;
using System.Net.Sockets;

namespace LocalLobby;

public class SocketAdapter : IClientAdapter
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;


    public SocketAdapter(TcpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _stream = _client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
        _writer = new StreamWriter(_stream, Encoding.UTF8, leaveOpen: true)
        {
            AutoFlush = true
        };
    }

    public async Task<string?> ReadAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _reader.ReadLineAsync().WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (IOException)
        {
            // Connection closed or network error
            return null;
        }
    }

    public async Task WriteAsync(string message, CancellationToken cancellationToken)
    {
        if (_writer == null) return;

        try
        {
            await _writer.WriteLineAsync(message).WaitAsync(cancellationToken);
        }
        catch (IOException)
        {
            // Connection closed
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _stream?.Dispose();
        _client?.Close();
    }
}