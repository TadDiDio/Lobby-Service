using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    /// <summary>
    /// Arbitrates requests and responses between the front and backends. Everything talks with this controller and its submodules,
    /// never directly with the provider or view. In general, prefer interacting through the static Lobby API because it provides
    /// useful helpers.
    /// </summary>
    public class LobbyController : IDisposable
    {
        /// <summary>
        /// Invoked when the local member enters a lobby whether by joining or creating.
        /// </summary>
        public event Action OnEnteredLobby;

        /// <summary>
        /// Invoked when the local member leaves a lobby whether voluntary or kicked.
        /// </summary>
        public event Action<LeaveInfo> OnLeftLobby;

        /// <summary>
        /// Invoked when a member other than yourself enters your lobby.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberJoined;

        /// <summary>
        /// Invoked when a member other than yourself leaves your lobby.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberLeft;

        /// <summary>
        /// Invoked when you gain ownership of a lobby.
        /// </summary>
        public event Action OnOwnershipGained;
        
        /// <summary>
        /// Invoked when you lose ownership of a lobby.
        /// </summary>
        public event Action OnOwnershipLost;
        
        /// <summary>
        /// Invoked when the lobby data is updated.
        /// </summary>
        public event Action<LobbyDataUpdate> OnLobbyDataUpdated;
        
        /// <summary>
        /// Invoked when member data is updated.
        /// </summary>
        public event Action<MemberDataUpdate> OnMemberDataUpdated;
        
        /// <summary>
        /// The friends capabilities
        /// </summary>
        public IFriendAPI Friends { get; private set; }
        
        /// <summary>
        /// The procedures capabilities
        /// </summary>
        public IProcedureAPI Procedures { get; private set; }
        
        /// <summary>
        /// The friends capabilities
        /// </summary>
        public IChatAPI Chat { get; private set; }

        /// <summary>
        /// The browser capabilities.
        /// </summary>
        public IBrowserAPIInternal Browser { get; private set; }
        
        /// <summary>
        /// The capabilities of the currently configured provider.
        /// </summary>
        public ILobbyCapabilities Capabilities => _capabilities; 
        private MutableLobbyCapabilities _capabilities;
        
        // Controller state
        private readonly LobbyRules _rules;
        private readonly Dictionary<int, List<Task>> _joinOperations = new();
        
        // Required core modules
        private LobbyModel _model;
        private BaseProvider _provider;
        private LobbyStateMachine _stateMachine;
        private readonly ViewModule _viewModule;
        private readonly StaleLobbyManager _staleLobbyManager;
        
        // Optional core modules
        private IHeartbeatAPI _heartbeat;
        
        #region Initialization
        public LobbyController(BaseProvider provider, LobbyRules rules)
        {
            _rules = rules;
            _staleLobbyManager = new StaleLobbyManager();
            _viewModule = new ViewModule(this);
            SetProvider(provider);
        }

        public void Dispose()
        {
            DisposeOrDeprecate();
            ResetProviderModules();
        }
        
        /// <summary>
        /// Sets a new provider and safely closes any existing one.
        /// </summary>
        /// <param name="provider">The new provider to use.</param>
        public void SetProvider(BaseProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            CloseProvider();
            Initialize(provider);
        }
        
        private void CloseProvider()
        {
            if (_provider == null) return;

            Leave(); // Idempotent
            DisposeOrDeprecate();
            ResetProviderModules();
        }
        
        private void DisposeOrDeprecate()
        {
            if (_provider == null) return;
            
            // Provider will be disposed by last ongoing operation
            if (_joinOperations[_provider.GetHashCode()].Count > 0) _provider.MarkObsolete();
            else _provider.Dispose();
        }
        
        private void ResetProviderModules()
        {
            _provider.OnOtherMemberJoined -= OtherMemberJoined;
            _provider.OnOtherMemberLeft -= OtherMemberLeft;
            _provider.OnLobbyDataUpdated -= LobbyDataUpdated;
            _provider.OnMemberDataUpdated -= MemberDataUpdated;
            _provider.OnOwnerUpdated -= OnOwnerUpdated;
            _provider.OnLocalMemberKicked -= LocalMemberKicked;
            _provider.OnReceivedInvitation -= ReceivedInvite;
   
            _heartbeat.Dispose();
            Friends.Dispose();
            Procedures.Dispose();
            Chat.Dispose();
            Browser.Dispose();

            _model = null;
            _capabilities = null;

            OnEnteredLobby = null;
            OnLeftLobby = null;
            OnOtherMemberJoined = null;
            OnOtherMemberLeft = null;
            OnOwnershipGained = null;
            OnOwnershipLost = null;
        }
        
        private void Initialize(BaseProvider provider)
        {
            _model = new LobbyModel();
            _stateMachine = new LobbyStateMachine();
            _provider = provider;
            
            _provider.OnOtherMemberJoined += OtherMemberJoined;
            _provider.OnOtherMemberLeft += OtherMemberLeft;
            _provider.OnLobbyDataUpdated += LobbyDataUpdated;
            _provider.OnMemberDataUpdated += MemberDataUpdated;
            _provider.OnOwnerUpdated += OnOwnerUpdated;
            _provider.OnLocalMemberKicked += LocalMemberKicked;
            _provider.OnReceivedInvitation += ReceivedInvite;
            
            _joinOperations.Add(_provider.GetHashCode(), new List<Task>());
            
            ConstructCapabilities();
            
            _provider.Initialize();
            _viewModule.ResetView(Capabilities);
            
            if (_provider.ShouldFlushStaleLobbies() && _staleLobbyManager.TryGetStaleId(_provider.GetType(), out var staleId))
            {
                _provider.Leave(staleId);
                _staleLobbyManager.EraseId(_provider.GetType());
            }
        }

        private void ConstructCapabilities()
        {
            _capabilities = new MutableLobbyCapabilities();
          
            if (_provider.Friends != null)
            {
                Friends = new FriendModule(_viewModule, _provider.Friends);
                _capabilities.SupportsFriends = true;
                _capabilities.FriendCapabilities = _provider.Friends.Capabilities;
            }
            else Friends = new NullFriendModule();

            if (_provider.Procedures != null)
            {
                Procedures = new ProcedureModule(_provider.Procedures, _model);
                _capabilities.SupportsProcedures = true;
            }
            else Procedures = new NullProcedureModule();
            
            if (_provider.Chat != null)
            {
                Chat = new ChatModule(_viewModule, _provider.Chat, _model);
                _capabilities.SupportsChat = true;
                _capabilities.ChatCapabilities = _provider.Chat.Capabilities;
            }
            else Chat = new NullChatModule();

            if (_provider.Heartbeat != null && _rules.UseHeartbeatTimeout)
            {
                _heartbeat = new HeartbeatModule(this, _provider.Heartbeat, _model);
            }
            else _heartbeat = new NullHeartbeatModule();
            
            if (_provider.Browser != null)
            {
                IBrowserFilterAPI filter;
                if (_provider.Browser.Filter != null)
                {
                    filter = new BrowserFilterModule(_viewModule, _provider.Browser.Filter);
                    _capabilities.SupportsBrowserFilter = true;
                }
                else filter = new NullBrowserFilterModule();
                
                IBrowserSorterAPI sorter = null;
                if (_provider.Browser.Filter != null)
                {
                    sorter = new BrowserSorterModule(_viewModule);
                    _capabilities.SupportsBrowserSorter = true;
                }
                else filter = new NullBrowserFilterModule();
                
                Browser = new BrowserModule(_viewModule, _provider.Browser, filter, sorter);
            }
            else Browser = new NullBrowserModule(new NullBrowserFilterModule(), new NullBrowserSorterModule());
        }
        #endregion
        
        #region Data
        /// <summary>
        /// Gets the local member.
        /// </summary>
        /// <returns></returns>
        public LobbyMember LocalMember => _provider.GetLocalUser();

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
        /// <param name="view">The view to connect.</param>
        public void ConnectView(IView view) => _viewModule.Connect(view);
        
        /// <summary>
        /// Disconnects a view from the lobby.
        /// </summary>
        /// <param name="view">The view to disconnect.</param>
        public void DisconnectView(IView view) => _viewModule.Disconnect(view);
        
        #endregion
        
        #region Core
        /// <summary>
        /// Attempts to create a lobby.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        public void Create(CreateLobbyRequest request)
        {
            if (_model.InLobby)
            {
                if (_rules.CreateWhileInLobbyPolicy == null)
                {
                    LobbyLogger.LogWarning("No policy set in rules for handling an attempt to create a lobby while already in one. Request denied.");
                    return;
                }

                if (!_rules.CreateWhileInLobbyPolicy.Execute(this, request, _model.LobbyId)) return;
            }

            RegisterAndStartJoinOperation(CreateAsync(request, _provider));
        }

        private async Task CreateAsync(CreateLobbyRequest request, BaseProvider provider)
        {
            if (!TrySetState(LobbyState.Joining)) return;

            _viewModule.DisplayCreateRequested(request);
            
            var result = await provider.CreateAsync(request);

            var isObsolete = provider.IsObsolete();
            if (!result.Success || isObsolete)
            {
                HandleEnterFailure(request, result, isObsolete ? provider : null);
                return;
            }

            _model.Initialize(result);
            TrySetState(LobbyState.InLobby, true);

            OnLocalMemberEnteredLobby(true, result.LobbyId);
            _viewModule.DisplayCreateResult(result);
        }

        /// <summary>
        /// Attempts to join a lobby.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        public void Join(JoinLobbyRequest request)
        {
            if (_model.InLobby)
            {
                if (_rules.JoinWhileInLobbyPolicy == null)
                {
                    LobbyLogger.LogWarning("No policy set in rules for handling an attempt to join a lobby while already in one. Request denied.");
                    return;
                }

                if (!_rules.JoinWhileInLobbyPolicy.Execute(this, request, _model.LobbyId)) return;
            }

            RegisterAndStartJoinOperation(JoinAsync(request, _provider));
        }

        private async Task JoinAsync(JoinLobbyRequest request, BaseProvider provider)
        {
            if (!TrySetState(LobbyState.Joining)) return;

            _viewModule.DisplayJoinRequested(request);

            var result = await provider.JoinAsync(request);

            var isObsolete = provider.IsObsolete();
            if (!result.Success || isObsolete)
            {
                HandleEnterFailure(request, result, isObsolete ? provider : null);
                return;
            }

            _model.Initialize(result);
            TrySetState(LobbyState.InLobby, true);

            OnLocalMemberEnteredLobby(false, result.LobbyId);
            _viewModule.DisplayJoinResult(result);
        }
        
        /// <summary>
        /// Invites a user to your lobby. Must be in a lobby for this to have effect.
        /// </summary>
        /// <param name="member">The member to invite.</param>
        public void SendInvite(LobbyMember member)
        {
            if (!ValidatePermission(LobbyState.InLobby, _rules.OnlyOwnerCanInvite)) return;

            var sent = _provider.SendInvite(_model.LobbyId, member);

            var result = new InviteSentInfo
            {
                InviteSent = sent,
                Member = member
            };

            _viewModule.DisplaySentInvite(result);
        }
        
        /// <summary>
        /// Leaves the lobby.
        /// </summary>
        public void Leave(bool selfKick = false)
        {
            if (!ValidatePermission(LobbyState.InLobby, false)) return;
            if (!TrySetState(LobbyState.NotInLobby)) return;

            _provider.Leave(_model.LobbyId);

            LeaveReason reason = LeaveReason.UserRequested;
            KickInfo? kickInfo = null;

            if (selfKick)
            {
                reason = LeaveReason.Kicked;
                kickInfo = new KickInfo
                {
                    Reason = KickReason.OwnerStoppedResponding
                };
            }

            var info = new LeaveInfo
            {
                Member = LocalMember,
                LeaveReason = reason,
                KickInfo = kickInfo
            };
            
            OnLocalMemberLeftLobby(info);
        }

        /// <summary>
        /// Attempts to set the lobby owner. Only the owner can do this.
        /// </summary>
        /// <param name="newOwner">The new owner.</param>
        public void SetOwner(LobbyMember newOwner)
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;

            if (!_provider.SetOwner(_model.LobbyId, newOwner)) return;

            OnOwnerUpdated(newOwner);
        }

        /// <summary>
        /// Gets lobby metadata. If querying a lobby you are in, this will pull from the local cache instantly.
        /// </summary>
        /// <param name="key">The key to get.</param>
        /// <param name="defaultValue">The value to return if the key does not exist.</param>
        /// <param name="lobbyId">The lobby to get for. If unspecified we will draw from the lobby the user is in.</param>
        /// <returns>The keyed value or defaultValue if none.</returns>
        public string GetLobbyDataOrDefault(string key, string defaultValue, ProviderId lobbyId = null)
        {
            if (lobbyId == null || lobbyId.Equals(_model.LobbyId)) return _model.InLobby
                ? _model.LobbyData.GetOrDefault(key, defaultValue)
                : defaultValue;

            return _provider.GetLobbyData(lobbyId, key, defaultValue);
        }

        /// <summary>
        /// Gets a member's metadata. If querying a lobby you are in, this will pull from the local cache instantly.
        /// </summary>
        /// <param name="member">The member to query.</param>
        /// <param name="key">The key to get.</param>
        /// <param name="defaultValue">The value to return if the key does not exist.</param>
        /// <param name="lobbyId">The lobby to get for. If unspecified we will draw from the lobby the user is in.</param>
        /// <returns>The keyed value or defaultValue if none.</returns>
        public string GetMemberDataOrDefault(LobbyMember member, string key, string defaultValue, ProviderId lobbyId = null)
        {
            if (lobbyId == null || lobbyId.Equals(_model.LobbyId)) return _model.InLobby
                ? _model.MemberData[member].GetOrDefault(key, defaultValue)
                : defaultValue;

            return _provider.GetMemberData(lobbyId, member, key, defaultValue);
        }

        /// <summary>
        /// Attempts to set metadata on the lobby. Only the owner can do this.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set it to.</param>
        public void SetLobbyData(string key, string value)
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;
            if (!_provider.SetLobbyData(_model.LobbyId, key, value)) return;

            _model.LobbyData.Set(key, value);

            var update = new LobbyDataUpdate
            {
                Data = _model.LobbyData
            };

            _viewModule.DisplayUpdateLobbyData(update);
        }

        /// <summary>
        /// Sets metadata on the local member.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set it to.</param>
        public void SetMemberData(string key, string value)
        {
            if (!ValidatePermission(LobbyState.InLobby, false)) return;

            _provider.SetLocalMemberData(_model.LobbyId, key, value);
            _model.MemberData[LocalMember].Set(key, value);

            var update = new MemberDataUpdate
            {
                Member = LocalMember,
                Data = _model.MemberData[LocalMember]
            };

            _viewModule.DisplayUpdateMemberData(update);
        }

        /// <summary>
        /// Attempts to kick a member. Only the owner can do this.
        /// </summary>
        /// <param name="member">The member to kick.</param>
        public void KickMember(LobbyMember member)
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;

            // Kicking self will likely not be handled as expected.
            if (member == LocalMember) return;
            
            if (!_provider.KickMember(_model.LobbyId, member)) return;

            _model.RemoveMember(member);

            var result = new LeaveInfo
            {
                Member = member,
                KickInfo = new KickInfo
                {
                    Reason = KickReason.General
                },
                LeaveReason = LeaveReason.Kicked
            };

            _viewModule.DisplayOtherMemberLeft(result);
        }

        /// <summary>
        /// Attempts to close the lobby. Only the owner can do this.
        /// </summary>
        public void CloseAndLeave()
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;
            if (!TrySetState(LobbyState.NotInLobby)) return;
            if (!_provider.CloseAndLeave(_model.LobbyId)) return;

            var info = new LeaveInfo
            {
                Member = LocalMember,
                LeaveReason = LeaveReason.UserRequested,
                KickInfo = null
            };
            
            OnLocalMemberLeftLobby(info);
        }
        #endregion
        
        #region Event Handlers
        private void OnLocalMemberEnteredLobby(bool asOwner, ProviderId lobbyId)
        {
            if (_provider.ShouldFlushStaleLobbies())
            {
                _staleLobbyManager.RecordId(_provider.GetType(), lobbyId);
            }
            
            _heartbeat.StartOwnHeartbeat(_rules.HeartbeatIntervalSeconds, _rules.HeartbeatTimeoutSeconds);

            if (asOwner) SubToAllHeartbeats();
            else _heartbeat.SubscribeToHeartbeat(_model.Owner);
            
            OnEnteredLobby?.Invoke();
        }
        
        private void OnLocalMemberLeftLobby(LeaveInfo info)
        {
            _model.Reset();
            _heartbeat.ClearSubscriptions();
            
            if (_provider.ShouldFlushStaleLobbies())
            {
                _staleLobbyManager.EraseId(_provider.GetType());
            }

            OnLeftLobby?.Invoke(info);
            _viewModule.DisplayLocalMemberLeft(info);
        }
        
        private void OtherMemberJoined(MemberJoinedInfo info)
        {
            if (!_model.InLobby) return;

            _model.AddMember(info);
            
            if (IsOwner) _heartbeat.SubscribeToHeartbeat(info.Member);

            OnOtherMemberJoined?.Invoke(info.Member);
            _viewModule.DisplayOtherMemberJoined(info);
        }

        private void OtherMemberLeft(LeaveInfo info)
        {
            if (!_model.InLobby) return;

            _model.RemoveMember(info.Member);
         
            if (IsOwner) _heartbeat.UnsubscribeFromHeartbeat(info.Member);
            
            OnOtherMemberLeft?.Invoke(info.Member);
            _viewModule.DisplayOtherMemberLeft(info);
        }

        private void OnOwnerUpdated(LobbyMember newOwner)
        {
            if (!_model.InLobby) return;

            var wasOwner = IsOwner;            
            _model.Owner = newOwner;

            var becameOwner = IsOwner && !wasOwner;
            var lostOwner   = !IsOwner && wasOwner;
            
            _heartbeat.ClearSubscriptions();

            if (lostOwner) OnOwnershipLost?.Invoke();
            
            if (becameOwner)
            {
                SubToAllHeartbeats();
                OnOwnershipGained?.Invoke();
            }
            else _heartbeat.SubscribeToHeartbeat(newOwner);

            _viewModule.DisplayUpdateOwner(newOwner);
        }
        
        private void LocalMemberKicked(KickInfo info)
        {
            if (!_model.InLobby) return;
            
            OnLocalMemberLeftLobby(new LeaveInfo
            {
                Member = LocalMember,
                KickInfo = info,
                LeaveReason = LeaveReason.Kicked
            });
        }

        private void ReceivedInvite(LobbyInvite invite)
        {
            _viewModule.DisplayReceivedInvite(invite);
        }

        private void LobbyDataUpdated(LobbyDataUpdate update)
        {
            if (!_model.InLobby) return;

            _model.LobbyData = update.Data;
            
            OnLobbyDataUpdated?.Invoke(update);
            _viewModule.DisplayUpdateLobbyData(update);
        }

        private void MemberDataUpdated(MemberDataUpdate update)
        {
            if (!_model.InLobby) return;

            _model.MemberData[update.Member] = update.Data;
            OnMemberDataUpdated?.Invoke(update);
            _viewModule.DisplayUpdateMemberData(update);
        }
        #endregion
        
        #region Helpers
        private void RegisterAndStartJoinOperation(Task operation)
        {
            var cachedProvider = _provider;
            
            _joinOperations[cachedProvider.GetHashCode()].Add(operation);
            operation.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.LogException(t.Exception);
                }

                _joinOperations[cachedProvider.GetHashCode()].Remove(operation);
            });
        }
        
        private bool ValidatePermission(LobbyState requiredState, bool requiresOwnership)
        {
            var state = _stateMachine.State == requiredState;
            var owner = IsOwner;
            return state && (!requiresOwnership || owner);
        }

        private bool TrySetState(LobbyState newState, bool shouldSucceed = false)
        {
            var success = _stateMachine.TryTransition(newState);

            if (shouldSucceed && !success) throw new InvalidOperationException("A state change that should succeed failed!");

            return success;
        }
        
        private void HandleEnterFailure<T>(T request, EnterLobbyResult result, BaseProvider obsoleteProvider = null)
        {
            var reason = result.FailureReason;

            if (obsoleteProvider != null)
            {
                reason = EnterFailedReason.StaleRequest;

                if (result.Success) obsoleteProvider.Leave(new ProviderId(result.LobbyId.ToString()));

                // Indicates that only this operation remains
                if (_joinOperations[obsoleteProvider.GetHashCode()].Count == 1)
                {
                    obsoleteProvider.Dispose();
                }
            }

            TrySetState(LobbyState.NotInLobby, true);

            switch (request)
            {
                case CreateLobbyRequest createRequest:
                    
                    if (_rules.CreateFailedPolicy == null)
                    {
                        LobbyLogger.LogWarning("No policy set in rules for handling creation failure.");
                        return;
                    }

                    _rules.CreateFailedPolicy.Handle(this, new EnterFailedResult<CreateLobbyRequest>
                    {
                        Reason = reason,
                        Request = createRequest,
                    });

                    _viewModule.DisplayCreateResult(EnterLobbyResult.Failed(reason));
                    break;
                case JoinLobbyRequest joinRequest:
                    if (_rules.JoinFailedPolicy == null)
                    {
                        LobbyLogger.LogWarning("No policy set in rules for handling join failure.");
                        return;
                    }

                    _rules.JoinFailedPolicy.Handle(this, new EnterFailedResult<JoinLobbyRequest>
                    {
                        Reason = reason,
                        Request = joinRequest,
                    });
                    _viewModule.DisplayJoinResult(EnterLobbyResult.Failed(reason));
                    break;
                default: throw new ArgumentException($"T request was {request.GetType().Name} must be a CreateLobbyRequest or a JoinLobbyRequest");
            }
        }
        
        private void SubToAllHeartbeats()
        {
            foreach (var member in _model.Members)
            {
                if (member == LocalMember) continue;
                _heartbeat.SubscribeToHeartbeat(member);
            }
        }
        #endregion
    }
}
