using Sanctum_Core;
using Sanctum_Core_Testing.PlaytableTests.SpecialActionTests;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.That(container.ContainerCollectionToList(), Is.EqualTo(expectedCards));
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
    }
}
