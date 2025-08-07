namespace LocalLobby;

public class ConsoleAdapter : IClientAdapter
{
    public void Dispose()
    {
        // No-op
    }

    public async Task<string?> ReadAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string?>();

        // Register cancellation callback to cancel TCS if token cancels
        cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        await Task.Run(() =>
        {
            try
            {
                var line = Console.ReadLine();
                tcs.TrySetResult(line);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        return tcs.Task.Result;
    }

    public async Task WriteAsync(string message, CancellationToken cancellationToken)
    {
        await Task.Run(() => Console.WriteLine(message), cancellationToken);
    }
}
