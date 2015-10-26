using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kontext.WindowKontext
{
    public class PreviewWindow : Form
    {
        private const int CS_DROPSHADOW = 131072;
#pragma warning disable 649
        private IContainer components;
#pragma warning restore 649
        private Panel pnlBorder;

        public PreviewWindow()
        {
            InitializeComponent();
        }

        public PreviewWindow(int top, int left)
            : this()
        {
            Top = top;
            Left = left;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;
                createParams.ClassStyle |= 131072;
                return createParams;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlBorder = new Panel();
            SuspendLayout();
            pnlBorder.BorderStyle = BorderStyle.Fixed3D;
            pnlBorder.Dock = DockStyle.Fill;
            pnlBorder.Location = new Point(0, 0);
            pnlBorder.Name = "pnlBorder";
            pnlBorder.Size = new Size(256, 193);
            pnlBorder.TabIndex = 0;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(256, 193);
            Controls.Add(pnlBorder);
            FormBorderStyle = FormBorderStyle.None;
            Name = "PreviewWindow";
            ShowInTaskbar = false;
            Text = "PreviewWindow";
            TopMost = true;
            ResumeLayout(false);
        }
    }
}