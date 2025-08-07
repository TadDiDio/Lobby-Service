namespace LocalLobby.Command;

public interface ICommand
{
    public Response Execute(string lobbyId, LobbyManager manager);
}