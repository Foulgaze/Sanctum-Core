using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing
{
    internal class PlayerTests
    {
        private readonly TestNetworkAdministrator networkAdministrator = new();
        private List<Playtable> playtables;
        [SetUp]
        public void Setup()
        {
            this.networkAdministrator.ClearLists();
        }

        [Test]
        public void DecklistTest()
        {
            int tableCount = 4;
            List<string> uuids = Enumerable.Range(0, tableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            TestHelperFunctions.AddPlayers(uuids, this.playtables);
            List<Player> relevantPlayer = TestHelperFunctions.GetRelevantPlayersFromTables(uuids, this.playtables);
            relevantPlayer.ForEach(player => player.DeckListRaw.Value = player.Uuid);
            TestHelperFunctions.GetAllPlayers(this.playtables).ForEach(player => Assert.That(player.Uuid, Is.EqualTo(player.DeckListRaw.Value)));
        }

        [Test]
        public void AddPlayerTest()
        {
            int tableCount = 4;
            List<string> uuids = Enumerable.Range(0, tableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            TestHelperFunctions.AddPlayers(uuids, this.playtables);
            List<Player> relevantPlayer = TestHelperFunctions.GetRelevantPlayersFromTables(uuids, this.playtables);
            relevantPlayer.ForEach(player => player.DeckListRaw.Value = player.Uuid);
            TestHelperFunctions.GetAllPlayers(this.playtables).ForEach(player => Assert.That(player.Uuid, Is.EqualTo(player.DeckListRaw.Value)));
        }

        [Test]
        public void InitializePlayerTest()
        {
            string uuid = Guid.NewGuid().ToString();
            string playerName = "TestPlayer";
            int startingHealth = 20;
            NetworkAttributeFactory networkAttributeFactory = new();
            CardFactory cardFactory = new(networkAttributeFactory);

            Player player = new(uuid, playerName, startingHealth, networkAttributeFactory, cardFactory);

            Assert.That(player.Uuid, Is.EqualTo(uuid));
            Assert.That(player.Name, Is.EqualTo(playerName));
            Assert.That(player.Health.Value, Is.EqualTo(startingHealth));
            Assert.That(player.DeckListRaw.Value, Is.EqualTo(""));
            Assert.That(player.ReadiedUp.Value, Is.False);
        }

        [Test]
        public void CheckDeckValidation()
        {
            string uuid = Guid.NewGuid().ToString();
            string playerName = "TestPlayer";
            int startingHealth = 20;
            NetworkAttributeFactory networkAttributeFactory = new();
            CardFactory cardFactory = new(networkAttributeFactory);

            Player player = new(uuid, playerName, startingHealth, networkAttributeFactory, cardFactory);
            player.DeckListRaw.Value = "100 Plains";
            Assert.That(100, Is.EqualTo(DeckListParser.ParseDeckList(player.DeckListRaw.Value).Count));
        }
    }
}
