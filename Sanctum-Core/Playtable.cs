using Sanctum_Core_Logger;
using System.ComponentModel;

namespace Sanctum_Core
{
    public class Playtable
    {
        private readonly List<Player> _players = new();
        public readonly int readyUpNeeded;
        public readonly NetworkAttribute<bool> GameStarted;
        private readonly CardFactory cardFactory;
        public readonly NetworkAttributeFactory networkAttributeFactory;
        public event Action<Card> cardCreated = delegate { };

        /// <summary>
        /// Creates playtable
        /// </summary>
        /// <param name="playerCount">Number of players present in table</param>
        /// <param name="cardsPath">Path to cards.csv</param>
        /// <param name="tokensPath">Path to tokens.csv</param>
        public Playtable(int playerCount, string cardsPath, string tokensPath, bool isSlavePlaytable = false)
        {
            this.networkAttributeFactory = new NetworkAttributeFactory(isSlavePlaytable);
            this.cardFactory = new CardFactory(this.networkAttributeFactory);
            this.cardFactory.cardCreated += this.CardCreated;
            this.readyUpNeeded = playerCount;
            CardData.LoadCardNames(cardsPath);
            CardData.LoadCardNames(tokensPath, true);
            this.GameStarted = this.networkAttributeFactory.AddNetworkAttribute("main-started", false);
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
            Player player = new(uuid, name, 40, this.networkAttributeFactory, this.cardFactory);
            player.ReadiedUp.valueChanged += this.CheckForStartGame;
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
        public void HandleSpecialAction(string rawInput, string callerUUID)
        {
            string[] data = rawInput.Split('|');
            if(data.Length < 2)
            {
                // Log this
                Logger.LogError($"Unable to handle special action, cannot split data properly - {rawInput}");
                return;
            }
            if (!int.TryParse(data[0], out int specialAction))
            {
                Logger.LogError($"Unable to parse specialinput enum {data[0]} - {rawInput}");
                // Log this
                return;
            }
            Player? callingPlayer = this.GetPlayer(callerUUID);
            if (callingPlayer == null)
            {
                Logger.LogError($"Unable to find player of uuid {callerUUID} (for special action)");
                // Log this
                return;
            }
            switch (specialAction)
            {
                case (int)SpecialAction.Draw:
                    SpecialActions.DrawCards(this, callingPlayer, data[1]);
                    break;
                case (int)SpecialAction.Mill:
                    SpecialActions.MillCards(this, callingPlayer, data[1]);
                    break;
                case (int)SpecialAction.Exile:
                    SpecialActions.ExileCards(this, callingPlayer, data[1]);
                    break;
                case (int)SpecialAction.CreateToken:
                    SpecialActions.CreateTokenCard(this.cardFactory, callingPlayer, string.Join("|", data[1..]));
                    break;
                case (int)SpecialAction.CopyCard:
                    SpecialActions.CreateCopyCard(this.cardFactory, data[1]);
                    break;
                case (int)SpecialAction.PutCardXFrom:
                    _ = SpecialActions.PutCardXFromTopOrBottom(this.cardFactory, callingPlayer.GetCardContainer(CardZone.Library), data[1..]);
                    break;
                case (int)SpecialAction.Shuffle:
                    SpecialActions.Shuffle(this, callerUUID);
                    break;
                default:
                    // Log this
                    Logger.LogError($"Special action was called with unknown special action enum - {specialAction} - {rawInput}");
                    break;
            }
        }

        private void CardCreated(Card card)
        {
            cardCreated(card);
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
                cards.ForEach(card => library.InsertCardIntoContainer(0, true, card, null, false));
            }
        }
    }
}
