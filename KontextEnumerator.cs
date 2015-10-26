using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontext.Items;
using Kontext.WindowKontext;

namespace Kontext
{
    public static class KontextEnumerator
    {
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        public static List<KontextItem> GetAllItems(List<IntPtr> handlesToIgnore = null)
        {
            var allItems = new List<KontextItem>();

            // Add the desktop to ignore list
            handlesToIgnore = handlesToIgnore ?? new List<IntPtr>();
            handlesToIgnore.Add(GetDesktopWindow());

            // Add visual windows 
            allItems.AddRange(WindowsEnumerator.GetKontextualItems(handlesToIgnore));

            // That's it :)
            return allItems;
        }
    }
}