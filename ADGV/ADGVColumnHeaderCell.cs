using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADGV
{
    public enum ADGVColumnHeaderCellBehavior : byte
    {
        DisabledHidden = 0,
        Disabled,
        Default,
        Sorting,
        SortingFiltering
    }
    
    public class ADGVColumnHeaderCell : DataGridViewColumnHeaderCell
    {
        private Image filterImage = Properties.Resources.AddFilter;
        private Size filterButtonImageSize = new Size(16, 16);
        private bool filterButtonPressed = false;
        private bool filterButtonOver = false;
        private Rectangle filterButtonOffsetBounds = Rectangle.Empty;
        private Rectangle filterButtonImageBounds = Rectangle.Empty;
        private Padding filterButtonMargin = new Padding(3, 4, 3, 4);
        private ADGVColumnHeaderCellBehavior cellBehavior;

        public FilterDateTimeGrouping DateTimeGrouping
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

        public Size MinimumSize
        {
            get
            {
                return new Size(this.filterButtonImageSize.Width + this.filterButtonMargin.Left + this.filterButtonMargin.Right,
                    this.filterButtonImageSize.Height + this.filterButtonMargin.Bottom + this.filterButtonMargin.Top);
            }
        }

        public FilterMenuSortType ActiveSortType
        {
            get
            {
                if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
                    return this.FilterMenu.ActiveSortType;
                else
                    return FilterMenuSortType.None;
            }
        }

        public FilterMenuFilterType ActiveFilterType
        {
            get
            {
                if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering)
                    return this.FilterMenu.ActiveFilterType;
                else
                    return FilterMenuFilterType.None;
            }
        }

        public String SortString
        {
            get
            {
                if (this.ActiveSortType != FilterMenuSortType.None)
                    return this.FilterMenu.SortString;
                else
                    return "";
            }
        }

        public String FilterString
        {
            get
            {
                if (this.ActiveFilterType != FilterMenuFilterType.None)
                    return this.FilterMenu.FilterString;
                else
                    return "";
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
                    if (value == ADGVColumnHeaderCellBehavior.Disabled || value == ADGVColumnHeaderCellBehavior.DisabledHidden)
                    {
                        this.filterButtonPressed = false;
                        this.filterButtonOver = false;
                    }

                    string filter = this.FilterString;
                    string sort = this.SortString;

                    this.cellBehavior = value;
                    this.RefreshImage();
                    this.RepaintCell();

                    if (filter != this.FilterString)
                        this.FilterMenu_FilterChanged(this, new EventArgs());

                    if (sort != this.SortString)
                        this.FilterMenu_SortChanged(this, new EventArgs());
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.FilterMenu.FilterChanged -= FilterMenu_FilterChanged;
            this.FilterMenu.SortChanged -= FilterMenu_SortChanged;
            this.FilterMenu.Dispose();
            this.FilterMenu = null;

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

            if (oldADGVCell != null && oldADGVCell.FilterMenu != null)
            {
                this.FilterMenu = oldADGVCell.FilterMenu;
                this.filterImage = oldADGVCell.filterImage;
                this.filterButtonPressed = oldADGVCell.filterButtonPressed;
                this.filterButtonOver = oldADGVCell.filterButtonOver;
                this.filterButtonOffsetBounds = oldADGVCell.filterButtonOffsetBounds;
                this.filterButtonImageBounds = oldADGVCell.filterButtonImageBounds;

                this.FilterMenu.FilterChanged += new EventHandler(FilterMenu_FilterChanged);
                this.FilterMenu.SortChanged += new EventHandler(FilterMenu_SortChanged);
            }
            else
            {
                this.FilterMenu = new ADGVFilterMenu(oldCell.OwningColumn.ValueType);
                this.FilterMenu.FilterChanged += new EventHandler(FilterMenu_FilterChanged);
                this.FilterMenu.SortChanged += new EventHandler(FilterMenu_SortChanged);
            }
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

        internal void ClearSorting(FilterMenuSortType sort = FilterMenuSortType.None, bool fireEvent = false)
        {
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
            {
                this.FilterMenu.ClearSorting(FilterMenuSortType.None, fireEvent);
                this.RefreshImage();
                this.RepaintCell();
            }
        }

        internal void ClearFilter(bool fireEvent = false)
        {
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering)
            {
                this.FilterMenu.ClearFilter(fireEvent);
                this.RefreshImage();
                this.RepaintCell();
            }
        }

        private void RefreshImage()
        {
            if (this.ActiveFilterType == FilterMenuFilterType.Loaded)
            {
                this.filterImage = Properties.Resources.SavedFilter;
            }
            else
                if (this.ActiveFilterType == FilterMenuFilterType.None)
                {
                    if (this.ActiveSortType == FilterMenuSortType.None)
                        this.filterImage = Properties.Resources.AddFilter;
                    else if (this.ActiveSortType == FilterMenuSortType.ASC)
                        this.filterImage = Properties.Resources.ASC;
                    else
                        this.filterImage = Properties.Resources.DESC;
                }
                else
                {
                    if (this.ActiveSortType == FilterMenuSortType.None)
                        this.filterImage = Properties.Resources.Filter;
                    else if (this.ActiveSortType == FilterMenuSortType.ASC)
                        this.filterImage = Properties.Resources.FilterASC;
                    else
                        this.filterImage = Properties.Resources.FilterDESC;
                }
        }

        private void FilterMenu_FilterChanged(object sender, EventArgs e)
        {
            this.RefreshImage();
            this.RepaintCell();
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering)
                this.FilterChanged(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
        }

        private void FilterMenu_SortChanged(object sender, EventArgs e)
        {
            this.RefreshImage();
            this.RepaintCell();
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting || this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering)
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
            if (this.CellBehavior != ADGVColumnHeaderCellBehavior.Default && this.CellBehavior != ADGVColumnHeaderCellBehavior.DisabledHidden && this.SortGlyphDirection != SortOrder.None)
                this.SortGlyphDirection = SortOrder.None;

            base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                cellState, value, formattedValue,
                errorText, cellStyle, advancedBorderStyle, paintParts);

            if (this.CellBehavior != ADGVColumnHeaderCellBehavior.Default && this.CellBehavior != ADGVColumnHeaderCellBehavior.DisabledHidden && paintParts.HasFlag(DataGridViewPaintParts.ContentBackground))
            {
                this.filterButtonOffsetBounds = this.GetFilterBounds(true);
                this.filterButtonImageBounds = this.GetFilterBounds(false);
                Rectangle buttonBounds = this.filterButtonOffsetBounds;
                if (buttonBounds != null && clipBounds.IntersectsWith(buttonBounds))
                {
                    ControlPaint.DrawBorder(graphics, buttonBounds, Color.Gray, ButtonBorderStyle.Solid);
                    buttonBounds.Inflate(-1, -1);
                    using (Brush b = new SolidBrush((this.CellBehavior != ADGVColumnHeaderCellBehavior.Disabled && this.filterButtonOver) ? Color.LightGray : Color.White))
                        graphics.FillRectangle(b, buttonBounds);

                    graphics.DrawImage(this.filterImage, buttonBounds);
                }
            }
        }

        private Rectangle GetFilterBounds(Boolean withOffset = true)
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
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
            {
                if (this.filterButtonImageBounds.Contains(e.X, e.Y) && e.Button == MouseButtons.Left && !this.filterButtonPressed)
                {
                    this.filterButtonPressed = true;
                    this.filterButtonOver = true;
                    this.RepaintCell();
                }
            }
            else if (this.CellBehavior == ADGVColumnHeaderCellBehavior.Default)
                base.OnMouseDown(e);
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering || this.CellBehavior == ADGVColumnHeaderCellBehavior.Sorting)
            {
                if (e.Button == MouseButtons.Left && this.filterButtonPressed)
                {
                    this.filterButtonPressed = false;
                    this.filterButtonOver = false;
                    
                    if (this.filterButtonImageBounds.Contains(e.X, e.Y))
                    {
                        if (this.CellBehavior == ADGVColumnHeaderCellBehavior.SortingFiltering)
                            this.FilterPopup(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
                        else
                        {
                            if (this.ActiveSortType == FilterMenuSortType.ASC)
                                this.FilterMenu.ClearSorting(FilterMenuSortType.DESC, true);
                            else if (this.ActiveSortType == FilterMenuSortType.DESC)
                                this.FilterMenu.ClearSorting(FilterMenuSortType.None, true);
                            else
                                this.FilterMenu.ClearSorting(FilterMenuSortType.ASC, true);

                            this.RefreshImage();
                        }
                    }
                    this.RepaintCell();
                }
            }
            else if (this.CellBehavior == ADGVColumnHeaderCellBehavior.Default)
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

        public void SetLoadedFilterMode(Boolean Enabled)
        {
            this.FilterMenu.SetLoadedFilterMode(Enabled);
            this.RefreshImage();
            this.RepaintCell();
        }
    }

    public delegate void ADGVFilterEventHandler(object sender, ADGVFilterEventArgs e);

    public class ADGVFilterEventArgs : EventArgs
    {
        public ADGVFilterMenu FilterMenu { get; private set; }

        public DataGridViewColumn Column { get; private set; }

        public ADGVFilterEventArgs(ADGVFilterMenu filterMenu, DataGridViewColumn column)
        {
            this.FilterMenu = filterMenu;
            this.Column = column;
        }
    }
}
