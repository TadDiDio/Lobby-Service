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
            if (!value.StartsWith(SharedLogger.Header)) return;
            
            Debug.Log(value);
        }

        public override void Write(string value)
        {
            if (!value.StartsWith(SharedLogger.Header)) return;

            Debug.Log(value);
        }

        private static TextWriter _out;
        private static TextWriter _err;
        
        public static void Redirect()
        {
            _out = Console.Out;
            _err = Console.Error;
            
            Console.SetOut(new ConsoleRedirector());
            Console.SetError(new ConsoleErrorRedirector());
        }

        public static void Return()
        {
            if (_out == null || _err == null) return;
                
            Console.SetOut(_out);
            Console.SetError(_err);
        }

        private class ConsoleErrorRedirector : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
            public override void WriteLine(string value)
            {
                if (!value.StartsWith(SharedLogger.Header)) return;
                
                Debug.LogError(value);
            }

            public override void Write(string value)
            {
                if (!value.StartsWith(SharedLogger.Header)) return;

                Debug.LogError(value);
            }
        }
    }
}