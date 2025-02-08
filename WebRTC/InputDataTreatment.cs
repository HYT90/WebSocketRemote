using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebRTCRemote
{
    internal class InputDataTreatment
    {
        private static float offset = Constants.DisplayZoomOut / Constants.DisplayRatio;
        // mouse event
        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);


        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        // keyboard event
        // 定義結構體以描述輸入事件
        // 定義 INPUT 結構
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion u;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        const int INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;


        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);


        public static async Task Operation(string message)
        {
            Data data = JsonSerializer.Deserialize<Data>(message);

            Console.WriteLine("----------------------------------");
            Console.WriteLine("From client: ", message);
            Console.WriteLine("----------------------------------");

            if(data == null) return;

            switch (data.Type)
            {
                //left click
                case 1:
                    MouseClick(data, MouseEvent.MOUSEEVENTF_LEFTCLICK);                    
                    break;
                //right click
                case 2:
                    MouseClick(data, MouseEvent.MOUSEEVENTF_RIGHTCLICK);
                    break;
                //dbclick
                case 3:
                    break;
                //keyboard
                case 4:
                    Keyboard(data);
                    break;
                //mouse move
                case 5:
                    //MouseMove(data, MouseEvent.MOUSEEVENTF_MOVE);
                    break;
            }
        }

        private static void MouseClick(Data data, MouseEvent mouseEvent)
        {
            float screenX = (float)data.X.Value * offset;
            float screenY = (float)data.Y.Value * offset;
            SetCursorPos((int)screenX, (int)screenY);
            mouse_event((uint)mouseEvent, (uint)screenX, (uint)screenY, 0, 0);
        }
        
        private static void MouseMove(Data data, MouseEvent mouseEvent)
        {
            float screenX = (float)(data.X.Value/2.5f)*0.85f;
            float screenY = (float)(data.Y.Value/2.5f) * 0.85f;
            SetCursorPos((int)screenX, (int)screenY);
            mouse_event((uint)mouseEvent, (uint)screenX, (uint)screenY, 0, 0);
        }

        private static void Keyboard(Data data)
        {
            ushort key = (ushort)data.Key.Value;
            Console.WriteLine(key);
            GetAsyncKeyState(key);
            KeyboardInput(key);
        }

        private static void KeyboardInput(ushort k)
        {
            INPUT[] inputs = new INPUT[2];

            // 模擬按下
            inputs[0] = new INPUT
            {
                type = 1, // 輸入類型: 鍵盤
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = k,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // 模擬釋放
            inputs[1] = new INPUT
            {
                type = 1, // 輸入類型: 鍵盤
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = k,
                        wScan = 0,
                        dwFlags = 2, // KEYEVENTF_KEYUP 標誌
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // 傳送模擬按鍵輸入
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }
    }
}
