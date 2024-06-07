namespace Sanctum_Core
{
    public class Card
    {
        public int Id { get; }
        public CardInfo FrontInfo { get; }
        public CardInfo BackInfo { get; }
        public CardInfo CurrentInfo => this.isFlipped.Value ? this.BackInfo : this.FrontInfo;
        public CardContainer CurrentLocation { get; set; }
        public NetworkAttribute<int> power;
        public NetworkAttribute<int> toughness;
        public NetworkAttribute<bool> tapped;
        public NetworkAttribute<string> name;
        public NetworkAttribute<bool> isFlipped;
        public bool ethereal = false;


        public Card(int id, CardInfo FrontInfo, CardInfo BackInfo, NetworkAttributeFactory networkAttributeFactory)
        {
            this.Id = id;
            this.FrontInfo = FrontInfo;
            this.BackInfo = BackInfo;

            this.InitializeAttributes(networkAttributeFactory);
        }

        /// <summary>
        /// Checks if card has backside
        /// </summary>
        /// <returns><Returns true if the card has a backside/returns>
        public bool HasBackside()
        {
            return this.BackInfo != null;
        }

        private void InitializeAttributes(NetworkAttributeFactory networkAttributeFactory)
        {
            this.isFlipped = networkAttributeFactory.AddNetworkAttribute<bool>(this.Id.ToString(), false);
            this.isFlipped.valueChange += this.UpdateAttributes;
            this.power = networkAttributeFactory.AddNetworkAttribute<int>(this.Id.ToString(), this.ParsePT(this.CurrentInfo.power));
            this.toughness = networkAttributeFactory.AddNetworkAttribute<int>(this.Id.ToString(), this.ParsePT(this.CurrentInfo.toughness));
            this.tapped = networkAttributeFactory.AddNetworkAttribute<bool>(this.Id.ToString(), false);
        }


        private void UpdateAttributes(object sender, EventArgs e) // No need to network because flipped is netwokred
        {
            this.power.NonNetworkedSet(this.ParsePT(this.CurrentInfo.power));
            this.toughness.NonNetworkedSet(this.ParsePT(this.CurrentInfo.toughness));
            this.name.NonNetworkedSet(this.CurrentInfo.name);
        }

        private int ParsePT(string value)
        {

            return !int.TryParse(value, out int parsedValue) ? 0 : parsedValue;
        }

    }
}
