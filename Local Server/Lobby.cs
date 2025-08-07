namespace LocalLobby;

public class Lobby
{
    private string _ownerId;
    private List<string> _memberIds = new();
    private Dictionary<string, Dictionary<string, string>> _memberData = new();

    public Lobby(string ownerId)
    {
        _ownerId = ownerId;

        AddMember(ownerId);
    }

    public Response AddMember(string memberId)
    {
        if (_memberIds.Contains(memberId))
        {
            // TODO: Return an error
            return null;
        }

        _memberIds.Add(memberId);
        _memberData[memberId] = new Dictionary<string, string>();

        // TODO: Return actual value
        return null;
    }
}