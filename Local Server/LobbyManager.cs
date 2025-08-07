namespace LocalLobby;

public class LobbyManager
{
    private Dictionary<string, Lobby> _lobbies = new();

    public string CreateLobby(string ownerId)
    {
        string lobbyId = Guid.NewGuid().ToString();

        _lobbies.Add(lobbyId, new Lobby(ownerId));

        return lobbyId;
    }
}