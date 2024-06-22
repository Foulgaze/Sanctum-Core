using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core
{
    public enum NetworkInstruction
    {
        CreateLobby, JoinLobby, PlayersInLobby, InvalidCommand, LobbyDescription, StartGame, NetworkAttribute, BoardUpdate
    }

    public class Server
    {
        private readonly TcpListener _listener;
        private readonly List<Lobby> _lobbies = new();
        public int portNumber = 51522; // Change to ENV
        public const int bufferSize = 4096;
        public const int lobbyCodeLength = 4;

        public Server(int portNumber = 51522)
        {
            this.portNumber = portNumber;
            this._listener = new TcpListener(IPAddress.Any, portNumber);
        }

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
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream, new StringBuilder(), bufferSize);
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
                    this.AddToLobby(networkCommand, client);
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
            newLobby.concurrentPlayers.Add(new PlayerDescription(data[1], clientUUID, client));
            this._lobbies.Add(newLobby);

            if (newLobby.concurrentPlayers.Count == newLobby.size)
            {
                Thread thread = new(newLobby.StartLobby);
                thread.Start();
                
            }
        }
        //  Format should be [lobbyCode|name]
        private void AddToLobby(NetworkCommand networkCommand, TcpClient client)
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
            lobby.concurrentPlayers.Add(new PlayerDescription(data[1], clientUUID, client));

            List<string> playerNames = lobby.concurrentPlayers.Select(p => p.name).ToList();

            if (lobby.concurrentPlayers.Count == lobby.size)
            {
                Thread thread = new(lobby.StartLobby);
                thread.Start();
                return;
            }

            foreach (PlayerDescription player in lobby.concurrentPlayers)
            {
                SendMessage(player.client.GetStream(), NetworkInstruction.PlayersInLobby, JsonConvert.SerializeObject(playerNames));
            }
        }

        private static string AddMessageSize(string message)
        {
            string msgByteSize = message.Length.ToString().PadLeft(4, '0');
            return msgByteSize + message;
        }

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
