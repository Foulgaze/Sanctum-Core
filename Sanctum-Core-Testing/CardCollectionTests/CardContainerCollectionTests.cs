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
            this.cardFactory = new(this.networkAttributeFactory);
            this.idToCard = this.GetCardFactoryDictionary();
            this.cardContainerCollection = new(
                CardZone.Library,
                "Player1",
                3,
                5,
                false,
                this.networkAttributeFactory,
                this.cardFactory);
            this.cardId = 0;
        }

        [Test]
        public void InsertCardIntoContainer_ShouldAddCard_WhenChangeShouldBeNetworkedIsFalse()
        {
            Card card = this.GenerateCard();

            this.cardContainerCollection.InsertCardIntoContainer(null, true, card, null, false);

            List<CardContainer> containers = this.GetCardContainers(this.cardContainerCollection);

            Assert.That(containers.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards.Count, Is.EqualTo(1));
            Assert.That(containers[0].Cards[0].Id, Is.EqualTo(card.Id));
        }

        [Test]
        public void RemoveCardFromContainer_ShouldRaiseBoardChangedEvent_WhenCardIsRemoved()
        {
            bool eventRaised = false;
            this.cardContainerCollection.boardChanged += (sender, args) =>
            {
                eventRaised = true;
                Assert.That(args.PropertyName, Is.EqualTo("removed"));
            };
            Card card = this.GenerateCard();
            this.cardContainerCollection.InsertCardIntoContainer(null, true, card, null, false);

            _ = this.cardContainerCollection.RemoveCardFromContainer(card.Id);

            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void GetTopCard_ShouldReturnTopCard_WhenContainersAreNotEmpty()
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
        public void GetTopCard_ShouldReturnNull_WhenContainersAreEmpty()
        {
            Card? topCard = this.cardContainerCollection.GetTopCard();

            Assert.IsNull(topCard);
        }

        private Card GenerateCard()
        {
            int id = this.cardId++;
            this.idToCard[id] = new Card(id, new CardInfo(), null, this.networkAttributeFactory);
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

