using Sanctum_Core_Logger;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core_Server
{
    public class LobbyConnection
    {
        public readonly string name;
        public readonly string uuid;
        private readonly TcpClient client;
        private readonly StringBuilder buffer = new();
        public NetworkStream stream => this.client.GetStream();
        private readonly int bufferSize;
        public bool IsNetworkStreamClosed = false;
        public LobbyConnection(string name, string uuid, TcpClient client, int bufferSize = 4096)
        {
            this.name = name;
            this.uuid = uuid;
            this.client = client;
            this.bufferSize = bufferSize;
        }

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
