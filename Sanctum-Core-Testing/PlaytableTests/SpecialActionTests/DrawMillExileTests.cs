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
        

        [Test]
        public void TestDrawCards()
        {
        }

        [Test]
        public void TestDrawCardsTooManyCards()
        {

        }
        [Test]
        public void TestDrawCardsNegative()
        {
            return;
        }
    }
}
