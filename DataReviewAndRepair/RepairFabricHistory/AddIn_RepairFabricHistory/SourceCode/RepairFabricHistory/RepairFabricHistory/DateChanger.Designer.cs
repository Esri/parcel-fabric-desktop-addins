namespace RepairFabricHistory
{
  partial class DateChanger
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
      this.label3 = new System.Windows.Forms.Label();
      this.cboBoxFields = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.button2 = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
      this.cboBoxFabricClasses = new System.Windows.Forms.ComboBox();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.SuspendLayout();
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(11, 59);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(152, 13);
      this.label3.TabIndex = 15;
      this.label3.Text = "Choose a date field to change:";
      // 
      // cboBoxFields
      // 
      this.cboBoxFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboBoxFields.FormattingEnabled = true;
      this.cboBoxFields.Location = new System.Drawing.Point(11, 76);
      this.cboBoxFields.Name = "cboBoxFields";
      this.cboBoxFields.Size = new System.Drawing.Size(303, 21);
      this.cboBoxFields.TabIndex = 14;
      this.cboBoxFields.SelectedIndexChanged += new System.EventHandler(this.cboBoxFields_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(11, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(118, 13);
      this.label1.TabIndex = 12;
      this.label1.Text = "Choose the fabric layer:";
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(320, 183);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(101, 23);
      this.button2.TabIndex = 11;
      this.button2.Text = "Cancel";
      this.button2.UseVisualStyleBackColor = true;
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Location = new System.Drawing.Point(320, 212);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(101, 23);
      this.button1.TabIndex = 10;
      this.button1.Text = "Change Date";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // dateTimePicker1
      // 
      this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dateTimePicker1.Location = new System.Drawing.Point(222, 127);
      this.dateTimePicker1.Name = "dateTimePicker1";
      this.dateTimePicker1.Size = new System.Drawing.Size(92, 20);
      this.dateTimePicker1.TabIndex = 9;
      // 
      // cboBoxFabricClasses
      // 
      this.cboBoxFabricClasses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboBoxFabricClasses.FormattingEnabled = true;
      this.cboBoxFabricClasses.Location = new System.Drawing.Point(14, 26);
      this.cboBoxFabricClasses.Name = "cboBoxFabricClasses";
      this.cboBoxFabricClasses.Size = new System.Drawing.Size(300, 21);
      this.cboBoxFabricClasses.TabIndex = 8;
      this.cboBoxFabricClasses.SelectedIndexChanged += new System.EventHandler(this.cboBoxFabricClasses_SelectedIndexChanged);
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.Checked = true;
      this.radioButton1.Location = new System.Drawing.Point(11, 108);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(237, 17);
      this.radioButton1.TabIndex = 16;
      this.radioButton1.TabStop = true;
      this.radioButton1.Text = "Choose a new date for the selected features:";
      this.radioButton1.UseVisualStyleBackColor = true;
      this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.Location = new System.Drawing.Point(11, 157);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(212, 17);
      this.radioButton2.TabIndex = 17;
      this.radioButton2.Text = "Clear the date field for selected features";
      this.radioButton2.UseVisualStyleBackColor = true;
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // DateChanger
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(432, 245);
      this.Controls.Add(this.radioButton2);
      this.Controls.Add(this.radioButton1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.cboBoxFields);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.dateTimePicker1);
      this.Controls.Add(this.cboBoxFabricClasses);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DateChanger";
      this.ShowInTaskbar = false;
      this.Text = "Change Date By Layer";
      this.Load += new System.EventHandler(this.DateChanger_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label3;
    internal System.Windows.Forms.ComboBox cboBoxFields;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button1;
    internal System.Windows.Forms.DateTimePicker dateTimePicker1;
    internal System.Windows.Forms.ComboBox cboBoxFabricClasses;
    internal System.Windows.Forms.RadioButton radioButton1;
    internal System.Windows.Forms.RadioButton radioButton2;
  }
}