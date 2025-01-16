using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace WebRTCRemote
{
    internal class 預覽視窗 : Form
    {
        private PictureBox? pictureBox;
        private System.Windows.Forms.Timer timer;
        private const float ratio = Constants.DisplayRatio;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tick">畫面更新間隔時間(ms)</param>
        public 預覽視窗(int tick)
        {
            this.Text = "Screen Capture Preview";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializePictureBox();
            InitializeTimer(tick, Timer_Tick);

        }

        private void InitializeTimer(int tick, EventHandler handler)
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = tick;
            timer.Tick += handler;
            timer.Start();
        } 

        private void Timer_Tick(object? sender, EventArgs e)
        {
            ScreenCapture();
        }

        /// <summary>
        /// 擷取螢幕畫面並顯示在 PictureBox 中;
        /// </summary>
        private void ScreenCapture()
        {
            pictureBox.Image = ScreenStream.ResizeScreenImage(ScreenStream.RecordImage(), ratio);
        }

        private void InitializePictureBox()
        {
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.Normal;
            this.Controls.Add(pictureBox);

            pictureBox.MouseClick += PictureBox_MouseClick;
        }

        #region Client 對 遠端GUI控制
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002; 
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008; 
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            Point clickPosition = e.Location;

            int screenX = clickPosition.X; 
            int screenY = clickPosition.Y;

            SetCursorPos(screenX, screenY); 
            if(e.Button == MouseButtons.Left)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)screenX, (uint)screenY, 0, 0);
            }
            else if(e.Button == MouseButtons.Right)
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)screenX, (uint)screenY, 0, 0);
            }
        }
        #endregion
    }
}
