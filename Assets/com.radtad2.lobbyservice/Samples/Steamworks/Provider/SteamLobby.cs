using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LobbyService.Heartbeat;
using LobbyService.Procedure;
using Steamworks;
using UnityEngine;

namespace LobbyService.Samples.Steam
{
    public class SteamLobby : BaseLobbyProvider,
        ILobbyFriendService,
        ILobbyProcedureService,
        ILobbyChatService,
        ILobbyHeartbeatService,
        ILobbyBrowserService
    {
        // ==== Core ====
        private LobbyController _controller;

        private List<string> _lobbyKeys;
        private List<string> _memberKeys;

        private Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;
        private Callback<LobbyChatMsg_t> _lobbyChatMsg;
        private Callback<LobbyChatUpdate_t> _lobbyChatUpdated;
        private Callback<LobbyDataUpdate_t> _lobbyDataUpdated;
        private Callback<LobbyInvite_t> _lobbyInvited;

        // This is a unique string filter to separate our game from the masses in 480
        private const string GameFilterKey = "pvp_enabled_480_destroyer";
        private const string CloseProcedureKey = "close_lobby";
        private const string KickProcedureKey = "kick_member";
        // ==== End Core ====



        // ==== Friends ====
        private float _interval;
        private FriendDiscoveryFilter _filter;
        private CancellationTokenSource _friendCts;
        // ==== End Friends ====



        // ==== Procedures ====
        private Dictionary<string, LobbyProcedure> _procedureMap = new();
        private const string ProcedureHeader = "[PROC]";
        private struct ProcedureMeta
        {
            public bool Valid;
            public string Key;
            public string[] Arguments;
            public CSteamID? Target; // Null if broadcast
        }
        // ===== End Procedures ====


        // ==== Heartbeat ====
        private const string HeartbeatProcedureKey = "ping_heartbeat";

        private float _lastHeartbeatTime = float.NegativeInfinity;

        private float _heartbeatIntervalSeconds;
        private float _heartbeatTimeoutSeconds;

        private CancellationTokenSource _heartbeatCts;

        private struct HeartbeatMeta
        {
            public float LastPingTime;
            public ProviderId LobbyId;
        }
        private Dictionary<LobbyMember, HeartbeatMeta> _heartbeats = new();
        // ==== End Heartbeat ====



        // ==== Chat ====
        private const string DirectMessageHeader = "[DIRECT]";
        // ==== End Chat ====



        // ==== Browsing ====
        private Dictionary<string, LobbyNumberFilter> _numberFilters = new();
        private Dictionary<string, string> _stringFilters = new();
        private ELobbyDistanceFilter? _distanceFilter;
        private List<(string Key, ILobbySorter Sorter)> _sorters = new();
        private int? _slotsAvailableFilter;
        private int? _limitResponsesFilter;
        // ==== End Browsing ====


        /// <summary>
        /// Creates a mew steam lobby provider.
        /// </summary>
        /// <param name="lobbyKeys">Exhaustive list of keys for all lobby data.</param>
        /// <param name="memberKeys">Exhaustive list of keys for all member data.</param>
        public SteamLobby(List<string> lobbyKeys, List<string> memberKeys)
        {
            _lobbyKeys = lobbyKeys;
            _memberKeys = memberKeys;

            _lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(ProcessAcceptedInvitation);
            _lobbyChatMsg = Callback<LobbyChatMsg_t>.Create(ProcessChatMessage);
            _lobbyChatUpdated = Callback<LobbyChatUpdate_t>.Create(ProcessLobbyChatUpdated);
            _lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(ProcessLobbyDataUpdated);
            _lobbyInvited = Callback<LobbyInvite_t>.Create(ProcessReceivedInvitation);

            RegisterProcedures();
        }

        private void RegisterProcedures()
        {
            // Heartbeat
            RegisterProcedureLocally(new LobbyProcedure
            {
                ExecuteLocally = false,
                InvokePermission = InvokePermission.All,
                Key = HeartbeatProcedureKey,
                Procedure = UpdateHeartbeatProcedure
            });

            // Close
            RegisterProcedureLocally(new LobbyProcedure
            {
                ExecuteLocally = false,
                InvokePermission = InvokePermission.Owner,
                Key = CloseProcedureKey,
                Procedure = CloseProcedure
            });

            // Kick
            RegisterProcedureLocally(new LobbyProcedure
            {
                ExecuteLocally = false,
                InvokePermission = InvokePermission.Owner,
                Key = KickProcedureKey,
                Procedure = KickProcedure
            });
        }

        public override void Dispose()
        {
            _lobbyJoinRequested?.Dispose();
            _lobbyChatMsg?.Dispose();
            _lobbyChatUpdated?.Dispose();
            _lobbyDataUpdated?.Dispose();
            _lobbyInvited?.Dispose();
        }

        #region Core
        public override event Action<MemberJoinedInfo> OnOtherMemberJoined;
        public override event Action<LeaveInfo> OnOtherMemberLeft;
        public override event Action<LobbyDataUpdate> OnLobbyDataUpdated;
        public override event Action<MemberDataUpdate> OnMemberDataUpdated;
        public override event Action<LobbyInvite> OnReceivedInvitation;
        public override event Action<KickInfo> OnLocalMemberKicked;
        public override event Action<LobbyMember> OnOwnerUpdated;

        public override void Initialize(LobbyController controller)
        {
            _controller = controller;
        }

        private bool ValidSteamId(ProviderId id, out CSteamID steamId)
        {
            steamId = CSteamID.Nil;
            if (id == null) return false;

            if (!ulong.TryParse(id.ToString(), out ulong ulongId))
            {
                Debug.LogError($"Invalid lobby id: {id}. Could not parse to ulong.");
                return false;
            }

            steamId = (CSteamID)ulongId;
            return true;
        }

        private List<LobbyMember> GetLobbyMembers(ProviderId lobbyId)
        {
            if (!ValidSteamId(lobbyId, out var steamId)) return null;
            var members = new List<LobbyMember>();

            int memberCount = SteamMatchmaking.GetNumLobbyMembers(steamId);
            for (int i = 0; i < memberCount; i++)
            {
                CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(steamId, i);
                string name = SteamFriends.GetFriendPersonaName(memberId);
                members.Add(new LobbyMember(new ProviderId(memberId.ToString()), name));
            }

            return members;
        }


        private LobbyMember GetOwner(CSteamID lobbyId)
        {
            var ownerId = SteamMatchmaking.GetLobbyOwner(lobbyId);
            return new LobbyMember(new ProviderId(ownerId.ToString()), SteamFriends.GetFriendPersonaName(ownerId));
        }

        private Metadata GetAllLobbyData(ProviderId lobbyId)
        {
            if (!ValidSteamId(lobbyId, out var steamId)) return null;

            var data = new Metadata();
            foreach (var key in _lobbyKeys)
            {
                var value = SteamMatchmaking.GetLobbyData(steamId, key);
                data.Set(key, string.IsNullOrEmpty(value) ? null : value);
            }

            return data;
        }

        private Metadata GetAllMemberData(ProviderId lobbyId, LobbyMember member)
        {
            if (!ValidSteamId(lobbyId, out var steamId)) return null;
            if (!ValidSteamId(member.Id, out var memberId)) return null;

            Metadata data = new();
            foreach (var key in _memberKeys)
            {
                var value = SteamMatchmaking.GetLobbyMemberData(steamId, memberId, key);
                data.Set(key, string.IsNullOrEmpty(value) ? null : value);
            }

            return data;
        }

        private Dictionary<LobbyMember, Metadata> GetAllMemberData(ProviderId lobbyId)
        {
            if (!ValidSteamId(lobbyId, out var steamId)) return null;

            var memberCount = SteamMatchmaking.GetNumLobbyMembers(steamId);

            var allData = new Dictionary<LobbyMember, Metadata>();

            for (int i = 0; i < memberCount; i++)
            {
                var memberId = SteamMatchmaking.GetLobbyMemberByIndex(steamId, i);
                var member = new LobbyMember(new ProviderId(memberId.ToString()), SteamFriends.GetFriendPersonaName(memberId));

                var data = GetAllMemberData(lobbyId, member);
                allData.Add(member, data);
            }

            return allData;
        }

        public override LobbyMember GetLocalUser()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return null;
            }

