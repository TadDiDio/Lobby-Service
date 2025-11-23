using System;
using System.Threading.Tasks;
using LobbyService.Example;
using UnityEngine;

namespace LobbyService.LocalServer.Example
{
    public class LocalLobbyBootstrapper : MonoBehaviour
    {
        [SerializeField] private LocalSampleView view;
        [SerializeField] private LobbyRules rules;
        
        private void Start()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // We need to initialize the asynchronous backend
                if (!await LocalLobby.WaitForInitializationAsync(destroyCancellationToken)) return;
                
                // The provider depends on the API, so create this afterwards
                var provider = new LocalProvider();

                // OPTIONAL: Override default lobby rules 
                Lobby.SetRules(rules);
            
                // OPTIONAL: Decide how to handle any calls that have already happened. 
                // This works for past calls too which is helpful if a view accidentally fired a request
                // in its awake method before this runs. The default is to queue and run commands after the 
                // lobby is set up but you can change it to ignore calls by uncommenting the next line
            
                // Lobby.SetPreInitStrategy(new DropPreInitStrategy()); // Or create your own strat inheriting from IPreInitStrategy
            
                // REQUIRED: This is the only call needed to start the lobby system. 
                // It must know which backend to use. This can be safely called again whenever you wish to hotswap backends.
                Lobby.SetProvider(provider);
            
                // Attaches the view to the lobby to allow it to receive updates. This
                // is safe to call at any time (including before the above calls).
                Lobby.ConnectView(view);
            }
            catch (OperationCanceledException)
            {
                // Expected if the operation is cancelled
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}