using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing
{
    internal class TestCardContainer
    {
        private readonly TestNetworkAdministrator networkAdministrator = new();
        private List<Playtable> playtables;

        [OneTimeSetUp]
        public void Init()
        {
            CardData.LoadCardNames("cards.csv");
        }

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
            relevantPlayer.ForEach(player => player.DeckListRaw.Value = "100 Plains");
            relevantPlayer.ForEach(player => player.ReadiedUp.Value = true);
            relevantPlayer.ForEach(player => Assert.That(100, Is.EqualTo(player.GetCardContainer(CardZone.Library).Containers[0].Cards.Count)));
        }

        [Test]
        public void CorrectCardIDTest()
        {
            int playtableCount = 4;
            List<string> playerUUIDs = Enumerable.Range(0, playtableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(playtableCount);
            TestHelperFunctions.AddPlayers(playerUUIDs, this.playtables);
            List<Player> relevantPlayers = TestHelperFunctions.GetRelevantPlayersFromTables(playerUUIDs, this.playtables);
            relevantPlayers.ForEach(player => player.DeckListRaw.Value = "100 Plains");
            relevantPlayers.ForEach(player => player.ReadiedUp.Value = true);
            foreach (string playerUUID in playerUUIDs)
            {
                List<Player> playersWithUUID = TestHelperFunctions.GetAllPlayerOfUUID(playerUUID, this.playtables);
                List<int> initialCardIds = playersWithUUID.First().GetCardContainer(CardZone.Library).Containers[0].Cards.Select(card => card.Id).ToList();
                foreach (Player player in playersWithUUID)
                {
                    Assert.That(initialCardIds, Is.EqualTo(player.GetCardContainer(CardZone.Library).Containers[0].Cards.Select(card => card.Id).ToList()));
                }
            }
        }


        [Test]
        public void MoveCardBetweenContainersTest()
        {
            int playtableCount = 4;
            List<string> playerUUIDs = Enumerable.Range(0, playtableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(playtableCount);
            TestHelperFunctions.AddPlayers(playerUUIDs, this.playtables);
            List<Player> relevantPlayers = TestHelperFunctions.GetRelevantPlayersFromTables(playerUUIDs, this.playtables);
            relevantPlayers.ForEach(player => player.DeckListRaw.Value = "100 Plains");
            relevantPlayers.ForEach(player => player.ReadiedUp.Value = true);

            Player testPlayer = relevantPlayers.First();
            CardContainerCollection library = testPlayer.GetCardContainer(CardZone.Library);
            CardContainerCollection hand = testPlayer.GetCardContainer(CardZone.Hand);

            // Move first card from library to hand
            Card cardToMove = library.Containers[0].Cards[0];
            _ = library.RemoveCardFromContainer(cardToMove.Id);
            hand.InsertCardIntoContainer(0, true, cardToMove, null, false);

            Assert.That(library.Containers[0].Cards.Count, Is.EqualTo(99));
            Assert.That(hand.Containers[0].Cards.Count, Is.EqualTo(1));
            Assert.That(hand.Containers[0].Cards[0].Id, Is.EqualTo(cardToMove.Id));
        }

        [Test]
        public void CreateNewContainerIfFullTest()
        {
            int playtableCount = 4;
            List<string> playerUUIDs = Enumerable.Range(0, playtableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(playtableCount);
            TestHelperFunctions.AddPlayers(playerUUIDs, this.playtables);
            List<Player> relevantPlayers = TestHelperFunctions.GetRelevantPlayersFromTables(playerUUIDs, this.playtables);
            relevantPlayers.ForEach(player => player.DeckListRaw.Value = "100 Plains");
            relevantPlayers.ForEach(player => player.ReadiedUp.Value = true);

            Player testPlayer = relevantPlayers.First();
            CardContainerCollection library = testPlayer.GetCardContainer(CardZone.Library);
            CardContainerCollection hand = testPlayer.GetCardContainer(CardZone.Hand);

            List<Card> libraryCards = library.Containers[0].Cards;

            for (int i = 0; i < 50; i++)
            {
                
                hand.InsertCardIntoContainer(0, false, libraryCards[0],null, false);
            }

            Assert.That(library.Containers.Count, Is.EqualTo(1));
            Assert.That(hand.Containers.Count, Is.EqualTo(1));

            Assert.That(library.Containers[0].Cards.Count, Is.EqualTo(50));
            Assert.That(hand.Containers[0].Cards.Count, Is.EqualTo(50));
        }

        [Test]
        public void RemoveCards()
        {
            int playtableCount = 4;
            List<string> playerUUIDs = Enumerable.Range(0, playtableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(playtableCount);
            TestHelperFunctions.AddPlayers(playerUUIDs, this.playtables);
            List<Player> relevantPlayers = TestHelperFunctions.GetRelevantPlayersFromTables(playerUUIDs, this.playtables);
            relevantPlayers.ForEach(player => player.DeckListRaw.Value = "100 Plains");
            relevantPlayers.ForEach(player => player.ReadiedUp.Value = true);

            Player testPlayer = relevantPlayers.First();
            CardContainerCollection library = testPlayer.GetCardContainer(CardZone.Library);
            CardContainerCollection hand = testPlayer.GetCardContainer(CardZone.Hand);

            List<Card> libraryCards = library.Containers[0].Cards;

            for (int i = 0; i < 50; i++)
            {

                library.removeCardID.Value = libraryCards[0].Id;
            }
            Assert.That(library.Containers[0].Cards.Count, Is.EqualTo(50));
        }

        [Test]
        public void MoveCardWithinSameContainerTest()
        {
            int playtableCount = 4;
            List<string> playerUUIDs = Enumerable.Range(0, playtableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(playtableCount);
            TestHelperFunctions.AddPlayers(playerUUIDs, this.playtables);
            List<Player> relevantPlayers = TestHelperFunctions.GetRelevantPlayersFromTables(playerUUIDs, this.playtables);
            relevantPlayers.ForEach(player => player.DeckListRaw.Value = "100 Plains");
            relevantPlayers.ForEach(player => player.ReadiedUp.Value = true);

            Player testPlayer = relevantPlayers.First();
            CardContainerCollection library = testPlayer.GetCardContainer(CardZone.Library);

            // Move a card within the same container
            Card cardToMove = library.Containers[0].Cards[0];
            _ = library.RemoveCardFromContainer(cardToMove.Id);
            library.InsertCardIntoContainer(0, false, cardToMove, 1, false);

            Assert.That(library.Containers[0].Cards.Count, Is.EqualTo(100));
            Assert.That(library.Containers[0].Cards[1].Id, Is.EqualTo(cardToMove.Id));
        }

        [Test]
        public void TestInsertingNoNewContainer()
        {

            List<Player> relevantPlayers = this.SetupBoardForContainerTestings(4);
            Player testPlayer = relevantPlayers.First();
            CardContainerCollection mainField = testPlayer.GetCardContainer(CardZone.MainField);
            List<Card> libraryCards = testPlayer.GetCardContainer(CardZone.Library).Containers[0].Cards;

            // Fill the first container to max capacity
            for (int i = 0; i < 11; ++i)
            {
                Card card = libraryCards[0];
                mainField.InsertCardIntoContainer(0, false, card, null, true);
            }

            Assert.That(4, Is.EqualTo(mainField.Containers.Count));

        }

        [Test]
        public void TestInsertingNewContainer()
        {

            List<Player> relevantPlayers = this.SetupBoardForContainerTestings(4);
            Player testPlayer = relevantPlayers.First();
            CardContainerCollection mainField = testPlayer.GetCardContainer(CardZone.MainField);
            List<Card> libraryCards = testPlayer.GetCardContainer(CardZone.Library).Containers[0].Cards;

            // Fill the first container to max capacity
            for (int i = 0; i < 11; ++i)
            {
                Card card = libraryCards[0];
                mainField.InsertCardIntoContainer(0, true, card, null, true);
            }

            for (int i = 0; i < 11; ++i)
            {
                Card card = libraryCards[0];
                mainField.InsertCardIntoContainer(i, false, card, null, true);
            }

            Assert.That(11, Is.EqualTo(mainField.Containers.Count));

            for (int i = 0; i < 12; ++i)
            {
                Card card = libraryCards[0];
                mainField.InsertCardIntoContainer(i, false, card, null, true);
            }
            Assert.That(12, Is.EqualTo(mainField.Containers.Count));
        }

        [Test]
        public void InsertCardIntoEmptyMainField()
        {
            List<Player> relevantPlayers = this.SetupBoardForContainerTestings(4);
            Player testPlayer = relevantPlayers.First();
            CardContainerCollection mainField = testPlayer.GetCardContainer(CardZone.MainField);
            List<Card> libraryCards = testPlayer.GetCardContainer(CardZone.Library).Containers[0].Cards;

            Card cardToInsert = libraryCards[0];
            mainField.InsertCardIntoContainer(0, true, cardToInsert, null, true);

            Assert.That(mainField.Containers[0].Cards.Count, Is.EqualTo(1));
            Assert.That(mainField.Containers[0].Cards[0].Id, Is.EqualTo(cardToInsert.Id));
        }

        [Test]
        public void InsertCardAtSpecificPositionInMainField()
        {
            List<Player> relevantPlayers = this.SetupBoardForContainerTestings(4);
            Player testPlayer = relevantPlayers.First();
            CardContainerCollection mainField = testPlayer.GetCardContainer(CardZone.MainField);
            List<Card> libraryCards = testPlayer.GetCardContainer(CardZone.Library).Containers[0].Cards;

            // Insert two cards
            Card firstCard = libraryCards[0];
            Card secondCard = libraryCards[1];
            Card thirdCard = libraryCards[2];

            mainField.InsertCardIntoContainer(0, true, firstCard, null, true);
            mainField.InsertCardIntoContainer(0, false, secondCard, null, true);
            mainField.InsertCardIntoContainer(0, false, thirdCard, 1, true);

            foreach (Player player in TestHelperFunctions.GetAllPlayerOfUUID(testPlayer.Uuid, this.playtables))
            {
                Assert.That(player.GetCardContainer(CardZone.MainField).Containers[0].Cards.Count, Is.EqualTo(3));
                Assert.That(player.GetCardContainer(CardZone.MainField).Containers[0].Cards[1].Id, Is.EqualTo(thirdCard.Id));
            }
        }

        [Test]
        public void OneContainerTest()
        {
            List<Player> relevantPlayers = this.SetupBoardForContainerTestings(4);
            Player testPlayer = relevantPlayers.First();
            CardContainerCollection library = testPlayer.GetCardContainer(CardZone.Library);
            List<Card> libraryCards = library.Containers[0].Cards;

            // Fill the first container to max capacity (assuming max capacity is 10)
            for (int i = 0; i < 10; i++)
            {
                Card card = libraryCards[i];
                library.InsertCardIntoContainer(i, true, card, null, true);
            }
            foreach (Player player in TestHelperFunctions.GetAllPlayerOfUUID(testPlayer.Uuid, this.playtables))
            {
                Assert.That(player.GetCardContainer(CardZone.Library).Containers.Count, Is.EqualTo(1));
            }
        }

        private List<Player> SetupBoardForContainerTestings(int playtableCount)
        {
            List<string> playerUUIDs = Enumerable.Range(0, playtableCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            this.playtables = this.networkAdministrator.CreatePlaytables(playtableCount);
            TestHelperFunctions.AddPlayers(playerUUIDs, this.playtables);
            List<Player> relevantPlayers = TestHelperFunctions.GetRelevantPlayersFromTables(playerUUIDs, this.playtables);
            relevantPlayers.ForEach(player => player.DeckListRaw.Value = "100 Plains");
            relevantPlayers.ForEach(player => player.ReadiedUp.Value = true);
            return relevantPlayers;
        }
    }
}
