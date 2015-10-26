using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Kontext.WindowKontext
{
    internal class WindowEnumerator
    {
        public delegate bool WindowEnumDelegate(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hwnd, WindowEnumDelegate del, int lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder bld, int size);

        public void GetContextMenu()
        {
            WindowEnumDelegate del = WindowEnumProc;
            EnumChildWindows(IntPtr.Zero, del, 0);
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        public bool WindowEnumProc(IntPtr hwnd, int lParam)
        {
            var bld = new StringBuilder(256);
            GetWindowText(hwnd, bld, 256);
            var str = bld.ToString();
            if (hwnd == (IntPtr) 32768)
            {
                var num = (int) MessageBox.Show("lalala");
            }
            if (str.Length > 0)
                Console.WriteLine(str);
            return true;
        }
    }
}