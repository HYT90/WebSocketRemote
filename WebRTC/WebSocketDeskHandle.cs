using SIPSorcery.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebRTCRemote
{
    internal class WebSocketDeskHandle : WebRTCWebSocketPeer
    {
        public Func<byte[], int, Task> Handle;

        protected override async void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            Console.WriteLine($"class WebSocketDeskHandle {DateTime.Now}: Receive some message from client.");
            await Handle(e.RawData, e.RawData.Length);
        }

        protected override void OnOpen()
        {
            base.OnOpen();           
        }
    }
}
