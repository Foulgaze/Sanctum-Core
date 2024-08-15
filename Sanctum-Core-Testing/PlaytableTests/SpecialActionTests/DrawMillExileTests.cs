using Sanctum_Core;
using Sanctum_Core_Testing.PlaytableTests.SpecialActionTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing
{
    internal class SpecialActionTests
    {
        private Playtable playtable;
        [SetUp]
        public void Setup()
        {
            this.playtable = SpecialActionHelper.CreatePlaytable(2);
        }

        private void TestSpecialAction(SpecialAction action, int cardCount, CardZone expectedZone, List<List<int>> expectedCards)
        {
            int playerId = 0;
            this.playtable.HandleSpecialAction($"{(int)action}|{cardCount}", playerId.ToString());
            Player? player = this.playtable.GetPlayer(playerId.ToString());
            Assert.IsNotNull(player);
            CardContainerCollection container = player.GetCardContainer(expectedZone);
            Assert.That(container.ToList(), Is.EqualTo(expectedCards));
        }

        [TestCase(SpecialAction.Draw, 10, CardZone.Hand, 90, 10)]
        [TestCase(SpecialAction.Mill, 10, CardZone.Graveyard, 90, 10)]
        [TestCase(SpecialAction.Exile, 10, CardZone.Exile, 90, 10)]
        [TestCase(SpecialAction.Draw, 101, CardZone.Hand, 0, 100)]
        [TestCase(SpecialAction.Mill, 101, CardZone.Graveyard, 0, 100)]
        [TestCase(SpecialAction.Exile, 101, CardZone.Exile, 0, 100)]
        [TestCase(SpecialAction.Draw, -1, CardZone.Hand, 0, -1)]
        [TestCase(SpecialAction.Mill, -1, CardZone.Graveyard, 0, -1)]
        [TestCase(SpecialAction.Exile, -1, CardZone.Exile, 0, -1)]
        public void TestSpecialActions(SpecialAction action, int count, CardZone zone, int start, int listCount)
        {
            List<List<int>> passList = listCount <= 0 ? new() : new List<List<int>> { Enumerable.Range(start, listCount).Reverse().ToList() };
            this.TestSpecialAction(action, count, zone,passList);
        }

        private int GetCardZoneCount(CardContainerCollection collection)
        {
            List<List<int>> cards = collection.ToList();
            if (cards.Count == 0)
            {
                return 0;
            }
            return cards[0].Count;


        }

        [TestCase("5", CardZone.Library, CardZone.Hand, 100, 5)]
        [TestCase("5", CardZone.Library, CardZone.Graveyard, 100, 5)]
        [TestCase("5", CardZone.Library, CardZone.Exile, 100, 5)]
        [TestCase("invalid", CardZone.Library, CardZone.Hand, 100, 0)]
        [TestCase("-5", CardZone.Library, CardZone.Hand, 100, 0)]
        [TestCase("105", CardZone.Library, CardZone.Hand, 100, 100)]
        public void TestMoveCards(string rawCardCount, CardZone sourceZone, CardZone targetZone, int initialCardCount, int expectedMovedCardCount)
        {
            Player? player = this.playtable.GetPlayer(0.ToString());
            Assert.IsNotNull(player);
            SpecialActions.MoveCards(this.playtable, player, rawCardCount, sourceZone, targetZone);

            int sourceCount = this.GetCardZoneCount(player.GetCardContainer(sourceZone));
            int targetCount = this.GetCardZoneCount(player.GetCardContainer(targetZone));

            Assert.That(sourceCount, Is.EqualTo(initialCardCount - expectedMovedCardCount), $"Expected {expectedMovedCardCount} cards to be moved from {sourceZone}.");
            Assert.That(targetCount, Is.EqualTo(expectedMovedCardCount), $"Expected {expectedMovedCardCount} cards to be moved to {targetZone}.");
        }
    }
}
