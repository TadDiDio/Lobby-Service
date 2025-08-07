using System.Text.Json;

namespace LocalLobby.Command;

public class CommandEnvelope
{
    public string LobbyId { get; set; }
    public CommandType Type { get; set; }
    public JsonElement Payload { get; set; }

    public CommandEnvelope(string lobbyId, CommandType type, JsonElement payload)
    {
        LobbyId = lobbyId;
        Type = type;
        Payload = payload;
    }
}