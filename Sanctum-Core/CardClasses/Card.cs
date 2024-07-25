
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
        public NetworkAttribute<bool> tapped;
        public NetworkAttribute<string> name;
        public NetworkAttribute<bool> isUsingBackSide;
        public NetworkAttribute<bool> isFlipped;
        public bool ethereal = false;
        private readonly NetworkAttributeFactory networkAttributeFactory;

        public Card(int id, CardInfo FrontInfo, CardInfo? BackInfo, NetworkAttributeFactory networkAttributeFactory)
        {
            this.networkAttributeFactory = networkAttributeFactory;
            this.Id = id;
            this.FrontInfo = FrontInfo;
            this.BackInfo = BackInfo;

            this.isUsingBackSide = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-usingbackside", false);
            this.isUsingBackSide.valueChange += this.UpdateAttributes;
            this.isFlipped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-flipped", false);
            this.power = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-power", this.ParsePT(this.CurrentInfo.power));
            this.toughness = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-toughness", this.ParsePT(this.CurrentInfo.toughness));
            this.tapped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-tapped", false);
            this.name = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Id}-name", "");
        }

        /// <summary>
        /// Checks if card has backside
        /// </summary>
        /// <returns><Returns true if the card has a backside/returns>
        public bool HasBackside()
        {
            return this.BackInfo != null;
        }

        public void UpdateAttributes(object? sender, EventArgs e) // No need to network because flipped is netwokred
        {
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
