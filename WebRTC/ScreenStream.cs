using System.Management;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WebRTCRemote
{
    public static class ScreenStream
    {
        //screen capture size
        static int height = Constants.ScreenHeight;
        static int width = Constants.ScreenWidth;

        private static float MouseClickPosRatio = Constants.DisplayRatio * Constants.DisplayZoomOut;

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

        private static void DrawCursor(Graphics g)
        {
            // 獲取滑鼠資訊
            CURSORINFO cursorInfo;
            cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            if (GetCursorInfo(out cursorInfo) && cursorInfo.flags == CURSOR_SHOWING)
            {
                // 繪製滑鼠游標
                Cursor cursor = new Cursor(cursorInfo.hCursor);
                // 顯示並修正滑鼠位置
                cursor.Draw(g, new Rectangle((int)((float)cursorInfo.ptScreenPos.X * Constants.DisplayRatio), (int)((float)cursorInfo.ptScreenPos.Y * Constants.DisplayRatio), cursor.Size.Width, cursor.Size.Height));
            }
        }

        public static Bitmap RecordImage()
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bmp.Size);

                DrawCursor(g);
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

        public static Bitmap ResizeScreenImage(Image image)
        {
            float w = (float)width / MouseClickPosRatio;
            float h = (float)height / MouseClickPosRatio;

            Bitmap newImage = new Bitmap((int)w, (int)h);
            using Graphics g = Graphics.FromImage(newImage);
            g.DrawImage(image, 0, 0, w, h);

            return newImage;
        }

        /// <summary>
        /// 將目前顯示器畫面擷取並轉換成 Blob（即byte[]） 回傳
        /// </summary>
        /// <returns></returns>
        public static byte[] RecordImageBytes()
        {
            byte[] imageBytes;

            using(MemoryStream ms = new MemoryStream())
            {
                ResizeScreenImage(RecordImage()).Save(ms, ImageFormat.Jpeg);
                imageBytes = ms.ToArray();
                //Bitmap bmp = new(ms); 轉回bmp類型
            }
            return imageBytes;
        }

        public static string RecordImageBase64String()
        {
            return Convert.ToBase64String(RecordImageBytes());
        }

        /// <summary>
        /// LockBits 方法用於直接存取位圖的像素數據。它通過以下步驟實現：
        ///鎖定位圖：使用 LockBits 鎖定位圖並取得 BitmapData，其中包含有關圖像數據的資訊。
        ///存取數據指標：取得圖像數據的指標，這樣可以直接讀取或修改圖像的像素值。
        ///解鎖位圖：修改完成後，使用 UnlockBits 解鎖位圖。
        ///此方法主要用於高效的像素級別的圖像處理，適合需要頻繁讀寫圖像數據的操作。
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ConvertBitmapToByteArray()
        {
            var bitmap = RecordImage();
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int numBytes = bmpData.Stride * bitmap.Height;
            byte[] bmpBytes = new byte[numBytes];

            Marshal.Copy(bmpData.Scan0, bmpBytes, 0, numBytes);
            bitmap.UnlockBits(bmpData);

            return bmpBytes;
        }
    }
}
