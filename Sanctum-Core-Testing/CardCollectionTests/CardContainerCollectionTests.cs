using Newtonsoft.Json;
using Sanctum_Core;
using System.Reflection;

namespace Sanctum_Core_Testing.CardCollectionTests
{
    public class CardContainerCollectionTests
    {
        private NetworkAttributeFactory networkAttributeFactory;
        private CardFactory cardFactory;
        private CardContainerCollection cardContainerCollection;
        private int cardId;
        private Dictionary<int, Card> idToCard;

        [SetUp]
        public void Setup()
        {
            this.networkAttributeFactory = new();
            this.cardFactory = new(this.networkAttributeFactory, false);
            this.idToCard = this.GetCardFactoryDictionary();
            this.cardContainerCollection = new(
                CardZone.Library,
                "Player1",
                3,
                5,
                false,
                this.networkAttributeFactory,
                this.cardFactory, false);
            this.cardId = 0;
        }

        [Test]
        public void InsertCardIntoContainerFalseNetworked()
        {
            Card card = this.GenerateCard();

            this.cardContainerCollection.InsertCardIntoContainer(null, true, card, null, false);

            bool eventRaised = false;
            this.cardContainerCollection.boardState.valueChanged += (_) => eventRaised = true;

            List<CardContainer> containers = this.GetCardContainers(this.cardContainerCollection);

            Assert.That(containers.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards[0].Id, Is.EqualTo(card.Id));
            Assert.IsFalse(eventRaised);
        }

        [Test]
        public void RemoveCardFromContainerNetworked()
        {
            bool eventRaised = false;
            this.cardContainerCollection.removeCardIds.valueChanged += (attribute) =>
            {
                eventRaised = true;
                Assert.That(attribute.SerializedValue, Is.EqualTo(JsonConvert.SerializeObject(new List<int>() { 0 })));
            };
            Card card = this.GenerateCard();
            this.cardContainerCollection.InsertCardIntoContainer(null, true, card, null, true);

            _ = this.cardContainerCollection.RemoveCardFromContainer(card.Id);

            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void GetTopCard()
        {
            Card card1 = this.GenerateCard();
            Card card2 = this.GenerateCard();
            this.cardContainerCollection.InsertCardIntoContainer(null, true, card1, null, false);
            this.cardContainerCollection.InsertCardIntoContainer(null, true, card2, null, false);
            Card? topCard = this.cardContainerCollection.GetTopCard();
            Assert.IsNotNull(topCard);
            Assert.That(topCard.Id, Is.EqualTo(card2.Id));
        }

        [Test]
        public void GetTopCardNull()
        {
            Card? topCard = this.cardContainerCollection.GetTopCard();

            Assert.IsNull(topCard);
        }

        [Test]
        public void CheckCardCount()
        {
            Card card = this.GenerateCard();

            Assert.That(this.cardContainerCollection.GetTotalCardCount(), Is.EqualTo(0));
            this.cardContainerCollection.InsertCardIntoContainer(null, true, card, null, false);

            List<CardContainer> containers = this.GetCardContainers(this.cardContainerCollection);

            Assert.That(containers.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards[0].Id, Is.EqualTo(card.Id));
            Assert.That(this.cardContainerCollection.GetTotalCardCount(), Is.EqualTo(1));

        }

        [Test]
        public void MoveEtherealCardToLibrary()
        {
            Card card = this.GenerateCard();
            card.isEthereal = true;
            Assert.That(this.cardContainerCollection.GetTotalCardCount(), Is.EqualTo(0));
            this.cardContainerCollection.InsertCardIntoContainer(null, true, card, null, false);

            List<CardContainer> containers = this.GetCardContainers(this.cardContainerCollection);

            Assert.That(containers.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards.Count, Is.EqualTo(0));
            Assert.That(this.cardContainerCollection.GetTotalCardCount(), Is.EqualTo(0));

        }

        [Test]
        public void MoveEtherealCardToField()
        {
            Card card = this.GenerateCard();
            card.isEthereal = true;
            this.cardContainerCollection.Zone = CardZone.MainField;
            Assert.That(this.cardContainerCollection.GetTotalCardCount(), Is.EqualTo(0));
            this.cardContainerCollection.InsertCardIntoContainer(null, true, card, null, false);
            this.cardContainerCollection.Zone = CardZone.Library;

            List<CardContainer> containers = this.GetCardContainers(this.cardContainerCollection);

            Assert.That(containers.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards.Count, Is.EqualTo(1));
            Assert.That(this.cardContainerCollection.GetTotalCardCount(), Is.EqualTo(1));
        }

        [Test]
        public void StackCards()
        {
            List<Card> cards = Enumerable.Range(0, 15).Select(x => this.GenerateCard()).ToList();
            foreach (Card card in cards)
            {
                this.cardContainerCollection.InsertCardIntoContainer(null, false, card, null, true);
            }
            Assert.That(this.cardContainerCollection.GetTotalCardCount(), Is.EqualTo(15));
            List<CardContainer> containers = this.GetCardContainers(this.cardContainerCollection);
            Assert.That(containers.Count, Is.EqualTo(3));
            foreach (CardContainer container in containers)
            {
                Assert.That(container.GetCardIDs().Count, Is.EqualTo(5));
            }
        }

        private Card GenerateCard()
        {
            int id = this.cardId++;
            this.idToCard[id] = new Card(id, new CardInfo(), null, this.networkAttributeFactory, false, false);
            return this.idToCard[id];
        }

        private Dictionary<int, Card> GetCardFactoryDictionary()
        {
            FieldInfo? idToCardField = typeof(CardFactory).GetField("idToCard", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(idToCardField);
            Assert.IsNotNull(this.cardFactory);
            Dictionary<int, Card>? idToCard = idToCardField.GetValue(this.cardFactory) as Dictionary<int, Card>;
            Assert.IsNotNull(idToCard);
            return idToCard;
        }

        private List<CardContainer> GetCardContainers(CardContainerCollection collection)
        {
            FieldInfo? containersField = typeof(CardContainerCollection).GetField("Containers", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(containersField);
            List<CardContainer>? containers = containersField.GetValue(collection) as List<CardContainer>;
            Assert.IsNotNull(containers);

            return containers;
        }
    }
}

