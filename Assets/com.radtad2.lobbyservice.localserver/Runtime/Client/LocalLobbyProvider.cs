using System;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class LocalLobbyProvider : BaseLobbyProvider
    {
        public override event Action<MemberJoinedInfo> OnOtherMemberJoined;
        public override event Action<LeaveInfo> OnOtherMemberLeft;
        public override event Action<KickInfo> OnLocalMemberKicked;
        public override event Action<LobbyInvite> OnReceivedInvitation;
        public override event Action<LobbyDataUpdate> OnLobbyDataUpdated;
        public override event Action<MemberDataUpdate> OnMemberDataUpdated;
        public override event Action<LobbyMember> OnOwnerUpdated;
        
        private LobbyController _controller;
        public override void Initialize(LobbyController controller)
        {
            EnsureInitialized();

            LocalLobby.OnOtherMemberJoined += OnOtherMemberJoined;
            LocalLobby.OnOtherMemberLeft += OnOtherMemberLeft;
            LocalLobby.OnLocalMemberKicked += OnLocalMemberKicked;
            LocalLobby.OnReceivedInvitation += OnReceivedInvitation;
            LocalLobby.OnLobbyDataUpdated += OnLobbyDataUpdated;
            LocalLobby.OnMemberDataUpdated += OnMemberDataUpdated;
            LocalLobby.OnOwnerUpdated += OnOwnerUpdated;
            
            _controller = controller;
        }

        public override void Dispose()
        {
            LocalLobby.OnOtherMemberJoined -= OnOtherMemberJoined;
            LocalLobby.OnOtherMemberLeft -= OnOtherMemberLeft;
            LocalLobby.OnLocalMemberKicked -= OnLocalMemberKicked;
            LocalLobby.OnReceivedInvitation -= OnReceivedInvitation;
            LocalLobby.OnLobbyDataUpdated -= OnLobbyDataUpdated;
            LocalLobby.OnMemberDataUpdated -= OnMemberDataUpdated;
            LocalLobby.OnOwnerUpdated -= OnOwnerUpdated;
        }
        
        private void EnsureInitialized()
        {
            if (!LocalLobby.Initialized)
                throw new InvalidOperationException("LocalLobby must be initialized before use");
        }
        
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
                Capacity = request.Capacity
            });

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
                snapshot.MemberData.ToMemberData()
            );
        }

        public override async Task<EnterLobbyResult> JoinAsync(LobbyService.JoinLobbyRequest request)
        {
            EnsureInitialized();

            var result = await LocalLobby.Join(new JoinLobbyRequest
            {
                LobbyId = request.LobbyId.ToString()
            });
            
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
                snapshot.MemberData.ToMemberData()
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
            
            LocalLobby.KickMember(new KickMemberRequest
            {
                LobbyId = lobbyId.ToString(),
                KickeeId = member.Id.ToString()
            });
            
            return false;
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
    }
}