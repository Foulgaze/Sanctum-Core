using Newtonsoft.Json;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core
{
    public class TimeChecker
    {
        private DateTime lastCheckedTime;
        private readonly double timeToWait;

        public TimeChecker(double timeToWait = 0.5)
        {
            this.lastCheckedTime = DateTime.Now;
            this.timeToWait = timeToWait;
        }

        public bool HasTimerPassed()
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan timeElapsed = currentTime - this.lastCheckedTime;

            if (timeElapsed.TotalMinutes >= this.timeToWait)
            {
                this.lastCheckedTime = currentTime;
                return true;
            }

            return false;
        }
    }
    public class Lobby
    {
        public event Action<Lobby> OnLobbyClosed = delegate { };
        private readonly Playtable playtable;
        public readonly int size;
        public readonly string code;
        public List<LobbyConnection> connections = new();
        private readonly TimeChecker timeChecker = new();
        public bool LobbyStarted { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lobby"/> class with the specified size and code.
        /// </summary>
        /// <param name="lobbySize">The maximum number of players allowed in the lobby.</param>
        /// <param name="lobbyCode">The unique code identifying the lobby.</param>
        /// <remarks>
        /// The constructor sets up the playtable for the lobby, loading the card and token data from CSV files located in the designated assets directory.
        /// </remarks>
        public Lobby(int lobbySize, string lobbyCode)
        {
            this.size = lobbySize;
            this.code = lobbyCode;
            string path = Path.GetFullPath(@"..\..\..\..\Sanctum-Core\Assets\");
            this.playtable = new Playtable(lobbySize, $"{path}cards.csv", $"{path}tokens.csv");
        }

        private void NetworkAttributeChanged(NetworkAttribute attribute)
        {
            this.SendMessage(NetworkInstruction.NetworkAttribute, $"{attribute.Id}|{attribute.SerializedValue}");
        }


        private void NetworkCardCreation(Card card)
        {
            this.connections.ForEach(playerDescription => Server.SendMessage(playerDescription.client.GetStream(), NetworkInstruction.CardCreation, $"{card.CurrentInfo.uuid}|{card.Id}"));
        }

        private void SendMessage(NetworkInstruction instruction, string payload)
        {
            this.connections.ForEach(playerDescription => Server.SendMessage(playerDescription.client.GetStream(), instruction, payload));
        }

        private void InitGame()
        {
            this.LobbyStarted = true;  
            this.connections.ForEach(description => this.playtable.AddPlayer(description.uuid, description.name));
            string lobbyDescription = JsonConvert.SerializeObject(this.connections.ToDictionary(player => player.uuid, player => player.name));
            this.connections.ForEach(description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.StartGame, lobbyDescription));
            this.playtable.networkAttributeFactory.attributeValueChanged += this.NetworkAttributeChanged;
            this.playtable.cardCreated += this.NetworkCardCreation;
        }

        /// <summary>
        /// Starts the lobby by initializing the game and continuously listening for player commands.
        /// </summary>
        public void StartLobby()
        {
            this.InitGame();
            while (true)
            {
                for(int i = this.connections.Count - 1; i >= 0; --i)
                {
                    LobbyConnection connection = this.connections[i];
                    NetworkCommand? command = connection.GetNetworkCommand();
                    if(connection.IsNetworkStreamClosed)
                    {
                        this.connections.RemoveAt(i);
                        this.connections.ForEach(this.SendDisconnectMessages);
                        continue;
                    }
                    this.HandleCommand(command, connection.uuid);
                }
                if (this.connections.Count == 0)
                {
                    OnLobbyClosed?.Invoke(this);
                    Logger.Log($"Closing lobby {this.code}");
                    return;
                }
            }
        }

        /// <summary>
        /// Adds a player to the lobby's connection list
        /// </summary>
        /// <param name="connection">The lobby connection that is being added</param>
        public void AddConnection(LobbyConnection connection)
        {
            if (this.LobbyStarted)
            {
                return;
            }
            this.connections.Add(connection);
            if (this.connections.Count == this.size)
            {
                Thread thread = new(this.StartLobby);
                thread.Start();
            }
        }

        public string SerializedLobbyNames()
        {
            return JsonConvert.SerializeObject(this.connections.Select(connection => connection.name).ToList());
        }

        private void SendDisconnectMessages(LobbyConnection disconnectedPlayers)
        {
            this.connections.ForEach(description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.Disconnect, disconnectedPlayers.uuid));
        }

        private void HandleCommand(NetworkCommand? command, string uuid)
        {
            if (command == null)
            {
                return;
            }
            Logger.Log($"Received command [{command}]");
            switch (command.opCode)
            {
                case (int)NetworkInstruction.NetworkAttribute:
                    this.playtable.networkAttributeFactory.HandleNetworkedAttribute(command.instruction);
                    break;
                case (int)NetworkInstruction.SpecialAction:
                    this.playtable.HandleSpecialAction(command.instruction,uuid);
                    break;
                default:
                    // Ignore!
                    break;
            }
        }
    }
}
