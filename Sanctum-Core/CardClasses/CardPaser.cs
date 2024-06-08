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
            List<string> cardNames = new();

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
                    // Log this
                    continue;
                }

                if (!int.TryParse(trimmedLine[..spaceIndex], out int cardCount))
                {
                    // Log this
                    continue;
                }
                string cardName = trimmedLine[(spaceIndex + 1)..];
                cardNames.AddRange(Enumerable.Repeat(cardName, cardCount));
            }
            return cardNames;
        }

    }
}
