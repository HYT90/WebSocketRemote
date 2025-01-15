using System.Management;
using System.Drawing.Imaging;

namespace WebRTCRemote
{
    public static class ScreenStream
    {
        //screen capture size
        static int height;
        static int width;


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
                g.CopyFromScreen(new Point(0, 0), Point.Empty, bmp.Size);
            }
            return bmp;
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
                RecordImage().Save(ms, ImageFormat.Jpeg);
                imageBytes = ms.ToArray();
                //Bitmap bmp = new(ms); 轉回bmp類型
            }
            return imageBytes;
        }

        //擷取音源輸出
    }
}
