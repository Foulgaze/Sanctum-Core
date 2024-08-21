using Newtonsoft.Json;
using System.Text;

namespace Sanctum_Core_Server
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
