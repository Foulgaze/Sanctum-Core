using System.ComponentModel;
using System.Net.Sockets;

namespace Sanctum_Core
{
    public class NetworkMock : NetworkStream
    {
        public event PropertyChangedEventHandler mockSendData = delegate { };

        public string BUFFER = "";

        public NetworkMock(Socket socket) : base(socket)
        {
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (this.BUFFER.Length == 0)
            {
                return 0;
            }

            int bytesToRead = Math.Min(size, this.BUFFER.Length);

            byte[] data = System.Text.Encoding.UTF8.GetBytes(this.BUFFER);
            Array.Copy(data, 0, buffer, offset, bytesToRead);

            this.BUFFER = this.BUFFER[bytesToRead..];

            return bytesToRead;
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            mockSendData(System.Text.Encoding.UTF8.GetString(buffer, offset, size), null);

        }

        public override bool DataAvailable => this.BUFFER.Length != 0;
    }
}
