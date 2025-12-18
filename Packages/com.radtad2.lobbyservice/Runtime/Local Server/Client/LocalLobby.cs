using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
    
namespace LobbyService.LocalServer
{
    /// <summary>
    /// API for interacting with the local lobby server.
    /// </summary>
    public static class LocalLobby
    {
        public static bool Initialized { get; private set; }
        private static Task<bool> _initTask;

        public const string PortKey = "LobbyService.LocalServer.Port";

        private static LocalLobbyClient _client;
        private static LobbyMember _localUser;
        private static Dictionary<string, LobbySnapshot> _cachedLobbies;
        
        private static CancellationTokenSource _shutdownCts;

        static LocalLobby()
        {
            Application.quitting += Shutdown;
        }
        
        public static async Task<bool> WaitForInitializationAsync(CancellationToken token)
        {
            if (Initialized) return true;
            if (_initTask != null) return await _initTask;

            _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            
            _initTask = InitializeAsync(_shutdownCts.Token);
            return await _initTask;
        }
        
        private static async Task<bool> InitializeAsync(CancellationToken token)
        {
            if (Initialized) return true;
            
            try
            {
                ConsoleRedirector.Redirect();
                
                _cachedLobbies = new Dictionary<string, LobbySnapshot>();
                _client = new LocalLobbyClient(IPAddress.Loopback, LobbyConfig.Port);
                if (!await _client.ConnectAsync(token)) return false;

                var welcome = await GetResponseAsync<WelcomeResponse>(new ConnectRequest(), 3f, token);

                if (token.IsCancellationRequested) return false;
                
                if (welcome.Error is not Error.Ok) throw new Exception($"Failed to receive welcome response with error: {welcome.Error}");

                _client.OnMessageReceived += HandleMessage;
                _localUser = welcome.Response.LocalMember.ToLobbyMember();
                
                Initialized = true;
                LobbyLogger.Log($"Initialized as user {_localUser}");
                Application.quitting += Shutdown;
                return true;
            }
            catch (OperationCanceledException)
            {
                /* Ignored */
                return false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
            finally
            {
                if (!Initialized) _initTask = null;
                _shutdownCts = null;
            }
        }

        public static void Shutdown()
        {
            if (!Initialized) return;
            
            LobbyLogger.Log("Shutting down...");
            Initialized = false;
            Application.quitting -= Shutdown;
            
            if (_shutdownCts is { IsCancellationRequested: false })
            {
                _shutdownCts.Cancel();
                _shutdownCts.Dispose();
                _shutdownCts = null;
            }
            
            _initTask = null;
            Initialized = false;
            _localUser = null;

            _cachedLobbies = null;
            _client?.Dispose();
            
            ConsoleRedirector.Return();
        }
        
        private static void SendCommand(IRequest request)
        {
            var message = Message.CreateRequest(request);
            _client.Send(message);
        }
        
        private static async Task<RequestResponse<T>> GetResponseAsync<T>(IRequest request, float timeoutSeconds, CancellationToken token = default) where T : IResponse
        {
            try
            {
                var message = Message.CreateRequest(request);
                var task = _client.WaitForResponse(message.RequestId, timeoutSeconds, token);

                _client.Send(message);

                var response = await task;

                if (response == null)
                {
                    return new RequestResponse<T>
                    {
                        Error = Error.Timeout,
                        Response = default
                    };
                }

                if (!MessageTypeRegistry.TryGetType(response.Type, out var type) ||
                    response.Payload.ToObject(type) is not T typedResponse)
                {
                    return new RequestResponse<T>
                    {
                        Error = Error.Serialization,
                        Response = default
                    };
                }

                return new RequestResponse<T>
                {
                    Error = response.Error,
                    Response = typedResponse,
                };
            }
            catch (OperationCanceledException)
            {
                return new RequestResponse<T>
                {
                    Error = Error.Cancelled,
                    Response = default
                };
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new RequestResponse<T>
                {
                    Error = Error.Unknown,
                    Response = default
                };
            }
        }

        private static void CacheSnapshot(LobbySnapshot snapshot)
        {
            _cachedLobbies[snapshot.LobbyId.ToString()] = snapshot;
        }
        
        private static void HandleMessage(Message message)
        {
            if (!MessageTypeRegistry.TryGetType(message.Type, out var type)) return;

            if (message.Payload.ToObject(type) is not IEvent evt) return;

            switch (evt)
            {
                case OtherMemberJoinedEvent joined:
                    OnOtherMemberJoined?.Invoke(new MemberJoinedInfo
                    {
                        Member = joined.Member.ToLobbyMember(),
                        Data = joined.Metadata.ToMeta()
                    });
                    break;
                case OtherMemberLeftEvent left:
                    OnOtherMemberLeft?.Invoke(new LeaveInfo
                    {
                        Member = left.Member.ToLobbyMember(),
                        LeaveReason = LeaveReason.UserRequested,
                        KickInfo = null
                    });
                    break;
                case LocalMemberKickedEvent kicked:
                    OnLocalMemberKicked?.Invoke(new KickInfo
                    {
                        Reason = (KickReason)kicked.KickReason
                    });
                    break;
                case ReceivedInviteEvent invite:
                    OnReceivedInvitation?.Invoke(new LobbyInvite
                    {
                        LobbyId = new ProviderId(invite.LobbyId.ToString()),
                        Sender = invite.Sender.ToLobbyMember()
                    });
                    break;
                case LobbyDataUpdateEvent update:
                    OnLobbyDataUpdated?.Invoke(new LobbyDataUpdate
                    {
                        Data = update.Metadata.ToMeta()
                    });
                    break;
                case MemberDataUpdateEvent update:
                    OnMemberDataUpdated?.Invoke(new MemberDataUpdate
                    {
                        Member = update.Member.ToLobbyMember(),
                        Data = update.Metadata.ToMeta()
                    });
                    break;
                case OwnerUpdateEvent update:
                    OnOwnerUpdated?.Invoke(update.NewOwner.ToLobbyMember());
                    break;
                case ChatEvent chat:
                    OnChat?.Invoke(new LobbyChatMessage
                    {
                        Sender = chat.Sender.ToLobbyMember(),
                        Content = chat.Content,
                        Direct = chat.Direct,
                    });
                    break;
            }
        }
        
        #region Core
        public static event Action<MemberJoinedInfo> OnOtherMemberJoined;
        public static event Action<LeaveInfo> OnOtherMemberLeft;
        public static event Action<KickInfo> OnLocalMemberKicked;
        public static event Action<LobbyInvite> OnReceivedInvitation;
        public static event Action<LobbyDataUpdate> OnLobbyDataUpdated;
        public static event Action<MemberDataUpdate> OnMemberDataUpdated;
        public static event Action<LobbyMember> OnOwnerUpdated;
        public static event Action<LobbyChatMessage> OnChat;
        
        public static LobbyMember GetLocalUser() => _localUser;

        public static async Task<RequestResponse<EnterResponse>> Create(CreateLobbyRequest request, float timeoutSeconds = 3f, CancellationToken token = default)
        {
           return await GetResponseAsync<EnterResponse>(request, timeoutSeconds, token);
        }
        
        public static async Task<RequestResponse<EnterResponse>> Join(JoinLobbyRequest request, float timeoutSeconds = 3f, CancellationToken token = default)
        {
            return await GetResponseAsync<EnterResponse>(request, timeoutSeconds, token);
        }

        public static void Leave(LeaveLobbyRequest request)
        {
            SendCommand(request);
        }

        public static void Invite(InviteMemberRequest request)
        {
            SendCommand(request);
        }

        public static void CloseLobby(CloseLobbyRequest request)
        {
            SendCommand(request);
        }

        public static void SetOwner(SetOwnerRequest request)
        {
            SendCommand(request);
        }

        public static void KickMember(KickMemberRequest request)
        {
            SendCommand(request);
        }

        public static void SetLobbyData(LobbyDataRequest request)
        {
            SendCommand(request);
        }

        public static void SetMemberData(MemberDataRequest request)
        {
            SendCommand(request);
        }

        public static string GetLobbyDataOrDefault(string lobbyId, string key, string defaultValue)
        {
            if (_cachedLobbies.TryGetValue(lobbyId, out var lobby))
            {
                return lobby.LobbyData.GetValueOrDefault(key, defaultValue);
            }
            return defaultValue;
        }

        public static string GetMemberDataOrDefault(string lobbyId, string memberId, string key, string defaultValue)
        {
            if (!Guid.TryParse(memberId, out var id)) return defaultValue;
            if (_cachedLobbies.TryGetValue(lobbyId, out var lobby) && lobby.MemberData.TryGetValue(id, out var data))
            {
                return data.GetValueOrDefault(key, defaultValue);
            }
            return defaultValue;
        }
        #endregion
        
        #region Friends

        public static async Task<RequestResponse<QueryFriendsResponse>> GetFriends(float timeoutSeconds = 3f, CancellationToken token = default)
        {
            return await GetResponseAsync<QueryFriendsResponse>(new QueryFriendsRequest(), timeoutSeconds, token);
        }
        
        #endregion

        #region Browsing
        public static async Task<RequestResponse<BrowseResponse>> Browse(float timeoutSeconds = 3f, CancellationToken token = default)
        {
            var result = await GetResponseAsync<BrowseResponse>(new BrowseRequest(), timeoutSeconds, token);

            if (result.Error is Error.Ok)
            {
                foreach (var snapshot in result.Response.Snapshots) CacheSnapshot(snapshot);
            }
            
            return result;
        }

        public static void ApplyNumberFilter(ApplyNumberFilterRequest request)
        {
            SendCommand(request);
        }
        
        public static void ApplyStringFilter(ApplyStringFilterRequest request)
        {
            SendCommand(request);
        }
        
        public static void ApplySlotsAvailableFilter(ApplySlotsAvailableFilterRequest request)
        {
            SendCommand(request);
        }
        
        public static void ApplyLimitResponsesFilter(ApplyLimitResponsesFilterRequest request)
        {
            SendCommand(request);
        }
        
        #endregion
        
        #region Chat

        public static void SendChatMessage(ChatMessageRequest message)
        {
            SendCommand(message);
        }
        
        public static void SendDirectMessage(DirectMessageRequest message)
        {
            SendCommand(message);
        }
        
        #endregion
    }
}