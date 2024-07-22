using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    NetworkCommand? command = NetworkCommandManager.GetNextNetworkCommand(description.client.GetStream(), description.buffer, Server.bufferSize);
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
