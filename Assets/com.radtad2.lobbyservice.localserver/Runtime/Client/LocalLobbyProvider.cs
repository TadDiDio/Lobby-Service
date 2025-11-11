using System;
using System.Threading.Tasks;
using UnityEngine;

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
        public override void Initialize(LobbyController controller)
        {
            throw new NotImplementedException();
        }

        public override LobbyMember GetLocalUser()
        {
            if (!LocalLobby.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return null;
            }

            return LocalLobby.GetLocalUser();
        }

        public override async Task<EnterLobbyResult> CreateAsync(LobbyService.CreateLobbyRequest request)
        {
            if (!LocalLobby.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return EnterLobbyResult.Failed(EnterFailedReason.BackendNotInitialized);
            }
            
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

        public override Task<EnterLobbyResult> JoinAsync(LobbyService.JoinLobbyRequest request)
        {
            throw new NotImplementedException();
        }

        public override bool SendInvite(ProviderId lobbyId, LobbyMember member)
        {
            throw new NotImplementedException();
        }

        public override void Leave(ProviderId lobbyId)
        {
            throw new NotImplementedException();
        }

        public override bool Close(ProviderId lobbyId)
        {
            throw new NotImplementedException();
        }

        public override bool SetOwner(ProviderId lobbyId, LobbyMember newOwner)
        {
            throw new NotImplementedException();
        }

        public override bool KickMember(ProviderId lobbyId, LobbyMember member)
        {
            throw new NotImplementedException();
        }

        public override bool SetLobbyData(ProviderId lobbyId, string key, string value)
        {
            throw new NotImplementedException();
        }

        public override void SetLocalMemberData(ProviderId lobbyId, string key, string value)
        {
            throw new NotImplementedException();
        }

        public override string GetLobbyData(ProviderId lobbyId, string key, string defaultValue)
        {
            throw new NotImplementedException();
        }

        public override string GetMemberData(ProviderId lobbyId, LobbyMember member, string key, string defaultValue)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}