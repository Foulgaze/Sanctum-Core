using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing
{
    public class TestNetworkAdministrator
    {
        private readonly List<NetworkMock?> networkMocks = new();
        private readonly List<string> networkCommands = new();
        List<Playtable> playtables;
        public List<Playtable> CreatePlaytables(int playerCount)
        {
            this.playtables = new();
            for(int i = 0; i < playerCount; ++i)
            {
                Playtable newTable = new(true);
                NetworkManager? manager = (NetworkManager?)typeof(Playtable).GetField("_networkManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(newTable) ?? throw new Exception("Could not find Network Manager");
                NetworkMock? mock = (NetworkMock?)typeof(NetworkMock).GetField("rwStream", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager) ?? throw new Exception("Could not find Network Mock");
                this.networkMocks.Add(mock);
                this.playtables.Add(newTable);
            }
            return this.playtables;
        }

        private void HandleAddMessage(object sender, PropertyChangedEventArgs e)
        {
            string instruction = (string)sender;
            foreach (NetworkMock networkMock in this.networkMocks)
            {
                networkMock.BUFFER += instruction;
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
