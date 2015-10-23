using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Kontext.Items;

namespace Kontext
{
    public class EditKontext : Form
    {
        private readonly List<IntPtr> _excludedHandles;
        private readonly Kontext _kontext;
        private readonly Form _previewWindow = new PreviewWindow();
        private readonly WindowsPreview _previewer;
        private readonly ImageList icons = new ImageList();
        private Button btnCancel;
        private Button btnOk;
        private ColumnHeader clmName;
        private IContainer components;
        private Label lblKontextName;
        private ListView lstKontextItems;
        private ColumnHeader clmAllKontextItems;
        private Label label1;
        private TextBox txtKontextName;
        private ContextMenu selectOptionsMenu;

        public EditKontext(Kontext kontext, List<IntPtr> excludedHandles = null)
            : this(excludedHandles)
        {
            _kontext = kontext;
            lstKontextItems.Items.Cast<ListViewItem>()
                .ToList()
                .Where(lvi => _kontext.Items.Contains(lvi.Tag as KontextItem))
                .ToList()
                .ForEach(lvi => lvi.Checked = true);
            txtKontextName.Text = kontext.Name;
            txtKontextName.Enabled = false;
            btnOk.Enabled = true;
        }

        private void CheckItems(Func<KontextItem, List<Kontext>, bool> predicate)
        {
            foreach (ListViewItem lvi in lstKontextItems.Items)
            {
                // Get the item
                var ki = lvi.Tag as KontextItem;
                if (ki == null)
                {
                    continue;
                }

                // Get Containing Kontexts
                var containingKontexts = Kontexts.AllKontexts.Where(k => k.Items.Contains(ki)).ToList();

                // Call predicate
                lvi.Checked = predicate(ki, containingKontexts);
            }
        }

        public EditKontext(List<IntPtr> excludedHandles = null)
        {
            _excludedHandles = excludedHandles ?? new List<IntPtr>();
            InitializeComponent();
            _kontext = new Kontext(string.Empty);

            icons.Images.Clear();
            lstKontextItems.Items.Clear();

            // Enumerate the kontext items
            List<KontextItem> kontextItems = KontextEnumerator.GetAllItems(new List<IntPtr>(_excludedHandles));
            if (kontextItems == null || ! kontextItems.Any())
            {
                return;
            }

            // Regenerate the icons list
            lstKontextItems.SmallImageList = null;
            foreach (KontextItem ki in kontextItems)
            {
                Icon icon = ki.GetIcon();
                if (icon != null)
                    icons.Images.Add(ki.Name, icon.ToBitmap());
            }
            lstKontextItems.SmallImageList = icons;

            // Add all items
            lstKontextItems.Items.AddRange(
                kontextItems
                    .Select(ki => new ListViewItem(ki.Name)
                    {
                        Tag = (object) ki,
                        ImageKey = icons.Images.ContainsKey(ki.Name) ? ki.Name : string.Empty
                    }).ToArray());
            _previewer = new WindowsPreview(_previewWindow.Handle,
                (int) (Screen.FromHandle(Handle).WorkingArea.Width*0.2));
            btnOk.Enabled = false;
        }

