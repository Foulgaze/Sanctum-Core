using CsvHelper;
using System.Globalization;

namespace Sanctum_Core
{
    public static class CardData
    {
        private static readonly Dictionary<string, CardInfo> nameToInfoStandardCards = new();
        private static readonly Dictionary<string, CardInfo> nameToInfoTokenCards = new();

        private static readonly HashSet<string> filesLoaded = new();

        // Load all card names into a HashSet for quick lookup
        public static void LoadCardNames(string filePath, bool tokens = false)
        {
            if(filesLoaded.Contains(filePath))
            {
                return;
            }
            if(!File.Exists(filePath))
            {
                throw new Exception($"Could not find filePath : {filePath}");
            }
            Dictionary<string, CardInfo> cardData = tokens ? nameToInfoTokenCards : nameToInfoStandardCards;
            _ = filesLoaded.Add(filePath);
            using StreamReader reader = new(filePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            _ = csv.Read();
            _ = csv.ReadHeader();
            while (csv.Read())
            {
                CardInfo c = csv.GetRecord<CardInfo>();
                cardData[c.name] = c;
                cardData[c.faceName] = c;
            }
        }

        // Check if a card name exists in the loaded data
        public static bool DoesCardExist(string cardName, bool isToken = false)
        {
            if (filesLoaded.Count == 0)
            {
                throw new Exception("Must load cardnames");
            }

            return isToken ? nameToInfoTokenCards.ContainsKey(cardName) : nameToInfoStandardCards.ContainsKey(cardName);
        }

        // Get card information, either from loaded data or by loading it
        public static CardInfo? GetCardInfo(string? cardName, bool isToken = false)
        {
            if (cardName == null)
            {
                return null;
            }
            Dictionary<string, CardInfo> searchDict = isToken ? nameToInfoTokenCards : nameToInfoStandardCards;
            return !searchDict.ContainsKey(cardName) ? null : searchDict[cardName];
        }
    }
}