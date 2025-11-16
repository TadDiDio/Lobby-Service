using System;
using System.Threading.Tasks;
using LobbyService.Samples.Steam;
using UnityEngine;

namespace LobbyService.LocalServer
{
    public class LocalLobbyBootstrapper : MonoBehaviour
    {
        [SerializeField] private Sample view;
        [SerializeField] private LobbyController controller;
        
        private void Start()
        {
            _ = InitializeWhenReady();
        }

        private async Task InitializeWhenReady()
        {
            try
            {
                // Local provider relies on LocalLobby API so wait for that to initialize.
                if (!await LocalLobby.WaitForInitializationAsync(destroyCancellationToken)) return;
        
                var provider = new LocalProvider();

                // Set the provider in the controller after it is created
                controller.SetProvider(provider);

                // Handle the view after the provider is set.
                // A provider must be set before any action can occur on the controller.
                view.SetController(controller);
            
                controller.ConnectView(view);
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