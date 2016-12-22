namespace ParcelEditHelper
{
  partial class ToolbarHelpDlg
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolbarHelpDlg));
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStripBtnVideoHelp = new System.Windows.Forms.ToolStripButton();
      this.toolStripBtnGoBack = new System.Windows.Forms.ToolStripButton();
      this.webBrowser1 = new System.Windows.Forms.WebBrowser();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // toolStrip1
      // 
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripBtnVideoHelp,
            this.toolStripBtnGoBack});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(524, 27);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      this.toolStrip1.Visible = false;
      this.toolStrip1.MouseEnter += new System.EventHandler(this.toolStrip1_MouseEnter);
      // 
      // toolStripBtnVideoHelp
      // 
      this.toolStripBtnVideoHelp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripBtnVideoHelp.Image")));
      this.toolStripBtnVideoHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripBtnVideoHelp.Name = "toolStripBtnVideoHelp";
      this.toolStripBtnVideoHelp.Size = new System.Drawing.Size(72, 24);
      this.toolStripBtnVideoHelp.Text = "Video";
      this.toolStripBtnVideoHelp.ToolTipText = "Video Help";
      this.toolStripBtnVideoHelp.Click += new System.EventHandler(this.toolStripBtnVideoHelp_Click);
      // 
      // toolStripBtnGoBack
      // 
      this.toolStripBtnGoBack.Image = ((System.Drawing.Image)(resources.GetObject("toolStripBtnGoBack.Image")));
      this.toolStripBtnGoBack.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripBtnGoBack.Name = "toolStripBtnGoBack";
      this.toolStripBtnGoBack.Size = new System.Drawing.Size(64, 24);
      this.toolStripBtnGoBack.Text = "Back";
      this.toolStripBtnGoBack.Click += new System.EventHandler(this.toolStripBtnGoBack_Click);
      // 
      // webBrowser1
      // 
      this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.webBrowser1.Location = new System.Drawing.Point(0, 27);
      this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
      this.webBrowser1.Name = "webBrowser1";
      this.webBrowser1.Size = new System.Drawing.Size(524, 344);
      this.webBrowser1.TabIndex = 1;
      // 
      // ToolbarHelpDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(524, 371);
      this.Controls.Add(this.webBrowser1);
      this.Controls.Add(this.toolStrip1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "ToolbarHelpDlg";
      this.Text = "Help";
      this.TopMost = true;
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton toolStripBtnVideoHelp;
    private System.Windows.Forms.ToolStripButton toolStripBtnGoBack;
    internal System.Windows.Forms.WebBrowser webBrowser1;
  }
}