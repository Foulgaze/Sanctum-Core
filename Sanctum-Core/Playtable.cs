using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;

namespace Sanctum_Core
{
    public class Playtable
    {
        private readonly List<Player> _players = new();

        /*private readonly NetworkManager _networkManager = null;*/


        public readonly NetworkAttribute<int> readyUpNeeded;

        public bool GameStarted { get; set; } = false;

        public event PropertyChangedEventHandler gameStarted = delegate { };

        public Playtable(int playerCount = 4)
        {
            this.readyUpNeeded = new NetworkAttribute<int>("0", playerCount);
            /*this._networkManager = new NetworkManager(this.networkAttributeFactory, mock);*/
            /*this._networkManager.NetworkCommandHandler.networkInstructionEvents[NetworkInstruction.NetworkAttribute] += this.networkAttributeFactory.HandleNetworkedAttribute;*/
        }

        public void AddPlayer(string uuid, string name)
        {
            if (this.GameStarted || this._players.Where(player => player.Uuid == uuid).Count() != 0)
            {
                return;
            }
            Player player = new(uuid, name, 40);
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
            return this._players.FirstOrDefault(player => player.Uuid == uuid);
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
                List<string> cardNames = DeckListParser.ParseDeckList(player.DeckListRaw.Value);
                CardContainerCollection library = player.GetCardContainer(CardZone.Library);
                List<Card> cards = CardFactory.LoadCardNames(cardNames);
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
