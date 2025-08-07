using LocalLobby.Command;

namespace LocalLobby;

public class Lobby
{
    private string _lobbyId;
    private string _ownerId;
    private List<string> _memberIds = new();
    private Dictionary<string, Dictionary<string, string>> _memberData = new();

    public Lobby(string lobbyId, string ownerId)
    {
        _lobbyId = lobbyId;
        _ownerId = ownerId;

        AddMember(ownerId);
    }

    public Response AddMember(string memberId)
    {
        if (_memberIds.Contains(memberId))
        {
            // TODO: Return an error
            return Response.Error("Member was already in the lobby");
        }

        _memberIds.Add(memberId);
        _memberData[memberId] = new Dictionary<string, string>();

        // TODO: Return actual value
        return Response.Success($"Member {memberId} added to lobby {_lobbyId}");
    }
}