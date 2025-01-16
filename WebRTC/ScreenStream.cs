using System.Management;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WebRTCRemote
{
    public static class ScreenStream
    {
        //screen capture size
        static int height;
        static int width;

        private static float MousePosRatio = Constants.DisplayRatio;

        // 使用 P/Invoke 從 user32.dll 中引入 GetCursorInfo 函數，用於獲取滑鼠游標資訊。
        [DllImport("user32.dll")] 
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        // 定義滑鼠位置
        // StructLayout(LayoutKind.Sequential) 表示這個結構體的欄位在記憶體中按照定義的順序排列。
        [StructLayout(LayoutKind.Sequential)] 
        private struct POINT 
        { 
            public int X; 
            public int Y; 
        }

        // 定義滑鼠游標資訊
        // cbSize：結構體的大小（以字節為單位）。
        // flags：表示游標狀態的標誌。例如，CURSOR_SHOWING 表示游標正在顯示。
        // hCursor：游標的 Handle，指向 Windows 中的游標資源。
        // ptScreenPos：游標在螢幕上的位置，使用 POINT 結構體表示。
        [StructLayout(LayoutKind.Sequential)] 
        private struct CURSORINFO 
        { 
            public int cbSize; 
            public int flags; 
            public IntPtr hCursor; 
            public POINT ptScreenPos; 
        }

        // Windows API 中定義的標誌值。表示游標正在顯示。
        private const int CURSOR_SHOWING = 0x00000001;

        static ScreenStream() 
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["ScreenHeight"] != null && obj["ScreenWidth"] != null)
                {
                    height = Int32.Parse(obj["ScreenHeight"].ToString()!);
                    width = Int32.Parse(obj["ScreenWidth"].ToString()!);
                }

            }
        }

        public static Bitmap RecordImage()
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bmp.Size);

                // 獲取滑鼠資訊
                CURSORINFO cursorInfo;
                cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                if (GetCursorInfo(out cursorInfo) && cursorInfo.flags == CURSOR_SHOWING)
                { 
                    // 繪製滑鼠游標
                    Cursor cursor = new Cursor(cursorInfo.hCursor); 
                    // 顯示並修正滑鼠位置
                    cursor.Draw(g, new Rectangle((int)((float)cursorInfo.ptScreenPos.X * MousePosRatio), (int)((float)cursorInfo.ptScreenPos.Y * MousePosRatio), cursor.Size.Width, cursor.Size.Height)); 
                }
            }
            return bmp;
        }


        public static Bitmap ResizeScreenImage(Image image, float ratio)
        {
            float w = (float)width/ratio;
            float h = (float)height/ratio;

            Bitmap newImage = new Bitmap((int)w, (int)h);
            using Graphics g = Graphics.FromImage(newImage);
            g.DrawImage(image, 0, 0, w, h);

            return newImage;
        }

        /// <summary>
        /// 將目前顯示器畫面擷取並轉換成 byte[] 回傳
        /// </summary>
        /// <returns></returns>
        public static byte[] RecordImageBytes()
        {
            byte[] imageBytes;

            using(MemoryStream ms = new MemoryStream())
            {
                RecordImage().Save(ms, ImageFormat.Png);
                imageBytes = ms.ToArray();
                //Bitmap bmp = new(ms); 轉回bmp類型
            }
            return imageBytes;
        }

        public static string RecordImageBase64String()
        {
            return Convert.ToBase64String(RecordImageBytes());
        }

        //擷取音源輸出
    }
}
