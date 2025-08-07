using LocalLobby.Command;

namespace LocalLobby;

public class LobbyManager
{
    private Dictionary<string, Lobby> _lobbies = new();

    /// <summary>
    /// Creates a lobby on the backend and adds the owner to it.
    /// </summary>
    /// <param name="ownerId">The client that should own the lobby initially.</param>
    /// <returns>The lobby id or an error.</returns>
    public Response CreateLobby(string ownerId)
    {
        string lobbyId = Guid.NewGuid().ToString();

        _lobbies.Add(lobbyId, new Lobby(lobbyId, ownerId));

        return Response.Success(lobbyId);
    }

    /// <summary>
    /// Closes a lobby on the backend.
    /// </summary>
    /// <param name="lobbyId">The lobby to close.</param>
    /// <returns>Success if the lobby exists.</returns>
    public Response CloseLobby(string lobbyId)
    {
        if (!_lobbies.TryGetValue(lobbyId, out _))
        {
            return Response.Error($"No lobby existed with id {lobbyId}");
        }

        _lobbies.Remove(lobbyId);

        return Response.Success($"Lobby {lobbyId} was closed.");
    }
}