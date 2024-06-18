using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Sanctum_Core;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            List<PlayerDescription> players = this.StartGameXPlayers(4);
            players.ForEach(desc => Server.SendMessage(desc.GetStream(), NetworkInstruction.NetworkAttribute, ))
        }


        // Returns Lobby Code, UUID, Network Stream
        private (string,PlayerDescription) CreateLobby(int playerCount, string playerName)
        {

            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"{playerCount}|{playerName}");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(),4096);
            string[] commandData = command.instruction.Split('|');
            return (commandData[1], new PlayerDescription(playerName, commandData[0], client));
        }

        private PlayerDescription AddToLobby(string playerName, string lobbyCode)
        {
            TcpClient client = new();
            client.Connect(IPAddress.Loopback, Server.portNumber);
            Server.SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, $"{lobbyCode}|{playerName}");
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), 4096);
            string[] commandData = command.instruction.Split('|');
            return new PlayerDescription(playerName, commandData[0], client);
        }

        private List<PlayerDescription> StartGameXPlayers(int playerCount)
        {

            Assert.That(playerCount > 0);
            (string lobbyCode, PlayerDescription lobbyPlayer) = this.CreateLobby(playerCount, "Player-0");
            List<PlayerDescription> returnList = new() { lobbyPlayer } ;
            for(int i = 1; i < playerCount; ++i)
            {
                TcpClient client = new();
                client.Connect(IPAddress.Loopback, Server.portNumber);
                returnList.Add(this.AddToLobby($"Player-{i}", lobbyCode));
            }
            return returnList;
            
        }
}
