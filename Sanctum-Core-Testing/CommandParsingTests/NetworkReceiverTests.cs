using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing.CommandParsingTests
{
    public class NetworkReceiverTests
    {
        [Test]
        public void ReadSocketData_ShouldAppendDataToBuffer_WhenDataIsAvailable()
        {
            StringBuilder buffer = new();
            int bufferSize = 1024;
            byte[] messageBytes = Encoding.UTF8.GetBytes("0044|00|Foulgaze");
            // Act
            NetworkReceiver.ReadSocketData(new MemoryStream(messageBytes), bufferSize, buffer);

            // Assert
            Assert.That(buffer.ToString(), Is.EqualTo("0044|00|Foulgaze"));
        }
    }
}
