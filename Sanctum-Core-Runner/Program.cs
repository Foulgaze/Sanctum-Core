using Sanctum_Core;
internal class Program
{
    private static void Main()
    {
        Server server = new();
        server.StartListening();
    }
}