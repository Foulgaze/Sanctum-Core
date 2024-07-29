using Newtonsoft.Json;
using Sanctum_Core;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core_Testing
{
    public class CSVTesting
    {

        [Test]
        public void LoadCSV()
        {
            string path = System.IO.Path.GetFullPath(@"..\..\..\..");
            CardData.LoadCardNames($"{path}/Sanctum-Core/Assets/cards.csv");
            Assert.IsNotNull(CardData.GetCardInfo("Plains"));
        }
    }
}
