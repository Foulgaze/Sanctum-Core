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
    using System.Net.Sockets;
    using System;
    using System.ComponentModel;
    using Unity.Properties;

    public enum NetworkInstruction
    {
        NetworkAttribute, PlayerConnection
    }

    public class NetworkManager
    {


        private TcpClient client;
        private NetworkStream rwStream;
        private string messageBuffer = "";
        private int bufferSize = 4096;
        private NetworkAttributeFactory NetworkAttributeFactory;
        private NetworkCommandHandler NetworkCommandHandler;
        private bool mock;




        public NetworkManager(NetworkAttributeFactory networkAttributeFactory, bool mock = false)
        {
            this.NetworkAttributeFactory = networkAttributeFactory;
            this.NetworkAttributeFactory.attributeValueChanged += NetworkAttributeChanged;
            this.NetworkCommandHandler = new NetworkCommandHandler();
            this.mock = mock;
        }

        public int Connect(string server, string username, int port)
        {
            try
            {
                if (!mock)
                {
                    client = new TcpClient(server, port);
                    rwStream = client.GetStream();
                }
                else
                {
                    rwStream = new NetworkMock(null);
                }
                SendMessage(NetworkInstruction.PlayerConnection, payload: username);
                return 0;
            }
            catch (ArgumentNullException e)
            {
                // Log Error
                return 1;
            }
            catch (SocketException e)
            {
                // Log Error
                return 2;
            }
        }

        public string AddMessageSize(string message)
        {
            string msgByteSize = message.Length.ToString();
            if (msgByteSize.Length > 4)
            {
                // Log Error?
            }
            while (msgByteSize.Length != 4)
            {
                msgByteSize = "0" + msgByteSize;
            }
            return msgByteSize + message;
        }

        public void SendMessage(NetworkInstruction networkInstruction, string payload = "", string serverOpCode = "01") // Sends a message to the server. The send format is {uuid opcode message} The spaces are not present 
        {
            string message = $"{1}|{serverOpCode}|{(int)networkInstruction:D2}|{payload}";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(AddMessageSize(message));
            rwStream.Write(data, 0, data.Length);

        }

        public void NetworkAttributeChanged(object sender, PropertyChangedEventArgs args)
        {
            SendMessage(NetworkInstruction.NetworkAttribute, $"{(int)sender}|{args.PropertyName}");
        }

        public void UpdateNetworkBuffer()
        {
            messageBuffer += NetworkReceiver.ReadSocketData(rwStream, bufferSize);
            messageBuffer = NetworkCommandHandler.ParseSocketData(messageBuffer);
        }
    }

}
