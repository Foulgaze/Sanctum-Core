
namespace Sanctum_Core
{
    public class Card
    {
        public int Id { get; }
        public CardInfo FrontInfo { get; }
        public CardInfo? BackInfo { get; }
        public CardInfo CurrentInfo => this.isUsingBackSide.Value && this.BackInfo != null ? this.BackInfo : this.FrontInfo;
        public CardContainerCollection? CurrentLocation { get; set; } = null;
        public NetworkAttribute<int> power;
        public NetworkAttribute<int> toughness;
        public NetworkAttribute<bool> isTapped;
        public NetworkAttribute<string> name;
        public NetworkAttribute<bool> isUsingBackSide;
        public NetworkAttribute<bool> isFlipped;
        public bool isEthereal = false;
        private readonly NetworkAttributeFactory networkAttributeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class with the specified attributes and sets up network attributes.
        /// </summary>
        /// <param name="id">The unique identifier for the card.</param>
        /// <param name="FrontInfo">The information for the front side of the card.</param>
        /// <param name="BackInfo">The information for the back side of the card (nullable).</param>
        /// <param name="networkAttributeFactory">The factory for creating network attributes.</param>
        /// <param name="isEthereal">Indicates whether the card will be destroyed upon being moved from the field.</param>
        public Card(int id, CardInfo FrontInfo, CardInfo? BackInfo, NetworkAttributeFactory networkAttributeFactory, bool isEthereal)
        {
            this.networkAttributeFactory = networkAttributeFactory;
            this.Id = id;
            this.FrontInfo = FrontInfo;
            this.BackInfo = BackInfo;
            this.isEthereal = isEthereal;

            this.isUsingBackSide = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-usingbackside", false);
            this.isUsingBackSide.valueChanged += this.UpdateAttributes;
            this.isFlipped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-flipped", false);
            this.power = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-power", this.ParsePT(this.CurrentInfo.power));
            this.toughness = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-toughness", this.ParsePT(this.CurrentInfo.toughness));
            this.isTapped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-tapped", false);
            this.name = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Id}-name", this.CurrentInfo.name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class as a copy of an existing card, with a new identifier.
        /// </summary>
        /// <param name="id">The unique identifier for the new card.</param>
        /// <param name="networkAttributeFactory">The factory for creating network attributes.</param>
        /// <param name="cardToCopy">The card to copy attributes from.</param>
        public Card(int id, NetworkAttributeFactory networkAttributeFactory, Card cardToCopy)
        {
            this.Id = id;
            this.FrontInfo = cardToCopy.FrontInfo;
            this.BackInfo = cardToCopy.BackInfo;
            this.isEthereal = cardToCopy.isEthereal;
            this.networkAttributeFactory = networkAttributeFactory;

            this.isUsingBackSide = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-usingbackside", cardToCopy.isUsingBackSide.Value);
            this.isUsingBackSide.valueChanged += this.UpdateAttributes;
            this.isFlipped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-flipped", cardToCopy.isFlipped.Value);
            this.power = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-power", cardToCopy.power.Value);
            this.toughness = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-toughness", cardToCopy.toughness.Value);
            this.isTapped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-tapped", cardToCopy.isTapped.Value);
            this.name = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Id}-name", this.CurrentInfo.name);
        }

        /// <summary>
        /// Checks if card has backside
        /// </summary>
        /// <returns><Returns true if the card has a backside/returns>
        public bool HasBackside()
        {
            return this.BackInfo != null;
        }

        /// <summary>
        /// Updates the card's attributes based on the current card information.
        /// Resets the flipped state and backside usage, then updates power, toughness, and name.
        /// </summary>
        /// <param name="sender">The source of the event triggering the update (can be null).</param>
        /// <param name="e">Event arguments associated with the update.</param>
        public void UpdateAttributes(NetworkAttribute? _)
        {
            this.isFlipped.SetValue(false);
            this.isUsingBackSide.SetValue(false);
            this.power.SetValue(this.ParsePT(this.CurrentInfo.power));
            this.toughness.SetValue(this.ParsePT(this.CurrentInfo.toughness));
            this.name.SetValue(this.CurrentInfo.name);
        }

        private int ParsePT(string value)
        {

            return !int.TryParse(value, out int parsedValue) ? 0 : parsedValue;
        }
    }
}
