using Sanctum_Core;
using System.Reflection;
namespace Sanctum_Core_Testing
{
    public class Tests
    {
        private readonly TestNetworkAdministrator networkAdministrator = new();
        private List<Playtable> playtables;
        [SetUp]
        public void Setup()
        {
            this.networkAdministrator.ClearLists();
        }

        [Test]
        public void AddPlayerTest()
        {
            int tableCount = 4;
            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            List<string> uuids = Enumerable.Range(0, tableCount).Select(_ => Guid.NewGuid().ToString()).ToList();
            for(int i = 0; i < uuids.Count; i++)
            {
                string uuid = uuids[i];
                Playtable playtable = this.playtables[i];
                playtable.AddOrRemovePlayer(uuid, uuid, true);
                foreach (Playtable itertable in this.playtables)
                {
                    List<Player> players = (List<Player>)typeof(Playtable).GetField("_players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(itertable) ?? throw new Exception("Could not find players");
                    Assert.AreEqual(players.Count, i + 1);
                    Player? addedPlayer = itertable.GetPlayer(uuid);
                    Assert.IsNotNull(addedPlayer);
                }
            }
        }

        [Test]
        public void RemovePlayerTest()
        {
            int tableCount = 4;
            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            List<string> uuids = Enumerable.Range(0, tableCount).Select(_ => Guid.NewGuid().ToString()).ToList();
            for (int i = 0; i < uuids.Count; i++)
            {
                string uuid = uuids[i];
                Playtable playtable = this.playtables[i];
                playtable.AddOrRemovePlayer(uuid, uuid, true);
            }

            for (int i = uuids.Count - 1; i > -1; --i)
            {
                string uuid = uuids[i];
                Playtable playtable = this.playtables[i];
                playtable.AddOrRemovePlayer(uuid, uuid, false);
                foreach (Playtable itertable in this.playtables)
                {
                    List<Player> players = (List<Player>)typeof(Playtable).GetField("_players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(itertable) ?? throw new Exception("Could not find players");
                    Assert.AreEqual(players.Count, i);
                }
            }
        }

        [Test]
        public void ReadyUpTest()
        {
            int tableCount = 4;
            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            List<string> uuids = Enumerable.Range(0, tableCount).Select(_ => Guid.NewGuid().ToString()).ToList();
            for (int i = 0; i < uuids.Count; i++)
            {
                string uuid = uuids[i];
                Playtable playtable = this.playtables[i];
                playtable.AddOrRemovePlayer(uuid, uuid, true);
                Player playerToReady = playtable.GetPlayer(uuid);
                playerToReady.ReadiedUp.Value = true;
            }
            foreach (Playtable table in this.playtables)
            {
                Assert.IsTrue(table.GameStarted);
                foreach (string uuid in uuids)
                {
                    Assert.IsTrue(table.GetPlayer(uuid).ReadiedUp.Value);
                }
            }
        }

        [Test]
        public void AddDuplicatePlayerTest()
        {
            int tableCount = 1;
            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            string uuid = Guid.NewGuid().ToString();
            Playtable playtable = this.playtables.First();
            playtable.AddOrRemovePlayer(uuid, uuid, true);
            playtable.AddOrRemovePlayer(uuid, uuid, true);

            List<Player> players = (List<Player>)typeof(Playtable).GetField("_players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(playtable) ?? throw new Exception("Could not find players");
            Assert.AreEqual(players.Count, 1); // Should only have one player
        }

        [Test]
        public void StartGameWithoutEnoughPlayers()
        {
            int tableCount = 1;
            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            string uuid1 = Guid.NewGuid().ToString();
            string uuid2 = Guid.NewGuid().ToString();
            Playtable playtable = this.playtables.First();

            playtable.AddOrRemovePlayer(uuid1, uuid1, true);
            playtable.AddOrRemovePlayer(uuid2, uuid2, true);

            Player player1 = playtable.GetPlayer(uuid1);
            Player player2 = playtable.GetPlayer(uuid2);

            player1.ReadiedUp.Value = true;
            player2.ReadiedUp.Value = true;

            Assert.IsFalse(playtable.GameStarted); // Game should not start as only 2 players ready and 4 needed
        }

        [Test]
        public void StartGameWithExactNumberOfPlayers()
        {
            int tableCount = 1;
            this.playtables = this.networkAdministrator.CreatePlaytables(tableCount);
            List<string> uuids = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid().ToString()).ToList();
            Playtable playtable = this.playtables.First();

            foreach (string uuid in uuids)
            {
                playtable.AddOrRemovePlayer(uuid, uuid, true);
                Player playerToReady = playtable.GetPlayer(uuid);
                playerToReady.ReadiedUp.Value = true;
            }

            Assert.IsTrue(playtable.GameStarted);
        }
    }
}