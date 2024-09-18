using Newtonsoft.Json;
using Sanctum_Core;
using Sanctum_Core_Logger;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Sanctum_Core_Server
{
    public class Lobby
    {
        public event Action<Lobby> OnLobbyClosed = delegate { };
        private readonly Playtable playtable;
        public readonly int size;
        public readonly string code;
        public List<LobbyConnection> connections = new();
        public event Action<Lobby> OnLobbyPlayersChanged = delegate { };
        public DateTime timeSinceLastInteracted { get; private set; }

        private bool closeLobby = false;
        private readonly CountdownTimer disconnectedPlayerCheck;

        public bool LobbyStarted { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lobby"/> class with the specified size and code.
        /// </summary>
        /// <param name="lobbySize">The maximum number of players allowed in the lobby.</param>
        /// <param name="lobbyCode">The unique code identifying the lobby.</param>
        /// <remarks>
        /// The constructor sets up the playtable for the lobby, loading the card and token data from CSV files located in the designated assets directory.
        /// </remarks>
        public Lobby(int lobbySize, string lobbyCode, double disconnectedPlayerCheckTime = 0.1)
        {
            this.size = lobbySize;
            this.code = lobbyCode;
            string path = Path.GetFullPath(@"..\..\..\..\Sanctum-Core\Assets\");
            this.playtable = new Playtable(lobbySize, $"{path}cards.csv", $"{path}tokens.csv");
            this.timeSinceLastInteracted = DateTime.Now;
            this.disconnectedPlayerCheck = new(disconnectedPlayerCheckTime);
        }

        /// <summary>
        /// Sends a message to all players
        /// </summary>
        /// <param name="instruction">Network Instruction enum to send</param>
        /// <param name="payload">Paylod of instruction</param>
        /// <param name="specificConnection">Only send to specific person</param>
        public void SendMessageToAllPlayers(NetworkInstruction instruction, string payload, LobbyConnection? specificConnection = null)
        {
            this.timeSinceLastInteracted = DateTime.Now;
            bool removedPlayers = false;
            for (int i = this.connections.Count - 1; i > -1; --i)
            {
                LobbyConnection connection = this.connections[i];
                if (specificConnection != null && connection != specificConnection)
                {
                    continue;
                }
                Server.SendMessage(connection.stream, instruction, payload);
                if (!connection.Connected)
                {
                    removedPlayers = true;
                    this.connections.RemoveAt(i);
                }
            }
            if (removedPlayers)
            {
                OnLobbyPlayersChanged(this);
            }
        }

        /// <summary>
        /// Starts the lobby by initializing the game and continuously listening for player commands.
        /// </summary>
        public void StartLobby()
        {
            this.InitGame();
            this.RunLobbyLoop();
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
            this.timeSinceLastInteracted = DateTime.Now;
            if (this.connections.Count == this.size)
            {
                this.LobbyStarted = true;
                Thread thread = new(this.StartLobby) { Name = $"Lobby - {this.code}" };
                thread.Start();
            }
            else
            {
                OnLobbyPlayersChanged(this);
            }
        }

        /// <summary>
        /// Gets the players in lobby names in a serialized form
        /// </summary>
        /// <returns></returns>
        public string SerializedLobbyNames()
        {
            return JsonConvert.SerializeObject(this.connections.Select(connection => connection.name).ToList());
        }

        /// <summary>
        /// Checks how long the lobby has been sitting without being interacted with
        /// </summary>
        /// <param name="currentTime"> The current time</param>
        /// <param name="allowedIdleTime">How long the lobby is allowed to wait</param>
        /// <returns>a bool representing if the lobby has timed out</returns>

        public bool CheckLobbyTimeout(DateTime currentTime, double allowedIdleTime)
        {
            this.closeLobby = (currentTime - this.timeSinceLastInteracted).TotalMinutes > allowedIdleTime
            return this.closeLobby;
        }

        private void NetworkAttributeChanged(NetworkAttribute attribute)
        {
            this.SendMessage(NetworkInstruction.NetworkAttribute, $"{attribute.Id}|{attribute.SerializedValue}");
        }
        private void SendMessage(NetworkInstruction instruction, string payload)
        {
            this.SendMessageToAllPlayers(instruction, payload);
        }

        private void InitGame()
        {
            this.connections.ForEach(description => this.playtable.AddPlayer(description.uuid, description.name));
            string lobbyDescription = JsonConvert.SerializeObject(this.connections.ToDictionary(player => player.uuid, player => player.name));
            this.SendMessageToAllPlayers(NetworkInstruction.StartGame, lobbyDescription);
            this.playtable.networkAttributeFactory.attributeValueChanged  += this.NetworkAttributeChanged;
        }

        /// <summary>
        /// Continuously listens for player commands and checks for disconnected players.
        /// </summary>
        private void RunLobbyLoop()
        {
            while (true)
            {
                bool checkForDisconnectedPlayers = this.disconnectedPlayerCheck.HasTimerPassed();

                this.ProcessConnections(checkForDisconnectedPlayers);

                if (this.connections.Count == 0 || this.closeLobby)
                {
                    this.CloseLobby();
                    break;
                }
            }
        }

        /// <summary>
        /// Processes all player connections, handles commands, and checks connectivity.
        /// </summary>
        /// <param name="checkForDisconnectedPlayers">Flag to indicate if connectivity should be checked.</param>
        private void ProcessConnections(bool checkForDisconnectedPlayers)
        {
            for (int i = this.connections.Count - 1; i >= 0; --i)
            {
                LobbyConnection connection = this.connections[i];

                if(checkForDisconnectedPlayers)
                {
                    this.CheckForConnectivity(connection);
                }

                if (!connection.Connected)
                {
                    this.connections.RemoveAt(i);
                    continue;
                }

                this.HandleConnectionCommands(connection);
            }
        }

        /// <summary>
        /// Handles the network commands and connectivity check for a single connection.
        /// </summary>
        /// <param name="connection">The lobby connection to process.</param>
        private void HandleConnectionCommands(LobbyConnection connection)
        {
            NetworkCommand? command = connection.GetNetworkCommand(readUntilData: false);
            this.HandleCommand(command);
        }

        /// <summary>
        /// Closes the lobby and logs the closure.
        /// </summary>
        private void CloseLobby()
        {
            OnLobbyClosed?.Invoke(this);
            Logger.Log($"Closing lobby {this.code}");
        }

        private void CheckForConnectivity(LobbyConnection connection)
        {
            try
            {
                connection.stream.Write(new byte[0], 0, 0);
            }
            catch
            {
                Logger.LogError($"Player {connection.name} has disconnected");
            }
        }
        private void HandleCommand(NetworkCommand? command)
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
                default:
                    // Ignore!
                    break;
            }
        }
    }
}
