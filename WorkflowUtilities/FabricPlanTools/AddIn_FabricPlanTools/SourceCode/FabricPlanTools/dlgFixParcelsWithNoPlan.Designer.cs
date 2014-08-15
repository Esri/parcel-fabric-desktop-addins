namespace FabricPlanTools
{
  partial class dlgFixParcelsWithNoPlan
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
      this.btnSelectAll = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnClearAll = new System.Windows.Forms.Button();
      this.radioBtnPlanID = new System.Windows.Forms.RadioButton();
      this.radioBtnUserDef = new System.Windows.Forms.RadioButton();
      this.txtPlanName = new System.Windows.Forms.TextBox();
      this.btnFix = new System.Windows.Forms.Button();
      this.btnNext = new System.Windows.Forms.Button();
      this.btnBack = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnFilterList = new System.Windows.Forms.Button();
      this.txtFilter = new System.Windows.Forms.TextBox();
      this.listPlans = new System.Windows.Forms.ListBox();
      this.radioBtnExistingPlan = new System.Windows.Forms.RadioButton();
      this.lblExisting = new System.Windows.Forms.Label();
      this.lblTarget = new System.Windows.Forms.Label();
      this.button2 = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.lblSelectionCount = new System.Windows.Forms.Label();
      this.chkSelectByGroups = new System.Windows.Forms.CheckBox();
      this.listViewByGroup = new System.Windows.Forms.ListView();
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(310, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Found fabric parcels that are not assigned to a valid plan record.";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 26);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(288, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Select parcels to fix then click Next to choose a target plan:";
      // 
      // btnSelectAll
      // 
      this.btnSelectAll.Location = new System.Drawing.Point(364, 49);
      this.btnSelectAll.Name = "btnSelectAll";
      this.btnSelectAll.Size = new System.Drawing.Size(101, 23);
      this.btnSelectAll.TabIndex = 3;
      this.btnSelectAll.Text = "Select All";
      this.btnSelectAll.UseVisualStyleBackColor = true;
      this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(364, 178);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(101, 23);
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnClearAll
      // 
      this.btnClearAll.Location = new System.Drawing.Point(364, 78);
      this.btnClearAll.Name = "btnClearAll";
      this.btnClearAll.Size = new System.Drawing.Size(101, 23);
      this.btnClearAll.TabIndex = 6;
      this.btnClearAll.Text = "Deselect All";
      this.btnClearAll.UseVisualStyleBackColor = true;
      this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
      // 
      // radioBtnPlanID
      // 
      this.radioBtnPlanID.AutoSize = true;
      this.radioBtnPlanID.Checked = true;
      this.radioBtnPlanID.Location = new System.Drawing.Point(26, 20);
      this.radioBtnPlanID.Name = "radioBtnPlanID";
      this.radioBtnPlanID.Size = new System.Drawing.Size(226, 17);
      this.radioBtnPlanID.TabIndex = 8;
      this.radioBtnPlanID.TabStop = true;
      this.radioBtnPlanID.Text = "named by original Plan ID, example: [5621]";
      this.radioBtnPlanID.UseVisualStyleBackColor = true;
      this.radioBtnPlanID.CheckedChanged += new System.EventHandler(this.radioBtnPlanID_CheckedChanged);
      // 
      // radioBtnUserDef
      // 
      this.radioBtnUserDef.AutoSize = true;
      this.radioBtnUserDef.Location = new System.Drawing.Point(26, 39);
      this.radioBtnUserDef.Name = "radioBtnUserDef";
      this.radioBtnUserDef.Size = new System.Drawing.Size(194, 17);
      this.radioBtnUserDef.TabIndex = 9;
      this.radioBtnUserDef.Text = "single plan, with the following name:";
      this.radioBtnUserDef.UseVisualStyleBackColor = true;
      this.radioBtnUserDef.CheckedChanged += new System.EventHandler(this.radioBtnUserDef_CheckedChanged);
      // 
      // txtPlanName
      // 
      this.txtPlanName.Enabled = false;
      this.txtPlanName.Location = new System.Drawing.Point(47, 61);
      this.txtPlanName.Name = "txtPlanName";
      this.txtPlanName.Size = new System.Drawing.Size(287, 20);
      this.txtPlanName.TabIndex = 10;
      this.txtPlanName.TextChanged += new System.EventHandler(this.txtPlanName_TextChanged);
      // 
      // btnFix
      // 
      this.btnFix.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnFix.Location = new System.Drawing.Point(371, 125);
      this.btnFix.Name = "btnFix";
      this.btnFix.Size = new System.Drawing.Size(101, 23);
      this.btnFix.TabIndex = 11;
      this.btnFix.Text = "Fix";
      this.btnFix.UseVisualStyleBackColor = true;
      this.btnFix.Visible = false;
      // 
      // btnNext
      // 
      this.btnNext.Location = new System.Drawing.Point(364, 236);
      this.btnNext.Name = "btnNext";
      this.btnNext.Size = new System.Drawing.Size(101, 23);
      this.btnNext.TabIndex = 12;
      this.btnNext.Text = "Next >";
      this.btnNext.UseVisualStyleBackColor = true;
      this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
      // 
      // btnBack
      // 
      this.btnBack.Location = new System.Drawing.Point(364, 207);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new System.Drawing.Size(101, 23);
      this.btnBack.TabIndex = 13;
      this.btnBack.Text = "< Back";
      this.btnBack.UseVisualStyleBackColor = true;
      this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnFilterList);
      this.panel1.Controls.Add(this.txtFilter);
      this.panel1.Controls.Add(this.listPlans);
      this.panel1.Controls.Add(this.radioBtnExistingPlan);
      this.panel1.Controls.Add(this.lblExisting);
      this.panel1.Controls.Add(this.lblTarget);
      this.panel1.Controls.Add(this.button2);
      this.panel1.Controls.Add(this.radioBtnUserDef);
      this.panel1.Controls.Add(this.radioBtnPlanID);
      this.panel1.Controls.Add(this.txtPlanName);
      this.panel1.Location = new System.Drawing.Point(15, 27);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(337, 233);
      this.panel1.TabIndex = 14;
      this.panel1.Visible = false;
      // 
      // btnFilterList
      // 
      this.btnFilterList.Enabled = false;
      this.btnFilterList.Location = new System.Drawing.Point(254, 103);
      this.btnFilterList.Name = "btnFilterList";
      this.btnFilterList.Size = new System.Drawing.Size(80, 23);
      this.btnFilterList.TabIndex = 22;
      this.btnFilterList.Text = "Find";
      this.btnFilterList.UseVisualStyleBackColor = true;
      this.btnFilterList.Click += new System.EventHandler(this.btnFilterList_Click);
      // 
      // txtFilter
      // 
      this.txtFilter.Enabled = false;
      this.txtFilter.Location = new System.Drawing.Point(175, 105);
      this.txtFilter.Name = "txtFilter";
      this.txtFilter.Size = new System.Drawing.Size(76, 20);
      this.txtFilter.TabIndex = 21;
      // 
      // listPlans
      // 
      this.listPlans.Enabled = false;
      this.listPlans.FormattingEnabled = true;
      this.listPlans.Location = new System.Drawing.Point(47, 129);
      this.listPlans.Name = "listPlans";
      this.listPlans.ScrollAlwaysVisible = true;
      this.listPlans.Size = new System.Drawing.Size(287, 95);
      this.listPlans.TabIndex = 20;
      this.listPlans.SelectedIndexChanged += new System.EventHandler(this.listPlans_SelectedIndexChanged);
      // 
      // radioBtnExistingPlan
      // 
      this.radioBtnExistingPlan.AutoSize = true;
      this.radioBtnExistingPlan.Location = new System.Drawing.Point(26, 106);
      this.radioBtnExistingPlan.Name = "radioBtnExistingPlan";
      this.radioBtnExistingPlan.Size = new System.Drawing.Size(114, 17);
      this.radioBtnExistingPlan.TabIndex = 17;
      this.radioBtnExistingPlan.TabStop = true;
      this.radioBtnExistingPlan.Text = "Select one from list";
      this.radioBtnExistingPlan.UseVisualStyleBackColor = true;
      this.radioBtnExistingPlan.CheckedChanged += new System.EventHandler(this.radioBtnExistingPlan_CheckedChanged);
      // 
      // lblExisting
      // 
      this.lblExisting.AutoSize = true;
      this.lblExisting.Location = new System.Drawing.Point(4, 90);
      this.lblExisting.Name = "lblExisting";
      this.lblExisting.Size = new System.Drawing.Size(67, 13);
      this.lblExisting.TabIndex = 14;
      this.lblExisting.Text = "Existing Plan";
      // 
      // lblTarget
      // 
      this.lblTarget.AutoSize = true;
      this.lblTarget.Location = new System.Drawing.Point(6, 5);
      this.lblTarget.Name = "lblTarget";
      this.lblTarget.Size = new System.Drawing.Size(64, 13);
      this.lblTarget.TabIndex = 11;
      this.lblTarget.Text = "New Plan(s)";
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(347, 268);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(101, 23);
      this.button2.TabIndex = 13;
      this.button2.Text = "< Back";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.btnBack_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 10);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(300, 13);
      this.label3.TabIndex = 15;
      this.label3.Text = "Choose a target and click Fix to assign the parcels to the plan.";
      this.label3.Visible = false;
      // 
      // listView1
      // 
      this.listView1.CheckBoxes = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.listView1.FullRowSelect = true;
      this.listView1.Location = new System.Drawing.Point(165, 256);
      this.listView1.MultiSelect = false;
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(346, 212);
      this.listView1.TabIndex = 16;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
      this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
      this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Plan ID not found";
      this.columnHeader1.Width = 120;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Parcel name";
      this.columnHeader2.Width = 210;
      // 
      // lblSelectionCount
      // 
      this.lblSelectionCount.AutoSize = true;
      this.lblSelectionCount.Location = new System.Drawing.Point(296, 269);
      this.lblSelectionCount.Name = "lblSelectionCount";
      this.lblSelectionCount.Size = new System.Drawing.Size(104, 13);
      this.lblSelectionCount.TabIndex = 17;
      this.lblSelectionCount.Text = "No parcels selected.";
      // 
      // chkSelectByGroups
      // 
      this.chkSelectByGroups.AutoSize = true;
      this.chkSelectByGroups.Location = new System.Drawing.Point(12, 265);
      this.chkSelectByGroups.Name = "chkSelectByGroups";
      this.chkSelectByGroups.Size = new System.Drawing.Size(147, 17);
      this.chkSelectByGroups.TabIndex = 18;
      this.chkSelectByGroups.Text = "Show grouped by Plan ID";
      this.chkSelectByGroups.UseVisualStyleBackColor = true;
      this.chkSelectByGroups.CheckedChanged += new System.EventHandler(this.chkSelectByGroups_CheckedChanged);
      // 
      // listViewByGroup
      // 
      this.listViewByGroup.CheckBoxes = true;
      this.listViewByGroup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
      this.listViewByGroup.FullRowSelect = true;
      this.listViewByGroup.Location = new System.Drawing.Point(42, 300);
      this.listViewByGroup.MultiSelect = false;
      this.listViewByGroup.Name = "listViewByGroup";
      this.listViewByGroup.Size = new System.Drawing.Size(316, 97);
      this.listViewByGroup.TabIndex = 19;
      this.listViewByGroup.UseCompatibleStateImageBehavior = false;
      this.listViewByGroup.View = System.Windows.Forms.View.Details;
      this.listViewByGroup.Visible = false;
      this.listViewByGroup.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewByGroup_ItemChecked);
      this.listViewByGroup.SelectedIndexChanged += new System.EventHandler(this.listViewByGroup_SelectedIndexChanged);
      this.listViewByGroup.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listViewByGroup_MouseDown);
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Plan ID not found";
      this.columnHeader3.Width = 120;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Parcel selection count";
      this.columnHeader4.Width = 210;
      // 
      // dlgFixParcelsWithNoPlan
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(572, 465);
      this.Controls.Add(this.chkSelectByGroups);
      this.Controls.Add(this.lblSelectionCount);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.btnBack);
      this.Controls.Add(this.btnNext);
      this.Controls.Add(this.btnFix);
      this.Controls.Add(this.btnClearAll);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnSelectAll);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.listViewByGroup);
      this.Controls.Add(this.listView1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "dlgFixParcelsWithNoPlan";
      this.ShowInTaskbar = false;
      this.Text = "Fix Parcels With No Plan";
      this.TopMost = true;
      this.Load += new System.EventHandler(this.dlgFixParcelsWithNoPlan_Load);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnSelectAll;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnClearAll;
    private System.Windows.Forms.Button btnFix;
    private System.Windows.Forms.Button btnNext;
    private System.Windows.Forms.Button btnBack;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblTarget;
    private System.Windows.Forms.Button button2;
    internal System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    internal System.Windows.Forms.Label lblSelectionCount;
    internal System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox chkSelectByGroups;
    internal System.Windows.Forms.ListView listViewByGroup;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.Label lblExisting;
    private System.Windows.Forms.Button btnFilterList;
    private System.Windows.Forms.TextBox txtFilter;
    internal System.Windows.Forms.ListBox listPlans;
    internal System.Windows.Forms.RadioButton radioBtnPlanID;
    internal System.Windows.Forms.RadioButton radioBtnUserDef;
    internal System.Windows.Forms.TextBox txtPlanName;
    internal System.Windows.Forms.RadioButton radioBtnExistingPlan;
  }
}