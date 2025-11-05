using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    /// <summary>
    /// Arbitrates important state between lobby and view. The view and lobby communicate directly for trivial things.
    /// This class manages high level state and determining if transitions are valid. Operations are delegated to
    /// the lobby and view.
    /// </summary>
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private LobbyRules rules;
        public LobbyRules Rules => rules;

        /// <summary>
        /// Invoked when you entered a lobby.
        /// </summary>
        public event Action OnEnteredLobby;

        /// <summary>
        /// Invoked when you left a lobby.
        /// </summary>
        public event Action OnLeftLobby;

        /// <summary>
        /// Invoked when your ownership status changes.
        /// </summary>
        public event Action<bool> OwnershipChanged;

        /// <summary>
        /// Invoked when a member other than the local one joins.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberJoined;

        /// <summary>
        /// Invoked when a member other than the local one leaves.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberLeft;

        private LobbyModel _model;
        private BaseLobbyProvider _provider;
        private StaleLobbyManager _staleLobbyManager;
        private List<ILobbyView> _views = new();

        private LobbyStateMachine _stateMachine;
        private CoreModule _core;
        private FriendsModule _friends;
        private ProcedureModule _procedures;
        private ChatModule _chat;
        private HeartbeatModule _heartbeat;
        private BrowserModule _browser;

        private Dictionary<int, List<Task>> _joinOperations = new();

        private void Awake() => _staleLobbyManager = new StaleLobbyManager(); // Can't initialize as field due to exception

        private void OnDestroy()
        {
            DisposeOrDeprecate();
            ResetController();
        }

        /// <summary>
        /// Sets a new provider and safely closes any existing one.
        /// </summary>
        /// <param name="provider">The new provider to use.</param>
        public void SetProviderAndRebuild(BaseLobbyProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            CloseProvider();
            Initialize(provider);
        }

        private void CloseProvider()
        {
            if (_provider == null) return;

            Leave();
            DisposeOrDeprecate();
            ResetController();
        }

        private void DisposeOrDeprecate()
        {
            if (_provider == null) return;
            
            if (_joinOperations[_provider.GetHashCode()].Count > 0)
            {
                _provider.MarkObsolete();
            }
            else _provider.Dispose();
        }

        private void ResetController()
        {
            if (_core != null)
            {
                _core.OnEnteredLobby -= OnLocalMemberEnteredLobby;
                _core.OnLeftLobby -= OnLocalMemberLeftLobby;
                _core.OnLocalOwnershipChanged -= OnLocalOwnershipChanged;
                _core.OnCreateLobbyFailed -= OnCreateFailed;
                _core.OnJoinLobbyFailed -= OnJoinFailed;
                _core.OnOtherMemberJoined -= OnOtherJoined;
                _core.OnOtherMemberLeft -= OnOtherLeft;
            }
   
            _core?.Dispose();
            _friends?.Dispose();
            _procedures?.Dispose();
            _chat?.Dispose();
            _heartbeat?.Dispose();
            _browser?.Dispose();

            _model = null;
        }

        private void Initialize(BaseLobbyProvider provider)
        {
            _model = new LobbyModel();
            _stateMachine = new LobbyStateMachine();
            _provider = provider;
            _core = new CoreModule(this, _provider, _model, _stateMachine);

            _core.OnEnteredLobby += OnLocalMemberEnteredLobby;
            _core.OnLeftLobby += OnLocalMemberLeftLobby;
            _core.OnLocalOwnershipChanged += OnLocalOwnershipChanged;
            _core.OnCreateLobbyFailed += OnCreateFailed;
            _core.OnJoinLobbyFailed += OnJoinFailed;
            _core.OnOtherMemberJoined += OnOtherJoined;
            _core.OnOtherMemberLeft += OnOtherLeft;

            _provider.Initialize(this);
            _joinOperations.Add(_provider.GetHashCode(), new List<Task>());

            if (_staleLobbyManager.TryGetStaleId(_provider.GetType(), out ProviderId staleId))
            {
                _provider.Leave(staleId);
                _staleLobbyManager.EraseId(_provider.GetType());
            }

            if (_provider is ILobbyFriendService friends)
            {
                _friends = new FriendsModule(friends);

                if (rules.AutoStartFriendPolling) friends.StartFriendPolling(rules.FriendDiscoveryFilter, rules.FriendPollingRateSeconds);
            }

            if (_provider is ILobbyProcedureService procedures) _procedures = new ProcedureModule(procedures, _model);
            if (_provider is ILobbyChatService chat) _chat = new ChatModule(this, chat, _model);
            if (_provider is ILobbyHeartbeatService heart && rules.UseHeartbeatTimeout) _heartbeat = new HeartbeatModule(this, _core, heart, _model);
            if (_provider is ILobbyBrowserService browser) _browser = new BrowserModule(this, browser);

            if (rules.AutoStartLobbies)
            {
                var request = rules.AutoLobbyCreateRequest;

                if (rules.NameAutoLobbyAfterUser)
                {
                    request.Name = $"{LocalMember}'s Lobby";
                }

                Create(request);
            }
        }

        #region Coordination
        private void OnLocalMemberEnteredLobby(bool asOwner, ProviderId lobbyId)
        {
            _staleLobbyManager.RecordId(_provider.GetType(), lobbyId);

            OnEnteredLobby?.Invoke();

            if (_heartbeat == null) return;

            _heartbeat.StartOwnHeartbeat(rules.HeartbeatIntervalSeconds, rules.HeartbeatTimeoutSeconds);

            if (asOwner) SubToAllHeartbeats();
            else _heartbeat.SubscribeToHeartbeat(_model.Owner);
        }

        private void SubToAllHeartbeats()
        {
            foreach (var member in _model.Members)
            {
                if (member == LocalMember) continue;
                _heartbeat.SubscribeToHeartbeat(member);
            }
        }
        private void OnLocalMemberLeftLobby(ProviderId lobbyId)
        {
            OnLeftLobby?.Invoke();

            _heartbeat?.StopHeartbeatAndClearSubscriptions();
            _staleLobbyManager.EraseId(_provider.GetType());
        }
        private void OnLocalOwnershipChanged(bool gainedOwnership)
        {
            OwnershipChanged?.Invoke(gainedOwnership);

            if (_heartbeat == null) return;

            foreach (var member in _model.Members)
            {
                if (member == LocalMember) continue;
                _heartbeat.UnsubscribeFromHeartbeat(member);
            }

            if (gainedOwnership) SubToAllHeartbeats();
            else _heartbeat.SubscribeToHeartbeat(_model.Owner);
        }
        private void OnCreateFailed(EnterFailedResult<CreateLobbyRequest> failure)
        {
            if (rules.CreateFailedPolicy == null)
            {
                Debug.LogWarning("No policy set in rules for handling creation failure.");
                return;
            }

            rules.CreateFailedPolicy.Handle(this, failure);
        }
        private void OnJoinFailed(EnterFailedResult<JoinLobbyRequest> failure)
        {
            if (rules.JoinFailedPolicy == null)
            {
                Debug.LogWarning("No policy set in rules for handling join failure.");
                return;
            }

            rules.JoinFailedPolicy.Handle(this, failure);
        }

        private void OnOtherJoined(LobbyMember member)
        {
            OnOtherMemberJoined?.Invoke(member);

            if (_heartbeat != null && IsOwner) _heartbeat.SubscribeToHeartbeat(member);
        }

        private void OnOtherLeft(LobbyMember member)
        {
            OnOtherMemberLeft?.Invoke(member);

            if (_heartbeat != null && IsOwner) _heartbeat.UnsubscribeFromHeartbeat(member);
        }

        #endregion

        #region Data
        /// <summary>
        /// Gets the local member.
        /// </summary>
        /// <returns></returns>
        public LobbyMember LocalMember => _provider?.GetLocalUser();

        /// <summary>
        /// Tells if you are the owner of the lobby.
        /// </summary>
        public bool IsOwner => _model.InLobby && _model.Owner == LocalMember;

        /// <summary>
        /// Gets a readonly copy of the current lobby state.
        /// </summary>
        /// <returns></returns>
        public IReadonlyLobbyModel Model => _model;
        #endregion

        #region View
        /// <summary>
        /// Connects a view to the lobby.
        /// </summary>
        public void ConnectView(ILobbyView view)
        {
            if (view == null) return;
            if (_model.InLobby)
            {
                if (view is ILobbyCoreView core) core.DisplayExistingLobby(_model);
                if (view is ILobbyFriendView friend) friend.DisplayUpdatedFriendList(GetFriends());
            }
            _views.Add(view);
        }

        /// <summary>
        /// Disconnects a view from the lobby.
        /// </summary>
        /// <param name="view"></param>
        public void DisconnectView(ILobbyView view)
        {
            if (view == null) return;
            _views.Remove(view);
        }

        /// <summary>
        /// Calls a method on each registered view.
        /// </summary>
        /// <param name="action">An action to perform.</param>
        /// <typeparam name="T">The type of view to update.</typeparam>
        public void BroadcastToViews<T>(Action<T> action)
        {
            foreach (var view in _views.OfType<T>()) action(view);
        }
        #endregion

        #region Core
        /// <summary>
        /// Attempts to create a lobby.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <param name="numPrevFailedAttempts">The number of previous failed attempts.</param>
        public void Create(CreateLobbyRequest request, int numPrevFailedAttempts = 0)
        {
            if (_model.InLobby)
            {
                if (rules.CreateWhileInLobbyPolicy == null)
                {
                    Debug.LogWarning("No policy set in rules for handling an attempt to create a lobby while already in one. Request denied.");
                    return;
                }

                var task = rules.CreateWhileInLobbyPolicy.Execute(_core, request, _model.LobbyId);
                RegisterAndStartJoinOperation(task);
                return;
            }

            RegisterAndStartJoinOperation(_core.CreateLobbyAsync(request, numPrevFailedAttempts));
        }

        /// <summary>
        /// Attempts to join a lobby.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <param name="numPrevFailedAttempts">The number of previous failed attempts.</param>
        public void Join(JoinLobbyRequest request, int numPrevFailedAttempts = 0)
        {
            if (_model.InLobby)
            {
                if (rules.JoinWhileInLobbyPolicy == null)
                {
                    Debug.LogWarning("No policy set in rules for handling an attempt to join a lobby while already in one. Request denied.");
                    return;
                }

                var task = rules.JoinWhileInLobbyPolicy.Execute(_core, request, _model.LobbyId);
                RegisterAndStartJoinOperation(task);
                return;
            }

            RegisterAndStartJoinOperation(_core.JoinLobbyAsync(request, numPrevFailedAttempts));
        }

        /// <summary>
        /// Attempts to send an invite. Only the owner can do this.
        /// </summary>
        /// <param name="member">The member to invite.</param>
        public void SendInvite(LobbyMember member) => _core.SendInvite(member);

        /// <summary>
        /// Leaves the lobby.
        /// </summary>
        public void Leave() => _core.Leave();

        /// <summary>
        /// Attempts to set the lobby owner. Only the owner can do this.
        /// </summary>
        /// <param name="newOwner">The new owner.</param>
        public void SetOwner(LobbyMember newOwner) => _core.SetOwner(newOwner);

        /// <summary>
        /// Gets lobby metadata.
        /// </summary>
        /// <param name="key">The key to get.</param>
        /// <param name="defaultValue">The value to return if the key does not exist.</param>
        /// <param name="lobbyId">The lobby to get for. If unspecified we will draw from the lobby the user is in.</param>
        /// <returns>The keyed value or defaultValue if none.</returns>
        public string GetLobbyDataOrDefault(string key, string defaultValue, ProviderId lobbyId = null)
        {
            if (lobbyId == null) return _core.GetLobbyDataOrDefault(key, defaultValue);

            return _provider?.GetLobbyData(lobbyId, key, defaultValue) ?? defaultValue;
        }

        /// <summary>
        /// Gets a member's metadata.
        /// </summary>
        /// <param name="member">The member to query.</param>
        /// <param name="key">The key to get.</param>
        /// <param name="defaultValue">The value to return if the key does not exist.</param>
        /// <param name="lobbyId">The lobby to get for. If unspecified we will draw from the lobby the user is in.</param>
        /// <returns>The keyed value or defaultValue if none.</returns>
        public string GetMemberDataOrDefault(LobbyMember member, string key, string defaultValue, ProviderId lobbyId = null)
        {
            if (lobbyId == null) return _core.GetMemberDataOrDefault(member, key, defaultValue);

            return _provider?.GetMemberData(lobbyId, member, key, defaultValue) ?? defaultValue;
        }

        /// <summary>
        /// Attempts to set metadata on the lobby. Only the owner can do this.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set it to.</param>
        public void SetLobbyData(string key, string value) => _core.SetLobbyData(key, value);

        /// <summary>
        /// Sets metadata on the local member.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set it to.</param>
        public void SetLocalMemberData(string key, string value) => _core.SetLocalMemberData(LocalMember, key, value);

        /// <summary>
        /// Attempts to kick a member. Only the owner can do this.
        /// </summary>
        /// <param name="member">The member to kick.</param>
        public void KickMember(LobbyMember member) => _core.KickMember(member);

        /// <summary>
        /// Attempts to close the lobby. Only the owner can do this.
        /// </summary>
        public void Close() => _core.Close();

        private void RegisterAndStartJoinOperation(Task operation)
        {
            _joinOperations[_provider.GetHashCode()].Add(operation);
            operation.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.LogException(t.Exception);
                }

                _joinOperations[_provider.GetHashCode()].Remove(operation);
            });
        }
        #endregion

        #region Friends
        /// <summary>
        /// Starts polling for friends.
        /// </summary>
        /// <param name="filter">The filter to use.</param>
        /// <param name="intervalSeconds">The seconds between polls.</param>
        public void StartFriendPolling(FriendDiscoveryFilter filter, float intervalSeconds) => _friends?.StartPolling(filter, intervalSeconds);

        /// <summary>
        /// Stops polling for friends.
        /// </summary>
        public void StopFriendPolling() => _friends?.StopPolling();

        /// <summary>
        /// Sets the friend filter to use when searching for friends.
        /// </summary>
        /// <param name="filter">The filter to use.</param>
        public void SetFriendPollingFilter(FriendDiscoveryFilter filter) => _friends?.SetFilter(filter);

        /// <summary>
        /// Sets the interval to poll on.
        /// </summary>
        /// <param name="intervalSeconds">Seconds between polls.</param>
        public void SetFriendPollingInterval(float intervalSeconds) => _friends?.SetInterval(intervalSeconds);

        /// <summary>
        /// Gets an up to date list of friends available to invite.
        /// </summary>
        /// <returns>The list of friends.</returns>
        public List<LobbyMember> GetFriends() => _friends?.GetFriends() ?? new List<LobbyMember>();
        #endregion

        #region Chat
        /// <summary>
        /// Sends a message to the chat.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendChatMessage(string message) => _chat?.SendMessage(message);

        /// <summary>
        /// Sends a direct message to another member. Will fail if lobby rules disallow this.
        /// </summary>
        /// <param name="member">The target.</param>
        /// <param name="message">The message.</param>
        public void SendDirectMessage(LobbyMember member, string message) => _chat?.SendDirectMessage(member, message);
        #endregion

        #region Browsing
        /// <summary>
        /// Tells if the provider supports the capabilities given.
        /// </summary>
        /// <param name="capabilities">The capabilities to test for.</param>
        /// <returns>True if every input capability is supported.</returns>
        public bool SupportsCapability(LobbyBrowserCapabilities capabilities) => _browser.SupportsCapability(capabilities);

        /// <summary>
        /// Searches for lobbies matching the current filters set.
        /// </summary>
        public void Browse() => _browser.Browse().LogExceptions();

        /// <summary>
        /// Adds a number filter. Only lobbies matching this key-value pair will be returned.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void AddBrowsingNumberFilter(LobbyNumberFilter filter) => _browser.AddNumberFilter(filter);

        /// <summary>
        /// Adds a string filter. Only lobbies matching this key-value pair will be returned.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void AddBrowsingStringFilter(LobbyStringFilter filter) => _browser.AddStringFilter(filter);

        /// <summary>
        /// Sets the max distance to search for lobbies in.
        /// </summary>
        /// <param name="value">The distance to cull by.</param>
        public void SetBrowsingDistanceFilter(LobbyDistance value) => _browser.AddDistanceFilter(value);

        /// <summary>
        /// Clears the browsing distance filter.
        /// </summary>
        public void ClearBrowsingDistanceFilter() => _browser.ClearDistanceFilter();

        /// <summary>
        /// Removes a number filter by the given key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public void RemoveBrowsingNumberFilter(string key) => _browser.RemoveNumberFilter(key);

        /// <summary>
        /// Removes a string filter by the given key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public void RemoveBrowsingStringFilter(string key) => _browser.RemoveStringFilter(key);

        /// <summary>
        /// Sets the number of slots that must be available.
        /// </summary>
        /// <param name="available">The number of slots.</param>
        public void SetSlotsAvailableFilter(int available) => _browser.SetSlotsAvailableFilter(available);

        /// <summary>
        /// Clears the slot availability filter.
        /// </summary>
        public void ClearSlotsAvailableFilter() => _browser.ClearSlotsAvailableFilter();

        /// <summary>
        /// Sets the maximum number of slots to return from a Browse call.
        /// </summary>
        /// <param name="limit">The limit.</param>
        public void SetLimitResponsesFilter(int limit) => _browser.SetLimitResponsesFilter(limit);

        /// <summary>
        /// Removes the response limit.
        /// </summary>
        public void ClearLimitResponsesFilter() => _browser.ClearLimitResponsesFilter();

        /// <summary>
        /// Removes all filters.
        /// </summary>
        public void ClearBrowsingFilters() => _browser.ClearAllFilters();

        /// <summary>
        /// Adds a sorter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyAndSorter">The sorter.</param>
        /// <remarks>Sorters are applied in the order they are added.</remarks>
        public void AddBrowsingSorter(LobbyKeyAndSorter keyAndSorter) => _browser.AddSorter(keyAndSorter);

        /// <summary>
        /// Removes a sorter from the browser.
        /// </summary>
        /// <param name="key">The sorter to remove.</param>
        public void RemoveBrowsingSorter(string key) => _browser.RemoveSorter(key);

        /// <summary>
        /// Clears the current browsing sorters.
        /// </summary>
        public void ClearBrowsingSorters() => _browser.ClearSorters();
        #endregion
    }
}
