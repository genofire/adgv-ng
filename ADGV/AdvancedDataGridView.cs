using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ADGV
{
    public class AdvancedDataGridView : DataGridView
    {
        private List<String> sortOrder = new List<String>();
        private List<String> filterOrder = new List<String>();
        private List<String> readyToShowFilters = new List<String>();

        private String sortString = null;
        private String filterString = null;
        private Boolean atoGenerateContextFilters = true;
        private Boolean dateWithTime = false;
        private Boolean timeFilter = false;
        private Boolean loadedFilter = false;

        public Boolean AutoGenerateContextFilters
        {
            get
            {
                return this.atoGenerateContextFilters;
            }
            set
            {
                this.atoGenerateContextFilters = value;
            }
        }

        public Boolean DateWithTime
        {
            get
            {
                return this.dateWithTime;
            }
            set
            {
                this.dateWithTime = value;
            }
        }

        public Boolean TimeFilter
        {
            get
            {
                return this.timeFilter;
            }
            set
            {
                this.timeFilter = value;
            }
        }

        public event EventHandler SortStringChanged;

        public event EventHandler FilterStringChanged;

        private IEnumerable<ADGVColumnHeaderCell> filterCells
        {
            get
            {
                return from DataGridViewColumn c in this.Columns
                       where c.HeaderCell != null && c.HeaderCell is ADGVColumnHeaderCell
                       select (c.HeaderCell as ADGVColumnHeaderCell);
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
                String old = value;
                if (old != this.sortString)
                {
                    this.sortString = value;
                    if (this.SortedColumn != null)
                    {
                        this.SortedColumn.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.None;
                    }
                    if (this.SortStringChanged != null)
                        SortStringChanged(this, new EventArgs());
                }
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
                String old = value;
                if (old != this.filterString)
                {
                    this.filterString = value;
                    if (this.FilterStringChanged != null)
                        FilterStringChanged(this, new EventArgs());
                }
            }
        }

        public AdvancedDataGridView()
        {
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;
            ADGVColumnHeaderCell cell = new ADGVColumnHeaderCell(e.Column.HeaderCell, this.AutoGenerateContextFilters);
            cell.DateWithTime = this.DateWithTime;
            cell.TimeFilter = this.TimeFilter;
            cell.SortChanged += new ADGVFilterEventHandler(eSortChanged);
            cell.FilterChanged += new ADGVFilterEventHandler(eFilterChanged);
            cell.FilterPopup += new ADGVFilterEventHandler(eFilterPopup);
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
                ADGVFilterMenu FilterMenu = e.FilterMenu;
                DataGridViewColumn Column = e.Column;

                System.Drawing.Rectangle rect = this.GetCellDisplayRectangle(Column.Index, -1, true);

                if (this.readyToShowFilters.Contains(Column.Name))
                    FilterMenu.Show(this, rect.Left, rect.Bottom, false);
                else
                {
                    this.readyToShowFilters.Add(Column.Name);

                    if (filterOrder.Count() > 0 && filterOrder.Last() == Column.Name)
                        FilterMenu.Show(this, rect.Left, rect.Bottom, true);
                    else
                        FilterMenu.Show(this, rect.Left, rect.Bottom, ADGVFilterMenu.GetValuesForFilter(this, Column.Name));
                }
            }
        }

        private void eFilterChanged(object sender, ADGVFilterEventArgs e)
        {
            if (this.Columns.Contains(e.Column))
            {
                ADGVFilterMenu FilterMenu = e.FilterMenu;
                DataGridViewColumn Column = e.Column;

                this.filterOrder.Remove(Column.Name);
                if (FilterMenu.ActiveFilterType != ADGVFilterMenuFilterType.None)
                    this.filterOrder.Add(Column.Name);

                this.FilterString = CreateFilterString();

                if (this.loadedFilter)
                {
                    this.loadedFilter = false;
                    foreach (ADGVColumnHeaderCell c in this.filterCells.Where(f => f.FilterMenu != FilterMenu))
                        c.SetLoadedFilterMode(false);
                }
            }
        }

        private void eSortChanged(object sender, ADGVFilterEventArgs e)
        {
            if (this.Columns.Contains(e.Column))
            {
                ADGVFilterMenu FilterMenu = e.FilterMenu;
                DataGridViewColumn Column = e.Column;

                this.sortOrder.Remove(Column.Name);
                if (FilterMenu.ActiveSortType != ADGVFilterMenuSortType.None)
                    this.sortOrder.Add(Column.Name);
                this.SortString = CreateSortString();
            }
        }

        private String CreateFilterString()
        {
            StringBuilder sb = new StringBuilder("");
            String appx = "";

            foreach (String name in this.filterOrder)
            {
                DataGridViewColumn Column = this.Columns[name];

                if (Column != null)
                {
                    ADGVColumnHeaderCell cell = Column.HeaderCell as ADGVColumnHeaderCell;
                    if (cell != null)
                    {
                        if (cell.FilterEnabled && cell.ActiveFilterType != ADGVFilterMenuFilterType.None)
                        {
                            sb.AppendFormat(appx + "(" + cell.FilterString + ")", Column.DataPropertyName);
                            appx = " AND ";
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private String CreateSortString()
        {
            StringBuilder sb = new StringBuilder("");
            String appx = "";

            foreach (String name in this.sortOrder)
            {
                DataGridViewColumn Column = this.Columns[name];

                if (Column != null)
                {
                    ADGVColumnHeaderCell cell = Column.HeaderCell as ADGVColumnHeaderCell;
                    if (cell != null)
                    {
                        if (cell.FilterEnabled && cell.ActiveSortType != ADGVFilterMenuSortType.None)
                        {
                            sb.AppendFormat(appx + cell.SortString, Column.DataPropertyName);
                            appx = ", ";
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public void EnableFilter(DataGridViewColumn Column)
        {
            if (this.Columns.Contains(Column))
            {
                ADGVColumnHeaderCell c = Column.HeaderCell as ADGVColumnHeaderCell;
                if (c != null)
                    this.EnableFilter(Column, c.DateWithTime, c.TimeFilter);
                else
                    this.EnableFilter(Column, this.DateWithTime, this.TimeFilter);
            }
        }

        public void EnableFilter(DataGridViewColumn Column, Boolean DateWithTime, Boolean TimeFilter)
        {
            if (this.Columns.Contains(Column))
            {
                ADGVColumnHeaderCell cell = Column.HeaderCell as ADGVColumnHeaderCell;
                if (cell != null)
                {
                    if (cell.DateWithTime != DateWithTime || cell.TimeFilter != TimeFilter || (!cell.FilterEnabled && (cell.FilterString.Length > 0 || cell.SortString.Length > 0)))
                        this.ClearFilter(true);

                    cell.DateWithTime = DateWithTime;
                    cell.TimeFilter = TimeFilter;
                    cell.FilterEnabled = true;
                    this.readyToShowFilters.Remove(Column.Name);
                }
                else
                {
                    Column.SortMode = DataGridViewColumnSortMode.Programmatic;
                    cell = new ADGVColumnHeaderCell(Column.HeaderCell, true);
                    cell.DateWithTime = this.DateWithTime;
                    cell.TimeFilter = this.TimeFilter;
                    cell.SortChanged += new ADGVFilterEventHandler(eSortChanged);
                    cell.FilterChanged += new ADGVFilterEventHandler(eFilterChanged);
                    cell.FilterPopup += new ADGVFilterEventHandler(eFilterPopup);
                    Column.MinimumWidth = cell.MinimumSize.Width;
                    if (this.ColumnHeadersHeight < cell.MinimumSize.Height)
                        this.ColumnHeadersHeight = cell.MinimumSize.Height;
                    Column.HeaderCell = cell;
                }
                Column.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
        }

        protected override void OnSorted(EventArgs e)
        {
            this.ClearSort();
            base.OnSorted(e);
        }

        public void DisableFilter(DataGridViewColumn Column)
        {
            if (this.Columns.Contains(Column))
            {
                ADGVColumnHeaderCell cell = Column.HeaderCell as ADGVColumnHeaderCell;
                if (cell != null)
                {
                    if (cell.FilterEnabled == true && (cell.SortString.Length > 0 || cell.FilterString.Length > 0))
                    {
                        this.ClearFilter(true);
                        cell.FilterEnabled = false;
                    }
                    else
                        cell.FilterEnabled = false;
                    this.filterOrder.Remove(Column.Name);
                    this.sortOrder.Remove(Column.Name);
                    this.readyToShowFilters.Remove(Column.Name);
                }
                Column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }

        public void LoadFilter(String Filter, String Sorting = null)
        {
            foreach (ADGVColumnHeaderCell c in this.filterCells)
                c.SetLoadedFilterMode(true);

            this.filterOrder.Clear();
            this.sortOrder.Clear();
            this.readyToShowFilters.Clear();

            if (Filter != null)
                this.FilterString = Filter;
            if (Sorting != null)
                this.SortString = Sorting;

            this.loadedFilter = true;
        }

        public void ClearSort(Boolean FireEvent = false)
        {
            foreach (ADGVColumnHeaderCell c in this.filterCells)
                c.ClearSorting();
            this.sortOrder.Clear();

            if (FireEvent)
                this.SortString = null;
            else
                this.sortString = null;
        }

        public void ClearFilter(Boolean FireEvent = false)
        {
            foreach (ADGVColumnHeaderCell c in this.filterCells)
            {
                c.ClearFilter();
            }
            this.filterOrder.Clear();

            if (FireEvent)
                this.FilterString = null;
            else
                this.filterString = null;
        }

        public DataGridViewCell FindCell(string ValueToFind, string ColumnName = null, int RowIndex = 0, int ColumnIndex = 0, Boolean isWholeWordSearch = true, Boolean isCaseSensitive = false)
        {
            if (ValueToFind != null && this.RowCount > 0 && this.ColumnCount > 0 && (ColumnName == null || (this.Columns.Contains(ColumnName) && this.Columns[ColumnName].Visible)))
            {
                RowIndex = Math.Max(0, RowIndex);

                if (!isCaseSensitive)
                    ValueToFind = ValueToFind.ToLower();

                if (ColumnName != null)
                {
                    int c = this.Columns[ColumnName].Index;
                    if (ColumnIndex > c)
                        RowIndex++;
                    for (int r = RowIndex; r < this.RowCount; r++)
                    {
                        string value = this.Rows[r].Cells[c].FormattedValue.ToString();
                        if (!isCaseSensitive)
                            value = value.ToLower();

                        if ((!isWholeWordSearch && value.Contains(ValueToFind)) || value.Equals(ValueToFind))
                            return this.Rows[r].Cells[c];
                    }
                }
                else
                {
                    ColumnIndex = Math.Max(0, ColumnIndex);

                    for (int r = RowIndex; r < this.RowCount; r++)
                    {
                        for (int c = ColumnIndex; c < this.ColumnCount; c++)
                        {
                            string value = this.Rows[r].Cells[c].FormattedValue.ToString();
                            if (!isCaseSensitive)
                                value = value.ToLower();

                            if ((!isWholeWordSearch && value.Contains(ValueToFind)) || value.Equals(ValueToFind))
                                return this.Rows[r].Cells[c];
                        }

                        ColumnIndex = 0;
                    }
                }
            }

            return null;
        }
    }
}