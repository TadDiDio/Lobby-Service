using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace LobbyService.LocalServer
{
    public class ConsoleRedirector : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        
        public override void WriteLine(string value)
        {
            if (!value.StartsWith("[Local Server]")) return;
            
            Debug.Log(value);
        }

        public override void Write(string value)
        {
            if (!value.StartsWith("[Local Server]")) return;

            Debug.Log(value);
        }

        public static void Redirect()
        {
            Console.SetOut(new ConsoleRedirector());
            Console.SetError(new ConsoleErrorRedirector());
        }

        private class ConsoleErrorRedirector : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
            public override void WriteLine(string value) => Debug.LogError(value);
            public override void Write(string value) => Debug.LogError(value);
        }
    }
}