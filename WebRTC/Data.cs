namespace WebRTCRemote
{
    public class Data
    {
        public int Type { get; set; }

        // 滑鼠位置
        public int? X {get;set;}
        public int? Y { get; set; }
        // Keyboard
        public int? Key { get; set; }
    }
}