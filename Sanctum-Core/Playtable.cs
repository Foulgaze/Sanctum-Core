using System.ComponentModel;

namespace Sanctum_Core
{
    public class Playtable
    {
        private readonly List<Player> _players = new();

        private readonly NetworkManager _networkManager;

        public NetworkAttribute<int> readyUpNeeded;

        public bool GameStarted { get; set; } = false;

        public event PropertyChangedEventHandler gameStarted = delegate { };

        private readonly CardFactory cardFactory;
        private readonly NetworkAttributeFactory networkAttributeFactory;

        public Playtable(bool mock = false)
        {
            this.readyUpNeeded = new NetworkAttribute<int>("0", 4);
            this.cardFactory = new CardFactory();
            this.networkAttributeFactory = new NetworkAttributeFactory();
            this.networkAttributeFactory.attributeValueChanged += this._networkManager.NetworkAttributeChanged;
            this._networkManager = new NetworkManager(this.networkAttributeFactory, mock);

        }

        /// <summary>
        /// Adds a player to the table
        /// </summary>
        /// <param name="name"> Name of the player</param>
        /// <param name="uuid"> UUID of player</param>
        public void AddPlayer(string name, string uuid)
        {
            if (this.GameStarted)
            {
                return;
            }
            Player player = new(uuid, name, 40, this.networkAttributeFactory, this.cardFactory);
            player.ReadiedUp.valueChange += this.CheckForStartGame;
            this._players.Add(player);
        }

        /// <summary>
        /// Gets a player in the game
        /// </summary>
        /// <param name="uuid">UUID of desired player</param>
        /// <returns>Player or null depending of if uuid exists</returns>
        public Player? GetPlayer(string uuid)
        {
            return this._players.FirstOrDefault(player => player.Uuid == uuid); ;
        }

        void CheckForStartGame(object obj, PropertyChangedEventArgs args)
        {
            int readyCount = this._players.Count(player => player.ReadiedUp.Value);
            if (readyCount >= this.readyUpNeeded.Value)
            {
                this.StartGame();
            }
        }

        void StartGame()
        {
            this.GameStarted = true;
            this.SetupDecks();
            gameStarted(this, new PropertyChangedEventArgs("Started"));
        }

        void SetupDecks()
        {
            foreach (Player player in this._players)
            {
                (List<string> cardNames, _) = CardParser.ParseDeckList(player.DeckListRaw.Value);
                CardContainerCollection library = player.GetCardContainer(CardZone.Library);
                List<Card> cards = this.cardFactory.LoadCardNames(cardNames, this.networkAttributeFactory);
                cards.ForEach(card => library.InsertCardIntoContainer(0, true, card, null, false));
            }
        }


        public bool RemovePlayer(string uuid)
        {
            Player player = this.GetPlayer(uuid);
            return player != null && this._players.Remove(player);
        }
    }
}
