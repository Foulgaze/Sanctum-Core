
using System.Reflection;

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
        public NetworkAttribute<(int, int, int)> changeCounters;
        public NetworkAttribute<int> redCounters;
        public NetworkAttribute<int> blueCounters;
        public NetworkAttribute<int> greenCounters;
        public NetworkAttribute<bool> isIncreasingPower { get; set; }
        public NetworkAttribute<bool> isIncreasingToughness { get; set; }


        public bool isEthereal = false;
        private readonly NetworkAttributeFactory networkAttributeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class with the specified attributes and sets up network attributes.
        /// </summary>
        /// <param name="id">The unique identifier for the card.</param>
        /// <param name="FrontInfo">The information for the frocnt side of the card.</param>
        /// <param name="BackInfo">The information for the back side of the card (nullable).</param>
        /// <param name="networkAttributeFactory">The factory for creating network attributes.</param>
        /// <param name="isEthereal">Indicates whether the card will be destroyed upon being moved from the field.</param>
        public Card(int id, CardInfo FrontInfo, CardInfo? BackInfo, NetworkAttributeFactory networkAttributeFactory, bool isEthereal, bool isSlave)
        {
            this.networkAttributeFactory = networkAttributeFactory;
            this.Id = id;
            this.FrontInfo = FrontInfo;
            this.BackInfo = BackInfo;
            this.isEthereal = isEthereal;
            this.InitializeAttributes(isSlave);
            this.RegisterListeners(isSlave);
        }

        private void InitializeAttributes(bool isSlave)
        {
            this.isUsingBackSide = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-usingbackside", false);
            this.isFlipped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-flipped", false);
            this.power = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-power", this.ParsePT(this.CurrentInfo.power));
            this.toughness = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-toughness", this.ParsePT(this.CurrentInfo.toughness));
            this.isTapped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-tapped", false);
            this.name = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Id}-name", this.CurrentInfo.name);
            this.isIncreasingPower = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-powerchange", false, setWithoutEqualityCheck: true, networkChange: isSlave, outsideSettable: !isSlave);
            this.isIncreasingToughness = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-toughnesschange", false, setWithoutEqualityCheck: true, networkChange: isSlave, outsideSettable: !isSlave);
            this.redCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-redcounters", 0);
            this.blueCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-bluecounters", 0);
            this.greenCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-greencounters", 0);
            this.changeCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-changecounters", (0, 0, 0), setWithoutEqualityCheck: true, networkChange: isSlave, outsideSettable: !isSlave);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class as a copy of an existing card, with a new identifier.
        /// </summary>
        /// <param name="id">The unique identifier for the new card.</param>
        /// <param name="networkAttributeFactory">The factory for creating network attributes.</param>
        /// <param name="cardToCopy">The card to copy attributes from.</param>
        public Card(int id, NetworkAttributeFactory networkAttributeFactory, Card cardToCopy, bool isSlave)
        {
            this.Id = id;
            this.FrontInfo = cardToCopy.FrontInfo;
            this.BackInfo = cardToCopy.BackInfo;
            this.isEthereal = true;
            this.networkAttributeFactory = networkAttributeFactory;

            this.InitializeAttributes(cardToCopy, isSlave);
            this.RegisterListeners(isSlave);
        }

        private void InitializeAttributes(Card cardToCopy, bool isSlave)
        {
            this.isUsingBackSide = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-usingbackside", cardToCopy.isUsingBackSide.Value);
            this.isFlipped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-flipped", cardToCopy.isFlipped.Value);
            this.power = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-power", cardToCopy.power.Value);
            this.toughness = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-toughness", cardToCopy.toughness.Value);
            this.isTapped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-tapped", cardToCopy.isTapped.Value);
            this.name = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Id}-name", this.CurrentInfo.name);
            this.isIncreasingPower = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-powerchange", false, setWithoutEqualityCheck: true, networkChange: isSlave);
            this.isIncreasingToughness = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-toughnesschange", false, setWithoutEqualityCheck: true, networkChange: isSlave);
            this.redCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-redcounters", cardToCopy.redCounters.Value);
            this.blueCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-bluecounters", cardToCopy.blueCounters.Value);
            this.greenCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-greencounters", cardToCopy.greenCounters.Value);
            this.changeCounters = this.networkAttributeFactory.AddNetworkAttribute($"{this.Id}-changecounters", (0, 0, 0), setWithoutEqualityCheck: true, networkChange: isSlave);
        }

        private void RegisterListeners(bool isSlave)
        {
            if(isSlave)
            {
                return;
            }
            this.isIncreasingToughness.valueChanged += this.ChangeToughness;
            this.changeCounters.valueChanged += this.HandleCounterChange;
            this.isIncreasingPower.valueChanged += this.ChangePower;
            this.isFlipped.valueChanged += this.HandleFlipped;
            this.isUsingBackSide.valueChanged += this.UpdateAttributes;
        }

        /// <summary>
        /// Deregisters all listeners from a card
        /// </summary>
        public void DeregisterListeners()
        {
            this.isIncreasingToughness.valueChanged -= this.ChangeToughness;
            this.changeCounters.valueChanged -= this.HandleCounterChange;
            this.isIncreasingPower.valueChanged -= this.ChangePower;
            this.isFlipped.valueChanged -= this.HandleFlipped;
            this.isUsingBackSide.valueChanged -= this.UpdateAttributes;
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
        /// <param name="attribute"> The attribute that has been changed</param>
        public void UpdateAttributes(NetworkAttribute? attribute)
        {
            if(attribute != this.isFlipped)
            {
                this.isFlipped.SetValue(false);
            }
            if(attribute != this.isUsingBackSide)
            {
                this.isUsingBackSide.SetValue(false);
            }
            this.isTapped.SetValue(false);
            this.redCounters.SetValue(0);
            this.blueCounters.SetValue(0);
            this.greenCounters.SetValue(0);
            this.power.SetValue(this.ParsePT(this.CurrentInfo.power));
            this.toughness.SetValue(this.ParsePT(this.CurrentInfo.toughness));
            this.name.SetValue(this.CurrentInfo.name);
        }

        private void ChangePower(NetworkAttribute _)
        {
            int incriment = this.isIncreasingPower.Value ? 1 : -1;
            this.power.SetValue(this.power.Value + incriment);
        }
        private void ChangeToughness(NetworkAttribute _)
        {
            int incriment = this.isIncreasingToughness.Value ? 1 : -1;
            this.toughness.SetValue(this.toughness.Value + incriment);
        }

        private void HandleCounterChange(NetworkAttribute _)
        {
            (int redChange, int greenChange, int blueChange) = this.changeCounters.Value;

            this.UpdateCounter(redChange, this.redCounters);
            this.UpdateCounter(greenChange, this.greenCounters);
            this.UpdateCounter(blueChange, this.blueCounters);
        }

        private void UpdateCounter(int changeValue, NetworkAttribute<int> counter)
        {
            counter.SetValue(counter.Value + changeValue);
        }


        private int ParsePT(string value)
        {

            return !int.TryParse(value, out int parsedValue) ? 0 : parsedValue;
        }

        private void HandleFlipped(NetworkAttribute attribute)
        {
            if (!((NetworkAttribute<bool>)attribute).Value)
            {
                this.UpdateAttributes(null);
            }
        }
    }
}
