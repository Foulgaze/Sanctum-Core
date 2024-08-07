using System.Runtime.CompilerServices;

namespace Sanctum_Core
{
    public enum SpecialAction { Draw, Mill, Exile, CreateToken, CopyCard, PutCardXFrom, Shuffle}
    public static class SpecialActions
    {
        private static readonly List<CardZone> boardCardZones = new()
        {
                CardZone.MainField,
                CardZone.LeftField,
                CardZone.RightField,
        };

        /// <summary>
        /// Moves a specified number of cards from one zone to another for a given player on the playtable.
        /// </summary>
        /// <param name="table">The playtable instance.</param>
        /// <param name="player">The player who is moving the cards.</param>
        /// <param name="rawCardCount">The number of cards to move, represented as a string.</param>
        /// <param name="sourceZone">The source card zone from which cards are to be moved.</param>
        /// <param name="targetZone">The target card zone to which cards are to be moved.</param>
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

        /// <summary>
        /// Draws a specified number of cards from the library to the player's hand.
        /// </summary>
        /// <param name="table">The playtable instance.</param>
        /// <param name="player">The player who is drawing the cards.</param>
        /// <param name="rawCardCount">The number of cards to draw, represented as a string.</param>
        public static void DrawCards(Playtable table, Player player, string rawCardCount)
        {
            MoveCards(table, player, rawCardCount, CardZone.Library, CardZone.Hand);
        }
        
        /// <summary>
        /// Mills (sends to the graveyard) a specified number of cards from the library.
        /// </summary>
        /// <param name="table">The playtable instance.</param>
        /// <param name="player">The player who is milling the cards.</param>
        /// <param name="rawCardCount">The number of cards to mill, represented as a string.</param>
        public static void MillCards(Playtable table, Player player, string rawCardCount)
        {
            MoveCards(table, player, rawCardCount, CardZone.Library, CardZone.Graveyard);
        }

        /// <summary>
        /// Exiles a specified number of cards from the library.
        /// </summary>
        /// <param name="table">The playtable instance.</param>
        /// <param name="player">The player who is exiling the cards.</param>
        /// <param name="rawCardCount">The number of cards to exile, represented as a string.</param>
        public static void ExileCards(Playtable table, Player player, string rawCardCount)
        {
            MoveCards(table, player, rawCardCount, CardZone.Library, CardZone.Exile);
        }

        /// <summary>
        /// Creates a token card for a player using the given raw data.
        /// </summary>
        /// <param name="cardFactory">The card factory to create the card.</param>
        /// <param name="player">The player who will receive the token card.</param>
        /// <param name="rawData">The raw data containing token name and optional origin card ID, separated by '|'.</param>
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

        /// <summary>
        /// Shuffles the player's library and updates the playtable.
        /// </summary>
        /// <param name="table">The playtable instance.</param>
        /// <param name="uuid">The UUID of the player who is shuffling.</param>
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

        /// <summary>
        /// Creates a copy of a card using the card ID and places it next to the original card.
        /// </summary>
        /// <param name="cardFactory">The card factory to create the card.</param>
        /// <param name="cardToCopyId">The ID of the card to copy, represented as a string.</param>
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

        /// <summary>
        /// Puts a card a specified number of positions from the top or bottom of the library.
        /// </summary>
        /// <param name="cardFactory">The card factory to get the card.</param>
        /// <param name="library">The card container collection representing the library.</param>
        /// <param name="data">An array of strings containing the starting location ("top" or "bottom"), card ID, and card distance.</param>
        /// <returns>True if the operation is successful, otherwise false.</returns>
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
