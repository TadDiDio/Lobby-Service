namespace LobbyService.LocalServer
{
    using System;
    using System.IO;
    using UnityEngine;

    public static class LobbyConfig
    {
        private static readonly string DirPath = Path.Combine(Application.persistentDataPath, "LobbyService");
        private static readonly string FilePath = Path.Combine(DirPath, "config.json");

        private const int DefaultPort = 54300;

        [Serializable]
        private class Data
        {
            public int Port = DefaultPort;
        }

        private static Data _cached;
        private static void EnsureLoaded()
        {
            if (_cached != null) return;
            if (!Directory.Exists(DirPath)) Directory.CreateDirectory(DirPath);

            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                _cached = JsonUtility.FromJson<Data>(json);
            }
            else
            {
                _cached = new Data();
                Save();
            }
        }

        public static int Port
        {
            get { EnsureLoaded(); return _cached.Port; }
            set 
            { 
                EnsureLoaded();
                _cached.Port = value is <= 65535 and > 0 ? value : DefaultPort; 
                Debug.Log($"Set cached to {_cached.Port}");
                Save(); 
            }
        }

        private static void Save()
        {
            if (!Directory.Exists(DirPath)) Directory.CreateDirectory(DirPath);
            string json = JsonUtility.ToJson(_cached, true);
            File.WriteAllText(FilePath, json);
        }
    }
}