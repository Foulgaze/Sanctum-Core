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
        public event PropertyChangedEventHandler boardChanged = delegate { };
        public event PropertyChangedEventHandler cardCreated = delegate { };

        /// <summary>
        /// Creates playtable
        /// </summary>
        /// <param name="playerCount">Number of players present in table</param>
        /// <param name="cardsPath">Path to cards.csv</param>
        /// <param name="tokensPath">Path to tokens.csv</param>
        public Playtable(int playerCount, string cardsPath, string tokensPath)
        {
            this.networkAttributeFactory = new NetworkAttributeFactory();
            this.cardFactory = new CardFactory(this.networkAttributeFactory);
            this.cardFactory.CardCreated += this.CardCreated;
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
            if (this.GameStarted.Value || this._players.Where(player => player.Uuid == uuid).Count() != 0)
            {
                return false;
            }
            Player player = new(uuid, name, 40, this.networkAttributeFactory, this.cardFactory);
            player.ReadiedUp.valueChange += this.CheckForStartGame;
            this._players.Add(player);
            player.boardChanged += this.BoardChanged;
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
        /// Networks the current state of a cardzone
        /// </summary>
        /// <param name="player"> The player whose zone is being updated</param>
        /// <param name="zone"> The zone that should be networked</param>
        public void UpdateCardZone(Player player, CardZone zone)
        {
            boardChanged(player.GetCardContainer(zone), new PropertyChangedEventArgs(string.Empty));
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

        public void HandleSpecialAction(string rawInput, string uuid)
        {
            string[] data = rawInput.Split('|');
            if(data.Length < 2)
            {
                // Log this
                return;
            }
            switch (data[0])
            {
                case "mill":
                    SpecialActions.MillCards(this, uuid, data[1]);
                    break;
                case "draw":
                    SpecialActions.DrawCards(this, uuid, data[1]);
                    break;
                case "exile":
                    SpecialActions.ExileCards(this, uuid, data[1]);
                    break;
                case "createtoken":
                    SpecialActions.CreateTokenCard(this.cardFactory, this.GetPlayer(uuid), string.Join("|", data[1..]));
                    break;
                default:
                    // Log this
                    break;
            }
        }

        private void BoardChanged(object? sender, PropertyChangedEventArgs e)
        {
            boardChanged(sender, e);
        }

        private void CardCreated(object? sender, PropertyChangedEventArgs e)
        {
            cardCreated(sender, e);
        }

        private void CheckForStartGame(object? obj, PropertyChangedEventArgs? args)
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
                boardChanged(library, new PropertyChangedEventArgs(string.Empty));
            }
        }
    }
}
