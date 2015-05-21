namespace DeleteSelectedParcels
{
  partial class dlgEmptyPlansList
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
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.lblSelectionCount = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // checkedListBox1
      // 
      this.checkedListBox1.CausesValidation = false;
      this.checkedListBox1.CheckOnClick = true;
      this.checkedListBox1.FormattingEnabled = true;
      this.checkedListBox1.HorizontalScrollbar = true;
      this.checkedListBox1.Location = new System.Drawing.Point(16, 34);
      this.checkedListBox1.Name = "checkedListBox1";
      this.checkedListBox1.Size = new System.Drawing.Size(291, 169);
      this.checkedListBox1.TabIndex = 0;
      this.checkedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
      this.checkedListBox1.Click += new System.EventHandler(this.checkedListBox1_Click);
      this.checkedListBox1.SelectedIndexChanged += new System.EventHandler(this.checkedListBox1_SelectedIndexChanged);
      this.checkedListBox1.SelectedValueChanged += new System.EventHandler(this.checkedListBox1_SelectedValueChanged);
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Enabled = false;
      this.button1.Location = new System.Drawing.Point(319, 210);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(101, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Delete";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(319, 180);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(101, 23);
      this.button2.TabIndex = 2;
      this.button2.Text = "Cancel";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(319, 34);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(101, 23);
      this.button3.TabIndex = 3;
      this.button3.Text = "Select All";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(319, 63);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(101, 23);
      this.button4.TabIndex = 4;
      this.button4.Text = "Deselect All";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 10);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(262, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Select empty plans from the list, and then click Delete:";
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // lblSelectionCount
      // 
      this.lblSelectionCount.AutoSize = true;
      this.lblSelectionCount.Location = new System.Drawing.Point(13, 215);
      this.lblSelectionCount.Name = "lblSelectionCount";
      this.lblSelectionCount.Size = new System.Drawing.Size(113, 13);
      this.lblSelectionCount.TabIndex = 6;
      this.lblSelectionCount.Text = "No empty plans found.";
      this.lblSelectionCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // dlgEmptyPlansList
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(436, 249);
      this.Controls.Add(this.lblSelectionCount);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.checkedListBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "dlgEmptyPlansList";
      this.Text = "Delete Empty Plans";
      this.TopMost = true;
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    public System.Windows.Forms.CheckedListBox checkedListBox1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblSelectionCount;
  }
}