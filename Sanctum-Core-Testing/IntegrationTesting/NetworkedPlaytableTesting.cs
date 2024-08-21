using Sanctum_Core;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Sanctum_Core_Server;

namespace Sanctum_Core_Testing
{
    public class NetworkedPlaytableTesting
    {
        private Server server;
        private Thread serverThread;
        private int uuidLength;
        [OneTimeSetUp]
        public void Init()
        {
            this.server = new(52521);
            this.serverThread = new Thread(new ThreadStart(this.server.StartListening)) { Name = "Server Thread" };
            this.serverThread.Start();
            this.uuidLength = Guid.NewGuid().ToString().Length;
        }

        private void CheckAttribute(List<LobbyConnection> players, NetworkStream stream,string payload)
        {
            _ = Server.SendMessage(stream, NetworkInstruction.NetworkAttribute, payload);
            foreach(LobbyConnection connection in players)
            {
                NetworkCommand? command = connection.GetNetworkCommand();
                Assert.IsNotNull(command);
                Assert.That(command.instruction, Is.EqualTo(payload));
            }
        }

        private List<LobbyConnection> StartAndSortPlayers(int playerCount)
        {
            List<LobbyConnection> players = this.StartGameXPlayers(playerCount);
            players.Sort((x, y) => x.uuid.CompareTo(y.uuid));
            return players;
        }


        [Test]
        public void NetworkAttributesTest()
        {
            Console.WriteLine("Here");
            List<LobbyConnection> players = this.StartGameXPlayers(2);
            for (int i = 0; i < players.Count; ++i)
            {
                LobbyConnection player = players[i];
                this.CheckAttribute(players, player.stream, $"{player.uuid}-health|{JsonConvert.SerializeObject(i)}");
                this.CheckAttribute(players, player.stream, $"{player.uuid}-decklist|{JsonConvert.SerializeObject($"{i}")}");
                this.CheckAttribute(players, player.stream, $"{player.uuid}-ready|{JsonConvert.SerializeObject(false)}");
            }
        }

