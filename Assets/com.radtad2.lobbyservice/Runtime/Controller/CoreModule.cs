using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyService
{
    public class CoreModule : ICoreModule, IDisposable
    {
        private LobbyModel _model;
        private LobbyController _controller;
        private BaseLobbyProvider _provider;
        private LobbyStateMachine _stateMachine;
        
        /// <summary>
        /// Invoked when the user has entered a lobby. Params tell if they are the owner and the lobby id.
        /// </summary>
        public event Action<bool, ProviderId> OnEnteredLobby;

        /// <summary>
        /// Invoked when the user has left a lobby. Param tells the lobby id left.
        /// </summary>
        public event Action<ProviderId> OnLeftLobby;

        /// <summary>
        /// Invoked when the local member's ownership status changes.
        /// </summary>
        public event Action<bool> OnLocalOwnershipChanged;

        /// <summary>
        /// Invoked when a create lobby request was sent but it fails.
        /// </summary>
        public event Action<EnterFailedResult<CreateLobbyRequest>> OnCreateLobbyFailed;

        /// <summary>
        /// Invoked when a join lobby request was sent but it fails.
        /// </summary>
        public event Action<EnterFailedResult<JoinLobbyRequest>> OnJoinLobbyFailed;

        /// <summary>
        /// Invoked when a member other than the local one joins the lobby.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberJoined;

        /// <summary>
        /// Invoked when a member other than the local one leaves the lobby.
        /// </summary>
        public event Action<LobbyMember> OnOtherMemberLeft;

        public CoreModule(LobbyController controller, BaseLobbyProvider provider, LobbyModel model, LobbyStateMachine stateMachine)
        {
            _controller = controller;
            _provider = provider;
            _model = model;
            _stateMachine = stateMachine;

            _provider.OnOtherMemberJoined  += OtherMemberJoined;
            _provider.OnOtherMemberLeft    += OtherMemberLeft;
            _provider.OnOwnerUpdated       += OwnerUpdated;
            _provider.OnLocalMemberKicked  += LocalMemberKicked;
            _provider.OnReceivedInvitation += ReceivedInvite;
            _provider.OnLobbyDataUpdated   += LobbyDataUpdated;
            _provider.OnMemberDataUpdated  += MemberDataUpdated;
        }

        public void Dispose()
        {
            _provider.OnOtherMemberJoined  -= OtherMemberJoined;
            _provider.OnOtherMemberLeft    -= OtherMemberLeft;
            _provider.OnOwnerUpdated       -= OwnerUpdated;
            _provider.OnLocalMemberKicked  -= LocalMemberKicked;
            _provider.OnReceivedInvitation -= ReceivedInvite;
            _provider.OnLobbyDataUpdated   -= LobbyDataUpdated;
            _provider.OnMemberDataUpdated  -= MemberDataUpdated;
        }

        private bool ValidatePermission(LobbyState requiredState, bool requiresOwnership)
        {
            var state = _stateMachine.State == requiredState;
            var owner = _controller.IsOwner;
            return state && (!requiresOwnership || owner);
        }

        private bool ValidatePermission(List<LobbyState> allowedStates, bool requiresOwnership)
        {
            var state = allowedStates.Any(s => s == _stateMachine.State);
            var owner = _controller.IsOwner;
            return state && (!requiresOwnership || owner);
        }

        private bool TrySetState(LobbyState newState, bool shouldSucceed = false)
        {
            var success = _stateMachine.TryTransition(newState);

            if (shouldSucceed && !success)
                throw new InvalidOperationException("A state change that should succeed failed!");

            return success;
        }

        public async Task CreateLobbyAsync(CreateLobbyRequest request, int numPrevFailedAttempts)
        {
            if (!TrySetState(LobbyState.Joining)) return;

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayCreateRequested(request));

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

            OnEnteredLobby?.Invoke(true, result.LobbyId);
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayCreateResult(result));
        }

        public async Task JoinLobbyAsync(JoinLobbyRequest request, int numPrevAttempts)
        {
            if (!TrySetState(LobbyState.Joining)) return;

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayJoinRequested(request));

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

            OnEnteredLobby?.Invoke(false, result.LobbyId);
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayJoinResult(result));
        }

        private void HandleEnterFailure<T>(T request, bool isObsolete, BaseLobbyProvider obsoleteProvider, EnterLobbyResult result, int attempt)
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
                    OnCreateLobbyFailed?.Invoke(new EnterFailedResult<CreateLobbyRequest>
                    {
                        Reason = reason,
                        Request = createRequest,
                        FailedAttempts = attempt + 1,
                    });

                    _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayCreateResult(EnterLobbyResult.Failed(reason)));
                    break;
                case JoinLobbyRequest joinRequest:
                    OnJoinLobbyFailed?.Invoke(new EnterFailedResult<JoinLobbyRequest>
                    {
                        Reason = reason,
                        Request = joinRequest,
                        FailedAttempts = attempt + 1,
                    });

                    _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayJoinResult(EnterLobbyResult.Failed(reason)));
                    break;
                default: throw new ArgumentException($"T request was {request.GetType().Name} must be a CreateLobbyRequest or a JoinLobbyRequest");
            }
        }

        public void Close()
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;
            if (!TrySetState(LobbyState.Leaving)) return;
            if (!_provider.Close(_model.LobbyId)) return;

            Leave();
        }

        public void Leave(bool selfKick = false)
        {
            if (!ValidatePermission(new List<LobbyState> {LobbyState.InLobby, LobbyState.Leaving}, false)) return;
            if (!TrySetState(LobbyState.Leaving)) return;

            _provider.Leave(_model.LobbyId);

            if (!TrySetState(LobbyState.NotInLobby, true)) return;

            var lobbyId = _model.LobbyId;
            _model.Reset();
            OnLeftLobby?.Invoke(lobbyId);

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
                Member = _controller.LocalMember,
                LeaveReason = reason,
                KickInfo = kickInfo
            };
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayLocalMemberLeft(info));
        }

        public void SendInvite(LobbyMember member)
        {
            if (!ValidatePermission(LobbyState.InLobby, _controller.Rules.OnlyOwnerCanInvite)) return;

            var sent = _provider.SendInvite(_model.LobbyId, member);

            var result = new InviteSentInfo
            {
                InviteSent = sent,
                Member = member
            };

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplaySendInvite(result));
        }

        public void SetOwner(LobbyMember newOwner)
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;

            if (!_provider.SetOwner(_model.LobbyId, newOwner)) return;

            _model.Owner = newOwner;
            OnLocalOwnershipChanged?.Invoke(false);
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayUpdateOwner(newOwner));
        }

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

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayOtherMemberLeft(result));
        }

        public void SetLobbyData(string key, string value)
        {
            if (!ValidatePermission(LobbyState.InLobby, true)) return;

            if (!_provider.SetLobbyData(_model.LobbyId, key, value)) return;

            _model.LobbyData.Set(key, value);

            var update = new LobbyDataUpdate
            {
                Data = _model.LobbyData
            };

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayUpdateLobbyData(update));
        }

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

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayUpdateMemberData(update));
        }

        public string GetLobbyDataOrDefault(string key, string defaultValue)
        {
            return _stateMachine.State is not LobbyState.InLobby
                ? defaultValue
                : _model.LobbyData.GetOrDefault(key, defaultValue);
        }

        public string GetMemberDataOrDefault(LobbyMember member, string key, string defaultValue)
        {
            return _stateMachine.State is not LobbyState.InLobby
                ? defaultValue
                : _model.MemberData[member].GetOrDefault(key, defaultValue);
        }

        private void OtherMemberJoined(MemberJoinedInfo info)
        {
            if (!_model.InLobby) return;

            _model.AddMember(info);
            OnOtherMemberJoined?.Invoke(info.Member);
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayOtherMemberJoined(info));
        }

        private void OtherMemberLeft(LeaveInfo info)
        {
            if (!_model.InLobby) return;

            _model.RemoveMember(info.Member);
            OnOtherMemberLeft?.Invoke(info.Member);
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayOtherMemberLeft(info));
        }

        private void OwnerUpdated(LobbyMember newOwner)
        {
            if (!_model.InLobby) return;

            var wasOwner = _controller.IsOwner;

            _model.Owner = newOwner;

            if (_controller.IsOwner != wasOwner)
            {
                OnLocalOwnershipChanged?.Invoke(_controller.IsOwner);
            }

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayUpdateOwner(newOwner));
        }

        private void LocalMemberKicked(KickInfo info)
        {
            if (!_model.InLobby) return;

            var lobbyId = _model.LobbyId;
            _model.Reset();
            OnLeftLobby?.Invoke(lobbyId);

            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayLocalMemberLeft(new LeaveInfo
            {
                Member = _controller.LocalMember,
                KickInfo = info,
                LeaveReason = LeaveReason.Kicked
            }));
        }

        private void ReceivedInvite(LobbyInvite invite)
        {
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayReceivedInvite(invite));
        }

        private void LobbyDataUpdated(LobbyDataUpdate update)
        {
            if (!_model.InLobby) return;

            _model.LobbyData = update.Data;
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayUpdateLobbyData(update));
        }

        private void MemberDataUpdated(MemberDataUpdate update)
        {
            if (!_model.InLobby) return;

            _model.MemberData[update.Member] = update.Data;
            _controller.BroadcastToViews<ILobbyCoreView>(v => v.DisplayUpdateMemberData(update));
        }
    }
}
