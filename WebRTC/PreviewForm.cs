
namespace WebRTCRemote
{
    internal class 預覽視窗 : Form
    {
        private PictureBox? pictureBox;
        private System.Windows.Forms.Timer timer;

        public 預覽視窗()
        {
            this.Text = "Screen Capture Preview";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitialPictureBox();
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 15;
            timer.Tick += Timer_Tick;
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
            pictureBox.Image = ScreenStream.RecordImage();
        }

        void InitialPictureBox()
        {
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.Normal;
            this.Controls.Add(pictureBox);
        }
    }
}
