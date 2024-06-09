using Sanctum_Core;
using System.Reflection;
namespace Sanctum_Core_Testing
{
    public class Tests
    {
        private TestNetworkAdministrator networkAdministrator;
        private List<Playtable> playtables;
        [SetUp]
        public void Setup()
        {
            this.networkAdministrator = new();
            this.playtables = this.networkAdministrator.CreatePlaytables(4);
        }

        [Test]
        public void Test1()
        {
            this.playtables[0].AddPlayer("P1", "1");
            foreach (Playtable playtable in this.playtables)
            {
                
                List<Player> players = (List<Player>)typeof(Playtable).GetField("_players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(playtable) ?? throw new Exception("Could not find Network Manager");
                Assert.That(players.Count == 1);
            }
        }
    }
}