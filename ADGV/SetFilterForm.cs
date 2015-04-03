using System;
using System.Linq;
using System.Resources;
using System.Windows.Forms;

namespace ADGV
{
    public partial class SetFilterForm : Form
    {
        private enum FilterType
        {
            Unknown,
            DateTime,
            String,
            Float,
            Integer
        }

        private FilterType filterType = FilterType.Unknown;
        private ResourceManager RM = null;
        private Control val1contol = null;
        private Control val2contol = null;
        private Boolean dateWithTime = true;
        private Boolean timeFilter = false;

        public String FilterString
        {
            get
            {
                return filterString;
            }
        }

        public String ViewFilterString
        {
            get
            {
                return viewfilterString;
            }
        }

        private SetFilterForm()
        {
            InitializeComponent();
        }

        private String FormatString(String Text)
        {
            String result = "";
            String s;
            String[] replace = { "%", "[", "]", "*", "\"", "`", "\\" };

            for (Int32 i = 0; i < Text.Length; i++)
            {
                s = Text[i].ToString();
                if (replace.Contains(s))
                    result += "[" + s + "]";
                else
                    result += s;
            }

            return result.Replace("'", "''");
        }

        public SetFilterForm(Type dataType, Boolean DateWithTime = true, Boolean TimeFilter = false)
            : this()
        {
            if (dataType == typeof(DateTime))
                this.filterType = FilterType.DateTime;
            else if (dataType == typeof(Int32) || dataType == typeof(Int64) || dataType == typeof(Int16) ||
                    dataType == typeof(UInt32) || dataType == typeof(UInt64) || dataType == typeof(UInt16) ||
                    dataType == typeof(Byte) || dataType == typeof(SByte))
                this.filterType = FilterType.Integer;
            else if (dataType == typeof(Single) || dataType == typeof(Double) || dataType == typeof(Decimal))
                this.filterType = FilterType.Float;
            else if (dataType == typeof(String))
                this.filterType = FilterType.String;
            else
                this.filterType = FilterType.Unknown;

            this.dateWithTime = DateWithTime;
            this.timeFilter = TimeFilter;
            switch (this.filterType)
            {
                case FilterType.DateTime:
                    this.val1contol = new DateTimePicker();
                    this.val2contol = new DateTimePicker();
                    if (this.timeFilter)
                    {
                        System.Globalization.DateTimeFormatInfo dt = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat;

                        (this.val1contol as DateTimePicker).CustomFormat = dt.ShortDatePattern + " " + dt.LongTimePattern;
                        (this.val2contol as DateTimePicker).CustomFormat = dt.ShortDatePattern + " " + dt.LongTimePattern;
                        (this.val1contol as DateTimePicker).Format = DateTimePickerFormat.Custom;
                        (this.val2contol as DateTimePicker).Format = DateTimePickerFormat.Custom;
                    }
                    else
                    {
                        (this.val1contol as DateTimePicker).Format = DateTimePickerFormat.Short;
                        (this.val2contol as DateTimePicker).Format = DateTimePickerFormat.Short;
                    }

                    this.FilterTypeComboBox.Items.AddRange(new String[] {
                        this.RM.GetString("setfilterform_filtertypecombobox_equal"),
                        this.RM.GetString("setfilterform_filtertypecombobox_notequal"),
                        this.RM.GetString("setfilterform_filtertypecombobox_before"),
                        this.RM.GetString("setfilterform_filtertypecombobox_after"),
                        this.RM.GetString("setfilterform_filtertypecombobox_between")
                    });
                    break;

                case FilterType.Integer:
                case FilterType.Float:
                    this.val1contol = new TextBox();
                    this.val2contol = new TextBox();
                    this.val1contol.TextChanged += eControlTextChanged;
                    this.val2contol.TextChanged += eControlTextChanged;
                    this.FilterTypeComboBox.Items.AddRange(new String[] {
                        this.RM.GetString("setfilterform_filtertypecombobox_equal"),
                        this.RM.GetString("setfilterform_filtertypecombobox_notequal"),
                        this.RM.GetString("setfilterform_filtertypecombobox_larger"),
                        this.RM.GetString("setfilterform_filtertypecombobox_lagerequals"),
                        this.RM.GetString("setfilterform_filtertypecombobox_less"),
                        this.RM.GetString("setfilterform_filtertypecombobox_lessequals"),
                        this.RM.GetString("setfilterform_filtertypecombobox_between")
                    });
                    this.val1contol.Tag = true;
                    this.val2contol.Tag = true;
                    this.okButton.Enabled = false;
                    break;

                default:
                    this.val1contol = new TextBox();
                    this.val2contol = new TextBox();
                    this.FilterTypeComboBox.Items.AddRange(new String[] {
                        this.RM.GetString("setfilterform_filtertypecombobox_equal"),
                        this.RM.GetString("setfilterform_filtertypecombobox_notequal"),
                        this.RM.GetString("setfilterform_filtertypecombobox_begins"),
                        this.RM.GetString("setfilterform_filtertypecombobox_nobegins"),
                        this.RM.GetString("setfilterform_filtertypecombobox_ends"),
                        this.RM.GetString("setfilterform_filtertypecombobox_noends"),
                        this.RM.GetString("setfilterform_filtertypecombobox_contain"),
                        this.RM.GetString("setfilterform_filtertypecombobox_nocontain")
                    });
                    break;
            }
            this.FilterTypeComboBox.SelectedIndex = 0;

            this.val1contol.Name = "val1contol";
            this.val1contol.Location = new System.Drawing.Point(30, 66);
            this.val1contol.Size = new System.Drawing.Size(166, 20);
            this.val1contol.TabIndex = 4;
            this.val1contol.Visible = true;
            this.val1contol.KeyDown += eControlKeyDown;

            this.val2contol.Name = "val2contol";
            this.val2contol.Location = new System.Drawing.Point(30, 108);
            this.val2contol.Size = new System.Drawing.Size(166, 20);
            this.val2contol.TabIndex = 5;
            this.val2contol.Visible = false;
            this.val2contol.VisibleChanged += new EventHandler(val2contol_VisibleChanged);
            this.val2contol.KeyDown += eControlKeyDown;

            this.Controls.Add(this.val1contol);
            this.Controls.Add(this.val2contol);

            this.errorProvider.SetIconAlignment(this.val1contol, ErrorIconAlignment.MiddleRight);
            this.errorProvider.SetIconPadding(this.val1contol, -18);
            this.errorProvider.SetIconAlignment(this.val2contol, ErrorIconAlignment.MiddleRight);
            this.errorProvider.SetIconPadding(this.val2contol, -18);
            this.val1contol.Select();
        }

