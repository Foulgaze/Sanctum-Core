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

        public Playtable(int playerCount)
        {
            this.networkAttributeFactory = new NetworkAttributeFactory();
            this.cardFactory = new CardFactory(this.networkAttributeFactory);
            this.readyUpNeeded = playerCount;
            this.GameStarted = this.networkAttributeFactory.AddNetworkAttribute("main-started", false);
        }

        private void BoardChanged(object sender, PropertyChangedEventArgs e)
        {
            boardChanged(sender, e);
        }

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

        private void CheckForStartGame(object obj, PropertyChangedEventArgs args)
        {
            int readyCount = this._players.Count(player => player.ReadiedUp.Value);
            if (readyCount >= this.readyUpNeeded)
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


        public bool RemovePlayer(string uuid)
        {
            Player? player = this.GetPlayer(uuid);
            return player != null && this._players.Remove(player);
        }
    }
}
