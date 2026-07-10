using System;
using System.Runtime.InteropServices;

namespace RemoteDesktopCore
{
    /// <summary>
    /// Simulates mouse and keyboard input on the local system
    /// </summary>
    public class InputSimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// Moves the mouse to specified coordinates
        /// </summary>
        public static void MoveMouse(int x, int y)
        {
            mouse_event(MOUSEEVENTF_MOVE, x, y, 0, IntPtr.Zero);
        }

        /// <summary>
        /// Performs a left mouse click
        /// </summary>
        public static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        /// <summary>
        /// Performs a right mouse click
        /// </summary>
        public static void RightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, IntPtr.Zero);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, IntPtr.Zero);
        }

        /// <summary>
        /// Scrolls the mouse wheel
        /// </summary>
        public static void Scroll(int delta)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, delta, IntPtr.Zero);
        }

        /// <summary>
        /// Presses a key
        /// </summary>
        public static void PressKey(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_KEYDOWN, IntPtr.Zero);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        /// <summary>
        /// Holds down a key
        /// </summary>
        public static void KeyDown(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_KEYDOWN, IntPtr.Zero);
        }

        /// <summary>
        /// Releases a held key
        /// </summary>
        public static void KeyUp(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }
    }
}
