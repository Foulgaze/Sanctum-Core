////using Sanctum_Core_Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sanctum_Core
{
    public enum SpecialAction { Draw, Mill, Exile, CreateToken, CopyCard, PutCardXFrom, Shuffle, Mulligan, MoveContainerCardsTo}
    public static class SpecialActions
    {
        private static readonly List<CardZone> boardCardZones = new List<CardZone>
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
                Logger.LogError($"Could not parse moving cards value - {rawCardCount}");
                return;
            }
            if (cardCount < 0)
            {
                // Log this
                Logger.LogError($"Attempted to move 0 cards - {rawCardCount} ");
                return;
            }
            Card? cardToBeMoved;
            bool loopedAtLeastOnce = false;
            CardContainerCollection sourceCollection = player.GetCardContainer(sourceZone);
            List<List<int>> originalCardList = sourceCollection.ToList();
            while (cardCount > 0)
            {
                loopedAtLeastOnce = true;
                cardToBeMoved = sourceCollection.GetTopCard();
                if (cardToBeMoved == null)
                {
                    break;
                }
                player.GetCardContainer(targetZone).InsertCardIntoContainer(null, false, cardToBeMoved, null, false);
                --cardCount;
            }
            if(loopedAtLeastOnce)
            {
                List<int> removedCardIds = UpdateRemovedCards(sourceCollection, originalCardList);
                sourceCollection.removeCardIds.SetValue(removedCardIds);
                table.UpdateCardZone(player, targetZone);
            }
        }

        private static List<int> UpdateRemovedCards(CardContainerCollection sourceCollection,List<List<int>> originalCardList)
        {
            List<List<int>> newCardList = sourceCollection.ToList();
            List<int> newCardListFlat = newCardList.SelectMany(x => x).ToList();
            List<int> originalCardListFlat = originalCardList.SelectMany(x => x).ToList();

            List<int> removedCards = originalCardListFlat.Except(newCardListFlat).ToList();
            return removedCards;
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
                Logger.LogError($"Data string is null or empty for creating token card - {rawData} ");
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
                Logger.LogError($"Failed to get origin card of id {rawCardOriginID} ");

                return;
            }

            Card? tokenCard = cardFactory.CreateCard(tokenName,true, true);
            if (tokenCard == null)
            {
                // Log failure to create token card
                Logger.LogError($"Failed to create token card of name - {tokenName} ");

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
                Logger.LogError($"Unable to get origin card id - {rawCardOriginID} ");

                return false;
            }

            originCard = cardFactory.GetCard(cardOriginID);
            if (originCard == null)
            {
                // Log that origin card not found
                Logger.LogError($"Origin card could not be found - {cardOriginID}");

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
                Logger.LogError($"Card of id {originCard.Id} currently has no currently location");

                return;
            }
            if(!boardCardZones.Contains(originCard.CurrentLocation.Zone))
            {
                // Log this
                Logger.LogError($"Trying to insert a token somewhere into a non board - {originCard.CurrentLocation.Zone} ");
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
                Logger.LogError($"Could not find player for shuffling with uuid - {uuid}");

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
                Logger.LogError($"Could not parse copy card id {cardToCopyId}");
                return;
            }
            Card? cardToCopy = cardFactory.GetCard(cardId);
            if(cardToCopy == null)
            {
                // Log this
                Logger.LogError($"Could not copy card id {cardToCopyId}");
                return;
            }
            Card? cardCopy = cardFactory.CreateCard(cardToCopy);
            if (cardToCopy.CurrentLocation == null || cardCopy == null)
            {
                // Log this
                Logger.LogError($"CopyToCopy has no current location or the copy is null - {cardToCopy.CurrentLocation} {cardCopy}");
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
                Logger.LogError($"Data length is not three - {data}");
                return false;
            }
            string startingLocation = data[0];
            string rawCardId = data[1];
            string rawCardDistance = data[2];
            if (!int.TryParse(rawCardId, out int cardId))
            {
                // Log this
                Logger.LogError($"Could not parse card id from - {rawCardId}");
                return false;
            }
            if (!int.TryParse(rawCardDistance, out int cardDistance) || cardDistance < 0)
            {
                // Log this
                Logger.LogError($"Could not parse card distance from {rawCardDistance}");
                return false;
            }
            if(startingLocation != "top" && startingLocation != "bottom")
            {
                // log this
                Logger.LogError($"Starting location is not top or bottom - {startingLocation}");
                return false;
            }
            Card? card = cardFactory.GetCard(cardId);
            if(card == null)
            {
                // Log this
                Logger.LogError($"Could not get card of id - {cardId} for moving");
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

        public static void Mulligan(Playtable table, Player clientPlayer)
        {
            MoveAllCardsFromSourceToDestinationZone(CardZone.Hand, CardZone.Library, clientPlayer,null, table);
            Shuffle(table,clientPlayer.Uuid);
            DrawCards(table, clientPlayer, "7");
        }

        private static void MoveAllCardsFromSourceToDestinationZone(CardZone sourceZone, CardZone destinationZone, Player clientPlayer, int? insertPosition, Playtable table)
        {
            CardContainerCollection destinationCollection = clientPlayer.GetCardContainer(destinationZone);
            CardContainerCollection sourceCollection = clientPlayer.GetCardContainer(sourceZone);
            List<Card> allCards = sourceCollection.ToCardList().SelectMany(sublist => sublist).ToList();
            foreach(Card card in allCards)
            {
                destinationCollection.InsertCardIntoContainer(insertPosition : null, createNewContainer : false, cardToInsert:card, cardContainerPosition: insertPosition, changeShouldBeNetworked: false);
            }
            if(allCards.Count != 0)
            {
                sourceCollection.removeCardIds.SetValue(allCards.Select(card => card.Id).ToList());
                table.UpdateCardZone(clientPlayer, destinationZone);
            }
            
        }
    }
}
