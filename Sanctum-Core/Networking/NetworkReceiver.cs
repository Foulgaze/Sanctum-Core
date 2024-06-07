using System.Net.Sockets;

namespace Sanctum_Core
{
    public class NetworkReceiver
    {
        // Start is called before the first frame update
        static public string ReadSocketData(NetworkStream rwStream, int bufferSize)
        {
            if (rwStream != null && rwStream.DataAvailable)
            {
                string completeMessage = string.Empty;

                // Buffer to store the response bytes.
                byte[] data = new byte[bufferSize];
                do
                {
                    int dataSize = rwStream.Read(data, 0, data.Length);
                    completeMessage += System.Text.Encoding.UTF8.GetString(data, 0, dataSize);
                } while (rwStream.DataAvailable); // Read entire stream in case buffer is too small

                // Data format {4 char Command Length | 32 Char UUID|2 Char OpCode | Up to 4060 Char Instruction}
                // Example [00442bc7f400-c637-462b-b28c-83ce20e74692|00|Foulgaze] A connection message from a new user named Foulgaze, with length 44 bytes
                return completeMessage;
            }
            return "";
        }
    }

}
