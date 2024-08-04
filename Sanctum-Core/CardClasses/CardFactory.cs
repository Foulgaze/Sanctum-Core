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
        private int cardID = 0;
        private readonly List<string> twoSidedCardLayouts = new() { "meld", "transform", "modal_dfc" };
        private readonly Dictionary<int, Card> idToCard = new();
        private readonly NetworkAttributeFactory networkAttributeFactory;
        public event PropertyChangedEventHandler CardCreated = delegate { };

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

        public Card? CreateCard(string cardName, bool network = false)
        {
            CardInfo? info = CardData.GetCardInfo(cardName);
            string? backName = null;
            if (info == null)
            {
                // To do
                // Add logger
                return null;
            }
            string frontName;
            if (this.twoSidedCardLayouts.Contains(info.layout))
            {
                (frontName, backName) = this.GetFrontBackNames(info.name);
            }
            else
            {
                frontName = info.name;
            }
            CardInfo? frontInfo = CardData.GetCardInfo(frontName);
            CardInfo? backInfo = CardData.GetCardInfo(backName);
            if(frontInfo == null)
            {
                return null;
            }
            Card newCard = new(this.cardID++, frontInfo, backInfo, this.networkAttributeFactory);
            this.idToCard[newCard.Id] = newCard;
            if(network)
            {
                CardCreated(this, new PropertyChangedEventArgs(""));
            }
            return newCard;
        }

        public Card? GetCard(int cardID)
        {
            _ = this.idToCard.TryGetValue(cardID, out Card? returnCard);
            return returnCard;
        }


        private (string, string?) GetFrontBackNames(string fullName)
        {
            fullName = fullName.Trim();

            int doubleSlashIndex = fullName.IndexOf("//");
            if (doubleSlashIndex == -1)
            {
                return (fullName, null);
            }
            string frontName = fullName[..doubleSlashIndex];
            string backName = fullName[doubleSlashIndex..];
            return (frontName, backName);
        }
    }
}
