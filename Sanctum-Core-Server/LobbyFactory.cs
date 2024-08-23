using Sanctum_Core_Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Server
{
    public class LobbyFactory
    {
        private readonly ConcurrentDictionary<string,Lobby> lobbies = new();
        private readonly string lobbyCodeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly int lobbyCodeLength;
        public LobbyFactory(int lobbyCodeLength) 
        {
            this.lobbyCodeLength = lobbyCodeLength;
        }

        /// <summary>
        /// Creates a lobby
        /// </summary>
        /// <param name="lobbySize">Size of lobby created</param>
        /// <param name="connection">New connection for lobby</param>
        public void CreateLobby(int lobbySize,string username, string uuid, TcpClient client)
        {
            LobbyConnection connection = new(username,uuid,client);
            Lobby newLobby = new(lobbySize, this.GenerateUniqueLobbyCode());
            newLobby.OnLobbyPlayersChanged += this.SendPlayersInLobby;
            newLobby.OnLobbyClosed += this.RemoveLobby;
            this.lobbies[newLobby.code] = newLobby;
            Server.SendMessage(connection.stream, NetworkInstruction.CreateLobby, $"{uuid}|{newLobby.code}");
            if(!connection.Connected)
            {
                return;
            }
            newLobby.AddConnection(connection);
        }

        /// <summary>
        /// Insert a connection into a lobby based on code
        /// </summary>
        /// <param name="lobbyCode">The code of the lobby the connection should be inserted into</param>
        /// <param name="connection">The connection to be inserted</param>
        /// <returns></returns>
        public bool InsertConnectionIntoLobby(string lobbyCode,string username, string uuid, TcpClient client)
        {
            LobbyConnection connection = new(username, uuid, client);
            if(!this.lobbies.TryGetValue(lobbyCode, out Lobby? matchingLobby))
            {
                Console.WriteLine($"Could not find lobby of code - {lobbyCode}");
                return false;
            }
            Server.SendMessage(connection.stream, NetworkInstruction.JoinLobby, $"{uuid}|{matchingLobby.size}"); 
            if(!connection.Connected)
            {
                return false;
            }
            matchingLobby.AddConnection(connection);
            return true;
        }

        /// <summary>
        /// Removes a lobby from the list
        /// </summary>
        /// <param name="lobby">Lobby to remove</param>
        public void RemoveLobby(Lobby lobby)
        {
            _ = this.lobbies.TryRemove(lobby.code, out Lobby? _);
        }

        public void CheckForDeadLobbies(double allowedIdleTime)
        {
            DateTime currentTime = DateTime.Now;
            List<Lobby> currentLobbies = this.lobbies.Values.Where(lobby => !lobby.LobbyStarted && lobby.CheckLobbyTimeout(currentTime, allowedIdleTime)).ToList();
            currentLobbies.ForEach(lobby =>
            {
                Logger.Log($"Removing lobby {lobby.code} due to idle"); 
                _ = this.lobbies.TryRemove(lobby.code, out _);
            });
        }

        private string GenerateUniqueLobbyCode()
        {
            string newCode;
            do
            {
                char[] stringChars = new char[this.lobbyCodeLength];
                Random random = new();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = this.lobbyCodeCharacters[random.Next(this.lobbyCodeCharacters.Length)];
                }

                newCode = new(stringChars);
            } while (this.lobbies.ContainsKey(newCode));

            return newCode;
        }

        private void SendPlayersInLobby(Lobby lobby)
        {
            string serializedLobby = lobby.SerializedLobbyNames();
            lobby.SendMessageToAllPlayers(NetworkInstruction.PlayersInLobby, serializedLobby);
        }
    }
}
