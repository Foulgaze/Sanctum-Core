namespace Sanctum_Core.CardClasses
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;

    public class Card
    {
        public int Id { get; }
        public CardInfo FrontInfo { get; }
        public CardInfo BackInfo { get; }
        public CardInfo CurrentInfo => isFlipped.Value ? BackInfo : FrontInfo;
        public CardContainer CurrentLocation { get; set; }
        public NetworkAttribute<int> power;
        public NetworkAttribute<int> toughness;
        public NetworkAttribute<bool> tapped;
        public NetworkAttribute<string> name;
        public NetworkAttribute<bool> isFlipped;
        public bool ethereal = false;


        public Card(int id, CardInfo FrontInfo, CardInfo BackInfo, NetworkAttributeFactory networkAttributeFactory)
        {
            Id = id;
            this.FrontInfo = FrontInfo;
            this.BackInfo = BackInfo;

            InitializeAttributes(networkAttributeFactory);
        }

        /// <summary>
        /// Checks if card has backside
        /// </summary>
        /// <returns><Returns true if the card has a backside/returns>
        public bool HasBackside()
        {
            return BackInfo != null;
        }

        private void InitializeAttributes(NetworkAttributeFactory networkAttributeFactory)
        {
            isFlipped = networkAttributeFactory.AddNetworkAttribute<bool>(Id.ToString(), false);
            isFlipped.valueChange += UpdateAttributes;
            power = networkAttributeFactory.AddNetworkAttribute<int>(Id.ToString(), ParsePT(CurrentInfo.power));
            toughness = networkAttributeFactory.AddNetworkAttribute<int>(Id.ToString(), ParsePT(CurrentInfo.toughness));
            tapped = networkAttributeFactory.AddNetworkAttribute<bool>(Id.ToString(), false);
        }


        private void UpdateAttributes(object sender, EventArgs e) // No need to network because flipped is netwokred
        {
            power.NonNetworkedSet(ParsePT(CurrentInfo.power));
            toughness.NonNetworkedSet(ParsePT(CurrentInfo.toughness));
            name.NonNetworkedSet(CurrentInfo.name);
        }

        private int ParsePT(string value)
        {
            int parsedValue;
            if (!int.TryParse(value, out parsedValue))
            {
                return 0;
            }
            return parsedValue;
        }

    }
}
