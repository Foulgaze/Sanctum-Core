using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    

    public class CardContainerCollection
    {
        private class InsertCardData
        {
            public int insertPosition;
            public int cardID;
            public int? containerInsertPosition;
            public bool createNewContainer;
            public InsertCardData(int insertPosition, int cardID, int? containerInsertPosition, bool createNewContainer)
            {
                this.insertPosition = insertPosition;
                this.cardID = cardID;
                this.containerInsertPosition = containerInsertPosition;
                this.createNewContainer = createNewContainer;
            }
        }
        public CardZone Zone { get; set; }
        public string Owner { get; }
        public List<CardContainer> Containers { get; set; } = new List<CardContainer>();
        private readonly int? maxContainerCount;
        private readonly int? maxContainerCardCount;
        readonly NetworkAttribute<InsertCardData> insertOrRemoveCard;
        public event PropertyChangedEventHandler containerChanged = delegate { };
        private readonly CardFactory CardFactory;

        public CardContainerCollection(CardZone zone, string owner, int? maxContainerCount, int? maxContainerCardCount, NetworkAttributeFactory networkAttributeManager, CardFactory cardFactory)
        {
            this.maxContainerCardCount = maxContainerCardCount;
            this.maxContainerCount = maxContainerCount;
            this.Zone = zone;
            this.Owner = owner;
            this.insertOrRemoveCard = networkAttributeManager.AddNetworkAttribute<InsertCardData>(owner, null);
            this.insertOrRemoveCard.valueChange += this.NetworkedCardInsert;
            this.CardFactory = cardFactory;

        }
        public void InsertCardIntoContainer(int insertPosition, bool createNewContainer, Card cardToInsert, int? cardContainerPosition, bool changeShouldBeNetworked)
        {

            if (changeShouldBeNetworked)
            {
                this.insertOrRemoveCard.Value = new InsertCardData(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer);
                return;
            }
            if(this.ProcessCardInsertion(new InsertCardData(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer)))
            {
                containerChanged(this, new PropertyChangedEventArgs("Inserted"));
            }
        }

        private void NetworkedCardInsert(object sender, PropertyChangedEventArgs args)
        {
            _ = this.ProcessCardInsertion((InsertCardData)sender);
        }

        private bool ProcessCardInsertion(InsertCardData cardChange)
        {
            cardChange.insertPosition = Math.Clamp(cardChange.insertPosition, 0, this.Containers.Count);
            CardContainer destinationContainer = this.DetermineDestinationContainer(cardChange.insertPosition, cardChange.createNewContainer);
            Card? insertCard = this.CardFactory.GetCard(cardChange.cardID);
            if (insertCard == null)
            {
                return false;
            }
            destinationContainer.AddCardToContainer(insertCard, cardChange.containerInsertPosition);
            return true;
        }

        private CardContainer CreateAndInsertCardContainer(int insertPosition)
        {
            CardContainer container = new(this.maxContainerCardCount);
            this.Containers.Insert(insertPosition, container);
            return container;
        }

        private CardContainer DetermineDestinationContainer(int targetContainerIndex, bool createNewContainer)
        {
            if (createNewContainer)
            {
                if (this.Containers.Count >= this.maxContainerCount)
                {
                    //Probably LOG this
                    return this.Containers[0];
                }
                return this.CreateAndInsertCardContainer(targetContainerIndex);
            }
            else
            {
                return this.Containers[targetContainerIndex];
            }
        }

        /// <summary>
        /// Gets the name of the card zone.
        /// </summary>
        /// <returns>The name of the card zone.</returns>
        public string GetName()
        {
            return Enum.GetName(typeof(CardZone), this.Zone);
        }
    }
}
