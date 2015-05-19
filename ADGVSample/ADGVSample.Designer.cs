namespace ADGVSample
{
    partial class ADGVSample
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.bindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataSet = new System.Data.DataSet();
            this.dataGridView = new ADGV.AdvancedDataGridView();
            this.searchToolBar = new ADGV.SearchToolBar();
            this.clearFiltersButton = new System.Windows.Forms.ToolStripButton();
            this.clearSortButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.searchButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.columnComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.behaviorComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.dateTimeGroupingLabel = new System.Windows.Forms.ToolStripLabel();
            this.timeGroupingComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip.Location = new System.Drawing.Point(0, 342);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(987, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // bindingSource
            // 
            this.bindingSource.AllowNew = true;
            this.bindingSource.DataSource = this.dataSet;
            this.bindingSource.Position = 0;
            this.bindingSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.bindingSource_ListChanged);
            // 
            // dataSet
            // 
            this.dataSet.DataSetName = "NewDataSet";
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToOrderColumns = true;
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ColumnHeadersHeight = 22;
            this.dataGridView.DataSource = this.bindingSource;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 67);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(987, 275);
            this.dataGridView.TabIndex = 2;
            this.dataGridView.SortStringChanged += new System.EventHandler(this.dataGridView_SortStringChanged);
            this.dataGridView.FilterStringChanged += new System.EventHandler(this.dataGridView_FilterStringChanged);
            // 
            // searchToolBar
            // 
            this.searchToolBar.AllowMerge = false;
            this.searchToolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.searchToolBar.Location = new System.Drawing.Point(0, 40);
            this.searchToolBar.MaximumSize = new System.Drawing.Size(0, 27);
            this.searchToolBar.MinimumSize = new System.Drawing.Size(0, 27);
            this.searchToolBar.Name = "searchToolBar";
            this.searchToolBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.searchToolBar.Size = new System.Drawing.Size(987, 27);
            this.searchToolBar.TabIndex = 3;
            this.searchToolBar.Text = "searchToolBar1";
            this.searchToolBar.Search += new ADGV.SearchToolBarSearchEventHandler(this.searchToolBar_Search);
            this.searchToolBar.VisibleChanged += new System.EventHandler(this.searchToolBar_VisibleChanged);
            // 
            // clearFiltersButton
            // 
            this.clearFiltersButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.clearFiltersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.clearFiltersButton.Name = "clearFiltersButton";
            this.clearFiltersButton.Size = new System.Drawing.Size(69, 37);
            this.clearFiltersButton.Text = "ClearFilters";
            this.clearFiltersButton.Click += new System.EventHandler(this.clearFilterButton_Click);
            // 
            // clearSortButton
            // 
            this.clearSortButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.clearSortButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.clearSortButton.Name = "clearSortButton";
            this.clearSortButton.Size = new System.Drawing.Size(59, 37);
            this.clearSortButton.Text = "ClearSort";
            this.clearSortButton.Click += new System.EventHandler(this.clearSortButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 40);
            // 
            // searchButton
            // 
            this.searchButton.CheckOnClick = true;
            this.searchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.searchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(46, 37);
            this.searchButton.Text = "Search";
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripSeparator3});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(83, 37);
            this.toolStripDropDownButton1.Text = "Saved filters";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(171, 22);
            this.toolStripMenuItem1.Text = "Save filter and sort";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(168, 6);
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearFiltersButton,
            this.clearSortButton,
            this.toolStripSeparator1,
            this.searchButton,
            this.toolStripSeparator4,
            this.columnComboBox,
            this.toolStripLabel1,
            this.behaviorComboBox,
            this.dateTimeGroupingLabel,
            this.timeGroupingComboBox,
            this.toolStripSeparator2,
            this.toolStripDropDownButton1});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(987, 40);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // columnComboBox
            // 
            this.columnComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.columnComboBox.MaxDropDownItems = 20;
            this.columnComboBox.Name = "columnComboBox";
            this.columnComboBox.Size = new System.Drawing.Size(121, 40);
            this.columnComboBox.SelectedIndexChanged += new System.EventHandler(this.columnComboBox_SelectedIndexChanged);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(56, 37);
            this.toolStripLabel1.Text = "Behavior:";
            // 
            // behaviorComboBox
            // 
            this.behaviorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.behaviorComboBox.MaxDropDownItems = 20;
            this.behaviorComboBox.Name = "behaviorComboBox";
            this.behaviorComboBox.Size = new System.Drawing.Size(121, 40);
            this.behaviorComboBox.SelectedIndexChanged += new System.EventHandler(this.behaviorComboBox_SelectedIndexChanged);
            // 
            // dateTimeGroupingLabel
            // 
            this.dateTimeGroupingLabel.Name = "dateTimeGroupingLabel";
            this.dateTimeGroupingLabel.Size = new System.Drawing.Size(111, 37);
            this.dateTimeGroupingLabel.Text = "DateTimeGrouping:";
            // 
            // timeGroupingComboBox
            // 
            this.timeGroupingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.timeGroupingComboBox.MaxDropDownItems = 20;
            this.timeGroupingComboBox.Name = "timeGroupingComboBox";
            this.timeGroupingComboBox.Size = new System.Drawing.Size(121, 40);
            this.timeGroupingComboBox.SelectedIndexChanged += new System.EventHandler(this.timeGroupingComboBox_SelectedIndexChanged);
            // 
            // ADGVSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 364);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.searchToolBar);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.statusStrip);
            this.Name = "ADGVSample";
            this.Text = "ADGVSample";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.BindingSource bindingSource;
        private ADGV.AdvancedDataGridView dataGridView;
        private System.Data.DataSet dataSet;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private ADGV.SearchToolBar searchToolBar;
        private System.Windows.Forms.ToolStripButton clearFiltersButton;
        private System.Windows.Forms.ToolStripButton clearSortButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton searchButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripComboBox timeGroupingComboBox;
        private System.Windows.Forms.ToolStripLabel dateTimeGroupingLabel;
        private System.Windows.Forms.ToolStripComboBox columnComboBox;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox behaviorComboBox;
    }
}

