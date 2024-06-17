using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core 
{
    public enum NetworkInstruction
    {
        CreateLobby, JoinLobby, PlayersInLobby, InvalidCommand
    }
    public class Server
    {
        private readonly TcpListener _listener;
        public const int portNumber = 51522; // Change to ENV
        private readonly List<Lobby> _lobbies = new();
        private const int bufferSize = 4096;
        public Server()
        {
            this._listener = new(IPAddress.Any, portNumber);
        }

        public void StartListening()
        {
            this._listener.Start();
            while (true)
            {
                TcpClient client = this._listener.AcceptTcpClient();
                this.HandleClient(client);
                return;
                Thread thread = new(() => this.HandleClient(client));
                thread.Start();
            }
        }
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(stream,new StringBuilder(), bufferSize);
            if(command is null)
            {
                client.Close();
                return;
            }
            this.HandleCommand(command, client);
            client.Close();
        }

        private void HandleCommand(NetworkCommand networkCommand, TcpClient client)
        {
            switch (networkCommand.opCode)
            {
                case (int)NetworkInstruction.CreateLobby:
                    this.CreateLobby(networkCommand, client);
                    return;
                case (int)NetworkInstruction.JoinLobby:
                    this.AddToLobby(networkCommand, client);
                    return;
                default:
                    return;
            }
        }


        private string GenerateLobbyCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
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

        private void CreateLobby(NetworkCommand networkCommand, TcpClient client)
        {
            string[] data = networkCommand.instruction.Split('|');
            if(data.Length != 2)
            {
                return; // Log this
            }
            if (!int.TryParse(data[0], out int playerCount))
            {
                return; // Log this
            }
            Lobby newLobby = new(playerCount, this.GenerateLobbyCode());
            string clientUUID = Guid.NewGuid().ToString();
            Server.SendMessage(client.GetStream(), NetworkInstruction.CreateLobby, $"{clientUUID}|{newLobby.lobbyCode}");
            newLobby.players.Add(new PlayerDescription(clientUUID, data[1], client));
            this._lobbies.Add(newLobby);
            /*newLobby.StartLobby();*/
        }
        private void AddToLobby(NetworkCommand networkCommand, TcpClient client)
        {
            string[] data = networkCommand.instruction.Split('|');
            if(data.Length != 2)
            {
                Server.SendMessage(client.GetStream(), NetworkInstruction.InvalidCommand, "Need to include Name and Lobby code");
                return; // Log this
            }
            Lobby? lobby = this._lobbies.Where(lobby => lobby.lobbyCode == data[0]).FirstOrDefault();
            if(lobby == null)
            {
                Server.SendMessage(client.GetStream(), NetworkInstruction.InvalidCommand, "Invalid lobby code");
                return;
            }
            string clientUUID = Guid.NewGuid().ToString();
            lobby.players.Add(new PlayerDescription(data[1],clientUUID , client));
            foreach (PlayerDescription playerDescription in lobby.players)
            {
                Server.SendMessage(client.GetStream(), NetworkInstruction.PlayersInLobby, JsonConvert.SerializeObject(lobby.players.Select(player => player.name)));
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

        public static void SendMessage(NetworkStream stream, NetworkInstruction networkInstruction,string payload)
        {
            string message = $"{((int)networkInstruction).ToString($"D{NetworkCommandManager.opCodeLength}")}|{payload}";
            byte[] data = Encoding.UTF8.GetBytes(Server.AddMessageSize(message));
            stream.Write(data, 0, data.Length);
        }
    }
}
