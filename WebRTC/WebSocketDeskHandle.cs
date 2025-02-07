using SIPSorcery.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebRTCRemote
{
    internal class WebSocketDeskHandle : WebRTCWebSocketPeer
    {
        public Action<byte[], int> Handle;

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            Console.WriteLine($"{DateTime.Now}: Receive some message from client.");
            Handle(e.RawData, e.RawData.Length);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
        }
    }
}
