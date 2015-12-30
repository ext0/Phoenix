using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixClient.Work.ClientOperations
{
    public static class ScreenCapture
    {
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        internal const Int32 CURSOR_SHOWING = 0x00000001;

        public static byte[] captureScreen(Screen screen, long compressLevel)
        {
            byte[] returnData = null;
            using (MemoryStream mem = new MemoryStream())
            {
                Rectangle screenSize = screen.Bounds;
                using (Bitmap target = new Bitmap(screenSize.Width, screenSize.Height))
                {
                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.CopyFromScreen(screenSize.X, screenSize.Y, 0, 0, new Size(screenSize.Width, screenSize.Height));
                        CURSORINFO pci;
                        pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
                        }
                    }
                    EncoderParameters ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)compressLevel);
                    target.Save(mem, getEncoderInfo("image/jpeg"), ep);
                    returnData = mem.ToArray();
                    DeleteObject(target.GetHbitmap());
                }
            }
            return returnData;
        }
        private static ImageCodecInfo getEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
