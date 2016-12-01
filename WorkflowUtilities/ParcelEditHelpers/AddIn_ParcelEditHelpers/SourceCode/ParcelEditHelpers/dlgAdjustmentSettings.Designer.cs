namespace ParcelEditHelper
{
  partial class dlgAdjustmentSettings
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.numRepeatCount = new System.Windows.Forms.NumericUpDown();
      this.txtConvergenceValue = new System.Windows.Forms.TextBox();
      this.txtDivergenceValue = new System.Windows.Forms.TextBox();
      this.lblUnits1 = new System.Windows.Forms.Label();
      this.lblUnits2 = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.lblUnits6 = new System.Windows.Forms.Label();
      this.lblUnits5 = new System.Windows.Forms.Label();
      this.lblUnits4 = new System.Windows.Forms.Label();
      this.lblUnits3 = new System.Windows.Forms.Label();
      this.txtClosePointsTolerance = new System.Windows.Forms.TextBox();
      this.txtDistTolerance = new System.Windows.Forms.TextBox();
      this.txtBearingTolerance = new System.Windows.Forms.TextBox();
      this.txtLinePtsOffsetTolerance = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.txtBrowseFilePath = new System.Windows.Forms.TextBox();
      this.cboReportType = new System.Windows.Forms.ComboBox();
      this.label10 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.label16 = new System.Windows.Forms.Label();
      this.lblUnits7 = new System.Windows.Forms.Label();
      this.txtBendLinesTolerance = new System.Windows.Forms.TextBox();
      this.chkBendLines = new System.Windows.Forms.CheckBox();
      this.chkIncludeDependentLines = new System.Windows.Forms.CheckBox();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.label14 = new System.Windows.Forms.Label();
      this.txtStraightRoadAngleTolerance = new System.Windows.Forms.TextBox();
      this.lblUnits9 = new System.Windows.Forms.Label();
      this.txtStraightRoadOffsetTolerance = new System.Windows.Forms.TextBox();
      this.chkStraightenRoadFrontages = new System.Windows.Forms.CheckBox();
      this.lblUnits8 = new System.Windows.Forms.Label();
      this.txtSnapLinePointTolerance = new System.Windows.Forms.TextBox();
      this.lblAfterAdj = new System.Windows.Forms.Label();
      this.chkSnapLinePointsToLines = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.numRepeatCount)).BeginInit();
      this.tabControl1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(5, 17);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(241, 17);
      this.label1.TabIndex = 0;
      this.label1.Text = "Repeat the adjustment a maximum of";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(361, 17);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(41, 17);
      this.label2.TabIndex = 2;
      this.label2.Text = "times";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(5, 58);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(237, 17);
      this.label3.TabIndex = 3;
      this.label3.Text = "Stop and report adjustment results if";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(9, 78);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(191, 17);
      this.label4.TabIndex = 4;
      this.label4.Text = "the maximum coordinate shift";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(165, 103);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(79, 17);
      this.label5.TabIndex = 5;
      this.label5.Text = "is less than";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(89, 130);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(156, 17);
      this.label6.TabIndex = 6;
      this.label6.Text = "increases by more than";
      // 
      // numRepeatCount
      // 
      this.numRepeatCount.Location = new System.Drawing.Point(288, 15);
      this.numRepeatCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.numRepeatCount.Name = "numRepeatCount";
      this.numRepeatCount.ReadOnly = true;
      this.numRepeatCount.Size = new System.Drawing.Size(67, 22);
      this.numRepeatCount.TabIndex = 7;
      this.numRepeatCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.numRepeatCount.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // txtConvergenceValue
      // 
      this.txtConvergenceValue.Location = new System.Drawing.Point(288, 100);
      this.txtConvergenceValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txtConvergenceValue.Name = "txtConvergenceValue";
      this.txtConvergenceValue.Size = new System.Drawing.Size(67, 22);
      this.txtConvergenceValue.TabIndex = 8;
      this.txtConvergenceValue.Text = "0.003";
      this.txtConvergenceValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtConvergenceValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtConvergenceValue_KeyDown);
      this.txtConvergenceValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtConvergenceValue_KeyPress);
      // 
      // txtDivergenceValue
      // 
      this.txtDivergenceValue.Location = new System.Drawing.Point(288, 128);
      this.txtDivergenceValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txtDivergenceValue.Name = "txtDivergenceValue";
      this.txtDivergenceValue.Size = new System.Drawing.Size(67, 22);
      this.txtDivergenceValue.TabIndex = 9;
      this.txtDivergenceValue.Text = "0.75";
      this.txtDivergenceValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtDivergenceValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDivergenceValue_KeyDown);
      this.txtDivergenceValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDivergenceValue_KeyPress);
      // 
      // lblUnits1
      // 
      this.lblUnits1.AutoSize = true;
      this.lblUnits1.Location = new System.Drawing.Point(361, 103);
      this.lblUnits1.Name = "lblUnits1";
      this.lblUnits1.Size = new System.Drawing.Size(118, 17);
      this.lblUnits1.TabIndex = 10;
      this.lblUnits1.Text = "<Unknown Units>";
      // 
      // lblUnits2
      // 
      this.lblUnits2.AutoSize = true;
      this.lblUnits2.Location = new System.Drawing.Point(361, 130);
      this.lblUnits2.Name = "lblUnits2";
      this.lblUnits2.Size = new System.Drawing.Size(118, 17);
      this.lblUnits2.TabIndex = 11;
      this.lblUnits2.Text = "<Unknown Units>";
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(427, 310);
      this.btnOK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(137, 32);
      this.btnOK.TabIndex = 12;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(284, 310);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(137, 32);
      this.btnCancel.TabIndex = 13;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage4);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Location = new System.Drawing.Point(12, 12);
      this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(552, 284);
      this.tabControl1.TabIndex = 14;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.lblUnits6);
      this.tabPage2.Controls.Add(this.lblUnits5);
      this.tabPage2.Controls.Add(this.lblUnits4);
      this.tabPage2.Controls.Add(this.lblUnits3);
      this.tabPage2.Controls.Add(this.txtClosePointsTolerance);
      this.tabPage2.Controls.Add(this.txtDistTolerance);
      this.tabPage2.Controls.Add(this.txtBearingTolerance);
      this.tabPage2.Controls.Add(this.txtLinePtsOffsetTolerance);
      this.tabPage2.Controls.Add(this.label11);
      this.tabPage2.Controls.Add(this.btnBrowse);
      this.tabPage2.Controls.Add(this.txtBrowseFilePath);
      this.tabPage2.Controls.Add(this.cboReportType);
      this.tabPage2.Controls.Add(this.label10);
      this.tabPage2.Controls.Add(this.label9);
      this.tabPage2.Controls.Add(this.label8);
      this.tabPage2.Controls.Add(this.label7);
      this.tabPage2.Location = new System.Drawing.Point(4, 25);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tabPage2.Size = new System.Drawing.Size(544, 255);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Reporting";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // lblUnits6
      // 
      this.lblUnits6.AutoSize = true;
      this.lblUnits6.Location = new System.Drawing.Point(372, 103);
      this.lblUnits6.Name = "lblUnits6";
      this.lblUnits6.Size = new System.Drawing.Size(118, 17);
      this.lblUnits6.TabIndex = 15;
      this.lblUnits6.Text = "<Unknown Units>";
      // 
      // lblUnits5
      // 
      this.lblUnits5.AutoSize = true;
      this.lblUnits5.Location = new System.Drawing.Point(372, 73);
      this.lblUnits5.Name = "lblUnits5";
      this.lblUnits5.Size = new System.Drawing.Size(118, 17);
      this.lblUnits5.TabIndex = 14;
      this.lblUnits5.Text = "<Unknown Units>";
      // 
      // lblUnits4
      // 
      this.lblUnits4.AutoSize = true;
      this.lblUnits4.Location = new System.Drawing.Point(372, 46);
      this.lblUnits4.Name = "lblUnits4";
      this.lblUnits4.Size = new System.Drawing.Size(61, 17);
      this.lblUnits4.TabIndex = 13;
      this.lblUnits4.Text = "seconds";
      // 
      // lblUnits3
      // 
      this.lblUnits3.AutoSize = true;
      this.lblUnits3.Location = new System.Drawing.Point(372, 17);
      this.lblUnits3.Name = "lblUnits3";
      this.lblUnits3.Size = new System.Drawing.Size(118, 17);
      this.lblUnits3.TabIndex = 12;
      this.lblUnits3.Text = "<Unknown Units>";
      // 
      // txtClosePointsTolerance
      // 
      this.txtClosePointsTolerance.Location = new System.Drawing.Point(288, 100);
      this.txtClosePointsTolerance.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txtClosePointsTolerance.Name = "txtClosePointsTolerance";
      this.txtClosePointsTolerance.Size = new System.Drawing.Size(67, 22);
      this.txtClosePointsTolerance.TabIndex = 11;
      this.txtClosePointsTolerance.Text = "0.6";
      this.txtClosePointsTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtClosePointsTolerance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtClosePointsTolerance_KeyDown);
      this.txtClosePointsTolerance.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtClosePointsTolerance_KeyPress);
      // 
      // txtDistTolerance
      // 
      this.txtDistTolerance.Location = new System.Drawing.Point(288, 14);
      this.txtDistTolerance.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txtDistTolerance.Name = "txtDistTolerance";
      this.txtDistTolerance.Size = new System.Drawing.Size(67, 22);
      this.txtDistTolerance.TabIndex = 10;
      this.txtDistTolerance.Text = "3";
      this.txtDistTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtDistTolerance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDistTolerance_KeyDown);
      this.txtDistTolerance.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDistTolerance_KeyPress);
      // 
      // txtBearingTolerance
      // 
      this.txtBearingTolerance.Location = new System.Drawing.Point(288, 42);
      this.txtBearingTolerance.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txtBearingTolerance.Name = "txtBearingTolerance";
      this.txtBearingTolerance.Size = new System.Drawing.Size(67, 22);
      this.txtBearingTolerance.TabIndex = 9;
      this.txtBearingTolerance.Text = "6000";
      this.txtBearingTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtBearingTolerance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBearingTolerance_KeyDown);
      this.txtBearingTolerance.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBearingTolerance_KeyPress);
      // 
      // txtLinePtsOffsetTolerance
      // 
      this.txtLinePtsOffsetTolerance.Location = new System.Drawing.Point(288, 70);
      this.txtLinePtsOffsetTolerance.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txtLinePtsOffsetTolerance.Name = "txtLinePtsOffsetTolerance";
      this.txtLinePtsOffsetTolerance.Size = new System.Drawing.Size(67, 22);
      this.txtLinePtsOffsetTolerance.TabIndex = 8;
      this.txtLinePtsOffsetTolerance.Text = "0.6";
      this.txtLinePtsOffsetTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtLinePtsOffsetTolerance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtLinePtsOffsetTolerance_KeyDown);
      this.txtLinePtsOffsetTolerance.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLinePtsOffsetTolerance_KeyPress);
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(5, 103);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(173, 17);
      this.label11.TabIndex = 7;
      this.label11.Text = "Report close points within:";
      // 
      // btnBrowse
      // 
      this.btnBrowse.Location = new System.Drawing.Point(376, 210);
      this.btnBrowse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(119, 32);
      this.btnBrowse.TabIndex = 6;
      this.btnBrowse.Text = "Browse...";
      this.btnBrowse.UseVisualStyleBackColor = true;
      this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
      // 
      // txtBrowseFilePath
      // 
      this.txtBrowseFilePath.Location = new System.Drawing.Point(9, 215);
      this.txtBrowseFilePath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txtBrowseFilePath.Name = "txtBrowseFilePath";
      this.txtBrowseFilePath.Size = new System.Drawing.Size(351, 22);
      this.txtBrowseFilePath.TabIndex = 5;
      this.txtBrowseFilePath.Text = "C:\\Users\\tim2379\\AppData\\Local\\Temp";
      // 
      // cboReportType
      // 
      this.cboReportType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboReportType.FormattingEnabled = true;
      this.cboReportType.Items.AddRange(new object[] {
            "Summary report",
            "Standard report",
            "Extended report"});
      this.cboReportType.Location = new System.Drawing.Point(157, 178);
      this.cboReportType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cboReportType.MaxDropDownItems = 3;
      this.cboReportType.Name = "cboReportType";
      this.cboReportType.Size = new System.Drawing.Size(201, 24);
      this.cboReportType.TabIndex = 4;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(5, 182);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(86, 17);
      this.label10.TabIndex = 3;
      this.label10.Text = "Report type:";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(5, 73);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(230, 17);
      this.label9.TabIndex = 2;
      this.label9.Text = "Report line points offset more than:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(5, 46);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(241, 17);
      this.label8.TabIndex = 1;
      this.label8.Text = "Report bearing residuals larger than:";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(5, 17);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(250, 17);
      this.label7.TabIndex = 0;
      this.label7.Text = "Report distance residuals larger than: ";
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.label1);
      this.tabPage1.Controls.Add(this.numRepeatCount);
      this.tabPage1.Controls.Add(this.label2);
      this.tabPage1.Controls.Add(this.lblUnits2);
      this.tabPage1.Controls.Add(this.label3);
      this.tabPage1.Controls.Add(this.lblUnits1);
      this.tabPage1.Controls.Add(this.label4);
      this.tabPage1.Controls.Add(this.txtDivergenceValue);
      this.tabPage1.Controls.Add(this.label5);
      this.tabPage1.Controls.Add(this.txtConvergenceValue);
      this.tabPage1.Controls.Add(this.label6);
      this.tabPage1.Location = new System.Drawing.Point(4, 25);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tabPage1.Size = new System.Drawing.Size(544, 255);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Iteration";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabPage4
      // 
      this.tabPage4.Controls.Add(this.label16);
      this.tabPage4.Controls.Add(this.lblUnits7);
      this.tabPage4.Controls.Add(this.txtBendLinesTolerance);
      this.tabPage4.Controls.Add(this.chkBendLines);
      this.tabPage4.Controls.Add(this.chkIncludeDependentLines);
      this.tabPage4.Location = new System.Drawing.Point(4, 25);
      this.tabPage4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(544, 255);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Lines";
      this.tabPage4.UseVisualStyleBackColor = true;
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(5, 17);
      this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(334, 17);
      this.label16.TabIndex = 4;
      this.label16.Text = "Line points are used to prevent gaps and overlaps. ";
      // 
      // lblUnits7
      // 
      this.lblUnits7.AutoSize = true;
      this.lblUnits7.Location = new System.Drawing.Point(127, 82);
      this.lblUnits7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblUnits7.Name = "lblUnits7";
      this.lblUnits7.Size = new System.Drawing.Size(118, 17);
      this.lblUnits7.TabIndex = 3;
      this.lblUnits7.Text = "<Unknown Units>";
      // 
      // txtBendLinesTolerance
      // 
      this.txtBendLinesTolerance.Location = new System.Drawing.Point(29, 79);
      this.txtBendLinesTolerance.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.txtBendLinesTolerance.Name = "txtBendLinesTolerance";
      this.txtBendLinesTolerance.Size = new System.Drawing.Size(88, 22);
      this.txtBendLinesTolerance.TabIndex = 2;
      this.txtBendLinesTolerance.Text = "3.0";
      this.txtBendLinesTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtBendLinesTolerance.TextChanged += new System.EventHandler(this.txtBendLinesTolerance_TextChanged);
      // 
      // chkBendLines
      // 
      this.chkBendLines.AutoSize = true;
      this.chkBendLines.Location = new System.Drawing.Point(29, 50);
      this.chkBendLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.chkBendLines.Name = "chkBendLines";
      this.chkBendLines.Size = new System.Drawing.Size(338, 21);
      this.chkBendLines.TabIndex = 1;
      this.chkBendLines.Text = "Bend lines to fit line points that are offset beyond";
      this.chkBendLines.UseVisualStyleBackColor = true;
      this.chkBendLines.CheckedChanged += new System.EventHandler(this.chkBendLines_CheckedChanged);
      // 
      // chkIncludeDependentLines
      // 
      this.chkIncludeDependentLines.AutoSize = true;
      this.chkIncludeDependentLines.Location = new System.Drawing.Point(29, 127);
      this.chkIncludeDependentLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.chkIncludeDependentLines.Name = "chkIncludeDependentLines";
      this.chkIncludeDependentLines.Size = new System.Drawing.Size(268, 21);
      this.chkIncludeDependentLines.TabIndex = 0;
      this.chkIncludeDependentLines.Text = "Include dependent lines in adjustment";
      this.chkIncludeDependentLines.UseVisualStyleBackColor = true;
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.label14);
      this.tabPage3.Controls.Add(this.txtStraightRoadAngleTolerance);
      this.tabPage3.Controls.Add(this.lblUnits9);
      this.tabPage3.Controls.Add(this.txtStraightRoadOffsetTolerance);
      this.tabPage3.Controls.Add(this.chkStraightenRoadFrontages);
      this.tabPage3.Controls.Add(this.lblUnits8);
      this.tabPage3.Controls.Add(this.txtSnapLinePointTolerance);
      this.tabPage3.Controls.Add(this.lblAfterAdj);
      this.tabPage3.Controls.Add(this.chkSnapLinePointsToLines);
      this.tabPage3.Location = new System.Drawing.Point(4, 25);
      this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(544, 255);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Post-processing";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(381, 164);
      this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(63, 17);
      this.label14.TabIndex = 8;
      this.label14.Text = "Seconds";
      // 
      // txtStraightRoadAngleTolerance
      // 
      this.txtStraightRoadAngleTolerance.Location = new System.Drawing.Point(304, 160);
      this.txtStraightRoadAngleTolerance.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.txtStraightRoadAngleTolerance.Name = "txtStraightRoadAngleTolerance";
      this.txtStraightRoadAngleTolerance.Size = new System.Drawing.Size(68, 22);
      this.txtStraightRoadAngleTolerance.TabIndex = 7;
      this.txtStraightRoadAngleTolerance.Text = "30";
      this.txtStraightRoadAngleTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblUnits9
      // 
      this.lblUnits9.AutoSize = true;
      this.lblUnits9.Location = new System.Drawing.Point(135, 164);
      this.lblUnits9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblUnits9.Name = "lblUnits9";
      this.lblUnits9.Size = new System.Drawing.Size(110, 17);
      this.lblUnits9.TabIndex = 6;
      this.lblUnits9.Text = "<Unkown Units>";
      // 
      // txtStraightRoadOffsetTolerance
      // 
      this.txtStraightRoadOffsetTolerance.Location = new System.Drawing.Point(29, 160);
      this.txtStraightRoadOffsetTolerance.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.txtStraightRoadOffsetTolerance.Name = "txtStraightRoadOffsetTolerance";
      this.txtStraightRoadOffsetTolerance.Size = new System.Drawing.Size(88, 22);
      this.txtStraightRoadOffsetTolerance.TabIndex = 5;
      this.txtStraightRoadOffsetTolerance.Text = "0.3";
      this.txtStraightRoadOffsetTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // chkStraightenRoadFrontages
      // 
      this.chkStraightenRoadFrontages.AutoSize = true;
      this.chkStraightenRoadFrontages.Location = new System.Drawing.Point(29, 127);
      this.chkStraightenRoadFrontages.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.chkStraightenRoadFrontages.Name = "chkStraightenRoadFrontages";
      this.chkStraightenRoadFrontages.Size = new System.Drawing.Size(253, 21);
      this.chkStraightenRoadFrontages.TabIndex = 4;
      this.chkStraightenRoadFrontages.Text = "Straighten co-linear line sequences";
      this.chkStraightenRoadFrontages.UseVisualStyleBackColor = true;
      // 
      // lblUnits8
      // 
      this.lblUnits8.AutoSize = true;
      this.lblUnits8.Location = new System.Drawing.Point(127, 82);
      this.lblUnits8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblUnits8.Name = "lblUnits8";
      this.lblUnits8.Size = new System.Drawing.Size(118, 17);
      this.lblUnits8.TabIndex = 3;
      this.lblUnits8.Text = "<Unknown Units>";
      // 
      // txtSnapLinePointTolerance
      // 
      this.txtSnapLinePointTolerance.Location = new System.Drawing.Point(29, 79);
      this.txtSnapLinePointTolerance.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.txtSnapLinePointTolerance.Name = "txtSnapLinePointTolerance";
      this.txtSnapLinePointTolerance.Size = new System.Drawing.Size(88, 22);
      this.txtSnapLinePointTolerance.TabIndex = 2;
      this.txtSnapLinePointTolerance.Text = "0.328";
      this.txtSnapLinePointTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblAfterAdj
      // 
      this.lblAfterAdj.AutoSize = true;
      this.lblAfterAdj.Location = new System.Drawing.Point(7, 17);
      this.lblAfterAdj.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblAfterAdj.Name = "lblAfterAdj";
      this.lblAfterAdj.Size = new System.Drawing.Size(203, 17);
      this.lblAfterAdj.TabIndex = 1;
      this.lblAfterAdj.Text = "After the adjustment completes";
      // 
      // chkSnapLinePointsToLines
      // 
      this.chkSnapLinePointsToLines.AutoSize = true;
      this.chkSnapLinePointsToLines.Location = new System.Drawing.Point(29, 50);
      this.chkSnapLinePointsToLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.chkSnapLinePointsToLines.Name = "chkSnapLinePointsToLines";
      this.chkSnapLinePointsToLines.Size = new System.Drawing.Size(306, 21);
      this.chkSnapLinePointsToLines.TabIndex = 0;
      this.chkSnapLinePointsToLines.Text = "Snap line points on to lines if they are within";
      this.chkSnapLinePointsToLines.UseVisualStyleBackColor = true;
      this.chkSnapLinePointsToLines.CheckedChanged += new System.EventHandler(this.chkSnapLinePointsToLines_CheckedChanged);
      // 
      // dlgAdjustmentSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(579, 352);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MaximizeBox = false;
      this.Name = "dlgAdjustmentSettings";
      this.Text = "Settings";
      this.TopMost = true;
      ((System.ComponentModel.ISupportInitialize)(this.numRepeatCount)).EndInit();
      this.tabControl1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.tabPage4.ResumeLayout(false);
      this.tabPage4.PerformLayout();
      this.tabPage3.ResumeLayout(false);
      this.tabPage3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.NumericUpDown numRepeatCount;
    private System.Windows.Forms.TextBox txtConvergenceValue;
    private System.Windows.Forms.TextBox txtDivergenceValue;
    private System.Windows.Forms.Label lblUnits1;
    private System.Windows.Forms.Label lblUnits2;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.TextBox txtBrowseFilePath;
    private System.Windows.Forms.TextBox txtClosePointsTolerance;
    private System.Windows.Forms.TextBox txtDistTolerance;
    private System.Windows.Forms.TextBox txtBearingTolerance;
    private System.Windows.Forms.TextBox txtLinePtsOffsetTolerance;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label lblUnits6;
    private System.Windows.Forms.Label lblUnits5;
    private System.Windows.Forms.Label lblUnits4;
    private System.Windows.Forms.Label lblUnits3;
    internal System.Windows.Forms.ComboBox cboReportType;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.Label lblUnits8;
    private System.Windows.Forms.TextBox txtSnapLinePointTolerance;
    private System.Windows.Forms.Label lblAfterAdj;
    private System.Windows.Forms.CheckBox chkSnapLinePointsToLines;
    private System.Windows.Forms.CheckBox chkStraightenRoadFrontages;
    private System.Windows.Forms.Label lblUnits9;
    private System.Windows.Forms.TextBox txtStraightRoadOffsetTolerance;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.TextBox txtStraightRoadAngleTolerance;
    private System.Windows.Forms.TabPage tabPage4;
    private System.Windows.Forms.CheckBox chkBendLines;
    private System.Windows.Forms.CheckBox chkIncludeDependentLines;
    private System.Windows.Forms.Label lblUnits7;
    private System.Windows.Forms.TextBox txtBendLinesTolerance;
    private System.Windows.Forms.Label label16;
  }
}