        [Test]
        public void TestMoveCard()
        {
            List<LobbyConnection> players = this.StartAndSortPlayers(4);
            NetworkAttributeManager nam = new(players);
            InsertCardData cardToMove = new(0, 0, null, true);

            _ = Server.SendMessage(players[0].stream, NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-1-insert|{JsonConvert.SerializeObject(cardToMove)}");
            nam.ReadPlayerData(2);
            string key = $"{players[0].uuid}-{(int)CardZone.Library}-removecards|{JsonConvert.SerializeObject(new List<int>{0 })}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
            key = $"{players[0].uuid}-{(int)CardZone.Graveyard}-boardstate|{JsonConvert.SerializeObject(new List<List<int>>{ new(){0 } })}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
        }

        [Test]
        public void TestMoveCardToBoard()
        {
            List<LobbyConnection> players = this.StartAndSortPlayers(4);
            NetworkAttributeManager nam = new(players);
            for (int i = 0; i < 4; ++i)
            {
                InsertCardData cardToMove = new(i, i, i, i == 0);
                _ = Server.SendMessage(players[0].stream, NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-{(int)CardZone.MainField}-insert|{JsonConvert.SerializeObject(cardToMove)}");
            }
            nam.ReadPlayerData(8);

            string key = $"{players[0].uuid}-{(int)CardZone.MainField}-boardstate|{JsonConvert.SerializeObject(new List<List<int>> { new() { 0, 1, 2 }, new() { 3 } })}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(players.Count));
        }



        private void SendSpecialAction(List<LobbyConnection> players, NetworkAttributeManager nam, string action)
        {
            _ = Server.SendMessage(players[0].stream, NetworkInstruction.SpecialAction, action);
            nam.ReadPlayerData(2);
        }

        private void AssertNetworkAttributes(NetworkAttributeManager nam, string uuid, CardZone zone, List<int> expectedIds, int playerCount, bool checkForRemoveCards)
        {
            string keyCollection = checkForRemoveCards ? "removecards" : "boardstate";
            string keyValue = checkForRemoveCards ? JsonConvert.SerializeObject(expectedIds) : JsonConvert.SerializeObject(new List<List<int>> { expectedIds });
            string key = $"{uuid}-{(int)zone}-{keyCollection}|{keyValue}";
            Assert.That(nam.networkAttributes.Count(item => key == item), Is.EqualTo(playerCount));
        }

        [Test]
        public void TestMill()
        {
            List<LobbyConnection> players = this.StartAndSortPlayers(4);
            NetworkAttributeManager nam = new(players);

            this.SendSpecialAction(players, nam, $"{(int)SpecialAction.Mill}|10");

            this.AssertNetworkAttributes(nam, players[0].uuid, CardZone.Library, Enumerable.Range(90, 10).ToList(), players.Count, true);
            this.AssertNetworkAttributes(nam, players[0].uuid, CardZone.Graveyard, Enumerable.Range(90,10).Reverse().ToList(), players.Count, false);

        }

        [Test]
        public void TestDrawCards()
        {
            List<LobbyConnection> players = this.StartAndSortPlayers(4);
            NetworkAttributeManager nam = new(players);

            this.SendSpecialAction(players, nam, $"{(int)SpecialAction.Draw}|10");

            this.AssertNetworkAttributes(nam, players[0].uuid, CardZone.Library, Enumerable.Range(90, 10).ToList(), players.Count, true);
            this.AssertNetworkAttributes(nam, players[0].uuid, CardZone.Hand, Enumerable.Range(90, 10).Reverse().ToList(), players.Count, false);

        }

        [Test]
        public void TestExileCards()
        {
            List<LobbyConnection> players = this.StartAndSortPlayers(4);
            NetworkAttributeManager nam = new(players);

            this.SendSpecialAction(players, nam, $"{(int)SpecialAction.Exile}|10");

            this.AssertNetworkAttributes(nam, players[0].uuid, CardZone.Library, Enumerable.Range(90, 10).ToList(), players.Count, true);
            this.AssertNetworkAttributes(nam, players[0].uuid, CardZone.Exile, Enumerable.Range(90, 10).Reverse().ToList(), players.Count, false);

        }

        [Test]
        public void TestCreateToken()
        {
            List<LobbyConnection> players = this.StartAndSortPlayers(4);
            NetworkAttributeManager nam = new(players);
            string tokenUUID = "5450889c-b58f-5974-955c-b5f0d88d1338";

            _ = Server.SendMessage(players[0].stream, NetworkInstruction.SpecialAction, $"{(int)SpecialAction.CreateToken}|{tokenUUID}");

            nam.ReadPlayerData(2);
            string cardCreationKey = $"{tokenUUID}|400";
            Assert.That(nam.networkAttributes.Count(item => cardCreationKey == item), Is.EqualTo(players.Count));
            string fieldKey = $"{players[0].uuid}-{(int)CardZone.MainField}|[[400]]";
            Assert.That(nam.networkAttributes.Count(item => cardCreationKey == item), Is.EqualTo(players.Count));
        }

        [Test]
        public void TestCreateTokenNextToCard()
        {
            List<LobbyConnection> players = this.StartAndSortPlayers(4);
            NetworkAttributeManager nam = new(players);
            for (int i = 0; i < 4; ++i)
            {
                InsertCardData cardToMove = new(i, i, i, i == 0);
                _ = Server.SendMessage(players[0].stream, NetworkInstruction.NetworkAttribute, $"{players[0].uuid}-{(int)CardZone.MainField}-insert|{JsonConvert.SerializeObject(cardToMove)}");
            }
            nam.ReadPlayerData(8);
            string tokenUUID = "5450889c-b58f-5974-955c-b5f0d88d1338";

            _ = Server.SendMessage(players[0].stream, NetworkInstruction.SpecialAction, $"{(int)SpecialAction.CreateToken}|{tokenUUID}|0");

            nam.ReadPlayerData(2);
            string cardCreationKey = $"{tokenUUID}|400";
            Assert.That(nam.networkAttributes.Count(item => cardCreationKey == item), Is.EqualTo(players.Count));
            string fieldKey = $"{players[0].uuid}-{(int)CardZone.MainField}|[[3,400]]";
            Assert.That(nam.networkAttributes.Count(item => cardCreationKey == item), Is.EqualTo(players.Count));

            tokenUUID = "5450889c-b58f-5974-955c-b5f0d88d1338";

            _ = Server.SendMessage(players[0].stream, NetworkInstruction.SpecialAction, $"{(int)SpecialAction.CreateToken}|{tokenUUID}|400");

            nam.ReadPlayerData(2);
            cardCreationKey = $"{tokenUUID}|400";
            Assert.That(nam.networkAttributes.Count(item => cardCreationKey == item), Is.EqualTo(players.Count));
            fieldKey = $"{players[0].uuid}-{(int)CardZone.MainField}|[[3,400,401]]";
            Assert.That(nam.networkAttributes.Count(item => cardCreationKey == item), Is.EqualTo(players.Count));

        }


        // Returns Lobby Code, UUID, Network Stream
        private (string, LobbyConnection) CreateLobby(int playerCount, string playerName)
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            _ = Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"{playerCount}|{playerName}");
            NetworkCommand? command = new LobbyConnection("", "", client).GetNetworkCommand();
            Assert.IsNotNull(command);
            string[] commandData = command.instruction.Split('|');
            return (commandData[1], new LobbyConnection(playerName, commandData[0], client));
        }

