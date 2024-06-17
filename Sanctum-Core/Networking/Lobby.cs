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
        private readonly int _lobbySize;
        public readonly ConcurrentBag<PlayerDescription> players = new();
        public readonly string lobbyCode;

        public Lobby(int lobbySize, string lobbyCode)
        {

            this._lobbySize = lobbySize;
            this.lobbyCode = lobbyCode;
        }

        public void StartLobby()
        {
            while (this._lobbySize != this.players.Count)
            {
                Thread.Sleep(100);
            }
            foreach (PlayerDescription playerDescription in this.players)
            {
                this.playtable.AddPlayer(playerDescription.uuid, playerDescription.name);
            }
            /*NetworkCommandManager.GetNextNetworkCommand();*/
        }

        public void StopLobby()
        {
            foreach(PlayerDescription playerDescription in this.players)
            {
                playerDescription.client.Close();
            }
        }
    }
}
