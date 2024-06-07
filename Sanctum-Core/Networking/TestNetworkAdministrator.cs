using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core.Networking
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    public class TestNetworkAdministrator
    {
        private List<NetworkMock> networkMocks = new List<NetworkMock>();
        private string BUFFER = "";

        public TestNetworkManager(List<Playtable> playtables)
        {
            foreach (Playtable table in playtables)
            {
                NetworkManager manager = (NetworkManager)typeof(Playtable).GetField("_networkManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(table);

                networkMocks.Add((NetworkMock)typeof(NetworkMock).GetField("rwStream", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager));
            }
        }

        private void HandleAddMessage(object sender, PropertyChangedEventArgs e)
        {
            string instruction = (string)sender;
            foreach (NetworkMock networkMock in networkMocks)
            {
                networkMock.BUFFER += instruction;
            }
        }
    }
}
