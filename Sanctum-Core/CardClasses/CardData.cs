using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Sanctum_Core
{
    public static class CardData
    {
        private static readonly Dictionary<string, CardInfo> nameToInfoStandardCards = new Dictionary<string, CardInfo>();
        private static readonly Dictionary<string, CardInfo> uuidToInfoTokenCards = new Dictionary<string, CardInfo>();
        private static readonly Dictionary<string, string> uuidToInfoCardName = new Dictionary<string, string>();

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
                    uuidToInfoTokenCards[currentCardInfo.uuid] = currentCardInfo;
                    continue;
                }
                uuidToInfoCardName[currentCardInfo.uuid] = currentCardInfo.name;
                nameToInfoStandardCards[currentCardInfo.name] = currentCardInfo;
            }
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

        /// <summary>
        /// Gets the backside of the card based on the uuid
        /// </summary>
        /// <param name="frontsideUUID"> The uuid of the front side</param>
        /// <returns> Cardinfo of the backside </returns>
        /// <exception cref="Exception"> Breaks if not present </exception>
        public static CardInfo GetBackSide(string frontsideUUID)
        {

            if(!uuidToInfoCardName.ContainsKey(frontsideUUID))
            {
                throw new Exception($"Unable to find of uuid {frontsideUUID}");
            }
            string cardName = uuidToInfoCardName[frontsideUUID];
            if (!nameToInfoStandardCards.ContainsKey(cardName))
            {
                throw new Exception($"Unable to find of card name {cardName}");
            }
            return nameToInfoStandardCards[cardName];
        }

        private static string GetTokenName(string uuid)
        {
            CardInfo info = uuidToInfoTokenCards[uuid];
            return info.name;
        }

        public static List<(string, string)> GetTokenUUINamePairs()
        {
            List<(string, string)> pairs = new List<(string,string)>();
            foreach (string uuid in uuidToInfoTokenCards.Keys)
            {
                pairs.Add((uuid, GetTokenName(uuid)));
            }
            return pairs.GroupBy(pair => pair.Item2)   // Group by the name (Item2)
            .Select(group => group.First()) // Select the first pair in each group
            .OrderBy(kvp => kvp.Item2).ToList();
        }
    }
}