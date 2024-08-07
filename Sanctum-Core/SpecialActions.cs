using System.Runtime.CompilerServices;

namespace Sanctum_Core
{
    public enum SpecialAction { Draw, Mill, Exile, CreateToken, CopyCard, PutCardXFrom, RevealTopCards, RevealLibrary, Shuffle}
    public static class SpecialActions
    {
        private static readonly List<CardZone> boardCardZones = new()
        {
                CardZone.MainField,
                CardZone.LeftField,
                CardZone.RightField,
        };
        public static void MoveCards(Playtable table, Player player, string rawCardCount, CardZone sourceZone, CardZone targetZone)
        {
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
            bool loopedAtLeastOnce = false;
            while (cardCount > 0)
            {
                loopedAtLeastOnce = true;
                cardToBeMoved = player.GetCardContainer(sourceZone).GetTopCard();
                if (cardToBeMoved == null)
                {
                    break;
                }
                player.GetCardContainer(targetZone).InsertCardIntoContainer(null, false, cardToBeMoved, null, false);
                --cardCount;
            }
            if(loopedAtLeastOnce)
            {
                table.UpdateCardZone(player, targetZone);
                table.UpdateCardZone(player, sourceZone);
            }
        }

        public static void DrawCards(Playtable table, Player player, string rawCardCount)
        {
            MoveCards(table, player, rawCardCount, CardZone.Library, CardZone.Hand);
        }

        public static void MillCards(Playtable table, Player player, string rawCardCount)
        {
            MoveCards(table, player, rawCardCount, CardZone.Library, CardZone.Graveyard);
        }

        public static void ExileCards(Playtable table, Player player, string rawCardCount)
        {
            MoveCards(table, player, rawCardCount, CardZone.Library, CardZone.Exile);
        }

        public static void CreateTokenCard(CardFactory cardFactory, Player player, string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
            {
                // Log this
                return;
            }

            string[] data = rawData.Split('|');
            string tokenName = data[0];
            string? rawCardOriginID = data.Length > 1 ? data[1] : null;

            CreateTokenCard(cardFactory, player, tokenName, rawCardOriginID);
        }

        private static void CreateTokenCard(CardFactory cardFactory, Player player, string tokenName, string? rawCardOriginID)
        {
            if (!TryGetOriginCard(cardFactory, rawCardOriginID, out Card? originCard))
            {
                // Log failure to get origin card
                return;
            }

            Card? tokenCard = cardFactory.CreateCard(tokenName,true, true);
            if (tokenCard == null)
            {
                // Log failure to create token card
                return;
            }
            InsertTokenCard(player, originCard, tokenCard);
        }

        private static bool TryGetOriginCard(CardFactory cardFactory, string? rawCardOriginID, out Card? originCard)
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

            originCard = cardFactory.GetCard(cardOriginID);
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

            
            if(originCard.CurrentLocation == null)
            {
                // Log this
                return;
            }
            if(!boardCardZones.Contains(originCard.CurrentLocation.Zone))
            {
                // Log this
                return;
            }
            originCard.CurrentLocation.InsertCardIntoContainerNextToCard(tokenCard, originCard);
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

        public static void CreateCopyCard(CardFactory cardFactory, string cardToCopyId)
        {
            if(!int.TryParse(cardToCopyId, out int cardId))
            {
                // Log this
                return;
            }
            Card? cardToCopy = cardFactory.GetCard(cardId);
            if(cardToCopy == null)
            {
                // Log this
                return;
            }
            Card? cardCopy = cardFactory.CreateCard(cardToCopy);
            if (cardToCopy.CurrentLocation == null || cardCopy == null)
            {
                // Log this
                return;
            }
            cardToCopy.CurrentLocation.InsertCardIntoContainerNextToCard(cardCopy, cardToCopy);

        }

        public static bool PutCardXFromTopOrBottom(CardFactory cardFactory,CardContainerCollection library,string[] data)
        {
            if(data.Length != 3)
            {
                // Log this
                return false;
            }
            string startingLocation = data[0];
            string rawCardId = data[1];
            string rawCardDistance = data[2];
            if (!int.TryParse(rawCardId, out int cardId))
            {
                // Log this
                return false;
            }
            if (!int.TryParse(rawCardDistance, out int cardDistance) || cardDistance < 0)
            {
                // Log this
                return false;
            }
            if(startingLocation is not "top" and not "bottom")
            {
                // log this
                return false;
            }
            Card? card = cardFactory.GetCard(cardId);
            if(card == null)
            {
                // Log this
                return false;
            }
            PutCardXFromTopOrBottom(card, cardDistance, startingLocation, library);
            return true;

        }
        private static void PutCardXFromTopOrBottom(Card cardToMove, int cardDistance, string startingLocation, CardContainerCollection zone)
        {
            int zoneCount = zone.GetTotalCardCount();
            if(cardToMove.CurrentLocation != null && cardToMove.CurrentLocation.Zone == CardZone.Library)
            {
                --zoneCount;
            }
            int insertPosition = startingLocation == "top" ? zoneCount - cardDistance : 0 + cardDistance;
            zone.InsertCardIntoContainer(0, false, cardToMove, insertPosition, true);
        }
    }
}
