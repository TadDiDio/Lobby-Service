using UnityEngine;

namespace LobbyService
{
    public class LobbyLogger
    {
        private const string Header = "[Lobby Service] ";
        
        public static void Log(string message)
        {
            message = Header + message;
            Debug.Log(message);
        }

        public static void LogWarning(string message)
        {
            message = Header + message;
            Debug.LogWarning(message);
        }
        
        public static void LogError(string message)
        {
            message = Header + message;
            Debug.LogError(message);
        }
    }
}