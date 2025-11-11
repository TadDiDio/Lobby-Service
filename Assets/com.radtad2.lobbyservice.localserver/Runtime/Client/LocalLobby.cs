using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace LobbyService.LocalServer
{
    /// <summary>
    /// API for interacting with the local lobby server.
    /// </summary>
    public static class LocalLobby
    {
        public static bool Initialized { get; private set; }

        private static Task _initTask;
        private static LocalLobbyClient _client;

        private static LobbyMember _localUser;
        
        
        /// <summary>
        /// Initializes the local lobby API.
        /// </summary>
        /// <param name="token">A token used for async init portions.</param>
        public static void Init(CancellationToken token)
        {
            ConsoleRedirector.Redirect();
            _initTask = InitializeAsync(token);
        }

        private static async Task InitializeAsync(CancellationToken token)
        {
            try
            {
                Launcher.EnsureServerExists();
                
                _client = new LocalLobbyClient(IPAddress.Loopback, ServerDetails.Port);
                if (!await _client.ConnectAsync(token)) return;

                var welcome = await GetResponseAsync<WelcomeResponse>(new ConnectRequest(), 3f, token);

                if (welcome.Error is not Error.Ok) return;
                
                _localUser = welcome.Response.LocalMember.ToLobbyMember();
                
                Initialized = true;
                Debug.Log($"[Local Lobby] Initialized as user {_localUser}");
            }
            catch (OperationCanceledException) { /* Ignored */ }
            catch (Exception e) { Debug.LogException(e); }
        }

        public static async Task WaitUntilReady()
        {
            if (_initTask == null) throw new InvalidOperationException("Init not called.");
            await _initTask;
        }
        
        public static void Shutdown()
        {
            _client?.Dispose();
        }

        private static void SendCommand(IRequest request)
        {
            var message = Message.CreateRequest(request);
            _client.Send(message);
        }
        
        private static async Task<RequestResponse<T>> GetResponseAsync<T>(IRequest request, float timeoutSeconds, CancellationToken token = default) where T : IResponse
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
            
            if (!MessageTypeRegistry.TryGetType(response.Type, out var type) || response.Payload.ToObject(type) is not T typedResponse)
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
    }
}