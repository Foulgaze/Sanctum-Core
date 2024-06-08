using Sanctum_Core;
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
            playtables[0].decklist
        }

        private void GetPlayerX(int player)
        {
            if()
        }
    }
}