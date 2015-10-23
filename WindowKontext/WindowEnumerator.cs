// Decompiled with JetBrains decompiler
// Type: Kontext.WindowEnumerator
// Assembly: Kontext, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A8E5B05C-B7A7-438A-88F0-1E017A5EC409
// Assembly location: C:\Users\Niv\Desktop\Kontext.exe

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Kontext
{
  internal class WindowEnumerator
  {
    [DllImport("user32.dll")]
    public static extern int EnumChildWindows(IntPtr hwnd, WindowEnumerator.WindowEnumDelegate del, int lParam);

    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hwnd, StringBuilder bld, int size);

    public void GetContextMenu()
    {
      WindowEnumerator.WindowEnumDelegate del = new WindowEnumerator.WindowEnumDelegate(this.WindowEnumProc);
      WindowEnumerator.EnumChildWindows(IntPtr.Zero, del, 0);
      Console.WriteLine("Press enter to exit");
      Console.ReadLine();
    }

    public bool WindowEnumProc(IntPtr hwnd, int lParam)
    {
      StringBuilder bld = new StringBuilder(256);
      WindowEnumerator.GetWindowText(hwnd, bld, 256);
      string str = bld.ToString();
      if (hwnd == (IntPtr) 32768)
      {
        int num = (int) MessageBox.Show("lalala");
      }
      if (str.Length > 0)
        Console.WriteLine(str);
      return true;
    }

    public delegate bool WindowEnumDelegate(IntPtr hwnd, int lParam);
  }
}
