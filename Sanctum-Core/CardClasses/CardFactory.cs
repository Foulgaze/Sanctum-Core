using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public static class CardFactory
    {
        private static int cardID = 0;
        private static readonly List<string> twoSidedCardLayouts = new() { "meld", "transform", "modal_dfc" };
        private static readonly Dictionary<int, Card> idToCard = new();

        /// <summary>
        /// Loads a list of cards based on the provided card names.
        /// </summary>
        /// <param name="cardNames">A list of card names to load information for.</param>
        /// <returns>A list of <see cref="Card"/> objects created from the provided card names.</returns>
        public static List<Card> LoadCardNames(List<string> cardNames)
        {
            List<Card> cards = new();
            foreach (string cardName in cardNames)
            {
                Card? newCard = CreateCard(cardName);
                if(newCard == null)
                {
                    continue;
                }
                cards.Add(newCard);
                idToCard[newCard.Id] = newCard;
            }
            return cards;
        }

        public static Card? CreateCard(string cardName)
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
            if (twoSidedCardLayouts.Contains(info.layout))
            {
                (frontName, backName) = GetFrontBackNames(info.name);
            }
            else
            {
                frontName = info.name;
            }
            Card newCard = new(cardID++, CardData.GetCardInfo(frontName), CardData.GetCardInfo(backName));
            return newCard;
        }

        public static Card? GetCard(int cardID)
        {
            _ = idToCard.TryGetValue(cardID, out Card returnCard);
            return returnCard;
        }


        private static (string, string?) GetFrontBackNames(string fullName)
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
