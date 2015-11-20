namespace FabricPointMoveToFeature
{
  partial class ConfigurationDLG
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
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.optPoints = new System.Windows.Forms.RadioButton();
      this.textBoxDescribeForPoints = new System.Windows.Forms.TextBox();
      this.optLines = new System.Windows.Forms.RadioButton();
      this.textBoxDescribeForLines = new System.Windows.Forms.TextBox();
      this.chkAutoMove = new System.Windows.Forms.CheckBox();
      this.chkReport = new System.Windows.Forms.CheckBox();
      this.lblUnits = new System.Windows.Forms.Label();
      this.txtReportTolerance = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tbConfiguration = new System.Windows.Forms.TabControl();
      this.tbMethod = new System.Windows.Forms.TabPage();
      this.cboFldChoice = new System.Windows.Forms.ComboBox();
      this.tbSelection = new System.Windows.Forms.TabPage();
      this.tbReporting = new System.Windows.Forms.TabPage();
      this.tbConfiguration.SuspendLayout();
      this.tbMethod.SuspendLayout();
      this.tbReporting.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(252, 352);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(83, 23);
      this.btnCancel.TabIndex = 0;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(341, 352);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(83, 23);
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // optPoints
      // 
      this.optPoints.AutoSize = true;
      this.optPoints.Checked = true;
      this.optPoints.Location = new System.Drawing.Point(25, 22);
      this.optPoints.Name = "optPoints";
      this.optPoints.Size = new System.Drawing.Size(154, 17);
      this.optPoints.TabIndex = 2;
      this.optPoints.TabStop = true;
      this.optPoints.Text = "Use point to point matching";
      this.optPoints.UseVisualStyleBackColor = true;
      this.optPoints.CheckedChanged += new System.EventHandler(this.optPoints_CheckedChanged);
      // 
      // textBoxDescribeForPoints
      // 
      this.textBoxDescribeForPoints.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.textBoxDescribeForPoints.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxDescribeForPoints.Location = new System.Drawing.Point(51, 45);
      this.textBoxDescribeForPoints.Multiline = true;
      this.textBoxDescribeForPoints.Name = "textBoxDescribeForPoints";
      this.textBoxDescribeForPoints.Size = new System.Drawing.Size(294, 50);
      this.textBoxDescribeForPoints.TabIndex = 3;
      this.textBoxDescribeForPoints.Text = "Fabric points are moved to the locations of the reference point features that hav" +
    "e the same id. Use point layer with a LONG field named:";
      // 
      // optLines
      // 
      this.optLines.AutoSize = true;
      this.optLines.Location = new System.Drawing.Point(25, 130);
      this.optLines.Name = "optLines";
      this.optLines.Size = new System.Drawing.Size(193, 17);
      this.optLines.TabIndex = 4;
      this.optLines.Text = "Use start and end locations of lines ";
      this.optLines.UseVisualStyleBackColor = true;
      this.optLines.CheckedChanged += new System.EventHandler(this.optLines_CheckedChanged);
      // 
      // textBoxDescribeForLines
      // 
      this.textBoxDescribeForLines.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.textBoxDescribeForLines.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxDescribeForLines.Location = new System.Drawing.Point(51, 153);
      this.textBoxDescribeForLines.Multiline = true;
      this.textBoxDescribeForLines.Name = "textBoxDescribeForLines";
      this.textBoxDescribeForLines.Size = new System.Drawing.Size(294, 51);
      this.textBoxDescribeForLines.TabIndex = 5;
      this.textBoxDescribeForLines.Text = "Fabric points that exactly match the start locations of a reference line are move" +
    "d to the end of that line.";
      // 
      // chkAutoMove
      // 
      this.chkAutoMove.AutoSize = true;
      this.chkAutoMove.Enabled = false;
      this.chkAutoMove.Location = new System.Drawing.Point(25, 207);
      this.chkAutoMove.Name = "chkAutoMove";
      this.chkAutoMove.Size = new System.Drawing.Size(15, 14);
      this.chkAutoMove.TabIndex = 6;
      this.chkAutoMove.UseVisualStyleBackColor = true;
      this.chkAutoMove.Visible = false;
      // 
      // chkReport
      // 
      this.chkReport.AutoSize = true;
      this.chkReport.Location = new System.Drawing.Point(20, 23);
      this.chkReport.Name = "chkReport";
      this.chkReport.Size = new System.Drawing.Size(249, 17);
      this.chkReport.TabIndex = 7;
      this.chkReport.Text = "Report fabric point changes that are more than ";
      this.chkReport.UseVisualStyleBackColor = true;
      this.chkReport.CheckedChanged += new System.EventHandler(this.chkReport_CheckedChanged);
      // 
      // lblUnits
      // 
      this.lblUnits.AutoSize = true;
      this.lblUnits.Location = new System.Drawing.Point(179, 49);
      this.lblUnits.Name = "lblUnits";
      this.lblUnits.Size = new System.Drawing.Size(90, 13);
      this.lblUnits.TabIndex = 8;
      this.lblUnits.Text = "<Unknown units>";
      // 
      // txtReportTolerance
      // 
      this.txtReportTolerance.Enabled = false;
      this.txtReportTolerance.Location = new System.Drawing.Point(39, 46);
      this.txtReportTolerance.Name = "txtReportTolerance";
      this.txtReportTolerance.Size = new System.Drawing.Size(125, 20);
      this.txtReportTolerance.TabIndex = 9;
      this.txtReportTolerance.Text = "10.00";
      this.txtReportTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtReportTolerance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtReportTolerance_KeyDown);
      this.txtReportTolerance.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtReportTolerance_KeyPress);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(46, 207);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(307, 13);
      this.label1.TabIndex = 10;
      this.label1.Text = "Automatically move fabric points after a reference line is created";
      this.label1.Visible = false;
      // 
      // tbConfiguration
      // 
      this.tbConfiguration.Controls.Add(this.tbMethod);
      this.tbConfiguration.Controls.Add(this.tbSelection);
      this.tbConfiguration.Controls.Add(this.tbReporting);
      this.tbConfiguration.Location = new System.Drawing.Point(12, 12);
      this.tbConfiguration.Name = "tbConfiguration";
      this.tbConfiguration.SelectedIndex = 0;
      this.tbConfiguration.Size = new System.Drawing.Size(412, 314);
      this.tbConfiguration.TabIndex = 11;
      // 
      // tbMethod
      // 
      this.tbMethod.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.tbMethod.Controls.Add(this.cboFldChoice);
      this.tbMethod.Controls.Add(this.textBoxDescribeForPoints);
      this.tbMethod.Controls.Add(this.chkAutoMove);
      this.tbMethod.Controls.Add(this.label1);
      this.tbMethod.Controls.Add(this.optPoints);
      this.tbMethod.Controls.Add(this.optLines);
      this.tbMethod.Controls.Add(this.textBoxDescribeForLines);
      this.tbMethod.Location = new System.Drawing.Point(4, 22);
      this.tbMethod.Name = "tbMethod";
      this.tbMethod.Padding = new System.Windows.Forms.Padding(3);
      this.tbMethod.Size = new System.Drawing.Size(404, 288);
      this.tbMethod.TabIndex = 0;
      this.tbMethod.Text = "Method";
      // 
      // cboFldChoice
      // 
      this.cboFldChoice.FormattingEnabled = true;
      this.cboFldChoice.Items.AddRange(new object[] {
            "FABRICPOINTID",
            "blah",
            "blah"});
      this.cboFldChoice.Location = new System.Drawing.Point(109, 90);
      this.cboFldChoice.Name = "cboFldChoice";
      this.cboFldChoice.Size = new System.Drawing.Size(186, 21);
      this.cboFldChoice.TabIndex = 12;
      this.cboFldChoice.Text = "FABRICPOINTID";
      this.cboFldChoice.SelectedIndexChanged += new System.EventHandler(this.cboFldChoice_SelectedIndexChanged);
      // 
      // tbSelection
      // 
      this.tbSelection.Location = new System.Drawing.Point(4, 22);
      this.tbSelection.Name = "tbSelection";
      this.tbSelection.Padding = new System.Windows.Forms.Padding(3);
      this.tbSelection.Size = new System.Drawing.Size(404, 288);
      this.tbSelection.TabIndex = 1;
      this.tbSelection.Text = "Selected Feature Options";
      this.tbSelection.UseVisualStyleBackColor = true;
      // 
      // tbReporting
      // 
      this.tbReporting.Controls.Add(this.chkReport);
      this.tbReporting.Controls.Add(this.lblUnits);
      this.tbReporting.Controls.Add(this.txtReportTolerance);
      this.tbReporting.Location = new System.Drawing.Point(4, 22);
      this.tbReporting.Name = "tbReporting";
      this.tbReporting.Size = new System.Drawing.Size(404, 288);
      this.tbReporting.TabIndex = 2;
      this.tbReporting.Text = "Reporting";
      this.tbReporting.UseVisualStyleBackColor = true;
      // 
      // ConfigurationDLG
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(429, 383);
      this.Controls.Add(this.tbConfiguration);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ConfigurationDLG";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Configuration";
      this.Load += new System.EventHandler(this.ConfigurationDLG_Load);
      this.tbConfiguration.ResumeLayout(false);
      this.tbMethod.ResumeLayout(false);
      this.tbMethod.PerformLayout();
      this.tbReporting.ResumeLayout(false);
      this.tbReporting.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    internal System.Windows.Forms.RadioButton optPoints;
    private System.Windows.Forms.TextBox textBoxDescribeForPoints;
    private System.Windows.Forms.TextBox textBoxDescribeForLines;
    internal System.Windows.Forms.RadioButton optLines;
    internal System.Windows.Forms.CheckBox chkAutoMove;
    internal System.Windows.Forms.TextBox txtReportTolerance;
    internal System.Windows.Forms.Label lblUnits;
    internal System.Windows.Forms.CheckBox chkReport;
    internal System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabPage tbMethod;
    private System.Windows.Forms.TabPage tbSelection;
    private System.Windows.Forms.TabPage tbReporting;
    internal System.Windows.Forms.TabControl tbConfiguration;
    internal System.Windows.Forms.ComboBox cboFldChoice;
  }
}