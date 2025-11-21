using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace LobbyService.LocalServer.Editor
{
    public class LocalLobbyServerWindow : EditorWindow
    {
        private const string GitHubRepoOwner = "TadDiDio";
        private const string GitHubRepoName  = "LocalLobbyServer";
        private const string DownloadFolder  = "LocalLobbyServerBin";

        private static Process _serverProcess;
        private static string _binaryPath;

        private bool _showLogWindow;
        private bool _autoStart;

        [MenuItem("Tools/Local Lobby Server")]
        public static void ShowWindow()
        {
            GetWindow<LocalLobbyServerWindow>("Local Lobby Server");
        }

        private void OnEnable()
        {
            EditorApplication.update += CheckAutoStart;
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckAutoStart;
        }

        private void OnGUI()
        {
            // Resolve correct binary path for this platform
            string platformKey = ServerPlatform.GetPlatformKey();
            string expectedName = ServerPlatform.GetBinaryFileName();
            _binaryPath = Path.Combine(Application.dataPath, "..", DownloadFolder, expectedName);

            GUILayout.Label("Local Lobby Server Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _autoStart = EditorGUILayout.Toggle("Auto Start", _autoStart);
            _showLogWindow = EditorGUILayout.Toggle("Show Log Window", _showLogWindow);

            EditorGUILayout.Space();

            // Download button
            if (GUILayout.Button("Pull Binary From GitHub Releases"))
            {
                DownloadServerBinary();
            }

            // -------------------------------
            // STATUS: Is the correct binary downloaded?
            // -------------------------------
            bool binaryExists = File.Exists(_binaryPath);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Platform:", platformKey.ToUpper());

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Binary Status:", GUILayout.Width(100));

                GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
                statusStyle.normal.textColor = binaryExists ? Color.green : Color.red;

                EditorGUILayout.LabelField(binaryExists ? "Downloaded" : "Not Downloaded", statusStyle);
            }

            // -------------------------------
            // CONTROL BUTTONS
            // -------------------------------

            GUI.enabled = binaryExists;

            if (GUILayout.Button("Start Server"))
            {
                StartServer();
            }

            GUI.enabled = _serverProcess != null && !_serverProcess.HasExited;

            if (GUILayout.Button("Stop Server"))
            {
                StopServer();
            }

            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Binary Location:");
            EditorGUILayout.LabelField(_binaryPath);
        }

        private static class ServerPlatform
        {
            public static string GetPlatformKey()
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return "win";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        return "mac";
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        return "linux";
#else
        throw new NotSupportedException("Unsupported platform.");
#endif
            }

            public static string GetBinaryFileName()
            {
                switch (GetPlatformKey())
                {
                    case "win":   return "LocalLobbyServer-win.exe";
                    case "mac":   return "LocalLobbyServer-mac";
                    case "linux": return "LocalLobbyServer-linux";
                    default:      throw new NotSupportedException();
                }
            }
        }
        private string GetAssetDownloadUrlForPlatform(string releaseJson, string expectedFileName)
        {
            const string key = "\"browser_download_url\":";

            foreach (var line in releaseJson.Split('\n'))
            {
                if (!line.Contains(key)) continue;
                if (!line.Contains(expectedFileName)) continue;

                int start = line.IndexOf("https", StringComparison.Ordinal);
                int end = line.IndexOf('"', start + 1);
                return line.Substring(start, end - start);
            }

            return null;
        }
        private async void DownloadServerBinary()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", DownloadFolder));

                EditorUtility.DisplayProgressBar("Downloading", "Fetching latest GitHub release...", 0.2f);

                using var client = new HttpClient();

                // GitHub API endpoint
                var releaseUrl = $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases/latest";
                client.DefaultRequestHeaders.Add("User-Agent", "UnityEditor");

                // Fetch JSON
                string json = await client.GetStringAsync(releaseUrl);

                // Determine correct platform + filename
                string platformKey = ServerPlatform.GetPlatformKey();     // "win" | "mac" | "linux"
                string expectedName = ServerPlatform.GetBinaryFileName(); // e.g. LocalLobbyServer-win.exe

                // Extract the proper asset URL
                string assetUrl = GetAssetDownloadUrlForPlatform(json, expectedName);
                if (assetUrl == null)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError($"Asset `{expectedName}` not found in GitHub release.");
                    return;
                }

                string localPath = Path.Combine(Application.dataPath, "..", DownloadFolder, expectedName);

                EditorUtility.DisplayProgressBar("Downloading", $"Downloading server binary for {platformKey}...", 0.5f);

                // Download binary
                var bytes = await client.GetByteArrayAsync(assetUrl);
                File.WriteAllBytes(localPath, bytes);

                EditorUtility.ClearProgressBar();
                Debug.Log($"Downloaded server binary for {platformKey} to: {localPath}");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(e);
            }
        }


        private void StartServer()
        {
            if (!File.Exists(_binaryPath))
            {
                Debug.LogError("Server binary not found. Download it first.");
                return;
            }

            if (IsServerRunning())
            {
                Debug.Log("Server is already running.");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _binaryPath,
                    WorkingDirectory = Path.GetDirectoryName(_binaryPath),
                    CreateNoWindow = !_showLogWindow,
                    UseShellExecute = !_showLogWindow,
                    RedirectStandardOutput = _showLogWindow,
                    RedirectStandardError  = _showLogWindow
                };

                _serverProcess = Process.Start(psi);

                if (_showLogWindow)
                {
                    Task.Run(async () =>
                    {
                        while (!_serverProcess.HasExited)
                        {
                            string line = await _serverProcess.StandardOutput.ReadLineAsync();
                            if (line != null)
                            {
                                Repaint();
                            }
                        }
                    });
                }

                Debug.Log("Local server started.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void StopServer()
        {
            try
            {
                if (_serverProcess == null || _serverProcess.HasExited)
                {
                    Debug.Log("Server is not running.");
                    return;
                }

                _serverProcess.Kill();
                _serverProcess = null;
                Debug.Log("Server stopped.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private bool IsServerRunning()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
                return true;

            // Fallback: check by process name
            string name = Path.GetFileNameWithoutExtension("LocalLobbyServer");
            return Process.GetProcessesByName(name).Length > 0;
        }

        private void CheckAutoStart()
        {
            if (!_autoStart) return;

            if (!IsServerRunning() && File.Exists(_binaryPath))
            {
                StartServer();
            }
        }
    }
}
