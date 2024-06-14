using CsvHelper;
using System.Globalization;

namespace Sanctum_Core
{
    public static class CardData
    {
        private static readonly Dictionary<string, CardInfo> loadedCards = new();
        private static bool cardsLoaded = false;

        // Load all card names into a HashSet for quick lookup
        public static void LoadCardNames(string filePath)
        {
            cardsLoaded = true;
            using StreamReader reader = new(filePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            _ = csv.Read();
            _ = csv.ReadHeader();
            while (csv.Read())
            {
                CardInfo c = csv.GetRecord<CardInfo>();
                loadedCards[c.name] = c;
                loadedCards[c.faceName] = c;
            }
        }

        // Check if a card name exists in the loaded data
        public static bool CheckCardName(string cardName)
        {
            return !cardsLoaded ? throw new Exception("Must load cardnames") : loadedCards.ContainsKey(cardName);
        }

        // Get card information, either from loaded data or by loading it
        public static CardInfo? GetCardInfo(string? cardName)
        {
            return cardName == null || !loadedCards.ContainsKey(cardName) ? null : loadedCards[cardName];
        }
    }
}
