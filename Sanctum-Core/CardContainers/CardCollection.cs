using System.ComponentModel;

namespace Sanctum_Core
{
    public class CardContainer
    {
        public List<Card> Cards { get; } = new();
        private readonly int? maxCardCount;
        private readonly CardZone parentZone;

        /// <summary>
        /// Occurs when the cards collection changes.
        /// </summary>
        public event PropertyChangedEventHandler cardsChanged = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="CardContainer"/> class with the specified zone and owner.
        /// </summary>
        /// <param name="zone">The zone of the card container.</param>
        /// <param name="owner">The owner of the card container.</param>
        public CardContainer(int? maxCardCount, CardZone parentZone)
        {
            this.maxCardCount = maxCardCount;
            this.parentZone = parentZone;
        }

        /// <summary>
        /// Adds a card to the container at the specified position.
        /// </summary>
        /// <param name="card">The card to add.</param>
        /// <param name="position">The position to insert the card at. If null, the card is added to the end.</param>
        /// <param name="networkChange">If set to <c>true</c>, triggers the <see cref="cardsChanged"/> event.</param>
        public void AddCardToContainer(Card card, int? position)
        {
            this.ModifyAddedCard(card);
            int insertPosition = position == null ? this.Cards.Count : position.Value;
            insertPosition = Math.Clamp(insertPosition, 0, this.Cards.Count);
            this.Cards.Insert(insertPosition, card);

            cardsChanged(this, new PropertyChangedEventArgs("added"));
        }

        private void ModifyAddedCard(Card card)
        {
            switch (this.parentZone)
            {
                case CardZone.MainField:
                case CardZone.LeftField:
                case CardZone.RightField:
                    this.ModifyCardForField();
                    return;
                default:
                    this.ModifyCardForPile(card);
                    return;
            }
        }

        private void ModifyCardForField()
        {
            // Nothing to do here. Yet
        }
        
        private void ModifyCardForPile(Card card)
        {
            // Reset Card To Original State
            if(card.isFlipped.Value)
            {
                card.isFlipped.SetValue(false);
            }
            if(card.isUsingBackSide.Value)
            {
                card.isUsingBackSide.SetValue(false);
            } 
            else
            {
                card.UpdateAttributes(null, new PropertyChangedEventArgs(""));
            }

        }

        public List<int> SerializeContainer()
        {
            return this.Cards.Select(card => card.Id).ToList();
        }

        public bool IsFull()
        {
            return this.maxCardCount != null && this.Cards.Count >= this.maxCardCount;
        }
    }
}