using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ADGV
{
    public class AdvancedDataGridView : DataGridView
    {
        private List<string> sortOrder = new List<string>();
        private List<string> filterOrder = new List<string>();
        private List<string> readyToShowFilters = new List<string>();

        private string sortString = null;
        private string filterString = null;

        public ADGVFilterMenuDateTimeGrouping DefaultDateTimeGrouping {get; set;}

        public ADGVColumnHeaderCellBehavior DefaultCellBehavior {get; set;}

        public event EventHandler SortStringChanged = delegate { };

        public event EventHandler FilterStringChanged = delegate { };

        private IEnumerable<ADGVColumnHeaderCell> filterCells
        {
            get
            {
                return from DataGridViewColumn c in this.Columns
                       where c.HeaderCell != null && c.HeaderCell is ADGVColumnHeaderCell
                       select (c.HeaderCell as ADGVColumnHeaderCell);
            }
        }

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
                    this.SortStringChanged(this, new EventArgs());
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
                    this.FilterStringChanged(this, new EventArgs());
                }
            }
        }

        public AdvancedDataGridView()
        {
            this.DefaultCellBehavior = ADGVColumnHeaderCellBehavior.SortingFiltering;
            this.DefaultDateTimeGrouping = ADGVFilterMenuDateTimeGrouping.Second;
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;
            ADGVColumnHeaderCell cell = new ADGVColumnHeaderCell(e.Column.HeaderCell, this.DefaultCellBehavior);
            cell.SortChanged += new ADGVFilterEventHandler(eSortChanged);
            cell.FilterChanged += new ADGVFilterEventHandler(eFilterChanged);
            cell.FilterPopup += new ADGVFilterEventHandler(eFilterPopup);
            cell.DateTimeGrouping = this.DefaultDateTimeGrouping;
            e.Column.MinimumWidth = cell.MinimumSize.Width;
            if (this.ColumnHeadersHeight < cell.MinimumSize.Height)
                this.ColumnHeadersHeight = cell.MinimumSize.Height;
            e.Column.HeaderCell = cell;

            base.OnColumnAdded(e);

        }

        protected override void OnColumnRemoved(DataGridViewColumnEventArgs e)
        {
            this.readyToShowFilters.Remove(e.Column.Name);
            this.filterOrder.Remove(e.Column.Name);
            this.sortOrder.Remove(e.Column.Name);

            ADGVColumnHeaderCell cell = e.Column.HeaderCell as ADGVColumnHeaderCell;
            if (cell != null)
            {
                cell.SortChanged -= eSortChanged;
                cell.FilterChanged -= eFilterChanged;
                cell.FilterPopup -= eFilterPopup;
                cell.Dispose();
            }
            base.OnColumnRemoved(e);
        }

        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            this.readyToShowFilters.Clear();
            base.OnRowsAdded(e);
        }

        protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
        {
            this.readyToShowFilters.Clear();
            base.OnRowsRemoved(e);
        }

        protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
        {
            this.readyToShowFilters.Remove(this.Columns[e.ColumnIndex].Name);
            base.OnCellValueChanged(e);
        }

        private void eFilterPopup(object sender, ADGVFilterEventArgs e)
        {
            if (this.Columns.Contains(e.Column))
            {
                System.Drawing.Rectangle rect = this.GetCellDisplayRectangle(e.Column.Index, -1, true);

                if (this.readyToShowFilters.Contains(e.Column.Name))
                    e.FilterMenu.Show(this, rect.Left, rect.Bottom, false);
                else
                {
                    this.readyToShowFilters.Add(e.Column.Name);

                    if (this.filterOrder.Count() > 0 && this.filterOrder.Last() == e.Column.Name)
                        e.FilterMenu.Show(this, rect.Left, rect.Bottom, true);
                    else
                        e.FilterMenu.Show(this, rect.Left, rect.Bottom, ADGVFilterMenu.GetValuesForFilter(this, e.Column.Name));
                }
            }
        }

        private void eFilterChanged(object sender, ADGVFilterEventArgs e)
        {
            if (this.Columns.Contains(e.Column))
            {
                this.filterOrder.Remove(e.Column.Name);
                if (e.FilterMenu.ActiveFilterType != ADGVFilterType.None)
                    this.filterOrder.Add(e.Column.Name);

                this.FilterString = this.CreateFilterString();
            }
        }

        private void eSortChanged(object sender, ADGVFilterEventArgs e)
        {
            if (this.Columns.Contains(e.Column))
            {
                this.sortOrder.Remove(e.Column.Name);
                
                if (e.FilterMenu.ActiveSortType != ADGVSortType.None)
                    this.sortOrder.Add(e.Column.Name);

                this.SortString = this.CreateSortString();
            }
        }

        private String CreateFilterString()
        {
            StringBuilder sb = new StringBuilder("");

            foreach (string name in this.filterOrder)
            {
                DataGridViewColumn column = this.Columns[name];

                if (column != null)
                {
                    ADGVColumnHeaderCell cell = column.HeaderCell as ADGVColumnHeaderCell;
                    if (cell != null && cell.ActiveFilterType != ADGVFilterType.None)
                    {
                        sb.AppendFormat("(" + cell.FilterString + ") AND ", column.DataPropertyName);
                    }
                }
            }
            if (sb.Length > 4)
                sb.Length -= 4;
            
            return sb.ToString();
        }

        private String CreateSortString()
        {
            StringBuilder sb = new StringBuilder("");

            foreach (string name in this.sortOrder)
            {
                DataGridViewColumn column = this.Columns[name];

                if (column != null)
                {
                    ADGVColumnHeaderCell cell = column.HeaderCell as ADGVColumnHeaderCell;
                    if (cell != null && cell.ActiveSortType != ADGVSortType.None)
                    {
                        sb.AppendFormat(cell.SortString + ", ", column.DataPropertyName);
                    }
                }
            }

            if (sb.Length > 2)
                sb.Length -= 2;

            return sb.ToString();
        }

        public void SetFilterBehavior(ADGVColumnHeaderCellBehavior behavior, DataGridViewColumn column = null)
        {
            if (column == null)
            {
                foreach(DataGridViewColumn c in this.Columns)
                {
                    ADGVColumnHeaderCell cell = c.HeaderCell as ADGVColumnHeaderCell;
                    if (cell != null)
                        cell.CellBehavior = behavior;
                }
            }
            else if (this.Columns.Contains(column))
            {
                ADGVColumnHeaderCell cell = column.HeaderCell as ADGVColumnHeaderCell;
                if (cell != null)
                    cell.CellBehavior = behavior;
            }
        }

        public void SetFilterDateTimeGrouping(ADGVFilterMenuDateTimeGrouping grouping, DataGridViewColumn column = null)
        {
            if (column == null)
            {
                foreach (DataGridViewColumn c in this.Columns)
                {
                    ADGVColumnHeaderCell cell = c.HeaderCell as ADGVColumnHeaderCell;
                    if (cell != null)
                        cell.DateTimeGrouping = grouping;
                }
            }
            else if (this.Columns.Contains(column))
            {
                ADGVColumnHeaderCell cell = column.HeaderCell as ADGVColumnHeaderCell;
                if (cell != null)
                    cell.DateTimeGrouping = grouping;
            }
        }

        public void LoadFilter(string filter, string sorting = null)
        {
            this.filterOrder.Clear();
            this.sortOrder.Clear();
            this.readyToShowFilters.Clear();

            if (filter != null)
                this.FilterString = filter;
            if (sorting != null)
                this.SortString = sorting;
        }

        public void ClearSort(bool fireEvent = false, DataGridViewColumn column = null)
        {
            if (column == null)
            {
                foreach (ADGVColumnHeaderCell c in this.filterCells)
                    c.ClearSorting();

                this.sortOrder.Clear();
            }
            else if (this.Columns.Contains(column))
            {
                ADGVColumnHeaderCell cell = column.HeaderCell as ADGVColumnHeaderCell;

                if (cell != null)
                {
                    cell.ClearSorting();
                    this.sortOrder.Remove(column.Name);
                }
            }

            if (fireEvent)
                this.SortString = null;
            else
                this.sortString = null;
        }
        
        public void ClearFilter(bool fireEvent = false, DataGridViewColumn column = null)
        {
            if (column == null)
            {
                foreach (ADGVColumnHeaderCell c in this.filterCells)
                    c.ClearFilter();

                this.filterOrder.Clear();
            }
            else if (this.Columns.Contains(column))
            {
                ADGVColumnHeaderCell cell = column.HeaderCell as ADGVColumnHeaderCell;
                
                if (cell != null)
                {
                    cell.ClearFilter();
                    this.filterOrder.Remove(column.Name);
                }
            }

            if (fireEvent)
                this.FilterString = null;
            else
                this.filterString = null;
        }

        public DataGridViewCell FindCell(string valueToFind, string columnName = null, int startRowIndex = 0, int startColumnIndex = 0, bool isWholeWordSearch = true, bool isCaseSensitive = false)
        {
            if (valueToFind != null && this.RowCount > 0 && this.ColumnCount > 0 && (columnName == null || (this.Columns.Contains(columnName) && this.Columns[columnName].Visible)))
            {
                startRowIndex = Math.Max(0, startRowIndex);

                if (!isCaseSensitive)
                    valueToFind = valueToFind.ToLower();

                if (columnName != null)
                {
                    int c = this.Columns[columnName].Index;
                    if (startColumnIndex > c)
                        startRowIndex++;
                    for (int r = startRowIndex; r < this.RowCount; r++)
                    {
                        string value = this.Rows[r].Cells[c].FormattedValue.ToString();
                        if (!isCaseSensitive)
                            value = value.ToLower();

                        if ((!isWholeWordSearch && value.Contains(valueToFind)) || value.Equals(valueToFind))
                            return this.Rows[r].Cells[c];
                    }
                }
                else
                {
                    startColumnIndex = Math.Max(0, startColumnIndex);

                    for (int r = startRowIndex; r < this.RowCount; r++)
                    {
                        for (int c = startColumnIndex; c < this.ColumnCount; c++)
                        {
                            string value = this.Rows[r].Cells[c].FormattedValue.ToString();
                            if (!isCaseSensitive)
                                value = value.ToLower();

                            if ((!isWholeWordSearch && value.Contains(valueToFind)) || value.Equals(valueToFind))
                                return this.Rows[r].Cells[c];
                        }

                        startColumnIndex = 0;
                    }
                }
            }

            return null;
        }
    }
}