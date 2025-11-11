using System;
using Newtonsoft.Json.Linq;

namespace LobbyService.LocalServer
{
    public class Message
    {
        public Error Error { get; set; } = Error.Ok;
        public Guid RequestId { get; set; } = Guid.Empty;
        public string Type { get; set; } = string.Empty;
        public JToken Payload {get; set;}

        public Message() { }
        
        public bool HasError => Error != Error.Ok;

        public static Message CreateRequest(IRequest request)
        {
            return new Message
            {
                Error = Error.Ok,
                RequestId = Guid.NewGuid(),
                Type = request.GetType().FullName,
                Payload = JObject.FromObject(request)
            };
        }

        public static Message CreateResponse(IResponse response, Guid requestId)
        {
            return new Message
            {
                Error = Error.Ok,
                RequestId = requestId,
                Type = response.GetType().FullName,
                Payload = JObject.FromObject(response)
            };
        }
        
        public static Message CreateFailure(Error error, Guid requestId)
        {
            return new Message
            {
                Error = error,
                RequestId = requestId
            };
        }
    }
}