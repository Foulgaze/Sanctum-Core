using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing.CardCollectionTests
{
    [TestFixture]
    public class CardContainerTests
    {
        private CardContainer? container;
        private NetworkAttributeFactory networkAttributeFactory;
        private int cardId;

        [SetUp]
        public void SetUp()
        {
            this.cardId = 0;
            this.networkAttributeFactory = new();
        }

        

        [Test]
        public void AddCardToContainer_AddsCardAtSpecifiedPosition()
        {
            this.container = new CardContainer(null, CardZone.Library);
            this.container.AddCardToContainer(this.GenerateCard(), null);
            this.container.AddCardToContainer(this.GenerateCard(), 0);

            Assert.That(this.container.Cards.Count, Is.EqualTo(2));
            Assert.That(this.container.Cards[0].Id, Is.EqualTo(1));
            Assert.That(this.container.Cards[1].Id, Is.EqualTo(0));
        }

        [Test]
        public void GetCardIDs_ReturnsCorrectIDs()
        {
            this.container = new CardContainer(null, CardZone.Library);
            for(int i = 0; i < 3; ++i)
            {
                this.container.AddCardToContainer(this.GenerateCard(), null);
            }

            List<int> expectedIDs = new(){ 0, 1, 2 };
            List<int> actualIDs = this.container.GetCardIDs();

            CollectionAssert.AreEqual(expectedIDs, actualIDs);
        }

        [Test]
        public void IsFull_ReturnsTrueWhenFull()
        {
            this.container = new CardContainer(2, CardZone.Library);
            for(int i = 0; i < 2; ++i)
            {
                this.container.AddCardToContainer(this.GenerateCard(), null);
            }

            Assert.IsTrue(this.container.IsFull());
        }

        [Test]
        public void IsFull_ReturnsFalseWhenNotFull()
        {
            this.container = new CardContainer(3, CardZone.Library);
            this.container.AddCardToContainer(this.GenerateCard(), null);

            Assert.IsFalse(this.container.IsFull());
        }


        [Test]
        public void IsFull_ReturnsFalseWhenNotFullNoMaxSize()
        {
            this.container = new CardContainer(null, CardZone.Library);
            this.container.AddCardToContainer(this.GenerateCard(), null);

            Assert.IsFalse(this.container.IsFull());
        }

        private Card GenerateCard()
        {
            return new Card(this.cardId++, new CardInfo(), null, this.networkAttributeFactory, false, false);
        }
    }
}
