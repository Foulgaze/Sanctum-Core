using Sanctum_Core;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Numerics;
using System.Diagnostics;

namespace Sanctum_Core_Testing
{
    public class PlaytableTesting
    {
        private Server server;
        private Thread serverThread;
        private int uuidLength;
        [OneTimeSetUp]
        public void Init()
        {
            this.server = new(52522);
            this.serverThread = new Thread(new ThreadStart(this.server.StartListening));
            this.serverThread.Start();
            this.uuidLength = Guid.NewGuid().ToString().Length;
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void NetworkAttributesTest()
        {
            List<PlayerDescription> players = this.StartGameXPlayers(4);
            for (int i = 0; i < players.Count; ++i)
            {
                PlayerDescription player = players[i];
                Server.SendMessage(player.client.GetStream(), NetworkInstruction.NetworkAttribute, $"{player.uuid}-health|{JsonConvert.SerializeObject(i)}");
                Server.SendMessage(player.client.GetStream(), NetworkInstruction.NetworkAttribute, $"{player.uuid}-decklist|{JsonConvert.SerializeObject($"{i}")}");
                Server.SendMessage(player.client.GetStream(), NetworkInstruction.NetworkAttribute, $"{player.uuid}-ready|{JsonConvert.SerializeObject(true)}");
            }
            NetworkCommand? command;
            for (int b = 0; b < 3; ++b)
            {
                for (int i = 0; i < players.Count; ++i)
                {
                    PlayerDescription player = players[i];
                    for (int a = 0; a < players.Count; ++a)
                    {
                        command = NetworkCommandManager.GetNextNetworkCommand(players[a].client.GetStream(), players[a].buffer, Server.bufferSize);
                        switch (b)
                        {
                            case 0:
                                ServerTesting.AssertCommandResults(command, NetworkInstruction.NetworkAttribute, $"{player.uuid}-health|{i}");
                                break;
                            case 1:
                                ServerTesting.AssertCommandResults(command, NetworkInstruction.NetworkAttribute, $"{player.uuid}-decklist|{JsonConvert.SerializeObject($"{i}")}");
                                break;
                            case 2:
                                ServerTesting.AssertCommandResults(command, NetworkInstruction.NetworkAttribute, $"{player.uuid}-ready|{JsonConvert.SerializeObject(true)}");
                                break;
                        }
                    }
                }
            }
            for (int i = 0; i < players.Count; ++i)
            {
                command = NetworkCommandManager.GetNextNetworkCommand(players[i].client.GetStream(), players[i].buffer, Server.bufferSize);

                ServerTesting.AssertCommandResults(command, NetworkInstruction.NetworkAttribute, $"main-started|{JsonConvert.SerializeObject(true)}");
            }
        }

        [Test]
        public void RemoveCardFromDeck()
        {
            List<PlayerDescription> players = this.StartGameXPlayers(4);
            players.Sort((x,y) => x.uuid.CompareTo(y.uuid));
            Server.SendMessage(players[0].client.GetStream(), NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-{0}-remove|0");
            foreach (PlayerDescription player in players)
            {
                NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(player.client.GetStream(), player.buffer, Server.bufferSize);
                Assert.IsNotNull(command);
                string[] data = command.instruction.Split('|');
                Assert.AreEqual(2, data.Length);
                Assert.AreEqual("0", $"{data[0]}");
                List<List<int>> deck = JsonConvert.DeserializeObject<List<List<int>>>(data[1]);
                Assert.AreEqual(deck.Count,1 );
                Assert.AreEqual(deck[0].Count,99 );
            }
        }



        // Returns Lobby Code, UUID, Network Stream
        private (string, PlayerDescription) CreateLobby(int playerCount, string playerName)
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"{playerCount}|{playerName}");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            string[] commandData = command.instruction.Split('|');
            return (commandData[1], new PlayerDescription(playerName, commandData[0], client));
        }

        private PlayerDescription AddToLobby(string playerName, string lobbyCode)
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{lobbyCode}|{playerName}");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            string[] commandData = command.instruction.Split('|');
            return new PlayerDescription(playerName, commandData[0], client);
        }

        private List<PlayerDescription> StartGameXPlayers(int playerCount)
        {

            Assert.That(playerCount > 0);
            (string lobbyCode, PlayerDescription lobbyPlayer) = this.CreateLobby(playerCount, "Player-0");
            List<PlayerDescription> returnList = new() { lobbyPlayer };
            for (int i = 1; i < playerCount; ++i)
            {
                TcpClient client = new();
                client.Connect(IPAddress.Loopback, this.server.portNumber);
                returnList.Add(this.AddToLobby($"Player-{i}", lobbyCode));
            }
            foreach (PlayerDescription player in returnList)
            {
                NetworkCommand? command;
                do
                {
                    command = NetworkCommandManager.GetNextNetworkCommand(player.client.GetStream(), player.buffer, Server.bufferSize);
                } while (command.opCode != (int) NetworkInstruction.StartGame);
            }
            returnList.Reverse();
            return returnList;

        }
    }
}
