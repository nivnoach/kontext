using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Kontext.Items;
using Kontext.WindowKontext;

namespace Kontext.Forms
{
    public class EditKontext : Form
    {
        private readonly List<IntPtr> _excludedHandles;
        private readonly Kontext _kontext;
        private readonly WindowsPreview _previewer;
        private readonly Form _previewWindow = new PreviewWindow();
        private readonly ImageList icons = new ImageList();
        private Button btnCancel;
        private Button btnOk;
        private ColumnHeader clmAllKontextItems;
        private ColumnHeader clmName;
#pragma warning disable 649
        private IContainer components;
#pragma warning restore 649
        private Label label1;
        private Label lblKontextName;
        private ListView lstKontextItems;
        private ContextMenu selectOptionsMenu;
        private TextBox txtKontextName;

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

        public EditKontext(List<IntPtr> excludedHandles = null)
        {
            _excludedHandles = excludedHandles ?? new List<IntPtr>();
            InitializeComponent();
            _kontext = new Kontext(string.Empty);

            icons.Images.Clear();
            lstKontextItems.Items.Clear();

            // Enumerate the kontext items
            var kontextItems = KontextEnumerator.GetAllItems(new List<IntPtr>(_excludedHandles));
            if (kontextItems == null || !kontextItems.Any())
            {
                return;
            }

            // Regenerate the icons list
            lstKontextItems.SmallImageList = null;
            foreach (var ki in kontextItems)
            {
                var icon = ki.GetIcon();
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
            var itemAt = lstKontextItems.GetItemAt(e.X, e.Y);
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
            txtKontextName = new TextBox();
            lblKontextName = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            lstKontextItems = new ListView();
            clmAllKontextItems = new ColumnHeader();
            clmName = new ColumnHeader();
            label1 = new Label();
            SuspendLayout();
            // 
            // txtKontextName
            // 
            txtKontextName.Anchor = (AnchorStyles.Top | AnchorStyles.Left)
                                    | AnchorStyles.Right;
            txtKontextName.Location = new Point(101, 18);
            txtKontextName.Margin = new Padding(6);
            txtKontextName.Name = "txtKontextName";
            txtKontextName.Size = new Size(395, 33);
            txtKontextName.TabIndex = 1;
            txtKontextName.TextChanged += txtKontextName_TextChanged;
            // 
            // lblKontextName
            // 
            lblKontextName.AutoSize = true;
            lblKontextName.Location = new Point(21, 21);
            lblKontextName.Name = "lblKontextName";
            lblKontextName.Size = new Size(69, 26);
            lblKontextName.TabIndex = 2;
            lblKontextName.Text = "Name:";
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(371, 373);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(125, 41);
            btnOk.TabIndex = 3;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(235, 373);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(131, 41);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // lstKontextItems
            // 
            lstKontextItems.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom)
                                      | AnchorStyles.Left)
                                     | AnchorStyles.Right;
            lstKontextItems.CheckBoxes = true;
            lstKontextItems.Columns.AddRange(new[]
            {
                clmAllKontextItems
            });
            lstKontextItems.FullRowSelect = true;
            lstKontextItems.Location = new Point(26, 75);
            lstKontextItems.Name = "lstKontextItems";
            lstKontextItems.Size = new Size(470, 292);
            lstKontextItems.TabIndex = 5;
            lstKontextItems.UseCompatibleStateImageBehavior = false;
            lstKontextItems.View = View.Details;
            lstKontextItems.MouseDown += lstKontextItems_MouseDown;
            lstKontextItems.MouseLeave += lstKontextItems_MouseLeave;
            lstKontextItems.MouseMove += lstKontextItems_MouseMove;
            // 
            // clmAllKontextItems
            // 
            clmAllKontextItems.Text = "";
            clmAllKontextItems.Width = 439;
            // 
            // clmName
            // 
            clmName.Text = "Available Windows";
            clmName.Width = 438;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Calibri", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(29, 58);
            label1.Name = "label1";
            label1.Size = new Size(214, 14);
            label1.TabIndex = 2;
            label1.Text = "Right-click windows\' list for select options";
            // 
            // EditKontext
            // 
            ClientSize = new Size(520, 426);
            ControlBox = false;
            Controls.Add(lstKontextItems);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(label1);
            Controls.Add(lblKontextName);
            Controls.Add(txtKontextName);
            Font = new Font("Calibri", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(6);
            Name = "EditKontext";
            Text = "Edit Kontext";
            FormClosed += EditKontext_FormClosed;
            ResumeLayout(false);
            PerformLayout();
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
                    new MenuItem("Select All", (sndr, args) => CheckItems((item, list) => true))
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