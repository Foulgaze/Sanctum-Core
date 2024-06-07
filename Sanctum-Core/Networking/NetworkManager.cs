using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public enum NetworkInstruction
    {
        NetworkAttribute, PlayerConnection
    }

    public class NetworkManager
    {


        private TcpClient client;
        private NetworkStream rwStream;
        private string messageBuffer = "";
        private readonly int bufferSize = 4096;
        private readonly NetworkAttributeFactory NetworkAttributeFactory;
        private readonly NetworkCommandHandler NetworkCommandHandler;
        private readonly bool mock;




        public NetworkManager(NetworkAttributeFactory networkAttributeFactory, bool mock = false)
        {
            this.NetworkAttributeFactory = networkAttributeFactory;
            this.NetworkAttributeFactory.attributeValueChanged += this.NetworkAttributeChanged;
            this.NetworkCommandHandler = new NetworkCommandHandler();
            this.mock = mock;
        }

        public int Connect(string server, string username, int port)
        {
            try
            {
                if (!this.mock)
                {
                    this.client = new TcpClient(server, port);
                    this.rwStream = this.client.GetStream();
                }
                else
                {
                    this.rwStream = new NetworkMock(null);
                }
                this.SendMessage(NetworkInstruction.PlayerConnection, payload: username);
                return 0;
            }
            catch (ArgumentNullException)
            {
                // Log Error
                return 1;
            }
            catch (SocketException)
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
            byte[] data = System.Text.Encoding.UTF8.GetBytes(this.AddMessageSize(message));
            this.rwStream.Write(data, 0, data.Length);

        }

        public void NetworkAttributeChanged(object sender, PropertyChangedEventArgs args)
        {
            this.SendMessage(NetworkInstruction.NetworkAttribute, $"{(int)sender}|{args.PropertyName}");
        }

        public void UpdateNetworkBuffer()
        {
            this.messageBuffer += NetworkReceiver.ReadSocketData(this.rwStream, this.bufferSize);
            this.messageBuffer = this.NetworkCommandHandler.ParseSocketData(this.messageBuffer);
        }
    }

}
