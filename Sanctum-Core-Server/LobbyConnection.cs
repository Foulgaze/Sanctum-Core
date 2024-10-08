﻿using Sanctum_Core_Logger;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core_Server
{
    public class LobbyConnection
    {
        public readonly string name;
        public readonly string uuid;
        public readonly TcpClient client;
        private readonly StringBuilder buffer = new();
        public NetworkStream stream => this.client.GetStream();
        public bool Connected => this.client.Connected;
        private readonly int bufferSize;
        public bool IsNetworkStreamClosed = false;

        /// <summary>
        /// This class represents the state of a player being in a lobby without the playtable having been created.
        /// </summary>
        /// <param name="name"> Name of connection</param>
        /// <param name="uuid"> UUID of connection</param>
        /// <param name="client"> TCPClient of connection </param>
        /// <param name="bufferSize"> Buffersize of client buffer</param>
        public LobbyConnection(string name, string uuid, TcpClient client, int bufferSize = 4096)
        {
            this.name = name;
            this.uuid = uuid;
            this.client = client;
            this.bufferSize = bufferSize;
        }

        /// <summary>
        /// Gets the next network command from the client buffer
        /// </summary>
        /// <param name="readUntilData"> Should the connection block until there is data?</param>
        /// <param name="timeout"> Timeout after reading for no data</param>
        /// <returns> The next network command from buffer if exists, else null</returns>
        public NetworkCommand? GetNetworkCommand(bool readUntilData = true, int? timeout = null)
        {
            NetworkStream stream = this.client.GetStream();
            stream.ReadTimeout = timeout ?? Timeout.Infinite;

            do
            {
                bool readSucceeded = NetworkReceiver.ReadSocketData(stream, this.bufferSize, this.buffer, out bool timedOut);
                if (!readSucceeded)
                {
                    this.HandleStreamClosure();
                    return null;
                }

                if (timedOut)
                {
                    return null;
                }

                try
                {
                    string? rawCommand = NetworkCommandManager.ParseSocketData(this.buffer);
                    NetworkCommand? networkCommand = NetworkCommandManager.ParseCommand(rawCommand);

                    if (networkCommand != null)
                    {
                        return networkCommand;
                    }
                }
                catch (Exception e)
                {
                    this.HandleStreamError(e);
                    return null;
                }
                if(!readUntilData)
                {
                    return null;
                }
            } while (true);
        }

        private void HandleStreamClosure()
        {
            this.IsNetworkStreamClosed = true;
            this.client.Close();
        }

        private void HandleStreamError(Exception e)
        {
            this.IsNetworkStreamClosed = true;
            this.client.Close();
            Logger.LogError($"Error parsing network data - {e}");
        }
    }
}
