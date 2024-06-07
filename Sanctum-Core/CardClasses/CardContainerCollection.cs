using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core.CardClasses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class InsertCardData
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
    public class CardContainerCollection
    {
        CardZone Zone { get; set; }
        public string Owner { get; }
        List<CardContainer> Containers { get; set; } = new List<CardContainer>();
        private int? maxContainerCount;
        private int? maxContainerCardCount;
        NetworkAttribute<InsertCardData> insertOrRemoveCard;
        public event PropertyChangedEventHandler containerChanged = delegate { };

        private CardFactory CardFactory;

        public CardContainerCollection(CardZone zone, string owner, int? maxContainerCount, int? maxContainerCardCount, NetworkAttributeFactory networkAttributeManager, CardFactory cardFactory)
        {
            this.maxContainerCardCount = maxContainerCardCount;
            this.maxContainerCount = maxContainerCount;
            this.Zone = zone;
            this.Owner = owner;
            this.insertOrRemoveCard = networkAttributeManager.AddNetworkAttribute<InsertCardData>(owner, null);
            this.insertOrRemoveCard.valueChange += NetworkedCardInsert;
            this.CardFactory = cardFactory;

        }
        public void InsertCardIntoContainer(int insertPosition, bool createNewContainer, Card cardToInsert, int? cardContainerPosition, bool changeShouldBeNetworked)
        {

            if (changeShouldBeNetworked)
            {
                insertOrRemoveCard.Value = new InsertCardData(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer);
                return;
            }
            ProcessCardInsertion(new InsertCardData(insertPosition, cardToInsert.Id, cardContainerPosition, createNewContainer));
            containerChanged(this, new PropertyChangedEventArgs("Inserted"));
        }

        private void NetworkedCardInsert(object sender, PropertyChangedEventArgs args)
        {
            ProcessCardInsertion((InsertCardData)sender);
        }

        private bool ProcessCardInsertion(InsertCardData cardChange)
        {
            cardChange.insertPosition = Mathf.Clamp(cardChange.insertPosition, 0, Containers.Count);
            CardContainer destinationContainer = DetermineDestinationContainer(cardChange.insertPosition, cardChange.createNewContainer);
            Card? insertCard = CardFactory.GetCard(cardChange.cardID);
            if (insertCard == null)
            {
                return false;
            }
            destinationContainer.AddCardToContainer(insertCard, cardChange.containerInsertPosition);
            return true;
        }

        private CardContainer CreateAndInsertCardContainer(int insertPosition)
        {
            CardContainer container = new CardContainer(Owner, maxContainerCardCount);
            Containers.Insert(insertPosition, container);
            return container;
        }

        private CardContainer DetermineDestinationContainer(int targetContainerIndex, bool createNewContainer)
        {
            if (createNewContainer)
            {
                if (Containers.Count >= maxContainerCount)
                {
                    //Probably LOG this
                    return Containers[0];
                }
                return CreateAndInsertCardContainer(targetContainerIndex);
            }
            else
            {
                return Containers[targetContainerIndex];
            }
        }

        /// <summary>
        /// Gets the name of the card zone.
        /// </summary>
        /// <returns>The name of the card zone.</returns>
        public string GetName()
        {
            return Enum.GetName(typeof(CardZone), Zone);
        }
    }
}
