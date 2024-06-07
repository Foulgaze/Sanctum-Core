using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    using System;
    using System.ComponentModel;
    using System.Net.Sockets;

    public class NetworkMock : NetworkStream
    {
        public event PropertyChangedEventHandler mockSendData = delegate { };

        public string BUFFER = "";

        public NetworkMock(Socket socket) : base(socket)
        {
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (BUFFER.Length == 0)
                return 0;

            int bytesToRead = Math.Min(size, BUFFER.Length);

            byte[] data = System.Text.Encoding.UTF8.GetBytes(BUFFER);
            Array.Copy(data, 0, buffer, offset, bytesToRead);

            BUFFER = BUFFER.Substring(bytesToRead);

            return bytesToRead;
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            mockSendData(System.Text.Encoding.UTF8.GetString(buffer, offset, size), null);

        }

        public override bool DataAvailable
        {
            get { return BUFFER.Length != 0; }
        }
    }
}
