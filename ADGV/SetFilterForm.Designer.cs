
namespace ADGV
{
    partial class SetFilterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RM = new System.Resources.ResourceManager("ADGV.Localization.ADGVStrings", typeof(ADGV.SetFilterForm).Assembly);

            this.components = new System.ComponentModel.Container();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.ColumnNameLabel = new System.Windows.Forms.Label();
            this.FilterTypeComboBox = new System.Windows.Forms.ComboBox();
            this.AndLabel = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(29, 139);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = this.RM.GetString("setfilterform_okbutton_text");
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(123, 139);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = this.RM.GetString("setfilterform_cancelbutton_text"); 
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ColumnNameLabel
            // 
            this.ColumnNameLabel.AutoSize = true;
            this.ColumnNameLabel.Location = new System.Drawing.Point(4, 9);
            this.ColumnNameLabel.Name = "ColumnNameLabel";
            this.ColumnNameLabel.Size = new System.Drawing.Size(140, 13);
            this.ColumnNameLabel.TabIndex = 2;
            this.ColumnNameLabel.Text = this.RM.GetString("setfilterform_columnnamelabel_text");
            // 
            // FilterTypeComboBox
            // 
            this.FilterTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FilterTypeComboBox.FormattingEnabled = true;
            this.FilterTypeComboBox.Location = new System.Drawing.Point(7, 32);
            this.FilterTypeComboBox.Name = "FilterTypeComboBox";
            this.FilterTypeComboBox.Size = new System.Drawing.Size(189, 21);
            this.FilterTypeComboBox.TabIndex = 3;
            this.FilterTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.FilterTypeComboBox_SelectedIndexChanged);
            // 
            // AndLabel
            // 
            this.AndLabel.AutoSize = true;
            this.AndLabel.Location = new System.Drawing.Point(7, 89);
            this.AndLabel.Name = "AndLabel";
            this.AndLabel.Size = new System.Drawing.Size(13, 13);
            this.AndLabel.TabIndex = 6;
            this.AndLabel.Text = this.RM.GetString("setfilterform_andlabel_text");
            this.AndLabel.Visible = false;
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // SetFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(205, 169);
            this.Controls.Add(this.AndLabel);
            this.Controls.Add(this.ColumnNameLabel);
            this.Controls.Add(this.FilterTypeComboBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SetFilterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.CancelButton = this.cancelButton;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = this.RM.GetString("setfilterform_text");
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label ColumnNameLabel;
        private System.Windows.Forms.ComboBox FilterTypeComboBox;
        private System.Windows.Forms.Label AndLabel;
        private string filterString = null;
        private string viewfilterString = null;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}