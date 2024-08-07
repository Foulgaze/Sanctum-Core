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

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="portNumber">The port number on which the server will listen for incoming connections. Default is 51522.</param>
        public Server(int portNumber = 51522)
        {
            this.portNumber = portNumber;
            this._listener = new TcpListener(IPAddress.Any, portNumber);
        }

        /// <summary>
        /// Starts the TCP listener to accept incoming client connections.
        /// </summary>
        public void StartListening()
        {

            this._listener.Start();

            while (true)
            {
                TcpClient client = this._listener.AcceptTcpClient();
                Thread thread = new(() => this.HandleClient(client));
                thread.Start();
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), bufferSize, timeout: 10000);
            if (command == null)
            {
                client.Close();
                return;
            }
            this.HandleCommand(command, client);
        }

        private void HandleCommand(NetworkCommand networkCommand, TcpClient client)
        {
            switch ((NetworkInstruction)networkCommand.opCode)
            {
                case NetworkInstruction.CreateLobby:
                    this.CreateLobby(networkCommand, client);
                    break;
                case NetworkInstruction.JoinLobby:
                    this.AddPlayerToLobby(networkCommand, client);
                    break;
                default:
                    SendInvalidCommand(client, "Invalid command");
                    break;
            }
        }

        private string GenerateLobbyCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string finalString;

            do
            {
                char[] stringChars = new char[lobbyCodeLength];
                Random random = new();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                finalString = new(stringChars);
            } while (this._lobbies.Any(lobby => lobby.code == finalString));

            return finalString;
        }

        private void CreateLobby(NetworkCommand networkCommand, TcpClient client)
        {
            string[] data = networkCommand.instruction.Split('|');
            if (data.Length != 2)
            {
                SendInvalidCommand(client, "Must include name and lobby code");
                return;
            }

            if (!int.TryParse(data[0], out int playerCount))
            {
                SendInvalidCommand(client, "Invalid lobby count");
                return;
            }

            Lobby newLobby = new(playerCount, this.GenerateLobbyCode());
            string clientUUID = Guid.NewGuid().ToString();
            SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"{clientUUID}|{newLobby.code}");
            this._lobbies.Add(newLobby);
            _ = this.AddPlayerAndCheckIfLobbyIsFull(newLobby, new PlayerDescription(data[1], clientUUID, client));
        }
        
        private void AddPlayerToLobby(NetworkCommand networkCommand, TcpClient client)
        {
            string[] data = networkCommand.instruction.Split('|');
            if (data.Length != 2)
            {
                SendInvalidCommand(client, "Need to include Name and Lobby code");
                return;
            }

            Lobby? lobby = this._lobbies.FirstOrDefault(l => l.code == data[0]);
            if (lobby == null)
            {
                SendInvalidCommand(client, "Invalid lobby code");
                return;
            }

            string clientUUID = Guid.NewGuid().ToString();
            SendMessage(client.GetStream(), NetworkInstruction.JoinLobby, clientUUID);

            if (!this.AddPlayerAndCheckIfLobbyIsFull(lobby, new PlayerDescription(data[1], clientUUID, client)))
            {
                this.NotifyPlayersInLobby(lobby);
            }
        }

        private void NotifyPlayersInLobby(Lobby lobby)
        {
            List<string> playerNames = lobby.concurrentPlayers.Select(p => p.name).ToList();

            foreach (PlayerDescription player in lobby.concurrentPlayers)
            {
                SendMessage(player.client.GetStream(), NetworkInstruction.PlayersInLobby, JsonConvert.SerializeObject(playerNames));
            }
        }

        private bool AddPlayerAndCheckIfLobbyIsFull(Lobby lobby, PlayerDescription player)
        {
            lobby.concurrentPlayers.Add(player);

            if (lobby.concurrentPlayers.Count < lobby.size)
            {
                return false;
            }

            // Start the lobby in a new thread if it's full
            Thread thread = new(lobby.StartLobby);
            thread.Start();
            return true;
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
