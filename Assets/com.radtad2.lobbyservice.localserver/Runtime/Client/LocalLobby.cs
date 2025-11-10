using System;
using System.Diagnostics;
using System.Net.Sockets;
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
        public static bool IsReady { get; private set; }

        private static Task _initTask;
        private static Communication _comms;
        
        private static TcpClient _client;
        
        /// <summary>
        /// Initializes the local lobby API.
        /// </summary>
        /// <param name="token">A token used for async init portions.</param>
        public static void Init(CancellationToken token)
        {
            _initTask = InitializeAsync(token);
        }

        private static async Task InitializeAsync(CancellationToken token)
        {
            try
            {
                Launcher.EnsureServerExists();
                
                _comms = new Communication();

                await _comms.InitAsync(token);
                IsReady = true;
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static async Task WaitUntilReady()
        {
            if (_initTask == null) throw new InvalidOperationException("Init not called.");
            await _initTask;
        }
        
        public static void Shutdown()
        {
            _client?.Close();
            _client?.Dispose();
        }

        public static void Create(string name)
        {
            var cmd = new CreateCommand
            {
                Name = name
            };

            _comms.SendCommand(cmd);
        }
    }
}