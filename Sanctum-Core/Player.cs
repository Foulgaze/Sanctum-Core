﻿namespace Sanctum_Core
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
        private readonly NetworkAttributeFactory networkAttributeFactory;
        private readonly CardFactory cardFactory; 
        public Player(string uuid, string name, int startingHealth, NetworkAttributeFactory networkAttributeFactory, CardFactory cardFactory)
        {
            this.Uuid = uuid;
            this.Name = name;
            this.networkAttributeFactory = networkAttributeFactory;
            this.cardFactory = cardFactory;
            this.Health = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Uuid}-health", startingHealth);
            this.DeckListRaw = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Uuid}-decklist", "");
            this.ReadiedUp = this.networkAttributeFactory.AddNetworkAttribute($"{this.Uuid}-ready", false);
            this.InitializeBoards();
        }

        void InitializeBoards()
        {
            this.zoneToContainer[CardZone.Library] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, false,this.networkAttributeFactory, this.cardFactory);
            this.zoneToContainer[CardZone.Graveyard] = new CardContainerCollection(CardZone.Graveyard, this.Uuid, 1, null,true, this.networkAttributeFactory, this.cardFactory);
            this.zoneToContainer[CardZone.Exile] = new CardContainerCollection(CardZone.Exile, this.Uuid, 1, null,true,this.networkAttributeFactory, this.cardFactory);
            this.zoneToContainer[CardZone.CommandZone] = new CardContainerCollection(CardZone.CommandZone, this.Uuid, 1, null,true, this.networkAttributeFactory, this.cardFactory);
            this.zoneToContainer[CardZone.Hand] = new CardContainerCollection(CardZone.Hand, this.Uuid, 1, null,true, this.networkAttributeFactory, this.cardFactory);
            this.zoneToContainer[CardZone.MainField] = new CardContainerCollection(CardZone.MainField, this.Uuid, null, 3, true,this.networkAttributeFactory, this.cardFactory);
            this.zoneToContainer[CardZone.LeftField] = new CardContainerCollection(CardZone.LeftField, this.Uuid, null, 3, true,this.networkAttributeFactory, this.cardFactory);
            this.zoneToContainer[CardZone.RightField] = new CardContainerCollection(CardZone.RightField, this.Uuid, null, 3,true, this.networkAttributeFactory, this.cardFactory);
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
