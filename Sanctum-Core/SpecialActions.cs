using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public static class SpecialActions
    {

        public static void MoveCards(Playtable table, string uuid, string rawCardCount, CardZone sourceZone, CardZone targetZone)
        {
            Player? player = table.GetPlayer(uuid);
            if (player is null)
            {
                // Log this
                return;
            }
            if (!int.TryParse(rawCardCount, out int cardCount))
            {
                // Log this
                return;
            }
            if (cardCount < 0)
            {
                // Log this
                return;
            }
            Card? cardToBeMoved;
            while (cardCount > 0)
            {
                cardToBeMoved = player.GetCardContainer(sourceZone).GetTopCard();
                if (cardToBeMoved == null)
                {
                    break;
                }
                player.GetCardContainer(targetZone).InsertCardIntoContainer(null, false, cardToBeMoved, null, false);
                --cardCount;
            }
            table.UpdateCardZone(player, targetZone);
            table.UpdateCardZone(player, sourceZone);
        }

        public static void DrawCards(Playtable table, string uuid, string rawCardCount)
        {
            MoveCards(table, uuid, rawCardCount, CardZone.Library, CardZone.Hand);
        }

        public static void MillCards(Playtable table, string uuid, string rawCardCount)
        {
            MoveCards(table, uuid, rawCardCount, CardZone.Library, CardZone.Graveyard);
        }

        public static void ExileCards(Playtable table, string uuid, string rawCardCount)
        {
            MoveCards(table, uuid, rawCardCount, CardZone.Library, CardZone.Exile);
        }

        /*public static void CreateRelatedCard(Playtable table, string newCardUUID, string originCardId)
        {
        }*/
        public static void Shuffle(Playtable table, string uuid)
        {
            Player? playerWhoIsShuffling = table.GetPlayer(uuid);
            if (playerWhoIsShuffling is null)
            {
                // Log this
                return;
            }
            CardContainerCollection library = playerWhoIsShuffling.GetCardContainer(CardZone.Library);
            library.Shuffle();
            table.UpdateCardZone(playerWhoIsShuffling, CardZone.Library);
        }
    }
}
