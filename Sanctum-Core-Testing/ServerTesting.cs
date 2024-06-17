using Sanctum_Core;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core_Testing
{
    public class ServerTesting
    {
        private Server server;
        private Thread serverThread;
        [OneTimeSetUp]
        public void Init()
        {
            this.server = new();
            this.serverThread = new Thread(new ThreadStart(this.server.StartListening));
            this.serverThread.Start();
        }

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void CreateLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            NetworkStream stream = client.GetStream();
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), 4096);
            Guid uuid = Guid.NewGuid();
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, $"4|{uuid}|Gabriel");
            command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            Assert.That(command.instruction.Length == 4);
            Assert.That(command.instruction.Where(c => !char.IsLetterOrDigit(c)).Count() == 0);
        }
    }
}
