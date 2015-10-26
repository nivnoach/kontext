using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kontext.Items;

namespace Kontext.WindowKontext
{
    public class WindowsEnumerator
    {
        private static readonly ConcurrentDictionary<IntPtr, VisualWindow> WindowsCache =
            new ConcurrentDictionary<IntPtr, VisualWindow>();

        private static List<IntPtr> handlesToIgnore;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        protected static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        public static List<KontextItem> GetKontextualItems(List<IntPtr> handlesToSkip)
        {
            // Garbage collect from cache
            var allNonWindows = WindowsCache.Keys.Where(key => !IsWindow(key)).ToList();
            VisualWindow tempObject;
            allNonWindows.ForEach(handle => WindowsCache.TryRemove(handle, out tempObject));

            // Enumerate new windows
            handlesToIgnore = handlesToSkip;
            EnumWindows(GetKontextualItems, IntPtr.Zero);

            return new List<KontextItem>(WindowsCache.Values);
        }

        private static bool GetKontextualItems(IntPtr hWnd, IntPtr lParam)
        {
            if (handlesToIgnore != null && handlesToIgnore.Contains(hWnd))
            {
                return true;
            }

            // Extract the window text
            var windowTextLength = GetWindowTextLength(hWnd);

            // If the window has title and visible - this is a legit kontext item
            if (windowTextLength > 0 && IsWindowVisible(hWnd) && !WindowsCache.ContainsKey(hWnd))
            {
                // Get the window title
                var strText = new StringBuilder(windowTextLength + 1);
                GetWindowText(hWnd, strText, windowTextLength + 1);

                // Create and cache a new item
                var newWindow = new VisualWindow(hWnd, strText.ToString());
                WindowsCache[hWnd] = newWindow;

                // Add to the result
                WindowsCache[hWnd] = newWindow;
            }

            return true;
        }

        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    }
}