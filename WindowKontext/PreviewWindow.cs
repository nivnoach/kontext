// Decompiled with JetBrains decompiler
// Type: Kontext.PreviewWindow
// Assembly: Kontext, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A8E5B05C-B7A7-438A-88F0-1E017A5EC409
// Assembly location: C:\Users\Niv\Desktop\Kontext.exe

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kontext
{
  public class PreviewWindow : Form
  {
    private const int CS_DROPSHADOW = 131072;
    private IContainer components;
    private Panel pnlBorder;

    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams createParams = base.CreateParams;
        createParams.ClassStyle |= 131072;
        return createParams;
      }
    }

    public PreviewWindow()
    {
      this.InitializeComponent();
    }

    public PreviewWindow(int top, int left)
      : this()
    {
      this.Top = top;
      this.Left = left;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.pnlBorder = new Panel();
      this.SuspendLayout();
      this.pnlBorder.BorderStyle = BorderStyle.Fixed3D;
      this.pnlBorder.Dock = DockStyle.Fill;
      this.pnlBorder.Location = new Point(0, 0);
      this.pnlBorder.Name = "pnlBorder";
      this.pnlBorder.Size = new Size(256, 193);
      this.pnlBorder.TabIndex = 0;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(256, 193);
      this.Controls.Add((Control) this.pnlBorder);
      this.FormBorderStyle = FormBorderStyle.None;
      this.Name = "PreviewWindow";
      this.ShowInTaskbar = false;
      this.Text = "PreviewWindow";
      this.TopMost = true;
      this.ResumeLayout(false);
    }
  }
}
