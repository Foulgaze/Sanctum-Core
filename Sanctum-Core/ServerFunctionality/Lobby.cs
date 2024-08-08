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
        private List<PlayerDescription> players = new();
        public readonly string code;
        public ConcurrentBag<PlayerDescription> concurrentPlayers = new();
        private readonly TimeChecker timeChecker = new();
        public bool GameStarted { get; set; } = false;

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

        private void NetworkAttributeChanged(object? sender, PropertyChangedEventArgs? args)
        {
            if (args == null)
            {
                Console.WriteLine($"Property changed event args is null for - {sender}");
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
                Console.WriteLine($"Sender is is null for network board change");
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
            this.GameStarted = true;  
            this.players = this.concurrentPlayers.ToList(); // Ignore concurrent players once lobby starts
            this.players.ForEach(description => this.playtable.AddPlayer(description.uuid, description.name));
            string lobbyDescription = JsonConvert.SerializeObject(this.players.ToDictionary(player => player.uuid, player => player.name));
            this.players.ForEach(description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.StartGame, lobbyDescription));
            this.playtable.networkAttributeFactory.attributeValueChanged += this.NetworkAttributeChanged;
            this.playtable.boardChanged += this.NetworkBoardChange;
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
                if (this.players.Count == 0)
                {
                    OnLobbyClosed?.Invoke(this);
                    Console.WriteLine("Closing");
                    return;
                }
                foreach (PlayerDescription playerDescription in this.players)
                {
                    NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(playerDescription.client.GetStream(), playerDescription.buffer, Server.bufferSize, false);
                    this.HandleCommand(command, playerDescription.uuid);
                }
                if (this.timeChecker.HasTimerPassed())
                {
                    List<PlayerDescription> closedConnections = this.CheckForClosedConnections();
                    this.players = this.players.Except(closedConnections).ToList();
                    closedConnections.ForEach(this.SendDisconnectMessages);
                }
            }
        }

        private void SendDisconnectMessages(PlayerDescription disconnectedPlayers)
        {
            this.players.ForEach(description => Server.SendMessage(description.client.GetStream(), NetworkInstruction.Disconnect, disconnectedPlayers.uuid));
        }

        private List<PlayerDescription> CheckForClosedConnections()
        {
            List<PlayerDescription> closedConnections = new();
            foreach (PlayerDescription description in this.players)
            {
                try
                {
                    // Attempt to read a byte from the stream
                    NetworkStream stream = description.client.GetStream();

                    // Set a read timeout (in milliseconds)
                    /*stream.ReadTimeout = 5000; // 5 seconds*/

                    byte[] buffer = new byte[1];
                    int bytesRead = stream.Read(buffer, 0, 1);
                    
                    // If no bytes were read, the connection is closed
                    if (bytesRead == 0)
                    {
                        closedConnections.Add(description);
                    }
                    else
                    {
                        _ = description.buffer.Append(System.Text.Encoding.UTF8.GetString(buffer));
                    }
                }
                catch (IOException ex)
                {
                    closedConnections.Add(description);
                    Console.WriteLine($"Client has been closed or read timed out: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            return closedConnections;
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
    }
}
