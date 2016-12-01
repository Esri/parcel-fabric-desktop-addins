namespace ParcelEditHelper
{
  partial class AdjustmentDockWindow
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
      this.btnRun = new System.Windows.Forms.Button();
      this.btnSettings = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblMaximumShift = new System.Windows.Forms.Label();
      this.lblAdjResult = new System.Windows.Forms.Label();
      this.chkUseLinePoints = new System.Windows.Forms.CheckBox();
      this.lblUnits1 = new System.Windows.Forms.Label();
      this.lblInfo2 = new System.Windows.Forms.Label();
      this.lblInfo1 = new System.Windows.Forms.Label();
      this.txtMainDistResReport = new System.Windows.Forms.TextBox();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.label1 = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnRun
      // 
      this.btnRun.Dock = System.Windows.Forms.DockStyle.Fill;
      this.btnRun.Location = new System.Drawing.Point(37, 0);
      this.btnRun.Name = "btnRun";
      this.btnRun.Size = new System.Drawing.Size(429, 29);
      this.btnRun.TabIndex = 0;
      this.btnRun.Text = "Run >";
      this.btnRun.UseVisualStyleBackColor = true;
      this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
      // 
      // btnSettings
      // 
      this.btnSettings.Dock = System.Windows.Forms.DockStyle.Left;
      this.btnSettings.Location = new System.Drawing.Point(0, 0);
      this.btnSettings.Name = "btnSettings";
      this.btnSettings.Size = new System.Drawing.Size(37, 29);
      this.btnSettings.TabIndex = 1;
      this.btnSettings.Text = "V";
      this.btnSettings.UseVisualStyleBackColor = true;
      this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.lblMaximumShift);
      this.panel1.Controls.Add(this.lblAdjResult);
      this.panel1.Controls.Add(this.chkUseLinePoints);
      this.panel1.Controls.Add(this.lblUnits1);
      this.panel1.Controls.Add(this.lblInfo2);
      this.panel1.Controls.Add(this.lblInfo1);
      this.panel1.Controls.Add(this.txtMainDistResReport);
      this.panel1.Controls.Add(this.linkLabel1);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Controls.Add(this.panel2);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(466, 445);
      this.panel1.TabIndex = 2;
      this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
      // 
      // lblMaximumShift
      // 
      this.lblMaximumShift.AutoSize = true;
      this.lblMaximumShift.Location = new System.Drawing.Point(19, 209);
      this.lblMaximumShift.Name = "lblMaximumShift";
      this.lblMaximumShift.Size = new System.Drawing.Size(88, 17);
      this.lblMaximumShift.TabIndex = 16;
      this.lblMaximumShift.Text = "<MAXIMUM>";
      this.lblMaximumShift.Visible = false;
      // 
      // lblAdjResult
      // 
      this.lblAdjResult.AutoSize = true;
      this.lblAdjResult.Location = new System.Drawing.Point(19, 184);
      this.lblAdjResult.Name = "lblAdjResult";
      this.lblAdjResult.Size = new System.Drawing.Size(79, 17);
      this.lblAdjResult.TabIndex = 15;
      this.lblAdjResult.Text = "<RESULT>";
      this.lblAdjResult.Visible = false;
      // 
      // chkUseLinePoints
      // 
      this.chkUseLinePoints.AutoSize = true;
      this.chkUseLinePoints.Checked = true;
      this.chkUseLinePoints.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkUseLinePoints.Location = new System.Drawing.Point(22, 135);
      this.chkUseLinePoints.Name = "chkUseLinePoints";
      this.chkUseLinePoints.Size = new System.Drawing.Size(227, 21);
      this.chkUseLinePoints.TabIndex = 14;
      this.chkUseLinePoints.Text = "Use line points (recommended)";
      this.chkUseLinePoints.UseVisualStyleBackColor = true;
      // 
      // lblUnits1
      // 
      this.lblUnits1.AutoSize = true;
      this.lblUnits1.Location = new System.Drawing.Point(94, 40);
      this.lblUnits1.Name = "lblUnits1";
      this.lblUnits1.Size = new System.Drawing.Size(118, 17);
      this.lblUnits1.TabIndex = 13;
      this.lblUnits1.Text = "<Unknown Units>";
      // 
      // lblInfo2
      // 
      this.lblInfo2.AutoSize = true;
      this.lblInfo2.Location = new System.Drawing.Point(19, 97);
      this.lblInfo2.Name = "lblInfo2";
      this.lblInfo2.Size = new System.Drawing.Size(209, 17);
      this.lblInfo2.TabIndex = 12;
      this.lblInfo2.Text = "three times this report tolerance";
      // 
      // lblInfo1
      // 
      this.lblInfo1.AutoSize = true;
      this.lblInfo1.Location = new System.Drawing.Point(19, 80);
      this.lblInfo1.Name = "lblInfo1";
      this.lblInfo1.Size = new System.Drawing.Size(285, 17);
      this.lblInfo1.TabIndex = 11;
      this.lblInfo1.Text = "Adjustment fails if any residual is more than ";
      // 
      // txtMainDistResReport
      // 
      this.txtMainDistResReport.Location = new System.Drawing.Point(22, 37);
      this.txtMainDistResReport.Name = "txtMainDistResReport";
      this.txtMainDistResReport.Size = new System.Drawing.Size(66, 22);
      this.txtMainDistResReport.TabIndex = 8;
      this.txtMainDistResReport.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.linkLabel1.Location = new System.Drawing.Point(0, 399);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(157, 17);
      this.linkLabel1.TabIndex = 4;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "About fabric adjustment";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(19, 17);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(230, 17);
      this.label1.TabIndex = 3;
      this.label1.Text = "Distance residual report tolerance :";
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.btnRun);
      this.panel2.Controls.Add(this.btnSettings);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel2.Location = new System.Drawing.Point(0, 416);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(466, 29);
      this.panel2.TabIndex = 2;
      // 
      // AdjustmentDockWindow
      // 
      this.Controls.Add(this.panel1);
      this.Name = "AdjustmentDockWindow";
      this.Size = new System.Drawing.Size(466, 445);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnRun;
    private System.Windows.Forms.Button btnSettings;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblInfo2;
    private System.Windows.Forms.Label lblInfo1;
    private System.Windows.Forms.Label lblUnits1;
    private System.Windows.Forms.CheckBox chkUseLinePoints;
    internal System.Windows.Forms.TextBox txtMainDistResReport;
    private System.Windows.Forms.Label lblMaximumShift;
    private System.Windows.Forms.Label lblAdjResult;

  }
}