        private void val2contol_VisibleChanged(object sender, EventArgs e)
        {
            this.AndLabel.Visible = this.val2contol.Visible;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.viewfilterString = null;
            this.filterString = null;
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (HasErrors())
            {
                this.okButton.Enabled = false;
                return;
            }

            String column = "[{0}] ";

            if (this.filterType == FilterType.Unknown)
                column = "Convert([{0}],System.String) ";

            this.filterString = column;

            switch (this.filterType)
            {
                case FilterType.DateTime:
                    DateTime dt = ((DateTimePicker)this.val1contol).Value;

                    if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_equal"))
                    {
                        if (this.dateWithTime)
                        {
                            if (this.timeFilter)
                                this.filterString += "= '" + dt.ToString("o") + "'";
                            else
                                this.filterString = "Convert([{0}], System.String) LIKE '" + dt.ToShortDateString() + "%'";
                        }
                        else
                            this.filterString += "= '" + dt.ToShortDateString() + "'";
                    }
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_before"))
                    {
                        if (this.timeFilter)
                            this.filterString += "< '" + dt.ToString("o") + "'";
                        else
                            this.filterString += "< '" + dt.ToShortDateString() + "'";
                    }
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_after"))
                    {
                        if (this.timeFilter)
                            this.filterString += "> '" + dt.ToString("o") + "'";
                        else
                            this.filterString += "> '" + dt.ToShortDateString() + "'";
                    }
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_between"))
                    {
                        DateTime dt1 = ((DateTimePicker)this.val2contol).Value;
                        if (dt1 < dt)
                        {
                            DateTime d = dt;
                            dt = dt1;
                            dt1 = d;
                        }
                        if (this.timeFilter)
                        {
                            this.filterString += ">= '" + dt.ToString("o") + "'";
                            this.filterString += " AND " + column + "<= '" + dt1.ToString("o") + "'";
                        }
                        else
                        {
                            this.filterString += ">= '" + dt.ToShortDateString() + "'";
                            this.filterString += " AND " + column + "<= '" + dt1.ToShortDateString() + "'";
                        }
                    }
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_notequal"))
                    {
                        if (this.dateWithTime)
                        {
                            if (this.timeFilter)
                                this.filterString += "<> '" + dt.ToString("o") + "'";
                            else
                                this.filterString = "Convert([{0}], System.String) NOT LIKE '" + dt.ToShortDateString() + "%'";
                        }
                        else
                            this.filterString += "<> '" + dt.ToShortDateString() + "'";
                    }
                    break;

                case FilterType.Integer:
                case FilterType.Float:

                    String num = this.val1contol.Text;

                    if (this.filterType == FilterType.Float)
                        num = num.Replace(",", ".");

                    if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_equal"))
                        this.filterString += "= " + num;
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_between"))
                    {
                        String num1 = this.val2contol.Text;
                        if (this.filterType == FilterType.Float)
                            num1 = num1.Replace(",", ".");

                        if (Double.Parse(num) > Double.Parse(num1))
                        {
                            String nn = num;
                            num = num1;
                            num1 = nn;
                        }
                        this.filterString += ">= " + num + " AND " + column + "<= " + num1;
                    }
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_notequal"))
                        this.filterString += "<> " + num;
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_larger"))
                        this.filterString += "> " + num;
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_lagerequals"))
                        this.filterString += ">= " + num;
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_less"))
                        this.filterString += "< " + num;
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_lessequals"))
                        this.filterString += "<= " + num;
                    break;

                default:
                    String txt = this.FormatString(this.val1contol.Text);
                    if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_equal"))
                        this.filterString += "LIKE '" + txt + "'";
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_notequal"))
                        this.filterString += "NOT LIKE '" + txt + "'";
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_begins"))
                        this.filterString += "LIKE '" + txt + "%'";
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_ends"))
                        this.filterString += "LIKE '%" + txt + "'";
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_nobegins"))
                        this.filterString += "NOT LIKE '" + txt + "%'";
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_noends"))
                        this.filterString += "NOT LIKE '%" + txt + "'";
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_contain"))
                        this.filterString += "LIKE '%" + txt + "%'";
                    else if (this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_nocontain"))
                        this.filterString += "NOT LIKE '%" + txt + "%'";
                    break;
            }

            if (this.filterString != column)
            {
                this.viewfilterString = this.RM.GetString("setfilterform_viewfilterstring_mustbe") + " " + this.FilterTypeComboBox.Text + " \"" + this.val1contol.Text + "\"";
                if (this.val2contol.Visible)
                    this.viewfilterString += " " + this.AndLabel.Text + " \"" + this.val2contol.Text + "\"";
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                this.filterString = null;
                this.viewfilterString = null;
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }

            this.Close();
        }

        private void FilterTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.val2contol.Visible = this.FilterTypeComboBox.Text == this.RM.GetString("setfilterform_filtertypecombobox_between");
            if (String.IsNullOrEmpty(val1contol.Text) || !val2contol.Visible)
                this.val1contol.Select();
            else if (val2contol.Visible)
                this.val2contol.Select();
            this.okButton.Enabled = !HasErrors();
        }

