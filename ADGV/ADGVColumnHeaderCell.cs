using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADGV
{
    public enum ADGVColumnHeaderCellBehavior : byte
    {
        DisabledHidden = 0,
        Disabled,
        SortingStandartGlyph,
        Sorting,
        SortingFiltering
    }
    
    public class ADGVColumnHeaderCell : DataGridViewColumnHeaderCell
    {
        private Size filterButtonImageSize = new Size(16, 16);
        private bool filterButtonPressed = false;
        private bool filterButtonOver = false;
        private Rectangle filterButtonOffsetBounds = Rectangle.Empty;
        private Rectangle filterButtonImageBounds = Rectangle.Empty;
        private Padding filterButtonMargin = new Padding(3, 4, 3, 4);
        private ADGVColumnHeaderCellBehavior cellBehavior;

        public ADGVFilterMenuDateTimeGrouping DateTimeGrouping
        {
            get
            {
                return this.FilterMenu.DateTimeGrouping;
            }
            set
            {
                if (this.DateTimeGrouping != value)
                {
                    this.FilterMenu.DateTimeGrouping = value;
                }
            }
        }

        public ADGVFilterMenu FilterMenu { get; private set; }

        public event ADGVFilterEventHandler FilterPopup = delegate { };

        public event ADGVFilterEventHandler SortChanged = delegate { };

        public event ADGVFilterEventHandler FilterChanged = delegate { };

        public event ADGVFilterEventHandler CellBehaviorChanged = delegate { };

        public event ADGVFilterEventHandler DateTimeGroupingChanged = delegate { };

        public Size MinimumSize
        {
            get
            {
                return new Size(this.filterButtonImageSize.Width + this.filterButtonMargin.Left + this.filterButtonMargin.Right,
                    this.filterButtonImageSize.Height + this.filterButtonMargin.Bottom + this.filterButtonMargin.Top);
            }
        }

        public ADGVSortType ActiveSortType
        {
            get
            {
                return this.FilterMenu.ActiveSortType;
            }
        }

        public ADGVFilterType ActiveFilterType
        {
            get
            {
                return this.FilterMenu.ActiveFilterType;
            }
        }

        public string SortString
        {
            get
            {
                return this.FilterMenu.SortString;
            }
        }

        public string FilterString
        {
            get
            {
                return this.FilterMenu.FilterString;
            }
        }

        public ADGVColumnHeaderCellBehavior CellBehavior
        {
            get
            {
                return this.cellBehavior;
            }
            set
            {
                if (value != this.CellBehavior)
                {
                    this.cellBehavior = value;
                    this.RepaintCell();
                    this.CellBehaviorChanged(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.FilterMenu != null)
            {
                this.FilterMenu.FilterChanged -= FilterMenu_FilterChanged;
                this.FilterMenu.SortChanged -= FilterMenu_SortChanged;
                this.FilterMenu.DateTimeGroupingChanged -= FilterMenu_DateTimeGroupingChanged;
                this.FilterMenu.Dispose();
                this.FilterMenu = null;
            }

            base.Dispose(disposing);
        }

        public ADGVColumnHeaderCell(DataGridViewColumnHeaderCell oldCell, ADGVColumnHeaderCellBehavior cellBehavior)
        {
            this.Tag = oldCell.Tag;
            this.ErrorText = oldCell.ErrorText;
            this.ToolTipText = oldCell.ToolTipText;
            this.Value = oldCell.Value;
            this.ValueType = oldCell.ValueType;
            this.ContextMenuStrip = oldCell.ContextMenuStrip;
            this.Style = oldCell.Style;
            this.cellBehavior = cellBehavior;
            
            ADGVColumnHeaderCell oldADGVCell = oldCell as ADGVColumnHeaderCell;
                        
            if (oldADGVCell != null)
            {
                if (oldADGVCell.FilterMenu != null)
                    this.FilterMenu = oldADGVCell.FilterMenu.Clone() as ADGVFilterMenu;
                else
                    this.FilterMenu = new ADGVFilterMenu(oldCell.OwningColumn.ValueType);

                this.filterButtonPressed = oldADGVCell.filterButtonPressed;
                this.filterButtonOver = oldADGVCell.filterButtonOver;
                this.filterButtonOffsetBounds = oldADGVCell.filterButtonOffsetBounds;
                this.filterButtonImageBounds = oldADGVCell.filterButtonImageBounds;
            }
            else
            {
                this.FilterMenu = new ADGVFilterMenu(oldCell.OwningColumn.ValueType);
            }
                        
            this.FilterMenu.FilterChanged += FilterMenu_FilterChanged;
            this.FilterMenu.SortChanged += FilterMenu_SortChanged;
            this.FilterMenu.DateTimeGroupingChanged += FilterMenu_DateTimeGroupingChanged;
        }

        public void SetCustomFilter(string filter, string filterName = null, bool fireEvent = false)
        {
            this.FilterMenu.SetCustomFilter(filter, filterName, fireEvent);
        }

        void FilterMenu_DateTimeGroupingChanged(object sender, EventArgs e)
        {
            this.DateTimeGroupingChanged(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
        }

        public override object Clone()
        {
            return new ADGVColumnHeaderCell(this, this.CellBehavior);
        }

        private void RepaintCell()
        {
            if (this.Displayed && this.DataGridView != null)
                this.DataGridView.InvalidateCell(this);
        }

        public void ClearSorting(bool fireEvent = false)
        {
            this.FilterMenu.ClearSorting(fireEvent);
            this.RepaintCell();
        }

        public void SetSorting(ADGVSortType sort, bool fireEvent = false)
        {
            this.FilterMenu.SetSorting(sort, fireEvent);
            this.RepaintCell();
        }

        public void ClearFilter(bool fireEvent = false)
        {
            this.FilterMenu.ClearFilter(fireEvent);
            this.RepaintCell();
        }

        private void FilterMenu_FilterChanged(object sender, EventArgs e)
        {
            this.RepaintCell();
            this.FilterChanged(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
        }

        private void FilterMenu_SortChanged(object sender, EventArgs e)
        {
            this.RepaintCell();
            this.SortChanged(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
        }

        protected override void Paint(
            Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates cellState,
            object value, object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingStandartGlyph)
            {
                switch (this.ActiveSortType)
                {
                    case ADGVSortType.ASC:
                        this.SortGlyphDirection = SortOrder.Ascending;
                        break;
                    case ADGVSortType.DESC:
                        this.SortGlyphDirection = SortOrder.Descending;
                        break;
                    case ADGVSortType.None:
                        this.SortGlyphDirection = SortOrder.None;
                        break;
                }
            }
            else
                this.SortGlyphDirection = SortOrder.None;

            base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                cellState, value, formattedValue,
                errorText, cellStyle, advancedBorderStyle, paintParts);

            if (this.CellBehavior != ADGVColumnHeaderCellBehavior.SortingStandartGlyph && this.CellBehavior != ADGVColumnHeaderCellBehavior.DisabledHidden && paintParts.HasFlag(DataGridViewPaintParts.ContentBackground))
            {
                Image filterImage = Properties.Resources.AddFilter;

                if (this.cellBehavior == ADGVColumnHeaderCellBehavior.Sorting || this.ActiveFilterType == ADGVFilterType.None)
                    switch (this.ActiveSortType)
                    {
                        case ADGVSortType.ASC:
                            filterImage = Properties.Resources.ASC;
                            break;
                        case ADGVSortType.DESC:
                            filterImage = Properties.Resources.DESC;
                            break;
                        case ADGVSortType.None:
                            filterImage = Properties.Resources.AddFilter;
                            break;
                    }
                else
                    switch (this.ActiveSortType)
                    {
                        case ADGVSortType.ASC:
                            filterImage = Properties.Resources.FilterASC;
                            break;
                        case ADGVSortType.DESC:
                            filterImage = Properties.Resources.FilterDESC;
                            break;
                        case ADGVSortType.None:
                            filterImage = Properties.Resources.Filter;
                            break;
                    }

                this.filterButtonOffsetBounds = this.GetFilterButtonBounds(true);
                this.filterButtonImageBounds = this.GetFilterButtonBounds(false);
                Rectangle buttonBounds = this.filterButtonOffsetBounds;
                if (buttonBounds != null && clipBounds.IntersectsWith(buttonBounds))
                {
                    ControlPaint.DrawBorder(graphics, buttonBounds, Color.Gray, ButtonBorderStyle.Solid);
                    buttonBounds.Inflate(-1, -1);
                    using (Brush b = new SolidBrush((this.CellBehavior != ADGVColumnHeaderCellBehavior.Disabled && this.filterButtonOver) ? Color.LightGray : Color.White))
                        graphics.FillRectangle(b, buttonBounds);

                    graphics.DrawImage(filterImage, buttonBounds);
                }
            }
        }

        private Rectangle GetFilterButtonBounds(Boolean withOffset = true)
        {
            Rectangle cell = this.DataGridView.GetCellDisplayRectangle(this.ColumnIndex, -1, false);

            Point p = new Point((withOffset ? cell.Right : this.OwningColumn.Width) - this.filterButtonImageSize.Width - this.filterButtonMargin.Right,
                (withOffset ? cell.Bottom : cell.Height) - this.filterButtonImageSize.Height - this.filterButtonMargin.Bottom);

            return new Rectangle(p, this.filterButtonImageSize);
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
            {
                if (this.filterButtonImageBounds.Contains(e.X, e.Y) && !this.filterButtonOver)
                {
                    this.filterButtonOver = true;
                    this.RepaintCell();
                }
                else if (!this.filterButtonImageBounds.Contains(e.X, e.Y) && this.filterButtonOver)
                {
                    this.filterButtonOver = false;
                    this.RepaintCell();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !this.filterButtonPressed)
            {
                if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
                {
                    if (this.filterButtonImageBounds.Contains(e.X, e.Y))
                    {
                        this.filterButtonPressed = true;
                        this.filterButtonOver = true;
                        this.RepaintCell();
                    }
                }
                else if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingStandartGlyph)
                {
                    this.filterButtonPressed = true;
                    this.RepaintCell();
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.filterButtonPressed)
            {
                this.filterButtonPressed = false;

                if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingStandartGlyph)
                    switch (this.ActiveSortType)
                    {
                        case ADGVSortType.ASC:
                            this.FilterMenu.SetSorting(ADGVSortType.DESC, true);
                            break;
                        case ADGVSortType.DESC:
                            this.FilterMenu.SetSorting(ADGVSortType.None, true);
                            break;
                        case ADGVSortType.None:
                            this.FilterMenu.SetSorting(ADGVSortType.ASC, true);
                            break;
                    }
                else if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
                {
                    this.filterButtonOver = false;
                    
                    if (this.filterButtonImageBounds.Contains(e.X, e.Y))
                    {
                        if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering)
                            this.FilterPopup(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
                        else
                            switch (this.ActiveSortType)
                            {
                                case ADGVSortType.ASC:
                                    this.FilterMenu.SetSorting(ADGVSortType.DESC, true);
                                    break;
                                case ADGVSortType.DESC:
                                    this.FilterMenu.SetSorting(ADGVSortType.None, true);
                                    break;
                                case ADGVSortType.None:
                                    this.FilterMenu.SetSorting(ADGVSortType.ASC, true);
                                    break;
                            }
                    }
                }
                this.RepaintCell();
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
            {
                this.filterButtonOver = false;
                this.RepaintCell();
            }

            base.OnMouseLeave(rowIndex);
        }
    }

    public delegate void ADGVFilterEventHandler(object sender, ADGVFilterEventArgs e);

    public class ADGVFilterEventArgs : EventArgs
    {
        public ADGVFilterMenu FilterMenu { get; private set; }

        public DataGridViewColumn Column { get; private set; }

        public ADGVColumnHeaderCell HeaderCell
        {
            get
            {
                return this.Column.HeaderCell as ADGVColumnHeaderCell;
            }
        }

        public ADGVFilterEventArgs(ADGVFilterMenu filterMenu, DataGridViewColumn column)
        {
            this.FilterMenu = filterMenu;
            this.Column = column;
        }
    }
}
