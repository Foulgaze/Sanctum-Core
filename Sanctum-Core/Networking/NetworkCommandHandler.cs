using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core.Networking
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.ComponentModel;


    public class NetworkCommandHandler
    {
        public Dictionary<NetworkInstruction, PropertyChangedEventHandler> networkInstructionEvents = new Dictionary<NetworkInstruction, PropertyChangedEventHandler>();
        public NetworkCommandHandler()
        {
            foreach (NetworkInstruction instruction in Enum.GetValues(typeof(NetworkInstruction)))
            {
                networkInstructionEvents[instruction] = delegate { };
            }
        }

        public string ParseSocketData(string messageBuffer)
        {
            while (true)
            {
                string messageLength = "";
                int messageLengthRemaining = 0;
                int i = 0;
                while (messageLength.Length != 4)
                {
                    if (i >= messageBuffer.Length)
                    {
                        return messageBuffer;
                    }

                    messageLength = messageLength + messageBuffer[i++];
                }

                messageLengthRemaining = Int32.Parse(messageLength);


                string currentCommand = "";
                if (messageLengthRemaining > messageBuffer.Length - 4)
                {
                    return messageBuffer;
                }

                currentCommand = messageBuffer.Substring(i, messageLengthRemaining);
                ParseCommand(currentCommand);
                messageBuffer = messageBuffer.Substring(i + messageLengthRemaining);
            }
        }
        private void ParseCommand(string completeMessage)
        {
            // Parsing recieved message into UUID, opCode, and Content

            int breakPos = completeMessage.IndexOf("|");
            string msgUUID = completeMessage.Substring(0, breakPos);
            int opCode = Int32.Parse(completeMessage.Substring(breakPos + 1, 2));
            string instruction = completeMessage.Substring(breakPos + 4);

            if (!Enum.IsDefined(typeof(NetworkInstruction), opCode))
            {
                // LOG THIS
                return;
            }
            networkInstructionEvents[(NetworkInstruction)opCode](instruction, new PropertyChangedEventArgs("NetworkEvent"));

        }
    }

}
