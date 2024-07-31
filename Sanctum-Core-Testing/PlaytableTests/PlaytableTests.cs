using NUnit.Framework;
using Sanctum_Core;
using System.ComponentModel;

namespace Sanctum_Core_Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private Playtable playtable;
        private readonly string filepath = "path_to_card_data_file"; // Update this path to the actual file path

        [SetUp]
        public void Setup()
        {
            string path = Path.GetFullPath(@"..\..\..\..");
            this.playtable = new Playtable(2, $"{path}/Sanctum-Core/Assets/cards.csv");
        }

        [Test]
        public void AddPlayer_ShouldReturnTrue_WhenPlayerIsAdded()
        {
            bool result = this.playtable.AddPlayer("player1", "Player One");
            Assert.IsTrue(result);
        }

        [Test]
        public void AddPlayer_ShouldReturnFalse_WhenGameHasStarted()
        {
            _ = this.playtable.AddPlayer("player1", "Player One");
            this.GetPlayer("player1").ReadiedUp.SetValue(true);

            _ = this.playtable.AddPlayer("player2", "Player Two");
            this.GetPlayer("player2").ReadiedUp.SetValue(true);

            bool result = this.playtable.AddPlayer("player3", "Player Three");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetPlayer_ShouldReturnPlayer_WhenPlayerExists()
        {
            _ = this.playtable.AddPlayer("player1", "Player One");
            Player player = this.GetPlayer("player1");
            Assert.That(player.Name, Is.EqualTo("Player One"));
        }

        [Test]
        public void GetPlayer_ShouldReturnNull_WhenPlayerDoesNotExist()
        {
            Player? player = this.playtable.GetPlayer("nonexistent");
            Assert.IsNull(player);
        }

        [Test]
        public void RemovePlayer_ShouldReturnTrue_WhenPlayerIsRemoved()
        {
            _ = this.playtable.AddPlayer("player1", "Player One");
            bool result = this.playtable.RemovePlayer("player1");

            Assert.IsTrue(result);
            Assert.IsNull(this.playtable.GetPlayer("player1"));
        }

        [Test]
        public void RemovePlayer_ShouldReturnFalse_WhenPlayerDoesNotExist()
        {
            bool result = this.playtable.RemovePlayer("nonexistent");
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdateCardZone_ShouldRaiseBoardChangedEvent()
        {
            bool eventRaised = false;
            this.playtable.boardChanged += (sender, args) => eventRaised = true;

            _ = this.playtable.AddPlayer("player1", "Player One");
            Player player = this.GetPlayer("player1");
            this.playtable.UpdateCardZone(player, CardZone.Library);

            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void CheckForStartGame_ShouldStartGame_WhenEnoughPlayersAreReady()
        {
            _ = this.playtable.AddPlayer("player1", "Player One");
            _ = this.playtable.AddPlayer("player2", "Player Two");
            Player? p1 = this.playtable.GetPlayer("player1");
            Assert.IsNotNull(p1);
            Player? p2 = this.playtable.GetPlayer("player2");
            Assert.IsNotNull(p2);
            p1.ReadiedUp.SetValue(true);
            p2.ReadiedUp.SetValue(true);

            Assert.IsTrue(this.playtable.GameStarted.Value);
        }

        private Player GetPlayer(string playerName)
        {
            Player? player = this.playtable.GetPlayer(playerName);
            Assert.IsNotNull(player);
            return player;
        }
    }
}
