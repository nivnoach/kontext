using System;
using System.Windows.Forms;
using Kontext.Forms;

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