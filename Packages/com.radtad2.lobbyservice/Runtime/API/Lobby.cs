using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LobbyService
{
    public static class Lobby
    {
        /// <summary>
        /// Holds actions for lobby browsing.
        /// </summary>
        [UsedImplicitly] public static IBrowserAPI Browser { get; }
        
        /// <summary>
        /// Holds actions for managing lobby friends.
        /// </summary>
        [UsedImplicitly] public static IFriendAPI Friends { get; }
        
        /// <summary>
        /// Holds actions for lobby chat.
        /// </summary>
        [UsedImplicitly] public static IChatAPI Chat { get; }
        
        /// <summary>
        /// Holds actions for lobby procedures.
        /// </summary>
        [UsedImplicitly] public static IProcedureAPI Procedure { get; }
        
        /// <summary>
        /// Whether the lobby is allowing actions to be run or queued.
        /// True when in playmode, false otherwise.
        /// </summary>
        public static bool AllowingActions { get; private set; }
        
        private static LobbyController _controller;
        private static PreInitWrapper _preInitWrapper;
        private static LobbyRules _rules = new();
        
        static Lobby()
        {
            _preInitWrapper = new PreInitWrapper(new ExecutePreInitStrategy());
            
            var browserProxy = ModuleProxyFactory.Create<IBrowserAPIInternal>(_preInitWrapper);
            browserProxy.Filter = ModuleProxyFactory.Create<IBrowserFilterAPI>(_preInitWrapper);
            browserProxy.Sorter = ModuleProxyFactory.Create<IBrowserSorterAPI>(_preInitWrapper);
            Browser = browserProxy;

            Friends = ModuleProxyFactory.Create<IFriendAPI>(_preInitWrapper);
            Chat =  ModuleProxyFactory.Create<IChatAPI>(_preInitWrapper);
            Procedure = ModuleProxyFactory.Create<IProcedureAPI>(_preInitWrapper);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnEnterPlayMode()
        {
            AllowingActions = true;
            Application.quitting += Shutdown;
        }
        private static void Shutdown()
        {
            Application.quitting -= Shutdown;
            AllowingActions = false;

            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }

            // ReSharper disable SuspiciousTypeConversion.Global
            ((ModuleProxy<IBrowserAPIInternal>)Browser)?.DetachTarget();
            ((ModuleProxy<IFriendAPI>)Friends).DetachTarget();
            ((ModuleProxy<IChatAPI>)Chat).DetachTarget();
            ((ModuleProxy<IProcedureAPI>)Procedure).DetachTarget();
            // ReSharper restore SuspiciousTypeConversion.Global
            
            _preInitWrapper.Reset(new ExecutePreInitStrategy());
        }
        
        /// <summary>
        /// Determines what to do with all previous and future calls before a provider is set.
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        /// <remarks>Changing this will also affect all previous lobby actions since they are buffered
        /// until a provider is set.</remarks>
        public static void SetPreInitStrategy(IPreInitStrategy strategy)
        {
            if (strategy == null) return;
            _preInitWrapper.SetStrategy(strategy);
        }

        /// <summary>
        /// Sets the rules for the lobby to follow.
        /// </summary>
        /// <param name="rules">The rules.</param>
        public static void SetRules(LobbyRules rules)
        {
            if (rules != null) _rules = rules;
        }
        
        /// <summary>
        /// Safely sets a new provider for the controller. 
        /// </summary>
        /// <param name="newProvider">The new provider.</param>
        public static void SetProvider(BaseProvider newProvider)
        {
            if (_controller == null)
            {
                _controller = new LobbyController(newProvider, _rules);
                
                // ReSharper disable SuspiciousTypeConversion.Global
                ((ModuleProxy<IBrowserAPIInternal>)Browser)?.AttachTarget(_controller.Browser);
                ((ModuleProxy<IBrowserFilterAPI>)Browser?.Filter)?.AttachTarget(_controller.Browser.Filter);
                ((ModuleProxy<IBrowserSorterAPI>)Browser?.Sorter)?.AttachTarget(_controller.Browser.Sorter);
                
                ((ModuleProxy<IFriendAPI>)Friends).AttachTarget(_controller.Friends);
                ((ModuleProxy<IChatAPI>)Chat).AttachTarget(_controller.Chat);
                ((ModuleProxy<IProcedureAPI>)Procedure).AttachTarget(_controller.Procedures);
                // ReSharper restore SuspiciousTypeConversion.Global

                _controller.OnEnteredLobby      += () => OnEnteredLobby?.Invoke();
                _controller.OnLeftLobby         += i => OnLeftLobby?.Invoke(i);
                _controller.OnOtherMemberJoined += m => OnOtherMemberJoined?.Invoke(m);
                _controller.OnOtherMemberLeft   += m => OnOtherMemberLeft?.Invoke(m);
                _controller.OnOwnershipGained   += () => OnOwnershipGained?.Invoke();
                _controller.OnOwnershipLost     += () => OnOwnershipLost?.Invoke();
                
                if (_rules.AutoStartFriendPolling) Friends.StartPolling(_rules.FriendDiscoveryFilter, _rules.FriendPollingRateSeconds);
            
                _preInitWrapper.Flush();
            }
            else _controller.SetProvider(newProvider);
        }

        #region Core
        private static void Dispatch(Action call)
        {
            if (!AllowingActions) return;

            if (_controller == null)
            {
                if (_rules.WarnOnPreInitCommands)
                {
                    Debug.LogWarning("Received a call before initialization");
                }
                _preInitWrapper.RegisterAction(call);
            }
            else call();
        }
        
        /// <summary>
        /// Invoked when the local member enters a lobby whether by joining or creating.
        /// </summary>
        public static event Action OnEnteredLobby;

        /// <summary>
        /// Invoked when the local member leaves a lobby whether voluntary or kicked.
        /// </summary>
        public static event Action<LeaveInfo> OnLeftLobby;

        /// <summary>
        /// Invoked when a member other than yourself enters your lobby.
        /// </summary>
        public static event Action<LobbyMember> OnOtherMemberJoined;

        /// <summary>
        /// Invoked when a member other than yourself leaves your lobby.
        /// </summary>
        public static event Action<LobbyMember> OnOtherMemberLeft;

        /// <summary>
        /// Invoked when you gain ownership of a lobby.
        /// </summary>
        public static event Action OnOwnershipGained;
        
        /// <summary>
        /// Invoked when you lose ownership of a lobby.
        /// </summary>
        public static event Action OnOwnershipLost;
        
        /// <summary>
        /// Tries to create a lobby.
        /// </summary>
        /// <param name="request">The request details.</param>
        public static void Create(CreateLobbyRequest request) => Dispatch(() => _controller.Create(request));

        /// <summary>
        /// Tries to join a lobby.
        /// </summary>
        /// <param name="request">The request details.</param>
        public static void Join(JoinLobbyRequest request) => Dispatch(() => _controller.Join(request));

        /// <summary>
        /// Sends an invitation to a friend.
        /// </summary>
        /// <param name="invitee">The friend to invite.</param>
        public static void SendInvite(LobbyMember invitee) => Dispatch(() => _controller.SendInvite(invitee));

        /// <summary>
        /// Leaves a lobby.
        /// </summary>
        public static void Leave() => Dispatch(() => _controller.Leave());

        /// <summary>
        /// Tries to close a lobby and leaves it.
        /// </summary>
        /// <remarks>You must be the current owner to do this.</remarks>
        public static void CloseAndLeave() => Dispatch(() => _controller.CloseAndLeave());
        
        /// <summary>
        /// Tries to set a new owner.
        /// </summary>
        /// <param name="newOwner">The lobby member to promote.</param>
        /// <remarks>You must be the current owner to do this.</remarks>
        public static void SetOwner(LobbyMember newOwner) =>  Dispatch(() => _controller.SetOwner(newOwner));

        /// <summary>
        /// Tries to kick a member.
        /// </summary>
        /// <param name="member">The lobby member to kick.</param>
        /// <remarks>You must be the current owner to do this.</remarks>
        public static void KickMember(LobbyMember member) => Dispatch(() => _controller.KickMember(member));

        /// <summary>
        /// Tries to set metadata for the lobby.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set it to.</param>
        /// <remarks>You must be the current owner to do this.</remarks>
        public static void SetLobbyData(string key, string value) => Dispatch(() => _controller.SetLobbyData(key, value));
        
        /// <summary>
        /// Sets metadata for the local member.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set it to.</param>
        /// <remarks>This sets only the local member's data. If you want to give the owner the ability to set anyone's
        /// member data, you should implement it as a procedure in the IProcedureProvider.</remarks>
        public static void SetMemberData(string key, string value) => Dispatch(() => _controller.SetMemberData(key, value));
        #endregion
        
        #region View
        /// <summary>
        /// Connects a view to the lobby.
        /// </summary>
        /// <param name="view">The view to connect.</param>
        public static void ConnectView(IView view) => Dispatch(() => _controller.ConnectView(view));
        
        /// <summary>
        /// Disconnects a view from the lobby.
        /// </summary>
        /// <param name="view">The view to disconnect.</param>
        public static void DisconnectView(IView view) => Dispatch(() => _controller.DisconnectView(view));
        
        #endregion
        
        #region Read Surface
        /// <summary>
        /// The rules currently governing the lobby service.
        /// </summary>
        public static LobbyRules Rules => _rules;
        
        /// <summary>
        /// The local member or a special Unknown member if the system is not initialized.
        /// </summary>
        public static LobbyMember LocalMember => _controller == null ? LobbyMember.Unknown : _controller.LocalMember;

        /// <summary>
        /// True if you are the owner of a lobby, otherwise false.
        /// </summary>
        public static bool IsOwner => _controller?.IsOwner ?? false;
        
        /// <summary>
        /// A snapshot of all current lobby state.
        /// </summary>
        public static IReadonlyLobbyModel Model => _controller == null ? new LobbyModel() : _controller.Model;
        
        /// <summary>
        /// Gets lobby data or default if the key doesn't exist or the backend is not initialized.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <param name="defaultValue">The value to return on error.</param>
        /// <param name="lobbyId">Optionally, specify an id for a lobby you are not in.
        /// Leave default to query the lobby you are in.</param>
        /// <returns>The value associated with the key or default value on error.</returns>
        public static string GetLobbyDataOrDefault(string key, string defaultValue, ProviderId lobbyId = null)
        {
            return _controller == null ? defaultValue : _controller.GetLobbyDataOrDefault(key, defaultValue, lobbyId);
        }

        /// <summary>
        /// Gets member data or default if the key doesn't exist or the backend is not initialized.
        /// </summary>
        /// <param name="member">The member to query.</param>
        /// <param name="key">The key to query.</param>
        /// <param name="defaultValue">The value to return on error.</param>
        /// <param name="lobbyId">Optionally, specify an id for a lobby you are not in.
        /// Leave default to query the lobby you are in.</param>
        /// <returns>The value associated with the key or default value on error.</returns>            
        public static string GetMemberDataOrDefault(LobbyMember member, string key, string defaultValue, ProviderId lobbyId = null)
        {
            return _controller == null ? defaultValue : _controller.GetMemberDataOrDefault(member, key, defaultValue, lobbyId);
        }
        #endregion
    }
}