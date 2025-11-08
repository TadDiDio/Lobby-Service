using Newtonsoft.Json.Linq;

namespace LobbyService.LocalServer
{
    public class Message
    {
        public string Type;
        public JToken Args;

        public Message(ICommand command)
        {
            Type = command.GetType().Name;
            Args = JToken.FromObject(command);
        }
    }
}