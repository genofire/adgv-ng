using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ADGV;

namespace ADGVSample
{
    public partial class ADGVSample : Form
    {
        private Dictionary<int, String> filters = new Dictionary<int, String>();
        private Dictionary<int, String> sort = new Dictionary<int, String>();
        private DataTable dt;
        public ADGVSample()
        {
            InitializeComponent();
            this.dataGridView.AutoGenerateColumns = true;
            this.toolStripButton2.Checked = this.dataGridView.DateWithTime;
            this.toolStripButton1.Checked = this.dataGridView.TimeFilter;
            AddData();
        }

        private void AddData()
        {
            dt = this.dataSet.Tables.Add("Table1");
            dt.Columns.Add("boolean", typeof(Boolean));
            dt.Columns.Add("int", typeof(int));
            dt.Columns.Add("decimal", typeof(decimal));
            dt.Columns.Add("double", typeof(double));
            dt.Columns.Add("date", typeof(DateTime));
            dt.Columns.Add("string", typeof(string));
            dt.Columns.Add("guid", typeof(Guid));

            for (int i = 0; i <= 9; i++)
            {
                dt.Rows.Add(new object[] { 
                    i % 2 == 0 ? true:false, 
                    i, 
                    (decimal)i*2/3,
                    i % 2 == 0 ? (double)i*2/3 : (double)i/2, 
                    DateTime.Today.AddHours(i*2).AddHours(i%2 == 0 ?i*10+1:0).AddMinutes(i%2 == 0 ?i*10+1:0).AddSeconds(i%2 == 0 ?i*10+1:0).AddMilliseconds(i%2 == 0 ?i*10+1:0),
                    i*2 % 3 == 0 ? null : i.ToString()+" str", 
                    
                    Guid.NewGuid() 
                });
            }
            dt.Rows.Add(new object[] { null, null, null, null, null, null, null });
            this.bindingSource.DataMember = dt.TableName;
        }
        

        private void dataGridView_SortStringChanged(object sender, EventArgs e)
        {
            this.bindingSource.Sort = this.dataGridView.SortString;
        }

        private void dataGridView_FilterStringChanged(object sender, EventArgs e)
        {
            this.bindingSource.Filter = this.dataGridView.FilterString;
        }

        private void clearFilterButton_Click(object sender, EventArgs e)
        {
            this.dataGridView.ClearFilter(true);
        }

        private void clearSortButton_Click(object sender, EventArgs e)
        {
            this.dataGridView.ClearSort(true);
        }

        private void bindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Total rows - " + this.bindingSource.List.Count.ToString();
            this.searchToolBar.SetColumns(this.dataGridView.Columns);
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (this.searchButton.Checked)
                this.searchToolBar.Show();
            else
                this.searchToolBar.Hide();
        }

        private void hideColumnButton_Click(object sender, EventArgs e)
        {
            this.dataGridView.Columns["boolean"].Visible = hideColumnButton.Checked;
            this.searchToolBar.SetColumns(this.dataGridView.Columns);
        }

        private void searchToolBar_VisibleChanged(object sender, EventArgs e)
        {
            this.searchButton.Checked = this.searchToolBar.Visible;
        }

        private void searchToolBar_Search(object sender, SearchToolBarSearchEventArgs e)
        {
            int startColumn = 0;
            int startRow = 0;
            if (!e.FromBegin)
            {
                bool endcol = this.dataGridView.CurrentCell.ColumnIndex + 1 >= this.dataGridView.ColumnCount;
                bool endrow = this.dataGridView.CurrentCell.RowIndex + 1 >= this.dataGridView.RowCount;

                if (endcol && endrow)
                {
                    startColumn = this.dataGridView.CurrentCell.ColumnIndex;
                    startRow = this.dataGridView.CurrentCell.RowIndex;
                }
                else
                {
                    startColumn = endcol ? 0 : this.dataGridView.CurrentCell.ColumnIndex + 1;
                    startRow = this.dataGridView.CurrentCell.RowIndex + (endcol ? 1 : 0);
                }
            }
            DataGridViewCell c = this.dataGridView.FindCell(
                e.ValueToSearch,
                e.ColumnToSearch != null ? e.ColumnToSearch.Name : null,
                startRow,
                startColumn,
                e.WholeWord,
                e.CaseSensitive);

            if (c != null)
                this.dataGridView.CurrentCell = c;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.dataGridView.DateWithTime = !this.dataGridView.DateWithTime;
            this.dataGridView.EnableFilter(this.dataGridView.Columns["date"], this.dataGridView.DateWithTime, this.dataGridView.TimeFilter);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.dataGridView.TimeFilter = !this.dataGridView.TimeFilter;
            this.dataGridView.EnableFilter(this.dataGridView.Columns["date"], this.dataGridView.DateWithTime, this.dataGridView.TimeFilter);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (!toolStripButton3.Checked)
                this.dataGridView.DisableFilter(this.dataGridView.Columns["int"]);
            else 
                this.dataGridView.EnableFilter(this.dataGridView.Columns["int"]);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int i = filters.Count + 1;
            filters.Add(i, this.dataGridView.FilterString);
            sort.Add(i, this.dataGridView.SortString);

            ToolStripMenuItem itm = new ToolStripMenuItem(i.ToString());
            itm.Click += itm_Click;
            this.toolStripDropDownButton1.DropDownItems.Add(itm);
        }

        void itm_Click(object sender, EventArgs e)
        {
            int i = int.Parse((sender as ToolStripMenuItem).Text);
            this.dataGridView.LoadFilter(filters[i], sort[i]);
        }

        private void dataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {

        }
    }
}
