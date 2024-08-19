using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class LobbyConnection
    {
        public readonly string name;
        public readonly string uuid;
        private readonly TcpClient client;
        private readonly StringBuilder buffer = new();
        private readonly int bufferSize;
        public bool IsNetworkStreamClosed = false;
        public LobbyConnection(string name, string uuid, TcpClient client)
        {
            this.name = name;
            this.uuid = uuid;
            this.client = client;
        }

        public NetworkStream? GetStream()
        {
            NetworkStream stream;
            try
            {
                stream = this.client.GetStream();
                return stream;
            }
            catch
            {
                this.IsNetworkStreamClosed = true;
                this.client.Close();
                return null;
            }
        }

        public NetworkCommand? GetNetworkCommand(bool readUntilData = true, int? timeout = null)
        {
            NetworkStream stream;
            try
            {
                stream = this.client.GetStream();
            }
            catch
            {
                this.IsNetworkStreamClosed = true;
                this.client.Close();
                return null;
            }

            NetworkCommand? networkCommand;
            DateTime startTime = DateTime.UtcNow;
            do
            {
                // Check if the timeout has been reached
                if (timeout != null && (DateTime.UtcNow - startTime).TotalMilliseconds >= timeout)
                {
                    // Log timeout event here if needed
                    Logger.Log($"Read timed out after {timeout} seconds");
                    return null;
                }

                bool readSucceded = NetworkReceiver.ReadSocketData(stream, this.bufferSize, this.buffer);
                if(!readSucceded)
                {
                    this.IsNetworkStreamClosed = true;
                    this.client.Close();
                    return null;
                }
                try
                {
                    string? rawCommand = NetworkCommandManager.ParseSocketData(this.buffer);
                    networkCommand = NetworkCommandManager.ParseCommand(rawCommand);
                    if (networkCommand != null)
                    {
                        return networkCommand;
                    }
                }
                catch (Exception e)
                {
                    this.IsNetworkStreamClosed = true;
                    this.client.Close();
                    Logger.LogError($"Error parsing network data - {e}");
                    return null;
                }

                if (!readUntilData)
                {
                    return null;
                }
            } while (true);
        }
    }
}
