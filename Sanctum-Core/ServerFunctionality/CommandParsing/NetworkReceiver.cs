using System.Net.Sockets;
using System.Text;
namespace Sanctum_Core
{
    public class NetworkReceiver
    {
        /// <summary>
        /// Reads client data into buffer
        /// </summary>
        /// <param name="rwStream"></param>
        /// <param name="bufferSize"></param>
        /// <param name="buffer"></param>
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
