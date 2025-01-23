using System.Text;
using System.Text.Json;

namespace WebRTCRemote
{
    internal class RemoteHandle
    {
        private static Dictionary<int, Action<int>> operations = new Dictionary<int,Action<int>>();


        public static void DataContentReceived(byte[] buffer, int Size)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, Size);
            Data data = JsonSerializer.Deserialize<Data>(message);


            Console.WriteLine(message);

            // 建立一事件處理類別 處理輸入的 data 
            InputDataTreatment.Operation(data);
        }
    }
}
