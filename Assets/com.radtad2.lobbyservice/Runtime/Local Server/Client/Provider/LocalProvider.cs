using System;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class LocalProvider : BaseProvider
    {
        public override event Action<MemberJoinedInfo> OnOtherMemberJoined;
        public override event Action<LeaveInfo> OnOtherMemberLeft;
        public override event Action<KickInfo> OnLocalMemberKicked;
        public override event Action<LobbyInvite> OnReceivedInvitation;
        public override event Action<LobbyDataUpdate> OnLobbyDataUpdated;
        public override event Action<MemberDataUpdate> OnMemberDataUpdated;
        public override event Action<LobbyMember> OnOwnerUpdated;

        private CancellationTokenSource _lifetimeCts;
        private LobbyController _controller;
        public override void Initialize(LobbyController controller)
        {
            EnsureInitialized();

            LocalLobby.OnOtherMemberJoined += HandleOtherMemberJoined;
            LocalLobby.OnOtherMemberLeft += HandleOtherMemberLeft;
            LocalLobby.OnLocalMemberKicked += HandleLocalMemberKicked;
            LocalLobby.OnReceivedInvitation += HandleReceivedInvitation;
            LocalLobby.OnLobbyDataUpdated += HandleLobbyDataUpdated;
            LocalLobby.OnMemberDataUpdated += HandleMemberDataUpdated;
            LocalLobby.OnOwnerUpdated += HandleOwnerUpdated;
            
            _controller = controller;
            
            _lifetimeCts = new CancellationTokenSource();
        }

        public override void DisposeThis()
        {
            LocalLobby.OnOtherMemberJoined -= HandleOtherMemberJoined;
            LocalLobby.OnOtherMemberLeft -= HandleOtherMemberLeft;
            LocalLobby.OnLocalMemberKicked -= HandleLocalMemberKicked;
            LocalLobby.OnReceivedInvitation -= HandleReceivedInvitation;
            LocalLobby.OnLobbyDataUpdated -= HandleLobbyDataUpdated;
            LocalLobby.OnMemberDataUpdated -= HandleMemberDataUpdated;
            LocalLobby.OnOwnerUpdated -= HandleOwnerUpdated;
            
            _lifetimeCts?.Cancel();
        }

        public override bool ShouldFlushStaleLobbies() => false;
        public override IProcedureProvider Procedures { get; } = null;
        public override IHeartbeatProvider Heartbeat { get; } = null;
        public override IBrowserProvider Browser { get; } = new LocalBrowserProvider();
        public override IFriendProvider Friends { get; } = new LocalFriendsProvider();
        public override IChatProvider Chat { get; } = new LocalChatProvider();

        private void EnsureInitialized()
        {
            if (!LocalLobby.Initialized) throw new InvalidOperationException("LocalLobby must be initialized before use");
        }
        
        #region Core
        public override LobbyMember GetLocalUser()
        {
            EnsureInitialized();

            return LocalLobby.GetLocalUser();
        }

        public override async Task<EnterLobbyResult> CreateAsync(LobbyService.CreateLobbyRequest request)
        {
            EnsureInitialized();
            
            var result = await LocalLobby.Create(new CreateLobbyRequest
            {
                Capacity = request.Capacity,
				Name = request.Name,
                LobbyType = request.LobbyType.ToLocalLobbyType()
            }, token: _lifetimeCts.Token);

            if (result.Error is not Error.Ok)
            {
                return EnterLobbyResult.Failed(EnterFailedReason.General);
            }

            var snapshot = result.Response.Snapshot;
            return EnterLobbyResult.Succeeded
            (
                new ProviderId(snapshot.LobbyId.ToString()),
                snapshot.Owner.ToLobbyMember(),
                LocalLobby.GetLocalUser(),
                snapshot.Capacity,
                snapshot.LobbyType.ToLobbyType(),
                snapshot.Members.ToLobbyMembers(),
                snapshot.LobbyData.ToMeta(),
                snapshot.MemberData.ToMemberData(snapshot.Members)
            );
        }

        public override async Task<EnterLobbyResult> JoinAsync(LobbyService.JoinLobbyRequest request)
        {
            EnsureInitialized();

            var result = await LocalLobby.Join(new JoinLobbyRequest
            {
                LobbyId = request.LobbyId.ToString()
            }, token: _lifetimeCts.Token);
            
            if (result.Error is not Error.Ok)
            {
                return EnterLobbyResult.Failed(EnterFailedReason.General);
            }
            
            var snapshot = result.Response.Snapshot;
            return EnterLobbyResult.Succeeded
            (
                new ProviderId(snapshot.LobbyId.ToString()),
                snapshot.Owner.ToLobbyMember(),
                LocalLobby.GetLocalUser(),
                snapshot.Capacity,
                snapshot.LobbyType.ToLobbyType(),
                snapshot.Members.ToLobbyMembers(),
                snapshot.LobbyData.ToMeta(),
                snapshot.MemberData.ToMemberData(snapshot.Members)
            );
        }

        public override bool SendInvite(ProviderId lobbyId, LobbyMember member)
        {
            EnsureInitialized();

            LocalLobby.Invite(new InviteMemberRequest
            {
                LobbyId = lobbyId.ToString(),
                InviteeId = member.Id.ToString()
            });
            
            return true;
        }

        public override void Leave(ProviderId lobbyId)
        {
            EnsureInitialized();
            
            LocalLobby.Leave(new LeaveLobbyRequest
            {
                LobbyId = lobbyId.ToString()
            });
        }

        public override bool Close(ProviderId lobbyId)
        {
            EnsureInitialized();
            
            LocalLobby.CloseLobby(new CloseLobbyRequest
            {
                LobbyId = lobbyId.ToString()
            });
            
            return true;
        }

        public override bool SetOwner(ProviderId lobbyId, LobbyMember newOwner)
        {
            EnsureInitialized();

            LocalLobby.SetOwner(new SetOwnerRequest
            {
                LobbyId = lobbyId.ToString(),
                NewOwnerId = newOwner.Id.ToString()
            });
            
            return true;
        }

        public override bool KickMember(ProviderId lobbyId, LobbyMember member)
        {
            EnsureInitialized();

            if (!_controller.IsOwner) return false;
            
            LocalLobby.KickMember(new KickMemberRequest
            {
                LobbyId = lobbyId.ToString(),
                KickeeId = member.Id.ToString()
            });
            
            return true;
        }

        public override bool SetLobbyData(ProviderId lobbyId, string key, string value)
        {
            EnsureInitialized();
            
            LocalLobby.SetLobbyData(new LobbyDataRequest
            {
                LobbyId = lobbyId.ToString(),
                Key = key,
                Value = value
            });
            
            return false;
        }

        public override void SetLocalMemberData(ProviderId lobbyId, string key, string value)
        {
            EnsureInitialized();
            
            LocalLobby.SetMemberData(new MemberDataRequest
            {
                LobbyId = lobbyId.ToString(),
                Key = key,
                Value = value
            });
        }

        public override string GetLobbyData(ProviderId lobbyId, string key, string defaultValue)
        {
            EnsureInitialized();
            
            return LocalLobby.GetLobbyDataOrDefault(lobbyId.ToString(), key, defaultValue);
        }

        public override string GetMemberData(ProviderId lobbyId, LobbyMember member, string key, string defaultValue)
        {
            EnsureInitialized();
            return LocalLobby.GetMemberDataOrDefault(lobbyId.ToString(), member.Id.ToString(), key, defaultValue);
        }
        
        private void HandleOtherMemberJoined(MemberJoinedInfo info) => OnOtherMemberJoined?.Invoke(info);
        private void HandleOtherMemberLeft(LeaveInfo info) => OnOtherMemberLeft?.Invoke(info);
        private void HandleLocalMemberKicked(KickInfo info) => OnLocalMemberKicked?.Invoke(info);
        private void HandleReceivedInvitation(LobbyInvite invite) => OnReceivedInvitation?.Invoke(invite);
        private void HandleLobbyDataUpdated(LobbyDataUpdate update) => OnLobbyDataUpdated?.Invoke(update);
        private void HandleMemberDataUpdated(MemberDataUpdate update) => OnMemberDataUpdated?.Invoke(update);
        private void HandleOwnerUpdated(LobbyMember newOwner) => OnOwnerUpdated?.Invoke(newOwner);
        #endregion
    }
}