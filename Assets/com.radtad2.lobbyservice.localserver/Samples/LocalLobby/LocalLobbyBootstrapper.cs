using System;
using System.Threading.Tasks;
using LobbyService.Samples.Steam;
using UnityEngine;

namespace LobbyService.LocalServer
{
    public class LocalLobbyBootstrapper : MonoBehaviour
    {
        [SerializeField] private SampleCoreView view;
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
                await LocalLobby.WaitForInitializationAsync(destroyCancellationToken);
            
                var provider = new LocalLobbyProvider();

                // Set the provider in the controller after it is created
                controller.SetProvider(provider);
            
                // Handle the view after the provider is set.
                // A provider must be set before any action can occur on the controller.
                controller.ConnectView(view);
                view.SetController(controller);
            }
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