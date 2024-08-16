﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class LobbyFactory
    {
        private readonly List<Lobby> lobbies = new();
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
        public void CreateLobby(int lobbySize,LobbyConnection connection)
        {
            Lobby newLobby = new(lobbySize, this.GenerateUniqueLobbyCode());
            newLobby.OnLobbyClosed += this.RemoveLobby;
            this.lobbies.Add(newLobby);
            newLobby.AddConnection(connection);
        }

        /// <summary>
        /// Insert a connection into a lobby based on code
        /// </summary>
        /// <param name="lobbyCode">The code of the lobby the connection should be inserted into</param>
        /// <param name="connection">The connection to be inserted</param>
        /// <returns></returns>
        public bool InsertConnectionIntoLobby(string lobbyCode, LobbyConnection connection)
        {
            Lobby? relevantLobby = this.lobbies.FirstOrDefault(lobby => lobby.code == lobbyCode);
            if(relevantLobby == null)
            {
                return false;
            }
            relevantLobby.AddConnection(connection);
            return true;
        }

        public void RemoveLobby(Lobby lobby)
        {
            _ = this.lobbies.Remove(lobby);
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
            } while (this.lobbies.Any(lobby => lobby.code == newCode));

            return newCode;
        }
    }
}
