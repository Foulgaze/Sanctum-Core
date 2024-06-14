using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanctum_Core
{
    public enum NetworkInstruction
    {
        Connection, PlayerConnection, NetworkAttribute
    }

    public class NetworkManager
    {
        private TcpClient client;
        private NetworkStream rwStream;
        private readonly StringBuilder messageBuffer = new();
        private readonly int bufferSize = 4096;
        private readonly NetworkAttributeFactory NetworkAttributeFactory;
        public NetworkCommandHandler NetworkCommandHandler { get; }
        private readonly bool isMock;

        public NetworkManager(NetworkAttributeFactory networkAttributeFactory, bool mock = false)
        {
            this.NetworkAttributeFactory = networkAttributeFactory;
            this.NetworkAttributeFactory.attributeValueChanged += this.NetworkAttributeChanged;
            this.NetworkCommandHandler = new NetworkCommandHandler();
            this.isMock = mock;
        }

        public void Connect(string server, int port)
        {
            if (!this.isMock)
            {
                this.client = new TcpClient(server, port);
                this.rwStream = this.client.GetStream();
            }
            else
            {
                TcpListener listener = new(IPAddress.Parse(server), port);
                listener.Start();
                Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(server, port);
                this.rwStream = new NetworkMock(socket);
            }
            this.SendMessage(NetworkInstruction.Connection);
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

        public void SendMessage(NetworkInstruction networkInstruction, string payload = "", string serverOpCode = "01")
        {
            string message = this.isMock ? $"{1}|{(int)networkInstruction:D2}|{payload}" : $"{1}|{serverOpCode}|{(int)networkInstruction:D2}|{payload}";
            byte[] data = Encoding.UTF8.GetBytes(this.AddMessageSize(message));
            this.rwStream.Write(data, 0, data.Length);
        }

        public void NetworkAttributeChanged(object sender, PropertyChangedEventArgs args)
        {
            this.SendMessage(NetworkInstruction.NetworkAttribute, $"{(string)sender}|{args.PropertyName}");
        }

        public void UpdateNetworkBuffer()
        {
            _ = this.messageBuffer.Append(NetworkReceiver.ReadSocketData(this.rwStream, this.bufferSize));
            this.NetworkCommandHandler.ParseSocketData(this.messageBuffer);
        }
    }
}
