using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing
{
    public class TestNetworkAdministrator
    {
        private readonly List<NetworkMock?> networkMocks = new();
        private readonly List<NetworkManager> networkManagers = new();
        private readonly List<string> networkCommands = new();
        private int startPort = 10000;

        List<Playtable> playtables;

        public void ClearLists()
        {
            this.networkMocks.Clear();
            this.networkManagers.Clear();
            this.networkCommands.Clear();
        }
        public List<Playtable> CreatePlaytables(int playerCount)
        {
            this.playtables = new();
            for (int i = 0; i < playerCount; ++i)
            {
                Playtable newTable = new(true);
                newTable.ConnectToServer("127.0.0.1", this.startPort++);
                NetworkManager manager = (NetworkManager)typeof(Playtable).GetField("_networkManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(newTable) ?? throw new Exception("Could not find Network Manager");
                NetworkStream mock = (NetworkStream)typeof(NetworkManager).GetField("rwStream", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager) ?? throw new Exception("Could not find Network Mock");
                ((NetworkMock)mock).mockSendData += this.HandleAddMessage;
                this.networkMocks.Add((NetworkMock)mock);
                this.networkManagers.Add(manager);
                this.playtables.Add(newTable);
            }
            return this.playtables;
        }

        private void HandleAddMessage(object sender, PropertyChangedEventArgs e)
        {
            string instruction = (string)sender;
            for(int i = 0; i < this.networkMocks.Count; ++i)
            {
                NetworkMock networkMock = this.networkMocks[i];
                NetworkManager networkManager = this.networkManagers[i];
                networkMock.BUFFER += instruction;
                networkManager.UpdateNetworkBuffer();
            }
            this.networkCommands.Add(instruction);
          
        }

        public Player GetXPlayerFromXBoard(int boardNumber, string uuid)
        {
            if (boardNumber >= this.playtables.Count)
            {
                throw new Exception($"Invalid board access: {boardNumber}");
            }
            Player? player = this.playtables[boardNumber].GetPlayer(uuid);
            return player ?? throw new Exception("Could not find player");
        }
    }
}
