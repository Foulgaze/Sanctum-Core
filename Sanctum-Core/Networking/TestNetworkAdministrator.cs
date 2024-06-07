using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class TestNetworkAdministrator
    {
        private readonly List<NetworkMock> networkMocks = new();
        public TestNetworkAdministrator(List<Playtable> playtables)
        {
            foreach (Playtable table in playtables)
            {
                NetworkManager manager = (NetworkManager)typeof(Playtable).GetField("_networkManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(table);

                this.networkMocks.Add((NetworkMock)typeof(NetworkMock).GetField("rwStream", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager));
            }
        }

        private void HandleAddMessage(object sender, PropertyChangedEventArgs e)
        {
            string instruction = (string)sender;
            foreach (NetworkMock networkMock in this.networkMocks)
            {
                networkMock.BUFFER += instruction;
            }
        }
    }
}
