using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core
{
    public enum NetworkInstruction
    {
        CreateLobby, JoinLobby, PlayersInLobby, InvalidCommand, LobbyDescription, StartGame, NetworkAttribute, BoardUpdate, CardCreation, SpecialAction, Disconnect
    }

    public class Server
    {
        private readonly TcpListener _listener;
        private readonly List<Lobby> _lobbies = new();
        public int portNumber = 51522; // Change to ENV
        public const int bufferSize = 4096;
        public const int lobbyCodeLength = 4;
        private readonly LobbyFactory lobbyFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="portNumber">The port number on which the server will listen for incoming connections. Default is 51522.</param>
        public Server(int portNumber = 51522, int lobbyCodeLength = 4)
        {
            this.portNumber = portNumber;
            this.lobbyFactory = new(lobbyCodeLength);
            this.lobbyFactory.notifyLobbyPlayer += Server.SendMessage;
            this._listener = new TcpListener(IPAddress.Any, portNumber);
        }

        /// <summary>
        /// Starts the TCP listener to accept incoming client connections.
        /// </summary>
        public void StartListening()
        {
            Logger.Log($"Server is now listening on port {this.portNumber}");
            this._listener.Start();

            while (true)
            {
                TcpClient client = this._listener.AcceptTcpClient();
                Thread thread = new(() => this.HandleClient(client));
                thread.Start();
            }
        }



        private bool HandleClient(TcpClient client)
        {
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(client.GetStream(), new StringBuilder(), bufferSize, timeout: 10000);
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
            if (!int.TryParse(data[0], out int playerCount))
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
                Logger.LogError("Need to include Name and Lobby code");
                return false;
            }
            string clientUUID = Guid.NewGuid().ToString();
            bool insertedIntoLobby = this.lobbyFactory.InsertConnectionIntoLobby(data[0], data[1], clientUUID, client);
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
            stream.Write(data, 0, data.Length);
        }

        private static void SendInvalidCommand(TcpClient client, string message)
        {
            SendMessage(client.GetStream(), NetworkInstruction.InvalidCommand, message);
        }
    }
}
