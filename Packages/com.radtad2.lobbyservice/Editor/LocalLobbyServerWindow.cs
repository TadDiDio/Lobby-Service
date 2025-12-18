using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace LobbyService.LocalServer.Editor
{
    public class LocalLobbyServerWindow : EditorWindow
    {
        private const string GitHubRepoOwner = "TadDiDio";
        private const string GitHubRepoName  = "LocalLobbyServer";
        private const string DownloadFolder  = "LocalLobbyServerBin";

        private static string _binaryPath;
        private static Process _serverProcess;
        
        private bool _isServerRunning;
        private CancellationTokenSource _pollingCts;

        [MenuItem("Tools/Local Lobby Server")]
        public static void ShowWindow()
        {
            GetWindow<LocalLobbyServerWindow>("Local Lobby Server");
        }

        private string GetCleanDownloadFolder()
        {
            string downloadFolder = GetDownloadFolder();
            string resolvedFolder = Path.IsPathRooted(downloadFolder) ? downloadFolder
                : Path.Combine(Application.dataPath, "..", downloadFolder);

            return Path.GetFullPath(resolvedFolder);
        }
        
        private void OnEnable()
        {
            StartServerPolling();
        }

        private void OnDisable()
        {
            StopServerPolling();
        }
        
        private void OnGUI()
        {
            string expectedBinaryName = ServerPlatform.GetBinaryFileName();
            string resolvedFolder = GetCleanDownloadFolder();
            
            if (!Directory.Exists(resolvedFolder)) Directory.CreateDirectory(resolvedFolder);

            _binaryPath = Path.Combine(resolvedFolder, expectedBinaryName);

            GUILayout.Label("Local Lobby Server Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Download Folder", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            string newFolder = EditorGUILayout.TextField(resolvedFolder);

            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string picked = EditorUtility.OpenFolderPanel("Select Download Folder", resolvedFolder, "");
                if (!string.IsNullOrEmpty(picked))
                {
                    newFolder = picked;
                }
            }
            
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                newFolder = DownloadFolder;
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (newFolder != resolvedFolder)
            {
                SetDownloadFolder(newFolder);
                resolvedFolder = GetCleanDownloadFolder();
                
                if (!Directory.Exists(resolvedFolder)) Directory.CreateDirectory(resolvedFolder);
            
                _binaryPath = Path.Combine(resolvedFolder, expectedBinaryName);
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Pull Binary From GitHub Releases"))
            {
                DownloadServerBinaryAsync(resolvedFolder);
            }
            
            bool binaryExists = File.Exists(_binaryPath);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Platform:", ServerPlatform.GetPlatformKey().ToUpper());
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Binary Status:", GUILayout.Width(100));
            
                GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = binaryExists ? Color.green : Color.red }
                };
            
                EditorGUILayout.LabelField(binaryExists ? "Downloaded" : "Not Downloaded", statusStyle);
            }
            
            if (binaryExists)
            {
                EditorGUILayout.BeginHorizontal();
            
                if (GUILayout.Button("Reveal", GUILayout.Width(100)))
                {
                    RevealInFileBrowser(_binaryPath);
                }
            
                if (GUILayout.Button("Delete Binary", GUILayout.Width(120)))
                {
                    if (EditorUtility.DisplayDialog("Delete Local Server Binary?",
                            $"Are you sure you want to delete:\n\n{_binaryPath}",
                            "Delete", "Cancel"))
                    {
                        try
                        {
                            File.Delete(_binaryPath);
                            AssetDatabase.Refresh(); // just in case
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            
                EditorGUILayout.EndHorizontal();
            }

            GUI.enabled = binaryExists && !IsServerRunning();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Server Status:", GUILayout.Width(100));
                
                GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = IsServerRunning() ? Color.green : Color.red }
                };
                
                EditorGUILayout.LabelField(IsServerRunning() ? "Running" : "Not Running", statusStyle);
            }
            
            EditorGUI.BeginChangeCheck();

            string portStr = EditorGUILayout.TextField("Port", LobbyConfig.Port.ToString());

            if (EditorGUI.EndChangeCheck())
            {
                if (int.TryParse(portStr, out int newPort) && newPort is > 0 and <= 65535)
                {
                    LobbyConfig.Port = newPort;
                }
                else
                {
                    EditorGUILayout.HelpBox("Invalid port (1–65535).", MessageType.Warning);
                }
            }
            
            if (GUILayout.Button("Start Server"))
            {
                StartServer();
            }

            GUI.enabled = IsServerRunning();

            if (GUILayout.Button("Stop Server"))
            {
                StopServer();
            }

            GUI.enabled = true;
        }

        private void RevealInFileBrowser(string path)
        {
            if (!File.Exists(path))
            {
                LobbyLogger.LogError("Cannot reveal file. It does not exist.");
                return;
            }

#if UNITY_EDITOR_WIN
            Process.Start("explorer.exe", $"/select,\"{path}\"");
#elif UNITY_EDITOR_OSX
            Process.Start("open", $"-R \"{path}\"");
#else
            Process.Start("xdg-open", Path.GetDirectoryName(path));
#endif
        }

        #region Download
        
        private const string DownloadFolderPrefKey = "LocalLobbyServer.DownloadFolder";

        private string GetDownloadFolder()
        {
            return EditorPrefs.GetString(DownloadFolderPrefKey, "LocalLobbyServer");
        }

        private void SetDownloadFolder(string folder)
        {
            EditorPrefs.SetString(DownloadFolderPrefKey, folder);
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
        
        [Serializable]
        private class GitHubRelease
        {
            public GitHubAsset[] assets;
        }

        [Serializable]
        private class GitHubAsset
        {
            public long id;
            public string name;
        }
        
        private long ExtractAssetId(string json, string expectedName)
        {
            var release = JsonUtility.FromJson<GitHubRelease>(json);
            if (release?.assets == null)
                return 0;

            foreach (var asset in release.assets)
                if (asset.name == expectedName)
                    return asset.id;

            return 0;
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
        
        private async void DownloadServerBinaryAsync(string downloadFolder)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", DownloadFolder));

                EditorUtility.DisplayProgressBar("Downloading", "Fetching latest GitHub release...", 0.2f);

                using var metaClient = new HttpClient();
                metaClient.DefaultRequestHeaders.Add("User-Agent", "UnityEditor");

                string releaseUrl = $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases/latest";
                string json = await metaClient.GetStringAsync(releaseUrl);

                string platformKey  = ServerPlatform.GetPlatformKey();
                string expectedName = ServerPlatform.GetBinaryFileName();

                string assetBrowserUrl = GetAssetDownloadUrlForPlatform(json, expectedName);
                if (assetBrowserUrl == null)
                {
                    EditorUtility.ClearProgressBar();
                    LobbyLogger.LogError($"Asset `{expectedName}` not found in GitHub release.");
                    return;
                }

                long assetId = ExtractAssetId(json, expectedName);
                if (assetId == 0)
                {
                    EditorUtility.ClearProgressBar();
                    LobbyLogger.LogError($"Failed to resolve asset ID for `{expectedName}`.");
                    return;
                }

                string assetApiUrl = $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases/assets/{assetId}";

                string localPath = Path.Combine(downloadFolder, expectedName);
                EditorUtility.DisplayProgressBar(
                    "Downloading",
                    $"Downloading server binary for {platformKey}...",
                    0.5f
                );

                using (var downloadClient = new HttpClient())
                {
                    downloadClient.DefaultRequestHeaders.Add("User-Agent", "UnityEditor");
                    downloadClient.DefaultRequestHeaders.Add("Accept", "application/octet-stream");

                    byte[] bytes = await downloadClient.GetByteArrayAsync(assetApiUrl);
                    await File.WriteAllBytesAsync(localPath, bytes);
                }

                EditorUtility.ClearProgressBar();
                LobbyLogger.Log($"Downloaded server binary for {platformKey} to: {localPath}");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(e);
            }
        }

        #endregion

        #region Server
        private void StartServer()
        {
            if (!File.Exists(_binaryPath))
            {
                LobbyLogger.LogError("Server binary not found. Download it first.");
                return;
            }

            if (IsServerRunning())
            {
                LobbyLogger.Log("Server is already running.");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _binaryPath,
                    Arguments = $"{LobbyConfig.Port}",
                    WorkingDirectory = Path.GetDirectoryName(_binaryPath) ?? ".",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                _serverProcess = Process.Start(psi);
                _isServerRunning = true;

                LobbyLogger.Log("Local server started.");
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
                    LobbyLogger.Log("Server is not running.");
                    return;
                }

                _serverProcess.Kill();
                _serverProcess = null;
                _isServerRunning = false;
                LobbyLogger.Log("Server stopped.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void StartServerPolling()
        {
            if (_pollingCts != null) return;

            _pollingCts = new CancellationTokenSource();
            PollServerLoop(_pollingCts.Token);
        }

        private void StopServerPolling()
        {
            try
            {
                _pollingCts?.Cancel();
                _pollingCts = null;
            }
            catch
            {
                // ignored
            }
        }
        
        
        private async void PollServerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_serverProcess is { HasExited: false })
                    {
                        _isServerRunning = true;
                    }
                    else
                    {
                        var processes = Process.GetProcessesByName(ServerPlatform.GetBinaryFileName().Split(".")[0]);
                        
                        _isServerRunning = processes.Length > 0;
                        if (_isServerRunning)
                        {
                            _serverProcess = processes[0];
                        }
                    }

                    await Task.Delay(1500, token);  
                }
                catch
                {
                    _isServerRunning = false;
                }
            }
        }

        private bool IsServerRunning() => _isServerRunning;
        #endregion
    }
}
