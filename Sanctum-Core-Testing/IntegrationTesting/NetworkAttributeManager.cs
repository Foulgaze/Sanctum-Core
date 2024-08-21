﻿
using Sanctum_Core_Logger;
using Sanctum_Core_Server;

namespace Sanctum_Core_Testing
{
    public class NetworkAttributeManager
    {
        private readonly List<LobbyConnection> _playerDescriptions = new();
        public List<string> networkAttributes = new();
        public NetworkAttributeManager(List<LobbyConnection> playerDescriptions) 
        {
            this._playerDescriptions = playerDescriptions;
        }

        public void ReadPlayerData(int instructionCount)
        {
            for(int i = 0; i < instructionCount; i++)
            {
                Logger.Log($"Loop - {i}");
                foreach (LobbyConnection description in this._playerDescriptions)
                {
                    NetworkCommand? command = description.GetNetworkCommand(timeout: 5000);
                    this.HandleCommand(command);
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
