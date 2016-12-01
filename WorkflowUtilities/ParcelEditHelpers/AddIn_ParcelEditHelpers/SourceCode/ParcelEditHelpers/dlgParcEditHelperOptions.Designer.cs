namespace ParcelEditHelper
{
  partial class dlgParcEditHelperOptions
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
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.chkCopyDirection = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.btnChkField = new System.Windows.Forms.Button();
      this.txtFldName = new System.Windows.Forms.TextBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(12, 12);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(555, 444);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.chkCopyDirection);
      this.tabPage1.Controls.Add(this.label2);
      this.tabPage1.Controls.Add(this.btnChkField);
      this.tabPage1.Controls.Add(this.txtFldName);
      this.tabPage1.Location = new System.Drawing.Point(4, 25);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(547, 415);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Lines Grid";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // chkCopyDirection
      // 
      this.chkCopyDirection.AutoSize = true;
      this.chkCopyDirection.Location = new System.Drawing.Point(6, 6);
      this.chkCopyDirection.Name = "chkCopyDirection";
      this.chkCopyDirection.Size = new System.Drawing.Size(384, 21);
      this.chkCopyDirection.TabIndex = 6;
      this.chkCopyDirection.Text = "Copy entered directions to a field in the fabric lines table";
      this.chkCopyDirection.UseVisualStyleBackColor = true;
      this.chkCopyDirection.CheckedChanged += new System.EventHandler(this.chkCopyDirection_CheckedChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(30, 35);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(81, 17);
      this.label2.TabIndex = 5;
      this.label2.Text = "Field name:";
      // 
      // btnChkField
      // 
      this.btnChkField.Location = new System.Drawing.Point(297, 29);
      this.btnChkField.Name = "btnChkField";
      this.btnChkField.Size = new System.Drawing.Size(91, 28);
      this.btnChkField.TabIndex = 4;
      this.btnChkField.Text = "Check...";
      this.btnChkField.UseVisualStyleBackColor = true;
      this.btnChkField.Click += new System.EventHandler(this.btnChkField_Click);
      // 
      // txtFldName
      // 
      this.txtFldName.Location = new System.Drawing.Point(117, 32);
      this.txtFldName.Name = "txtFldName";
      this.txtFldName.Size = new System.Drawing.Size(174, 22);
      this.txtFldName.TabIndex = 3;
      this.txtFldName.TextChanged += new System.EventHandler(this.txtFldName_TextChanged);
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(251, 466);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(100, 28);
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(357, 466);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(100, 28);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnApply
      // 
      this.btnApply.Location = new System.Drawing.Point(463, 466);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(100, 28);
      this.btnApply.TabIndex = 3;
      this.btnApply.Text = "Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // dlgParcEditHelperOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(579, 506);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.tabControl1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.HelpButton = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "dlgParcEditHelperOptions";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Options";
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnChkField;
    private System.Windows.Forms.TextBox txtFldName;
    private System.Windows.Forms.Button btnApply;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox chkCopyDirection;
  }
}