            var id = SteamUser.GetSteamID();
            var name = SteamFriends.GetPersonaName();
            return new LobbyMember(new ProviderId(id.ToString()), name);
        }

        public override async Task<EnterLobbyResult> CreateAsync(CreateLobbyRequest request)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steamworks is not initialized.");
                return EnterLobbyResult.Failed(EnterFailedReason.BackendNotInitialized);
            }

            var tcs = new TaskCompletionSource<bool>();

            ELobbyType type = request.LobbyType switch
            {
                LobbyType.Public      => ELobbyType.k_ELobbyTypePublic,
                LobbyType.InviteOnly  => ELobbyType.k_ELobbyTypePrivate,
                _                     => throw new ArgumentOutOfRangeException()
            };

            ProviderId lobbyId = null;

            using var callResult = CallResult<LobbyCreated_t>.Create();
            var handle = SteamMatchmaking.CreateLobby(type, request.Capacity);

            callResult.Set(handle, (result, error) =>
            {
                if (error || result.m_eResult is not EResult.k_EResultOK)
                {
                    tcs.TrySetResult(false);
                    return;
                }

                var id = (CSteamID)result.m_ulSteamIDLobby;

                SteamMatchmaking.SetLobbyData(id, SteamLobbyKeys.ServerAddress, SteamUser.GetSteamID().ToString());
                SteamMatchmaking.SetLobbyData(id, SteamLobbyKeys.Name, request.Name);
                SteamMatchmaking.SetLobbyData(id, SteamLobbyKeys.Type, request.LobbyType.ToString());

                // Temp: only for testing during PvP enabled 480 space.
                SteamMatchmaking.SetLobbyData(id, GameFilterKey, "_BLACK_VEIL_");

                lobbyId = new ProviderId(id.ToString());
                tcs.SetResult(true);
            });

            if (!await tcs.Task) return EnterLobbyResult.Failed(EnterFailedReason.General);

            var localMember = GetLocalUser();
            var members = GetLobbyMembers(lobbyId);
            var lobbyData = GetAllLobbyData(lobbyId);
            var memberData = GetAllMemberData(lobbyId);
            var capacity = request.Capacity;

            return EnterLobbyResult.Succeeded
            (
                lobbyId,
                localMember,
                localMember,
                capacity,
                request.LobbyType,
                members,
                lobbyData,
                memberData
            );
        }

        public override async Task<EnterLobbyResult> JoinAsync(JoinLobbyRequest request)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return EnterLobbyResult.Failed(EnterFailedReason.BackendNotInitialized);
            }

            var tcs = new TaskCompletionSource<bool>();
            if (!ValidSteamId(request.LobbyId, out var lobbySteamId)) return EnterLobbyResult.Failed(EnterFailedReason.InvalidId);

            var handle = SteamMatchmaking.JoinLobby(lobbySteamId);
            using var callResult = CallResult<LobbyEnter_t>.Create();

            callResult.Set(handle, (result, error) =>
            {
                if (error || result.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                {
                    tcs.SetResult(false);
                    return;
                }

                tcs.SetResult(true);
            });

            if (!await tcs.Task) return EnterLobbyResult.Failed(EnterFailedReason.General);

            var providerLobbyId = new ProviderId(lobbySteamId.ToString());
            var owner = GetOwner(lobbySteamId);

            var localMember = GetLocalUser();
            var members = GetLobbyMembers(providerLobbyId);
            var lobbyData = GetAllLobbyData(providerLobbyId);
            var memberData = GetAllMemberData(providerLobbyId);

            var capacity = SteamMatchmaking.GetLobbyMemberLimit(lobbySteamId);
            var type = SteamMatchmaking.GetLobbyData(lobbySteamId, SteamLobbyKeys.Type);
            Enum.TryParse<LobbyType>(type, out var strongType);

            return EnterLobbyResult.Succeeded
            (
                providerLobbyId,
                owner,
                localMember,
                capacity,
                strongType,
                members,
                lobbyData,
                memberData
            );
        }

        private void ProcessAcceptedInvitation(GameLobbyJoinRequested_t request)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return;
            }
            _controller.Join(new JoinLobbyRequest
            {
                LobbyId = new ProviderId(request.m_steamIDLobby.ToString())
            });
        }

        private void ProcessReceivedInvitation(LobbyInvite_t invite)
        {
            var name = SteamFriends.GetFriendPersonaName((CSteamID)invite.m_ulSteamIDUser);
            var sender = new LobbyMember(new ProviderId(invite.m_ulSteamIDUser.ToString()), name);

            OnReceivedInvitation?.Invoke(new LobbyInvite
            {
                LobbyId = new ProviderId(invite.m_ulSteamIDLobby.ToString()),
                Sender = sender
            });
        }

        public override void Leave(ProviderId lobbyId)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return;
            }

            if (!ValidSteamId(lobbyId, out var id)) return;
            SteamMatchmaking.LeaveLobby(id);
        }

        public override bool Close(ProviderId lobbyId)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return false;
            }

            if (!ValidSteamId(lobbyId, out var id)) return false;


            var result = Broadcast
            (
                lobbyId,
                CloseProcedureKey,
                lobbyId.ToString()
            );

            if (!result) return false;

            SteamMatchmaking.LeaveLobby(id);
            return true;
        }

        public override bool SendInvite(ProviderId lobbyId, LobbyMember member)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return false;
            }

            if (!ValidSteamId(lobbyId, out var lobbySteamId)) return false;
            if (!ValidSteamId(member.Id, out var memberSteamId)) return false;

            SteamMatchmaking.InviteUserToLobby(lobbySteamId, memberSteamId);
            return true;
        }

        public override bool SetOwner(ProviderId lobbyId, LobbyMember newOwner)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return false;
            }

            if (!ValidSteamId(lobbyId, out var lobbySteamId)) return false;
            if (!ValidSteamId(newOwner.Id, out var newOwnerSteamId)) return false;

            return SteamMatchmaking.SetLobbyOwner(lobbySteamId, newOwnerSteamId);
        }

        public override bool KickMember(ProviderId lobbyId, LobbyMember member)
        {
            if (!ValidSteamId(lobbyId, out var steamId)) return false;
            if (!ValidSteamId(member.Id, out var memberId)) return false;

            if (SteamMatchmaking.GetLobbyOwner(steamId) != SteamUser.GetSteamID()) return false;

            var sent = Broadcast(
                lobbyId,
                KickProcedureKey,
                lobbyId.ToString(),
                memberId.ToString(),
                nameof(KickReason.General)
            );

            return sent;
        }

        // Kick member procedure: Expects 3 args: ulong: lobby id | ulong target id | KickReason: reason
        private async Task KickProcedure(string[] args)
        {
            if (args.Length < 3) return;
            if (!ValidSteamId(new ProviderId(args[0]), out var steamId)) return;
            if (!ValidSteamId(new ProviderId(args[1]), out var memberId)) return;

            if (!Enum.TryParse<KickReason>(args[2], out var reason))
            {
                reason = KickReason.General;
            }

            var info = new KickInfo
            {
                Reason = reason
            };

            if (memberId == SteamUser.GetSteamID())
            {
                SteamMatchmaking.LeaveLobby(steamId);
                OnLocalMemberKicked?.Invoke(info);
            }
            else
            {
                var member = new LobbyMember(new ProviderId(memberId.ToString()),
                    SteamFriends.GetFriendPersonaName(memberId));
                OnOtherMemberLeft?.Invoke(new LeaveInfo
                {
                    Member = member,
                    LeaveReason = LeaveReason.Kicked,
                    KickInfo = info
                });
            }

            await Task.CompletedTask;
        }

        public override bool SetLobbyData(ProviderId lobbyId, string key, string value)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return false;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return false;
            return SteamMatchmaking.SetLobbyData(steamId, key, value);
        }

        public override void SetLocalMemberData(ProviderId lobbyId, string key, string value)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return;
            SteamMatchmaking.SetLobbyMemberData(steamId, key, value);
        }

        public override string GetLobbyData(ProviderId lobbyId, string key, string defaultValue)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return defaultValue;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return defaultValue;

            var result = SteamMatchmaking.GetLobbyData(steamId, key);

            return string.IsNullOrEmpty(result) ? defaultValue : result;
        }

        public override string GetMemberData(ProviderId lobbyId, LobbyMember member, string key, string defaultValue)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return defaultValue;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return defaultValue;
            if (!ValidSteamId(member.Id, out var memberId)) return defaultValue;

            var result = SteamMatchmaking.GetLobbyMemberData(steamId, memberId, key);

            return string.IsNullOrEmpty(result) ? defaultValue : result;
        }

        private void ProcessLobbyChatUpdated(LobbyChatUpdate_t update)
        {
            const int enteredMask = 0x0001;
            const int leftMask    = 0x0002;

            bool entered = (update.m_rgfChatMemberStateChange & enteredMask) != 0;
            bool left    = (update.m_rgfChatMemberStateChange & leftMask)    != 0;

            var userId = update.m_ulSteamIDUserChanged;
            var displayName = SteamFriends.GetFriendPersonaName((CSteamID)userId);
            LobbyMember member = new LobbyMember(new ProviderId(userId.ToString()), displayName);

            var lobbyId = new ProviderId(update.m_ulSteamIDLobby.ToString());

            if (entered)
            {
                OnOtherMemberJoined?.Invoke(new MemberJoinedInfo
                {
                    Member = member,
                    Data = GetAllMemberData(lobbyId, member)
                });
            }
            else if (left)
            {
                OnOtherMemberLeft?.Invoke(new LeaveInfo
                {
                    Member = member,
                    LeaveReason = LeaveReason.UserRequested,
                    KickInfo = null
                });
            }
        }

        private void ProcessLobbyDataUpdated(LobbyDataUpdate_t update)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return;
            }

            if (update.m_ulSteamIDLobby == update.m_ulSteamIDMember)
            {
                var owner = GetOwner((CSteamID)update.m_ulSteamIDLobby);
                OnOwnerUpdated?.Invoke(owner);

                var data = new LobbyDataUpdate
                {
                    Data = GetAllLobbyData(new ProviderId(update.m_ulSteamIDLobby.ToString()))
                };

                OnLobbyDataUpdated?.Invoke(data);
            }
            else
            {
                var id = new ProviderId(update.m_ulSteamIDMember.ToString());
                var name = SteamFriends.GetFriendPersonaName((CSteamID)update.m_ulSteamIDMember);
                var member = new LobbyMember(id, name);
                var data = new MemberDataUpdate
                {
                    Member = member,
                    Data = GetAllMemberData(new ProviderId(update.m_ulSteamIDLobby.ToString()), member)
                };

                OnMemberDataUpdated?.Invoke(data);
            }
        }

        private async Task CloseProcedure(string[] args)
        {
            if (args.Length < 1) return;

            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return;
            }

            if (!ValidSteamId(new ProviderId(args[0]), out var steamId)) return;

            SteamMatchmaking.LeaveLobby(steamId);

            OnLocalMemberKicked?.Invoke(new KickInfo
            {
                Reason = KickReason.LobbyClosed
            });

            await Task.CompletedTask;
        }
        #endregion

        #region Friends
        public event Action<List<LobbyMember>> FriendsUpdated;

        public void StartFriendPolling(FriendDiscoveryFilter filter, float intervalSeconds, CancellationToken token = default)
        {
            if (!SteamManager.Initialized) return;

            _interval = intervalSeconds;
            _filter = filter;

            _friendCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_friendCts.Token, token);

            _ = DiscoverFriends(cts.Token);
        }

        public void SetFriendPollingInterval(float intervalSeconds)=> _interval = intervalSeconds;

        public void SetFriendPollingFilter(FriendDiscoveryFilter filter) => _filter = filter;

        public void StopFriendPolling()
        {
            _friendCts?.Cancel();
            _friendCts?.Dispose();
            _friendCts = null;
        }

        public async Task<Texture2D> GetFriendAvatar(LobbyMember member, CancellationToken token = default)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return null;
            }
            
            if (!ValidSteamId(member.Id, out var id)) return null;

            int handle = SteamFriends.GetMediumFriendAvatar(id);
            
            if (!SteamUtils.GetImageSize(handle, out uint width, out uint height))
            {
                Debug.LogError("Failed to get image size for avatar.");
                return null;
            }
            
            var imageBuffer = new byte[width * height * 4];
            if (!SteamUtils.GetImageRGBA(handle, imageBuffer, (int)(width * height * 4)))
            {
                Debug.LogError("Failed to get RGBA data for avatar.");
                return null;
            }

            var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(imageBuffer);
            texture.Apply();

            await Task.CompletedTask;
            return texture;
        }

        private async Task DiscoverFriends(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    List<LobbyMember> members = new();
                    if (!SteamManager.Initialized)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_interval), token);
                        continue;
                    }

                    var flags = EFriendFlags.k_EFriendFlagImmediate;
                    var count = SteamFriends.GetFriendCount(flags);
                    var results = new CSteamID[count];

                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetFriendByIndex(i, flags);
                    }

                    foreach (var id in results)
                    {
                        var state = SteamFriends.GetFriendPersonaState(id);

                        if (_filter is FriendDiscoveryFilter.All || state is EPersonaState.k_EPersonaStateOnline)
                        {
                            members.Add(new LobbyMember(new ProviderId(id.ToString()), SteamFriends.GetFriendPersonaName(id)));
                        }
                    }

                    FriendsUpdated?.Invoke(members);
                    await Task.Delay(TimeSpan.FromSeconds(_interval), token);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion

        #region Heartbeat
        public event Action<HeartbeatTimeout> OnHeartbeatTimeout;
        public void StartOwnHeartbeat(ProviderId lobbyId, float intervalSeconds, float othersTimeoutSeconds)
        {
            if (_heartbeatCts != null) StopHeartbeatAndClearSubscriptions();

            _heartbeatIntervalSeconds = intervalSeconds;
            _heartbeatTimeoutSeconds = othersTimeoutSeconds;
            _heartbeatCts = new CancellationTokenSource();
            _ = HeartbeatLoop(lobbyId);
        }

        public void StopHeartbeatAndClearSubscriptions()
        {
            if (!_heartbeatCts?.IsCancellationRequested ?? false) _heartbeatCts?.Cancel();

            _heartbeats.Clear();
            _heartbeatCts?.Dispose();
            _heartbeatCts = null;
        }

        public void SubscribeToHeartbeat(ProviderId lobbyId, LobbyMember member)
        {
            Debug.Log($"Subbing to {member} at time {Time.time}");
            _heartbeats[member] = new HeartbeatMeta
            {
                LobbyId = lobbyId,
                LastPingTime = Time.time + 10, // 10s grace period
            };
        }

        public void UnsubscribeFromHeartbeat(ProviderId lobbyId, LobbyMember member)
        {
            _heartbeats.Remove(member);
        }

        private async Task HeartbeatLoop(ProviderId lobbyId)
        {
            try
            {
                while (!_heartbeatCts.IsCancellationRequested)
                {
                    float now = Time.time;

                    // Send own heartbeat
                    if (now > _lastHeartbeatTime + _heartbeatIntervalSeconds)
                    {
                        Broadcast(
                            lobbyId,
                            HeartbeatProcedureKey,
                            lobbyId.ToString(),
                            GetLocalUser().Id.ToString()
                        );

                        _lastHeartbeatTime = now;
                    }

                    // Check others' heartbeats
                    var timedOut = new List<LobbyMember>();
                    foreach (var kvp in _heartbeats)
                    {
                        if (now > kvp.Value.LastPingTime + _heartbeatTimeoutSeconds)
                        {
                            Debug.Log($"{kvp.Key} timed out because now is {now} and last ping time was {kvp.Value.LastPingTime}");
                            timedOut.Add(kvp.Key);
                        }
                    }

                    foreach (var member in timedOut)
                    {
                        var senderLobbyId = _heartbeats[member].LobbyId;

                        // Prevent duplicating events for continuously timeout members
                        UnsubscribeFromHeartbeat(null, member);

                        OnHeartbeatTimeout?.Invoke(new HeartbeatTimeout
                        {
                            LobbyId = senderLobbyId,
                            Member = member
                        });
                    }

                    // How often we test, not how often we broadcast
                    await Task.Delay(TimeSpan.FromSeconds(0.5f), _heartbeatCts.Token);
                }
            }
            catch (OperationCanceledException) { /* Ignored */ }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // Lobby procedure - Expects two args: ulong: lobbyId | ulong: memberId
        private async Task UpdateHeartbeatProcedure(string[] args)
        {
            if (args.Length < 2) return;

            var lobbyIdStr = args[0];
            var memberIdStr = args[1];

            if (!ValidSteamId(new ProviderId(lobbyIdStr), out var _)) return;
            if (!ValidSteamId(new ProviderId(memberIdStr), out var memberId)) return;

            var lobbyMember = new LobbyMember(new ProviderId(memberIdStr), SteamFriends.GetFriendPersonaName(memberId));

            if (_heartbeats.ContainsKey(lobbyMember))
            {
                Debug.Log($"Updating {lobbyMember}'s time to {Time.time}");
                _heartbeats[lobbyMember] = new HeartbeatMeta
                {
                    LobbyId = new ProviderId(lobbyIdStr),
                    LastPingTime = Time.time
                };
            }

            await Task.CompletedTask;
        }
        #endregion

        #region Procedures
        public void RegisterProcedureLocally(LobbyProcedure procedure)
        {
            _procedureMap[procedure.Key] = procedure;
        }

        public bool Broadcast(ProviderId lobbyId, string key, params string[] args)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return false;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return false;

            bool isOwner = _controller.IsOwner;

            if (!HasPermissionToSend(isOwner, _procedureMap[key]))
            {
                Debug.LogWarning($"Attempted to invoke procedure with key '{key}' but did not have permission.");
                return false;
            }

            var meta = new ProcedureMeta
            {
                Valid = true,
                Key = key,
                Arguments = args,
                Target = null
            };

            string strEncoded = EncodeProcedure(meta);

            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(strEncoded);

            return SteamMatchmaking.SendLobbyChatMsg(steamId, encoded, encoded.Length);
        }

        public bool Target(ProviderId lobbyId, LobbyMember target, string key, params string[] args)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return false;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return false;
            if (!ValidSteamId(target.Id, out var memberId)) return false;

            bool isOwner = GetLocalUser().Id.Equals(new ProviderId(SteamMatchmaking.GetLobbyOwner(steamId).ToString()));

            if (!HasPermissionToSend(isOwner, _procedureMap[key]))
            {
                Debug.LogWarning($"Attempted to invoke procedure with key '{key}' but did not have permission.");
                return false;
            }

            var meta = new ProcedureMeta
            {
                Valid = true,
                Key = key,
                Arguments = args,
                Target = memberId
            };

            string strEncoded = EncodeProcedure(meta);
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(strEncoded);

            return SteamMatchmaking.SendLobbyChatMsg(steamId, encoded, encoded.Length);
        }

        private bool HasPermissionToSend(bool isOwner, LobbyProcedure procedure)
        {
            switch (procedure.InvokePermission)
            {
                case InvokePermission.Owner:
                    if (!isOwner) return false;
                    break;
                case InvokePermission.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private void ProcessChatMessage(LobbyChatMsg_t msg)
        {
            const int normal = 1;

            if (msg.m_eChatEntryType is not normal) return;

            var steamId = (CSteamID)msg.m_ulSteamIDLobby;
            var msgId = (int)msg.m_iChatID;
            var bufferSize = 512;
            var buffer = new byte[bufferSize];

            int size = SteamMatchmaking.GetLobbyChatEntry(steamId, msgId, out var senderId, buffer, bufferSize, out var type);

            if (size <= 0) return;

            // Convert the byte array into a string (UTF-8)
            string text = System.Text.Encoding.UTF8.GetString(buffer, 0, size);
            text = text.Trim();

            var procedure = DecodeProcedure(text);

            if (procedure.Valid)
            {
                HandleProcedure(procedure, msg);
                return;
            }

            // We now know its a chat message. Check direct first:
            var sender = new LobbyMember(new ProviderId(senderId.ToString()), SteamFriends.GetFriendPersonaName(senderId));

            // If direct it will follow the format HEADER:(ulong)targetId:message
            var parts = text.Split(new[] { ':' }, 3, StringSplitOptions.None);

            if (parts.Length == 3 && parts[0] == DirectMessageHeader)
            {
                if (!ValidSteamId(new ProviderId(parts[1]), out var targetId))
                    return;

                // Only deliver to sender or intended recipient
                var selfId = SteamUser.GetSteamID();
                if (selfId != targetId && selfId != senderId)
                    return;

                var message = parts[2]; // keeps any ':' inside the message

                OnDirectMessageReceived?.Invoke(new LobbyChatMessage
                {
                    Content = message,
                    Sender = sender,
                    Type = LobbyMessageType.Direct
                });
                return;
            }

            // Regular chat message
            OnChatMessageReceived?.Invoke(new LobbyChatMessage
            {
                Content = text,
                Sender = sender,
                Type = LobbyMessageType.General
            });
        }

        private void HandleProcedure(ProcedureMeta meta, LobbyChatMsg_t msg)
        {
            if (!meta.Valid) return;

            if (!_procedureMap.TryGetValue(meta.Key, out var procedure))
            {
                Debug.LogWarning($"Could not find a procedure with the key {meta.Key}");
                return;
            }

            var localId = SteamUser.GetSteamID();
            bool sentByUs = (CSteamID)msg.m_ulSteamIDUser == localId;
            bool isLocalExecution = sentByUs && procedure.ExecuteLocally;
            bool isRemoteBroadcast = !meta.Target.HasValue && !sentByUs;
            bool isExplicitTarget = meta.Target == localId;

            bool isTarget = isLocalExecution || isRemoteBroadcast || isExplicitTarget;

            if (isTarget) RunProcedure(procedure, meta.Arguments);
        }

        private async void RunProcedure(LobbyProcedure procedure, string[] args)
        {
            try
            {
               await procedure.Procedure(args);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private string EncodeProcedure(ProcedureMeta procedureMeta)
        {
            string targetPart = procedureMeta.Target.HasValue ? procedureMeta.Target.Value.ToString() : "";

            // Join arguments with '|'
            string argsPart = string.Join("|", procedureMeta.Arguments);

            // Format: Header:Key:Target:arg1|arg2|...
            return $"{ProcedureHeader}:{procedureMeta.Key}:{targetPart}:{argsPart}";
        }

        private ProcedureMeta DecodeProcedure(string rawText)
        {
            var meta = new ProcedureMeta { Valid = false };

            if (string.IsNullOrEmpty(rawText)) return meta;

            // Format: Header:Key:Target:arg1|arg2|...
            // Split into four parts: Header, Key, Target, Args
            string[] parts = rawText.Split(new[] { ':' }, 4); // limit to 4 splits
            if (parts.Length < 3) return meta;                // need at least Header, Key and Target

            if (parts[0] != ProcedureHeader) return meta;
            meta.Key = parts[1];

            // Parse Target SteamID if present
            if (string.IsNullOrEmpty(parts[2]))
                meta.Target = null;
            else
                meta.Target = new CSteamID(ulong.Parse(parts[2]));

            // Parse arguments
            meta.Arguments = parts.Length == 4 && !string.IsNullOrEmpty(parts[3])
                ? parts[3].Split('|')
                : Array.Empty<string>();

            meta.Valid = true;
            return meta;
        }
        #endregion

        #region Chat
        public event Action<LobbyChatMessage> OnChatMessageReceived;
        public event Action<LobbyChatMessage> OnDirectMessageReceived;
        public void SendChatMessage(ProviderId lobbyId, string message)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return;

            var cmd = DecodeProcedure(message);
            if (cmd.Valid)
            {
                Debug.LogWarning("User attempted to invoke a procedure through chat! Caught and ignored.");
                return;
            }

            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(message);
            SteamMatchmaking.SendLobbyChatMsg(steamId, encoded, encoded.Length);
        }

        public bool SendDirectMessage(ProviderId lobbyId, LobbyMember member, string message)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return false;
            }

            if (!ValidSteamId(lobbyId, out var steamId)) return false;
            if (!ValidSteamId(member.Id, out var memberId)) return false;

            var cmd = DecodeProcedure(message);
            if (cmd.Valid)
            {
                Debug.LogWarning("User attempted to invoke a procedure through chat! Caught and ignored.");
                return false;
            }

            byte[] encoded = System.Text.Encoding.UTF8.GetBytes($"{DirectMessageHeader}:{memberId}:{message}");
            return SteamMatchmaking.SendLobbyChatMsg(steamId, encoded, encoded.Length);
        }
        #endregion

        #region Browsing

        public LobbyBrowserCapabilities Capabilities { get; } =
            LobbyBrowserCapabilities.StringFilter |
            LobbyBrowserCapabilities.NumberFilter |
            LobbyBrowserCapabilities.DistanceFilter |
            LobbyBrowserCapabilities.SlotsAvailableFilter |
            LobbyBrowserCapabilities.LimitResponseCountFilter |
            LobbyBrowserCapabilities.Sorting;

        public async Task<List<LobbyDescriptor>> Browse()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Using steam lobby provider but steam is not initialized.");
                return new List<LobbyDescriptor>();
            }

            ApplyFilters();

            var tcs = new TaskCompletionSource<uint>();
            using var callResult = CallResult<LobbyMatchList_t>.Create();
            var handle = SteamMatchmaking.RequestLobbyList();

            callResult.Set(handle, (result, error) =>
            {
                tcs.TrySetResult(error ? 0 : result.m_nLobbiesMatching);
            });

            var numLobbies = await tcs.Task;
            var result = new List<LobbyDescriptor>();

            for (int i = 0; i < numLobbies; i++)
            {
                var steamId = SteamMatchmaking.GetLobbyByIndex(i);

                result.Add(new LobbyDescriptor
                {
                    LobbyId = new ProviderId(steamId.ToString()),
                    Name = SteamMatchmaking.GetLobbyData(steamId, SteamLobbyKeys.Name),
                    MemberCount = SteamMatchmaking.GetNumLobbyMembers(steamId),
                    MaxMembers = SteamMatchmaking.GetLobbyMemberLimit(steamId),

                    // Steam only returns lobbies that are public or invisible, and also joinable.
                    IsJoinable = true
                });
            }

            ApplySorters(result);

            return result;
        }

        private void ApplyFilters()
        {
            foreach (var kvp in _numberFilters)
            {
                ELobbyComparison comparison = kvp.Value.ComparisonType switch
                {
                    ComparisonType.NotEqual           => ELobbyComparison.k_ELobbyComparisonNotEqual,
                    ComparisonType.LessThan           => ELobbyComparison.k_ELobbyComparisonLessThan,
                    ComparisonType.LessThanOrEqual    => ELobbyComparison.k_ELobbyComparisonEqualToOrLessThan,
                    ComparisonType.Equal              => ELobbyComparison.k_ELobbyComparisonEqual,
                    ComparisonType.GreaterThan        => ELobbyComparison.k_ELobbyComparisonGreaterThan,
                    ComparisonType.GreaterThanOrEqual => ELobbyComparison.k_ELobbyComparisonEqualToOrGreaterThan,
                    _                                 => throw new ArgumentOutOfRangeException()
                };

                SteamMatchmaking.AddRequestLobbyListNumericalFilter(kvp.Key, kvp.Value.Value, comparison);
            }

            foreach (var kvp in _stringFilters)
            {
                SteamMatchmaking.AddRequestLobbyListStringFilter(kvp.Key, kvp.Value, ELobbyComparison.k_ELobbyComparisonEqual);
            }

            if (_distanceFilter.HasValue)
            {
                SteamMatchmaking.AddRequestLobbyListDistanceFilter(_distanceFilter.Value);
            }

            if (_slotsAvailableFilter.HasValue)
            {
                SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(_slotsAvailableFilter.Value);
            }

            if (_limitResponsesFilter.HasValue)
            {
                SteamMatchmaking.AddRequestLobbyListResultCountFilter(_limitResponsesFilter.Value);
            }
        }

        private void ApplySorters(List<LobbyDescriptor> lobbies)
        {
            var comparer = new CompositeLobbySorterComparer(_sorters.Select(s => s.Sorter));

            lobbies.Sort(comparer);
        }

        public void AddNumberFilter(LobbyNumberFilter filter)
        {
            _numberFilters[filter.Key] = filter;
        }

        public void AddStringFilter(LobbyStringFilter filter)
        {
            _stringFilters[filter.Key] = filter.Value;
        }

        public void RemoveNumberFilter(string key)
        {
            _numberFilters.Remove(key);
        }

        public void RemoveStringFilter(string key)
        {
            _stringFilters.Remove(key);
        }

        public void RemoveFilter(string key)
        {
            _numberFilters.Remove(key);
            _stringFilters.Remove(key);
        }

        public void SetDistanceFilter(LobbyDistance value)
        {
            _distanceFilter = value switch
            {
                LobbyDistance.Default   => ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault,
                LobbyDistance.Near      => ELobbyDistanceFilter.k_ELobbyDistanceFilterClose,
                LobbyDistance.Far       => ELobbyDistanceFilter.k_ELobbyDistanceFilterFar,
                LobbyDistance.WorldWide => ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide,
                _                       => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public void ClearDistanceFilter()
        {
            _distanceFilter = null;
        }

        public void SetSlotsAvailableFilter(int slots)
        {
            _slotsAvailableFilter = slots;
        }

        public void ClearSlotsAvailableFilter()
        {
            _slotsAvailableFilter = null;
        }

        public void SetLimitResponsesFilter(int limit)
        {
            _limitResponsesFilter = limit;
        }

        public void ClearLimitResponsesFilter()
        {
            _limitResponsesFilter = null;
        }

        public void ClearAllFilters()
        {
            _numberFilters.Clear();
            _stringFilters.Clear();

            _distanceFilter = null;
            _limitResponsesFilter = null;
            _slotsAvailableFilter = null;
        }

        public void AddSorter(LobbyKeyAndSorter kvp)
        {
            _sorters.Add((kvp.Key, kvp.Sorter));
        }

        public void RemoveSorter(string key)
        {
            int index = _sorters.FindIndex(s => s.Key == key);
            if (index >= 0) _sorters.RemoveAt(index);
        }

        public void ClearSorters()
        {
            _sorters.Clear();
        }
        #endregion
    }
}
