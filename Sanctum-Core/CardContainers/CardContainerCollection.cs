using Newtonsoft.Json;
//using Sanctum_Core_Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    /// <summary>
    /// Class that is a wrapper for data about inserting a card. Null insertposition means insert at end. 
    /// </summary>
    public class InsertCardData
    {
        public int? insertPosition;
        public int cardId;
        public int? containerInsertPosition;
        public bool createNewContainer;
        public InsertCardData(int? insertPosition, int cardID, int? containerInsertPosition, bool createNewContainer)
        {
            this.insertPosition = insertPosition;
            this.cardId = cardID;
            this.containerInsertPosition = containerInsertPosition;
            this.createNewContainer = createNewContainer;
        }
    }
    public class CardContainerCollection
    {

        public CardZone Zone { get; set; }
        public string Owner { get; }
        private readonly List<CardContainer> Containers = new List<CardContainer>();
        private readonly int? maxContainerCount;
        private readonly int? maxCardCountPerContainer;
        public  NetworkAttribute<InsertCardData> insertCardData;
        public  NetworkAttribute<List<int>> removeCardIds;
        public  NetworkAttribute<List<List<int>>> boardState;
        public  NetworkAttribute<bool> revealTopCard;
        private readonly CardFactory CardFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardContainerCollection"/> class with the specified parameters.
        /// </summary>
        /// <param name="zone">The card zone associated with this container collection.</param>
        /// <param name="owner">The owner of the card container collection.</param>
        /// <param name="maxContainerCount">The maximum number of containers allowed (nullable).</param>
        /// <param name="maxContainerCardCount">The maximum number of cards allowed per container (nullable).</param>
        /// <param name="revealTopCard">Indicates whether the top card of each container should be revealed.</param>
        /// <param name="networkAttributeFactory">The factory for managing network attributes.</param>
        /// <param name="cardFactory">The factory for creating cards.</param>
        public CardContainerCollection(CardZone zone, string owner, int? maxContainerCount, int? maxContainerCardCount, bool revealTopCard, NetworkAttributeFactory networkAttributeFactory, CardFactory cardFactory, bool isSlave = false)
        {
            this.maxCardCountPerContainer = maxContainerCardCount;
            this.maxContainerCount = maxContainerCount;
            this.Zone = zone;
            this.Owner = owner;
            this.CardFactory = cardFactory;
            this.InitializeAttributes(networkAttributeFactory, revealTopCard, isSlave);
            if(!isSlave)
            {
                this.insertCardData.valueChanged += this.NetworkedCardInsert;
            }
        }

        private void InitializeAttributes(NetworkAttributeFactory networkAttributeFactory, bool revealTopCard, bool isSlave)
        {
            this.insertCardData = networkAttributeFactory.AddNetworkAttribute($"{this.Owner}-{(int)this.Zone}-insert", new InsertCardData(null, 0, null, false), true, networkChange: isSlave);
            this.revealTopCard = networkAttributeFactory.AddNetworkAttribute($"{this.Owner}-{(int)this.Zone}-reveal", revealTopCard);
            this.removeCardIds = networkAttributeFactory.AddNetworkAttribute($"{this.Owner}-{(int)this.Zone}-removecards", new List<int>());
            this.boardState = networkAttributeFactory.AddNetworkAttribute($"{this.Owner}-{(int)this.Zone}-boardstate", new List<List<int>>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insertPosition"></param>
        /// <param name="createNewContainer"></param>
        /// <param name="cardToInsert"></param>
        /// <param name="cardContainerPosition"></param>
        /// <param name="changeShouldBeNetworked"></param>
        public void InsertCardIntoContainer(int? insertPosition, bool createNewContainer, Card cardToInsert, int? cardContainerPosition, bool changeShouldBeNetworked)
        {

            if (changeShouldBeNetworked)
            {
                InsertCardData newCardData = new InsertCardData(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer);
                this.insertCardData.SetValue(newCardData);
                return;
            }
            _ = this.ProcessCardInsertion(new InsertCardData(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer), false);
        }
        /// <summary>
        /// Removes a card from the first card container that matches ID
        /// </summary>
        /// <param name="cardId">The id of the card to remove</param>
        /// <returns>if removal was successfull</returns>
        public bool RemoveCardFromContainer(int cardId, bool networkChange = true)
        {
            foreach (CardContainer container in this.Containers)
            {
                Card? cardToRemove = container.Cards.FirstOrDefault(card => card.Id == cardId);
                if (cardToRemove != null)
                {
                    if (!container.Cards.Remove(cardToRemove))
                    {
                        return false;
                    }
                    if (container.Cards.Count == 0)
                    {
                        _ = this.Containers.Remove(container);
                    }
                    if(networkChange)
                    {
                        this.removeCardIds.SetValue(new List<int>{ cardId });
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the name of the card zone.
        /// </summary>
        /// <returns>The name of the card zone.</returns>
        public string GetName()
        {
            string? name = Enum.GetName(typeof(CardZone), this.Zone) ?? "Unable To Find";
            return name;
        }

        /// <summary>
        /// Aggregates all the card ids in the container in list of list form
        /// </summary>
        /// <returns> list of list of all card ids</returns>
        public List<List<int>> ToList()
        {
            return this.Containers.Select(container => container.GetCardIDs()).ToList();
        }

        /// <summary>
        /// Aggregates all the cards in the container in list of list form 
        /// </summary>
        /// <returns>A list of list containing cards</returns>
        public List<List<Card>> ToCardList()
        {
            return this.Containers.Select(container => container.GetCards()).ToList();
        }

        /// <summary>
        /// Gets the top card
        /// </summary>
        /// <returns> Returns the top card of the last collection or null if empty</returns>
        public Card? GetTopCard()
        {
            if(this.Containers.Count == 0)
            {
                return null;
            }
            return this.Containers.Last().Cards.Last();
        }

        /// <summary>
        /// Shuffles the deck
        /// </summary>
        public void Shuffle()
        {
            this.Containers.ForEach(container => container.Shuffle());
        }

        /// <summary>
        /// The total number of cards in containers
        /// </summary>
        /// <returns>The total number of cards in containers</returns>
        public int GetTotalCardCount()
        {
            return this.Containers.Aggregate(0, (acc, container) => acc + container.Cards.Count());
        }

        /// <summary>
        /// Checks if a specified card is contained within any of the containers.
        /// </summary>
        /// <param name="card">The card to check for.</param>
        /// <returns>True if the card is found in any container, otherwise false.</returns>
        public bool ContainsCard(Card card)
        {
            return this.Containers.Any(container => container.Cards.Contains(card));
        }

        /// <summary>
        /// Inserts a specified card into the container next to a given card.
        /// </summary>
        /// <param name="insertCard">The card to insert.</param>
        /// <param name="cardToFind">The card to find and insert next to.</param>
        public void InsertCardIntoContainerNextToCard(Card insertCard, Card cardToFind)
        {
            int? containerIndex = null;
            for(int i = 0; i <  this.Containers.Count; ++i)
            {
                if (this.Containers[i].Cards.Contains(cardToFind))
                {
                    containerIndex = i;
                    break;
                }
            }
            if(containerIndex == null)
            {
                return;
            }
            this.InsertCardIntoContainer(containerIndex, false, insertCard, this.Containers[(int)containerIndex].Cards.IndexOf(cardToFind) + 1, true);

        }


        private void NetworkedCardInsert(NetworkAttribute cardInsertion)
        {
            InsertCardData? result;
            try
            {
                result = JsonConvert.DeserializeObject<InsertCardData>(cardInsertion.SerializedValue);
            }
            catch
            {
                result = null;
            }
            if (result == null)
            {
                Logger.LogError($"Unable to convert InsertCardData from {cardInsertion.SerializedValue}");
                // Log this
                return;
            }
            _ = this.ProcessCardInsertion(result, true);
        }

        private bool ProcessCardInsertion(InsertCardData cardToBeInserted, bool networkChange)
        {
            if(this.IsFull())
            {
                // log
                Logger.LogError($"Attempted to insert card into full container {cardToBeInserted}");
                return false;
            }
            Card? insertCard = this.CardFactory.GetCard(cardToBeInserted.cardId);
            if (insertCard == null)
            {
                // Log this
                Logger.LogError($"Could not find insert card of id {cardToBeInserted.cardId}");
                return false;
            }
            _ = insertCard.CurrentLocation?.RemoveCardFromContainer(cardToBeInserted.cardId, networkChange);
            insertCard.CurrentLocation = this;
            CardContainer destinationContainer = this.DetermineDestinationContainer(cardToBeInserted.insertPosition, cardToBeInserted.createNewContainer);
            destinationContainer.AddCardToContainer(insertCard, cardToBeInserted.containerInsertPosition);
            if (networkChange)
            {
                this.boardState.SetValue(this.ToList());
            }
            return true;
        }

        private bool IsFull()
        {
            if(!this.maxContainerCount.HasValue || this.Containers.Count < this.maxContainerCount)
            {
                return false;
            }
            return this.Containers.All(container => container.IsFull());
        }


        private CardContainer CreateAndInsertCardContainer(int insertPosition)
        {
            if (this.Containers.Count >= this.maxContainerCount)
            {
                return this.FindFirstEmptyContainer() ?? throw new Exception("Max container count reached, with all containers full");
            }
            CardContainer container = new CardContainer(this.maxCardCountPerContainer, this.Zone);
            this.Containers.Insert(insertPosition, container);
            return container;
        }

        private CardContainer? FindFirstEmptyContainer()
        {
            return this.Containers.FirstOrDefault(container => !container.IsFull());
        }

        private CardContainer DetermineDestinationContainer(int? targetContainerIndex, bool createNewContainer)
        {
            int rightwardBound = createNewContainer ? this.Containers.Count : Math.Max(0, this.Containers.Count - 1);
            int insertPosition = targetContainerIndex.HasValue ? Math.Clamp((int)targetContainerIndex, 0, rightwardBound) : rightwardBound;
            if (createNewContainer || this.Containers.Count == 0)
            {
                return this.CreateAndInsertCardContainer(insertPosition);
            }
            else
            {
                if (!this.Containers[insertPosition].IsFull())
                {
                    return this.Containers[insertPosition];
                }
                else
                {
                    return this.FindFirstEmptyContainer() ?? this.CreateAndInsertCardContainer(this.Containers.Count);
                }
            }
        }
    }
}