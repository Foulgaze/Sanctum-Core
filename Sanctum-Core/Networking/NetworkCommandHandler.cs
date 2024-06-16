using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class NetworkCommandHandler
    {
        

        public Dictionary<NetworkInstruction, PropertyChangedEventHandler> networkInstructionEvents = new();
        public static int messageLengthLength = 4;
        public NetworkCommandHandler()
        {
            foreach (NetworkInstruction instruction in Enum.GetValues(typeof(NetworkInstruction)))
            {
                this.networkInstructionEvents[instruction] = delegate { };
            }
        }

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
        public static NetworkCommand? ParseCommand(string? command)
        {
            if (command == null)
            {
                return null;
            }
            // Parsing recieved message into UUID, opCode, and Content

            int breakPos = command.IndexOf("|");
            string msgUUID = command[..breakPos];
            if(!int.TryParse(command.Substring(breakPos + 1, 2),out int opCode ))
            {
                return null;
            }
            string instruction = command[(breakPos + 4)..];

            return new NetworkCommand(msgUUID, opCode, instruction);

        }
    }
}
