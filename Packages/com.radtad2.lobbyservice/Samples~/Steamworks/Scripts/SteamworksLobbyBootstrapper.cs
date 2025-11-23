using LobbyService.Example.Steam;
using UnityEngine;

namespace LobbyService.Example
{
    public class SteamworksLobbyBootstrapper : MonoBehaviour
    {
        [SerializeField] private SteamSampleView view;
        [SerializeField] private LobbyRules rules;
        
        // Change this to see the minimal example.
        private bool usePreferred = true;
        
        private void Start()
        {
            if (usePreferred) PreferredSetup();
            else MinimalSetup();
        }

        private void PreferredSetup()
        {
            // We pass in the list of keys we are using so steam knows what data to pull during updates.
            // Modify SteamLobbyKeys.LobbyKeys and .MemberKeys to include any additional keys you care about.
            var provider = new SteamProvider(SteamLobbyKeys.LobbyKeys, SteamLobbyKeys.MemberKeys);

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

        private void MinimalSetup()
        {
            // Bare minimum, uses all defaults (including rules i.e. the rules in inspector are ignored).
            var provider = new SteamProvider(SteamLobbyKeys.LobbyKeys, SteamLobbyKeys.MemberKeys);
            Lobby.SetProvider(provider);
            Lobby.ConnectView(view);
        }
    }
}