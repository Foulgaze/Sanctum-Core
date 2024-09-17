using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sanctum_Core
{
    public class CardContainer
    {
        public List<Card> Cards { get; } = new List<Card>();
        private readonly int? maxCardCount;
        private readonly CardZone parentZone;


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
        public void AddCardToContainer(Card card, int? position)
        {
            if(card.isEthereal && !this.ContainerIsOnField())
            {
                return;
            }
            this.ModifyAddedCard(card);
            int insertPosition = position == null ? this.Cards.Count : position.Value;
            insertPosition = Math.Clamp(insertPosition, 0, this.Cards.Count);
            this.Cards.Insert(insertPosition, card);
        }

        /// <summary>
        /// Shuffles the card container
        /// </summary>
        public void Shuffle()
        {
            Random rng = new Random();
            int n = this.Cards.Count;
            while (n > 1)
            {
                --n;
                int k = rng.Next(n + 1);
                (this.Cards[n], this.Cards[k]) = (this.Cards[k], this.Cards[n]);
            }
        }

        /// <summary>
        /// Gets the card IDs
        /// </summary>
        /// <returns>A list of all the card ids</returns>
        public List<int> GetCardIDs()
        {
            return this.Cards.Select(card => card.Id).ToList();
        }

        /// <summary>
        /// Gets the cards
        /// </summary>
        /// <returns>A list of all the card</returns>
        public List<Card> GetCards()
        {
            return this.Cards;
        }

        /// <summary>
        /// Checks if collection is full
        /// </summary>
        /// <returns>Returns if container is full</returns>
        public bool IsFull()
        {
            return this.maxCardCount != null && this.Cards.Count >= this.maxCardCount;
        }

        private bool ContainerIsOnField()
        {
            return this.parentZone == CardZone.MainField || this.parentZone == CardZone.RightField || this.parentZone == CardZone.LeftField;
        }

        private void ModifyAddedCard(Card card)
        {
            if (this.ContainerIsOnField())
            {
                this.ModifyCardForField();
                return;
            }
            this.ModifyCardForPile(card);
        }

        private void ModifyCardForField()
        {
            // Nothing to do here. Yet
        }

        private void ModifyCardForPile(Card card)
        {
            card.UpdateAttributes(null);
        }
    }
}