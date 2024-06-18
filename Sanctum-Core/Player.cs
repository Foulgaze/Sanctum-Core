namespace Sanctum_Core
{
    public enum CardZone { Library, Graveyard, Exile, CommandZone, MainField, LeftField, RightField, Hand }
    public class Player
    {
        public string Name { get; }
        public string Uuid { get; }
        public NetworkAttribute<string> DeckListRaw { get; }
        public NetworkAttribute<int> Health { get; }
        public NetworkAttribute<bool> ReadiedUp { get; set; }

        private readonly Dictionary<CardZone, CardContainerCollection> zoneToContainer = new();
        public Player(string uuid, string name, int startingHealth)
        {
            this.Uuid = uuid;
            this.Name = name;
            this.Health = NetworkAttributeFactory.AddNetworkAttribute<int>($"{this.Uuid}-health", startingHealth);
            this.DeckListRaw = NetworkAttributeFactory.AddNetworkAttribute<string>($"{this.Uuid}-decklist", "");
            this.ReadiedUp = NetworkAttributeFactory.AddNetworkAttribute($"{this.Uuid}-ready", false);
            this.InitializeBoards();
        }

        void InitializeBoards()
        {
            this.zoneToContainer[CardZone.Library] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null);
            this.zoneToContainer[CardZone.Graveyard] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null);
            this.zoneToContainer[CardZone.Exile] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null);
            this.zoneToContainer[CardZone.CommandZone] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null);
            this.zoneToContainer[CardZone.Hand] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null);
            this.zoneToContainer[CardZone.MainField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3);
            this.zoneToContainer[CardZone.LeftField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3);
            this.zoneToContainer[CardZone.RightField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3);
        }

        /// <summary>
        /// Gets cardzone from player
        /// </summary>
        /// <param name="zone">Zone to get</param>
        /// <returns>Requested zone</returns>
        public CardContainerCollection GetCardContainer(CardZone zone)
        {
            return this.zoneToContainer[zone];
        }
    }
}
