using System.Text.Json.Serialization;

namespace LocalLobby.Command;

// TODO: This needs to be way more flexible to support proper event raising on the other side.

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResponseType
{
    Error,
    Success
}

public class Response
{
    public ResponseType Type { get; set; }
    public string Message { get; set; }

    private Response(ResponseType type, string message)
    {
        Type = type;
        Message = message;
    }

    public static Response Error(string message)
    {
        return new Response(ResponseType.Error, message);
    }
    public static Response Success(string message)
    {
        return new Response(ResponseType.Success, message);
    }
}