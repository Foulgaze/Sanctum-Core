using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{ 
    public static class CardData
    {
        private static readonly Dictionary<string, CardInfo> loadedCards = new();
        private static readonly Dictionary<string, CardInfo> loadedTokens = new();


        public static CardInfo? GetCardInfo(string cardName)
        {
            return loadedCards.ContainsKey(cardName) ? loadedCards[cardName] : null;
        }

        public static void LoadCards(string pathToCards, string pathToTokens)
        {
            LoadAllCardsCSV(loadedCards, pathToCards);
            LoadAllCardsCSV(loadedTokens, pathToTokens);
        }

        public static void LoadAllCardsCSV(Dictionary<string, CardInfo> nameToCardInfo, string filePath)
        {
            using StreamReader reader = new(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                var c = csv.GetRecord<CardInfo>();
                nameToCardInfo[c.name] = c;
                nameToCardInfo[c.faceName] = c;
            }
        }

    }
}
