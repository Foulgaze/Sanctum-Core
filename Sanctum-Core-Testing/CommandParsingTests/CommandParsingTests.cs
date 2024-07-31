using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing
{
    public class NetworkCommandManagerTests
    {
        [Test]
        public void ParseSocketData_ShouldReturnCommand_WhenValidData()
        {
            StringBuilder messageBuffer = new();
            _ = messageBuffer.Append("0012Test Command");

            string? result = NetworkCommandManager.ParseSocketData(messageBuffer);

            Assert.That(result, Is.EqualTo("Test Command"));
            Assert.That(messageBuffer.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ParseSocketData_ShouldReturnNull_WhenBufferIsTooShort()
        {
            StringBuilder messageBuffer = new();
            _ = messageBuffer.Append("004");

            string? result = NetworkCommandManager.ParseSocketData(messageBuffer);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseSocketData_ShouldThrowException_WhenMessageLengthIsFlawed()
        {
            StringBuilder messageBuffer = new();
            _ = messageBuffer.Append("004A");

            _ = Assert.Throws<Exception>(() => NetworkCommandManager.ParseSocketData(messageBuffer));
        }

        [Test]
        public void GetNextNetworkCommand_ShouldReturnNetworkCommand_WhenDataIsAvailable()
        {
            StringBuilder buffer = new();
            int bufferSize = 1024;
            byte[] messageBytes = Encoding.UTF8.GetBytes("0048{\"opCode\": 1, \"instruction\": \"Test Instruction\"}");
            NetworkCommand? result = NetworkCommandManager.GetNextNetworkCommand(new MemoryStream(messageBytes), buffer, bufferSize, true);

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.opCode, Is.EqualTo(1));
            Assert.That(result?.instruction, Is.EqualTo("Test Instruction"));
        }

        [Test]
        public void GetNextNetworkCommand_ShouldReturnNull_WhenNoDataAvailable()
        {
            StringBuilder buffer = new();
            int bufferSize = 1024;
            NetworkCommand? result = NetworkCommandManager.GetNextNetworkCommand(new MemoryStream(0), buffer, bufferSize, false);

            Assert.That(result, Is.Null);
        }
    }
}
