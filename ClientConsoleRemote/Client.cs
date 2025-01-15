using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace ClientConsoleRemote
{
    internal class Client
    {
        private Uri uri;
        private ClientWebSocket ws;

        public Client(byte[] ip, int port)
        {
            IPEndPoint ep = new IPEndPoint(new IPAddress(ip), port);
            uri = new($"ws://{ep}");
            ws = new();
        }

        public async Task StartAsync()
        {
            Console.WriteLine(ws.State);
            await ws.ConnectAsync(uri, CancellationToken.None);
            Console.WriteLine(ws.State);
            Console.WriteLine("WebSocket client connected.");
            Task receiving = Receive(ws);
            Task sending = Send(ws);
            await Task.WhenAll(receiving, sending);

            ws.Dispose();
        }

        private static async Task Receive(ClientWebSocket ws)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count); Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ws.State);
                Console.WriteLine(ex.Message);
            }
            
        }
        private static async Task Send(ClientWebSocket ws)
        {
            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    string message = Console.ReadLine(); 
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ws.State);
                Console.WriteLine(ex.Message);
            }
            
        }

    }
}
