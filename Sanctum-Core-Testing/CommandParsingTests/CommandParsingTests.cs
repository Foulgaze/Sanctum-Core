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
    }
}
