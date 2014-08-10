using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GraphicsProcessor
{
    public class ScreenCapture
    {
        #region Singleton

        static readonly Lazy<ScreenCapture> lazy = new Lazy<ScreenCapture>(() => new ScreenCapture());

        public static ScreenCapture Instance { get { return lazy.Value; } }

        ScreenCapture()
        {
        }

        #endregion

        Bitmap _testScreenshot = null;
        bool _useGDI = false;

        public Bitmap TestScreenshot
        {
            get { return _testScreenshot; }
            set { _testScreenshot = value; }
        }

        public bool UseGDI
        {
            get { return _useGDI; }
            set { _useGDI = value; }
        }

        #region DllImport

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr ptr);

        #endregion

        public Bitmap CaptureScreen(int x, int y, Size size, bool useGDI)
        {
            if (_testScreenshot != null)
            {
                return BitmapProcessor.Copy(_testScreenshot, new Rectangle(new Point(x, y), size));
            }
            else if (useGDI)
            {
                IntPtr hDesk = GetDesktopWindow();
                IntPtr hSrce = GetWindowDC(hDesk);
                IntPtr hDest = CreateCompatibleDC(hSrce);
                IntPtr hBmp = CreateCompatibleBitmap(hSrce, size.Width, size.Height);
                IntPtr hOldBmp = SelectObject(hDest, hBmp);
                bool b = BitBlt(hDest, 0, 0, size.Width, size.Height, hSrce, x, y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
                Bitmap bmp = Bitmap.FromHbitmap(hBmp);
                SelectObject(hDest, hOldBmp);
                DeleteObject(hBmp);
                DeleteDC(hDest);
                ReleaseDC(hDesk, hSrce);
                return bmp;
            }
            else
            {
                Bitmap memoryImage = new Bitmap(size.Width, size.Height);
                System.Drawing.Graphics memoryGraphics = System.Drawing.Graphics.FromImage(memoryImage);
                memoryGraphics.CopyFromScreen(x, y, 0, 0, size);
                return memoryImage;
            }
        }

        public Bitmap CaptureScreen(int x, int y, Size size)
        {
            return CaptureScreen(x, y, size, _useGDI);
        }

        public Bitmap CaptureScreen(Rectangle rect, bool useGDI)
        {
            return CaptureScreen(rect.X, rect.Y, rect.Size, useGDI);
        }

        public Bitmap CaptureScreen(Rectangle rect)
        {
            return CaptureScreen(rect.X, rect.Y, rect.Size, _useGDI);
        }

        public Bitmap CaptureScreen(int x1, int y1, int x2, int y2, bool useGDI)
        {
            Size size = new Size(x2 - x1, y2 - y1);
            return CaptureScreen(x1, y1, size, useGDI);
        }

        public Bitmap CaptureScreen(int x1, int y1, int x2, int y2)
        {
            return CaptureScreen(x1, y1, x2, y2, _useGDI);
        }

        public Bitmap CaptureScreen(Point p, Size size, bool useGDI)
        {
            return CaptureScreen(p.X, p.Y, size, useGDI);
        }

        public Bitmap CaptureScreen(Point p, Size size)
        {
            return CaptureScreen(p, size, _useGDI);
        }

        public Color CapturePixel(int x, int y, bool useGDI)
        {
            Bitmap bmp = CaptureScreen(x, y, new Size(1, 1), useGDI);

            Color result = bmp.GetPixel(0, 0);

            bmp.Dispose();
            bmp = null;

            return result;
        }

        public Color CapturePixel(int x, int y)
        {
            return CapturePixel(x, y, _useGDI);
        }

        public Color CapturePixel(Point p)
        {
            return CapturePixel(p, _useGDI);
        }

        public Color CapturePixel(Point p, bool useGDI)
        {
            return CapturePixel(p.X, p.Y, useGDI);
        }
    }
}
