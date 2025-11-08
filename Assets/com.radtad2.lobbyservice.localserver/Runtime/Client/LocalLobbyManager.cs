using UnityEngine;

namespace LobbyService.LocalServer
{
    public class LocalLobbyManager : MonoBehaviour
    {
        private void Awake()
        {
            LocalLobby.Init(destroyCancellationToken);
        }
    }
}