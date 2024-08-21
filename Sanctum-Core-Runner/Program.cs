using Sanctum_Core_Server;
internal class Program
{
    private static void Main()
    {
        Server server = new();
        server.StartListening();
    }
}