﻿
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

        public Card(int id, CardInfo FrontInfo, CardInfo? BackInfo, NetworkAttributeFactory networkAttributeFactory, bool isEthereal)
        {
            this.networkAttributeFactory = networkAttributeFactory;
            this.Id = id;
            this.FrontInfo = FrontInfo;
            this.BackInfo = BackInfo;
            this.isEthereal = isEthereal;

            this.isUsingBackSide = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-usingbackside", false);
            this.isUsingBackSide.valueChange += this.UpdateAttributes;
            this.isFlipped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-flipped", false);
            this.power = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-power", this.ParsePT(this.CurrentInfo.power));
            this.toughness = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-toughness", this.ParsePT(this.CurrentInfo.toughness));
            this.isTapped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-tapped", false);
            this.name = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Id}-name", "");
        }

        public Card(int id, NetworkAttributeFactory networkAttributeFactory, Card cardToCopy)
        {
            this.Id = id;
            this.FrontInfo = cardToCopy.FrontInfo;
            this.BackInfo = cardToCopy.BackInfo;
            this.isEthereal = cardToCopy.isEthereal;
            this.networkAttributeFactory = networkAttributeFactory;

            this.isUsingBackSide = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-usingbackside", cardToCopy.isUsingBackSide.Value);
            this.isUsingBackSide.valueChange += this.UpdateAttributes;
            this.isFlipped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-flipped", cardToCopy.isFlipped.Value);
            this.power = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-power", cardToCopy.power.Value);
            this.toughness = this.networkAttributeFactory.AddNetworkAttribute<int>($"{this.Id}-toughness", cardToCopy.toughness.Value);
            this.isTapped = this.networkAttributeFactory.AddNetworkAttribute<bool>($"{this.Id}-tapped", cardToCopy.isTapped.Value);
            this.name = this.networkAttributeFactory.AddNetworkAttribute<string>($"{this.Id}-name", cardToCopy.name.Value);
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
