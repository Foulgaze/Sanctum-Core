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

        public void ParseSocketData(StringBuilder messageBuffer)
        {
            while (true)
            {
                if (messageBuffer.Length < messageLengthLength)
                {
                    return;
                }

                string messageLength = messageBuffer.ToString(0, 4);

                if (!int.TryParse(messageLength, out int messageLengthRemaining))
                {
                    // Handle parse error if necessary
                    return;
                }

                if (messageLengthRemaining > messageBuffer.Length - 4)
                {
                    return;
                }

                string currentCommand = messageBuffer.ToString(messageLengthLength, messageLengthRemaining);
                _ = messageBuffer.Remove(0, messageLengthLength + messageLengthRemaining);
                this.ParseCommand(currentCommand);
            }
        }
        private void ParseCommand(string completeMessage)
        {
            // Parsing recieved message into UUID, opCode, and Content

            int breakPos = completeMessage.IndexOf("|");
            string msgUUID = completeMessage[..breakPos];
            int opCode = int.Parse(completeMessage.Substring(breakPos + 1, 2));
            string instruction = completeMessage[(breakPos + 4)..];

            if (!Enum.IsDefined(typeof(NetworkInstruction), opCode))
            {
                // LOG THIS
                return;
            }
            this.networkInstructionEvents[(NetworkInstruction)opCode](instruction, new PropertyChangedEventArgs("NetworkEvent"));

        }
    }
}
