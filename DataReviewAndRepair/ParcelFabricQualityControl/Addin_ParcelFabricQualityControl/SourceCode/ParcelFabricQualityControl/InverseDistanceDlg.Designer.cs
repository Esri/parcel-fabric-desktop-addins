﻿namespace ParcelFabricQualityControl
{
  partial class InverseDistanceDlg
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InverseDistanceDlg));
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.lblDistanceUnits1 = new System.Windows.Forms.Label();
      this.lblRecalc = new System.Windows.Forms.Label();
      this.button3 = new System.Windows.Forms.Button();
      this.txtDistDifference = new System.Windows.Forms.TextBox();
      this.txtHeightParameter = new System.Windows.Forms.TextBox();
      this.chkApplyScaleFactor = new System.Windows.Forms.CheckBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.cboUnits = new System.Windows.Forms.ComboBox();
      this.btnChange = new System.Windows.Forms.Button();
      this.txtServiceURL = new System.Windows.Forms.TextBox();
      this.cboScaleMethod = new System.Windows.Forms.ComboBox();
      this.txtScaleFactor = new System.Windows.Forms.TextBox();
      this.optComputeForMe = new System.Windows.Forms.RadioButton();
      this.optUserEnteredScaleFactor = new System.Windows.Forms.RadioButton();
      this.chkDistanceDifference = new System.Windows.Forms.CheckBox();
      this.chkReportResults = new System.Windows.Forms.CheckBox();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.btnGetScaleFromEditor = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      resources.ApplyResources(this.button1, "button1");
      this.button1.Name = "button1";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      resources.ApplyResources(this.button2, "button2");
      this.button2.Name = "button2";
      this.button2.UseVisualStyleBackColor = true;
      // 
      // lblDistanceUnits1
      // 
      resources.ApplyResources(this.lblDistanceUnits1, "lblDistanceUnits1");
      this.lblDistanceUnits1.Name = "lblDistanceUnits1";
      // 
      // lblRecalc
      // 
      resources.ApplyResources(this.lblRecalc, "lblRecalc");
      this.lblRecalc.Name = "lblRecalc";
      // 
      // button3
      // 
      resources.ApplyResources(this.button3, "button3");
      this.button3.Name = "button3";
      this.button3.UseVisualStyleBackColor = true;
      // 
      // txtDistDifference
      // 
      resources.ApplyResources(this.txtDistDifference, "txtDistDifference");
      this.txtDistDifference.Name = "txtDistDifference";
      this.txtDistDifference.Tag = "High numbers will keep more of the existing attribute lengths on lines unchanged." +
    "/Low numbers will be more likely to inverse lines (0.00 will inverse all lines.)" +
    "";
      this.txtDistDifference.TextChanged += new System.EventHandler(this.txtDistDifferance_TextChanged);
      this.txtDistDifference.MouseHover += new System.EventHandler(this.txtDistDifferance_MouseHover);
      // 
      // txtHeightParameter
      // 
      resources.ApplyResources(this.txtHeightParameter, "txtHeightParameter");
      this.txtHeightParameter.Name = "txtHeightParameter";
      // 
      // chkApplyScaleFactor
      // 
      resources.ApplyResources(this.chkApplyScaleFactor, "chkApplyScaleFactor");
      this.chkApplyScaleFactor.Checked = true;
      this.chkApplyScaleFactor.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkApplyScaleFactor.Name = "chkApplyScaleFactor";
      this.chkApplyScaleFactor.UseVisualStyleBackColor = true;
      this.chkApplyScaleFactor.CheckedChanged += new System.EventHandler(this.chkApplyScaleFactor_CheckedChanged);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnGetScaleFromEditor);
      this.panel1.Controls.Add(this.cboUnits);
      this.panel1.Controls.Add(this.btnChange);
      this.panel1.Controls.Add(this.txtServiceURL);
      this.panel1.Controls.Add(this.cboScaleMethod);
      this.panel1.Controls.Add(this.txtScaleFactor);
      this.panel1.Controls.Add(this.optComputeForMe);
      this.panel1.Controls.Add(this.optUserEnteredScaleFactor);
      this.panel1.Controls.Add(this.txtHeightParameter);
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Name = "panel1";
      // 
      // cboUnits
      // 
      this.cboUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      resources.ApplyResources(this.cboUnits, "cboUnits");
      this.cboUnits.FormattingEnabled = true;
      this.cboUnits.Items.AddRange(new object[] {
            resources.GetString("cboUnits.Items"),
            resources.GetString("cboUnits.Items1")});
      this.cboUnits.Name = "cboUnits";
      // 
      // btnChange
      // 
      resources.ApplyResources(this.btnChange, "btnChange");
      this.btnChange.Name = "btnChange";
      this.btnChange.UseVisualStyleBackColor = true;
      this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
      // 
      // txtServiceURL
      // 
      resources.ApplyResources(this.txtServiceURL, "txtServiceURL");
      this.txtServiceURL.Name = "txtServiceURL";
      this.txtServiceURL.ReadOnly = true;
      this.txtServiceURL.Tag = "http://elevation.arcgis.com/arcgis/services/WorldElevation/Terrain/ImageServer";
      this.txtServiceURL.MouseHover += new System.EventHandler(this.txtServiceURL_MouseHover);
      // 
      // cboScaleMethod
      // 
      this.cboScaleMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      resources.ApplyResources(this.cboScaleMethod, "cboScaleMethod");
      this.cboScaleMethod.FormattingEnabled = true;
      this.cboScaleMethod.Items.AddRange(new object[] {
            resources.GetString("cboScaleMethod.Items"),
            resources.GetString("cboScaleMethod.Items1")});
      this.cboScaleMethod.Name = "cboScaleMethod";
      this.cboScaleMethod.SelectedIndexChanged += new System.EventHandler(this.cboScaleMethod_SelectedIndexChanged);
      // 
      // txtScaleFactor
      // 
      resources.ApplyResources(this.txtScaleFactor, "txtScaleFactor");
      this.txtScaleFactor.Name = "txtScaleFactor";
      // 
      // optComputeForMe
      // 
      resources.ApplyResources(this.optComputeForMe, "optComputeForMe");
      this.optComputeForMe.Name = "optComputeForMe";
      this.optComputeForMe.UseVisualStyleBackColor = true;
      this.optComputeForMe.CheckedChanged += new System.EventHandler(this.optComputeForMe_CheckedChanged);
      // 
      // optUserEnteredScaleFactor
      // 
      resources.ApplyResources(this.optUserEnteredScaleFactor, "optUserEnteredScaleFactor");
      this.optUserEnteredScaleFactor.Checked = true;
      this.optUserEnteredScaleFactor.Name = "optUserEnteredScaleFactor";
      this.optUserEnteredScaleFactor.TabStop = true;
      this.optUserEnteredScaleFactor.UseVisualStyleBackColor = true;
      this.optUserEnteredScaleFactor.CheckedChanged += new System.EventHandler(this.optUserEnteredScaleFactor_CheckedChanged);
      // 
      // chkDistanceDifference
      // 
      resources.ApplyResources(this.chkDistanceDifference, "chkDistanceDifference");
      this.chkDistanceDifference.Checked = true;
      this.chkDistanceDifference.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkDistanceDifference.Name = "chkDistanceDifference";
      this.chkDistanceDifference.UseVisualStyleBackColor = true;
      this.chkDistanceDifference.CheckedChanged += new System.EventHandler(this.chkDistanceDifference_CheckedChanged);
      // 
      // chkReportResults
      // 
      resources.ApplyResources(this.chkReportResults, "chkReportResults");
      this.chkReportResults.Checked = true;
      this.chkReportResults.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkReportResults.Name = "chkReportResults";
      this.chkReportResults.UseVisualStyleBackColor = true;
      // 
      // textBox1
      // 
      resources.ApplyResources(this.textBox1, "textBox1");
      this.textBox1.Name = "textBox1";
      // 
      // btnGetScaleFromEditor
      // 
      resources.ApplyResources(this.btnGetScaleFromEditor, "btnGetScaleFromEditor");
      this.btnGetScaleFromEditor.Name = "btnGetScaleFromEditor";
      this.btnGetScaleFromEditor.Tag = "Get the scale from the Editing options";
      this.btnGetScaleFromEditor.UseVisualStyleBackColor = true;
      this.btnGetScaleFromEditor.Click += new System.EventHandler(this.btnGetOffsetFromEditor_Click);
      this.btnGetScaleFromEditor.MouseHover += new System.EventHandler(this.btnGetScaleFromEditor_MouseHover);
      // 
      // InverseDistanceDlg
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.chkReportResults);
      this.Controls.Add(this.chkDistanceDifference);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.chkApplyScaleFactor);
      this.Controls.Add(this.txtDistDifference);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.lblRecalc);
      this.Controls.Add(this.lblDistanceUnits1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.HelpButton = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "InverseDistanceDlg";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Load += new System.EventHandler(this.InverseDistanceDlg_Load);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    internal System.Windows.Forms.Label lblDistanceUnits1;
    private System.Windows.Forms.Label lblRecalc;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnChange;
    internal System.Windows.Forms.TextBox txtDistDifference;
    internal System.Windows.Forms.TextBox txtHeightParameter;
    internal System.Windows.Forms.TextBox txtScaleFactor;
    internal System.Windows.Forms.CheckBox chkApplyScaleFactor;
    internal System.Windows.Forms.TextBox txtServiceURL;
    internal System.Windows.Forms.ComboBox cboScaleMethod;
    internal System.Windows.Forms.RadioButton optComputeForMe;
    internal System.Windows.Forms.RadioButton optUserEnteredScaleFactor;
    internal System.Windows.Forms.CheckBox chkDistanceDifference;
    internal System.Windows.Forms.CheckBox chkReportResults;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Button btnGetScaleFromEditor;
    internal System.Windows.Forms.ComboBox cboUnits;
  }
}