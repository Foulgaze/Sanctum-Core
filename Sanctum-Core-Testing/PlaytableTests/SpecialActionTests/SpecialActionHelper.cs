using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing.PlaytableTests.SpecialActionTests
{
    internal class SpecialActionHelper
    {
        private static void SetupGame(Playtable table, int playerCount)
        {
            for (int i = 0; i < playerCount; ++i)
            {
                bool successfullAdd = table.AddPlayer(i.ToString(), i.ToString());
                Assert.IsTrue(successfullAdd);
                Player? player = table.GetPlayer(i.ToString());
                Assert.IsNotNull(player);
                player.ReadiedUp.SetValue(true);
            }
            Assert.IsTrue(table.GameStarted.Value);
        }
        public static Playtable CreatePlaytable(int playerCount)
        {
            string path = Path.GetFullPath(@"..\..\..\..");
            Playtable playtable = new(playerCount, $"{path}/Sanctum-Core/Assets/cards.csv", $"{path}/Sanctum-Core/Assets/tokens.csv");
            SpecialActionHelper.SetupGame(playtable, playerCount);
            return playtable;
        }
    }
}
