using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCRemote
{
    internal class InputDataTreatment
    {
        // mouse event
        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);


        // keyboard event
        // 定義結構體以描述輸入事件
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        const int INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public static void Operation(Data data)
        {
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
                    //MouseClick(data, MouseEvent.MOUSEEVENTF_MOVE);
                    break;
            }
        }

        private static void MouseClick(Data data, MouseEvent mouseEvent)
        {
            float screenX = (float)data.X.Value / Constants.DisplayRatio;
            float screenY = (float)data.Y.Value / Constants.DisplayRatio;
            SetCursorPos((int)screenX, (int)screenY);
            mouse_event((uint)mouseEvent, (uint)screenX, (uint)screenY, 0, 0);
        }

        private static void Keyboard(Data data)
        {
            Console.WriteLine((ushort)data.Key.Value);
        }
    }
}