        public Kontext GetKontext()
        {
            return _kontext;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _kontext.Name = txtKontextName.Text;
            _kontext.Items.Clear();
            foreach (ListViewItem listViewItem in lstKontextItems.Items)
            {
                if (listViewItem.Checked)
                    _kontext.Items.Add(listViewItem.Tag as KontextItem);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        private void lstKontextItems_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem itemAt = lstKontextItems.GetItemAt(e.X, e.Y);
            if (itemAt == null)
                return;
            var kontextItem = itemAt.Tag as KontextItem;
            if (kontextItem == null)
                return;
            _previewWindow.Top = Cursor.Position.Y;
            _previewWindow.Left = lstKontextItems.PointToScreen(Point.Empty).X + lstKontextItems.Width;
            _previewWindow.Show();
            SetActiveWindow(Handle);
            _previewer.SetPreviewFor(kontextItem.GetHandle());
        }

        private void EditKontext_FormClosed(object sender, FormClosedEventArgs e)
        {
            _previewer.Dispose();
            _previewWindow.Dispose();
        }

        private void lstKontextItems_MouseLeave(object sender, EventArgs e)
        {
            _previewWindow.Hide();
        }

        private void txtKontextName_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;
            btnOk.Enabled = !string.IsNullOrEmpty(textBox.Text);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtKontextName = new System.Windows.Forms.TextBox();
            this.lblKontextName = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lstKontextItems = new System.Windows.Forms.ListView();
            this.clmAllKontextItems = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtKontextName
            // 
            this.txtKontextName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKontextName.Location = new System.Drawing.Point(101, 18);
            this.txtKontextName.Margin = new System.Windows.Forms.Padding(6);
            this.txtKontextName.Name = "txtKontextName";
            this.txtKontextName.Size = new System.Drawing.Size(395, 33);
            this.txtKontextName.TabIndex = 1;
            this.txtKontextName.TextChanged += new System.EventHandler(this.txtKontextName_TextChanged);
            // 
            // lblKontextName
            // 
            this.lblKontextName.AutoSize = true;
            this.lblKontextName.Location = new System.Drawing.Point(21, 21);
            this.lblKontextName.Name = "lblKontextName";
            this.lblKontextName.Size = new System.Drawing.Size(69, 26);
            this.lblKontextName.TabIndex = 2;
            this.lblKontextName.Text = "Name:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(371, 373);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(125, 41);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(235, 373);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(131, 41);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lstKontextItems
            // 
            this.lstKontextItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstKontextItems.CheckBoxes = true;
            this.lstKontextItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmAllKontextItems});
            this.lstKontextItems.FullRowSelect = true;
            this.lstKontextItems.Location = new System.Drawing.Point(26, 75);
            this.lstKontextItems.Name = "lstKontextItems";
            this.lstKontextItems.Size = new System.Drawing.Size(470, 292);
            this.lstKontextItems.TabIndex = 5;
            this.lstKontextItems.UseCompatibleStateImageBehavior = false;
            this.lstKontextItems.View = System.Windows.Forms.View.Details;
            this.lstKontextItems.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstKontextItems_MouseDown);
            this.lstKontextItems.MouseLeave += new System.EventHandler(this.lstKontextItems_MouseLeave);
            this.lstKontextItems.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lstKontextItems_MouseMove);
            // 
            // clmAllKontextItems
            // 
            this.clmAllKontextItems.Text = "";
            this.clmAllKontextItems.Width = 439;
            // 
            // clmName
            // 
            this.clmName.Text = "Available Windows";
            this.clmName.Width = 438;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(29, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(214, 14);
            this.label1.TabIndex = 2;
            this.label1.Text = "Right-click windows\' list for select options";
            // 
            // EditKontext
            // 
            this.ClientSize = new System.Drawing.Size(520, 426);
            this.ControlBox = false;
            this.Controls.Add(this.lstKontextItems);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblKontextName);
            this.Controls.Add(this.txtKontextName);
            this.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "EditKontext";
            this.Text = "Edit Kontext";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EditKontext_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        /// <summary>
        ///     Close a window using ESC key
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lstKontextItems_MouseDown(object sender, MouseEventArgs e)
        {
            // Initialize the selection menu
            LazyInitializer.EnsureInitialized(ref selectOptionsMenu, () => new ContextMenu(
                new[]
                {
                    new MenuItem("Select None", (sndr, args) => CheckItems((item, list) => false)),

                    new MenuItem("Select Visible",
                        (sndr, args) =>
                            CheckItems((item, list) => item.GetVisibilityLevel() == VisibilityLevel.Visible)),
                    new MenuItem("Select All Non-Kontexted", (sndr, args) => CheckItems((item, list) =>
                        Kontexts.AllKontexts.All(k => !k.Items.Contains(item)))),
                    new MenuItem("Select All", (sndr, args) => CheckItems((item, list) => true)),
                }
                ));

            // Show selection options menu
            if (e.Button == MouseButtons.Right)
            {
                selectOptionsMenu.Show(lstKontextItems, e.Location);
            }
        }
    }
}