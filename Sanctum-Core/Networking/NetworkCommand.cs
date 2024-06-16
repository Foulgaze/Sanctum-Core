using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class NetworkCommand
    {
        public readonly string UUID;
        public readonly int opCode;
        public readonly string instruction;
        public NetworkCommand(string UUID, int opCode, string instruction)
        {
            this.UUID = UUID;
            this.opCode = opCode;
            this.instruction = instruction;
        }
    }
}
