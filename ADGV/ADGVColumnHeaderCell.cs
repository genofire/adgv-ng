using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADGV
{
    public enum ADGVColumnHeaderCellBehavior : byte
    {
        DisabledHidden = 0,
        Disabled,
        Standart,
        Sorting,
        SortingFiltering
    }
    
    public class ADGVColumnHeaderCell : DataGridViewColumnHeaderCell
    {
        private Image filterImage = Properties.Resources.AddFilter;
        private Size filterButtonImageSize = new Size(16, 16);
        private Boolean filterButtonPressed = false;
        private Boolean filterButtonOver = false;
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

        public ADGVFilterMenuSortType ActiveSortType
        {
            get
            {
                if (this.FilterEnabled)
                    return this.FilterMenu.ActiveSortType;
                else
                    return ADGVFilterMenuSortType.None;
            }
        }

        public ADGVFilterMenuFilterType ActiveFilterType
        {
            get
            {
                if (this.FilterEnabled)
                    return this.FilterMenu.ActiveFilterType;
                else
                    return ADGVFilterMenuFilterType.None;
            }
        }

        public String SortString
        {
            get
            {
                if (this.FilterEnabled)
                    return this.FilterMenu.SortString;
                else
                    return "";
            }
        }

        public String FilterString
        {
            get
            {
                if (this.FilterEnabled)
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
                    bool refreshed = false;

                    if (this.FilterMenu.FilterString.Length > 0)
                    {
                        FilterMenu_FilterChanged(this, new EventArgs());
                        refreshed = true;
                    }
                    if (this.FilterMenu.SortString.Length > 0)
                    {
                        FilterMenu_SortChanged(this, new EventArgs());
                        refreshed = true;
                    }

                    this.cellBehavior = value;
                    
                    if (!refreshed)
                        this.RepaintCell();
                }

                if (!value)
                {
                    this.filterButtonPressed = false;
                    this.filterButtonOver = false;
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
            this.CellBehavior = cellBehavior;

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

        internal void ClearSorting(bool fireEvent = false)
        {
            if (this.FilterEnabled)
            {
                this.FilterMenu.ClearSorting(fireEvent);
                this.RefreshImage();
                this.RepaintCell();
            }
        }

        internal void ClearFilter(bool fireEvent = false)
        {
            if (this.FilterEnabled)
            {
                this.FilterMenu.ClearFilter(fireEvent);
                this.RefreshImage();
                this.RepaintCell();
            }
        }

        private void RefreshImage()
        {
            if (this.ActiveFilterType == ADGVFilterMenuFilterType.Loaded)
            {
                this.filterImage = Properties.Resources.SavedFilter;
            }
            else
                if (this.ActiveFilterType == ADGVFilterMenuFilterType.None)
                {
                    if (this.ActiveSortType == ADGVFilterMenuSortType.None)
                        this.filterImage = Properties.Resources.AddFilter;
                    else if (this.ActiveSortType == ADGVFilterMenuSortType.ASC)
                        this.filterImage = Properties.Resources.ASC;
                    else
                        this.filterImage = Properties.Resources.DESC;
                }
                else
                {
                    if (this.ActiveSortType == ADGVFilterMenuSortType.None)
                        this.filterImage = Properties.Resources.Filter;
                    else if (this.ActiveSortType == ADGVFilterMenuSortType.ASC)
                        this.filterImage = Properties.Resources.FilterASC;
                    else
                        this.filterImage = Properties.Resources.FilterDESC;
                }
        }

        private void FilterMenu_FilterChanged(object sender, EventArgs e)
        {
            this.RefreshImage();
            this.RepaintCell();
            if (this.FilterEnabled)
                this.FilterChanged(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
        }

        private void FilterMenu_SortChanged(object sender, EventArgs e)
        {
            this.RefreshImage();
            this.RepaintCell();
            if (this.FilterEnabled)
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
            if (this.CellBehavior != ADGVColumnHeaderCellBehavior.Standart && this.CellBehavior != ADGVColumnHeaderCellBehavior.DisabledHidden && this.SortGlyphDirection != SortOrder.None)
                this.SortGlyphDirection = SortOrder.None;

            base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                cellState, value, formattedValue,
                errorText, cellStyle, advancedBorderStyle, paintParts);

            if (this.CellBehavior != ADGVColumnHeaderCellBehavior.Standart && this.CellBehavior != ADGVColumnHeaderCellBehavior.DisabledHidden && paintParts.HasFlag(DataGridViewPaintParts.ContentBackground))
            {
                this.filterButtonOffsetBounds = this.GetFilterBounds(true);
                this.filterButtonImageBounds = this.GetFilterBounds(false);
                Rectangle buttonBounds = this.filterButtonOffsetBounds;
                if (buttonBounds != null && clipBounds.IntersectsWith(buttonBounds))
                {
                    ControlPaint.DrawBorder(graphics, buttonBounds, Color.Gray, ButtonBorderStyle.Solid);
                    buttonBounds.Inflate(-1, -1);
                    using (Brush b = new SolidBrush(this.filterButtonOver ? Color.LightGray : Color.White))
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
            if (this.FilterEnabled)
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
            if (this.FilterEnabled)
            {
                if (this.filterButtonImageBounds.Contains(e.X, e.Y) && e.Button == MouseButtons.Left && !this.filterButtonPressed)
                {
                    this.filterButtonPressed = true;
                    this.filterButtonOver = true;
                    this.RepaintCell();
                }
            }
            else
                base.OnMouseDown(e);
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if (this.FilterEnabled)
            {
                if (e.Button == MouseButtons.Left && this.filterButtonPressed)
                {
                    this.filterButtonPressed = false;
                    this.filterButtonOver = false;
                    this.RepaintCell();
                    if (this.filterButtonImageBounds.Contains(e.X, e.Y))
                    {
                        this.FilterPopup(this, new ADGVFilterEventArgs(this.FilterMenu, this.OwningColumn));
                    }
                }
            }
            else
                base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            if (this.FilterEnabled && this.filterButtonOver)
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