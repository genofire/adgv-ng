using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace ADGV
{
    public enum ADGVFilterMenuFilterType : byte
    {
        None = 0,
        Custom = 1,
        CheckList = 2,
        Loaded = 3
    }

    public enum ADGVFilterMenuSortType : byte
    {
        None = 0,
        ASC = 1,
        DESC = 2
    }

    public class ADGVFilterMenu : ContextMenuStrip
    {
        #region ToolStripMenuItems

        private ToolStripMenuItem SortASCMenuItem;
        private ToolStripMenuItem SortDESCMenuItem;
        private ToolStripMenuItem CancelSortMenuItem;
        private ToolStripSeparator toolStripSeparator1MenuItem;
        private ToolStripMenuItem CancelFilterMenuItem;
        private ToolStripMenuItem FiltersMenuItem;
        private ToolStripMenuItem SetupFilterMenuItem;
        private ToolStripSeparator toolStripSeparator2MenuItem;
        private ToolStripSeparator toolStripSeparator3MenuItem;
        private ToolStripMenuItem lastfilter1MenuItem;
        private ToolStripMenuItem lastfilter2MenuItem;
        private ToolStripMenuItem lastfilter3MenuItem;
        private ToolStripMenuItem lastfilter4MenuItem;
        private ToolStripMenuItem lastfilter5MenuItem;
        private TreeView CheckList;
        private Button okButton;
        private Button cancelButton;
        private ToolStripControlHost CheckFilterListControlHost;
        private ToolStripControlHost CheckFilterListButtonsControlHost;
        private ToolStripControlHost ResizeBoxControlHost;
        private Panel CheckFilterListPanel;
        private Panel CheckFilterListButtonsPanel;

        #endregion ToolStripMenuItems

        private Dictionary<Int32, String> months = null;
        private ResourceManager RM = null;
        private ADGVFilterMenuFilterType activeFilterType = ADGVFilterMenuFilterType.None;
        private ADGVFilterMenuSortType activeSortType = ADGVFilterMenuSortType.None;
        private String sortString = null;
        private String filterString = null;
        private TripleTreeNode[] startingNodes = null;
        private TripleTreeNode[] filterNodes = null;
        private static Point resizeStartPoint = new Point(1, 1);
        private Point resizeEndPoint = new Point(-1, -1);

        public Type DataType { get; private set; }

        public Boolean DateWithTime { get; set; }

        public Boolean TimeFilter { get; set; }

        public ADGVFilterMenuSortType ActiveSortType
        {
            get
            {
                return this.activeSortType;
            }
        }

        public ADGVFilterMenuFilterType ActiveFilterType
        {
            get
            {
                return this.activeFilterType;
            }
        }

        public String SortString
        {
            get
            {
                return this.sortString == null ? "" : this.sortString;
            }
            private set
            {
                this.CancelSortMenuItem.Enabled = (value != null && value.Length > 0);
                this.sortString = value;
            }
        }

        public String FilterString
        {
            get
            {
                return this.filterString == null ? "" : this.filterString;
            }

            private set
            {
                this.CancelFilterMenuItem.Enabled = (value != null && value.Length > 0);
                this.filterString = value;
            }
        }

        public event EventHandler SortChanged;

        public event EventHandler FilterChanged;

        public ADGVFilterMenu(Type DataType)
            : base()
        {
            this.DataType = DataType;
            this.DateWithTime = true;
            this.TimeFilter = false;

            this.RM = new System.Resources.ResourceManager("ADGV.Localization.ADGVStrings", typeof(ADGV.ADGVFilterMenu).Assembly);
            this.months = new Dictionary<Int32, String>();
            this.months.Add(1, this.RM.GetString("month1"));
            this.months.Add(2, this.RM.GetString("month2"));
            this.months.Add(3, this.RM.GetString("month3"));
            this.months.Add(4, this.RM.GetString("month4"));
            this.months.Add(5, this.RM.GetString("month5"));
            this.months.Add(6, this.RM.GetString("month6"));
            this.months.Add(7, this.RM.GetString("month7"));
            this.months.Add(8, this.RM.GetString("month8"));
            this.months.Add(9, this.RM.GetString("month9"));
            this.months.Add(10, this.RM.GetString("month10"));
            this.months.Add(11, this.RM.GetString("month11"));
            this.months.Add(12, this.RM.GetString("month12"));

            #region Interface

            this.SortASCMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SortDESCMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CancelSortMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1MenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.CancelFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FiltersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetupFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2MenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.lastfilter1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lastfilter2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lastfilter3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lastfilter4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lastfilter5MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3MenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.CheckList = new TreeView();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.CheckFilterListPanel = new Panel();
            this.CheckFilterListButtonsPanel = new Panel();
            this.CheckFilterListButtonsControlHost = new ToolStripControlHost(this.CheckFilterListButtonsPanel);
            this.CheckFilterListControlHost = new ToolStripControlHost(this.CheckFilterListPanel);
            this.ResizeBoxControlHost = new ToolStripControlHost(new Control());//(this.ResizePictureBox);
            this.SuspendLayout();
            //
            // MenuStrip
            //
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.AutoSize = false;
            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
            this.Size = new System.Drawing.Size(287, 340);
            this.Closed += new ToolStripDropDownClosedEventHandler(FilterContextMenu_Closed);
            this.LostFocus += new EventHandler(FilterContextMenu_LostFocus);
            //
            // SortASCMenuItem
            //
            this.SortASCMenuItem.Name = "SortASCMenuItem";
            this.SortASCMenuItem.AutoSize = false;
            this.SortASCMenuItem.Size = new System.Drawing.Size(this.Width - 1, 22);
            this.SortASCMenuItem.Click += new System.EventHandler(this.SortASCMenuItem_Click);
            this.SortASCMenuItem.MouseEnter += new System.EventHandler(this.SortASCMenuItem_MouseEnter);
            this.SortASCMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            //
            // SortDESCMenuItem
            //
            this.SortDESCMenuItem.Name = "SortDESCMenuItem";
            this.SortDESCMenuItem.AutoSize = false;
            this.SortDESCMenuItem.Size = new System.Drawing.Size(this.Width - 1, 22);
            this.SortDESCMenuItem.Click += new System.EventHandler(this.SortDESCMenuItem_Click);
            this.SortDESCMenuItem.MouseEnter += new System.EventHandler(this.SortASCMenuItem_MouseEnter);
            this.SortDESCMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            //
            // CancelSortMenuItem
            //
            this.CancelSortMenuItem.Name = "CancelSortMenuItem";
            this.CancelSortMenuItem.Enabled = false;
            this.CancelSortMenuItem.AutoSize = false;
            this.CancelSortMenuItem.Size = new System.Drawing.Size(this.Width - 1, 22);
            this.CancelSortMenuItem.Text = this.RM.GetString("cancelsortmenuitem_text");
            this.CancelSortMenuItem.Click += new System.EventHandler(this.CancelSortMenuItem_Click);
            this.CancelSortMenuItem.MouseEnter += new System.EventHandler(this.SortASCMenuItem_MouseEnter);
            //
            // toolStripSeparator1MenuItem
            //
            this.toolStripSeparator1MenuItem.Name = "toolStripSeparator1MenuItem";
            this.toolStripSeparator1MenuItem.Size = new System.Drawing.Size(this.Width - 4, 6);
            //
            // CancelFilterMenuItem
            //
            this.CancelFilterMenuItem.Name = "CancelFilterMenuItem";
            this.CancelFilterMenuItem.Enabled = false;
            this.CancelFilterMenuItem.AutoSize = false;
            this.CancelFilterMenuItem.Size = new System.Drawing.Size(this.Width - 1, 22);
            this.CancelFilterMenuItem.Text = this.RM.GetString("cancelfiltermenuitem_text");
            this.CancelFilterMenuItem.Click += new System.EventHandler(this.CancelFilterMenuItem_Click);
            this.CancelFilterMenuItem.MouseEnter += new System.EventHandler(this.SortASCMenuItem_MouseEnter);
            //
            // SetupFilterMenuItem
            //
            this.SetupFilterMenuItem.Name = "SetupFilterMenuItem";
            this.SetupFilterMenuItem.Size = new System.Drawing.Size(152, 22);
            this.SetupFilterMenuItem.Text = this.RM.GetString("setupfiltermenuitem_text");
            this.SetupFilterMenuItem.Click += new System.EventHandler(this.SetupFilterMenuItem_Click);
            //
            // toolStripMenuItem2
            //
            this.toolStripSeparator2MenuItem.Name = "toolStripSeparator2MenuItem";
            this.toolStripSeparator2MenuItem.Size = new System.Drawing.Size(149, 6);
            this.toolStripSeparator2MenuItem.Visible = false;
            //
            // lastfilter1MenuItem
            //
            this.lastfilter1MenuItem.Name = "lastfilter1MenuItem";
            this.lastfilter1MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter1MenuItem.Tag = "0";
            this.lastfilter1MenuItem.Text = null;
            this.lastfilter1MenuItem.Visible = false;
            this.lastfilter1MenuItem.Click += new System.EventHandler(this.lastfilter1MenuItem_Click);
            this.lastfilter1MenuItem.TextChanged += new System.EventHandler(this.lastfilter1MenuItem_TextChanged);
            this.lastfilter1MenuItem.VisibleChanged += new System.EventHandler(this.lastfilter1MenuItem_VisibleChanged);
            //
            // lastfilter2MenuItem
            //
            this.lastfilter2MenuItem.Name = "lastfilter2MenuItem";
            this.lastfilter2MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter2MenuItem.Tag = "1";
            this.lastfilter2MenuItem.Text = null;
            this.lastfilter2MenuItem.Visible = false;
            this.lastfilter2MenuItem.Click += new System.EventHandler(this.lastfilter1MenuItem_Click);
            this.lastfilter2MenuItem.TextChanged += new System.EventHandler(this.lastfilter1MenuItem_TextChanged);
            //
            // lastfilter3MenuItem
            //
            this.lastfilter3MenuItem.Name = "lastfilter3MenuItem";
            this.lastfilter3MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter3MenuItem.Tag = "2";
            this.lastfilter3MenuItem.Text = null;
            this.lastfilter3MenuItem.Visible = false;
            this.lastfilter3MenuItem.Click += new System.EventHandler(this.lastfilter1MenuItem_Click);
            this.lastfilter3MenuItem.TextChanged += new System.EventHandler(this.lastfilter1MenuItem_TextChanged);
            //
            // lastfilter4MenuItem
            //
            this.lastfilter4MenuItem.Name = "lastfilter4MenuItem";
            this.lastfilter4MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter4MenuItem.Tag = "3";
            this.lastfilter4MenuItem.Text = null;
            this.lastfilter4MenuItem.Visible = false;
            this.lastfilter4MenuItem.Click += new System.EventHandler(this.lastfilter1MenuItem_Click);
            this.lastfilter4MenuItem.TextChanged += new System.EventHandler(this.lastfilter1MenuItem_TextChanged);
            //
            // lastfilter5MenuItem
            //
            this.lastfilter5MenuItem.Name = "lastfilter5MenuItem";
            this.lastfilter5MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter5MenuItem.Tag = "4";
            this.lastfilter5MenuItem.Text = null;
            this.lastfilter5MenuItem.Visible = false;
            this.lastfilter5MenuItem.Click += new System.EventHandler(this.lastfilter1MenuItem_Click);
            this.lastfilter5MenuItem.TextChanged += new System.EventHandler(this.lastfilter1MenuItem_TextChanged);
            //
            // FiltersMenuItem
            //
            this.FiltersMenuItem.Name = "FiltersMenuItem";
            this.FiltersMenuItem.AutoSize = false;
            this.FiltersMenuItem.Size = new System.Drawing.Size(this.Width - 1, 22);
            this.FiltersMenuItem.Image = Properties.Resources.Filter;
            this.FiltersMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            this.FiltersMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SetupFilterMenuItem,
            this.toolStripSeparator2MenuItem,
            this.lastfilter1MenuItem,
            this.lastfilter2MenuItem,
            this.lastfilter3MenuItem,
            this.lastfilter4MenuItem,
            this.lastfilter5MenuItem});
            this.FiltersMenuItem.MouseEnter += new System.EventHandler(this.SortASCMenuItem_MouseEnter);
            this.FiltersMenuItem.Paint += new PaintEventHandler(FiltersMenuItem_Paint);
            //
            // toolStripMenuItem3
            //
            this.toolStripSeparator3MenuItem.Name = "toolStripSeparator3MenuItem";
            this.toolStripSeparator3MenuItem.Size = new System.Drawing.Size(this.Width - 4, 6);
            //
            // okButton
            //
            this.okButton.Name = "okButton";
            this.okButton.BackColor = Button.DefaultBackColor;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Margin = new Padding(0);
            this.okButton.Size = new Size(75, 23);
            this.okButton.Text = this.RM.GetString("okbutton_text");
            this.okButton.Click += new EventHandler(okButton_Click);
            //
            // cancelButton
            //
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.BackColor = Button.DefaultBackColor;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Margin = new Padding(0);
            this.cancelButton.Size = new Size(75, 23);
            this.cancelButton.Text = this.RM.GetString("cancelbutton_text");
            this.cancelButton.Click += new EventHandler(cancelButton_Click);
            //
            // ResizeBoxControlHost
            //
            this.ResizeBoxControlHost.Name = "ResizeBoxControlHost";
            this.ResizeBoxControlHost.Control.Cursor = Cursors.SizeNWSE;
            this.ResizeBoxControlHost.AutoSize = false;
            this.ResizeBoxControlHost.Padding = new Padding(0);
            this.ResizeBoxControlHost.Margin = new Padding(this.Width - 45, 0, 0, 0);
            this.ResizeBoxControlHost.Size = new System.Drawing.Size(10, 10);
            this.ResizeBoxControlHost.Paint += new PaintEventHandler(ResizeGrip_Paint);
            this.ResizeBoxControlHost.MouseDown += new MouseEventHandler(ResizePictureBox_MouseDown);
            this.ResizeBoxControlHost.MouseUp += new MouseEventHandler(ResizePictureBox_MouseUp);
            this.ResizeBoxControlHost.MouseMove += new MouseEventHandler(ResizePictureBox_MouseMove);
            //
            // CheckFilterListControlHost
            //
            this.CheckFilterListControlHost.Name = "CheckFilterListControlHost";
            this.CheckFilterListControlHost.AutoSize = false;
            this.CheckFilterListControlHost.Size = new System.Drawing.Size(this.Width - 35, 180);
            this.CheckFilterListControlHost.Padding = new Padding(0);
            this.CheckFilterListControlHost.Margin = new Padding(0);
            //
            // CheckFilterListButtonsControlHost
            //
            this.CheckFilterListButtonsControlHost.Name = "CheckFilterListButtonsControlHost";
            this.CheckFilterListButtonsControlHost.AutoSize = false;
            this.CheckFilterListButtonsControlHost.Size = new System.Drawing.Size(this.Width - 35, 24);
            this.CheckFilterListButtonsControlHost.Padding = new Padding(0);
            this.CheckFilterListButtonsControlHost.Margin = new Padding(0);
            //
            // CheckFilterListPanel
            //
            this.CheckFilterListPanel.Name = "CheckFilterListPanel";
            this.CheckFilterListPanel.AutoSize = false;
            this.CheckFilterListPanel.Size = this.CheckFilterListControlHost.Size;
            this.CheckFilterListPanel.Padding = new Padding(0);
            this.CheckFilterListPanel.Margin = new Padding(0);
            this.CheckFilterListPanel.BackColor = this.BackColor;
            this.CheckFilterListPanel.BorderStyle = BorderStyle.None;
            this.CheckFilterListPanel.Controls.Add(this.CheckList);
            //
            // CheckList
            //
            this.CheckList.Name = "CheckList";
            this.CheckList.AutoSize = false;
            this.CheckList.Padding = new Padding(0);
            this.CheckList.Margin = new Padding(0);
            this.CheckList.Bounds = new Rectangle(4, 4, this.CheckFilterListPanel.Width - 8, this.CheckFilterListPanel.Height - 8);
            this.CheckList.StateImageList = GetCheckImages();
            this.CheckList.CheckBoxes = false;
            this.CheckList.MouseLeave += new EventHandler(CheckList_MouseLeave);
            this.CheckList.NodeMouseClick += new TreeNodeMouseClickEventHandler(CheckList_NodeMouseClick);
            this.CheckList.KeyDown += new KeyEventHandler(CheckList_KeyDown);
            this.CheckList.MouseEnter += CheckList_MouseEnter;
            this.CheckList.NodeMouseDoubleClick += CheckList_NodeMouseDoubleClick;
            //
            // CheckFilterListButtonsPanel
            //
            this.CheckFilterListButtonsPanel.Name = "CheckFilterListButtonsPanel";
            this.CheckFilterListButtonsPanel.AutoSize = false;
            this.CheckFilterListButtonsPanel.Size = this.CheckFilterListButtonsControlHost.Size;
            this.CheckFilterListButtonsPanel.Padding = new Padding(0);
            this.CheckFilterListButtonsPanel.Margin = new Padding(0);
            this.CheckFilterListButtonsPanel.BackColor = this.BackColor;
            this.CheckFilterListButtonsPanel.BorderStyle = BorderStyle.None;
            this.CheckFilterListButtonsPanel.Controls.AddRange(new Control[] {
            this.okButton,
            this.cancelButton
        });
            this.okButton.Location = new Point(this.CheckFilterListButtonsPanel.Width - 164, 0);
            this.cancelButton.Location = new Point(this.CheckFilterListButtonsPanel.Width - 79, 0);

            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SortASCMenuItem,
            this.SortDESCMenuItem,
            this.CancelSortMenuItem,
            this.toolStripSeparator1MenuItem,
            this.CancelFilterMenuItem,
            this.FiltersMenuItem,
            this.toolStripSeparator3MenuItem,
            this.CheckFilterListControlHost,
            this.CheckFilterListButtonsControlHost,
            this.ResizeBoxControlHost});

            this.ResumeLayout(false);

            #endregion Interface

            if (this.DataType == typeof(DateTime))
            {
                this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_datetime");
                this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_datetime");
                this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text_datetime");
                this.SortASCMenuItem.Image = Properties.Resources.ASCnum;
                this.SortDESCMenuItem.Image = Properties.Resources.DESCnum;
            }
            else if (this.DataType == typeof(Boolean))
            {
                this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_text");
                this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_boolean");
                this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text_boolean");
                this.SortASCMenuItem.Image = Properties.Resources.ASCbool;
                this.SortDESCMenuItem.Image = Properties.Resources.DESCbool;
            }
            else if (this.DataType == typeof(Int32) || this.DataType == typeof(Int64) || this.DataType == typeof(Int16) ||
                this.DataType == typeof(UInt32) || this.DataType == typeof(UInt64) || this.DataType == typeof(UInt16) ||
                this.DataType == typeof(Byte) || this.DataType == typeof(SByte) || this.DataType == typeof(Decimal) ||
                this.DataType == typeof(Single) || this.DataType == typeof(Double))
            {
                this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_numeric");
                this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_numeric");
                this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text_numeric");
                this.SortASCMenuItem.Image = Properties.Resources.ASCnum;
                this.SortDESCMenuItem.Image = Properties.Resources.DESCnum;
            }
            else
            {
                this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_text");
                this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_text");
                this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text");
                this.SortASCMenuItem.Image = Properties.Resources.ASCtxt;
                this.SortDESCMenuItem.Image = Properties.Resources.DESCtxt;
            }

            this.FiltersMenuItem.Enabled = this.DataType != typeof(Boolean);
            this.FiltersMenuItem.Checked = this.ActiveFilterType == ADGVFilterMenuFilterType.Custom;
            this.MinimumSize = new Size(this.PreferredSize.Width, this.PreferredSize.Height);
            this.ResizeMenu(this.MinimumSize.Width, this.MinimumSize.Height);
        }

        private void CheckList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TripleTreeNode n = e.Node as TripleTreeNode;
            SetNodesCheckedState(this.CheckList.Nodes, false);
            n.CheckState = CheckState.Unchecked;
            NodeCheckChange(n);
            this.okButton_Click(this, new EventArgs());
        }

        private void CheckList_MouseEnter(object sender, EventArgs e)
        {
            this.CheckList.Focus();
        }

        private ImageList GetCheckImages()
        {
            ImageList images = new System.Windows.Forms.ImageList();
            Bitmap unCheckImg = new Bitmap(16, 16);
            Bitmap checkImg = new Bitmap(16, 16);
            Bitmap mixedImg = new Bitmap(16, 16);

            using (Bitmap img = new Bitmap(16, 16))
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                    unCheckImg = (Bitmap)img.Clone();
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
                    checkImg = (Bitmap)img.Clone();
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
                    mixedImg = (Bitmap)img.Clone();
                }
            }

            images.Images.Add("uncheck", unCheckImg);
            images.Images.Add("check", checkImg);
            images.Images.Add("mixed", mixedImg);

            return images;
        }

        public static IEnumerable<DataGridViewCell> GetValuesForFilter(DataGridView grid, String columnName)
        {
            var vals =
                from DataGridViewRow r in grid.Rows
                where r.IsNewRow == false
                select r.Cells[columnName];

            return vals;
        }

        #region Show Methods

        public void Show(Control control, int x, int y, IEnumerable<DataGridViewCell> vals)
        {
            RefreshFilterMenu(vals);
            if (this.activeFilterType == ADGVFilterMenuFilterType.Custom)
                SetNodesCheckedState(this.CheckList.Nodes, false);
            DuplicateNodes();
            base.Show(control, x, y);
        }

        public void Show(Control control, int x, int y, Boolean RestoreFilter)
        {
            if (RestoreFilter)
                RestoreFilterNodes();
            DuplicateNodes();
            base.Show(control, x, y);
        }

        private void RefreshFilterMenu(IEnumerable<DataGridViewCell> vals)
        {
            this.CheckList.BeginUpdate();
            this.CheckList.Nodes.Clear();

            if (vals != null)
            {
                TripleTreeNode allnode = TripleTreeNode.CreateAllsNode(this.RM.GetString("tripletreenode_allnode_text") + "            ");
                allnode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                this.CheckList.Nodes.Add(allnode);

                if (vals.Count() > 0)
                {
                    var nonulls = vals.Where<DataGridViewCell>(c => c.Value != null && c.Value != DBNull.Value);

                    if (vals.Count() != nonulls.Count())
                    {
                        TripleTreeNode nullnode = TripleTreeNode.CreateEmptysNode(this.RM.GetString("tripletreenode_nullnode_text") + "               ");
                        nullnode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                        this.CheckList.Nodes.Add(nullnode);
                    }

                    #region Datetime

                    if (this.DataType == typeof(DateTime))
                    {
                        var years =
                            from year in nonulls
                            group year by ((DateTime)year.Value).Year into y
                            orderby y.Key ascending
                            select y;

                        foreach (var y in years)
                        {
                            TripleTreeNode yearnode = TripleTreeNode.CreateYearNode(y.Key.ToString(), y.Key);
                            this.CheckList.Nodes.Add(yearnode);

                            var months =
                                from month in y
                                group month by ((DateTime)month.Value).Month into m
                                orderby m.Key ascending
                                select m;

                            foreach (var m in months)
                            {
                                TripleTreeNode monthnode = yearnode.CreateChildNode(this.months[m.Key], m.Key);

                                var days =
                                    from day in m
                                    group day by ((DateTime)day.Value).Day into d
                                    orderby d.Key ascending
                                    select d;

                                foreach (var d in days)
                                {
                                    TripleTreeNode daysnode;

                                    if (!this.TimeFilter)
                                        daysnode = monthnode.CreateChildNode(d.Key.ToString("D2"), d.First().Value);
                                    else
                                    {
                                        if (!this.DateWithTime)
                                        {
                                            daysnode = monthnode.CreateChildNode(d.Key.ToString("D2"), d.First().Value);
                                            TripleTreeNode hoursnode = daysnode.CreateChildNode("## " + this.RM.GetString("checknodetree_hour"), null);
                                            TripleTreeNode minsnode = hoursnode.CreateChildNode("## " + this.RM.GetString("checknodetree_minute"), null);
                                            TripleTreeNode secsnode = minsnode.CreateChildNode("## " + this.RM.GetString("checknodetree_second"), null);
                                            TripleTreeNode msecsnode = secsnode.CreateChildNode("### " + this.RM.GetString("checknodetree_millisecond"), null);
                                        }
                                        else
                                        {
                                            daysnode = monthnode.CreateChildNode(d.Key.ToString("D2"), d.Key);

                                            var hours =
                                                from hour in d
                                                group hour by ((DateTime)hour.Value).Hour into h
                                                orderby h.Key ascending
                                                select h;

                                            foreach (var h in hours)
                                            {
                                                TripleTreeNode hoursnode = daysnode.CreateChildNode(h.Key.ToString("D2") + " " + this.RM.GetString("checknodetree_hour"), h.Key);

                                                var mins =
                                                    from min in h
                                                    group min by ((DateTime)min.Value).Minute into mn
                                                    orderby mn.Key ascending
                                                    select mn;

                                                foreach (var mn in mins)
                                                {
                                                    TripleTreeNode minsnode = hoursnode.CreateChildNode(mn.Key.ToString("D2") + " " + this.RM.GetString("checknodetree_minute"), mn.Key);

                                                    var secs =
                                                        from sec in mn
                                                        group sec by ((DateTime)sec.Value).Second into s
                                                        orderby s.Key ascending
                                                        select s;

                                                    foreach (var s in secs)
                                                    {
                                                        TripleTreeNode secsnode = minsnode.CreateChildNode(s.Key.ToString("D2") + " " + this.RM.GetString("checknodetree_second"), s.Key);

                                                        var msecs =
                                                            from msec in s
                                                            group msec by ((DateTime)msec.Value).Millisecond into ms
                                                            orderby ms.Key ascending
                                                            select ms;

                                                        foreach (var ms in msecs)
                                                        {
                                                            TripleTreeNode msecsnode = secsnode.CreateChildNode(ms.Key.ToString("D3") + " " + this.RM.GetString("checknodetree_millisecond"), ms.First().Value);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion Datetime

                    #region Boolean

                    else if (this.DataType == typeof(Boolean))
                    {
                        var values = nonulls.Where<DataGridViewCell>(c => (Boolean)c.Value == true);

                        if (values.Count() != nonulls.Count())
                        {
                            TripleTreeNode node = TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_false"), false);
                            this.CheckList.Nodes.Add(node);
                        }

                        if (values.Count() > 0)
                        {
                            TripleTreeNode node = TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_true"), true);
                            this.CheckList.Nodes.Add(node);
                        }
                    }

                    #endregion Boolean

                    #region String

                    else
                    {
                        foreach (var v in nonulls.GroupBy(c => c.Value).OrderBy(g => g.Key))
                        {
                            TripleTreeNode node = TripleTreeNode.CreateNode(v.First().FormattedValue.ToString(), v.Key);
                            this.CheckList.Nodes.Add(node);
                        }
                    }

                    #endregion String
                }
            }

            this.CheckList.EndUpdate();
        }

        #endregion Show Methods

        #region CheckList

        private void DuplicateNodes()
        {
            this.startingNodes = new TripleTreeNode[this.CheckList.Nodes.Count];
            Int32 i = 0;
            foreach (TripleTreeNode n in this.CheckList.Nodes)
            {
                this.startingNodes[i] = n.Clone();
                i++;
            }
        }

        private void DuplicateFilterNodes()
        {
            this.filterNodes = new TripleTreeNode[this.CheckList.Nodes.Count];
            int i = 0;
            foreach (TripleTreeNode n in this.CheckList.Nodes)
            {
                this.filterNodes[i] = n.Clone();
                i++;
            }
        }

        private void RestoreNodes()
        {
            this.CheckList.Nodes.Clear();
            if (startingNodes != null)
                this.CheckList.Nodes.AddRange(this.startingNodes);
        }

        private void RestoreFilterNodes()
        {
            this.CheckList.Nodes.Clear();
            if (filterNodes != null)
                this.CheckList.Nodes.AddRange(this.filterNodes);
        }

        private TripleTreeNode GetAllsNode()
        {
            TripleTreeNode result = null;
            Int32 i = 0;
            foreach (TripleTreeNode n in this.CheckList.Nodes)
            {
                if (n.NodeType == TripleTreeNodeType.AllsNode)
                {
                    result = n;
                    break;
                }
                else if (i > 2)
                    break;
                else
                    i++;
            }

            return result;
        }

        private TripleTreeNode GetNullNode()
        {
            TripleTreeNode result = null;
            Int32 i = 0;
            foreach (TripleTreeNode n in this.CheckList.Nodes)
            {
                if (n.NodeType == TripleTreeNodeType.EmptysNode)
                {
                    result = n;
                    break;
                }
                else if (i > 2)
                    break;
                else
                    i++;
            }

            return result;
        }

        private void SetNodesCheckedState(TreeNodeCollection nodes, Boolean isChecked)
        {
            foreach (TripleTreeNode node in nodes)
            {
                node.Checked = isChecked;
                if (node.Nodes != null && node.Nodes.Count > 0)
                    SetNodesCheckedState(node.Nodes, isChecked);
            }
        }

        private CheckState UpdateNodesCheckState(TreeNodeCollection nodes)
        {
            CheckState result = CheckState.Unchecked;
            Boolean isFirstNode = true;
            Boolean isAllNodesSomeCheckState = true;

            foreach (TripleTreeNode n in nodes)
            {
                if (n.NodeType == TripleTreeNodeType.AllsNode)
                    continue;

                if (n.Nodes.Count > 0)
                {
                    n.CheckState = UpdateNodesCheckState(n.Nodes);
                }

                if (isFirstNode)
                {
                    result = n.CheckState;
                    isFirstNode = false;
                }
                else
                    if (result != n.CheckState)
                        isAllNodesSomeCheckState = false;
            }

            if (isAllNodesSomeCheckState)
                return result;
            else
                return CheckState.Indeterminate;
        }

        private void RefreshNodesState()
        {
            CheckState state = UpdateNodesCheckState(this.CheckList.Nodes);

            GetAllsNode().CheckState = state;
            this.okButton.Enabled = !(state == CheckState.Unchecked);
        }

        private void CheckList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeViewHitTestInfo HitTestInfo = this.CheckList.HitTest(e.X, e.Y);
            if (HitTestInfo != null && HitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                NodeCheckChange(e.Node as TripleTreeNode);
        }

        private void CheckList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                NodeCheckChange(this.CheckList.SelectedNode as TripleTreeNode);
        }

        private void NodeCheckChange(TripleTreeNode node)
        {
            if (node.CheckState == CheckState.Checked)
                node.CheckState = CheckState.Unchecked;
            else
                node.CheckState = CheckState.Checked;

            if (node.NodeType == TripleTreeNodeType.AllsNode)
            {
                SetNodesCheckedState(this.CheckList.Nodes, node.Checked);
                this.okButton.Enabled = node.Checked;
            }
            else
            {
                if (node.Nodes.Count > 0)
                {
                    SetNodesCheckedState(node.Nodes, node.Checked);
                }

                RefreshNodesState();
            }
        }

        private String CreateFilterString(IEnumerable<TripleTreeNode> nodes)
        {
            StringBuilder sb = new StringBuilder("");
            String appx = this.DataType == typeof(DateTime) && !this.TimeFilter && this.DateWithTime ? " OR " : ", ";

            if (nodes != null && nodes.Count() > 0)
            {
                if (this.DataType == typeof(DateTime))
                {
                    foreach (TripleTreeNode n in nodes)
                    {
                        if (n.Checked && (
                                (n.NodeType == TripleTreeNodeType.DayDateTimeNode && (!this.TimeFilter || !this.DateWithTime)) ||
                                (n.NodeType == TripleTreeNodeType.MSecDateTimeNode && this.TimeFilter && this.DateWithTime)
                            ))
                        {
                            if (this.TimeFilter && this.DateWithTime)
                                sb.Append("'" + ((DateTime)n.Value).ToString("o") + "'" + appx);
                            else if (!this.TimeFilter && this.DateWithTime)
                                sb.Append("(Convert([{0}], System.String) LIKE '" + ((DateTime)n.Value).ToShortDateString() + "%')" + appx);
                            else
                                sb.Append("'" + ((DateTime)n.Value).ToShortDateString() + "'" + appx);
                        }
                        else if (n.CheckState != CheckState.Unchecked && n.Nodes.Count > 0)
                        {
                            String subnode = CreateFilterString(n.Nodes.AsParallel().Cast<TripleTreeNode>().Where(sn => sn.CheckState != CheckState.Unchecked));
                            if (subnode.Length > 0)
                                sb.Append(subnode + appx);
                        }
                    }
                }
                else if (this.DataType == typeof(Boolean))
                {
                    foreach (TripleTreeNode n in nodes)
                    {
                        sb.Append(n.Value.ToString());
                        break;
                    }
                }
                else if (this.DataType == typeof(Int32) || this.DataType == typeof(Int64) || this.DataType == typeof(Int16) ||
                    this.DataType == typeof(UInt32) || this.DataType == typeof(UInt64) || this.DataType == typeof(UInt16) ||
                    this.DataType == typeof(Byte) || this.DataType == typeof(SByte))
                {
                    foreach (TripleTreeNode n in nodes)
                        sb.Append(n.Value.ToString() + appx);
                }
                else if (this.DataType == typeof(Single) || this.DataType == typeof(Double) || this.DataType == typeof(Decimal))
                {
                    foreach (TripleTreeNode n in nodes)
                        sb.Append(n.Value.ToString().Replace(",", ".") + appx);
                }
                else
                {
                    foreach (TripleTreeNode n in nodes)
                        sb.Append("'" + this.FormatString(n.Value.ToString()) + "'" + appx);
                }
            }

            if (sb.Length > appx.Length && this.DataType != typeof(Boolean))
                sb.Remove(sb.Length - appx.Length, appx.Length);

            return sb.ToString();
        }

        private String FormatString(String Text)
        {
            String result = "";
            String s;
            String[] replace = { "%", "[", "]", "*", "\"", "`", "\\" };

            for (Int32 i = 0; i < Text.Length; i++)
            {
                s = Text[i].ToString();
                if (replace.Contains(s))
                    result += "[" + s + "]";
                else
                    result += s;
            }

            return result.Replace("'", "''");
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            TripleTreeNode NullorALLNode = GetAllsNode();
            this.FiltersMenuItem.Checked = false;

            if (NullorALLNode != null && NullorALLNode.Checked)
                CancelFilterMenuItem_Click(null, new EventArgs());
            else
            {
                String oldfilter = this.FilterString;
                this.FilterString = "";
                this.activeFilterType = ADGVFilterMenuFilterType.CheckList;

                if (this.CheckList.Nodes.Count > 1)
                {
                    NullorALLNode = GetNullNode();
                    if (NullorALLNode != null && NullorALLNode.Checked)
                        this.FilterString = "[{0}] IS NULL";

                    if (this.CheckList.Nodes.Count > 2 || NullorALLNode == null)
                    {
                        String filter = CreateFilterString(
                            this.CheckList.Nodes.AsParallel().Cast<TripleTreeNode>().Where(
                                n => n.NodeType != TripleTreeNodeType.AllsNode
                                    && n.NodeType != TripleTreeNodeType.EmptysNode
                                    && n.CheckState != CheckState.Unchecked
                            )
                        );

                        if (filter.Length > 0)
                        {
                            if (this.FilterString.Length > 0)
                                this.FilterString += " OR ";

                            if (this.DataType == typeof(DateTime))
                            {
                                if (!this.TimeFilter && this.DateWithTime)
                                    this.FilterString += filter;
                                else
                                    this.FilterString += "[{0}] IN (" + filter + ")";
                            }
                            else if (this.DataType == typeof(Boolean))
                                this.FilterString += "{0}=" + filter;
                            else if (this.DataType == typeof(Int32) || this.DataType == typeof(Int64) || this.DataType == typeof(Int16) ||
                                        this.DataType == typeof(UInt32) || this.DataType == typeof(UInt64) || this.DataType == typeof(UInt16) ||
                                        this.DataType == typeof(Byte) || this.DataType == typeof(SByte) || this.DataType == typeof(String))
                                this.FilterString += "[{0}] IN (" + filter + ")";
                            else
                                this.FilterString += "Convert([{0}],System.String) IN (" + filter + ")";
                        }
                    }
                }

                DuplicateFilterNodes();

                if (oldfilter != this.FilterString && this.FilterChanged != null)
                    FilterChanged(this, new EventArgs());
            }
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            RestoreNodes();
            this.Close();
        }

        #endregion CheckList

        #region Sort Menus

        private void SortASCMenuItem_Click(object sender, EventArgs e)
        {
            SortASCMenuItem.Checked = true;
            SortDESCMenuItem.Checked = false;
            this.activeSortType = ADGVFilterMenuSortType.ASC;
            String oldsort = this.SortString;
            this.SortString = "[{0}] ASC";

            if (oldsort != this.SortString && this.SortChanged != null)
                SortChanged(this, new EventArgs());
        }

        private void SortDESCMenuItem_Click(object sender, EventArgs e)
        {
            SortASCMenuItem.Checked = false;
            SortDESCMenuItem.Checked = true;
            this.activeSortType = ADGVFilterMenuSortType.DESC;
            String oldsort = this.SortString;
            this.SortString = "[{0}] DESC";

            if (oldsort != this.SortString && this.SortChanged != null)
                SortChanged(this, new EventArgs());
        }

        private void CancelSortMenuItem_Click(object sender, EventArgs e)
        {
            String oldsort = this.SortString;
            ClearSorting();
            if (oldsort != this.SortString && this.SortChanged != null)
                SortChanged(this, new EventArgs());
        }

        public void ClearSorting()
        {
            String oldsort = this.SortString;
            this.SortASCMenuItem.Checked = false;
            this.SortDESCMenuItem.Checked = false;
            this.activeSortType = ADGVFilterMenuSortType.None;
            this.SortString = null;
        }

        #endregion Sort Menus

        #region Filter Menu

        private void SetupFilterMenuItem_Click(object sender, EventArgs e)
        {
            SetFilterForm flt = new SetFilterForm(this.DataType, this.DateWithTime, this.TimeFilter);
            if (flt.ShowDialog() == DialogResult.OK)
                AddCustomFilter(flt.FilterString, flt.ViewFilterString);
        }

        private void AddCustomFilter(String FilterString, String ViewFilterString)
        {
            Int32 index = -1;

            for (Int32 i = 2; i < FiltersMenuItem.DropDownItems.Count; i++)
            {
                if (FiltersMenuItem.DropDown.Items[i].Available)
                {
                    if (FiltersMenuItem.DropDownItems[i].Text == ViewFilterString && FiltersMenuItem.DropDownItems[i].Tag.ToString() == FilterString)
                    {
                        index = i;
                        break;
                    }
                }
                else
                    break;
            }

            if (index < 2)
            {
                for (Int32 i = FiltersMenuItem.DropDownItems.Count - 2; i > 1; i--)
                {
                    if (FiltersMenuItem.DropDownItems[i].Available)
                    {
                        FiltersMenuItem.DropDownItems[i + 1].Text = FiltersMenuItem.DropDownItems[i].Text;
                        FiltersMenuItem.DropDownItems[i + 1].Tag = FiltersMenuItem.DropDownItems[i].Tag;
                    }
                }
                index = 2;

                FiltersMenuItem.DropDownItems[2].Text = ViewFilterString;
                FiltersMenuItem.DropDownItems[2].Tag = FilterString;
            }

            SetCustomFilter(index);
        }

        private void SetCustomFilter(int FiltersMenuItemIndex)
        {
            if (this.activeFilterType == ADGVFilterMenuFilterType.CheckList)
                SetNodesCheckedState(this.CheckList.Nodes, false);

            String filterstring = this.FiltersMenuItem.DropDownItems[FiltersMenuItemIndex].Tag.ToString();
            String viewfilterstring = this.FiltersMenuItem.DropDownItems[FiltersMenuItemIndex].Text;

            if (FiltersMenuItemIndex != 2)
            {
                for (int i = FiltersMenuItemIndex; i > 2; i--)
                {
                    FiltersMenuItem.DropDownItems[i].Text = FiltersMenuItem.DropDownItems[i - 1].Text;
                    FiltersMenuItem.DropDownItems[i].Tag = FiltersMenuItem.DropDownItems[i - 1].Tag;
                }

                FiltersMenuItem.DropDownItems[2].Text = viewfilterstring;
                FiltersMenuItem.DropDownItems[2].Tag = filterstring;
            }

            for (int i = 3; i < FiltersMenuItem.DropDownItems.Count; i++)
            {
                (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = false;
            }

            (this.FiltersMenuItem.DropDownItems[2] as ToolStripMenuItem).Checked = true;
            this.activeFilterType = ADGVFilterMenuFilterType.Custom;
            String oldfilter = this.FilterString;
            this.FilterString = filterstring;
            SetNodesCheckedState(this.CheckList.Nodes, false);
            DuplicateFilterNodes();
            this.FiltersMenuItem.Checked = true;
            this.okButton.Enabled = false;
            if (oldfilter != this.FilterString && this.FilterChanged != null)
                FilterChanged(this, new EventArgs());
        }

        private void lastfilter1MenuItem_VisibleChanged(object sender, EventArgs e)
        {
            toolStripSeparator2MenuItem.Visible = !lastfilter1MenuItem.Visible;
            (sender as ToolStripMenuItem).VisibleChanged -= lastfilter1MenuItem_VisibleChanged;
        }

        private void lastfilter1MenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuitem = sender as ToolStripMenuItem;

            for (int i = 2; i < FiltersMenuItem.DropDownItems.Count; i++)
            {
                if (FiltersMenuItem.DropDownItems[i].Text == menuitem.Text && FiltersMenuItem.DropDownItems[i].Tag.ToString() == menuitem.Tag.ToString())
                {
                    SetCustomFilter(i);
                    break;
                }
            }
        }

        private void lastfilter1MenuItem_TextChanged(object sender, EventArgs e)
        {
            (sender as ToolStripMenuItem).Available = true;
            (sender as ToolStripMenuItem).TextChanged -= lastfilter1MenuItem_TextChanged;
        }

        private void CancelFilterMenuItem_Click(object sender, EventArgs e)
        {
            String oldfilter = this.FilterString;
            this.ClearFilter();
            if (oldfilter != this.FilterString && this.FilterChanged != null)
                FilterChanged(this, new EventArgs());
        }

        public void ClearFilter()
        {
            for (int i = 2; i < FiltersMenuItem.DropDownItems.Count - 1; i++)
            {
                (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = false;
            }
            this.activeFilterType = ADGVFilterMenuFilterType.None;
            SetNodesCheckedState(this.CheckList.Nodes, true);
            String oldsort = this.FilterString;
            this.FilterString = null;
            this.filterNodes = null;
            this.FiltersMenuItem.Checked = false;
            this.okButton.Enabled = true;
        }

        #endregion Filter Menu

        #region FilterMenu Interface Events

        private void SortASCMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Enabled)
                (sender as ToolStripMenuItem).Select();
        }

        private void FilterContextMenu_Closed(Object sender, EventArgs e)
        {
            ClearResizeBox();
            this.startingNodes = null;
        }

        private void FilterContextMenu_LostFocus(Object sender, EventArgs e)
        {
            if (!this.ContainsFocus)
                this.Close();
        }

        private void CheckList_MouseLeave(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void FiltersMenuItem_Paint(Object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(this.FiltersMenuItem.Width - 12, 7, 10, 10);
            ControlPaint.DrawMenuGlyph(e.Graphics, rect, MenuGlyph.Arrow, Color.Black, Color.Transparent);
        }

        #endregion FilterMenu Interface Events

        #region ResizeEvents

        private void ResizePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ClearResizeBox();
            }
        }

        private void ResizePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    PaintResizeBox(e.X, e.Y);
            }
        }

        private void ResizePictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (resizeEndPoint.X != -1)
            {
                ClearResizeBox();
                if (this.Visible)
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        int newWidth = e.X + this.Width - this.ResizeBoxControlHost.Width;
                        int newHeight = e.Y + this.Height - this.ResizeBoxControlHost.Height;

                        newWidth = Math.Max(newWidth, this.MinimumSize.Width);
                        newHeight = Math.Max(newHeight, this.MinimumSize.Height);

                        ResizeMenu(newWidth, newHeight);
                    }
            }
        }

        private void ResizeMenu(int W, int H)
        {
            this.SortASCMenuItem.Width = W - 1;
            this.SortDESCMenuItem.Width = W - 1;
            this.CancelSortMenuItem.Width = W - 1;
            this.CancelFilterMenuItem.Width = W - 1;
            this.SetupFilterMenuItem.Width = W - 1;
            this.FiltersMenuItem.Width = W - 1;
            this.CheckFilterListControlHost.Size = new System.Drawing.Size(W - 35, H - 160);
            this.CheckFilterListPanel.Size = new System.Drawing.Size(W - 35, H - 160);
            this.CheckList.Bounds = new Rectangle(4, 4, W - 35 - 8, H - 160 - 8);
            this.CheckFilterListButtonsControlHost.Size = new System.Drawing.Size(W - 35, 24);
            this.CheckFilterListButtonsControlHost.Size = new Size(W - 35, 24);
            this.okButton.Location = new Point(W - 35 - 164, 0);
            this.cancelButton.Location = new Point(W - 35 - 79, 0);
            this.ResizeBoxControlHost.Margin = new Padding(W - 46, 0, 0, 0);
            this.Size = new Size(W, H);
        }

        private void ResizeGrip_Paint(Object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Properties.Resources.ResizeGrip, 0, 0);
        }

        private void PaintResizeBox(int X, int Y)
        {
            ClearResizeBox();

            X += this.Width - this.ResizeBoxControlHost.Width;
            Y += this.Height - this.ResizeBoxControlHost.Height;

            X = Math.Max(X, this.MinimumSize.Width - 1);
            Y = Math.Max(Y, this.MinimumSize.Height - 1);

            Point StartPoint = this.PointToScreen(ADGVFilterMenu.resizeStartPoint);
            Point EndPoint = this.PointToScreen(new Point(X, Y));

            Rectangle rc = new Rectangle();

            rc.X = Math.Min(StartPoint.X, EndPoint.X);
            rc.Width = Math.Abs(StartPoint.X - EndPoint.X);

            rc.Y = Math.Min(StartPoint.Y, EndPoint.Y);
            rc.Height = Math.Abs(StartPoint.Y - EndPoint.Y);

            ControlPaint.DrawReversibleFrame(rc, Color.Black, FrameStyle.Dashed);

            resizeEndPoint.X = EndPoint.X;
            resizeEndPoint.Y = EndPoint.Y;
        }

        private void ClearResizeBox()
        {
            if (resizeEndPoint.X != -1)
            {
                Point StartPoint = this.PointToScreen(ADGVFilterMenu.resizeStartPoint);

                Rectangle rc = new Rectangle(StartPoint.X, StartPoint.Y, resizeEndPoint.X, resizeEndPoint.Y);

                rc.X = Math.Min(StartPoint.X, resizeEndPoint.X);
                rc.Width = Math.Abs(StartPoint.X - resizeEndPoint.X);

                rc.Y = Math.Min(StartPoint.Y, resizeEndPoint.Y);
                rc.Height = Math.Abs(StartPoint.Y - resizeEndPoint.Y);

                ControlPaint.DrawReversibleFrame(rc, Color.Black, FrameStyle.Dashed);

                resizeEndPoint.X = -1;
            }
        }

        #endregion ResizeEvents

        public void SetLoadedFilterMode(Boolean Enabled)
        {
            this.SetupFilterMenuItem.Enabled = !Enabled;
            this.CancelFilterMenuItem.Enabled = Enabled;
            if (Enabled)
            {
                this.activeFilterType = ADGVFilterMenuFilterType.Loaded;
                this.sortString = null;
                this.filterString = null;
                this.filterNodes = null;
                this.FiltersMenuItem.Checked = false;
                for (int i = 2; i < FiltersMenuItem.DropDownItems.Count - 1; i++)
                {
                    (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = false;
                }
                this.CheckList.Nodes.Clear();
                TripleTreeNode allnode = TripleTreeNode.CreateAllsNode(this.RM.GetString("tripletreenode_allnode_text") + "            ");
                allnode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                allnode.CheckState = CheckState.Indeterminate;
                this.CheckList.Nodes.Add(allnode);
            }
            else
            {
                this.activeFilterType = ADGVFilterMenuFilterType.None;
            }
        }
    }
}