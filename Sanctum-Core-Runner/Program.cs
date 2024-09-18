using Sanctum_Core_Server;
internal class Program
{
    private static void Main()
    {
        Server server = new(deadLobbyCheckTimer: 0.1, allowedLobbyIdleDuration: 0.1);
        server.StartListening();
    }
}