using System.Net.Sockets;
using System.Text;
namespace Sanctum_Core
{
    public class NetworkReceiver
    {
        /// <summary>
        /// Reads data from the specified stream into the provided buffer until no more data is available.
        /// </summary>
        /// <param name="rwStream">The stream from which to read data.</param>
        /// <param name="bufferSize">The maximum size of the buffer to read from the stream.</param>
        /// <param name="buffer">The <see cref="StringBuilder"/> where the read data will be appended.</param>
        public static void ReadSocketData(Stream rwStream, int bufferSize, StringBuilder buffer)
        {
            if(rwStream is null || (rwStream is NetworkStream stream && !stream.DataAvailable) || !rwStream.CanRead)
            {
                return;
            }

            static bool EndCondition(Stream rwStream, int dataSize)
            {
                return rwStream is NetworkStream stream ? stream.DataAvailable : dataSize != 0;
            }

            byte[] data = new byte[bufferSize];
            int dataSize;
            do
            {
                dataSize = rwStream.Read(data, 0, data.Length);
                _ = buffer.Append(Encoding.UTF8.GetString(data, 0, dataSize));
            } while (EndCondition(rwStream,dataSize)); 
        }
    }
}
