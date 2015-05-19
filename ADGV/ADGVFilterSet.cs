using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADGV
{
    public class ADGVFilterSet : List<ADGVFilterRecord>
    {
        public void Add(ADGVColumnHeaderCell cell)
        {
            if (cell != null && cell.OwningColumn != null)
            {
                this.RemoveAll(r => r.DataPropertyName == cell.OwningColumn.DataPropertyName);
                this.Add(new ADGVFilterRecord(cell.OwningColumn.DataPropertyName, cell.FilterString, cell.ActiveFilterType));
            }
        }

        public void Remove(string columnDataPropertyName)
        {
            this.RemoveAll(r => r.DataPropertyName == columnDataPropertyName);
        }

        public void Remove(ADGVColumnHeaderCell cell)
        {
            if (cell != null && cell.OwningColumn != null)
                this.Remove(cell.OwningColumn.DataPropertyName);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("");

            foreach (ADGVFilterRecord r in this)
            {
                sb.AppendFormat("(" + r.FilterString + ") AND ", r.DataPropertyName);
            }

            if (sb.Length > 4)
                sb.Length -= 4;

            return sb.ToString();
        }
    }

    public class ADGVSortSet : List<ADGVSortRecord>
    {
        public void Add(ADGVColumnHeaderCell cell)
        {
            if (cell != null && cell.OwningColumn != null)
            {
                this.RemoveAll(r => r.DataPropertyName == cell.OwningColumn.DataPropertyName);
                this.Add(new ADGVSortRecord(cell.OwningColumn.DataPropertyName, cell.SortString, cell.ActiveSortType));
            }
        }

        public void Remove(string columnDataPropertyName)
        {
            this.RemoveAll(r => r.DataPropertyName == columnDataPropertyName);
        }

        public void Remove(ADGVColumnHeaderCell cell)
        {
            if (cell != null && cell.OwningColumn != null)
                this.Remove(cell.OwningColumn.DataPropertyName);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("");

            foreach (ADGVSortRecord r in this)
            {
                sb.AppendFormat(r.SortString + ", ", r.DataPropertyName);
            }

            if (sb.Length > 4)
                sb.Length -= 4;

            return sb.ToString();
         }
    }

    public struct ADGVFilterRecord
    {
        public string DataPropertyName;
        public string FilterString;
        public ADGVFilterType FilterType;

        public ADGVFilterRecord(string column, string filter, ADGVFilterType filterType)
        {
            DataPropertyName = column;
            FilterString = filter;
            FilterType = filterType;
        }
    }

    public struct ADGVSortRecord
    {
        public string DataPropertyName;
        public ADGVSortType SortType;
        public string SortString;

        public ADGVSortRecord(string column, string sort, ADGVSortType sortType)
        {
            DataPropertyName = column;
            SortString = sort;
            SortType = sortType;
        }
    }
}
