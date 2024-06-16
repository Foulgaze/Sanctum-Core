using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core 
{
    public enum NetworkInstruction
    {
        CreateLobby, PlayersInLobby
    }
    public class Server
    {
        private enum ConnectType { CreateLobby, JoinLobby };
        private readonly TcpListener _listener;
        private const int _portNumber = 51522; // Change to ENV
        private readonly List<Lobby> _lobbies;
        private const int bufferSize = 4096;
        public Server()
        {
            this._listener = new(IPAddress.Any, _portNumber);
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
            StringBuilder messageBuffer = new();
            NetworkReceiver.ReadSocketData(stream, bufferSize, messageBuffer);
            NetworkCommand? networkCommand;
            while (true)
            {
                try
                {
                    string? rawCommand = NetworkCommandHandler.ParseSocketData(messageBuffer);
                    networkCommand = NetworkCommandHandler.ParseCommand(rawCommand);
                    if(networkCommand != null)
                    {
                        break;
                    }
                }
                catch
                {
                    client.Close();
                    return;
                }
            }
            this.HandleCommand(networkCommand, stream);
            client.Close();
        }

        private void HandleCommand(NetworkCommand networkCommand, NetworkStream stream)
        {
            switch (networkCommand.opCode)
            {
                case (int)ConnectType.CreateLobby:
                    this.HandleCreateLobby(networkCommand, stream);
                    return;
                case (int)ConnectType.JoinLobby:
                    this.HandleJoinLobby(networkCommand, stream);
                    return;
                default:
                    return;
            }
        }


        private string GenerateLobbyCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string finalString;
            do
            {
                char[] stringChars = new char[4];
                Random random = new();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                finalString = new(stringChars);
            } while (this._lobbies.Where(lobby => lobby.lobbyCode == finalString).Count() != 0);
            
            return finalString;
        }

        private void HandleCreateLobby(NetworkCommand networkCommand, NetworkStream stream)
        {
            string[] data = networkCommand.instruction.Split('|');
            if(data.Length != 3)
            {
                return; // Log this
            }
            if (!int.TryParse(data[0], out int playerCount))
            {
                return; // Log this
            }
            Lobby newLobby = new(playerCount, this.GenerateLobbyCode());
            Server.SendMessage(stream, NetworkInstruction.CreateLobby, newLobby.lobbyCode);
            newLobby.players.Add(new PlayerDescription(data[1], data[2], stream));
            this._lobbies.Add(newLobby);
            newLobby.StartLobby();
        }
        private void HandleJoinLobby(NetworkCommand networkCommand, NetworkStream stream)
        {
            string[] data = networkCommand.instruction.Split('|');
            if(data.Length != 4)
            {
                return; // Log this
            }
            if (!int.TryParse(data[0], out int lobbyCode))
            {
                return; // Log this
            }
            Lobby? lobby = this._lobbies.Where(lobby => lobby.lobbyCode == data[1]).FirstOrDefault();
            if(lobby == null)
            {
                return;
            }
            lobby.players.Add(new PlayerDescription(data[2], data[3], stream));
            List<string> playerNames = new(lobby.players.Select(player => player.name));
            foreach (PlayerDescription playerDescription in lobby.players)
            {
                Server.SendMessage(stream, NetworkInstruction.PlayersInLobby, JsonConvert.SerializeObject(playerNames));
            }
        }

        private static string AddMessageSize(string message)
        {
            string msgByteSize = message.Length.ToString();
            if (msgByteSize.Length > 4)
            {
                // Log Error
            }
            while (msgByteSize.Length != 4)
            {
                msgByteSize = "0" + msgByteSize;
            }
            return msgByteSize + message;
        }

        public static void SendMessage(NetworkStream stream, NetworkInstruction networkInstruction, string payload = "")
        {
            string message = $"{(int)networkInstruction:D3}|{payload}";
            byte[] data = Encoding.UTF8.GetBytes(Server.AddMessageSize(message));
            stream.Write(data, 0, data.Length);
        }
    }
}
