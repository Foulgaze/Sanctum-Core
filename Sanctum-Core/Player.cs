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
        public Player(string uuid, string name, int startingHealth, NetworkAttributeFactory networkAttributeFactory, CardFactory cardFactory)
        {
            this.Uuid = uuid;
            this.Name = name;
            this.Health = networkAttributeFactory.AddNetworkAttribute<int>(this.Uuid, startingHealth);
            this.DeckListRaw = networkAttributeFactory.AddNetworkAttribute<string>(this.Uuid, "");
            this.ReadiedUp = networkAttributeFactory.AddNetworkAttribute(this.Uuid, false);
            this.InitializeBoards(networkAttributeFactory, cardFactory);
        }

        void InitializeBoards(NetworkAttributeFactory networkAttributeFactory, CardFactory cardFactory)
        {
            this.zoneToContainer[CardZone.Library] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);
            this.zoneToContainer[CardZone.Graveyard] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);
            this.zoneToContainer[CardZone.Exile] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);
            this.zoneToContainer[CardZone.CommandZone] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);
            this.zoneToContainer[CardZone.Hand] = new CardContainerCollection(CardZone.Library, this.Uuid,1,null, networkAttributeFactory, cardFactory);
            this.zoneToContainer[CardZone.MainField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3, networkAttributeFactory, cardFactory);
            this.zoneToContainer[CardZone.LeftField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3, networkAttributeFactory, cardFactory);
            this.zoneToContainer[CardZone.RightField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3, networkAttributeFactory, cardFactory);
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
