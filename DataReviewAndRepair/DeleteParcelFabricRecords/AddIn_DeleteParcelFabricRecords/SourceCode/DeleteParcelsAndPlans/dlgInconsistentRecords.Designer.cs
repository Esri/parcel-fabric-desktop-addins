namespace DeleteSelectedParcels
{
  partial class dlgInconsistentRecords
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(dlgInconsistentRecords));
      this.tvOrphanRecs = new System.Windows.Forms.TreeView();
      this.label1 = new System.Windows.Forms.Label();
      this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
      this.btnSelectAll = new System.Windows.Forms.Button();
      this.btnDeselectAll = new System.Windows.Forms.Button();
      this.btnDelete = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnBack = new System.Windows.Forms.Button();
      this.btnNext = new System.Windows.Forms.Button();
      this.btnSaveReport = new System.Windows.Forms.Button();
      this.txtInClauseReport = new System.Windows.Forms.TextBox();
      this.btnSaveReport2 = new System.Windows.Forms.Button();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.SuspendLayout();
      // 
      // tvOrphanRecs
      // 
      this.tvOrphanRecs.ItemHeight = 19;
      this.tvOrphanRecs.Location = new System.Drawing.Point(28, 181);
      this.tvOrphanRecs.Name = "tvOrphanRecs";
      this.tvOrphanRecs.Size = new System.Drawing.Size(61, 34);
      this.tvOrphanRecs.TabIndex = 0;
      this.tvOrphanRecs.Visible = false;
      this.tvOrphanRecs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvOrphanRecs_AfterSelect);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 10);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(298, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Full scan done. Choose the type of records to report or delete:";
      // 
      // checkedListBox1
      // 
      this.checkedListBox1.CheckOnClick = true;
      this.checkedListBox1.FormattingEnabled = true;
      this.checkedListBox1.Location = new System.Drawing.Point(16, 34);
      this.checkedListBox1.Name = "checkedListBox1";
      this.checkedListBox1.Size = new System.Drawing.Size(291, 199);
      this.checkedListBox1.TabIndex = 2;
      this.checkedListBox1.SelectedValueChanged += new System.EventHandler(this.checkedListBox1_SelectedValueChanged);
      // 
      // btnSelectAll
      // 
      this.btnSelectAll.Location = new System.Drawing.Point(319, 34);
      this.btnSelectAll.Name = "btnSelectAll";
      this.btnSelectAll.Size = new System.Drawing.Size(101, 23);
      this.btnSelectAll.TabIndex = 3;
      this.btnSelectAll.Text = "Select All";
      this.btnSelectAll.UseVisualStyleBackColor = true;
      this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
      // 
      // btnDeselectAll
      // 
      this.btnDeselectAll.Location = new System.Drawing.Point(319, 63);
      this.btnDeselectAll.Name = "btnDeselectAll";
      this.btnDeselectAll.Size = new System.Drawing.Size(101, 23);
      this.btnDeselectAll.TabIndex = 4;
      this.btnDeselectAll.Text = "Deselect All";
      this.btnDeselectAll.UseVisualStyleBackColor = true;
      this.btnDeselectAll.Click += new System.EventHandler(this.btnDeselectAll_Click);
      // 
      // btnDelete
      // 
      this.btnDelete.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnDelete.Location = new System.Drawing.Point(95, 181);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(101, 23);
      this.btnDelete.TabIndex = 5;
      this.btnDelete.Text = "Delete";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Visible = false;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(319, 152);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(101, 23);
      this.btnCancel.TabIndex = 6;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnBack
      // 
      this.btnBack.Enabled = false;
      this.btnBack.Location = new System.Drawing.Point(319, 181);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new System.Drawing.Size(101, 23);
      this.btnBack.TabIndex = 7;
      this.btnBack.Text = "< Back";
      this.btnBack.UseVisualStyleBackColor = true;
      this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
      // 
      // btnNext
      // 
      this.btnNext.Location = new System.Drawing.Point(319, 210);
      this.btnNext.Name = "btnNext";
      this.btnNext.Size = new System.Drawing.Size(101, 23);
      this.btnNext.TabIndex = 8;
      this.btnNext.Text = "Next >";
      this.btnNext.UseVisualStyleBackColor = true;
      this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
      // 
      // btnSaveReport
      // 
      this.btnSaveReport.Location = new System.Drawing.Point(95, 137);
      this.btnSaveReport.Name = "btnSaveReport";
      this.btnSaveReport.Size = new System.Drawing.Size(101, 23);
      this.btnSaveReport.TabIndex = 9;
      this.btnSaveReport.Text = "Save Report...";
      this.btnSaveReport.UseVisualStyleBackColor = true;
      this.btnSaveReport.Visible = false;
      this.btnSaveReport.Click += new System.EventHandler(this.btnSaveReport_Click);
      // 
      // txtInClauseReport
      // 
      this.txtInClauseReport.Location = new System.Drawing.Point(195, 214);
      this.txtInClauseReport.Multiline = true;
      this.txtInClauseReport.Name = "txtInClauseReport";
      this.txtInClauseReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtInClauseReport.Size = new System.Drawing.Size(71, 39);
      this.txtInClauseReport.TabIndex = 10;
      this.txtInClauseReport.Visible = false;
      this.txtInClauseReport.MouseEnter += new System.EventHandler(this.txtInClauseReport_MouseEnter);
      // 
      // btnSaveReport2
      // 
      this.btnSaveReport2.Location = new System.Drawing.Point(12, 239);
      this.btnSaveReport2.Name = "btnSaveReport2";
      this.btnSaveReport2.Size = new System.Drawing.Size(101, 23);
      this.btnSaveReport2.TabIndex = 11;
      this.btnSaveReport2.Text = "Save Report...";
      this.btnSaveReport2.UseVisualStyleBackColor = true;
      this.btnSaveReport2.Visible = false;
      this.btnSaveReport2.Click += new System.EventHandler(this.btnSaveReport2_Click);
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "InconsistenciesOK.png");
      this.imageList1.Images.SetKeyName(1, "InconsistencyWarning.png");
      this.imageList1.Images.SetKeyName(2, "InconsistencyError.png");
      this.imageList1.Images.SetKeyName(3, "InconsistencyInfo.png");
      // 
      // dlgInconsistentRecords
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(435, 266);
      this.Controls.Add(this.btnSaveReport2);
      this.Controls.Add(this.txtInClauseReport);
      this.Controls.Add(this.btnSaveReport);
      this.Controls.Add(this.btnNext);
      this.Controls.Add(this.btnBack);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnDelete);
      this.Controls.Add(this.tvOrphanRecs);
      this.Controls.Add(this.btnDeselectAll);
      this.Controls.Add(this.btnSelectAll);
      this.Controls.Add(this.checkedListBox1);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "dlgInconsistentRecords";
      this.ShowInTaskbar = false;
      this.Text = "Full Fabric Scan";
      this.TopMost = true;
      this.Load += new System.EventHandler(this.dlgInconsistentRecords_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    internal System.Windows.Forms.CheckedListBox checkedListBox1;
    internal System.Windows.Forms.Button btnSaveReport;
    internal System.Windows.Forms.Button btnSelectAll;
    internal System.Windows.Forms.Button btnDelete;
    internal System.Windows.Forms.Button btnNext;
    internal System.Windows.Forms.Button btnDeselectAll;
    internal System.Windows.Forms.Button btnBack;
    internal System.Windows.Forms.TextBox txtInClauseReport;
    internal System.Windows.Forms.TreeView tvOrphanRecs;
    internal System.Windows.Forms.Button btnCancel;
    internal System.Windows.Forms.Label label1;
    internal System.Windows.Forms.Button btnSaveReport2;
    private System.Windows.Forms.ImageList imageList1;
  }
}