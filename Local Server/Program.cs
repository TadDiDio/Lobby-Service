using LocalLobby;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        int port = args.Length > 0 ? int.Parse(args[0]) : 54333;
        bool createConsoleClient = args.Length > 1 ? bool.Parse(args[1]) : true;

        Server server = new(port);
        await server.RunAsync(createConsoleClient);
        return 0;
    }
}
