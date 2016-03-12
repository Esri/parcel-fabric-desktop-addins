namespace ParcelFabricQualityControl
{
  partial class ReportDLG
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
      this.txtReport = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // txtReport
      // 
      this.txtReport.Dock = System.Windows.Forms.DockStyle.Top;
      this.txtReport.Location = new System.Drawing.Point(0, 0);
      this.txtReport.Multiline = true;
      this.txtReport.Name = "txtReport";
      this.txtReport.ReadOnly = true;
      this.txtReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtReport.Size = new System.Drawing.Size(460, 312);
      this.txtReport.TabIndex = 0;
      this.txtReport.TabStop = false;
      this.txtReport.WordWrap = false;
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button1.Location = new System.Drawing.Point(365, 318);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(83, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // ReportDLG
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(460, 353);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.txtReport);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ReportDLG";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Report";
      this.TopMost = true;
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    internal System.Windows.Forms.TextBox txtReport;
    private System.Windows.Forms.Button button1;
  }
}