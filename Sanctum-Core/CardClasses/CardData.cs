using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Sanctum_Core
{
    public static class CardData
    {
        private static readonly Dictionary<string, CardInfo> nameToInfoStandardCards = new Dictionary<string, CardInfo>();
        private static readonly Dictionary<string, CardInfo> uuidToInfoTokenCards = new Dictionary<string, CardInfo>();

        private static readonly HashSet<string> filesLoaded = new HashSet<string>();

        /// <summary>
        /// Loads card names and information from a CSV file into the appropriate dictionary.
        /// </summary>
        /// <param name="filePath">The path to the CSV file containing card information.</param>
        /// <param name="isLoadingTokens">Indicates whether the cards being loaded are tokens. Default is false.</param>
        /// <exception cref="Exception">Thrown if the file path does not exist.</exception>
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
            using StreamReader reader = new StreamReader(filePath);
            using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
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

        /// <summary>
        /// Checks if a card with the specified identifier exists in the loaded data.
        /// </summary>
        /// <param name="cardIdentifier">The name or UUID of the card to check.</param>
        /// <param name="isToken">Indicates whether to check in the token cards dictionary. Default is false.</param>
        /// <returns>True if the card exists, otherwise false.</returns>
        /// <exception cref="Exception">Thrown if no card data has been loaded.</exception>
        public static bool DoesCardExist(string cardIdentifier, bool isToken = false)
        {
            if (filesLoaded.Count == 0)
            {
                throw new Exception("Must load cardnames");
            }

            return isToken ? uuidToInfoTokenCards.ContainsKey(cardIdentifier) : nameToInfoStandardCards.ContainsKey(cardIdentifier);
        }

        /// <summary>
        /// Retrieves card information for a specified card identifier from the loaded data.
        /// </summary>
        /// <param name="cardIdentifier">The name or UUID of the card to retrieve information for.</param>
        /// <param name="isToken">Indicates whether to search in the token cards dictionary. Default is false.</param>
        /// <returns>The <see cref="CardInfo"/> for the specified card, or null if not found.</returns>
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