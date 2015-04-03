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
        Custom,
        CheckList,
        Loaded
    }

    public enum ADGVFilterMenuSortType : byte
    {
        None = 0,
        ASC,
        DESC
    }
    
    public enum FilterDateTimeGrouping : byte
    {
        None = 0,
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second
    }

    public enum FilterDataType : byte
    {
        Text = 0,
        Int,
        Float,
        DateTime,
        Boolean
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

        private String[] months = null;
        private ResourceManager RM = null;
        private String sortString = null;
        private String filterString = null;
        private IEnumerable<TripleTreeNode> startingNodes = null;
        private IEnumerable<TripleTreeNode> filterNodes = null;
        private TripleTreeNode emptysNode = null;
        private TripleTreeNode allsNode = null;

        private static Point resizeStartPoint = new Point(1, 1);
        private Point resizeEndPoint = new Point(-1, -1);
        private FilterDateTimeGrouping dateTimeGrouping = FilterDateTimeGrouping.Second;

        public FilterDataType DataType { get; private set; }

        public FilterDateTimeGrouping DateTimeGrouping 
        {
            get
            {
                return this.dateTimeGrouping;
            }
            set
            {
                if (this.dateTimeGrouping != value)
                {
                    this.dateTimeGrouping = value;

                    if (this.DataType == FilterDataType.DateTime)
                    {
                        if (this.filterNodes != null)
                            this.filterNodes = UpdateNodeList(this.filterNodes);

                        if (this.CheckList.Nodes.Count > 0)
                            this.filterNodes = UpdateNodeList(this.CheckList.Nodes.Cast<TripleTreeNode>());
                    }
                }
            }
        }

        private IEnumerable<TripleTreeNode> UpdateNodeList(IEnumerable<TripleTreeNode> source)
        {
            var oldNodes = source.SelectMany(n => n.AllNodes).Where(n => n.NodeType == TripleTreeNodeType.MSecDateTimeNode || n.NodeType == TripleTreeNodeType.EmptysNode || n.NodeType == TripleTreeNodeType.AllsNode).ToList();
            var nodes = this.CreateNodesList(oldNodes.Where(n => n.NodeType != TripleTreeNodeType.AllsNode).Select(n => n.Value));

            oldNodes = oldNodes.Where(n => n.CheckState == CheckState.Checked).ToList();
            if (nodes.Count() > 0)
            {
                if (oldNodes.Count() > 0)
                {
                    if (oldNodes.Any(n => n.NodeType == TripleTreeNodeType.AllsNode))
                    {
                        SetNodesCheckedState(nodes, true);
                    }
                    else
                    {
                        foreach (var node in nodes.SelectMany(n => n.AllNodes).Where(n => n.NodeType == TripleTreeNodeType.MSecDateTimeNode || n.NodeType == TripleTreeNodeType.EmptysNode))
                        {
                            if (oldNodes.Any(n => n.Value.Equals(node.Value)))
                            {
                                node.CheckState = CheckState.Checked;
                                oldNodes.RemoveAll(n => n.Value.Equals(node.Value));
                            }
                        }

                        var allsNode = nodes.FirstOrDefault(n => n.NodeType == TripleTreeNodeType.AllsNode);
                        if (allsNode != null)
                            allsNode.CheckState = UpdateNodesCheckState(nodes);
                    }
                }
                return nodes;
            }
            else
                return null;
        }

        public ADGVFilterMenuSortType ActiveSortType { get; private set; }

        public ADGVFilterMenuFilterType ActiveFilterType { get; private set; }
        
        public String SortString
        {
            get
            {
                return this.sortString;
            }
            private set
            {
                value = string.IsNullOrWhiteSpace(value) ? null : value;

                if (value != this.sortString)
                {
                    this.sortString = value;
                    this.CancelSortMenuItem.Enabled = value != null;

                    SortChanged(this, new EventArgs());
                }
            }
        }

        public String FilterString
        {
            get
            {
                return this.filterString;
            }

            private set
            {
                this.filterString = string.IsNullOrWhiteSpace(value) ? null : value;
                this.CancelFilterMenuItem.Enabled = this.filterString != null;
            }
        }

        public event EventHandler SortChanged = delegate { };

        public event EventHandler FilterChanged = delegate { };

        public ADGVFilterMenu(Type DataType)
            : base()
        {
            if (DataType == typeof(Boolean))
                this.DataType = FilterDataType.Boolean;
            else if (DataType == typeof(DateTime))
                this.DataType = FilterDataType.DateTime;
            else if (DataType == typeof(Int32) || DataType == typeof(Int64) || DataType == typeof(Int16) ||
                    DataType == typeof(UInt32) || DataType == typeof(UInt64) || DataType == typeof(UInt16) ||
                    DataType == typeof(Byte) || DataType == typeof(SByte))
                this.DataType = FilterDataType.Int;
            else if (DataType == typeof(Single) || DataType == typeof(Double) || DataType == typeof(Decimal))
                this.DataType = FilterDataType.Float;
            else
                this.DataType = FilterDataType.Text;
            
            this.ActiveFilterType = ADGVFilterMenuFilterType.None;
            this.ActiveSortType = ADGVFilterMenuSortType.None;

            this.RM = new System.Resources.ResourceManager("ADGV.Localization.ADGVStrings", typeof(ADGV.ADGVFilterMenu).Assembly);

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
            // toolStripSeparator3MenuItem
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

            switch (this.DataType)
            {
                case FilterDataType.DateTime:
                    this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_datetime");
                    this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_datetime");
                    this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text_datetime");
                    this.SortASCMenuItem.Image = Properties.Resources.ASCnum;
                    this.SortDESCMenuItem.Image = Properties.Resources.DESCnum;
                    this.months = new String[13];
                    this.months[1] = this.RM.GetString("month1");
                    this.months[2] = this.RM.GetString("month2");
                    this.months[3] = this.RM.GetString("month3");
                    this.months[4] = this.RM.GetString("month4");
                    this.months[5] = this.RM.GetString("month5");
                    this.months[6] = this.RM.GetString("month6");
                    this.months[7] = this.RM.GetString("month7");
                    this.months[8] = this.RM.GetString("month8");
                    this.months[9] = this.RM.GetString("month9");
                    this.months[10] = this.RM.GetString("month10");
                    this.months[11] = this.RM.GetString("month11");
                    this.months[12] = this.RM.GetString("month12");
                    break;
                case FilterDataType.Boolean:
                    this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_text");
                    this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_boolean");
                    this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text_boolean");
                    this.SortASCMenuItem.Image = Properties.Resources.ASCbool;
                    this.SortDESCMenuItem.Image = Properties.Resources.DESCbool;
                    break;
                case FilterDataType.Int:
                case FilterDataType.Float:
                    this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_numeric");
                    this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_numeric");
                    this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text_numeric");
                    this.SortASCMenuItem.Image = Properties.Resources.ASCnum;
                    this.SortDESCMenuItem.Image = Properties.Resources.DESCnum;
                    break;
                default:
                    this.FiltersMenuItem.Text = this.RM.GetString("filtersmenuitem_text_text");
                    this.SortASCMenuItem.Text = this.RM.GetString("sortascmenuitem_text_text");
                    this.SortDESCMenuItem.Text = this.RM.GetString("sortdescmenuitem_text");
                    this.SortASCMenuItem.Image = Properties.Resources.ASCtxt;
                    this.SortDESCMenuItem.Image = Properties.Resources.DESCtxt;
                    break;
            }

            this.FiltersMenuItem.Enabled = this.DataType != FilterDataType.Boolean;
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

            using (Bitmap img = new Bitmap(16, 16))
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                    images.Images.Add("uncheck", (Bitmap)img.Clone());
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
                    images.Images.Add("check", (Bitmap)img.Clone());
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
                    images.Images.Add("mixed", (Bitmap)img.Clone());
                }
            }
            return images;
        }

        public static IEnumerable<object> GetValuesForFilter(DataGridView grid, String columnName, bool useFormatedValue = false)
        {
            var vals = 
                useFormatedValue ?
                    from DataGridViewRow r in grid.Rows
                    where r.IsNewRow == false
                    select r.Cells[columnName].FormattedValue
                :
                    from DataGridViewRow r in grid.Rows
                    where r.IsNewRow == false
                    select r.Cells[columnName].Value;

            return vals.Distinct();
        }

        #region Show Methods

        public void Show(Control control, int x, int y, IEnumerable<object> vals)
        {
            this.LoadNodes(CreateNodesList(vals));
            
            if (this.ActiveFilterType == ADGVFilterMenuFilterType.Custom)
                SetNodesCheckedState(this.CheckList.Nodes, false);

            this.startingNodes = this.DuplicateNodes();
            base.Show(control, x, y);
        }

        public void Show(Control control, int x, int y, bool restoreFilter)
        {
            if (restoreFilter)
                this.LoadNodes(this.filterNodes);

            this.startingNodes = this.DuplicateNodes();
            base.Show(control, x, y);
        }

        private void LoadNodes(IEnumerable<TripleTreeNode> nodes)
        {
            this.CheckList.BeginUpdate();
            
            this.allsNode = null;
            this.emptysNode = null;

            this.CheckList.Nodes.Clear();

            if (nodes != null && nodes.Count() > 0)
            {
                this.CheckList.Nodes.AddRange(nodes.ToArray());

                for (int i = Math.Min(3, this.CheckList.Nodes.Count); i > 0; i--)
                {
                    var n = this.CheckList.Nodes[i - 1] as TripleTreeNode;
                    if (n != null)
                    {
                        if (n.NodeType == TripleTreeNodeType.AllsNode)
                            this.allsNode = n;
                        else
                            if (n.NodeType == TripleTreeNodeType.EmptysNode)
                                this.emptysNode = n;
                    }
                }
            }

            this.CheckList.EndUpdate();
        }

        private string GetDateTimeString(DateTime d, FilterDateTimeGrouping g, bool showTime)
        {
            if (showTime)
            {
                switch (g)
                {
                    case FilterDateTimeGrouping.Month:
                        return string.Format("{0:D2} {1:D2}:{2:D2}:{3:D2}.{4}", d.Day, d.Hour, d.Minute, d.Second, d.Millisecond.ToString("D3"));
                    case FilterDateTimeGrouping.Day:
                        return string.Format("{0:D2}:{1:D2}:{2:D2}.{3}", d.Hour, d.Minute, d.Second, d.Millisecond.ToString("D3"));
                    case FilterDateTimeGrouping.Hour:
                        return string.Format("{0:D2}:{1:D2}.{2}", d.Minute, d.Second, d.Millisecond.ToString("D3"));
                    case FilterDateTimeGrouping.Minute:
                        return string.Format("{0:D2}.{1}", d.Second, d.Millisecond.ToString("D3"));
                    default:
                        return string.Format("{0} {1:D2}:{2:D2}:{3:D2}.{4}", d.ToShortDateString(), d.Hour, d.Minute, d.Second, d.Millisecond.ToString("D3"));
                }
            }
            else
            {
                switch (g)
                {
                    case FilterDateTimeGrouping.None:
                    case FilterDateTimeGrouping.Year:
                            return d.ToShortDateString();
                    case FilterDateTimeGrouping.Month:
                        return d.Day.ToString("D2");
                    default:
                        return "--:--";
                }

            }
        }

        private IEnumerable<TripleTreeNode> CreateNodesList(IEnumerable<object> vals)
        {
            List<TripleTreeNode> nodes = new List<TripleTreeNode>();

            if (vals != null)
            {
                var valsCnt = vals.Count();

                if (valsCnt > 0)
                {
                    TripleTreeNode allnode = TripleTreeNode.CreateAllsNode(this.RM.GetString("tripletreenode_allnode_text") + "            ");
                    allnode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                    nodes.Add(allnode);

                    vals = vals.Where(v => v != null && v != DBNull.Value);
                    var nonullsCnt = vals.Count();

                    if (valsCnt > nonullsCnt)
                    {
                        TripleTreeNode nullnode = TripleTreeNode.CreateEmptysNode(this.RM.GetString("tripletreenode_nullnode_text") + "               ");
                        nullnode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                        nodes.Add(nullnode);
                    }

                    if (nonullsCnt > 0)
                    {
                        if (this.DataType ==  FilterDataType.DateTime)
                        {
                            #region Datetime

                            bool hasTime = vals.OfType<DateTime>().Any(d => d.Hour > 0 || d.Minute > 0 || d.Second > 0 || d.Millisecond > 0);

                            if (this.dateTimeGrouping == FilterDateTimeGrouping.None)
                            {
                                foreach (var d in vals.OfType<DateTime>().OrderBy(d => d))
                                    nodes.Add(TripleTreeNode.CreateMSecNode(GetDateTimeString(d, this.dateTimeGrouping, hasTime), d));
                            }
                            else
                            {
                                var years =
                                from year in vals.OfType<DateTime>().OrderBy(d => d)
                                group year by year.Year into y
                                select y;

                                foreach (var y in years)
                                {
                                    var yearnode = TripleTreeNode.CreateYearNode(y.Key.ToString(), y.Key);
                                    nodes.Add(yearnode);

                                    if (this.dateTimeGrouping == FilterDateTimeGrouping.Year)
                                    {
                                        foreach (var d in y)
                                            yearnode.Nodes.Add(TripleTreeNode.CreateMSecNode(GetDateTimeString(d, this.dateTimeGrouping, hasTime), d));
                                    }
                                    else
                                    {
                                        var months =
                                            from month in y
                                            group month by month.Month into m
                                            select m;

                                        foreach (var m in months)
                                        {
                                            var monthnode = yearnode.CreateChildNode(this.months[m.Key], m.Key);

                                            if (this.dateTimeGrouping == FilterDateTimeGrouping.Month || !hasTime)
                                            {
                                                foreach (var d in m)
                                                    monthnode.Nodes.Add(TripleTreeNode.CreateMSecNode(GetDateTimeString(d, this.dateTimeGrouping, hasTime), d));
                                            }
                                            else
                                            {
                                                var days =
                                                    from day in m
                                                    group day by day.Day into d
                                                    select d;

                                                foreach (var d in days)
                                                {
                                                    var daysnode = monthnode.CreateChildNode(d.Key.ToString("D2"), d.Key);
                                                    
                                                    if (this.dateTimeGrouping == FilterDateTimeGrouping.Day)
                                                    {
                                                        foreach (var t in d)
                                                            daysnode.Nodes.Add(TripleTreeNode.CreateMSecNode(GetDateTimeString(t, this.dateTimeGrouping, true), t));
                                                    }
                                                    else
                                                    {
                                                        var hours =
                                                            from hour in d
                                                            group hour by hour.Hour into h
                                                            select h;

                                                        foreach (var h in hours)
                                                        {
                                                            var hoursnode = daysnode.CreateChildNode(h.Key.ToString("D2") + " " + this.RM.GetString("checknodetree_hour"), h.Key);

                                                            if (this.dateTimeGrouping == FilterDateTimeGrouping.Hour)
                                                            {
                                                                foreach (var t in h)
                                                                    hoursnode.Nodes.Add(TripleTreeNode.CreateMSecNode(GetDateTimeString(t, this.dateTimeGrouping, true), t));
                                                            }
                                                            else
                                                            {
                                                                var mins =
                                                                    from min in h
                                                                    group min by min.Minute into mn
                                                                    select mn;

                                                                foreach (var mn in mins)
                                                                {
                                                                    var minsnode = hoursnode.CreateChildNode(mn.Key.ToString("D2") + " " + this.RM.GetString("checknodetree_minute"), mn.Key);

                                                                    if (this.dateTimeGrouping == FilterDateTimeGrouping.Minute)
                                                                    {
                                                                        foreach (var t in mn)
                                                                            minsnode.Nodes.Add(TripleTreeNode.CreateMSecNode(GetDateTimeString(t, this.dateTimeGrouping, true), t));
                                                                    }
                                                                    else
                                                                    {
                                                                        var secs =
                                                                            from sec in mn
                                                                            group sec by sec.Second into s
                                                                            select s;

                                                                        foreach (var s in secs)
                                                                        {
                                                                            var secsnode = minsnode.CreateChildNode(s.Key.ToString("D2") + " " + this.RM.GetString("checknodetree_second"), s.Key);

                                                                            var msecs =
                                                                                from msec in s
                                                                                group msec by msec.Millisecond into ms
                                                                                select ms;

                                                                            foreach (var ms in msecs)
                                                                                secsnode.CreateChildNode(ms.Key.ToString("D3") + " " + this.RM.GetString("checknodetree_millisecond"), ms.First());
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
                                }
                            }
                            #endregion Datetime
                        }
                        else if (this.DataType == FilterDataType.Boolean)
                        {
                            if (nonullsCnt > 1)
                            {
                                nodes.Add(TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_false"), false));
                                nodes.Add(TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_true"), true));
                            }
                            else if ((Boolean)vals.First())
                            {
                                nodes.Add(TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_true"), true));
                            }
                            else
                                nodes.Add(TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_false"), false));
                        }
                        else 
                        {
                            foreach (var v in vals.OrderBy(o => o))
                                nodes.Add(TripleTreeNode.CreateNode(v.ToString(), v));
                        }
                    }
                }
            }

            return nodes;
        }

        #endregion Show Methods

        #region CheckList

        private IEnumerable<TripleTreeNode> DuplicateNodes()
        {
            if (this.CheckList.Nodes.Count > 0)
            {
                var result = new TripleTreeNode[this.CheckList.Nodes.Count];
                foreach (TripleTreeNode n in this.CheckList.Nodes)
                    result[n.Index] = n.Clone();

                return result;
            }
            else
                return null;
        }
              
        private void SetNodesCheckedState(IEnumerable<TripleTreeNode> nodes, Boolean isChecked)
        {
            foreach (TripleTreeNode node in nodes)
            {
                node.Checked = isChecked;
                if (node.Nodes.Count > 0)
                    SetNodesCheckedState(node.Nodes, isChecked);
            }
        }
        
        private void SetNodesCheckedState(TreeNodeCollection nodes, Boolean isChecked)
        {
            foreach (TripleTreeNode node in nodes)
            {
                node.Checked = isChecked;
                if (node.Nodes.Count > 0)
                    SetNodesCheckedState(node.Nodes, isChecked);
            }
        }

        private CheckState UpdateNodesCheckState(IEnumerable<TripleTreeNode> nodes)
        {
            CheckState? result = null;

            foreach (TripleTreeNode n in nodes)
            {
                if (n.NodeType == TripleTreeNodeType.AllsNode)
                    continue;

                if (n.Nodes.Count > 0)
                    n.CheckState = UpdateNodesCheckState(n.Nodes);

                if (result.HasValue)
                {
                    if (result.Value != n.CheckState)
                        result = CheckState.Indeterminate;
                }
                else
                    result = n.CheckState;
            }

            return result.HasValue ? result.Value : CheckState.Unchecked;
        }

        private CheckState UpdateNodesCheckState(TreeNodeCollection nodes)
        {
            CheckState? result = null;

            foreach (TripleTreeNode n in nodes)
            {
                if (n.NodeType == TripleTreeNodeType.AllsNode)
                    continue;

                if (n.Nodes.Count > 0)
                    n.CheckState = UpdateNodesCheckState(n.Nodes);

                if (result.HasValue)
                {
                    if (result.Value != n.CheckState)
                        result = CheckState.Indeterminate;
                }
                else
                    result = n.CheckState;
            }

            return result.HasValue ? result.Value : CheckState.Unchecked;
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

                CheckState state = UpdateNodesCheckState(this.CheckList.Nodes);

                if (this.allsNode != null)
                    this.allsNode.CheckState = state;

                this.okButton.Enabled = !(state == CheckState.Unchecked);
            }
        }

        private String CreateFilterString()
        {
            StringBuilder sb = new StringBuilder("");

            var nodes = this.CheckList.Nodes.AsParallel().Cast<TripleTreeNode>()
                    .Where(n => n.NodeType != TripleTreeNodeType.AllsNode
                           && n.NodeType != TripleTreeNodeType.EmptysNode
                           && n.CheckState != CheckState.Unchecked);
           
            if (nodes.Count() > 0)
            {
                if (this.DataType == FilterDataType.Boolean)
                {
                    if (nodes.Count() == 1)
                        sb.Append(nodes.First().Value.ToString());
                }
                else
                {
                    if (this.DataType == FilterDataType.DateTime)
                    {
                        foreach (var d in nodes.SelectMany(n => n.AllNodes)
                            .Where(cn => cn.CheckState == CheckState.Checked && cn.NodeType == TripleTreeNodeType.MSecDateTimeNode)
                            .Select(n => n.Value).Cast<DateTime>())
                        {
                            sb.Append("'" + d.ToString("o") + "', ");
                        }
                    }
                    else if (this.DataType == FilterDataType.Int)
                    {
                        foreach (var n in nodes)
                            sb.Append(n.Value.ToString() + ", ");
                    }
                    else if (this.DataType == FilterDataType.Float)
                    {
                        foreach (var n in nodes)
                            sb.Append(n.Value.ToString().Replace(",", ".") + ", ");
                    }
                    else foreach (var n in nodes)
                        {
                            sb.Append("'" + n.Value.ToString().Replace("'", "''").Replace("{", "{{").Replace("}", "}}") + "', ");
                        }

                    sb.Length -= 2;
                }
            }

            return sb.ToString();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.FiltersMenuItem.Checked = false;

            if (this.allsNode != null && this.allsNode.Checked)
                CancelFilterMenuItem_Click(null, new EventArgs());
            else
            {
                this.ActiveFilterType = ADGVFilterMenuFilterType.CheckList;
                string newfilter = null;
                
                if (this.CheckList.Nodes.Count > 1)
                {
                    if (this.CheckList.Nodes.Count > 2 || this.emptysNode == null)
                    {
                        String filter = CreateFilterString();
                    
                        if (filter.Length > 0)
                        {
                            if (this.DataType == FilterDataType.Boolean)
                                newfilter = "[{0}]=" + filter;
                            else if (this.DataType == FilterDataType.Float)
                                newfilter = "Convert([{0}],System.String) IN (" + filter + ")";
                            else
                                newfilter = "[{0}] IN (" + filter + ")"; 
                        }
                    }
                    
                    if (this.emptysNode != null && this.emptysNode.Checked)
                    {
                        if (newfilter != null)
                            newfilter = "[{0}] IS NULL OR " + newfilter;
                        else
                            newfilter = "[{0}] IS NULL";
                    } 
                    else if (this.DataType == FilterDataType.Boolean && newfilter == null)
                    {
                        newfilter = "[{0}] NOT IS NULL";
                    }
                }

                if (newfilter != this.FilterString)
                {
                    this.filterNodes = this.DuplicateNodes();
                    this.FilterString = newfilter;

                    FilterChanged(this, new EventArgs());
                }
            }
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.LoadNodes(this.startingNodes);
            this.Close();
        }

        #endregion CheckList

        #region Sort Menus

        private void SortASCMenuItem_Click(object sender, EventArgs e)
        {
            SortASCMenuItem.Checked = true;
            SortDESCMenuItem.Checked = false;
            this.ActiveSortType = ADGVFilterMenuSortType.ASC;
            this.SortString = "[{0}] ASC";
        }

        private void SortDESCMenuItem_Click(object sender, EventArgs e)
        {
            SortASCMenuItem.Checked = false;
            SortDESCMenuItem.Checked = true;
            this.ActiveSortType = ADGVFilterMenuSortType.DESC;
            this.SortString = "[{0}] DESC";
        }

        private void CancelSortMenuItem_Click(object sender, EventArgs e)
        {
            String oldSort = this.SortString;
            this.ClearSorting();

            if (oldSort != this.SortString)
                SortChanged(this, new EventArgs());
        }

        public void ClearSorting()
        {
            this.SortASCMenuItem.Checked = false;
            this.SortDESCMenuItem.Checked = false;
            this.ActiveSortType = ADGVFilterMenuSortType.None;
            this.CancelSortMenuItem.Enabled = false;
            this.sortString = null;
        }

        #endregion Sort Menus

        #region Filter Menu

        private void SetupFilterMenuItem_Click(object sender, EventArgs e)
        {
            SetFilterForm flt = new SetFilterForm(this.DataType);
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
            if (this.ActiveFilterType == ADGVFilterMenuFilterType.CheckList)
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
            this.ActiveFilterType = ADGVFilterMenuFilterType.Custom;
            String oldfilter = this.filterString;
            this.FilterString = filterstring;
            this.SetNodesCheckedState(this.CheckList.Nodes, false);
            this.filterNodes = this.DuplicateNodes();
            this.FiltersMenuItem.Checked = true;
            this.okButton.Enabled = false;
            if (oldfilter != this.FilterString)
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
            if (oldfilter != this.FilterString)
                FilterChanged(this, new EventArgs());
        }

        public void ClearFilter()
        {
            for (int i = 2; i < FiltersMenuItem.DropDownItems.Count - 1; i++)
            {
                (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = false;
            }
            this.ActiveFilterType = ADGVFilterMenuFilterType.None;
            
            SetNodesCheckedState(this.CheckList.Nodes, true);

            this.FilterString = null;
            this.filterNodes = null;
            this.FiltersMenuItem.Checked = false;
            this.okButton.Enabled = true;
        }

        #endregion Filter Menu

        #region Interface Events

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
            this.FiltersMenuItem.Enabled = !Enabled;
            this.CancelFilterMenuItem.Enabled = Enabled;

            if (Enabled)
            {
                this.ActiveFilterType = ADGVFilterMenuFilterType.Loaded;
                this.sortString = null;
                this.filterString = null;
                this.filterNodes = null;
                this.startingNodes = null;
                this.FiltersMenuItem.Checked = false;

                for (int i = 2; i < FiltersMenuItem.DropDownItems.Count - 1; i++)
                {
                    (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = false;
                }
                
                var allsNode = TripleTreeNode.CreateAllsNode(this.RM.GetString("tripletreenode_allnode_text") + "            ");
                allsNode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                allsNode.CheckState = CheckState.Indeterminate;
                
                LoadNodes(new TripleTreeNode[] {allsNode});
            }
            else
            {
                this.ActiveFilterType = ADGVFilterMenuFilterType.None;
            }
        }
    }
}