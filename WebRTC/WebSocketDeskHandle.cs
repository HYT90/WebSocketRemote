using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebRTCRemote
{
    internal class WebSocketDeskHandle : WebSocketBehavior
    {
        public Action<byte[], int> Handle;

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now}: Receive some message from client.");
            Handle(e.RawData, e.RawData.Length);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
        }
    }
}
