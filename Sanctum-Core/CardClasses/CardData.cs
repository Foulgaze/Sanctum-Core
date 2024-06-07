using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core.CardClasses
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Formats.Asn1;
    using System.Globalization;
    using System.IO;
    using CsvHelper;

    public static class CardData
    {
        private static Dictionary<string, CardInfo> loadedCards = new Dictionary<string, CardInfo>();
        private static Dictionary<string, CardInfo> loadedTokens = new Dictionary<string, CardInfo>();


        public static CardInfo? GetCardInfo(string cardName)
        {
            if (loadedCards.ContainsKey(cardName))
            {
                return loadedCards[cardName];
            }
            return null;
        }

        public static void LoadCards(string pathToCards, string pathToTokens)
        {
            LoadAllCardsCSV(loadedCards, pathToCards);
            LoadAllCardsCSV(loadedTokens, pathToTokens);
        }

        public static void LoadAllCardsCSV(Dictionary<string, CardInfo> nameToCardInfo, string filePath)
        {

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
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
}
