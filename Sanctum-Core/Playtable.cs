//using Sanctum_Core_Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sanctum_Core
{
    public class Playtable
    {
        private readonly List<Player> _players = new List<Player>();
        public readonly int readyUpNeeded;
        public readonly NetworkAttribute<bool> GameStarted;
        public readonly CardFactory cardFactory;
        public readonly NetworkAttributeFactory networkAttributeFactory;
        public readonly NetworkAttribute<(string,string,SpecialAction)> specialAction;
        private readonly bool isSlave;
        /// <summary>
        /// Creates playtable
        /// </summary>
        /// <param name="playerCount">Number of players present in table</param>
        /// <param name="cardsPath">Path to cards.csv</param>
        /// <param name="tokensPath">Path to tokens.csv</param>
        public Playtable(int playerCount, string cardsPath, string tokensPath, bool isSlave = false)
        {
            this.networkAttributeFactory = new NetworkAttributeFactory(isSlave);
            this.cardFactory = new CardFactory(this.networkAttributeFactory);
            this.specialAction = this.networkAttributeFactory.AddNetworkAttribute<(string,string,SpecialAction)>("playtable-specialaction", (string.Empty,string.Empty,0), networkChange: isSlave, setWithoutEqualityCheck: true);
            if(!isSlave)
            {
                this.specialAction.valueChanged += this.HandleSpecialAction;
            }
            this.readyUpNeeded = playerCount;
            this.isSlave = isSlave;
            CardData.LoadCardNames(cardsPath);
            CardData.LoadCardNames(tokensPath, true);
            this.GameStarted = this.networkAttributeFactory.AddNetworkAttribute("main-started", false);
            if(isSlave) // If slave playtable, then ignore checking for readiness, and just listen to main playtable for ready message.
            {
                this.GameStarted.nonNetworkChange += (_) => this.StartGame();
                this.cardFactory.copyCardCreated.nonNetworkChange += this.HandleCardCopy;
                this.cardFactory.tokenCardCreation.nonNetworkChange += this.HandleCardToken;
            }
        }

        
        /// <summary>
        /// Adds a player to the playtable
        /// </summary>
        /// <param name="uuid"> the uuid of the player</param>
        /// <param name="name"> the name of the player</param>
        /// <returns> If the player was succesfully added</returns>
        public bool AddPlayer(string uuid, string name)
        {
            if (this.GameStarted.Value || this._players.Where(player => player.Uuid == uuid).Count() != 0 || this._players.Count >= this.readyUpNeeded)
            {
                return false;
            }
            Player player = new Player(uuid, name, 40, this.networkAttributeFactory, this.cardFactory, this.isSlave);
            if(!this.isSlave)
            {
                player.ReadiedUp.valueChanged += this.CheckForStartGame;
            }
            this._players.Add(player);
            return true;
        }

        /// <summary>
        /// Gets a player in the game
        /// </summary>
        /// <param name="uuid">UUID of desired player</param>
        /// <returns>Player or null depending of if uuid exists</returns>
        public Player? GetPlayer(string uuid)
        {
            return this._players.FirstOrDefault(player => player.Uuid == uuid);
        }

        /// <summary>
        /// Removes a player from the playtable
        /// </summary>
        /// <param name="uuid"> UUID of player to remove</param>
        /// <returns>if player was removed</returns>
        public bool RemovePlayer(string uuid)
        {
            Player? player = this.GetPlayer(uuid);
            return player != null && this._players.Remove(player);
        }

        /// <summary>
        /// Networks the current state of a cardzone
        /// </summary>
        /// <param name="player"> The player whose zone is being updated</param>
        /// <param name="zone"> The zone that should be networked</param>
        public void UpdateCardZone(Player player, CardZone zone)
        {
            CardContainerCollection collection = player.GetCardContainer(zone);
            collection.boardState.SetValue(collection.ToList());
        }

        /// <summary>
        /// Handles special actions triggered by players based on the input received.
        /// </summary>
        /// <param name="rawInput">The raw input string containing the special action type and associated data.</param>
        /// <param name="callerUUID">The unique identifier of the player initiating the action.</param>
        public void HandleSpecialAction(NetworkAttribute attribute)
        {
            (string rawInput, string callerUUID, SpecialAction action) = ((NetworkAttribute<(string,string,SpecialAction)>)attribute).Value;
            Player? callingPlayer = this.GetPlayer(callerUUID);
            if (callingPlayer == null)
            {
                Logger.LogError($"Unable to find player of uuid {callerUUID} (for special action)");
                // Log this
                return;
            }
            switch ((int)action)
            {
                case (int)SpecialAction.Draw:
                    SpecialActions.DrawCards(this, callingPlayer, rawInput);
                    break;
                case (int)SpecialAction.Mill:
                    SpecialActions.MillCards(this, callingPlayer, rawInput);
                    break;
                case (int)SpecialAction.Exile:
                    SpecialActions.ExileCards(this, callingPlayer, rawInput);
                    break;
                case (int)SpecialAction.CreateToken:
                    SpecialActions.CreateTokenCard(this.cardFactory, callingPlayer, rawInput);
                    break;
                case (int)SpecialAction.CopyCard:       
                    SpecialActions.CreateCopyCard(this.cardFactory, rawInput);
                    break;
                case (int)SpecialAction.PutCardXFrom:
                    _ = SpecialActions.PutCardXFromTopOrBottom(this.cardFactory, callingPlayer.GetCardContainer(CardZone.Library), rawInput);
                    break;
                case (int)SpecialAction.Shuffle:
                    SpecialActions.Shuffle(this, callerUUID);
                    break;
                case (int)SpecialAction.Mulligan:
                    SpecialActions.Mulligan(this, callingPlayer);
                    break;
                case (int)SpecialAction.MoveContainerCardsTo:
                    SpecialActions.MoveContainerCards(this, callingPlayer, rawInput);
                    break;
                default:
                    // Log this
                    Logger.LogError($"Special action was called with unknown special action enum - {action} - {rawInput}");
                    break;
            }
        }

        private void HandleCardCopy(NetworkAttribute attribute)
        {
            int cardId = ((NetworkAttribute<int>)attribute).Value;
            Card? card = this.cardFactory.GetCard(cardId);
            if(card == null)
            {
                Logger.LogError($"Unable to find card of id {cardId}");
                return;
            }
            _ = this.cardFactory.CreateCard(card);
        }

        private void HandleCardToken(NetworkAttribute attribute)
        {
            string tokenUUID = ((NetworkAttribute<string>)attribute).Value;
            _ = this.cardFactory.CreateCard(tokenUUID, isTokenCard: true, changeShouldBeNetworked: false);
        }

        private void CheckForStartGame(NetworkAttribute _)
        {
            int readyCount = this._players.Count(player => player.ReadiedUp.Value);
            if (readyCount >= this.readyUpNeeded && !this.GameStarted.Value)
            {
                this.StartGame();
            }
        }

        private void StartGame()
        {
            this.GameStarted.SetValue(true);
            this._players.Sort((x, y) => x.Uuid.CompareTo(y.Uuid));
            this.SetupDecks();
        }

        private void SetupDecks()
        {
            foreach (Player player in this._players)
            {
                List<string> cardNames = DeckListParser.ParseDeckList(player.DeckListRaw.Value);
                cardNames.Sort();
                CardContainerCollection library = player.GetCardContainer(CardZone.Library);
                List<Card> cards = this.cardFactory.LoadCardNames(cardNames);
                if(!this.isSlave)
                {
                    cards.ForEach(card => library.InsertCardIntoContainer(0, true, card, null, false));
                    this.UpdateCardZone(player, CardZone.Library);
                }
            }
        }
    }
}
