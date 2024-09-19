using Sanctum_Core_Server;
internal class Program
{
    private static void Main()
    {
        Server server = new(deadLobbyCheckTimer: 1, allowedLobbyIdleDuration: 1);
        server.StartListening();
    }
}