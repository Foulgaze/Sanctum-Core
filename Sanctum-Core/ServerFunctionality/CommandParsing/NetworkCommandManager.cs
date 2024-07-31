using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public static class NetworkCommandManager
    {

        public const int messageLengthLength = 4;
        public const int opCodeLength = 3;


        public static string? ParseSocketData(StringBuilder messageBuffer)
        {
            while (true)
            {
                if (messageBuffer.Length < messageLengthLength)
                {
                    return null;
                }

                string messageLength = messageBuffer.ToString(0, 4);

                if (!int.TryParse(messageLength, out int messageLengthRemaining))
                {
                    // Handle parse error if necessary
                    throw new Exception("Flawed message sent. Breaking off connection");
                }

                if (messageLengthRemaining > messageBuffer.Length - 4)
                {
                    return null;
                }

                string currentCommand = messageBuffer.ToString(messageLengthLength, messageLengthRemaining);
                _ = messageBuffer.Remove(0, messageLengthLength + messageLengthRemaining);
                return currentCommand;
            }
        }

        public static NetworkCommand? GetNextNetworkCommand(Stream stream, StringBuilder buffer, int bufferSize, bool readUntilData = true)
        {
            NetworkCommand? networkCommand;
            do
            {
                NetworkReceiver.ReadSocketData(stream, bufferSize, buffer);
                try
                {
                    string? rawCommand = ParseSocketData(buffer);
                    networkCommand = ParseCommand(rawCommand);
                    if (networkCommand != null)
                    {
                        return networkCommand;
                    }
                }
                catch
                {
                    // Log this.
                    return null;
                }
                if (!readUntilData)
                {
                    return null;
                }
            } while (true);
        }
        public static NetworkCommand? ParseCommand(string? command)
        {
            if (command == null)
            {
                return null;
            }
            try
            {
                NetworkCommand? networkCommand = JsonConvert.DeserializeObject<NetworkCommand>(command);
                return networkCommand;
            }
            catch
            {
                return null;
            }
        }
    }
}
