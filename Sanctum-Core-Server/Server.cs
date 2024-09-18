using Newtonsoft.Json;
using Sanctum_Core_Logger;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core_Server
{
    public enum NetworkInstruction
    {
        CreateLobby, JoinLobby, PlayersInLobby, InvalidCommand, LobbyDescription, StartGame, NetworkAttribute, Disconnect
    }

    public class Server
    {
        private readonly TcpListener _listener;
        public int portNumber = 51522; // Change to ENV
        public const int bufferSize = 4096;
        public const int lobbyCodeLength = 4;
        private readonly LobbyFactory lobbyFactory;
        private readonly CountdownTimer? deadLobbyClock;
        private readonly double allowedLobbyIdleTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="portNumber">The port number on which the server will listen for incoming connections. Default is 51522.</param>
        /// <param name="lobbyCodeLength">How long a lobby code is, default is 4 i.e. (A3C1)</param>
        /// <param name="deadLobbyCheckTimer">How often the server checks for and clears inactive lobbies, in minutes</param>
        /// <param name="allowedLobbyIdleDuration">How often a lobby can sit idle before it is considered dead, in minutes</param>
        public Server(int portNumber = 51522, int lobbyCodeLength = 4, double? deadLobbyCheckTimer = 5, double allowedLobbyIdleDuration = 5)
        {
            this.portNumber = portNumber;
            this.lobbyFactory = new(lobbyCodeLength);
            this._listener = new TcpListener(IPAddress.Any, portNumber);
            this.deadLobbyClock = deadLobbyCheckTimer == null ? null : new CountdownTimer((double)deadLobbyCheckTimer);
            this.allowedLobbyIdleTime = allowedLobbyIdleDuration;
        }

        /// <summary>
        /// Starts the TCP listener to accept incoming client connections.
        /// </summary>
        public void StartListening()
        {
            Logger.Log($"Server is now listening on port {this.portNumber}");
            this._listener.Start();
            Thread lobbyClean = new(this.LobbyCleaning) { Name = "Lobby Cleaning Thread"};
            lobbyClean.Start();
            while (true)
            {
                TcpClient client = this._listener.AcceptTcpClient();
                Thread thread = new(() => this.HandleClient(client)) { Name = "Handling Client Thread"};
                thread.Start();
            }
        }

        /// <summary>
        /// A continuous loop that checks for dead lobbies
        /// </summary>
        public void LobbyCleaning()
        {
            while(true)
            {
                if (this.deadLobbyClock != null && this.deadLobbyClock.HasTimerPassed())
                {
                    this.lobbyFactory.CheckForDeadLobbies(this.allowedLobbyIdleTime);
                }
            }
        }

        private bool HandleClient(TcpClient client)
        {
            LobbyConnection connection = new("", "", client);
            NetworkCommand? command = connection.GetNetworkCommand(timeout: 10000);
            if (command == null)
            {
                Console.WriteLine("Disconnecting client");
                client.Close();
                return true;
            }
            return this.HandleCommand(command, client);
        }

        private bool HandleCommand(NetworkCommand networkCommand, TcpClient client)
        {
            Logger.Log($"Server received command {networkCommand}");
            switch ((NetworkInstruction)networkCommand.opCode)
            {
                case NetworkInstruction.CreateLobby:
                    return this.CreateLobby(networkCommand, client);
                case NetworkInstruction.JoinLobby:
                    return this.AddPlayerToLobby(networkCommand, client);
                default:
                    SendInvalidCommand(client, "Invalid command");
                    return false;
            }
        }

        private bool CreateLobby(NetworkCommand networkCommand, TcpClient client)
        {
            string[] data = networkCommand.instruction.Split('|');
            if (data.Length != 2)
            {
                Logger.LogError("Must include name and lobby code");
                return false;
            }
            if (!int.TryParse(data[0], out int playerCount) || playerCount < 1)
            {
                SendInvalidCommand(client, "Invalid lobby count");
                return false;
            }
            string clientUUID = Guid.NewGuid().ToString();
            this.lobbyFactory.CreateLobby(playerCount, data[1], clientUUID, client);
            return true;
        }
        
        private bool AddPlayerToLobby(NetworkCommand networkCommand, TcpClient client)
        {
            string[] data = networkCommand.instruction.Split('|');
            if (data.Length != 2)
            {
                SendInvalidCommand(client, "Need to include Name and Lobby code");
                return false;
            }
            string clientUUID = Guid.NewGuid().ToString();
            bool insertedIntoLobby = this.lobbyFactory.InsertConnectionIntoLobby(data[0], data[1], clientUUID, client);
            if (!insertedIntoLobby)
            {
                SendInvalidCommand(client, "Invalid Code");
            }
            return insertedIntoLobby;
        }

        private static string AddMessageSize(string message)
        {
            string msgByteSize = message.Length.ToString().PadLeft(4, '0');
            return msgByteSize + message;
        }

        /// <summary>
        /// Sends a serialized network command over the specified <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="NetworkStream"/> to send the message through.</param>
        /// <param name="instruction">The <see cref="NetworkInstruction"/> that indicates the type of command being sent.</param>
        /// <param name="payload">The string payload containing additional data for the command.</param>
        public static void SendMessage(NetworkStream stream, NetworkInstruction instruction, string payload)
        {
            Logger.Log($"Sending {new NetworkCommand((int)instruction, payload)}");
            string message = JsonConvert.SerializeObject(new NetworkCommand((int)instruction, payload));
            byte[] data = Encoding.UTF8.GetBytes(AddMessageSize(message));
            try
            {
                stream.Write(data, 0, data.Length);

            }
            catch (Exception e)
            {
                Logger.Log($"Problem sending {payload}. Error - {e}");
            }
        }

        private static void SendInvalidCommand(TcpClient client, string message)
        {
            SendMessage(client.GetStream(), NetworkInstruction.InvalidCommand, message);
        }
    }
}
