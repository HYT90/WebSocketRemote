using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace WebRTCRemote
{
    internal class Server
    {
        private string endpoint;
        private HttpListener httpListener;




        public Server(byte[] ip, int port) 
        {
            IPEndPoint ep = new(new IPAddress(ip), port);
            endpoint = ep.ToString();
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://{endpoint}/");
        }

        public async Task RunAsync()
        {
            httpListener.Start();
            Console.WriteLine($"WebSocket server started at ws://{endpoint}/");

            while (true)
            {
                HttpListenerContext context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    Console.WriteLine($"{context.Request.RemoteEndPoint} has connected.");
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = wsContext.WebSocket;

                    await Echo(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private static async Task Echo(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received from client: " + message);

                    byte[] responseBuffer = Encoding.UTF8.GetBytes("Echo from server: " + message);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //嘗試重新連線

            }
            
        }
    }
}
