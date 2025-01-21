using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Text.Json;

namespace WebRTCRemote
{
    internal static class Server
    {
        private static string? endpoint;
        private static HttpListener? httpListener;
        private static WebSocket? webSocket;

        public static void InitalizeServer(IPAddress ip, int port)
        {
            IPEndPoint ep = new(ip, port);
            endpoint = ep.ToString();
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://{endpoint}/");
        }

        public static async Task RunAsync()
        {
            httpListener.Start();
            Console.WriteLine($"WebSocket server started at ws://{endpoint}/");
            while (webSocket == null || webSocket.State != WebSocketState.Open)
            {
                HttpListenerContext context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    Console.WriteLine($"{context.Request.RemoteEndPoint} has connected.");
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    webSocket = wsContext.WebSocket;

                    //var send = Send();
                    //var echo = Echo();
                    Task.Run(Send);
                    await Echo();

                    //await Task.WhenAll(echo, send);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private static async Task Echo()
        {
            byte[] buffer = new byte[Constants.PacketSize];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result == null) break;

                    if(result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
                    }

                    if (result.Count>0)
                    {
                        //string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        //var data = JsonSerializer.Deserialize<Data>(message);

                        //Console.WriteLine(message);

                        RemoteHandle.DataContentReceived(buffer, result.Count);
                    }
                    //byte[] responseBuffer = Encoding.UTF8.GetBytes("Echo from server: " + message);
                    //await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Here is from Echo(). {ex.Message}");
                webSocket.Dispose();
            }
            
        }

        private static async Task Send()
        {
            DateTime nextLoop = DateTime.Now;
            try
            {
                while(webSocket.State == WebSocketState.Open && nextLoop < DateTime.Now)
                {
                    var data = Encoding.UTF8.GetBytes(ScreenStream.RecordImageBase64String());
                    await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
                    nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                    if(nextLoop > DateTime.Now)
                    {
                        await Task.Delay(nextLoop - DateTime.Now);
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Here is from Send(). {ex.Message}");
                webSocket.Dispose();
            }
            
            
        }
    }
}
