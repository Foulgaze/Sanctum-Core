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
        public NetworkCommandHandler()
        {
            foreach (NetworkInstruction instruction in Enum.GetValues(typeof(NetworkInstruction)))
            {
                this.networkInstructionEvents[instruction] = delegate { };
            }
        }

        public string ParseSocketData(string messageBuffer)
        {
            while (true)
            {
                string messageLength = "";
                int i = 0;
                while (messageLength.Length != 4)
                {
                    if (i >= messageBuffer.Length)
                    {
                        return messageBuffer;
                    }

                    messageLength = messageLength + messageBuffer[i++];
                }

                int messageLengthRemaining = int.Parse(messageLength);
                if (messageLengthRemaining > messageBuffer.Length - 4)
                {
                    return messageBuffer;
                }

                string currentCommand = messageBuffer.Substring(i, messageLengthRemaining);
                this.ParseCommand(currentCommand);
                messageBuffer = messageBuffer[(i + messageLengthRemaining)..];
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
