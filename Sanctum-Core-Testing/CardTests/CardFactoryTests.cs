using NUnit.Framework.Internal;
using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing.CardTests
{
    internal class CardFactoryTests
    {
        private NetworkAttributeFactory networkAttributeFactory;
        private CardFactory cardFactory;

        [SetUp]
        public void SetUp()
        {
            string path = System.IO.Path.GetFullPath(@"..\..\..\..");
            CardData.LoadCardNames($"{path}/Sanctum-Core/Assets/cards.csv");
            this.networkAttributeFactory = new();
            this.cardFactory = new CardFactory(this.networkAttributeFactory, false);
        }

        [Test]
        public void LoadCardTransform()
        {
            // Arrange
            List<string> meldCards = new()
            {
                "Bruna, the Fading Light",
                "Gisela, the Broken Blade"
            };

            List<string> transformCards = new()
            {
                "Delver of Secrets",
                "Arlinn Kord",
                "Huntmaster of the Fells",
                "Westvale Abbey",
                "Thing in the Ice"
            };

            List<string> modalDoubleFacedCards = new()
            {
                "Valki, God of Lies",
                "Halvar, God of Battle",
                "Esika, God of the Tree",
                "Kazandu Mammoth"
            };

            // Combine all card names into a single list
            List<string> cardNames = new();
            cardNames.AddRange(meldCards);
            cardNames.AddRange(transformCards);
            cardNames.AddRange(modalDoubleFacedCards);

            // Act
            List<Card> cards = this.cardFactory.LoadCardNames(cardNames);

            // Assert
            Assert.That(cards.Count, Is.EqualTo(cardNames.Count), "Not all cards were loaded.");
            Assert.That(cards.FirstOrDefault(card => card.CurrentInfo.name == "Westvale Abbey").BackInfo.name, Is.EqualTo("Ormendahl, Profane Prince"));
            // Check that all loaded cards have a backside
            foreach (Card card in cards)
            {
                Assert.That(card.HasBackside(), Is.True, $"Card {card.CurrentInfo.name} does not have a backside.");
            }
            Assert.That(this.cardFactory.LoadCardNames(new List<string> { "Plains" })[0].HasBackside(), Is.False);
        }

        [Test]
        public void LoadCardNames_ValidNames_ReturnsCards()
        {
            // Arrange
            List<string> cardNames = new(){ "Plains", "Swamp" };

            // Act
            List<Card> cards = this.cardFactory.LoadCardNames(cardNames);

            // Assert
            Assert.That(cards.Count, Is.EqualTo(2));
        }

        [Test]
        public void LoadCardNames_SomeInvalidNames_ReturnsOnlyValidCards()
        {
            // Arrange
            List<string> cardNames = new() { "Plains", "InvalidCard" };

            // Act
            List<Card> cards = this.cardFactory.LoadCardNames(cardNames);

            // Assert
            Assert.That(cards.Count, Is.EqualTo(1));
        }

        [Test]
        public void CreateCard_InvalidCardName_ReturnsNull()
        {
            // Arrange
            string cardName = "InvalidCard";

            // Act
            Card? card = this.cardFactory.CreateCard(cardName);

            // Assert
            Assert.IsNull(card);
        }

        [Test]
        public void GetCard_ExistingCardID_ReturnsCard()
        {
            // Arrange
            Card? card = this.cardFactory.CreateCard("Plains");
            Assert.IsNotNull(card);
            int cardID = card.Id;

            // Act
            Card? retrievedCard = this.cardFactory.GetCard(cardID);

            // Assert
            Assert.IsNotNull(retrievedCard);
            Assert.That(retrievedCard.Id, Is.EqualTo(cardID));
        }

        [Test]
        public void GetCard_NonExistingCardID_ReturnsNull()
        {
            // Act
            Card? retrievedCard = this.cardFactory.GetCard(999); // Assuming this ID doesn't exist

            // Assert
            Assert.IsNull(retrievedCard);
        }
    }
}
