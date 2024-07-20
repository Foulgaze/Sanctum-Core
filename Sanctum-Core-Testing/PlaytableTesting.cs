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

        private void CheckAttribute(List<PlayerDescription> players, NetworkStream stream,string payload)
        {
            Server.SendMessage(stream, NetworkInstruction.NetworkAttribute, payload);
            foreach(PlayerDescription player in players)
            {
                NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(player.client.GetStream(), player.buffer, Server.bufferSize);
                Assert.IsNotNull(command);
                Assert.That(command.instruction, Is.EqualTo(payload));
            }
        }
        [Test]
        public void NetworkAttributesTest()
        {
            List<PlayerDescription> players = this.StartGameXPlayers(4);
            for (int i = 0; i < players.Count; ++i)
            {
                PlayerDescription player = players[i];
                this.CheckAttribute(players, player.client.GetStream(), $"{player.uuid}-health|{JsonConvert.SerializeObject(i)}");
                this.CheckAttribute(players, player.client.GetStream(), $"{player.uuid}-decklist|{JsonConvert.SerializeObject($"{i}")}");
                this.CheckAttribute(players, player.client.GetStream(), $"{player.uuid}-ready|{JsonConvert.SerializeObject(true)}");
            }
        }

        [Test]
        public void TestRemoveCard()
        {
            List<PlayerDescription> players = this.StartGameXPlayers(4);
            players.Sort((x,y) => x.uuid.CompareTo(y.uuid));
            Server.SendMessage(players[0].client.GetStream(), NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-0-remove|{JsonConvert.SerializeObject(0)}");
            NetworkAttributeManager nam = new(players);
            nam.ReadPlayerData(1);
            string key = $"{players[0].uuid}-0|{JsonConvert.SerializeObject(new List<List<int>> { Enumerable.Range(1, 99).ToList() })}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
        }

        [Test]
        public void TestMoveCard()
        {
            List<PlayerDescription> players = this.StartGameXPlayers(4);
            players.Sort((x, y) => x.uuid.CompareTo(y.uuid));
            NetworkAttributeManager nam = new(players);
            InsertCardData cardToMove = new(0,0,null,true);
            Server.SendMessage(players[0].client.GetStream(), NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-1-insert|{JsonConvert.SerializeObject(cardToMove)}");
            nam.ReadPlayerData(2);
            string key = $"{players[0].uuid}-{(int)CardZone.Library}|{JsonConvert.SerializeObject(new List<List<int>> { Enumerable.Range(1, 99).ToList()})}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
            key = $"{players[0].uuid}-{(int)CardZone.Graveyard}|{JsonConvert.SerializeObject(new List<List<int>> { new() { 0 } })}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
        }

        [Test]
        public void TestMoveCardToBoard()
        {
            List<PlayerDescription> players = this.StartGameXPlayers(4);
            players.Sort((x, y) => x.uuid.CompareTo(y.uuid));
            NetworkAttributeManager nam = new(players);
            for(int i = 0; i < 4; ++i)
            {
                InsertCardData cardToMove = new(i, i, i, i == 0);
                Server.SendMessage(players[0].client.GetStream(), NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-{(int)CardZone.MainField}-insert|{JsonConvert.SerializeObject(cardToMove)}");
            }
            nam.ReadPlayerData(8);
            string key = $"{players[0].uuid}-{(int)CardZone.MainField}|{JsonConvert.SerializeObject(new List<List<int>> { new() {0,1,2 }, new() { 3 } })}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
            for(int i = 0; i < 4; ++i)
            {
                Server.SendMessage(players[0].client.GetStream(), NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-{(int)CardZone.MainField}-remove|{JsonConvert.SerializeObject(i)}");
            }
            nam.ReadPlayerData(4);
            key = $"{players[0].uuid}-{(int)CardZone.MainField}|{JsonConvert.SerializeObject(new List<List<int>> {})}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
        }


        // Returns Lobby Code, UUID, Network Stream
        private (string, PlayerDescription) CreateLobby(int playerCount, string playerName)
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"{playerCount}|{playerName}");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            string[] commandData = command.instruction.Split('|');
            return (commandData[1], new PlayerDescription(playerName, commandData[0], client));
        }

        private PlayerDescription AddToLobby(string playerName, string lobbyCode)
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{lobbyCode}|{playerName}");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            Assert.IsNotNull(command);
            string[] commandData = command.instruction.Split('|');
            return new PlayerDescription(playerName, commandData[0], client);
        }

        private void HandleNetworkAttribute(List<PlayerDescription> allPlayers, PlayerDescription currentPlayer, string payload)
        {
            Server.SendMessage(currentPlayer.client.GetStream(), NetworkInstruction.NetworkAttribute, payload);
            NetworkCommand? command;
            foreach (PlayerDescription player in allPlayers)
            {
                do
                {
                    command = NetworkCommandManager.GetNextNetworkCommand(player.client.GetStream(), player.buffer, Server.bufferSize);
                    Assert.IsNotNull(command);
                } while (command != null && command.opCode != (int)NetworkInstruction.NetworkAttribute);
            }
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
                NetworkCommand? command = null;
                do
                {
                    command = NetworkCommandManager.GetNextNetworkCommand(player.client.GetStream(), player.buffer, Server.bufferSize);
                    Assert.IsNotNull(command);
                } while (command != null && command.opCode != (int) NetworkInstruction.StartGame);
            }

            returnList.ForEach(player => this.HandleNetworkAttribute(returnList,player,$"{player.uuid}-decklist|{JsonConvert.SerializeObject("100 Plains")}"));
            returnList.ForEach(player => this.HandleNetworkAttribute(returnList,player, $"{player.uuid}-ready|{JsonConvert.SerializeObject(true)}"));
            
            foreach (PlayerDescription player in returnList)
            {
                NetworkCommand? command;
                do
                {
                    command = NetworkCommandManager.GetNextNetworkCommand(player.client.GetStream(), player.buffer, Server.bufferSize);
                } while (command != null && command.opCode != (int)NetworkInstruction.NetworkAttribute);
            }

            for (int i = 0; i < playerCount; ++i)
            {
                string expectedList = JsonConvert.SerializeObject(new List<List<int>>() { Enumerable.Range(i * 100, 100).ToList()});
                foreach (PlayerDescription player in returnList)
                {
                    NetworkCommand? command;
                    do
                    {
                        command = NetworkCommandManager.GetNextNetworkCommand(player.client.GetStream(), player.buffer, Server.bufferSize);
                        Assert.IsNotNull(command);
                    } while (command != null && command.opCode != (int)NetworkInstruction.BoardUpdate);
                    Assert.IsNotNull(command);
                    string[] data = command.instruction.Split('|');
                    Assert.That(expectedList, Is.EqualTo(data[1]));
                }
            }
            returnList.Sort((x, y) => x.uuid.CompareTo(y.uuid));
            return returnList;
        }
    }
}
