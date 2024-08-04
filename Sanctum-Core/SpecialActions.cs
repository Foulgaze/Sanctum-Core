using System.Runtime.CompilerServices;

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

        public static void CreateTokenCard(CardFactory factory, Player? player, string rawData)
        {
            if (player == null || string.IsNullOrEmpty(rawData))
            {
                return;
            }

            string[] data = rawData.Split('|');
            string tokenName = data[0];
            string? rawCardOriginID = data.Length > 1 ? data[1] : null;

            CreateTokenCard(factory, player, tokenName, rawCardOriginID);
        }

        private static void CreateTokenCard(CardFactory factory, Player player, string tokenName, string? rawCardOriginID)
        {
            if (!TryGetOriginCard(factory, rawCardOriginID, out Card? originCard))
            {
                // Log failure to get origin card
                return;
            }

            Card? tokenCard = factory.CreateCard(tokenName, true);
            if (tokenCard == null)
            {
                // Log failure to create token card
                return;
            }

            InsertTokenCard(player, originCard, tokenCard);
        }

        private static bool TryGetOriginCard(CardFactory factory, string? rawCardOriginID, out Card? originCard)
        {
            originCard = null;

            if (string.IsNullOrEmpty(rawCardOriginID))
            {
                return true;
            }

            if (!int.TryParse(rawCardOriginID, out int cardOriginID))
            {
                // Log invalid cardOriginID format
                return false;
            }

            originCard = factory.GetCard(cardOriginID);
            if (originCard == null)
            {
                // Log that origin card not found
                return false;
            }

            return true;
        }

        private static void InsertTokenCard(Player player, Card? originCard, Card tokenCard)
        {
            CardContainerCollection mainBoard = player.GetCardContainer(CardZone.MainField);

            if (originCard == null)
            {
                mainBoard.InsertCardIntoContainer(null, false, tokenCard, null, true);
                return;
            }

            List<CardContainerCollection> collections = new()
            {
                    mainBoard,
                    player.GetCardContainer(CardZone.LeftField),
                    player.GetCardContainer(CardZone.RightField)
            };

            CardContainerCollection? targetCollection = collections.FirstOrDefault(container => container.ContainsCard(originCard));
            if (targetCollection == null)
            {
                // Log that the origin card's container was not found
                return;
            }

            targetCollection.InsertCardIntoContainerNextToCard(tokenCard, originCard);
        }

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
