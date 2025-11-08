using Newtonsoft.Json;

namespace LobbyService.LocalServer
{
    public static class Serializer
    {
        public static bool Deserialize(string json, out ICommand command)
        {
            command = null;
            
            var message = JsonConvert.DeserializeObject<Message>(json);
            if (message == null) return false;
            
            command = CommandRegistry.Get(message.Type);
            return command != null;
        }
        
        public static string Serialize(ICommand command)
        {
            var message = new Message(command);
            return JsonConvert.SerializeObject(message);
        }
    }
}