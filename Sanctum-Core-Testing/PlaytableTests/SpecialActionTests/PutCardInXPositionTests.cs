using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing.PlaytableTests.SpecialActionTests
{
    internal class PutCardInXPositionTests
    {
        private readonly Playtable playtable = SpecialActionHelper.CreatePlaytable(2);
        private readonly CardFactory cardFactory;


        [TestCase(0, 0, "top")]
        [TestCase(99, 5, "bottom")]
        [TestCase(10, 3, "bottom")]
        [TestCase(1, 0, "top")]      // Edge case: Zero Distance
        [TestCase(2, 0, "bottom")]   // Edge case: Zero Distance
        [TestCase(3, 99, "top")]     // Edge case: Maximum Distance
        [TestCase(4, 99, "bottom")]  // Edge case: Maximum Distance
        [TestCase(5, 1000, "top")]   // Edge case: Distance Greater than Library Size
        [TestCase(6, 1000, "bottom")]// Edge case: Distance Greater than Library Size
        [TestCase(7, -1, "top", true)]     // Failing case: Negative Distance
        [TestCase(8, -1, "bottom", true)]  // Failing case: Negative Distance
        [TestCase(9, 5, "middle", true)]   // Failing case: Invalid Placement
        [TestCase(-1, 5, "top", true)]     // Failing case: Non-Existent Card ID
        [TestCase(-1, 5, "bottom", true)]  // Failing case: Non-Existent Card ID
        public void TestPutCardXFromTopOrBottom(int cardId, int cardDistance, string expectedPlacement, bool testCaseShouldFail = false)
        {
            Player? player1 = this.playtable.GetPlayer(0.ToString());
            Assert.IsNotNull(player1);
            // Setup initial conditions
            CardContainerCollection library = player1.GetCardContainer(CardZone.Library);

            // Execute the method
            bool putXCardResult = SpecialActions.PutCardXFromTopOrBottom(this.playtable.cardFactory, library, $"{expectedPlacement}|{cardId}|{cardDistance}");

            List<List<int>> librarySerialized = library.ToList();
            Assert.That(librarySerialized.Count, Is.EqualTo(1));
            cardDistance = Math.Clamp(cardDistance, 0, librarySerialized[0].Count - 1);
            int resultPosition = expectedPlacement == "top" ? librarySerialized[0].Count - 1 - cardDistance : 0 + cardDistance;
            if (testCaseShouldFail)
            {
                Assert.That(putXCardResult, Is.EqualTo(!testCaseShouldFail));
            }
            else
            {
                Assert.That(librarySerialized[0][resultPosition], Is.EqualTo(cardId));
            }
        }
    }
}
