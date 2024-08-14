using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class CardFactory
    {
        private class CardIdCounter
        {
            private int cardID = 0;
            public int GetNextCardId()
            {
                return this.cardID++;
            }
        }
        private readonly CardIdCounter cardIdCounter = new();
        private readonly List<string> twoSidedCardLayouts = new() { "meld", "transform", "modal_dfc" };
        private readonly Dictionary<int, Card> idToCard = new();
        private readonly NetworkAttributeFactory networkAttributeFactory;
        public event PropertyChangedEventHandler cardCreated = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="CardFactory"/> class with the specified network attribute factory.
        /// </summary>
        /// <param name="networkAttributeFactory">The factory for creating network attributes.</param>
        public CardFactory(NetworkAttributeFactory networkAttributeFactory)
        {
            this.networkAttributeFactory = networkAttributeFactory;
        }


        /// <summary>
        /// Loads a list of cards based on the provided card names.
        /// </summary>
        /// <param name="cardNames">A list of card names to load information for.</param>
        /// <returns>A list of <see cref="Card"/> objects created from the provided card names.</returns>
        public List<Card> LoadCardNames(List<string> cardNames)
        {
            List<Card> cards = new();
            foreach (string cardName in cardNames)
            {
                Card? newCard = this.CreateCard(cardName);
                if(newCard == null)
                {
                    continue;
                }
                cards.Add(newCard);
            }
            return cards;
        }

        /// <summary>
        /// Creates a new card based on the specified card identifier and optional parameters.
        /// </summary>
        /// <param name="cardIdentifier">The identifier (name or UUID) of the card to create.</param>
        /// <param name="isTokenCard">Indicates whether the card is a token. Default is false.</param>
        /// <param name="changeShouldBeNetworked">Indicates whether the card creation should be networked. Default is false.</param>
        /// <returns>The created <see cref="Card"/> object, or null if the card could not be created.</returns>
        public Card? CreateCard(string cardIdentifier, bool isTokenCard = false, bool changeShouldBeNetworked = false)
        {
            CardInfo? info = CardData.GetCardInfo(cardIdentifier, isTokenCard);
            string? backName = null;
            if (info == null)
            {
                // To do
                // Add logger
                Logger.LogError($"Unable to find card from identifier {cardIdentifier} - isToken : {isTokenCard}");
                return null;
            }
            string frontName;
            if (this.twoSidedCardLayouts.Contains(info.layout))
            {
                (frontName, backName) = this.GetFrontBackNames(info, isTokenCard);
            }
            else
            {
                frontName = isTokenCard ? info.uuid : info.name;
            }
            CardInfo? frontInfo = CardData.GetCardInfo(frontName, isTokenCard);
            CardInfo? backInfo = CardData.GetCardInfo(backName, isTokenCard);
            if(frontInfo == null)
            {
                return null;
            }
            Card newCard = new(this.cardIdCounter.GetNextCardId(), frontInfo, backInfo, this.networkAttributeFactory, isTokenCard);
            this.idToCard[newCard.Id] = newCard;
            if(changeShouldBeNetworked)
            {
                cardCreated(newCard, new PropertyChangedEventArgs(""));
            }
            return newCard;
        }

        /// <summary>
        /// Creates a new card as a copy of an existing card.
        /// </summary>
        /// <param name="cardToCopy">The card to copy.</param>
        /// <returns>The created <see cref="Card"/> object, or null if the card could not be created.</returns>
        public Card? CreateCard(Card cardToCopy)
        {
            return new Card(this.cardIdCounter.GetNextCardId(),this.networkAttributeFactory, cardToCopy);
        }

        /// <summary>
        /// Retrieves a card based on its unique identifier.
        /// </summary>
        /// <param name="cardID">The unique identifier of the card to retrieve.</param>
        /// <returns>The <see cref="Card"/> object with the specified ID, or null if not found.</returns>
        public Card? GetCard(int cardID)
        {
            _ = this.idToCard.TryGetValue(cardID, out Card? returnCard);
            return returnCard;
        }


        private (string, string?) GetFrontBackNames(CardInfo info, bool isTokenCard = false)
        {
            string fullName = info.name.Trim();

            int doubleSlashIndex = fullName.IndexOf("//");
            if (doubleSlashIndex == -1)
            {
                return isTokenCard ? (info.uuid, null) : (fullName, null);
            }
            string frontName = fullName[..doubleSlashIndex];
            string backName = fullName[doubleSlashIndex..];
            if (isTokenCard) // This basically assumes that you'll only create tokens starting with front face
            {
                return (info.uuid, info.otherFace);
            }
            return (frontName, backName);
        }
    }
}
