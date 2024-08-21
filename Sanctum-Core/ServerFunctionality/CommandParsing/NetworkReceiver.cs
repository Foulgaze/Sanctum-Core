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
        public static bool ReadSocketData(NetworkStream rwStream, int bufferSize, StringBuilder buffer, out bool timedOut)
        {
            byte[] data = new byte[bufferSize];
            timedOut = false;
            int amountOfBytesRead;

            try
            {
                // Check if data is available before reading
                if (!rwStream.DataAvailable)
                {
                    return true; // No data, but not disconnected
                }

                amountOfBytesRead = rwStream.Read(data, 0, data.Length);
                _ = buffer.Append(Encoding.UTF8.GetString(data, 0, amountOfBytesRead));
            }
            catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                         socketEx.SocketErrorCode == SocketError.TimedOut)
            {
                Logger.Log("Read operation timed out.");
                timedOut = true;
                return true;
            }
            catch
            {
                return false;
            }

            if (amountOfBytesRead == 0)
            {
                return false;
            }

            return true;
        }

    }
}
