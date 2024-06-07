using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core.CardClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CardParser
    {
        /// <summary>
        /// Parses a decklist assuming its in MTGArena format. 
        /// </summary>
        /// <param name="deckList">The raw decklist string</param>
        /// <returns>(List of Successfully parsed names, List of Error Lines)</returns>
        public static (List<string>, List<string>) ParseDeckList(string deckList)
        {
            List<string> cardNames = new List<string>();
            List<string> problematicLines = new List<string>();

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
                    problematicLines.Add(trimmedLine);
                    continue;
                }

                int cardCount;
                if (!Int32.TryParse(trimmedLine.Substring(0, spaceIndex), out cardCount))
                {
                    problematicLines.Add(cardLine);
                    continue;
                }
                string cardName = trimmedLine.Substring(spaceIndex + 1);
                cardNames.AddRange(Enumerable.Repeat(cardName, cardCount));
            }
            return (cardNames, problematicLines);
        }

    }
}
