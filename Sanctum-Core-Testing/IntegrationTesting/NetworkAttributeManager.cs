
using Sanctum_Core;

namespace Sanctum_Core_Testing
{
    public class NetworkAttributeManager
    {
        private readonly List<PlayerDescription> _playerDescriptions = new();
        public List<string> networkAttributes = new();
        public NetworkAttributeManager(List<PlayerDescription> playerDescriptions) 
        {
            this._playerDescriptions = playerDescriptions;
        }

        public void ReadPlayerData(int instructionCount)
        {
            for(int i = 0; i < instructionCount; i++)
            {
                foreach (PlayerDescription description in this._playerDescriptions)
                {
                    NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(description.client.GetStream(), description.buffer, Server.bufferSize, timeout:5000);
                    this.HandleCommand(command);
                    /*if(command != null)
                    {
                        Console.WriteLine($"Instruction - {command.instruction}");
                    }*/
                }
            }
        }

        private void HandleCommand(NetworkCommand? command)
        {
            if (command == null)
            {
                return;
            }
            this.networkAttributes.Add(command.instruction);
        }
    }
}
