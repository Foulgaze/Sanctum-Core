using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{

    public class InsertCardData
    {
        public int? insertPosition;
        public int cardID;
        public int? containerInsertPosition;
        public bool createNewContainer;
        public InsertCardData(int? insertPosition, int cardID, int? containerInsertPosition, bool createNewContainer)
        {
            this.insertPosition = insertPosition;
            this.cardID = cardID;
            this.containerInsertPosition = containerInsertPosition;
            this.createNewContainer = createNewContainer;
        }
    }
    public class CardContainerCollection
    {

        public CardZone Zone { get; set; }
        public string Owner { get; }
        private readonly List<CardContainer> Containers = new();
        private readonly int? maxContainerCount;
        private readonly int? maxCardCountPerContainer;
        private readonly NetworkAttribute<InsertCardData> insertCardData;
        public NetworkAttribute<bool> revealTopCard;
        public event PropertyChangedEventHandler boardChanged = delegate { };
        private readonly CardFactory CardFactory;

        public CardContainerCollection(CardZone zone, string owner, int? maxContainerCount, int? maxContainerCardCount, bool revealTopCard, NetworkAttributeFactory networkAttributeManager, CardFactory cardFactory)
        {
            this.maxCardCountPerContainer = maxContainerCardCount;
            this.maxContainerCount = maxContainerCount;
            this.Zone = zone;
            this.Owner = owner;
            this.insertCardData = networkAttributeManager.AddNetworkAttribute($"{owner}-{(int)this.Zone}-insert", new InsertCardData(null, 0, null, false), true, false);
            this.revealTopCard = networkAttributeManager.AddNetworkAttribute($"{owner}-{(int)this.Zone}-reveal", revealTopCard);
            this.insertCardData.valueChange += this.NetworkedCardInsert;

            this.CardFactory = cardFactory;

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
                InsertCardData newCardData = new(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer);
                this.insertCardData.SetValue(newCardData);
                return;
            }
            _ = this.ProcessCardInsertion(new InsertCardData(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer), false);
        }
        /// <summary>
        /// Removes a card from the first card container that matches ID
        /// </summary>
        /// <param name="cardID">The id of the card to remove</param>
        /// <returns>true if removed else false</returns>
        public bool RemoveCardFromContainer(int cardID, bool networkChange = true)
        {
            foreach (CardContainer container in this.Containers)
            {
                Card? cardToRemove = container.Cards.FirstOrDefault(card => card.Id == cardID);
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
                        boardChanged(this, new PropertyChangedEventArgs("removed"));
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
        /// Creates a serialized version of the card containers
        /// </summary>
        /// <returns></returns>
        public List<List<int>> ContainerCollectionToList()
        {
            return this.Containers.Select(container => container.GetCardIDs()).ToList();
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

        public bool ContainsCard(Card card)
        {
            return this.Containers.Any(container => container.Cards.Contains(card));
        }

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


        private void NetworkedCardInsert(object? sender, PropertyChangedEventArgs? args)
        {
            InsertCardData? result;
            try
            {
                if (args == null || args.PropertyName == null)
                {
                    // Log this
                    return;
                }
                result = JsonConvert.DeserializeObject<InsertCardData>(args.PropertyName);
            }
            catch
            {
                result = null;
            }
            if (result == null)
            {
                // Log this
                return;
            }
            _ = this.ProcessCardInsertion(result, true);
        }

        private bool ProcessCardInsertion(InsertCardData cardChange, bool networkChange)
        {
            if(this.IsFull())
            {
                // log
                return false;
            }
            CardContainer destinationContainer = this.DetermineDestinationContainer(cardChange.insertPosition, cardChange.createNewContainer);
            Card? insertCard = this.CardFactory.GetCard(cardChange.cardID);
            if (insertCard == null)
            {
                // Log this
                return false;
            }
            _ = insertCard.CurrentLocation?.RemoveCardFromContainer(cardChange.cardID, networkChange);
            insertCard.CurrentLocation = this;
            destinationContainer.AddCardToContainer(insertCard, cardChange.containerInsertPosition);
            if (networkChange)
            {
                boardChanged(this, new PropertyChangedEventArgs("Oof"));
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
            CardContainer container = new(this.maxCardCountPerContainer, this.Zone);
            this.Containers.Insert(insertPosition, container);
            return container;
        }

        private CardContainer? FindFirstEmptyContainer()
        {
            return this.Containers.LastOrDefault(container => !container.IsFull());
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