using System.Text.Json;

namespace LocalLobby.Command;

public static class CommandRegistry
{
    private static readonly Dictionary<CommandType, Type> _map = new()
    {
        [CommandType.Create] = typeof(CreateCommand),
        [CommandType.Join] = typeof(JoinCommand),
        [CommandType.CancelJoin] = typeof(CancelJoinCommand),
        [CommandType.Leave] = typeof(LeaveCommand),
        [CommandType.Close] = typeof(CloseCommand),
        [CommandType.Invite] = typeof(InviteCommand),
        [CommandType.SetOwner] = typeof(SetOwnerCommand),
        [CommandType.Kick] = typeof(KickCommand),
        [CommandType.KickAndBan] = typeof(KickAndBanCommand)
    };

    public static object Deserialize(CommandEnvelope envelope)
    {
        if (!_map.TryGetValue(envelope.Type, out var type))
            throw new InvalidOperationException($"Unknown command type: {envelope.Type}");

        return envelope.Payload.Deserialize(type)!;
    }
}