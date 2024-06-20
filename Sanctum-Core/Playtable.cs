using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;

namespace Sanctum_Core
{
    public class Playtable
    {
        private readonly List<Player> _players = new();

        /*private readonly NetworkManager _networkManager = null;*/


        public readonly int readyUpNeeded;

        public readonly NetworkAttribute<bool> GameStarted;
        private readonly CardFactory cardFactory;
        public readonly NetworkAttributeFactory networkAttributeFactory;
        public Playtable(int playerCount)
        {
            this.networkAttributeFactory = new NetworkAttributeFactory();
            this.cardFactory = new CardFactory(this.networkAttributeFactory);
            this.readyUpNeeded = playerCount;
            this.GameStarted = this.networkAttributeFactory.AddNetworkAttribute("main-started", false);
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

        void CheckForStartGame(object obj, PropertyChangedEventArgs args)
        {
            int readyCount = this._players.Count(player => player.ReadiedUp.Value);
            if (readyCount >= this.readyUpNeeded)
            {
                this.StartGame();
            }
        }

        void StartGame()
        {
            this.GameStarted.SetValue(true);
            this.SetupDecks();
        }

        void SetupDecks()
        {
            foreach (Player player in this._players)
            {
                List<string> cardNames = DeckListParser.ParseDeckList(player.DeckListRaw.Value);
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
