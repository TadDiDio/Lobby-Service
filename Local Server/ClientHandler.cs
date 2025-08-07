using System.Text.Json;
using LocalLobby.Command;

namespace LocalLobby;

public class ClientHandler : IDisposable
{
    private IClientAdapter _clientAdapter;
    private LobbyManager _lobbyManager;

    public ClientHandler(IClientAdapter clientAdapter, LobbyManager lobbyManager)
    {
        _clientAdapter = clientAdapter;
        _lobbyManager = lobbyManager;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            string? json;
            while ((json = await _clientAdapter.ReadAsync(cancellationToken)) != null)
            {
                try
                {
                    CommandEnvelope? envelope = JsonSerializer.Deserialize<CommandEnvelope>(json);
                    if (envelope == null) throw new JsonException("String deserialied to a null value");

                    var payload = CommandRegistry.Deserialize(envelope);

                    if (payload is not ICommand command)
                        throw new JsonException($"Deserialized payload of type '{payload?.GetType().FullName}' does not implement ICommand.");

                    var response = command.Execute(envelope.LobbyId, _lobbyManager);
                    string message = JsonSerializer.Serialize(response);
                    await WriteAsync(message, cancellationToken);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Invalid json received: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected exception while parsing input: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
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