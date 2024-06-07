namespace Sanctum_Core
{
    using Sanctum_Core.CardClasses;
    using Sanctum_Core.Networking;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public enum CardZone { Library, Graveyard, Exile, CommandZone, MainField, LeftField, RightField, Hand }
    public class Player
    {
        public string Name { get; }
        public string Uuid { get; }
        public NetworkAttribute<string> DeckListRaw { get; }
        public NetworkAttribute<int> Health { get; }
        public NetworkAttribute<bool> ReadiedUp { get; set; }

        public bool GameStarted { get; } = false;

        private Dictionary<CardZone, CardContainerCollection> zoneToContainer = new Dictionary<CardZone, CardContainerCollection>();
        public Player(string uuid, string name, int startingHealth, NetworkAttributeFactory networkAttributeFactory, CardFactory cardFactory)
        {
            this.Uuid = uuid;
            this.Name = name;
            Health = networkAttributeFactory.AddNetworkAttribute<int>(Uuid, startingHealth);
            DeckListRaw = networkAttributeFactory.AddNetworkAttribute<string>(Uuid, "");
            ReadiedUp = networkAttributeFactory.AddNetworkAttribute(Uuid, false);
            InitializeBoards(networkAttributeFactory, cardFactory);
        }

        void InitializeBoards(NetworkAttributeFactory networkAttributeFactory, CardFactory cardFactory)
        {
            zoneToContainer[CardZone.Library] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);
            zoneToContainer[CardZone.Graveyard] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);
            zoneToContainer[CardZone.Exile] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);
            zoneToContainer[CardZone.CommandZone] = new CardContainerCollection(CardZone.Library, this.Uuid, 1, null, networkAttributeFactory, cardFactory);

            zoneToContainer[CardZone.Hand] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 1, networkAttributeFactory, cardFactory);

            zoneToContainer[CardZone.MainField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3, networkAttributeFactory, cardFactory);
            zoneToContainer[CardZone.LeftField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3, networkAttributeFactory, cardFactory);
            zoneToContainer[CardZone.RightField] = new CardContainerCollection(CardZone.Library, this.Uuid, null, 3, networkAttributeFactory, cardFactory);

        }

        /// <summary>
        /// Gets cardzone from player
        /// </summary>
        /// <param name="zone">Zone to get</param>
        /// <returns>Requested zone</returns>
        public CardContainerCollection GetCardContainer(CardZone zone)
        {
            return zoneToContainer[zone];
        }

        /// <summary>
        /// Determines if the current user decklist is valid
        /// </summary>
        /// <returns>A list of the lines/names that were not able to be parsed</returns>
        public List<string> ValidateDeckList(Func<string, bool> validateCardName)
        {
            (List<string> cardNames, List<string> errorLines) = CardParser.ParseDeckList(DeckListRaw.Value);
            if (errorLines.Count > 0)
            {
                return errorLines;
            }
            cardNames = cardNames.Where(x => !validateCardName(x)).ToList();
            return cardNames;
        }
    }
}
