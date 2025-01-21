using System.Text;
using System.Text.Json;

namespace WebRTCRemote
{
    internal class RemoteHandle
    {
        public static void DataContentReceived(byte[] buffer, int Size)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, Size);
            var data = JsonSerializer.Deserialize<Data>(message);

            Console.WriteLine(message);
        }
    }
}
