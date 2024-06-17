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
        private int uuidLength;
        [OneTimeSetUp]
        public void Init()
        {
            this.server = new();
            this.serverThread = new Thread(new ThreadStart(this.server.StartListening));
            this.serverThread.Start();
            this.uuidLength = Guid.NewGuid().ToString().Length;
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
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, $"4|Gabriel");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            Assert.That(command.instruction.Length == (this.uuidLength + 1 + Server.lobbyCodeLength)); // 36 UUID | 4 Lobby Code = 41
            string[] data = command.instruction.Split('|');
            Assert.That(36, Is.EqualTo(data[0].Length));
            Assert.That(data[1].Where(c => !char.IsLetterOrDigit(c)).Count() == 0);
        }

        [Test]
        public void NoNameCreateLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            NetworkStream stream = client.GetStream();
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, $"4");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            Assert.That(command.instruction == "Must include name and lobby code");
            Assert.That(command.opCode, Is.EqualTo((int)NetworkInstruction.InvalidCommand));
        }

        [Test]
        public void InvalidSizeCreateLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            NetworkStream stream = client.GetStream();
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, "Asd|Asd");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            Assert.That(command.instruction == "Invalid lobby count");
            Assert.That(command.opCode, Is.EqualTo((int)NetworkInstruction.InvalidCommand));
        }

        [Test]
        public void JoinLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"4|Gabriel");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            string[] data = command.instruction.Split('|');
            client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{data[1]}|Gabe");
            command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            Assert.That(command.opCode, Is.EqualTo((int)NetworkInstruction.JoinLobby));
            Assert.That(command.instruction.Length, Is.EqualTo(this.uuidLength));
        }

        [Test]
        public void IncorrectJoinLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            NetworkStream stream = client.GetStream();
            Server.SendMessage(stream, NetworkInstruction.JoinLobby, $"Spaghetti & Meatballs");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            Assert.That(command.opCode, Is.EqualTo((int)NetworkInstruction.InvalidCommand));
            Assert.That(command.instruction, Is.EqualTo("Need to include Name and Lobby code"));
        }
    }
}
