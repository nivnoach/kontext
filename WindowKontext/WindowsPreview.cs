using System;
using System.Runtime.InteropServices;

namespace Kontext.WindowKontext
{
    public class WindowsPreview : IDisposable
    {
        private static readonly int DWM_TNP_VISIBLE = 8;
        private static readonly int DWM_TNP_OPACITY = 4;
        private static readonly int DWM_TNP_RECTDESTINATION = 1;
        private readonly int _defaultWidth;
        private readonly IntPtr _targetHandle;
        private IntPtr _currentThumbHandle = IntPtr.Zero;

        public WindowsPreview(IntPtr targetHandle, int width)
        {
            _targetHandle = targetHandle;
            _defaultWidth = width;
        }

        public void Dispose()
        {
            if (!(_currentThumbHandle != IntPtr.Zero))
                return;
            DwmUnregisterThumbnail(_currentThumbHandle);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        private static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        private static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport("dwmapi.dll")]
        private static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DwmThumbnailProperties props);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        public bool SetPreviewFor(IntPtr sourceHandle)
        {
            if (sourceHandle == _currentThumbHandle)
                return true;
            if (_currentThumbHandle != IntPtr.Zero)
            {
                DwmUnregisterThumbnail(_currentThumbHandle);
                _currentThumbHandle = IntPtr.Zero;
            }
            if (sourceHandle == IntPtr.Zero)
                return true;
            if (DwmRegisterThumbnail(_targetHandle, sourceHandle, out _currentThumbHandle) == 0)
            {
                UpdateThumb();
                return true;
            }
            _currentThumbHandle = IntPtr.Zero;
            return false;
        }

        private void UpdateThumb()
        {
            if (_currentThumbHandle == IntPtr.Zero)
                return;
            PSIZE size;
            DwmQueryThumbnailSourceSize(_currentThumbHandle, out size);
            var props = new DwmThumbnailProperties
            {
                dwFlags = DWM_TNP_VISIBLE | DWM_TNP_RECTDESTINATION | DWM_TNP_OPACITY,
                fVisible = true,
                opacity = byte.MaxValue,
                rcDestination = new Rect(0, 0, _defaultWidth, _defaultWidth*size.Height/size.Width)
            };
            Rect lpRect;
            if (GetWindowRect(_targetHandle, out lpRect))
                MoveWindow(_targetHandle, lpRect.Left, lpRect.Top, props.rcDestination.Width, props.rcDestination.Height,
                    true);
            DwmUpdateThumbnailProperties(_currentThumbHandle, ref props);
        }

        internal struct DwmThumbnailProperties
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        internal struct PSIZE
        {
            public int Width;
            public int Height;

            public PSIZE(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        internal struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

            internal Rect(Rect other)
            {
                Left = other.Left;
                Top = other.Top;
                Right = other.Right;
                Bottom = other.Bottom;
            }

            internal Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }
    }
}