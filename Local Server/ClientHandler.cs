namespace LocalLobby;

public class ClientHandler : IDisposable
{
    private IClientAdapter _clientAdapter;

    public ClientHandler(IClientAdapter clientAdapter)
    {
        _clientAdapter = clientAdapter;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            string? command = "";
            while (command != null)
            {
                command = await _clientAdapter.ReadAsync(cancellationToken);

                // TODO: Switch on command
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task WriteAsync(string message, CancellationToken cancellationToken)
    {
        await _clientAdapter.WriteAsync(message, cancellationToken);
    }

    public void Dispose()
    {
        _clientAdapter.Dispose();
    }
}