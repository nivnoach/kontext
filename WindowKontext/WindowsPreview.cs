// Decompiled with JetBrains decompiler
// Type: Kontext.WindowsPreview
// Assembly: Kontext, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A8E5B05C-B7A7-438A-88F0-1E017A5EC409
// Assembly location: C:\Users\Niv\Desktop\Kontext.exe

using System;
using System.Runtime.InteropServices;

namespace Kontext
{
  public class WindowsPreview : IDisposable
  {
    private static readonly int DWM_TNP_VISIBLE = 8;
    private static readonly int DWM_TNP_OPACITY = 4;
    private static readonly int DWM_TNP_RECTDESTINATION = 1;
    private IntPtr _currentThumbHandle = IntPtr.Zero;
    private readonly int _defaultWidth;
    private readonly IntPtr _targetHandle;

    public WindowsPreview(IntPtr targetHandle, int width)
    {
      this._targetHandle = targetHandle;
      this._defaultWidth = width;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

    [DllImport("dwmapi.dll")]
    private static extern int DwmUnregisterThumbnail(IntPtr thumb);

    [DllImport("dwmapi.dll")]
    private static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out WindowsPreview.PSIZE size);

    [DllImport("dwmapi.dll")]
    private static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref WindowsPreview.DwmThumbnailProperties props);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out WindowsPreview.Rect lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

    public void Dispose()
    {
      if (!(this._currentThumbHandle != IntPtr.Zero))
        return;
      WindowsPreview.DwmUnregisterThumbnail(this._currentThumbHandle);
    }

    public bool SetPreviewFor(IntPtr sourceHandle)
    {
      if (sourceHandle == this._currentThumbHandle)
        return true;
      if (this._currentThumbHandle != IntPtr.Zero)
      {
        WindowsPreview.DwmUnregisterThumbnail(this._currentThumbHandle);
        this._currentThumbHandle = IntPtr.Zero;
      }
      if (sourceHandle == IntPtr.Zero)
        return true;
      if (WindowsPreview.DwmRegisterThumbnail(this._targetHandle, sourceHandle, out this._currentThumbHandle) == 0)
      {
        this.UpdateThumb();
        return true;
      }
      this._currentThumbHandle = IntPtr.Zero;
      return false;
    }

    private void UpdateThumb()
    {
      if (this._currentThumbHandle == IntPtr.Zero)
        return;
      WindowsPreview.PSIZE size;
      WindowsPreview.DwmQueryThumbnailSourceSize(this._currentThumbHandle, out size);
      WindowsPreview.DwmThumbnailProperties props = new WindowsPreview.DwmThumbnailProperties()
      {
        dwFlags = WindowsPreview.DWM_TNP_VISIBLE | WindowsPreview.DWM_TNP_RECTDESTINATION | WindowsPreview.DWM_TNP_OPACITY,
        fVisible = true,
        opacity = byte.MaxValue,
        rcDestination = new WindowsPreview.Rect(0, 0, this._defaultWidth, this._defaultWidth * size.Height / size.Width)
      };
      WindowsPreview.Rect lpRect;
      if (WindowsPreview.GetWindowRect(this._targetHandle, out lpRect))
        WindowsPreview.MoveWindow(this._targetHandle, lpRect.Left, lpRect.Top, props.rcDestination.Width, props.rcDestination.Height, true);
      WindowsPreview.DwmUpdateThumbnailProperties(this._currentThumbHandle, ref props);
    }

    internal struct DwmThumbnailProperties
    {
      public int dwFlags;
      public WindowsPreview.Rect rcDestination;
      public WindowsPreview.Rect rcSource;
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
        this.Width = width;
        this.Height = height;
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
        get
        {
          return this.Right - this.Left;
        }
      }

      public int Height
      {
        get
        {
          return this.Bottom - this.Top;
        }
      }

      internal Rect(WindowsPreview.Rect other)
      {
        this.Left = other.Left;
        this.Top = other.Top;
        this.Right = other.Right;
        this.Bottom = other.Bottom;
      }

      internal Rect(int left, int top, int right, int bottom)
      {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
      }
    }
  }
}
