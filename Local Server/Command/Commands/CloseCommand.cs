using System.Windows.Input;

namespace LocalLobby.Command;

public class CloseCommand : ICommand
{
    public Response Execute(string lobbyId, LobbyManager manager)
    {
        return manager.CloseLobby(lobbyId);
    }
}