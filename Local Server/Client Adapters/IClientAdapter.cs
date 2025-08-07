namespace LocalLobby;

public interface IClientAdapter : IDisposable
{
    public Task<string?> ReadAsync(CancellationToken cancellationToken);
    public Task WriteAsync(string message, CancellationToken cancellationToken);
}
