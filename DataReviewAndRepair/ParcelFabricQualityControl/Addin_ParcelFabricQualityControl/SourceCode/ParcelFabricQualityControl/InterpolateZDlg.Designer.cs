namespace ParcelFabricQualityControl
{
  partial class InterpolateZDlg
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InterpolateZDlg));
      this.label1 = new System.Windows.Forms.Label();
      this.optClearElevations = new System.Windows.Forms.RadioButton();
      this.optAssignZValues = new System.Windows.Forms.RadioButton();
      this.cboElevationSource = new System.Windows.Forms.ComboBox();
      this.cboUnits = new System.Windows.Forms.ComboBox();
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.chkReportResults = new System.Windows.Forms.CheckBox();
      this.txtHeightParameter = new System.Windows.Forms.TextBox();
      this.txtElevationLyr = new System.Windows.Forms.TextBox();
      this.lblHeightInput = new System.Windows.Forms.Label();
      this.btnUnits = new System.Windows.Forms.Button();
      this.cboLayerNameTIN = new System.Windows.Forms.ComboBox();
      this.btnChange = new System.Windows.Forms.Button();
      this.cboLayerNameDEM = new System.Windows.Forms.ComboBox();
      this.chkElevationDifference = new System.Windows.Forms.CheckBox();
      this.txtElevationDifference = new System.Windows.Forms.TextBox();
      this.lblElevationUnits = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      resources.ApplyResources(this.label1, "label1");
      this.label1.Name = "label1";
      // 
      // optClearElevations
      // 
      resources.ApplyResources(this.optClearElevations, "optClearElevations");
      this.optClearElevations.Name = "optClearElevations";
      this.optClearElevations.TabStop = true;
      this.optClearElevations.UseVisualStyleBackColor = true;
      this.optClearElevations.CheckedChanged += new System.EventHandler(this.optClearElevations_CheckedChanged);
      // 
      // optAssignZValues
      // 
      resources.ApplyResources(this.optAssignZValues, "optAssignZValues");
      this.optAssignZValues.Name = "optAssignZValues";
      this.optAssignZValues.TabStop = true;
      this.optAssignZValues.UseVisualStyleBackColor = true;
      this.optAssignZValues.CheckedChanged += new System.EventHandler(this.optAssignZValues_CheckedChanged);
      // 
      // cboElevationSource
      // 
      this.cboElevationSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboElevationSource.FormattingEnabled = true;
      this.cboElevationSource.Items.AddRange(new object[] {
            resources.GetString("cboElevationSource.Items"),
            resources.GetString("cboElevationSource.Items1"),
            resources.GetString("cboElevationSource.Items2")});
      resources.ApplyResources(this.cboElevationSource, "cboElevationSource");
      this.cboElevationSource.Name = "cboElevationSource";
      this.cboElevationSource.SelectedIndexChanged += new System.EventHandler(this.cboElevationSource_SelectedIndexChanged);
      // 
      // cboUnits
      // 
      this.cboUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboUnits.FormattingEnabled = true;
      this.cboUnits.Items.AddRange(new object[] {
            resources.GetString("cboUnits.Items"),
            resources.GetString("cboUnits.Items1")});
      resources.ApplyResources(this.cboUnits, "cboUnits");
      this.cboUnits.Name = "cboUnits";
      this.cboUnits.SelectedIndexChanged += new System.EventHandler(this.cboUnits_SelectedIndexChanged);
      this.cboUnits.DropDownClosed += new System.EventHandler(this.cboUnits_DropDownClosed);
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
      // chkReportResults
      // 
      resources.ApplyResources(this.chkReportResults, "chkReportResults");
      this.chkReportResults.Name = "chkReportResults";
      this.chkReportResults.UseVisualStyleBackColor = true;
      // 
      // txtHeightParameter
      // 
      resources.ApplyResources(this.txtHeightParameter, "txtHeightParameter");
      this.txtHeightParameter.Name = "txtHeightParameter";
      this.txtHeightParameter.TextChanged += new System.EventHandler(this.txtHeightParameter_TextChanged);
      this.txtHeightParameter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtHeightParameter_KeyDown);
      this.txtHeightParameter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtHeightParameter_KeyPress);
      // 
      // txtElevationLyr
      // 
      resources.ApplyResources(this.txtElevationLyr, "txtElevationLyr");
      this.txtElevationLyr.Name = "txtElevationLyr";
      this.txtElevationLyr.ReadOnly = true;
      this.txtElevationLyr.Tag = "";
      // 
      // lblHeightInput
      // 
      resources.ApplyResources(this.lblHeightInput, "lblHeightInput");
      this.lblHeightInput.Name = "lblHeightInput";
      // 
      // btnUnits
      // 
      resources.ApplyResources(this.btnUnits, "btnUnits");
      this.btnUnits.Name = "btnUnits";
      this.btnUnits.UseVisualStyleBackColor = true;
      this.btnUnits.Click += new System.EventHandler(this.btnUnits_Click);
      this.btnUnits.MouseHover += new System.EventHandler(this.btnUnits_MouseHover);
      // 
      // cboLayerNameTIN
      // 
      this.cboLayerNameTIN.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboLayerNameTIN.FormattingEnabled = true;
      resources.ApplyResources(this.cboLayerNameTIN, "cboLayerNameTIN");
      this.cboLayerNameTIN.Name = "cboLayerNameTIN";
      this.cboLayerNameTIN.SelectedIndexChanged += new System.EventHandler(this.cboLayerNameTIN_SelectedIndexChanged);
      this.cboLayerNameTIN.DropDownClosed += new System.EventHandler(this.cboLayerNameTIN_DropDownClosed);
      this.cboLayerNameTIN.MouseHover += new System.EventHandler(this.cboLayerNameTIN_MouseHover);
      // 
      // btnChange
      // 
      resources.ApplyResources(this.btnChange, "btnChange");
      this.btnChange.Name = "btnChange";
      this.btnChange.UseVisualStyleBackColor = true;
      this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
      // 
      // cboLayerNameDEM
      // 
      this.cboLayerNameDEM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboLayerNameDEM.FormattingEnabled = true;
      resources.ApplyResources(this.cboLayerNameDEM, "cboLayerNameDEM");
      this.cboLayerNameDEM.Name = "cboLayerNameDEM";
      this.cboLayerNameDEM.SelectedIndexChanged += new System.EventHandler(this.cboLayerNameDEM_SelectedIndexChanged);
      this.cboLayerNameDEM.DropDownClosed += new System.EventHandler(this.cboLayerNameDEM_DropDownClosed);
      this.cboLayerNameDEM.MouseHover += new System.EventHandler(this.cboLayerNameDEM_MouseHover);
      // 
      // chkElevationDifference
      // 
      resources.ApplyResources(this.chkElevationDifference, "chkElevationDifference");
      this.chkElevationDifference.Name = "chkElevationDifference";
      this.chkElevationDifference.UseVisualStyleBackColor = true;
      this.chkElevationDifference.CheckedChanged += new System.EventHandler(this.chkElevationDifference_CheckedChanged);
      // 
      // txtElevationDifference
      // 
      resources.ApplyResources(this.txtElevationDifference, "txtElevationDifference");
      this.txtElevationDifference.Name = "txtElevationDifference";
      this.txtElevationDifference.TextChanged += new System.EventHandler(this.txtElevationDifference_TextChanged);
      this.txtElevationDifference.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtElevationDifference_KeyDown);
      this.txtElevationDifference.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtElevationDifference_KeyPress);
      // 
      // lblElevationUnits
      // 
      resources.ApplyResources(this.lblElevationUnits, "lblElevationUnits");
      this.lblElevationUnits.Name = "lblElevationUnits";
      // 
      // InterpolateZDlg
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.lblElevationUnits);
      this.Controls.Add(this.txtElevationDifference);
      this.Controls.Add(this.chkElevationDifference);
      this.Controls.Add(this.cboLayerNameDEM);
      this.Controls.Add(this.btnChange);
      this.Controls.Add(this.cboLayerNameTIN);
      this.Controls.Add(this.btnUnits);
      this.Controls.Add(this.lblHeightInput);
      this.Controls.Add(this.txtElevationLyr);
      this.Controls.Add(this.txtHeightParameter);
      this.Controls.Add(this.chkReportResults);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.cboUnits);
      this.Controls.Add(this.cboElevationSource);
      this.Controls.Add(this.optAssignZValues);
      this.Controls.Add(this.optClearElevations);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.HelpButton = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "InterpolateZDlg";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Load += new System.EventHandler(this.InterpolateZDlg_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    internal System.Windows.Forms.ComboBox cboElevationSource;
    internal System.Windows.Forms.ComboBox cboUnits;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    internal System.Windows.Forms.TextBox txtHeightParameter;
    internal System.Windows.Forms.TextBox txtElevationLyr;
    private System.Windows.Forms.Label lblHeightInput;
    private System.Windows.Forms.Button btnUnits;
    internal System.Windows.Forms.RadioButton optClearElevations;
    internal System.Windows.Forms.RadioButton optAssignZValues;
    internal System.Windows.Forms.CheckBox chkReportResults;
    private System.Windows.Forms.ComboBox cboLayerNameTIN;
    private System.Windows.Forms.Button btnChange;
    private System.Windows.Forms.ComboBox cboLayerNameDEM;
    private System.Windows.Forms.Label lblElevationUnits;
    internal System.Windows.Forms.TextBox txtElevationDifference;
    internal System.Windows.Forms.CheckBox chkElevationDifference;
  }
}