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
        /// <returns>If the operation succeded or error'd</returns>
        public static bool ReadSocketData(Stream rwStream, int bufferSize, StringBuilder buffer)
        {
            byte[] data = new byte[bufferSize];
            int amountOfBytesRead;
            do
            {
                try
                {
                    amountOfBytesRead = rwStream.Read(data, 0, data.Length);
                }
                catch
                {
                    return false;
                }
                _ = buffer.Append(Encoding.UTF8.GetString(data, 0, amountOfBytesRead));
            } while (amountOfBytesRead != 0);
            return true;
        }
    }
}