        private LobbyConnection AddToLobby(string playerName, string lobbyCode)
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, this.server.portNumber);
            _ = Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{lobbyCode}|{playerName}");
            LobbyConnection temporaryLobbyConnection = new("", "", client);
            NetworkCommand? command = temporaryLobbyConnection.GetNetworkCommand();
            Assert.IsNotNull(command);
            string[] commandData = command.instruction.Split('|');
            return new LobbyConnection(playerName, commandData[0], client);
        }

        private void HandleNetworkAttribute(List<LobbyConnection> allPlayers, LobbyConnection currentPlayer, string payload)
        {
            _ = Server.SendMessage(currentPlayer.stream, NetworkInstruction.NetworkAttribute, payload);
            NetworkCommand? command;
            foreach (LobbyConnection connection in allPlayers)
            {
                do
                {
                    command = connection.GetNetworkCommand(timeout: 1000);
                    Assert.IsNotNull(command);
                } while (command != null && command.opCode != (int)NetworkInstruction.NetworkAttribute);
            }
        }

        private List<LobbyConnection> StartGameXPlayers(int playerCount)
        {

            Assert.That(playerCount > 0);
            (string lobbyCode, LobbyConnection lobbyPlayer) = this.CreateLobby(playerCount, "Player-0");
            List<LobbyConnection> returnList = new() { lobbyPlayer };
            for (int i = 1; i < playerCount; ++i)
            {
                TcpClient client = new();
                client.Connect(IPAddress.Loopback, this.server.portNumber);
                returnList.Add(this.AddToLobby($"Player-{i}", lobbyCode));
            }
            foreach (LobbyConnection connection in returnList)
            {
                NetworkCommand? command = null;
                do
                {
                    command = connection.GetNetworkCommand();
                    Assert.IsNotNull(command);
                } while (command != null && command.opCode != (int) NetworkInstruction.StartGame);
            }

            returnList.ForEach(player => this.HandleNetworkAttribute(returnList,player,$"{player.uuid}-decklist|{JsonConvert.SerializeObject("100 Plains")}"));
            returnList.ForEach(player => this.HandleNetworkAttribute(returnList,player, $"{player.uuid}-ready|{JsonConvert.SerializeObject(true)}"));
            
            foreach (LobbyConnection player in returnList)
            {
                NetworkCommand? command;
                do
                {
                    command = player.GetNetworkCommand();
                } while (command != null && command.opCode != (int)NetworkInstruction.NetworkAttribute);
            }
            returnList.Sort((x, y) => x.uuid.CompareTo(y.uuid));
            return returnList;
        }
    }
}
