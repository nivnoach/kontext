using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Kontext.Items;
using Kontext.Properties;
using Kontext.WindowKontext;

namespace Kontext.Forms
{
    public class Kontexts : Form
    {
        private const int WhKeyboardLl = 13;
        private const int WmKeydown = 256;
        private const int WmKeyup = 257;
        private static IntPtr _hhook = IntPtr.Zero;
        private static readonly List<Kontext> _kontexts = new List<Kontext>();
        private readonly Timer _fadeTimer = new Timer();
        private readonly List<PreviewContext> _previewWindows = new List<PreviewContext>();
        private bool _altPressed;
        private bool _ctrlPressed;
        private FadeDirection _fadeDirection;
        private Kontext _lastHoveredKontext;
        private readonly NotifyIcon _notifyIcon;
        private bool _shiftPressed;
        private ColumnHeader clmKontextName;
#pragma warning disable 649
        private IContainer components;
#pragma warning restore 649
        private ListView lstKontexts;
        private ColumnHeader clmKontexts;
        private PictureBox pictureBox1;

        public Kontexts()
        {
            InitializeComponent();
            _hhook = SetWindowsHookEx(13, HookProc, LoadLibrary("User32"), 0U);
            _fadeTimer.Enabled = true;
            _fadeTimer.Interval = 1;
            _fadeTimer.Tick += (EventHandler) ((sender, args) =>
            {
                Opacity += _fadeDirection == FadeDirection.FadeOut ? -0.1 : 0.1;
                if (_fadeDirection == FadeDirection.FadeOut && Opacity <= 0.0)
                {
                    _fadeTimer.Stop();
                    ClearPreviewedContext();
                    Hide();
                }
                else
                {
                    if (_fadeDirection != FadeDirection.FadeIn || Opacity < 1.0)
                        return;
                    _fadeTimer.Stop();
                }
            });
            ShowInTaskbar = false;
            _notifyIcon = new NotifyIcon
            {
                Icon = Resources.favicon,
                Visible = true,
                Text = Text,
                ContextMenu = new ContextMenu()
            };
            int num;
            _notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("About...",
                (a, b) => num = (int) new AboutWindow().ShowDialog()));
            _notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
            _notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("Exit", (a, b) => Application.Exit()));
        }

        public static List<Kontext> AllKontexts
        {
            get { return new List<Kontext>(_kontexts); }
        }

        public IEnumerable<Kontext> KontextList
        {
            get { return new List<Kontext>(_kontexts); }
        }

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance,
            uint threadId);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void MoveToForeground()
        {
            SetForegroundWindow(Handle);
        }

        public void AdjustToMonitor()
        {
            Location = new Point
            {
                Y = Screen.PrimaryScreen.WorkingArea.Left,
                X = Screen.PrimaryScreen.WorkingArea.Top
            };
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            Width = (int) (Screen.PrimaryScreen.WorkingArea.Width*0.2);
            lstKontexts.Columns[0].Width = lstKontexts.Width - 2;
        }

        private void UpdateKontextList()
        {
            lstKontexts.Items.Clear();
            lstKontexts.Items.AddRange(_kontexts.Select(k => new ListViewItem(k.Name)
            {
                Tag = (object) k
            }).ToArray().ToArray());
        }

        private void OpenKontextAddRemoveMenu(MouseEventArgs e)
        {
            var list = new List<MenuItem>();
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem("New Kontext...", (sender, args) =>
            {
                var editKontext =
                    new EditKontext(new List<IntPtr>(_previewWindows.Select(pw => pw.PreviewedWindow.Handle))
                    {
                        Handle
                    });
                if (editKontext.ShowDialog() != DialogResult.OK)
                    return;
                _kontexts.Add(editKontext.GetKontext());
                UpdateKontextList();
            }));
            var kontext = GetSelectedContext(e.Location, false);
            if (kontext != null)
            {
                contextMenu.MenuItems.Add(new MenuItem("-"));
                contextMenu.MenuItems.Add(new MenuItem(string.Format("Edit '{0}'...", kontext.Name),
                    (sender, args) =>
                        new EditKontext(kontext,
                            new List<IntPtr>(_previewWindows.Select(pw => pw.PreviewedWindow.Handle))
                            {
                                Handle
                            }).ShowDialog()));
                contextMenu.MenuItems.Add(new MenuItem(string.Format("Remove '{0}'", kontext.Name), (sender, args) =>
                {
                    _kontexts.Remove(kontext);
                    UpdateKontextList();
                }));
            }
            if (list.Any())
                contextMenu.MenuItems.Add(new MenuItem("-"));
            contextMenu.MenuItems.AddRange(list.ToArray());
            contextMenu.Show(this, new Point
            {
                X = e.X,
                Y = e.Y
            });
        }

        private void ShowSelectedKontext(MouseEventArgs e)
        {
            var kontext = GetSelectedContext(e.Location, false);
            if (kontext == null)
                return;
            KontextEnumerator.GetAllItems(new List<IntPtr> {Handle})
                .Where(k => !kontext.Items.Contains(k))
                .ToList()
                .ForEach(k => k.Hide(HideLevel.Accessible));
            kontext.ShowAll();
        }

        private Kontext GetSelectedContext(Point mousePoint, bool select = false)
        {
            var itemAt = lstKontexts.GetItemAt(mousePoint.X, mousePoint.Y);
            if (itemAt == null)
                return null;
            if (select)
            {
                itemAt.Selected = true;
                lstKontexts.Select();
                Application.DoEvents();
            }
            return itemAt.Tag as Kontext;
        }

        private void lstKontexts_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowSelectedKontext(e);
                MoveToForeground();
            }
            else
            {
                if (e.Button != MouseButtons.Right)
                    return;
                OpenKontextAddRemoveMenu(e);
            }
        }

        private IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0)
            {
                var num = Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr) 256)
                {
                    if (num.ToString(CultureInfo.CurrentCulture) == "164")
                        _altPressed = true;
                    if (num.ToString(CultureInfo.CurrentCulture) == "162")
                        _ctrlPressed = true;
                    if (num.ToString(CultureInfo.CurrentCulture) == "160")
                        _shiftPressed = true;
                }
                else if (wParam == (IntPtr) 257)
                {
                    if (num.ToString(CultureInfo.CurrentCulture) == "164")
                        _altPressed = false;
                    if (num.ToString(CultureInfo.CurrentCulture) == "162")
                        _ctrlPressed = false;
                    if (num.ToString(CultureInfo.CurrentCulture) == "160")
                        _shiftPressed = false;
                }
                var fadeDirection = _fadeDirection;
                _fadeDirection = !_shiftPressed || !_ctrlPressed || !_altPressed
                    ? FadeDirection.FadeOut
                    : FadeDirection.FadeIn;
                if (fadeDirection != _fadeDirection)
                {
                    // Let all reachable ("kontexted") items to refresh themselves
                    _kontexts.ForEach(k => k.Items.ForEach(ki => ki.RefreshState()));

                    // Fade the window in
                    Show();
                    _fadeTimer.Start();
                    if (_fadeDirection == FadeDirection.FadeIn)
                        MoveToForeground();
                }
            }
            return CallNextHookEx(_hhook, code, (int) wParam, lParam);
        }

        private void ClearPreviewedContext()
        {
            _lastHoveredKontext = null;
            _previewWindows.ForEach(w => w.PreviewedWindow.Hide());
            _previewWindows.ForEach(w =>
            {
                w.WindowsPreview.Dispose();
                w.PreviewedWindow.Dispose();
            });
            _previewWindows.Clear();
        }

        private void lstKontexts_MouseMove(object sender, MouseEventArgs e)
        {
            var itemAt = lstKontexts.GetItemAt(e.X, e.Y);
            if (itemAt == null)
            {
                ClearPreviewedContext();
            }
            else
            {
                var kontext = itemAt.Tag as Kontext;
                if (kontext == null)
                {
                    ClearPreviewedContext();
                }
                else
                {
                    if (kontext == _lastHoveredKontext)
                        return;
                    ClearPreviewedContext();
                    _lastHoveredKontext = kontext;
                    var num = Width + 10;
                    var top = Top;
                    var width = (int) (Screen.FromHandle(Handle).WorkingArea.Width*0.15);
                    foreach (var kontextItem in kontext.Items)
                    {
                        var previewWindow = new PreviewWindow();
                        var windowsPreview = new WindowsPreview(previewWindow.Handle, width);
                        windowsPreview.SetPreviewFor(kontextItem.GetHandle());
                        if (top + previewWindow.Height > Screen.PrimaryScreen.WorkingArea.Height)
                        {
                            top = Top;
                            num += width + 5;
                        }
                        previewWindow.Top = top;
                        previewWindow.Left = num;
                        top += previewWindow.Height + 5;
                        if (top > Screen.PrimaryScreen.WorkingArea.Height)
                        {
                            top = Top;
                            num += width + 5;
                        }
                        previewWindow.Show();
                        _previewWindows.Add(new PreviewContext
                        {
                            PreviewedWindow = previewWindow,
                            WindowsPreview = windowsPreview,
                            PreviewedKontext = kontextItem
                        });
                    }
                }
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
            this.lstKontexts = new System.Windows.Forms.ListView();
            this.clmKontexts = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmKontextName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lstKontexts
            // 
            this.lstKontexts.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstKontexts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstKontexts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstKontexts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmKontexts});
            this.lstKontexts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lstKontexts.Font = new System.Drawing.Font("DengXian", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstKontexts.FullRowSelect = true;
            this.lstKontexts.HotTracking = true;
            this.lstKontexts.HoverSelection = true;
            this.lstKontexts.Location = new System.Drawing.Point(12, 48);
            this.lstKontexts.MultiSelect = false;
            this.lstKontexts.Name = "lstKontexts";
            this.lstKontexts.Size = new System.Drawing.Size(308, 508);
            this.lstKontexts.TabIndex = 2;
            this.lstKontexts.UseCompatibleStateImageBehavior = false;
            this.lstKontexts.View = System.Windows.Forms.View.Details;
            this.lstKontexts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstKontexts_MouseDown);
            this.lstKontexts.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lstKontexts_MouseMove);
            // 
            // clmKontexts
            // 
            this.clmKontexts.Text = "Kontexts";
            // 
            // clmKontextName
            // 
            this.clmKontextName.Width = 312;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Kontext.Properties.Resources.icon_trans;
            this.pictureBox1.Location = new System.Drawing.Point(-2, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(322, 80);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // Kontexts
            // 
            this.ClientSize = new System.Drawing.Size(332, 568);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lstKontexts);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Kontexts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Kontexts";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        ///     Fade directory
        /// </summary>
        internal enum FadeDirection
        {
            FadeOut,
            FadeIn
        }

    }
}