        private void eControlTextChanged(object sender, EventArgs e)
        {
            Boolean hasErrors = false;
            switch (this.filterType)
            {
                case FilterType.Integer:
                    Int64 val;
                    hasErrors = !(Int64.TryParse((sender as TextBox).Text, out val));
                    break;

                case FilterType.Float:
                    Double val1;
                    hasErrors = !(Double.TryParse((sender as TextBox).Text, out val1));
                    break;
            }

            (sender as Control).Tag = hasErrors || (sender as TextBox).Text.Length == 0;

            if (hasErrors && (sender as TextBox).Text.Length > 0)
                this.errorProvider.SetError((sender as Control), this.RM.GetString("setfilterform_errorprovider_value"));
            else
                this.errorProvider.SetError((sender as Control), "");

            this.okButton.Enabled = !HasErrors();
        }

        private Boolean HasErrors()
        {
            return (this.val1contol.Visible && this.val1contol.Tag != null && ((Boolean)this.val1contol.Tag)) ||
                (this.val2contol.Visible && this.val2contol.Tag != null && ((Boolean)this.val2contol.Tag));
        }

        private void eControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (sender == this.val1contol)
                {
                    if (this.val2contol.Visible)
                        this.val2contol.Focus();
                    else
                        this.OKButton_Click(this.okButton, new EventArgs());
                }
                else
                {
                    this.OKButton_Click(this.okButton, new EventArgs());
                }

                e.SuppressKeyPress = false;
                e.Handled = true;
            }
        }
    }
}