using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixLib.Datastore.SendInput
{
    public class SendInputWrapper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
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

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        const int INPUT_MOUSE = 0;
        const int INPUT_KEYBOARD = 1;
        const int INPUT_HARDWARE = 2;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_UNICODE = 0x0004;
        const uint KEYEVENTF_SCANCODE = 0x0008;
        const uint XBUTTON1 = 0x0001;
        const uint XBUTTON2 = 0x0002;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_XDOWN = 0x0080;
        const uint MOUSEEVENTF_XUP = 0x0100;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;


        private MOUSEINPUT createMouseInput(int x, int y, uint data, uint t, uint flag)
        {
            MOUSEINPUT mi = new MOUSEINPUT();
            mi.dx = x;
            mi.dy = y;
            mi.mouseData = data;
            mi.time = t;
            mi.dwFlags = flag;
            return mi;
        }

        private KEYBDINPUT createKeybdInput(short wVK, uint flag)
        {
            KEYBDINPUT i = new KEYBDINPUT();
            i.wVk = (ushort)wVK;
            i.wScan = 0;
            i.time = 0;
            i.dwExtraInfo = IntPtr.Zero;
            i.dwFlags = flag;
            return i;
        }

        /// 

        /// Each time first move the upper left corner, then move from there.
        /// x, y: pixel value of position.
        /// 

        public void sim_mov(int x, int y)
        {
            INPUT[] inp = new INPUT[2];
            inp[0].type = INPUT_MOUSE;
            inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE);
            inp[1].type = INPUT_MOUSE;
            inp[1].mi = createMouseInput(x, y, 0, 0, MOUSEEVENTF_MOVE);
            SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
        }

        public void sim_left_click()
        {
            INPUT[] inp = new INPUT[2];
            inp[0].type = INPUT_MOUSE;
            inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_LEFTDOWN);
            inp[1].type = INPUT_MOUSE;
            inp[1].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_LEFTUP);
            SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
        }

        public void sim_right_click()
        {
            INPUT[] inp = new INPUT[2];
            inp[0].type = INPUT_MOUSE;
            inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_RIGHTDOWN);
            inp[1].type = INPUT_MOUSE;
            inp[1].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_RIGHTUP);
            SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
        }
    }
}