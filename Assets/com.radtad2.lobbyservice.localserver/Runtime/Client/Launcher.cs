using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LobbyService.LocalServer
{
    public static class Launcher
    {
        private static readonly string ProjectName = "LocalLobbyServer";
        private static readonly string PidFile = Path.Combine(Application.persistentDataPath, "lobby_server.pid");

        private static Process proc;
        
        public static void EnsureServerExists()
        {
            if (TryGetExistingServer(out proc)) return;
    
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var path = Directory.GetFiles(projectRoot, $"{ProjectName}.csproj", SearchOption.AllDirectories)
                .FirstOrDefault();
            
            if (path == null) throw new FileNotFoundException("Could not find LocalLobbyServer.csproj file!");

            proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{path}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    Verb = "open"
                }
            };
            
            proc.Start();
            File.WriteAllText(PidFile, proc.Id.ToString());
        }
        
        private static bool TryGetExistingServer(out Process process)
        {
            process = null;
            if (!File.Exists(PidFile))
                return false;


            if (int.TryParse(File.ReadAllText(PidFile), out int pid))
            {
                try
                {
                    var existing = Process.GetProcessById(pid);
                    if (!existing.HasExited)
                    {
                        process = existing;
                        return true;
                    }
                }
                catch
                {
                    // Process does not exist anymore
                }
            }

            File.Delete(PidFile);
            return false;
        }
    }
}