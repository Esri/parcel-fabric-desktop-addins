namespace ParcelEditHelper
{
  partial class dlgTraverseResults
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
      this.btnAdjust = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.panel2 = new System.Windows.Forms.Panel();
      this.txtMiscloseReport = new System.Windows.Forms.TextBox();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnAdjust
      // 
      this.btnAdjust.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnAdjust.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnAdjust.Location = new System.Drawing.Point(621, 0);
      this.btnAdjust.Name = "btnAdjust";
      this.btnAdjust.Size = new System.Drawing.Size(117, 32);
      this.btnAdjust.TabIndex = 0;
      this.btnAdjust.Text = "Accept";
      this.btnAdjust.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnCancel.Location = new System.Drawing.Point(504, 0);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(117, 32);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnAdjust);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 400);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(738, 32);
      this.panel1.TabIndex = 2;
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AllowUserToResizeRows = false;
      this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
      this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Left;
      this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
      this.dataGridView1.EnableHeadersVisualStyles = false;
      this.dataGridView1.GridColor = System.Drawing.SystemColors.ControlLight;
      this.dataGridView1.Location = new System.Drawing.Point(0, 0);
      this.dataGridView1.MultiSelect = false;
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.ReadOnly = true;
      this.dataGridView1.RowHeadersVisible = false;
      this.dataGridView1.RowTemplate.Height = 24;
      this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView1.Size = new System.Drawing.Size(505, 400);
      this.dataGridView1.TabIndex = 3;
      this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
      // 
      // Column1
      // 
      this.Column1.HeaderText = "#";
      this.Column1.MaxInputLength = 200;
      this.Column1.Name = "Column1";
      this.Column1.ReadOnly = true;
      this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.Column1.Width = 60;
      // 
      // Column2
      // 
      this.Column2.HeaderText = "Description";
      this.Column2.MaxInputLength = 475;
      this.Column2.Name = "Column2";
      this.Column2.ReadOnly = true;
      this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.Column2.Width = 155;
      // 
      // Column3
      // 
      this.Column3.HeaderText = "Computed Values";
      this.Column3.MaxInputLength = 475;
      this.Column3.Name = "Column3";
      this.Column3.ReadOnly = true;
      this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.Column3.Width = 155;
      // 
      // Column4
      // 
      this.Column4.HeaderText = "Residual Values";
      this.Column4.MaxInputLength = 400;
      this.Column4.Name = "Column4";
      this.Column4.ReadOnly = true;
      this.Column4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.Column4.Width = 130;
      // 
      // panel2
      // 
      this.panel2.AutoScroll = true;
      this.panel2.AutoSize = true;
      this.panel2.Controls.Add(this.txtMiscloseReport);
      this.panel2.Controls.Add(this.dataGridView1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(738, 400);
      this.panel2.TabIndex = 5;
      // 
      // txtMiscloseReport
      // 
      this.txtMiscloseReport.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtMiscloseReport.Location = new System.Drawing.Point(505, 0);
      this.txtMiscloseReport.Multiline = true;
      this.txtMiscloseReport.Name = "txtMiscloseReport";
      this.txtMiscloseReport.Size = new System.Drawing.Size(233, 400);
      this.txtMiscloseReport.TabIndex = 4;
      // 
      // dlgTraverseResults
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(738, 432);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "dlgTraverseResults";
      this.Text = "Traverse Results";
      this.panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnAdjust;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Panel panel1;
    internal System.Windows.Forms.DataGridView dataGridView1;
    private System.Windows.Forms.Panel panel2;
    internal System.Windows.Forms.TextBox txtMiscloseReport;
    private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
    private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
    private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
  }
}