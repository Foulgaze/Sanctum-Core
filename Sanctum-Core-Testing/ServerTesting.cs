using Newtonsoft.Json;
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
            AssertCommandResults(command, NetworkInstruction.InvalidCommand, "Must include name and lobby code");
        }

        [Test]
        public void InvalidSizeCreateLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            NetworkStream stream = client.GetStream();
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, "Asd|Asd");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), 4096);
            AssertCommandResults(command, NetworkInstruction.InvalidCommand, "Invalid lobby count");
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
            AssertCommandResults(command, NetworkInstruction.JoinLobby, null);
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
            AssertCommandResults(command, NetworkInstruction.InvalidCommand, "Need to include Name and Lobby code");
        }

        [Test]
        public void AddPlayerToLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"3|Gabriel");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            string[] data = command.instruction.Split('|');
            client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{data[1]}|Gabe");
            _ = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096); //  Skip Get UUID
            command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096); // Should be a list of player names
            AssertCommandResults(command, NetworkInstruction.PlayersInLobby, "[\"Gabe\",\"Gabriel\"]");
        }

        [Test]
        public void StartLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"2|Gabriel");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            string[] data = command.instruction.Split('|');
            string p1UUID = data[0];
            client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{data[1]}|Gabe");
            command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096); //  Skip Get UUID
            data = command.instruction.Split('|');
            string p2UUID = data[0];
            command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096); // Should be a start lobby call
            AssertCommandResults(command, NetworkInstruction.StartGame, null);
            Dictionary<string, string> expectedLobby = new() { { p1UUID, "Gabriel" } , { p2UUID , "Gabe"} };
            Dictionary<string, string> actualDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(command.instruction);
            CollectionAssert.AreEqual(expectedLobby, actualDictionary);
        }

        public static void AssertCommandResults(NetworkCommand? command,NetworkInstruction expectedOpCode, string? expectedPayload)
        {
            Assert.IsNotNull(command);
            Assert.That(command.opCode, Is.EqualTo((int)expectedOpCode));
            if(expectedPayload is not null)
            {
                Assert.That(command.instruction, Is.EqualTo(expectedPayload));
            }
        }
    }
}
