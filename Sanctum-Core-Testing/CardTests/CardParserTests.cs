using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing.CardTests
{
    internal class CardParserTests
    {
        [Test]
        public void ParseDeckList_ValidDeckList_ReturnsParsedNames()
        {
            string deckList = "4 Lightning Bolt\n2 Counterspell\n1 Snapcaster Mage";
            List<string> expected = new()
        {
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Counterspell",
            "Counterspell",
            "Snapcaster Mage"
        };

            List<string> result = DeckListParser.ParseDeckList(deckList);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ParseDeckList_EmptyLines_IgnoresEmptyLines()
        {
            string deckList = "4 Lightning Bolt\n\n2 Counterspell\n\n1 Snapcaster Mage\n";
            List<string> expected = new()
        {
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Counterspell",
            "Counterspell",
            "Snapcaster Mage"
        };

            List<string> result = DeckListParser.ParseDeckList(deckList);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ParseDeckList_InvalidLineFormat_IgnoresInvalidLines()
        {
            string deckList = "4 Lightning Bolt\nCounterspell\n1 Snapcaster Mage";
            List<string> expected = new()
        {
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Snapcaster Mage"
        };

            List<string> result = DeckListParser.ParseDeckList(deckList);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ParseDeckList_InvalidCardCount_IgnoresInvalidLines()
        {
            string deckList = "4 Lightning Bolt\nX Counterspell\n1 Snapcaster Mage";
            List<string> expected = new()
        {
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Lightning Bolt",
            "Snapcaster Mage"
        };

            List<string> result = DeckListParser.ParseDeckList(deckList);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ParseDeckList_EscapedCharactersInCardName_ParsesCorrectly()
        {
            string deckList = "4 Lightning\\ Bolt\n2 Counterspell\n1 Snapcaster Mage";
            List<string> expected = new()
        {
            "Lightning\\ Bolt",
            "Lightning\\ Bolt",
            "Lightning\\ Bolt",
            "Lightning\\ Bolt",
            "Counterspell",
            "Counterspell",
            "Snapcaster Mage"
        };

            List<string> result = DeckListParser.ParseDeckList(deckList);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ParseDeckList_TwoFacedCards()
        {
            string deckList = "4 Lightning // Bolt\n2 Counterspell\n1 Snapcaster Mage";
            List<string> expected = new()
            {
                "Lightning",
                "Lightning",
                "Lightning",
                "Lightning",
                "Counterspell",
                "Counterspell",
                "Snapcaster Mage"
            };

            List<string> result = DeckListParser.ParseDeckList(deckList);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ParseDeckList_ValidDeckListWithSpecialCharacters_ReturnsParsedNames()
        {
            string deckList = @"
            1 Ajani's Welcome
            1 Angel's Trumpet
            1 Anointer Priest
            1 Arcane Signet
            1 Ballyrush Banneret
            1 Blood of the Martyr
            1 Brave the Elements
            1 Captain of the Watch
            1 Legion's Landing // Adanto, The First Fort
            1 Catapult Master
            1 Cathar Commando
            1 Citywide Bust
            1 Court Street Denizen
            1 Darien, King of Kjeldor
            1 Daru Warchief
            1 Daxos, Blessed by the Sun
            1 Defiler of Faith
            1 Dictate of Heliod
            1 Disenchant
            1 Distinguished Conjurer
            1 Elspeth, Sun's Champion
            1 Glorious Anthem
            1 Guide of Souls
            1 Heraldic Banner
            1 Honor of the Pure
            1 Horn of Gondor
            1 Horn of Valhalla
            1 Hour of Reckoning
            1 Impassioned Orator
            1 Intangible Virtue
            1 Jade Monolith
            1 Jinxed Idol
            1 Keeper of the Accord
            1 Knight-Captain of Eos
            1 Linden, the Steadfast Queen
            1 Lunarch Veteran
            1 Make a Stand
            1 Marble Diamond
            1 Martial Coup
            1 Mass Calcify
            1 Mentor of the Meek
            1 Mind Stone
            1 Mobilization
            1 Mondrak, Glory Dominus
            1 Myrel, Shield of Argive
            1 Myriad Landscape
            1 Nomad Stadium
            1 Nomads' Assembly
            1 Odric, Master Tactician
            1 Path of Bravery
            30 Plains
            1 Prava of the Steel Legion
            1 Rescue Retriever
            1 Return to Dust
            1 Rootborn Defenses
            1 Rumor Gatherer
            1 Sivvi's Valor
            1 Skullclamp
            1 Sol Ring
            1 Soul Warden
            1 Soul's Attendant
            1 Spear of Heliod
            1 Sunscorched Desert
            1 Suture Priest
            1 Swiftfoot Boots
            1 Swords to Plowshares
            1 Unbreakable Formation
            1 Valiant Veteran
            1 Vengeful Townsfolk
            1 Wayfarer's Bauble
            1 Windbrisk Heights
        ";

            List<string> expected = new()
        {
            "Ajani's Welcome",
            "Angel's Trumpet",
            "Anointer Priest",
            "Arcane Signet",
            "Ballyrush Banneret",
            "Blood of the Martyr",
            "Brave the Elements",
            "Captain of the Watch",
            "Legion's Landing",
            "Catapult Master",
            "Cathar Commando",
            "Citywide Bust",
            "Court Street Denizen",
            "Darien, King of Kjeldor",
            "Daru Warchief",
            "Daxos, Blessed by the Sun",
            "Defiler of Faith",
            "Dictate of Heliod",
            "Disenchant",
            "Distinguished Conjurer",
            "Elspeth, Sun's Champion",
            "Glorious Anthem",
            "Guide of Souls",
            "Heraldic Banner",
            "Honor of the Pure",
            "Horn of Gondor",
            "Horn of Valhalla",
            "Hour of Reckoning",
            "Impassioned Orator",
            "Intangible Virtue",
            "Jade Monolith",
            "Jinxed Idol",
            "Keeper of the Accord",
            "Knight-Captain of Eos",
            "Linden, the Steadfast Queen",
            "Lunarch Veteran",
            "Make a Stand",
            "Marble Diamond",
            "Martial Coup",
            "Mass Calcify",
            "Mentor of the Meek",
            "Mind Stone",
            "Mobilization",
            "Mondrak, Glory Dominus",
            "Myrel, Shield of Argive",
            "Myriad Landscape",
            "Nomad Stadium",
            "Nomads' Assembly",
            "Odric, Master Tactician",
            "Path of Bravery",
            "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains", "Plains",
            "Prava of the Steel Legion",
            "Rescue Retriever",
            "Return to Dust",
            "Rootborn Defenses",
            "Rumor Gatherer",
            "Sivvi's Valor",
            "Skullclamp",
            "Sol Ring",
            "Soul Warden",
            "Soul's Attendant",
            "Spear of Heliod",
            "Sunscorched Desert",
            "Suture Priest",
            "Swiftfoot Boots",
            "Swords to Plowshares",
            "Unbreakable Formation",
            "Valiant Veteran",
            "Vengeful Townsfolk",
            "Wayfarer's Bauble",
            "Windbrisk Heights"
        };

            List<string> result = DeckListParser.ParseDeckList(deckList);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
