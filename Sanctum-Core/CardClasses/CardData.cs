using CsvHelper;
using System.Globalization;

namespace Sanctum_Core
{
    public static class CardData
    {
        private static readonly Dictionary<string, CardInfo> loadedCards = new();
        private static readonly HashSet<string> cardNames = new();

        // Load all card names into a HashSet for quick lookup
        public static void LoadCardNames(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var c = csv.GetRecord<CardInfo>();
                    loadedCards[c.name] = c;
                    loadedCards[c.faceName] = c;
                }
            }
        }

        // Check if a card name exists in the loaded data
        public static bool CheckCardName(string cardName)
        {
            return loadedCards.ContainsKey(cardName);
        }

        // Get card information, either from loaded data or by loading it
        public static CardInfo GetCardInfo(string cardName)
        {
            return loadedCards[cardName];
        }
    }
}
