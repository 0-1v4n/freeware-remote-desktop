using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RemoteDesktopCore
{
    /// <summary>
    /// Handles screen capture functionality using Windows API
    /// </summary>
    public class ScreenCapture
    {
        private const int SRCCOPY = 0x00CC0020;

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Captures the entire screen
        /// </summary>
        public static Bitmap CaptureScreen()
        {
            return CaptureScreen(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        /// <summary>
        /// Captures a specific area of the screen
        /// </summary>
        public static Bitmap CaptureScreen(int x, int y, int width, int height)
        {
            IntPtr hdcSrc = CreateDC("DISPLAY", null, null, IntPtr.Zero);
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOldBitmap = SelectObject(hdcDest, hBitmap);

            try
            {
                BitBlt(hdcDest, 0, 0, width, height, hdcSrc, x, y, SRCCOPY);
                Bitmap bitmap = Image.FromHbitmap(hBitmap);
                return bitmap;
            }
            finally
            {
                SelectObject(hdcDest, hOldBitmap);
                DeleteObject(hBitmap);
                DeleteDC(hdcDest);
                DeleteDC(hdcSrc);
            }
        }

        /// <summary>
        /// Gets primary screen dimensions
        /// </summary>
        public static (int Width, int Height) GetScreenDimensions()
        {
            return (Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }
    }
}
