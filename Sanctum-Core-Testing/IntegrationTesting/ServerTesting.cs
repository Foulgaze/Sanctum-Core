using Newtonsoft.Json;
using Sanctum_Core;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

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
            this.server = new(53522);
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
            _ = Server.SendMessage(stream, NetworkInstruction.CreateLobby, $"4|Gabriel");
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
            _ = Server.SendMessage(stream, NetworkInstruction.CreateLobby, "Asd|Asd");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            AssertCommandResults(command, NetworkInstruction.InvalidCommand, "Invalid lobby count");
        }

        [Test]
        public void JoinLobbyTest()
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            _ = Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"4|Gabriel");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            Assert.IsNotNull(command);
            string[] data = command.instruction.Split('|');
            client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            _ = Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{data[1]}|Gabe");
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
            _ = Server.SendMessage(stream, NetworkInstruction.JoinLobby, $"Spaghetti & Meatballs");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            AssertCommandResults(command, NetworkInstruction.InvalidCommand, "Need to include Name and Lobby code");
        }

 /*       [Test]
        public void AddPlayerToLobbyTest()
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"3|Gabriel");
            NetworkCommand? command = this.NonBlockingRead(client.GetStream(),30);
            Assert.IsNotNull(command);
            string[] data = command.instruction.Split('|');
            TcpClient client2 = new();
            client2.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client2.GetStream(), NetworkInstruction.JoinLobby, $"{data[1]}|Gabe");
            _ = this.NonBlockingRead(client2.GetStream(), 30);
            command = _ = this.NonBlockingRead(client2.GetStream(), 30);
            AssertCommandResults(command, NetworkInstruction.PlayersInLobby, "[\"Gabe\",\"Gabriel\"]");
        }*/

        [Test]
        public void StartLobbyTest()
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            _ = Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"2|Gabriel");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            Assert.IsNotNull(command);
            string[] data = command.instruction.Split('|');
            string p1UUID = data[0];
            string lobbyCode = data[1];
            TcpClient client2 = new();
            client2.Connect(IPAddress.Loopback, this.server.portNumber);
            _ = Server.SendMessage(client2.GetStream(), NetworkInstruction.JoinLobby, $"{lobbyCode}|Gabe");
            command = new LobbyConnection("", "", client2).GetNetworkCommand();
            Assert.IsNotNull(command);
            data = command.instruction.Split('|');
            string p2UUID = data[0];
            command = new LobbyConnection("", "", client).GetNetworkCommand();
            Assert.IsNotNull(command);
            AssertCommandResults(command, NetworkInstruction.StartGame, null);
            Dictionary<string, string> expectedLobby = new() { { p1UUID, "Gabriel" }, { p2UUID, "Gabe" } };
            Dictionary<string, string>? actualDictionary;
            try
            {
                actualDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(command.instruction);
            }
            catch
            {
                actualDictionary = null;
            }
            Assert.IsNotNull(actualDictionary);
            CollectionAssert.AreEqual(expectedLobby, actualDictionary);
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
