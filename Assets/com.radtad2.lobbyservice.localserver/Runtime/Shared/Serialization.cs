using Newtonsoft.Json;

namespace LobbyService.LocalServer
{
    public static class MessageSerializer
    {
        public static string Serialize(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }
        
        public static bool Deserialize(string json, out Message message)
        {
            message = JsonConvert.DeserializeObject<Message>(json);
            return message != null;
        }
    }
}