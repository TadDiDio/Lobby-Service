using System;
using System.Threading.Tasks;
using LobbyService.Samples.Steam;
using UnityEngine;

namespace LobbyService.LocalServer
{
    public class LocalLobbyBootstrapper : MonoBehaviour
    {
        [SerializeField] private SampleView view;
        [SerializeField] private LobbyRules rules;
        
        private void Start()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Local provider relies on LocalLobby API so wait for that to initialize.
                if (!await LocalLobby.WaitForInitializationAsync(destroyCancellationToken)) return;
        
                var provider = new LocalProvider();
                
                // Controller links itself to Lobby API in constructor so no need to cache a reference
                var controller = new LobbyController(provider, rules);
                
                // Attaches the view to the lobby to allow it to receive updates
                Lobby.ConnectView(view);
            }
            catch (OperationCanceledException) { /* Ignored */ }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnDestroy()
        {
            LocalLobby.Shutdown();
        }
    }
}