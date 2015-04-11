namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    partial class TimestampFilterForm
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
            this._dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this._dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this._labelStart = new System.Windows.Forms.Label();
            this._labelEnd = new System.Windows.Forms.Label();
            this._buttonApply = new System.Windows.Forms.Button();
            this._checkBoxDontAskAgain = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // _dateTimePickerStart
            // 
            this._dateTimePickerStart.Location = new System.Drawing.Point(12, 25);
            this._dateTimePickerStart.Name = "_dateTimePickerStart";
            this._dateTimePickerStart.Size = new System.Drawing.Size(200, 20);
            this._dateTimePickerStart.TabIndex = 0;
            // 
            // _dateTimePickerEnd
            // 
            this._dateTimePickerEnd.Location = new System.Drawing.Point(218, 25);
            this._dateTimePickerEnd.Name = "_dateTimePickerEnd";
            this._dateTimePickerEnd.Size = new System.Drawing.Size(200, 20);
            this._dateTimePickerEnd.TabIndex = 1;
            // 
            // _labelStart
            // 
            this._labelStart.AutoSize = true;
            this._labelStart.Location = new System.Drawing.Point(12, 9);
            this._labelStart.Name = "_labelStart";
            this._labelStart.Size = new System.Drawing.Size(32, 13);
            this._labelStart.TabIndex = 2;
            this._labelStart.Text = "Start:";
            // 
            // _labelEnd
            // 
            this._labelEnd.AutoSize = true;
            this._labelEnd.Location = new System.Drawing.Point(215, 9);
            this._labelEnd.Name = "_labelEnd";
            this._labelEnd.Size = new System.Drawing.Size(29, 13);
            this._labelEnd.TabIndex = 3;
            this._labelEnd.Text = "End:";
            // 
            // _buttonApply
            // 
            this._buttonApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonApply.Location = new System.Drawing.Point(343, 51);
            this._buttonApply.Name = "_buttonApply";
            this._buttonApply.Size = new System.Drawing.Size(75, 23);
            this._buttonApply.TabIndex = 4;
            this._buttonApply.Text = "Apply";
            this._buttonApply.UseVisualStyleBackColor = true;
            // 
            // _checkBoxDontAskAgain
            // 
            this._checkBoxDontAskAgain.AutoSize = true;
            this._checkBoxDontAskAgain.Location = new System.Drawing.Point(12, 57);
            this._checkBoxDontAskAgain.Name = "_checkBoxDontAskAgain";
            this._checkBoxDontAskAgain.Size = new System.Drawing.Size(100, 17);
            this._checkBoxDontAskAgain.TabIndex = 5;
            this._checkBoxDontAskAgain.Text = "Don\'t ask again";
            this._checkBoxDontAskAgain.UseVisualStyleBackColor = true;
            // 
            // TimestampFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 86);
            this.Controls.Add(this._checkBoxDontAskAgain);
            this.Controls.Add(this._buttonApply);
            this.Controls.Add(this._labelEnd);
            this.Controls.Add(this._labelStart);
            this.Controls.Add(this._dateTimePickerEnd);
            this.Controls.Add(this._dateTimePickerStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimestampFilterForm";
            this.Text = "Timestamp Filter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _labelStart;
        private System.Windows.Forms.Label _labelEnd;
        private System.Windows.Forms.Button _buttonApply;
        internal System.Windows.Forms.CheckBox _checkBoxDontAskAgain;
        internal System.Windows.Forms.DateTimePicker _dateTimePickerStart;
        internal System.Windows.Forms.DateTimePicker _dateTimePickerEnd;
    }
}