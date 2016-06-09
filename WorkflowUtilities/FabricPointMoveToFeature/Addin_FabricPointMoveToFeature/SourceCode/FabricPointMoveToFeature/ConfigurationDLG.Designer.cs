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
      this.chkMinimumMove = new System.Windows.Forms.CheckBox();
      this.chkReport = new System.Windows.Forms.CheckBox();
      this.lblUnits2 = new System.Windows.Forms.Label();
      this.txtReportTolerance = new System.Windows.Forms.TextBox();
      this.tbConfiguration = new System.Windows.Forms.TabControl();
      this.tbMethod = new System.Windows.Forms.TabPage();
      this.lblUnits1 = new System.Windows.Forms.Label();
      this.txtMinimumMove = new System.Windows.Forms.TextBox();
      this.cboFldChoice = new System.Windows.Forms.ComboBox();
      this.tbSelection = new System.Windows.Forms.TabPage();
      this.optMoveBasedOnSelectedParcels = new System.Windows.Forms.RadioButton();
      this.chkPromptForSelection = new System.Windows.Forms.CheckBox();
      this.optMoveBasedOnSelectedFeatures = new System.Windows.Forms.RadioButton();
      this.optMoveAllFeaturesNoSelection = new System.Windows.Forms.RadioButton();
      this.tbReporting = new System.Windows.Forms.TabPage();
      this.tbPointMerge = new System.Windows.Forms.TabPage();
      this.lblUnits3 = new System.Windows.Forms.Label();
      this.txtMergeTolerance = new System.Windows.Forms.TextBox();
      this.chkPointMerge = new System.Windows.Forms.CheckBox();
      this.tbConfiguration.SuspendLayout();
      this.tbMethod.SuspendLayout();
      this.tbSelection.SuspendLayout();
      this.tbReporting.SuspendLayout();
      this.tbPointMerge.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(455, 433);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(111, 28);
      this.btnCancel.TabIndex = 0;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(336, 433);
      this.btnOK.Margin = new System.Windows.Forms.Padding(4);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(111, 28);
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // optPoints
      // 
      this.optPoints.AutoSize = true;
      this.optPoints.Checked = true;
      this.optPoints.Location = new System.Drawing.Point(33, 27);
      this.optPoints.Margin = new System.Windows.Forms.Padding(4);
      this.optPoints.Name = "optPoints";
      this.optPoints.Size = new System.Drawing.Size(201, 21);
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
      this.textBoxDescribeForPoints.Location = new System.Drawing.Point(68, 55);
      this.textBoxDescribeForPoints.Margin = new System.Windows.Forms.Padding(4);
      this.textBoxDescribeForPoints.Multiline = true;
      this.textBoxDescribeForPoints.Name = "textBoxDescribeForPoints";
      this.textBoxDescribeForPoints.Size = new System.Drawing.Size(392, 62);
      this.textBoxDescribeForPoints.TabIndex = 3;
      this.textBoxDescribeForPoints.Text = "Fabric points are moved to the locations of the reference point features that hav" +
    "e the same id. Use point layer with a LONG field named:";
      // 
      // optLines
      // 
      this.optLines.AutoSize = true;
      this.optLines.Location = new System.Drawing.Point(33, 160);
      this.optLines.Margin = new System.Windows.Forms.Padding(4);
      this.optLines.Name = "optLines";
      this.optLines.Size = new System.Drawing.Size(255, 21);
      this.optLines.TabIndex = 4;
      this.optLines.Text = "Use start and end locations of lines ";
      this.optLines.UseVisualStyleBackColor = true;
      this.optLines.CheckedChanged += new System.EventHandler(this.optLines_CheckedChanged);
      // 
      // textBoxDescribeForLines
      // 
      this.textBoxDescribeForLines.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.textBoxDescribeForLines.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxDescribeForLines.Location = new System.Drawing.Point(68, 188);
      this.textBoxDescribeForLines.Margin = new System.Windows.Forms.Padding(4);
      this.textBoxDescribeForLines.Multiline = true;
      this.textBoxDescribeForLines.Name = "textBoxDescribeForLines";
      this.textBoxDescribeForLines.Size = new System.Drawing.Size(392, 63);
      this.textBoxDescribeForLines.TabIndex = 5;
      this.textBoxDescribeForLines.Text = "Fabric points that exactly match the start locations of a reference line are move" +
    "d to the end of that line.";
      // 
      // chkMinimumMove
      // 
      this.chkMinimumMove.AutoSize = true;
      this.chkMinimumMove.Location = new System.Drawing.Point(32, 263);
      this.chkMinimumMove.Margin = new System.Windows.Forms.Padding(4);
      this.chkMinimumMove.Name = "chkMinimumMove";
      this.chkMinimumMove.Size = new System.Drawing.Size(335, 21);
      this.chkMinimumMove.TabIndex = 6;
      this.chkMinimumMove.Text = "Do not move fabric points for changes less than:";
      this.chkMinimumMove.UseVisualStyleBackColor = true;
      this.chkMinimumMove.CheckedChanged += new System.EventHandler(this.chkMinimumMove_CheckedChanged);
      // 
      // chkReport
      // 
      this.chkReport.AutoSize = true;
      this.chkReport.Location = new System.Drawing.Point(35, 30);
      this.chkReport.Margin = new System.Windows.Forms.Padding(4);
      this.chkReport.Name = "chkReport";
      this.chkReport.Size = new System.Drawing.Size(205, 21);
      this.chkReport.TabIndex = 7;
      this.chkReport.Text = "Report fabric point changes";
      this.chkReport.UseVisualStyleBackColor = true;
      this.chkReport.CheckedChanged += new System.EventHandler(this.chkReport_CheckedChanged);
      // 
      // lblUnits2
      // 
      this.lblUnits2.AutoSize = true;
      this.lblUnits2.Location = new System.Drawing.Point(237, 61);
      this.lblUnits2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblUnits2.Name = "lblUnits2";
      this.lblUnits2.Size = new System.Drawing.Size(116, 17);
      this.lblUnits2.TabIndex = 8;
      this.lblUnits2.Text = "<Unknown units>";
      this.lblUnits2.Visible = false;
      // 
      // txtReportTolerance
      // 
      this.txtReportTolerance.Enabled = false;
      this.txtReportTolerance.Location = new System.Drawing.Point(64, 58);
      this.txtReportTolerance.Margin = new System.Windows.Forms.Padding(4);
      this.txtReportTolerance.Name = "txtReportTolerance";
      this.txtReportTolerance.Size = new System.Drawing.Size(165, 22);
      this.txtReportTolerance.TabIndex = 9;
      this.txtReportTolerance.Text = "10.00";
      this.txtReportTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtReportTolerance.Visible = false;
      this.txtReportTolerance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtReportTolerance_KeyDown);
      this.txtReportTolerance.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtReportTolerance_KeyPress);
      // 
      // tbConfiguration
      // 
      this.tbConfiguration.Controls.Add(this.tbMethod);
      this.tbConfiguration.Controls.Add(this.tbSelection);
      this.tbConfiguration.Controls.Add(this.tbReporting);
      this.tbConfiguration.Controls.Add(this.tbPointMerge);
      this.tbConfiguration.Location = new System.Drawing.Point(16, 16);
      this.tbConfiguration.Margin = new System.Windows.Forms.Padding(4);
      this.tbConfiguration.Name = "tbConfiguration";
      this.tbConfiguration.SelectedIndex = 0;
      this.tbConfiguration.Size = new System.Drawing.Size(549, 386);
      this.tbConfiguration.TabIndex = 11;
      // 
      // tbMethod
      // 
      this.tbMethod.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.tbMethod.Controls.Add(this.lblUnits1);
      this.tbMethod.Controls.Add(this.txtMinimumMove);
      this.tbMethod.Controls.Add(this.cboFldChoice);
      this.tbMethod.Controls.Add(this.textBoxDescribeForPoints);
      this.tbMethod.Controls.Add(this.chkMinimumMove);
      this.tbMethod.Controls.Add(this.optPoints);
      this.tbMethod.Controls.Add(this.optLines);
      this.tbMethod.Controls.Add(this.textBoxDescribeForLines);
      this.tbMethod.Location = new System.Drawing.Point(4, 25);
      this.tbMethod.Margin = new System.Windows.Forms.Padding(4);
      this.tbMethod.Name = "tbMethod";
      this.tbMethod.Padding = new System.Windows.Forms.Padding(4);
      this.tbMethod.Size = new System.Drawing.Size(541, 357);
      this.tbMethod.TabIndex = 0;
      this.tbMethod.Text = "Method";
      // 
      // lblUnits1
      // 
      this.lblUnits1.AutoSize = true;
      this.lblUnits1.Location = new System.Drawing.Point(234, 295);
      this.lblUnits1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblUnits1.Name = "lblUnits1";
      this.lblUnits1.Size = new System.Drawing.Size(116, 17);
      this.lblUnits1.TabIndex = 14;
      this.lblUnits1.Text = "<Unknown units>";
      // 
      // txtMinimumMove
      // 
      this.txtMinimumMove.Location = new System.Drawing.Point(61, 292);
      this.txtMinimumMove.Margin = new System.Windows.Forms.Padding(4);
      this.txtMinimumMove.Name = "txtMinimumMove";
      this.txtMinimumMove.Size = new System.Drawing.Size(165, 22);
      this.txtMinimumMove.TabIndex = 13;
      this.txtMinimumMove.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtMinimumMove.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMinimumMove_KeyDown);
      this.txtMinimumMove.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMinimumMove_KeyPress);
      // 
      // cboFldChoice
      // 
      this.cboFldChoice.FormattingEnabled = true;
      this.cboFldChoice.Items.AddRange(new object[] {
            "FABRICPOINTID",
            "blah",
            "blah"});
      this.cboFldChoice.Location = new System.Drawing.Point(145, 111);
      this.cboFldChoice.Margin = new System.Windows.Forms.Padding(4);
      this.cboFldChoice.Name = "cboFldChoice";
      this.cboFldChoice.Size = new System.Drawing.Size(247, 24);
      this.cboFldChoice.TabIndex = 12;
      this.cboFldChoice.Text = "FABRICPOINTID";
      this.cboFldChoice.SelectedIndexChanged += new System.EventHandler(this.cboFldChoice_SelectedIndexChanged);
      // 
      // tbSelection
      // 
      this.tbSelection.Controls.Add(this.optMoveBasedOnSelectedParcels);
      this.tbSelection.Controls.Add(this.chkPromptForSelection);
      this.tbSelection.Controls.Add(this.optMoveBasedOnSelectedFeatures);
      this.tbSelection.Controls.Add(this.optMoveAllFeaturesNoSelection);
      this.tbSelection.Location = new System.Drawing.Point(4, 25);
      this.tbSelection.Margin = new System.Windows.Forms.Padding(4);
      this.tbSelection.Name = "tbSelection";
      this.tbSelection.Padding = new System.Windows.Forms.Padding(4);
      this.tbSelection.Size = new System.Drawing.Size(541, 357);
      this.tbSelection.TabIndex = 1;
      this.tbSelection.Text = "Selected Feature Options";
      this.tbSelection.UseVisualStyleBackColor = true;
      // 
      // optMoveBasedOnSelectedParcels
      // 
      this.optMoveBasedOnSelectedParcels.AutoSize = true;
      this.optMoveBasedOnSelectedParcels.Location = new System.Drawing.Point(33, 100);
      this.optMoveBasedOnSelectedParcels.Margin = new System.Windows.Forms.Padding(4);
      this.optMoveBasedOnSelectedParcels.Name = "optMoveBasedOnSelectedParcels";
      this.optMoveBasedOnSelectedParcels.Size = new System.Drawing.Size(291, 21);
      this.optMoveBasedOnSelectedParcels.TabIndex = 3;
      this.optMoveBasedOnSelectedParcels.Text = "Move fabric points of the selected parcels";
      this.optMoveBasedOnSelectedParcels.UseVisualStyleBackColor = true;
      // 
      // chkPromptForSelection
      // 
      this.chkPromptForSelection.AutoSize = true;
      this.chkPromptForSelection.Location = new System.Drawing.Point(47, 139);
      this.chkPromptForSelection.Margin = new System.Windows.Forms.Padding(4);
      this.chkPromptForSelection.Name = "chkPromptForSelection";
      this.chkPromptForSelection.Size = new System.Drawing.Size(316, 21);
      this.chkPromptForSelection.TabIndex = 2;
      this.chkPromptForSelection.Text = "Prompt for choices when there is no selection";
      this.chkPromptForSelection.UseVisualStyleBackColor = true;
      // 
      // optMoveBasedOnSelectedFeatures
      // 
      this.optMoveBasedOnSelectedFeatures.AutoSize = true;
      this.optMoveBasedOnSelectedFeatures.Location = new System.Drawing.Point(33, 68);
      this.optMoveBasedOnSelectedFeatures.Margin = new System.Windows.Forms.Padding(4);
      this.optMoveBasedOnSelectedFeatures.Name = "optMoveBasedOnSelectedFeatures";
      this.optMoveBasedOnSelectedFeatures.Size = new System.Drawing.Size(362, 21);
      this.optMoveBasedOnSelectedFeatures.TabIndex = 1;
      this.optMoveBasedOnSelectedFeatures.Text = "Move fabric points of the selected reference features";
      this.optMoveBasedOnSelectedFeatures.UseVisualStyleBackColor = true;
      // 
      // optMoveAllFeaturesNoSelection
      // 
      this.optMoveAllFeaturesNoSelection.AutoSize = true;
      this.optMoveAllFeaturesNoSelection.Checked = true;
      this.optMoveAllFeaturesNoSelection.Location = new System.Drawing.Point(33, 27);
      this.optMoveAllFeaturesNoSelection.Margin = new System.Windows.Forms.Padding(4);
      this.optMoveAllFeaturesNoSelection.Name = "optMoveAllFeaturesNoSelection";
      this.optMoveAllFeaturesNoSelection.Size = new System.Drawing.Size(401, 21);
      this.optMoveAllFeaturesNoSelection.TabIndex = 0;
      this.optMoveAllFeaturesNoSelection.TabStop = true;
      this.optMoveAllFeaturesNoSelection.Text = "Ignore selections and move all fabric points with references";
      this.optMoveAllFeaturesNoSelection.UseVisualStyleBackColor = true;
      this.optMoveAllFeaturesNoSelection.CheckedChanged += new System.EventHandler(this.optAllFeaturesNoSelection_CheckedChanged);
      // 
      // tbReporting
      // 
      this.tbReporting.Controls.Add(this.chkReport);
      this.tbReporting.Controls.Add(this.lblUnits2);
      this.tbReporting.Controls.Add(this.txtReportTolerance);
      this.tbReporting.Location = new System.Drawing.Point(4, 25);
      this.tbReporting.Margin = new System.Windows.Forms.Padding(4);
      this.tbReporting.Name = "tbReporting";
      this.tbReporting.Size = new System.Drawing.Size(541, 357);
      this.tbReporting.TabIndex = 2;
      this.tbReporting.Text = "Reporting";
      this.tbReporting.UseVisualStyleBackColor = true;
      // 
      // tbPointMerge
      // 
      this.tbPointMerge.Controls.Add(this.lblUnits3);
      this.tbPointMerge.Controls.Add(this.txtMergeTolerance);
      this.tbPointMerge.Controls.Add(this.chkPointMerge);
      this.tbPointMerge.Location = new System.Drawing.Point(4, 25);
      this.tbPointMerge.Margin = new System.Windows.Forms.Padding(4);
      this.tbPointMerge.Name = "tbPointMerge";
      this.tbPointMerge.Size = new System.Drawing.Size(541, 357);
      this.tbPointMerge.TabIndex = 3;
      this.tbPointMerge.Text = "Merge Point Options";
      this.tbPointMerge.UseVisualStyleBackColor = true;
      // 
      // lblUnits3
      // 
      this.lblUnits3.AutoSize = true;
      this.lblUnits3.Location = new System.Drawing.Point(237, 61);
      this.lblUnits3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblUnits3.Name = "lblUnits3";
      this.lblUnits3.Size = new System.Drawing.Size(116, 17);
      this.lblUnits3.TabIndex = 9;
      this.lblUnits3.Text = "<Unknown units>";
      // 
      // txtMergeTolerance
      // 
      this.txtMergeTolerance.Location = new System.Drawing.Point(65, 58);
      this.txtMergeTolerance.Name = "txtMergeTolerance";
      this.txtMergeTolerance.Size = new System.Drawing.Size(165, 22);
      this.txtMergeTolerance.TabIndex = 1;
      this.txtMergeTolerance.Text = "0.003";
      this.txtMergeTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtMergeTolerance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMergeTolerance_KeyDown);
      this.txtMergeTolerance.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMergeTolerance_KeyPress);
      // 
      // chkPointMerge
      // 
      this.chkPointMerge.AutoSize = true;
      this.chkPointMerge.Checked = true;
      this.chkPointMerge.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkPointMerge.Location = new System.Drawing.Point(35, 30);
      this.chkPointMerge.Name = "chkPointMerge";
      this.chkPointMerge.Size = new System.Drawing.Size(406, 21);
      this.chkPointMerge.TabIndex = 0;
      this.chkPointMerge.Text = "Merge referenced fabric points that become grouped within:";
      this.chkPointMerge.UseVisualStyleBackColor = true;
      this.chkPointMerge.CheckedChanged += new System.EventHandler(this.chkPointMerge_CheckedChanged);
      // 
      // ConfigurationDLG
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(572, 471);
      this.Controls.Add(this.tbConfiguration);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ConfigurationDLG";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Configuration";
      this.Load += new System.EventHandler(this.ConfigurationDLG_Load);
      this.tbConfiguration.ResumeLayout(false);
      this.tbMethod.ResumeLayout(false);
      this.tbMethod.PerformLayout();
      this.tbSelection.ResumeLayout(false);
      this.tbSelection.PerformLayout();
      this.tbReporting.ResumeLayout(false);
      this.tbReporting.PerformLayout();
      this.tbPointMerge.ResumeLayout(false);
      this.tbPointMerge.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    internal System.Windows.Forms.RadioButton optPoints;
    private System.Windows.Forms.TextBox textBoxDescribeForPoints;
    private System.Windows.Forms.TextBox textBoxDescribeForLines;
    internal System.Windows.Forms.RadioButton optLines;
    internal System.Windows.Forms.CheckBox chkMinimumMove;
    internal System.Windows.Forms.TextBox txtReportTolerance;
    internal System.Windows.Forms.Label lblUnits2;
    internal System.Windows.Forms.CheckBox chkReport;
    private System.Windows.Forms.TabPage tbMethod;
    private System.Windows.Forms.TabPage tbSelection;
    private System.Windows.Forms.TabPage tbReporting;
    internal System.Windows.Forms.TabControl tbConfiguration;
    internal System.Windows.Forms.ComboBox cboFldChoice;
    internal System.Windows.Forms.CheckBox chkPromptForSelection;
    internal System.Windows.Forms.RadioButton optMoveBasedOnSelectedParcels;
    internal System.Windows.Forms.RadioButton optMoveBasedOnSelectedFeatures;
    internal System.Windows.Forms.RadioButton optMoveAllFeaturesNoSelection;
    internal System.Windows.Forms.Label lblUnits1;
    internal System.Windows.Forms.TextBox txtMinimumMove;
    private System.Windows.Forms.TabPage tbPointMerge;
    internal System.Windows.Forms.TextBox txtMergeTolerance;
    internal System.Windows.Forms.Label lblUnits3;
    internal System.Windows.Forms.CheckBox chkPointMerge;
  }
}