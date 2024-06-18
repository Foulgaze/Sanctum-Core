using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly Playtable playtable = new();
        public readonly int size;
        private List<PlayerDescription> players = new();
        public readonly string code;
        public ConcurrentBag<PlayerDescription> concurrentPlayers = new();

        public Lobby(int lobbySize, string lobbyCode)
        {

            this.size = lobbySize;
            this.code = lobbyCode;
        }
        private void InitGame()
        {
            this.players = this.concurrentPlayers.ToList(); // Ignore concurrent players once lobby starts
            this.players.ForEach(description => this.playtable.AddPlayer(description.uuid, description.name));
            string lobbyDescription = JsonConvert.SerializeObject(this.players.ToDictionary(player => player.uuid, player => player.name));
            this.players.ForEach(description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.StartGame, lobbyDescription));
        }

        public void StartLobby()
        {
            this.InitGame();
            while(true)
            {

            }
        }

        public void StopLobby()
        {
            this.players.ForEach(description => description.client.Close());
        }
    }
}
