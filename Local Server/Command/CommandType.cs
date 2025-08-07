using System.Text.Json.Serialization;

namespace LocalLobby.Command;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CommandType
{
    Create,
    Join,
    CancelJoin,
    Leave,
    Close,
    Invite,
    Kick,
    KickAndBan,
    SetOwner,
}
