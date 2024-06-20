using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class PlayerDescription
    {
        public readonly string name;
        public readonly string uuid;
        public readonly TcpClient client;
        public readonly StringBuilder buffer = new();
        public NetworkStream stream => this.client.GetStream();
        public PlayerDescription(string name, string uuid, TcpClient client)
        {
            this.name = name;
            this.uuid = uuid;
            this.client = client;
        }
    }
    public class Lobby
    {
        private readonly Playtable playtable;
        public readonly int size;
        private List<PlayerDescription> players = new();
        public readonly string code;
        public ConcurrentBag<PlayerDescription> concurrentPlayers = new();

        public Lobby(int lobbySize, string lobbyCode)
        {

            this.size = lobbySize;
            this.code = lobbyCode;
            this.playtable = new Playtable(lobbySize);
        }

        private void NetworkAttributeChanged(object sender, PropertyChangedEventArgs args)
        {
            this.players.ForEach
                (description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.NetworkAttribute, $"{sender}|{args.PropertyName}"));
        }
        private void InitGame()
        {
            this.players = this.concurrentPlayers.ToList(); // Ignore concurrent players once lobby starts
            this.players.ForEach(description => this.playtable.AddPlayer(description.uuid, description.name));
            string lobbyDescription = JsonConvert.SerializeObject(this.players.ToDictionary(player => player.uuid, player => player.name));
            this.players.ForEach(description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.StartGame, lobbyDescription));
            this.playtable.networkAttributeFactory.attributeValueChanged += this.NetworkAttributeChanged;
        }

        public void StartLobby()
        {
            this.InitGame();
            while(true)
            {
                foreach(PlayerDescription playerDescription in  this.players)
                {
                    NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(playerDescription.client.GetStream(), playerDescription.buffer, Server.bufferSize, false);
                    this.HandleCommand(command);
                }
            }
        }

        private void HandleCommand(NetworkCommand? command)
        {
            if(command == null)
            {
                return;
            }
            switch (command.opCode)
            {
                case (int)NetworkInstruction.NetworkAttribute:
                    this.playtable.networkAttributeFactory.HandleNetworkedAttribute(command.instruction, null);
                    break;
                default:
                    // Ignore!
                    break;
            }
        }

        public void StopLobby()
        {
            this.players.ForEach(description => description.client.Close());
        }
    }
}
