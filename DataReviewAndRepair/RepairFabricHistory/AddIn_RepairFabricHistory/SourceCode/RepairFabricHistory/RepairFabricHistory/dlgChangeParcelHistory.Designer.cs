namespace RepairFabricHistory
{
  partial class dlgChangeParcelHistory
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
      this.button2 = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.chkHistoryDateType = new System.Windows.Forms.CheckBox();
      this.chkToggleHistoric = new System.Windows.Forms.CheckBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.optNullDate = new System.Windows.Forms.RadioButton();
      this.optChooseDate = new System.Windows.Forms.RadioButton();
      this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
      this.cboHistoryDateField = new System.Windows.Forms.ComboBox();
      this.cboHistorical = new System.Windows.Forms.ComboBox();
      this.panelLSD = new System.Windows.Forms.Panel();
      this.chkLegalStartDate = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.dtLSDatePicker = new System.Windows.Forms.DateTimePicker();
      this.chkLegalEndDate = new System.Windows.Forms.CheckBox();
      this.optChooseLSDate = new System.Windows.Forms.RadioButton();
      this.optClearLSDate = new System.Windows.Forms.RadioButton();
      this.panelLED = new System.Windows.Forms.Panel();
      this.optClearLEDate = new System.Windows.Forms.RadioButton();
      this.optChooseLEDate = new System.Windows.Forms.RadioButton();
      this.dtLEDatePicker = new System.Windows.Forms.DateTimePicker();
      this.panelSED = new System.Windows.Forms.Panel();
      this.optClearSEDate = new System.Windows.Forms.RadioButton();
      this.optChooseSEDate = new System.Windows.Forms.RadioButton();
      this.dtSEDatePicker = new System.Windows.Forms.DateTimePicker();
      this.chkSystemEndDate = new System.Windows.Forms.CheckBox();
      this.helpProvider1 = new System.Windows.Forms.HelpProvider();
      this.panel1.SuspendLayout();
      this.panelLSD.SuspendLayout();
      this.panelLED.SuspendLayout();
      this.panelSED.SuspendLayout();
      this.SuspendLayout();
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(124, 329);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(101, 23);
      this.button2.TabIndex = 13;
      this.button2.Text = "Cancel";
      this.button2.UseVisualStyleBackColor = true;
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Location = new System.Drawing.Point(231, 329);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(101, 23);
      this.button1.TabIndex = 12;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // chkHistoryDateType
      // 
      this.chkHistoryDateType.AutoSize = true;
      this.chkHistoryDateType.Checked = true;
      this.chkHistoryDateType.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkHistoryDateType.Location = new System.Drawing.Point(376, 425);
      this.chkHistoryDateType.Name = "chkHistoryDateType";
      this.chkHistoryDateType.Size = new System.Drawing.Size(339, 17);
      this.chkHistoryDateType.TabIndex = 14;
      this.chkHistoryDateType.Text = "Choose the type of historic date to change for the selected parcels";
      this.chkHistoryDateType.UseVisualStyleBackColor = true;
      this.chkHistoryDateType.Visible = false;
      this.chkHistoryDateType.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
      // 
      // chkToggleHistoric
      // 
      this.chkToggleHistoric.AutoSize = true;
      this.chkToggleHistoric.Location = new System.Drawing.Point(376, 554);
      this.chkToggleHistoric.Name = "chkToggleHistoric";
      this.chkToggleHistoric.Size = new System.Drawing.Size(173, 17);
      this.chkToggleHistoric.TabIndex = 15;
      this.chkToggleHistoric.Text = "Change the selected parcels to";
      this.chkToggleHistoric.UseVisualStyleBackColor = true;
      this.chkToggleHistoric.Visible = false;
      this.chkToggleHistoric.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.optNullDate);
      this.panel1.Controls.Add(this.optChooseDate);
      this.panel1.Controls.Add(this.dateTimePicker1);
      this.panel1.Controls.Add(this.cboHistoryDateField);
      this.panel1.Location = new System.Drawing.Point(434, 448);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(389, 100);
      this.panel1.TabIndex = 16;
      this.panel1.Visible = false;
      // 
      // optNullDate
      // 
      this.optNullDate.AutoSize = true;
      this.optNullDate.Location = new System.Drawing.Point(3, 69);
      this.optNullDate.Name = "optNullDate";
      this.optNullDate.Size = new System.Drawing.Size(113, 17);
      this.optNullDate.TabIndex = 19;
      this.optNullDate.Text = "Clear the date field";
      this.optNullDate.UseVisualStyleBackColor = true;
      this.optNullDate.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // optChooseDate
      // 
      this.optChooseDate.AutoSize = true;
      this.optChooseDate.Checked = true;
      this.optChooseDate.Location = new System.Drawing.Point(3, 39);
      this.optChooseDate.Name = "optChooseDate";
      this.optChooseDate.Size = new System.Drawing.Size(117, 17);
      this.optChooseDate.TabIndex = 18;
      this.optChooseDate.TabStop = true;
      this.optChooseDate.Text = "Choose a new date";
      this.optChooseDate.UseVisualStyleBackColor = true;
      this.optChooseDate.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // dateTimePicker1
      // 
      this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dateTimePicker1.Location = new System.Drawing.Point(165, 37);
      this.dateTimePicker1.Name = "dateTimePicker1";
      this.dateTimePicker1.Size = new System.Drawing.Size(100, 20);
      this.dateTimePicker1.TabIndex = 1;
      // 
      // cboHistoryDateField
      // 
      this.cboHistoryDateField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboHistoryDateField.FormattingEnabled = true;
      this.cboHistoryDateField.Items.AddRange(new object[] {
            "Legal End Date",
            "Legal Start Date"});
      this.cboHistoryDateField.Location = new System.Drawing.Point(3, 3);
      this.cboHistoryDateField.MaxDropDownItems = 4;
      this.cboHistoryDateField.Name = "cboHistoryDateField";
      this.cboHistoryDateField.Size = new System.Drawing.Size(262, 21);
      this.cboHistoryDateField.TabIndex = 0;
      this.cboHistoryDateField.SelectedIndexChanged += new System.EventHandler(this.cboHistoryDateField_SelectedIndexChanged);
      // 
      // cboHistorical
      // 
      this.cboHistorical.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboHistorical.Enabled = false;
      this.cboHistorical.FormattingEnabled = true;
      this.cboHistorical.Items.AddRange(new object[] {
            "Historical",
            "Non-Historical"});
      this.cboHistorical.Location = new System.Drawing.Point(566, 554);
      this.cboHistorical.MaxDropDownItems = 3;
      this.cboHistorical.Name = "cboHistorical";
      this.cboHistorical.Size = new System.Drawing.Size(100, 21);
      this.cboHistorical.TabIndex = 17;
      this.cboHistorical.Visible = false;
      this.cboHistorical.SelectedIndexChanged += new System.EventHandler(this.cboHistorical_SelectedIndexChanged);
      // 
      // panelLSD
      // 
      this.panelLSD.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panelLSD.Controls.Add(this.optClearLSDate);
      this.panelLSD.Controls.Add(this.optChooseLSDate);
      this.panelLSD.Controls.Add(this.dtLSDatePicker);
      this.panelLSD.Enabled = false;
      this.panelLSD.Location = new System.Drawing.Point(12, 52);
      this.panelLSD.Name = "panelLSD";
      this.panelLSD.Size = new System.Drawing.Size(311, 68);
      this.panelLSD.TabIndex = 18;
      // 
      // chkLegalStartDate
      // 
      this.chkLegalStartDate.AutoSize = true;
      this.chkLegalStartDate.Location = new System.Drawing.Point(12, 34);
      this.chkLegalStartDate.Name = "chkLegalStartDate";
      this.chkLegalStartDate.Size = new System.Drawing.Size(103, 17);
      this.chkLegalStartDate.TabIndex = 0;
      this.chkLegalStartDate.Text = "Legal Start Date";
      this.chkLegalStartDate.UseVisualStyleBackColor = true;
      this.chkLegalStartDate.CheckedChanged += new System.EventHandler(this.chkLegalStartDate_CheckedChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(254, 13);
      this.label1.TabIndex = 19;
      this.label1.Text = "Choose the dates to change for the selected parcels";
      // 
      // dtLSDatePicker
      // 
      this.dtLSDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dtLSDatePicker.Location = new System.Drawing.Point(183, 4);
      this.dtLSDatePicker.Name = "dtLSDatePicker";
      this.dtLSDatePicker.Size = new System.Drawing.Size(100, 20);
      this.dtLSDatePicker.TabIndex = 1;
      // 
      // chkLegalEndDate
      // 
      this.chkLegalEndDate.AutoSize = true;
      this.chkLegalEndDate.Location = new System.Drawing.Point(12, 132);
      this.chkLegalEndDate.Name = "chkLegalEndDate";
      this.chkLegalEndDate.Size = new System.Drawing.Size(100, 17);
      this.chkLegalEndDate.TabIndex = 3;
      this.chkLegalEndDate.Text = "Legal End Date";
      this.chkLegalEndDate.UseVisualStyleBackColor = true;
      this.chkLegalEndDate.CheckedChanged += new System.EventHandler(this.chkLegalEndDate_CheckedChanged);
      // 
      // optChooseLSDate
      // 
      this.optChooseLSDate.AutoSize = true;
      this.optChooseLSDate.Checked = true;
      this.optChooseLSDate.Location = new System.Drawing.Point(23, 6);
      this.optChooseLSDate.Name = "optChooseLSDate";
      this.optChooseLSDate.Size = new System.Drawing.Size(117, 17);
      this.optChooseLSDate.TabIndex = 4;
      this.optChooseLSDate.TabStop = true;
      this.optChooseLSDate.Text = "Choose a new date";
      this.optChooseLSDate.UseVisualStyleBackColor = true;
      this.optChooseLSDate.CheckedChanged += new System.EventHandler(this.optChooseLSDate_CheckedChanged);
      // 
      // optClearLSDate
      // 
      this.optClearLSDate.AutoSize = true;
      this.optClearLSDate.Location = new System.Drawing.Point(23, 38);
      this.optClearLSDate.Name = "optClearLSDate";
      this.optClearLSDate.Size = new System.Drawing.Size(113, 17);
      this.optClearLSDate.TabIndex = 5;
      this.optClearLSDate.Text = "Clear the date field";
      this.optClearLSDate.UseVisualStyleBackColor = true;
      this.optClearLSDate.CheckedChanged += new System.EventHandler(this.optClearLSDate_CheckedChanged);
      // 
      // panelLED
      // 
      this.panelLED.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panelLED.Controls.Add(this.optClearLEDate);
      this.panelLED.Controls.Add(this.optChooseLEDate);
      this.panelLED.Controls.Add(this.dtLEDatePicker);
      this.panelLED.Enabled = false;
      this.panelLED.Location = new System.Drawing.Point(12, 150);
      this.panelLED.Name = "panelLED";
      this.panelLED.Size = new System.Drawing.Size(311, 68);
      this.panelLED.TabIndex = 20;
      // 
      // optClearLEDate
      // 
      this.optClearLEDate.AutoSize = true;
      this.optClearLEDate.Location = new System.Drawing.Point(23, 38);
      this.optClearLEDate.Name = "optClearLEDate";
      this.optClearLEDate.Size = new System.Drawing.Size(113, 17);
      this.optClearLEDate.TabIndex = 5;
      this.optClearLEDate.Text = "Clear the date field";
      this.optClearLEDate.UseVisualStyleBackColor = true;
      this.optClearLEDate.CheckedChanged += new System.EventHandler(this.optClearLEDate_CheckedChanged);
      // 
      // optChooseLEDate
      // 
      this.optChooseLEDate.AutoSize = true;
      this.optChooseLEDate.Checked = true;
      this.optChooseLEDate.Location = new System.Drawing.Point(23, 6);
      this.optChooseLEDate.Name = "optChooseLEDate";
      this.optChooseLEDate.Size = new System.Drawing.Size(117, 17);
      this.optChooseLEDate.TabIndex = 4;
      this.optChooseLEDate.TabStop = true;
      this.optChooseLEDate.Text = "Choose a new date";
      this.optChooseLEDate.UseVisualStyleBackColor = true;
      this.optChooseLEDate.CheckedChanged += new System.EventHandler(this.optChooseLEDate_CheckedChanged);
      // 
      // dtLEDatePicker
      // 
      this.dtLEDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dtLEDatePicker.Location = new System.Drawing.Point(183, 4);
      this.dtLEDatePicker.Name = "dtLEDatePicker";
      this.dtLEDatePicker.Size = new System.Drawing.Size(100, 20);
      this.dtLEDatePicker.TabIndex = 1;
      // 
      // panelSED
      // 
      this.panelSED.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panelSED.Controls.Add(this.optClearSEDate);
      this.panelSED.Controls.Add(this.optChooseSEDate);
      this.panelSED.Controls.Add(this.dtSEDatePicker);
      this.helpProvider1.SetHelpString(this.panelSED, "Changing the Sytem End Date will automatically set the Historical field value to " +
        "True. Clearing this date will set the Historical field value to False.");
      this.panelSED.Location = new System.Drawing.Point(13, 249);
      this.panelSED.Name = "panelSED";
      this.helpProvider1.SetShowHelp(this.panelSED, true);
      this.panelSED.Size = new System.Drawing.Size(310, 68);
      this.panelSED.TabIndex = 22;
      // 
      // optClearSEDate
      // 
      this.optClearSEDate.AutoSize = true;
      this.optClearSEDate.Location = new System.Drawing.Point(23, 38);
      this.optClearSEDate.Name = "optClearSEDate";
      this.optClearSEDate.Size = new System.Drawing.Size(113, 17);
      this.optClearSEDate.TabIndex = 5;
      this.optClearSEDate.Text = "Clear the date field";
      this.optClearSEDate.UseVisualStyleBackColor = true;
      this.optClearSEDate.CheckedChanged += new System.EventHandler(this.optClearSEDate_CheckedChanged);
      // 
      // optChooseSEDate
      // 
      this.optChooseSEDate.AutoSize = true;
      this.optChooseSEDate.Checked = true;
      this.optChooseSEDate.Location = new System.Drawing.Point(23, 6);
      this.optChooseSEDate.Name = "optChooseSEDate";
      this.optChooseSEDate.Size = new System.Drawing.Size(117, 17);
      this.optChooseSEDate.TabIndex = 4;
      this.optChooseSEDate.TabStop = true;
      this.optChooseSEDate.Text = "Choose a new date";
      this.optChooseSEDate.UseVisualStyleBackColor = true;
      this.optChooseSEDate.CheckedChanged += new System.EventHandler(this.optChooseSEDate_CheckedChanged);
      // 
      // dtSEDatePicker
      // 
      this.dtSEDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dtSEDatePicker.Location = new System.Drawing.Point(183, 4);
      this.dtSEDatePicker.Name = "dtSEDatePicker";
      this.dtSEDatePicker.Size = new System.Drawing.Size(100, 20);
      this.dtSEDatePicker.TabIndex = 1;
      // 
      // chkSystemEndDate
      // 
      this.chkSystemEndDate.AutoSize = true;
      this.chkSystemEndDate.Checked = true;
      this.chkSystemEndDate.CheckState = System.Windows.Forms.CheckState.Checked;
      this.helpProvider1.SetHelpString(this.chkSystemEndDate, "Changing the Sytem End Date will automatically set the Historical field value to " +
        "True. Clearing this date will set the Historical field value to False.");
      this.chkSystemEndDate.Location = new System.Drawing.Point(13, 231);
      this.chkSystemEndDate.Name = "chkSystemEndDate";
      this.helpProvider1.SetShowHelp(this.chkSystemEndDate, true);
      this.chkSystemEndDate.Size = new System.Drawing.Size(108, 17);
      this.chkSystemEndDate.TabIndex = 21;
      this.chkSystemEndDate.Text = "System End Date";
      this.chkSystemEndDate.UseVisualStyleBackColor = true;
      this.chkSystemEndDate.CheckedChanged += new System.EventHandler(this.chkSystemEndDate_CheckedChanged);
      // 
      // dlgChangeParcelHistory
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(371, 382);
      this.Controls.Add(this.panelSED);
      this.Controls.Add(this.chkSystemEndDate);
      this.Controls.Add(this.panelLED);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.panelLSD);
      this.Controls.Add(this.chkLegalEndDate);
      this.Controls.Add(this.cboHistorical);
      this.Controls.Add(this.chkLegalStartDate);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.chkToggleHistoric);
      this.Controls.Add(this.chkHistoryDateType);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.HelpButton = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "dlgChangeParcelHistory";
      this.ShowInTaskbar = false;
      this.Text = "Change Parcel History";
      this.Load += new System.EventHandler(this.dlgChangeParcelHistory_Load);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.panelLSD.ResumeLayout(false);
      this.panelLSD.PerformLayout();
      this.panelLED.ResumeLayout(false);
      this.panelLED.PerformLayout();
      this.panelSED.ResumeLayout(false);
      this.panelSED.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panelLSD;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel panelLED;
    private System.Windows.Forms.Panel panelSED;
    private System.Windows.Forms.RadioButton optNullDate;
    private System.Windows.Forms.RadioButton optChooseDate;
    private System.Windows.Forms.ComboBox cboHistoryDateField;
    private System.Windows.Forms.CheckBox chkHistoryDateType;
    private System.Windows.Forms.CheckBox chkToggleHistoric;
    private System.Windows.Forms.ComboBox cboHistorical;
    private System.Windows.Forms.DateTimePicker dateTimePicker1;
    internal System.Windows.Forms.RadioButton optClearLSDate;
    internal System.Windows.Forms.RadioButton optChooseLSDate;
    internal System.Windows.Forms.DateTimePicker dtLSDatePicker;
    internal System.Windows.Forms.CheckBox chkLegalStartDate;
    internal System.Windows.Forms.CheckBox chkLegalEndDate;
    internal System.Windows.Forms.RadioButton optClearLEDate;
    internal System.Windows.Forms.RadioButton optChooseLEDate;
    internal System.Windows.Forms.DateTimePicker dtLEDatePicker;
    internal System.Windows.Forms.RadioButton optClearSEDate;
    internal System.Windows.Forms.RadioButton optChooseSEDate;
    internal System.Windows.Forms.DateTimePicker dtSEDatePicker;
    internal System.Windows.Forms.CheckBox chkSystemEndDate;
    private System.Windows.Forms.HelpProvider helpProvider1;
  }
}