﻿using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core
{
    public class PlayerDescription
    {
        public readonly string name;
        public readonly string uuid;
        public readonly TcpClient client;
        public readonly StringBuilder buffer = new();
        public NetworkStream stream => this.client.GetStream();
        public PlayerDescription(string name, string uuid, TcpClient client)
        {
            this.name = name;
            this.uuid = uuid;
            this.client = client;
        }
    }
    public class Lobby
    {
        private readonly Playtable playtable;
        public readonly int size;
        private List<PlayerDescription> players = new();
        public readonly string code;
        public ConcurrentBag<PlayerDescription> concurrentPlayers = new();

        public Lobby(int lobbySize, string lobbyCode)
        {
            this.size = lobbySize;
            this.code = lobbyCode;
            string path = Path.GetFullPath(@"..\..\..\..\Sanctum-Core\Assets\");
            this.playtable = new Playtable(lobbySize, $"{path}cards.csv", $"{path}tokens.csv");
        }

        private void NetworkAttributeChanged(object? sender, PropertyChangedEventArgs? args)
        {
            if (args == null)
            {
                // Log this
                return;
            }
            this.SendMessage(NetworkInstruction.NetworkAttribute, $"{sender}|{args.PropertyName}");
        }

        private void NetworkBoardChange(object? sender, PropertyChangedEventArgs? args)
        {
            if (sender == null)
            {
                // Log this
                return;
            }
            CardContainerCollection cardContainerCollection = (CardContainerCollection)sender;
            List<List<int>> allCards = cardContainerCollection.ContainerCollectionToList();
            string cardsSerialized = JsonConvert.SerializeObject(allCards);
            this.SendMessage(NetworkInstruction.BoardUpdate, $"{cardContainerCollection.Owner}-{(int)cardContainerCollection.Zone}|{cardsSerialized}");
        }

        private void NetworkCardCreation(object? sender, PropertyChangedEventArgs? args)
        {
            if(sender is not Card)
            {
                return;
            }
            Card card = (Card)sender;
            // This function should probably only be called by create token. Hence the identifier is the UUID.
            this.players.ForEach(playerDescription => Server.SendMessage(playerDescription.client.GetStream(), NetworkInstruction.CardCreation, $"{card.CurrentInfo.uuid}|{card.Id}"));
        }

        private void SendMessage(NetworkInstruction instruction, string payload)
        {
            this.players.ForEach(playerDescription => Server.SendMessage(playerDescription.client.GetStream(), instruction, payload));
        }

        private void InitGame()
        {
            this.players = this.concurrentPlayers.ToList(); // Ignore concurrent players once lobby starts
            this.players.ForEach(description => this.playtable.AddPlayer(description.uuid, description.name));
            string lobbyDescription = JsonConvert.SerializeObject(this.players.ToDictionary(player => player.uuid, player => player.name));
            this.players.ForEach(description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.StartGame, lobbyDescription));
            this.playtable.networkAttributeFactory.attributeValueChanged += this.NetworkAttributeChanged;
            this.playtable.boardChanged += this.NetworkBoardChange;
            this.playtable.cardCreated += this.NetworkCardCreation;
        }

        public void StartLobby()
        {
            this.InitGame();
            while (true)
            {
                foreach (PlayerDescription playerDescription in this.players)
                {
                    NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(playerDescription.client.GetStream(), playerDescription.buffer, Server.bufferSize, false);
                    this.HandleCommand(command, playerDescription.uuid);
                }
            }
        }

        private void HandleCommand(NetworkCommand? command, string uuid)
        {
            if (command == null)
            {
                return;
            }
            switch (command.opCode)
            {
                case (int)NetworkInstruction.NetworkAttribute:
                    this.playtable.networkAttributeFactory.HandleNetworkedAttribute(command.instruction, new PropertyChangedEventArgs("instruction"));
                    break;
                case (int)NetworkInstruction.SpecialAction:
                    this.playtable.HandleSpecialAction(command.instruction,uuid);
                    break;
                default:
                    // Ignore!
                    break;
            }
        }

        public void StopLobby()
        {
            this.players.ForEach(description => description.client.Close());
        }
    }
}
