using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCRemote
{
    internal class Constants
    {
        public const string IP = "192.168.0.17";
        public const int Port = 8080;

        public const int TICKS_PER_SEC = 80;
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;
        public const float DisplayRatio = 1.25f;
        public const int PacketSize = 307200;

        private Constants() { }
    }
}
