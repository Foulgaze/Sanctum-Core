//using Sanctum_Core_Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public static class DeckListParser
    {
        /// <summary>
        /// Parses a decklist assuming its in MTGArena format. 
        /// </summary>
        /// <param name="deckList">The raw decklist string</param>
        /// <returns>(List of Successfully parsed names, List of Error Lines)</returns>
        public static List<string> ParseDeckList(string deckList)
        {
            List<string> cardNames = new List<string>();

            foreach (string cardLine in deckList.Split('\n'))
            {
                string trimmedLine = cardLine.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                int spaceIndex = trimmedLine.IndexOf(' ');
                if (spaceIndex == -1)
                {
                    Logger.LogError($"Invalid space index in decklist - {trimmedLine}");
                    // Log this
                    continue;
                }

                if (!int.TryParse(trimmedLine[..spaceIndex], out int cardCount))
                {
                    // Log this
                    Logger.LogError($"Invalid card count in {trimmedLine} - {trimmedLine[..spaceIndex]} is not a valid number");

                    continue;
                }
                int endIndex = trimmedLine.IndexOf("//"); // Only ever enter the firstname of a card i.e. Civilized Scholar // Homicidal Brute should just be returned as Civilized Scholar
                if (endIndex != -1 && (spaceIndex + 1 > endIndex))
                {
                    // Log this
                    Logger.LogError($"Space index + 1 ({spaceIndex + 1}) is before end index {endIndex}");

                    continue;
                }

                string cardName = endIndex == - 1 ? trimmedLine[(spaceIndex + 1)..] : trimmedLine[(spaceIndex + 1)..(endIndex - 1)];
                cardNames.AddRange(Enumerable.Repeat(cardName, cardCount));
            }
            return cardNames;
        }
    }
}
