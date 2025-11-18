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
                // 1st step is always to initialize the lobby system
                Lobby.Initialize();

                // Local provider relies on LocalLobby API so wait for that to initialize
                if (!await LocalLobby.WaitForInitializationAsync(destroyCancellationToken)) return;

                // LocalLobby init, let's make the provider now
                var provider = new LocalProvider();

                // Controller links itself to Lobby API in constructor so no need to cache a reference
                var controller = new LobbyController(provider, rules);

                // Attaches the view to the lobby to allow it to receive updates
                Lobby.ConnectView(view);
            }
            catch (OperationCanceledException)
            {
                // Expected if the operation is cancelled
                Shutdown();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Shutdown();
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Shutdown()
        {
            // It is important to call this when exiting to clear state. If domain reload is off and you do not
            // call this, you will see state persist to the next play run.
            Lobby.Shutdown();
            
            // Shutdown the specific backend. This is unrelated to Lobby.Shutdown and specifically handles
            // closing the local server resources
            LocalLobby.Shutdown();
        }
    }
}