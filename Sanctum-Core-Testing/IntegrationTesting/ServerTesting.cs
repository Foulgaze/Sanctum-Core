using Newtonsoft.Json;
using Sanctum_Core_Server;
using System.Net;
using System.Net.Sockets;

namespace Sanctum_Core_Testing
{
    public class ServerTesting
    {
        private const int MessageLength = 36;
        private Server server;
        private Thread serverThread;
        private int uuidLength;
        [OneTimeSetUp]
        public void Init()
        {
            this.server = new(53522, deadLobbyCheckTimer : null);
            this.serverThread = new Thread(new ThreadStart(this.server.StartListening)) { Name = "Server Thread" };
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
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            NetworkStream stream = client.GetStream();
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, $"4|Gabriel");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            Assert.IsNotNull(command);
            Assert.That(command.instruction.Length == (this.uuidLength + 1 + Server.lobbyCodeLength)); // 36 UUID | 4 Lobby Code = 41
            string[] data = command.instruction.Split('|');
            Assert.That(data[0].Length, Is.EqualTo(MessageLength));
            Assert.That(data[1].Where(c => !char.IsLetterOrDigit(c)).Count() == 0);
        }

        [Test]
        public void InvalidSizeCreateLobbyTest()
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            NetworkStream stream = client.GetStream();
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, "Asd|Asd");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            AssertCommandResults(command, NetworkInstruction.InvalidCommand, "Invalid lobby count");
        }

        [Test]
        public void JoinLobbyTest()
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"4|Gabriel");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            Assert.IsNotNull(command);
            string[] data = command.instruction.Split('|');
            client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{data[1]}|Gabe");
            command = new LobbyConnection("", "", client).GetNetworkCommand();
            Assert.IsNotNull(command);
            AssertCommandResults(command, NetworkInstruction.JoinLobby, null);
            Assert.That(command.instruction.Length, Is.EqualTo(this.uuidLength + 2));
        }

        [Test]
        public void IncorrectJoinLobbyTest()
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            NetworkStream stream = client.GetStream();
            Server.SendMessage(stream, NetworkInstruction.JoinLobby, $"Spaghetti & Meatballs");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            AssertCommandResults(command, NetworkInstruction.InvalidCommand, "Need to include Name and Lobby code");
        }

        public static void AssertCommandResults(NetworkCommand? command, NetworkInstruction expectedOpCode, string? expectedPayload)
        {
            Assert.IsNotNull(command);
            Assert.That(command.opCode, Is.EqualTo((int)expectedOpCode));
            if (expectedPayload is not null)
            {
                Assert.That(command.instruction, Is.EqualTo(expectedPayload));
            }
        }
    }
}
