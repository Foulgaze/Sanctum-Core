namespace Sanctum_Core
{
    public class NetworkCommand
    {
        public readonly int opCode;
        public readonly string instruction;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCommand"/> class with the specified operation code and instruction.
        /// </summary>
        /// <param name="opCode">The operation code that identifies the command type.</param>
        /// <param name="instruction">The instruction associated with the network command.</param>
        public NetworkCommand(int opCode, string instruction)
        {
            this.opCode = opCode;
            this.instruction = instruction;
        }

        public override string ToString()
        {
            return $"{(NetworkInstruction)this.opCode} - {this.instruction}";
        }
    }
}
