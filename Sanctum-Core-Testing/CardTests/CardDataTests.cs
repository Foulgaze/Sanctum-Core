using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing.CardTests
{
    internal class CardDataTests
    {
        [SetUp]
        public void InitData()
        {
            string path = System.IO.Path.GetFullPath(@"..\..\..\..");
            CardData.LoadCardNames($"{path}/Sanctum-Core/Assets/cards.csv");
        }

        [Test]
        public void VerifyRandomCardsExist()
        {
            string path = System.IO.Path.GetFullPath(@"..\..\..\..\Sanctum-Core-Testing\Assets\CardTests\Cards.txt");
            string rawCardList = File.ReadAllText(path);
            foreach(string cardName in rawCardList.Split('\n'))
            {
                Assert.IsNotNull(CardData.GetCardInfo(cardName.Trim()));
            }
        }

        [Test]
        public void VerifyCardInfo_Darien()
        {
            string cardName = "Darien, King of Kjeldor";

            CardInfo? info = CardData.GetCardInfo(cardName);
            Assert.IsNotNull(info);

            Assert.That(info.power, Is.EqualTo("3"));
            Assert.That(info.toughness, Is.EqualTo("3"));
            Assert.That(info.name, Is.EqualTo(cardName));
            Assert.That(info.type, Is.EqualTo("Legendary Creature — Human Soldier"));
        }

        [Test]
        public void VerifyCardInfo_LlanowarElves()
        {
            string cardName = "Llanowar Elves";

            CardInfo? info = CardData.GetCardInfo(cardName);
            Assert.IsNotNull(info);

            Assert.That(info.power, Is.EqualTo("1"));
            Assert.That(info.toughness, Is.EqualTo("1"));
            Assert.That(info.name, Is.EqualTo(cardName));
            Assert.That(info.type, Is.EqualTo("Creature — Elf Druid"));
        }

        [Test]
        public void VerifyCardInfo_Mountain()
        {
            string cardName = "Mountain";

            CardInfo? info = CardData.GetCardInfo(cardName);
            Assert.IsNotNull(info);

            Assert.That(info.power, Is.EqualTo(string.Empty));
            Assert.That(info.toughness, Is.EqualTo(string.Empty));
            Assert.That(info.name, Is.EqualTo(cardName));
            Assert.That(info.type, Is.EqualTo("Basic Land — Mountain"));
        }

        [Test]
        public void VerifyCardInfo_BlackLotus()
        {
            string cardName = "Black Lotus";

            CardInfo? info = CardData.GetCardInfo(cardName);
            Assert.IsNotNull(info);

            Assert.That(info.power, Is.EqualTo(string.Empty));
            Assert.That(info.toughness, Is.EqualTo(string.Empty));
            Assert.That(info.name, Is.EqualTo(cardName));
            Assert.That(info.type, Is.EqualTo("Artifact"));
        }

        [Test]
        public void VerifyCardInfo_ThaliaGuardianOfThraben()
        {
            string cardName = "Thalia, Guardian of Thraben";

            CardInfo? info = CardData.GetCardInfo(cardName);
            Assert.IsNotNull(info);

            Assert.That(info.power, Is.EqualTo("2"));
            Assert.That(info.toughness, Is.EqualTo("1"));
            Assert.That(info.name, Is.EqualTo(cardName));
            Assert.That(info.type, Is.EqualTo("Legendary Creature — Human Soldier"));
        }

        [Test]
        public void VerifyCardInfo_SerraAngel()
        {
            string cardName = "Serra Angel";

            CardInfo? info = CardData.GetCardInfo(cardName);
            Assert.IsNotNull(info);

            Assert.That(info.power, Is.EqualTo("4"));
            Assert.That(info.toughness, Is.EqualTo("4"));
            Assert.That(info.name, Is.EqualTo(cardName));
            Assert.That(info.type, Is.EqualTo("Creature — Angel"));
        }
    }
}
