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
    public class LobbyController : IDisposable
    {
        private LobbyRules _rules;

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
        public event Action<bool> OnLocalOwnershipStatusChanged;

        /// <summary>
        /// Invoked when a member other than the local one joins.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberJoined;

        /// <summary>
        /// Invoked when a member other than the local one leaves.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberLeft;

        // Controller state
        private bool _initialized;
        private Dictionary<int, List<Task>> _joinOperations = new();
        
        // Required core modules
        private LobbyModel _model;
        private BaseProvider _provider;
        private StaleLobbyManager _staleLobbyManager;
        private LobbyStateMachine _stateMachine;
        private ViewModule _viewModule;
        
        // Optional core modules
        private HeartbeatModule _heartbeat;
        
        // Extension modules
        public IFriendAPI Friends { get; private set; }
        public IProcedureAPI Procedures { get; private set; }
        public IChatAPI Chat { get; private set; }
        public IBrowserAPI Browser { get; private set; }
        
        
        public LobbyController(BaseProvider provider, LobbyRules rules)
        {
            _rules = rules;
            _staleLobbyManager = new StaleLobbyManager();
            _viewModule = new ViewModule(this);
            SetProvider(provider);
            
            Lobby.SetController(this);
        }

        public void Dispose()
        {
            DisposeOrDeprecate();
            ResetController();
        }
        
        private void Initialize(BaseProvider provider)
        {
            _model = new LobbyModel();
            _stateMachine = new LobbyStateMachine();
            _provider = provider;
            
            // sub to provider events

            _provider.Initialize(this);
            _joinOperations.Add(_provider.GetHashCode(), new List<Task>());

            if (_provider.ShouldAutoLeaveOnCreation() && _staleLobbyManager.TryGetStaleId(_provider.GetType(), out var staleId))
            {
                _provider.Leave(staleId);
                _staleLobbyManager.EraseId(_provider.GetType());
            }

            // TODO Convert the rest like this and also add capabilities;
            // Old:
            //if (_provider is IFriendProvider friends) _friend = new FriendModule(_viewModule, friends);
            // New:
            if (_provider.Friends != null)
            {
                Friends = new FriendModule(_viewModule, _provider.Friends);
            }
            else
            {
                Friends = new NullFriendModule();
            }
            
            if (_provider is IProcedureProvider procedures) Procedures = new ProcedureModule(procedures, _model);
            if (_provider is IChatProvider chat) Chat = new ChatModule(_viewModule, chat, _model);
            if (_provider is IHeartbeatProvider heart && _rules.UseHeartbeatTimeout) _heartbeat = new HeartbeatModule(this, heart, _model);
            // if (_provider is IBrowserProvider browser) _browser = new BrowserModule(_viewModule, browser);
            
            if (_rules.AutoStartFriendPolling) Friends?.StartPolling(_rules.FriendDiscoveryFilter, _rules.FriendPollingRateSeconds);
            
            _initialized = true;
            
            if (_rules.AutoStartLobbies)
            {
                var request = _rules.AutoLobbyCreateRequest;
                if (_rules.NameAutoLobbyAfterUser) request.Name = $"{LocalMember}'s Lobby";
                Create(request);
            }
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

            Leave();
            DisposeOrDeprecate();
            ResetController();

            _viewModule.Reset();
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
            // TODO: Unsub from provider methods
   
            Friends?.Dispose();
            Procedures?.Dispose();
            Chat?.Dispose();
            _heartbeat?.Dispose();
            Browser?.Dispose();

            _model = null;
        }

        #region Data
        /// <summary>
        /// Gets the local member.
        /// </summary>
        /// <returns></returns>
        public LobbyMember LocalMember => _provider?.GetLocalUser();

        /// <summary>
        /// Tells if you are the owner of the lobby.
        /// </summary>
        public bool IsOwner => _initialized && _model.InLobby && _model.Owner == LocalMember;

        /// <summary>
        /// Gets a readonly copy of the current lobby state.
        /// </summary>
        /// <returns></returns>
        public IReadonlyLobbyModel Model => _model;
        #endregion
        
        #region Core
        
        private bool ValidatePermission(LobbyState requiredState, bool requiresOwnership)
        {
            var state = _stateMachine.State == requiredState;
            var owner = IsOwner;
            return state && (!requiresOwnership || owner);
        }

        private bool ValidatePermission(List<LobbyState> allowedStates, bool requiresOwnership)
        {
            var state = allowedStates.Any(s => s == _stateMachine.State);
            var owner = IsOwner;
            return state && (!requiresOwnership || owner);
        }

        private bool TrySetState(LobbyState newState, bool shouldSucceed = false)
        {
            var success = _stateMachine.TryTransition(newState);

            if (shouldSucceed && !success)
                throw new InvalidOperationException("A state change that should succeed failed!");

            return success;
        }
        
        private void HandleEnterFailure<T>(T request, bool isObsolete, BaseProvider obsoleteProvider, EnterLobbyResult result, int attempt)
        {
            var reason = result.FailureReason;

            if (isObsolete)
            {
                reason = EnterFailedReason.StaleRequest;

                if (result.Success)
                {
                    obsoleteProvider.Leave(new ProviderId(result.LobbyId.ToString()));
                }

                obsoleteProvider.Dispose();
            }

            TrySetState(LobbyState.NotInLobby, true);

            switch (request)
            {
                case CreateLobbyRequest createRequest:
                    
                    if (_rules.CreateFailedPolicy == null)
                    {
                        Debug.LogWarning("No policy set in rules for handling creation failure.");
                        return;
                    }

                    _rules.CreateFailedPolicy.Handle(this, new EnterFailedResult<CreateLobbyRequest>
                    {
                        Reason = reason,
                        Request = createRequest,
                        FailedAttempts = attempt + 1,
                    });

                    _viewModule.DisplayCreateResult(EnterLobbyResult.Failed(reason));
                    break;
                case JoinLobbyRequest joinRequest:
                    if (_rules.JoinFailedPolicy == null)
                    {
                        Debug.LogWarning("No policy set in rules for handling join failure.");
                        return;
                    }

                    _rules.JoinFailedPolicy.Handle(this, new EnterFailedResult<JoinLobbyRequest>
                    {
                        Reason = reason,
                        Request = joinRequest,
                        FailedAttempts = attempt + 1,
                    });
                    _viewModule.DisplayJoinResult(EnterLobbyResult.Failed(reason));
                    break;
                default: throw new ArgumentException($"T request was {request.GetType().Name} must be a CreateLobbyRequest or a JoinLobbyRequest");
            }
        }

        private void LocalOwnershipChanged()
        {
            OnLocalOwnershipStatusChanged?.Invoke(IsOwner);

            if (_heartbeat == null) return;

            foreach (var member in _model.Members)
            {
                if (member == LocalMember) continue;
                _heartbeat.UnsubscribeFromHeartbeat(member);
            }

            if (IsOwner) SubToAllHeartbeats();
            else _heartbeat.SubscribeToHeartbeat(_model.Owner);
        }
        
        /// <summary>
        /// Attempts to create a lobby.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <param name="numPrevFailedAttempts">The number of previous failed attempts.</param>
        public void Create(CreateLobbyRequest request, int numPrevFailedAttempts = 0)
        {
            if (_model.InLobby)
            {
                if (_rules.CreateWhileInLobbyPolicy == null)
                {
                    Debug.LogWarning("No policy set in rules for handling an attempt to create a lobby while already in one. Request denied.");
                    return;
                }

                if (!_rules.CreateWhileInLobbyPolicy.Execute(this, request, _model.LobbyId)) return;
            }

            RegisterAndStartJoinOperation(CreateAsync(request, numPrevFailedAttempts));
        }

        private async Task CreateAsync(CreateLobbyRequest request, int numPrevFailedAttempts)
        {
            if (!TrySetState(LobbyState.Joining)) return;

            _viewModule.DisplayCreateRequested(request);

            var cachedProvider = _provider;
            var result = await _provider.CreateAsync(request);

            bool isObsolete = cachedProvider.IsObsolete();
            if (!result.Success || isObsolete)
            {
                HandleEnterFailure(request, isObsolete, cachedProvider, result, numPrevFailedAttempts);
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
        /// <param name="numPrevFailedAttempts">The number of previous failed attempts.</param>
        public void Join(JoinLobbyRequest request, int numPrevFailedAttempts = 0)
        {
            if (_model.InLobby)
            {
                if (_rules.JoinWhileInLobbyPolicy == null)
                {
                    Debug.LogWarning("No policy set in rules for handling an attempt to join a lobby while already in one. Request denied.");
                    return;
                }

                if (!_rules.JoinWhileInLobbyPolicy.Execute(this, request, _model.LobbyId)) return;
            }

            RegisterAndStartJoinOperation(JoinAsync(request, numPrevFailedAttempts));
        }

        private async Task JoinAsync(JoinLobbyRequest request, int numPrevAttempts)
        {
            if (!TrySetState(LobbyState.Joining)) return;

            _viewModule.DisplayJoinRequested(request);

            var cachedProvider = _provider;
            var result = await _provider.JoinAsync(request);

            bool isObsolete = cachedProvider.IsObsolete();
            if (!result.Success || isObsolete)
            {
                HandleEnterFailure(request, isObsolete, cachedProvider, result, numPrevAttempts);
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

            _viewModule.DisplaySendInvite(result);
        }
        
        /// <summary>
        /// Leaves the lobby.
        /// </summary>
        public void Leave(bool selfKick = false)
        {
            if (!ValidatePermission(new List<LobbyState> {LobbyState.InLobby, LobbyState.Leaving}, false)) return;
            if (!TrySetState(LobbyState.Leaving)) return;

            _provider.Leave(_model.LobbyId);

            if (!TrySetState(LobbyState.NotInLobby, true)) return;

            _model.Reset();
            OnLeftLobby?.Invoke();

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
            _viewModule.DisplayLocalMemberLeft(info);
        }

        /// <summary>
        /// Attempts to set the lobby owner. Only the owner can do this.
        /// </summary>
        /// <param name="newOwner">The new owner.</param>
        public void SetOwner(LobbyMember newOwner)
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;

            if (!_provider.SetOwner(_model.LobbyId, newOwner)) return;

            _model.Owner = newOwner;
            LocalOwnershipChanged();
            _viewModule.DisplayUpdateOwner(newOwner);
        }

        /// <summary>
        /// Gets lobby metadata.
        /// </summary>
        /// <param name="key">The key to get.</param>
        /// <param name="defaultValue">The value to return if the key does not exist.</param>
        /// <param name="lobbyId">The lobby to get for. If unspecified we will draw from the lobby the user is in.</param>
        /// <returns>The keyed value or defaultValue if none.</returns>
        public string GetLobbyDataOrDefault(string key, string defaultValue, ProviderId lobbyId = null)
        {
            if (lobbyId == null || lobbyId.Equals(_model.LobbyId)) return _stateMachine.State is not LobbyState.InLobby
                ? defaultValue
                : _model.LobbyData.GetOrDefault(key, defaultValue);

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
            if (lobbyId == null || lobbyId.Equals(_model.LobbyId)) return _stateMachine.State is not LobbyState.InLobby
                ? defaultValue
                : _model.MemberData[member].GetOrDefault(key, defaultValue);

            return _provider?.GetMemberData(lobbyId, member, key, defaultValue) ?? defaultValue;
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
        public void SetLocalMemberData(LobbyMember localMember, string key, string value)
        {
            if (!ValidatePermission(LobbyState.InLobby, false)) return;

            _provider.SetLocalMemberData(_model.LobbyId, key, value);
            _model.MemberData[localMember].Set(key, value);

            var update = new MemberDataUpdate
            {
                Member = localMember,
                Data = _model.MemberData[localMember]
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
        public void Close()
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;
            if (!TrySetState(LobbyState.Leaving)) return;
            if (!_provider.Close(_model.LobbyId)) return;

            Leave();
        }

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
        
        #region Event Handlers
        private void OnLocalMemberEnteredLobby(bool asOwner, ProviderId lobbyId)
        {
            if (_provider.ShouldAutoLeaveOnCreation())
            {
                _staleLobbyManager.RecordId(_provider.GetType(), lobbyId);
            }

            OnEnteredLobby?.Invoke();

            if (_heartbeat == null) return;

            _heartbeat.StartOwnHeartbeat(_rules.HeartbeatIntervalSeconds, _rules.HeartbeatTimeoutSeconds);

            if (asOwner) SubToAllHeartbeats();
            else _heartbeat.SubscribeToHeartbeat(_model.Owner);
        }
        
        private void OnLocalMemberLeftLobby(ProviderId lobbyId)
        {
            OnLeftLobby?.Invoke();

            _heartbeat?.StopHeartbeatAndClearSubscriptions();

            if (_provider.ShouldAutoLeaveOnCreation())
            {
                _staleLobbyManager.EraseId(_provider.GetType());
            }
        }
        
        private void OtherMemberJoined(MemberJoinedInfo info)
        {
            if (!_model.InLobby) return;

            _model.AddMember(info);
            
            OnOtherMemberJoined?.Invoke(info.Member);

            if (_heartbeat != null && IsOwner) _heartbeat.SubscribeToHeartbeat(info.Member);

            _viewModule.DisplayOtherMemberJoined(info);
        }

        private void OtherMemberLeft(LeaveInfo info)
        {
            if (!_model.InLobby) return;

            _model.RemoveMember(info.Member);
         
            OnOtherMemberLeft?.Invoke(info.Member);

            if (_heartbeat != null && IsOwner) _heartbeat.UnsubscribeFromHeartbeat(info.Member);
            
            _viewModule.DisplayOtherMemberLeft(info);
        }
        
        private void LocalMemberKicked(KickInfo info)
        {
            if (!_model.InLobby) return;

            _model.Reset();

            OnLeftLobby?.Invoke();

            _viewModule.DisplayLocalMemberLeft(new LeaveInfo
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
            _viewModule.DisplayUpdateLobbyData(update);
        }

        private void MemberDataUpdated(MemberDataUpdate update)
        {
            if (!_model.InLobby) return;

            _model.MemberData[update.Member] = update.Data;
            _viewModule.DisplayUpdateMemberData(update);
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
