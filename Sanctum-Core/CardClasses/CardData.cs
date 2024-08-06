using CsvHelper;
using System.Globalization;

namespace Sanctum_Core
{
    public static class CardData
    {
        private static readonly Dictionary<string, CardInfo> nameToInfoStandardCards = new();
        private static readonly Dictionary<string, CardInfo> uuidToInfoTokenCards = new();

        private static readonly HashSet<string> filesLoaded = new();

        // Load all card names into a HashSet for quick lookup
        public static void LoadCardNames(string filePath, bool isLoadingTokens = false)
        {
            if(filesLoaded.Contains(filePath))
            {
                return;
            }
            if(!File.Exists(filePath))
            {
                throw new Exception($"Could not find filePath : {filePath}");
            }
            Dictionary<string, CardInfo> cardData = isLoadingTokens ? uuidToInfoTokenCards : nameToInfoStandardCards;
            _ = filesLoaded.Add(filePath);
            using StreamReader reader = new(filePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            _ = csv.Read();
            _ = csv.ReadHeader();
            while (csv.Read())
            {
                CardInfo currentCardInfo = csv.GetRecord<CardInfo>();
                if(isLoadingTokens)
                {
                    cardData[currentCardInfo.uuid] = currentCardInfo;
                    continue;
                }
                cardData[currentCardInfo.name] = currentCardInfo;
            }
        }

        // Check if a card name exists in the loaded data
        public static bool DoesCardExist(string cardIdentifier, bool isToken = false)
        {
            if (filesLoaded.Count == 0)
            {
                throw new Exception("Must load cardnames");
            }

            return isToken ? uuidToInfoTokenCards.ContainsKey(cardIdentifier) : nameToInfoStandardCards.ContainsKey(cardIdentifier);
        }

        // Get card information, either from loaded data or by loading it
        public static CardInfo? GetCardInfo(string? cardIdentifier, bool isToken = false)
        {
            if (cardIdentifier == null)
            {
                return null;
            }
            Dictionary<string, CardInfo> searchDict = isToken ? uuidToInfoTokenCards : nameToInfoStandardCards;
            return !searchDict.ContainsKey(cardIdentifier) ? null : searchDict[cardIdentifier];
        }
    }
}