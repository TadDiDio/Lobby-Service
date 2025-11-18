using System;
using UnityEngine;

namespace LobbyService
{
    public static class Lobby
    {
        public static bool Initialized = false;
        public static IBrowserAPI Browser { get; private set; }
        public static IFriendAPI Friends { get; private set; }
        public static IChatAPI Chat { get; private set; }
        public static IProcedureAPI Procedure { get; private set; }
        
        private static LobbyController _controller;
        private static IPreInitStrategy _preInitStrategy = new DropPreInitStrategy();
        
        public static void Initialize(IPreInitStrategy strategy = null)
        {
            if (Initialized) return;
            
            if (strategy != null) _preInitStrategy = strategy;
            
            var browserProxy = ModuleProxyFactory.Create<IBrowserAPIInternal>(_preInitStrategy);
            browserProxy.Sorter = ModuleProxyFactory.Create<IBrowserSorterAPI>(_preInitStrategy);
            browserProxy.Sorter = ModuleProxyFactory.Create<IBrowserSorterAPI>(_preInitStrategy);
            Browser = browserProxy;
            
            Friends = ModuleProxyFactory.Create<IFriendAPI>(_preInitStrategy);
            Chat =  ModuleProxyFactory.Create<IChatAPI>(_preInitStrategy);
            Procedure = ModuleProxyFactory.Create<IProcedureAPI>(_preInitStrategy);
            
            Initialized = true;
        }

        public static void Shutdown()
        {
            if (!Initialized) return;
            
            Initialized = false;
            _preInitStrategy = new DropPreInitStrategy();
            if (_controller == null) return;
            _controller.Dispose();
            _controller = null;

            Browser = null;
            Friends = null;
            Chat = null;
            Procedure = null;
        }
        
        public static void SetController(LobbyController controller)
        {
            _controller = controller;
            
            // ReSharper disable SuspiciousTypeConversion.Global
            ((ModuleProxy<IBrowserAPIInternal>)Browser)?.AttachTarget(_controller.Browser);
            ((ModuleProxy<IFriendAPI>)Friends).AttachTarget(_controller.Friends);
            ((ModuleProxy<IChatAPI>)Chat).AttachTarget(_controller.Chat);
            ((ModuleProxy<IProcedureAPI>)Procedure).AttachTarget(_controller.Procedures);
            // ReSharper restore SuspiciousTypeConversion.Global
        }

        /// <summary>
        /// Safely sets a new provider for the controller. 
        /// </summary>
        /// <param name="newProvider">The new provider.</param>
        public static void SetProvider(BaseProvider newProvider) => Dispatch(() => _controller.SetProvider(newProvider));
            
        #region Core
        private static void Dispatch(Action call)
        {
            if (_controller == null) _preInitStrategy.Handle(call);
            else call();
        }
        
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
        /// Tries to close a lobby.
        /// </summary>
        /// <remarks>You must be the current owner to do this.</remarks>
        public static void Close() => Dispatch(() => _controller.Close());
        
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