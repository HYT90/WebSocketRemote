namespace WebRTCRemote
{
    public class Data
    {
        public string? Type { get; private set; }

        // 滑鼠位置
        public int? X { get; set; }
        public int? Y { get; set; }

        // Keyboad
        public string? Key { get; set; }
    }
}