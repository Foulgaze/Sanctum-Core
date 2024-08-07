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

        /// <summary>
        /// Parses the socket data from the provided message buffer and extracts the next command.
        /// </summary>
        /// <param name="messageBuffer">The buffer containing the incoming message data.</param>
        /// <returns>The parsed command as a string, or null if there is not enough data.</returns>
        /// <exception cref="Exception">Thrown when there is a flaw in the message format.</exception>
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

        /// <summary>
        /// Reads the next network command from the specified stream, populating the buffer as necessary.
        /// </summary>
        /// <param name="stream">The stream from which to read the network data.</param>
        /// <param name="buffer">The buffer used to temporarily store incoming data.</param>
        /// <param name="bufferSize">The size of the buffer for reading data from the stream.</param>
        /// <param name="readUntilData">Indicates whether to continue reading until a valid command is received.</param>
        /// <param name="timeout">The maximum time (in milliseconds) to wait for a valid command before timing out.</param>
        /// <returns>The next <see cref="NetworkCommand"/> if available, or null if no command can be read.</returns>
        public static NetworkCommand? GetNextNetworkCommand(Stream stream, StringBuilder buffer, int bufferSize, bool readUntilData = true, int? timeout = null)
        {
            NetworkCommand? networkCommand;
            DateTime startTime = DateTime.UtcNow;

            do
            {
                // Check if the timeout has been reached
                if (timeout != null && (DateTime.UtcNow - startTime).TotalMilliseconds >= timeout)
                {
                    // Log timeout event here if needed
                    return null;
                }

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
                    // Log this exception if necessary.
                    return null;
                }

                if (!readUntilData)
                {
                    return null;
                }
            } while (true);
        }

        /// <summary>
        /// Parses a network command from the given command string.
        /// </summary>
        /// <param name="command">The command string to parse.</param>
        /// <returns>A <see cref="NetworkCommand"/> object if parsing was successful; otherwise, null.</returns>
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
