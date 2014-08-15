namespace DeleteSelectedParcels
{
  partial class dlgTruncate
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
      this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.txtBoxSummary = new System.Windows.Forms.TextBox();
      this.btnTruncate = new System.Windows.Forms.Button();
      this.button5 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // checkedListBox1
      // 
      this.checkedListBox1.CheckOnClick = true;
      this.checkedListBox1.FormattingEnabled = true;
      this.checkedListBox1.Items.AddRange(new object[] {
            "Drop all control points",
            "Drop all plans, parcels, lines, points, and line-points",
            "Drop all fabric jobs",
            "Drop all feature adjustment information and vectors",
            "Reset the Accuracy table to the default record values"});
      this.checkedListBox1.Location = new System.Drawing.Point(16, 34);
      this.checkedListBox1.Name = "checkedListBox1";
      this.checkedListBox1.Size = new System.Drawing.Size(291, 199);
      this.checkedListBox1.TabIndex = 0;
      this.checkedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
      this.checkedListBox1.SelectedValueChanged += new System.EventHandler(this.checkedListBox1_SelectedValueChanged);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(319, 34);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(101, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Select All";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(319, 63);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(101, 23);
      this.button2.TabIndex = 2;
      this.button2.Text = "Deselect All";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 10);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(231, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Choose fabric table groups and then click Next:";
      // 
      // button3
      // 
      this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button3.Location = new System.Drawing.Point(319, 152);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(101, 23);
      this.button3.TabIndex = 4;
      this.button3.Text = "Cancel";
      this.button3.UseVisualStyleBackColor = true;
      // 
      // button4
      // 
      this.button4.Enabled = false;
      this.button4.Location = new System.Drawing.Point(319, 210);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(101, 23);
      this.button4.TabIndex = 5;
      this.button4.Text = "Next >";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // txtBoxSummary
      // 
      this.txtBoxSummary.Location = new System.Drawing.Point(12, 181);
      this.txtBoxSummary.Multiline = true;
      this.txtBoxSummary.Name = "txtBoxSummary";
      this.txtBoxSummary.ReadOnly = true;
      this.txtBoxSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtBoxSummary.Size = new System.Drawing.Size(42, 32);
      this.txtBoxSummary.TabIndex = 6;
      this.txtBoxSummary.Visible = false;
      // 
      // btnTruncate
      // 
      this.btnTruncate.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnTruncate.Location = new System.Drawing.Point(71, 187);
      this.btnTruncate.Name = "btnTruncate";
      this.btnTruncate.Size = new System.Drawing.Size(80, 26);
      this.btnTruncate.TabIndex = 5;
      this.btnTruncate.Text = "Truncate";
      this.btnTruncate.UseVisualStyleBackColor = true;
      this.btnTruncate.Visible = false;
      this.btnTruncate.Click += new System.EventHandler(this.btnTruncate_Click);
      // 
      // button5
      // 
      this.button5.Enabled = false;
      this.button5.Location = new System.Drawing.Point(319, 181);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(101, 23);
      this.button5.TabIndex = 7;
      this.button5.Text = "< Back";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // dlgTruncate
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(524, 249);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.txtBoxSummary);
      this.Controls.Add(this.btnTruncate);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.checkedListBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "dlgTruncate";
      this.ShowInTaskbar = false;
      this.Text = "Truncate Fabric";
      this.TopMost = true;
      this.Load += new System.EventHandler(this.dlgTruncate_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.TextBox txtBoxSummary;
    private System.Windows.Forms.Button btnTruncate;
    private System.Windows.Forms.Button button5;
    protected internal System.Windows.Forms.CheckedListBox checkedListBox1;
  }
}