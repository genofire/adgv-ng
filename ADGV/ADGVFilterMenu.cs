using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace ADGV
{
    public enum FilterMenuFilterType : byte
    {
        None = 0,
        Custom,
        CheckList,
        Loaded
    }

    public enum FilterMenuSortType : byte
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
        #region Controls

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

        #endregion Controls

        private string[] months = null;
        private ResourceManager RM = null;
        private string sortString = null;
        private string filterString = null;
        private IEnumerable<TripleTreeNode> startingNodes = null;
        private IEnumerable<TripleTreeNode> filterNodes = null;
        private TripleTreeNode emptysNode = null;
        private TripleTreeNode allsNode = null;
        private bool checkListChanged = false;
        
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
                            this.filterNodes = this.RecreateDateTimeNodes(this.filterNodes);

                        if (this.CheckList.Nodes.Count > 0)
                            this.LoadNodes(this.RecreateDateTimeNodes(this.CheckList.Nodes.Cast<TripleTreeNode>()));
                    }

                    this.DateTimeGroupingChanged(this, new EventArgs());
                }
            }
        }

        public FilterMenuSortType ActiveSortType { get; private set; }

        public FilterMenuFilterType ActiveFilterType { get; private set; }
        
        public string SortString
        {
            get
            {
                return String.IsNullOrWhiteSpace(this.sortString) ? "" : this.sortString;
            }
            private set
            {
                if (String.IsNullOrWhiteSpace(value))
                    value = null;

                if (value != this.sortString)
                {
                    this.sortString = value;
                    this.CancelSortMenuItem.Enabled = this.sortString != null;
                }
            }
        }

        public string FilterString
        {
            get
            {
                return String.IsNullOrWhiteSpace(this.filterString) ? "" : this.filterString;
            }

            private set
            {
                if (String.IsNullOrWhiteSpace(value))
                    value = null;

                if (value != this.filterString)
                {
                    this.filterString = value;
                    this.CancelFilterMenuItem.Enabled = this.filterString != null;
                }
            }
        }

        public event EventHandler SortChanged = delegate { };

        public event EventHandler FilterChanged = delegate { };

        public event EventHandler DateTimeGroupingChanged = delegate { };

        private void InitializeComponent()
        {
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
            this.ResizeBoxControlHost = new ToolStripControlHost(new Control());

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
            this.SortASCMenuItem.MouseEnter += new System.EventHandler(this.MenuItem_MouseEnter);
            this.SortASCMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            //
            // SortDESCMenuItem
            //
            this.SortDESCMenuItem.Name = "SortDESCMenuItem";
            this.SortDESCMenuItem.AutoSize = false;
            this.SortDESCMenuItem.Size = new System.Drawing.Size(this.Width - 1, 22);
            this.SortDESCMenuItem.Click += new System.EventHandler(this.SortDESCMenuItem_Click);
            this.SortDESCMenuItem.MouseEnter += new System.EventHandler(this.MenuItem_MouseEnter);
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
            this.CancelSortMenuItem.MouseEnter += new System.EventHandler(this.MenuItem_MouseEnter);
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
            this.CancelFilterMenuItem.MouseEnter += new System.EventHandler(this.MenuItem_MouseEnter);
            //
            // SetupFilterMenuItem
            //
            this.SetupFilterMenuItem.Name = "SetupFilterMenuItem";
            this.SetupFilterMenuItem.Size = new System.Drawing.Size(152, 22);
            this.SetupFilterMenuItem.Text = this.RM.GetString("setupfiltermenuitem_text");
            this.SetupFilterMenuItem.Click += new System.EventHandler(this.SetupFilterMenuItem_Click);
            //
            // toolStripSeparator2MenuItem
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
            this.lastfilter1MenuItem.Click += new System.EventHandler(this.lastfilterMenuItem_Click);
            this.lastfilter1MenuItem.TextChanged += new System.EventHandler(this.lastfilterMenuItem_TextChanged);
            this.lastfilter1MenuItem.VisibleChanged += new System.EventHandler(this.lastfilterMenuItem_VisibleChanged);
            //
            // lastfilter2MenuItem
            //
            this.lastfilter2MenuItem.Name = "lastfilter2MenuItem";
            this.lastfilter2MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter2MenuItem.Tag = "1";
            this.lastfilter2MenuItem.Text = null;
            this.lastfilter2MenuItem.Visible = false;
            this.lastfilter2MenuItem.Click += new System.EventHandler(this.lastfilterMenuItem_Click);
            this.lastfilter2MenuItem.TextChanged += new System.EventHandler(this.lastfilterMenuItem_TextChanged);
            //
            // lastfilter3MenuItem
            //
            this.lastfilter3MenuItem.Name = "lastfilter3MenuItem";
            this.lastfilter3MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter3MenuItem.Tag = "2";
            this.lastfilter3MenuItem.Text = null;
            this.lastfilter3MenuItem.Visible = false;
            this.lastfilter3MenuItem.Click += new System.EventHandler(this.lastfilterMenuItem_Click);
            this.lastfilter3MenuItem.TextChanged += new System.EventHandler(this.lastfilterMenuItem_TextChanged);
            //
            // lastfilter4MenuItem
            //
            this.lastfilter4MenuItem.Name = "lastfilter4MenuItem";
            this.lastfilter4MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter4MenuItem.Tag = "3";
            this.lastfilter4MenuItem.Text = null;
            this.lastfilter4MenuItem.Visible = false;
            this.lastfilter4MenuItem.Click += new System.EventHandler(this.lastfilterMenuItem_Click);
            this.lastfilter4MenuItem.TextChanged += new System.EventHandler(this.lastfilterMenuItem_TextChanged);
            //
            // lastfilter5MenuItem
            //
            this.lastfilter5MenuItem.Name = "lastfilter5MenuItem";
            this.lastfilter5MenuItem.Size = new System.Drawing.Size(152, 22);
            this.lastfilter5MenuItem.Tag = "4";
            this.lastfilter5MenuItem.Text = null;
            this.lastfilter5MenuItem.Visible = false;
            this.lastfilter5MenuItem.Click += new System.EventHandler(this.lastfilterMenuItem_Click);
            this.lastfilter5MenuItem.TextChanged += new System.EventHandler(this.lastfilterMenuItem_TextChanged);
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
            this.FiltersMenuItem.MouseEnter += new System.EventHandler(this.MenuItem_MouseEnter);
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
            this.CheckList.CheckBoxes = false;
            this.CheckList.StateImageList = new ImageList();
            this.CheckList.MouseLeave += new EventHandler(CheckList_MouseLeave);
            this.CheckList.NodeMouseClick += new TreeNodeMouseClickEventHandler(CheckList_NodeMouseClick);
            this.CheckList.KeyDown += new KeyEventHandler(CheckList_KeyDown);
            this.CheckList.MouseEnter += new EventHandler(CheckList_MouseEnter);
            this.CheckList.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(CheckList_NodeMouseDoubleClick);
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

            #endregion Interface

            using (Bitmap img = new Bitmap(16, 16))
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                    this.CheckList.StateImageList.Images.Add("uncheck", (Bitmap)img.Clone());
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
                    this.CheckList.StateImageList.Images.Add("check", (Bitmap)img.Clone());
                    CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
                    this.CheckList.StateImageList.Images.Add("mixed", (Bitmap)img.Clone());
                }
            }
            this.ActiveFilterType = FilterMenuFilterType.None;
            this.ActiveSortType = FilterMenuSortType.None;

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
            this.FiltersMenuItem.Checked = this.ActiveFilterType == FilterMenuFilterType.Custom;
            this.MinimumSize = new Size(this.PreferredSize.Width, this.PreferredSize.Height);
            this.ResizeMenu(this.MinimumSize.Width, this.MinimumSize.Height);

            this.ResumeLayout(false);
        }

        public ADGVFilterMenu(Type dataType)
            : base()
        {
            if (dataType == typeof(Boolean))
                this.DataType = FilterDataType.Boolean;
            else if (dataType == typeof(DateTime))
                this.DataType = FilterDataType.DateTime;
            else if (dataType == typeof(Int32) || dataType == typeof(Int64) || dataType == typeof(Int16) ||
                    dataType == typeof(UInt32) || dataType == typeof(UInt64) || dataType == typeof(UInt16) ||
                    dataType == typeof(Byte) || dataType == typeof(SByte))
                this.DataType = FilterDataType.Int;
            else if (dataType == typeof(Single) || dataType == typeof(Double) || dataType == typeof(Decimal))
                this.DataType = FilterDataType.Float;
            else
                this.DataType = FilterDataType.Text;

            this.InitializeComponent();
        }

        public ADGVFilterMenu(FilterDataType filterDataType)
            : base()
        {
            this.DataType = filterDataType;
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            this.RM = null;
            this.emptysNode = null;
            this.allsNode = null;
            this.startingNodes = null;
            this.filterNodes = null;
            this.ClearResizeBox();

            base.Dispose(disposing);
        }

        #region Public Methods

        public object Clone()
        {
            ADGVFilterMenu m = new ADGVFilterMenu(this.DataType);

            m.ActiveFilterType = this.ActiveFilterType;
            m.ActiveSortType = this.ActiveSortType;
            m.SortString = this.SortString;
            m.FilterString = this.FilterString;
            m.checkListChanged = this.checkListChanged;
            m.resizeEndPoint = this.resizeEndPoint;
            m.dateTimeGrouping = this.dateTimeGrouping;
            m.LoadNodes(this.CloneNodes(this.CheckList.Nodes));
            m.startingNodes = this.CloneNodes(this.startingNodes);
            m.filterNodes = this.CloneNodes(this.filterNodes);
            m.ResizeMenu(this.Width, this.Height);
            
            m.SortASCMenuItem.Checked = this.SortASCMenuItem.Checked;
            m.SortDESCMenuItem.Checked = this.SortDESCMenuItem.Checked;
            m.FiltersMenuItem.Enabled = this.FiltersMenuItem.Enabled ;
            m.FiltersMenuItem.Checked = this.FiltersMenuItem.Checked;
            m.okButton.Enabled = this.okButton.Enabled;
            m.cancelButton.Enabled = this.cancelButton.Enabled;

            if (!this.toolStripSeparator2MenuItem.Visible)
            {
                m.toolStripSeparator2MenuItem.Visible = false;
                m.lastfilter1MenuItem.VisibleChanged -= m.lastfilterMenuItem_VisibleChanged;
            }

            for (int i = 2; i < m.FiltersMenuItem.DropDownItems.Count; i++)
            {
                m.FiltersMenuItem.DropDownItems[i].Text = this.FiltersMenuItem.DropDownItems[i].Text;
                m.FiltersMenuItem.DropDownItems[i].Tag = this.FiltersMenuItem.DropDownItems[i].Tag;
                m.FiltersMenuItem.DropDownItems[i].Visible = this.FiltersMenuItem.DropDownItems[i].Visible;
                (m.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked;

                if (!this.FiltersMenuItem.DropDownItems[i].Available)
                {
                    m.FiltersMenuItem.DropDownItems[i].Available = false;
                    m.FiltersMenuItem.DropDownItems[i].TextChanged -= m.lastfilterMenuItem_TextChanged;
                }
            }

            return m;
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

        public void Show(Control control, int x, int y, IEnumerable<object> vals)
        {
            this.LoadNodes(this.CreateNodesList(vals));
            this.Show(control, x, y, false);
        }

        public void Show(Control control, int x, int y, bool restoreFilter)
        {
            this.checkListChanged = false;
            
            if (restoreFilter)
                this.LoadNodes(this.filterNodes);

            this.startingNodes = this.CloneNodes(this.CheckList.Nodes);
            base.Show(control, x, y);
        }

        public void ClearSorting(FilterMenuSortType sort = FilterMenuSortType.None, bool fireEvent = false)
        {
            string oldSort = this.SortString;
            this.SetSortingUI(sort);

            if (fireEvent && oldSort != this.SortString)
                this.SortChanged(this, new EventArgs());
        }

        public void ClearFilter(bool fireEvent = false)
        {
            for (int i = 2; i < FiltersMenuItem.DropDownItems.Count - 1; i++)
            {
                (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = false;
            }
            this.ActiveFilterType = FilterMenuFilterType.None;

            this.SetNodesCheckedState(this.CheckList.Nodes, true);

            string oldFilter = this.FilterString;
            this.FilterString = null;
            this.filterNodes = null;
            this.FiltersMenuItem.Checked = false;
            this.okButton.Enabled = true;

            if (fireEvent && oldFilter != this.FilterString)
                this.FilterChanged(this, new EventArgs());
        }

        public void SetLoadedFilterMode(bool enabled)
        {
            this.FiltersMenuItem.Enabled = !enabled;
            this.CancelFilterMenuItem.Enabled = enabled;

            if (enabled)
            {
                this.ActiveFilterType = FilterMenuFilterType.Loaded;
                this.SortString = null;
                this.FilterString = null;
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

                this.LoadNodes(new TripleTreeNode[] { allsNode });
            }
            else
            {
                this.ActiveFilterType = FilterMenuFilterType.None;
            }
        }

        #endregion Public Methods

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

        private string DateTimeToNodeText(DateTime date, FilterDateTimeGrouping grouping, bool showTime)
        {
            if (showTime)
            {
                switch (grouping)
                {
                    case FilterDateTimeGrouping.Month:
                        return string.Format("{0:D2} {1:D2}:{2:D2}:{3:D2}.{4}", date.Day, date.Hour, date.Minute, date.Second, date.Millisecond.ToString("D3"));
                    case FilterDateTimeGrouping.Day:
                        return string.Format("{0:D2}:{1:D2}:{2:D2}.{3}", date.Hour, date.Minute, date.Second, date.Millisecond.ToString("D3"));
                    case FilterDateTimeGrouping.Hour:
                        return string.Format("{0:D2}:{1:D2}.{2}", date.Minute, date.Second, date.Millisecond.ToString("D3"));
                    case FilterDateTimeGrouping.Minute:
                        return string.Format("{0:D2}.{1}", date.Second, date.Millisecond.ToString("D3"));
                    default:
                        return string.Format("{0} {1:D2}:{2:D2}:{3:D2}.{4}", date.ToShortDateString(), date.Hour, date.Minute, date.Second, date.Millisecond.ToString("D3"));
                }
            }
            else
            {
                switch (grouping)
                {
                    case FilterDateTimeGrouping.None:
                    case FilterDateTimeGrouping.Year:
                        return date.ToShortDateString();
                    case FilterDateTimeGrouping.Month:
                        return date.Day.ToString("D2");
                    default:
                        return "--:--";
                }

            }
        }

        private IEnumerable<TripleTreeNode> CreateNodesList(IEnumerable<object> values)
        {
            List<TripleTreeNode> nodes = new List<TripleTreeNode>();

            if (values != null)
            {
                var valsCnt = values.Count();

                if (valsCnt > 0)
                {
                    TripleTreeNode allnode = TripleTreeNode.CreateAllsNode(this.RM.GetString("tripletreenode_allnode_text") + "            ");
                    allnode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                    nodes.Add(allnode);

                    values = values.Where(v => v != null && v != DBNull.Value);
                    var nonullsCnt = values.Count();

                    if (valsCnt > nonullsCnt)
                    {
                        TripleTreeNode nullnode = TripleTreeNode.CreateEmptysNode(this.RM.GetString("tripletreenode_nullnode_text") + "               ");
                        nullnode.NodeFont = new Font(this.CheckList.Font, FontStyle.Bold);
                        nodes.Add(nullnode);
                    }

                    if (nonullsCnt > 0)
                    {
                        if (this.DataType == FilterDataType.DateTime)
                        {
                            #region Datetime

                            bool hasTime = values.OfType<DateTime>().Any(d => d.Hour > 0 || d.Minute > 0 || d.Second > 0 || d.Millisecond > 0);

                            if (this.dateTimeGrouping == FilterDateTimeGrouping.None)
                            {
                                foreach (var d in values.OfType<DateTime>().OrderBy(d => d))
                                    nodes.Add(TripleTreeNode.CreateMSecNode(DateTimeToNodeText(d, this.dateTimeGrouping, hasTime), d));
                            }
                            else
                            {
                                var years =
                                from year in values.OfType<DateTime>().OrderBy(d => d)
                                group year by year.Year into y
                                select y;

                                foreach (var y in years)
                                {
                                    var yearnode = TripleTreeNode.CreateYearNode(y.Key.ToString(), y.Key);
                                    nodes.Add(yearnode);

                                    if (this.dateTimeGrouping == FilterDateTimeGrouping.Year)
                                    {
                                        foreach (var d in y)
                                            yearnode.Nodes.Add(TripleTreeNode.CreateMSecNode(DateTimeToNodeText(d, this.dateTimeGrouping, hasTime), d));
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
                                                    monthnode.Nodes.Add(TripleTreeNode.CreateMSecNode(DateTimeToNodeText(d, this.dateTimeGrouping, hasTime), d));
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
                                                            daysnode.Nodes.Add(TripleTreeNode.CreateMSecNode(DateTimeToNodeText(t, this.dateTimeGrouping, true), t));
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
                                                                    hoursnode.Nodes.Add(TripleTreeNode.CreateMSecNode(DateTimeToNodeText(t, this.dateTimeGrouping, true), t));
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
                                                                            minsnode.Nodes.Add(TripleTreeNode.CreateMSecNode(DateTimeToNodeText(t, this.dateTimeGrouping, true), t));
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
                            else if ((Boolean)values.First())
                            {
                                nodes.Add(TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_true"), true));
                            }
                            else
                                nodes.Add(TripleTreeNode.CreateNode(this.RM.GetString("tripletreenode_boolean_false"), false));
                        }
                        else
                        {
                            foreach (var v in values.OrderBy(o => o))
                                nodes.Add(TripleTreeNode.CreateNode(v.ToString(), v));
                        }
                    }
                }
            }

            return nodes;
        }

        private IEnumerable<TripleTreeNode> RecreateDateTimeNodes(IEnumerable<TripleTreeNode> nodes)
        {
            var oldNodes = nodes.SelectMany(n => n.AllNodes).Where(n => n.NodeType == TripleTreeNodeType.MSecDateTimeNode || n.NodeType == TripleTreeNodeType.EmptysNode || n.NodeType == TripleTreeNodeType.AllsNode).ToList();
            var newNodes = this.CreateNodesList(oldNodes.Where(n => n.NodeType != TripleTreeNodeType.AllsNode).Select(n => n.Value));

            if (newNodes.Count() > 0)
            {
                oldNodes = oldNodes.Where(n => n.CheckState == CheckState.Checked).ToList();
                if (oldNodes.Count() > 0)
                {
                    if (oldNodes.Any(n => n.NodeType == TripleTreeNodeType.AllsNode))
                    {
                        this.SetNodesCheckedState(newNodes, true);
                    }
                    else
                    {
                        foreach (var node in newNodes.SelectMany(n => n.AllNodes).Where(n => n.NodeType == TripleTreeNodeType.MSecDateTimeNode || n.NodeType == TripleTreeNodeType.EmptysNode))
                        {
                            if (oldNodes.Any(n => n.Value.Equals(node.Value)))
                            {
                                node.CheckState = CheckState.Checked;
                                oldNodes.RemoveAll(n => n.Value.Equals(node.Value));
                            }
                            else
                                node.CheckState = CheckState.Unchecked;
                        }

                        var allsNode = newNodes.FirstOrDefault(n => n.NodeType == TripleTreeNodeType.AllsNode);
                        if (allsNode != null)
                            allsNode.CheckState = this.UpdateNodesCheckState(newNodes);
                    }
                }
                return newNodes;
            }
            else
                return null;
        }

        private IEnumerable<TripleTreeNode> CloneNodes(IEnumerable<TripleTreeNode> nodes)
        {
            if (nodes != null && nodes.Count() > 0)
            {
                var result = new TripleTreeNode[nodes.Count()];
                foreach (TripleTreeNode n in nodes)
                    result[n.Index] = n.Clone();

                return result;
            }
            else
                return null;
        }

        private IEnumerable<TripleTreeNode> CloneNodes(TreeNodeCollection nodes)
        {
            if (nodes != null && nodes.Count > 0)
            {
                var result = new TripleTreeNode[nodes.Count];
                foreach (TripleTreeNode n in nodes)
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
                    this.SetNodesCheckedState(node.Nodes, isChecked);
            }
        }

        private void SetNodesCheckedState(TreeNodeCollection nodes, Boolean isChecked)
        {
            foreach (TripleTreeNode node in nodes)
            {
                node.Checked = isChecked;
                if (node.Nodes.Count > 0)
                    this.SetNodesCheckedState(node.Nodes, isChecked);
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

        private void ChangeNodeCheck(TripleTreeNode node)
        {
            if (node.CheckState == CheckState.Checked)
                node.CheckState = CheckState.Unchecked;
            else
                node.CheckState = CheckState.Checked;

            if (node.NodeType == TripleTreeNodeType.AllsNode)
            {
                this.SetNodesCheckedState(this.CheckList.Nodes, node.Checked);
                this.okButton.Enabled = node.Checked;
            }
            else
            {
                if (node.Nodes.Count > 0)
                {
                    this.SetNodesCheckedState(node.Nodes, node.Checked);
                }

                CheckState state = this.UpdateNodesCheckState(this.CheckList.Nodes);

                if (this.allsNode != null)
                    this.allsNode.CheckState = state;

                this.okButton.Enabled = !(state == CheckState.Unchecked);
            }
        }

        private string CreateFilterString()
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

        private void SetCustomFilter(int filtersMenuItemIndex)
        {
            String newFilterString = this.FiltersMenuItem.DropDownItems[filtersMenuItemIndex].Tag.ToString();
            String newViewFilterString = this.FiltersMenuItem.DropDownItems[filtersMenuItemIndex].Text;

            if (filtersMenuItemIndex != 2)
            {
                for (int i = filtersMenuItemIndex; i > 2; i--)
                {
                    this.FiltersMenuItem.DropDownItems[i].Text = this.FiltersMenuItem.DropDownItems[i - 1].Text;
                    this.FiltersMenuItem.DropDownItems[i].Tag = this.FiltersMenuItem.DropDownItems[i - 1].Tag;
                }

                this.FiltersMenuItem.DropDownItems[2].Text = newViewFilterString;
                this.FiltersMenuItem.DropDownItems[2].Tag = newFilterString;
            }

            for (int i = 3; i < FiltersMenuItem.DropDownItems.Count; i++)
            {
                (this.FiltersMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked = false;
            }

            (this.FiltersMenuItem.DropDownItems[2] as ToolStripMenuItem).Checked = true;

            this.SetNodesCheckedState(this.CheckList.Nodes, false);
            this.filterNodes = this.CloneNodes(this.CheckList.Nodes);
            this.ActiveFilterType = FilterMenuFilterType.Custom;
            this.FiltersMenuItem.Checked = true;
            this.okButton.Enabled = false;
            
            if (newFilterString != this.FilterString)
            {
                this.FilterString = newFilterString;    
                this.FilterChanged(this, new EventArgs());
            }
        }
      
        private void SetSortingUI(FilterMenuSortType sort)
        {
            this.ActiveSortType = sort;

            switch (sort)
            {
                case FilterMenuSortType.ASC:
                    this.SortASCMenuItem.Checked = true;
                    this.SortDESCMenuItem.Checked = false;
                    this.SortString = "[{0}] ASC";
                    break;
                case FilterMenuSortType.DESC:
                    this.SortASCMenuItem.Checked = false;
                    this.SortDESCMenuItem.Checked = true;
                    this.SortString = "[{0}] DESC";
                    break;
                default:
                    this.SortASCMenuItem.Checked = false;
                    this.SortDESCMenuItem.Checked = false;
                    this.SortString = null;
                    break;
            }
        }

        #region Interface Events

        #region CheckList Interface Events
        
        private void CheckList_MouseLeave(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void CheckList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TripleTreeNode n = e.Node as TripleTreeNode;
            SetNodesCheckedState(this.CheckList.Nodes, false);
            n.CheckState = CheckState.Unchecked;
            ChangeNodeCheck(n);
            this.okButton_Click(this, new EventArgs());
        }

        private void CheckList_MouseEnter(object sender, EventArgs e)
        {
            this.CheckList.Focus();
        }

        private void CheckList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeViewHitTestInfo HitTestInfo = this.CheckList.HitTest(e.X, e.Y);
            if (HitTestInfo != null && HitTestInfo.Location == TreeViewHitTestLocations.StateImage)
            {
                ChangeNodeCheck(e.Node as TripleTreeNode);
                this.checkListChanged = true;
            }
        }

        private void CheckList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                ChangeNodeCheck(this.CheckList.SelectedNode as TripleTreeNode);
                this.checkListChanged = true;
            }
        }

        #endregion CheckList Interface Events

        #region Sorting Interface events

        private void SortASCMenuItem_Click(object sender, EventArgs e)
        {
            this.SetSortingUI(FilterMenuSortType.ASC);

            if (this.checkListChanged)
                this.LoadNodes(this.startingNodes);

            this.SortChanged(this, new EventArgs());
        }
        
        private void SortDESCMenuItem_Click(object sender, EventArgs e)
        {
            this.SetSortingUI(FilterMenuSortType.DESC);

            if (this.checkListChanged)
                this.LoadNodes(this.startingNodes);

            this.SortChanged(this, new EventArgs());
        }

        private void CancelSortMenuItem_Click(object sender, EventArgs e)
        {
            if (this.checkListChanged)
                this.LoadNodes(this.startingNodes);

            this.ClearSorting(FilterMenuSortType.None, true);
        }

        #endregion Sorting Interface events

        #region CustomFilter Interface events

        private void SetupFilterMenuItem_Click(object sender, EventArgs e)
        {
            SetFilterForm flt = new SetFilterForm(this.DataType);
            if (flt.ShowDialog() == DialogResult.OK)
            {
                Int32 index = -1;

                for (Int32 i = 2; i < this.FiltersMenuItem.DropDownItems.Count; i++)
                {
                    if (this.FiltersMenuItem.DropDown.Items[i].Available)
                    {
                        if (this.FiltersMenuItem.DropDownItems[i].Text == flt.ViewFilterString && this.FiltersMenuItem.DropDownItems[i].Tag.ToString() == flt.FilterString)
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
                    for (Int32 i = this.FiltersMenuItem.DropDownItems.Count - 2; i > 1; i--)
                    {
                        if (this.FiltersMenuItem.DropDownItems[i].Available)
                        {
                            this.FiltersMenuItem.DropDownItems[i + 1].Text = this.FiltersMenuItem.DropDownItems[i].Text;
                            this.FiltersMenuItem.DropDownItems[i + 1].Tag = this.FiltersMenuItem.DropDownItems[i].Tag;
                        }
                    }
                    index = 2;

                    this.FiltersMenuItem.DropDownItems[2].Text = flt.ViewFilterString;
                    this.FiltersMenuItem.DropDownItems[2].Tag = flt.FilterString;
                }

                this.SetCustomFilter(index);
            }
        }

        private void lastfilterMenuItem_VisibleChanged(object sender, EventArgs e)
        {
            this.toolStripSeparator2MenuItem.Visible = false;
            this.lastfilter1MenuItem.VisibleChanged -= lastfilterMenuItem_VisibleChanged;
        }

        private void lastfilterMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuitem = sender as ToolStripMenuItem;

            for (int i = 2; i < FiltersMenuItem.DropDownItems.Count; i++)
            {
                if (FiltersMenuItem.DropDownItems[i].Text == menuitem.Text && FiltersMenuItem.DropDownItems[i].Tag.ToString() == menuitem.Tag.ToString())
                {
                    this.SetCustomFilter(i);
                    break;
                }
            }
        }

        private void lastfilterMenuItem_TextChanged(object sender, EventArgs e)
        {
            (sender as ToolStripMenuItem).Available = true;
            (sender as ToolStripMenuItem).TextChanged -= lastfilterMenuItem_TextChanged;
        }

        #endregion CustomFilter Interface events

        private void MenuItem_MouseEnter(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Enabled)
                (sender as ToolStripMenuItem).Select();
        }

        private void FilterContextMenu_Closed(Object sender, EventArgs e)
        {
            this.ClearResizeBox();
            this.startingNodes = null;
        }

        private void FilterContextMenu_LostFocus(Object sender, EventArgs e)
        {
            if (!this.ContainsFocus)
                this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.FiltersMenuItem.Checked = false;

            if (this.allsNode != null && this.allsNode.Checked)
                CancelFilterMenuItem_Click(null, new EventArgs());
            else
            {
                this.ActiveFilterType = FilterMenuFilterType.CheckList;
                string newfilter = "";

                if (this.CheckList.Nodes.Count > 1)
                {
                    if (this.CheckList.Nodes.Count > 2 || this.emptysNode == null)
                    {
                        String filter = this.CreateFilterString();

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
                        if (newfilter != "")
                            newfilter = "[{0}] IS NULL OR " + newfilter;
                        else
                            newfilter = "[{0}] IS NULL";
                    }
                    else if (this.DataType == FilterDataType.Boolean && newfilter == "")
                    {
                        newfilter = "[{0}] NOT IS NULL";
                    }
                }

                if (newfilter != this.FilterString)
                {
                    this.filterNodes = this.CloneNodes(this.CheckList.Nodes);
                    this.FilterString = newfilter;

                    this.FilterChanged(this, new EventArgs());
                }
            }
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (this.checkListChanged)
                this.LoadNodes(this.startingNodes);
            this.Close();
        }

        private void FiltersMenuItem_Paint(Object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(this.FiltersMenuItem.Width - 12, 7, 10, 10);
            ControlPaint.DrawMenuGlyph(e.Graphics, rect, MenuGlyph.Arrow, Color.Black, Color.Transparent);
        }

        private void CancelFilterMenuItem_Click(object sender, EventArgs e)
        {
            this.ClearFilter();
            FilterChanged(this, new EventArgs());
        }

        #endregion Interface Events

        #region Resize Events

        private void ResizePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.ClearResizeBox();
            }
        }

        private void ResizePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    this.PaintResizeBox(e.X, e.Y);
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

                        this.ResizeMenu(newWidth, newHeight);
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

            Point StartPoint = this.PointToScreen(new Point(1, 1));
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
                Point StartPoint = this.PointToScreen(new Point(1, 1));

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
    }
}