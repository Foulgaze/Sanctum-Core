using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public static class NetworkCommandManager
    {

        public const int messageLengthLength = 4;
        public const int opCodeLength = 3;


        private static string? ParseSocketData(StringBuilder messageBuffer)
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

        public static NetworkCommand? GetNextNetworkCommand(NetworkStream stream, StringBuilder buffer, int bufferSize)
        {
            NetworkCommand? networkCommand;
            do
            {
                NetworkReceiver.ReadSocketData(stream, bufferSize, buffer);
                try
                {
                    string? rawCommand = NetworkCommandManager.ParseSocketData(buffer);
                    networkCommand = NetworkCommandManager.ParseCommand(rawCommand);
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
            } while(true);
        }
        private static NetworkCommand? ParseCommand(string? command)
        {
            if (command == null)
            {
                return null;
            }
            // Parsing recieved message into UUID, opCode, and Content
            string[] data = command.Split('|');
            if(data.Length < 2)
            {
                return null;
            }
            if (!int.TryParse(data[0],out int opCode ))
            {
                return null;
            }
            return new NetworkCommand(opCode, string.Join('|', data[1..]));

        }
    }
}
