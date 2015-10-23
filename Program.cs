// Decompiled with JetBrains decompiler
// Type: Kontext.Program
// Assembly: Kontext, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A8E5B05C-B7A7-438A-88F0-1E017A5EC409
// Assembly location: C:\Users\Niv\Desktop\Kontext.exe

using System;
using System.Windows.Forms;

namespace Kontext
{
    internal static class Program
    {
        private static Kontexts _kontexts;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _kontexts = new Kontexts();
            _kontexts.AdjustToMonitor();
            Application.Run();
        }
    }
}