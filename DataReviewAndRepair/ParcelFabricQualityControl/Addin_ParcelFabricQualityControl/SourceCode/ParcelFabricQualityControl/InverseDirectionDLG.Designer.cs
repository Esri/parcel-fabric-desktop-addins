namespace ParcelFabricQualityControl
{
  partial class InverseDirectionDLG
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InverseDirectionDLG));
      this.lblRecalc = new System.Windows.Forms.Label();
      this.chkDirectionDifference = new System.Windows.Forms.CheckBox();
      this.txtDirectionDifference = new System.Windows.Forms.TextBox();
      this.lblDirectionUnits = new System.Windows.Forms.Label();
      this.chkReportResults = new System.Windows.Forms.CheckBox();
      this.btnResetDefaults = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.lblDirectionOffset = new System.Windows.Forms.Label();
      this.lblDistanceUnits1 = new System.Windows.Forms.Label();
      this.txtSubtendedDist = new System.Windows.Forms.TextBox();
      this.txtDirectionOffset = new System.Windows.Forms.TextBox();
      this.lblAngleUnits = new System.Windows.Forms.Label();
      this.optManualEnteredDirnOffset = new System.Windows.Forms.RadioButton();
      this.optComputeDirnOffset = new System.Windows.Forms.RadioButton();
      this.chkSubtendedDistance = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.btnGetOffsetFromEditor = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblRecalc
      // 
      resources.ApplyResources(this.lblRecalc, "lblRecalc");
      this.lblRecalc.Name = "lblRecalc";
      // 
      // chkDirectionDifference
      // 
      resources.ApplyResources(this.chkDirectionDifference, "chkDirectionDifference");
      this.chkDirectionDifference.Checked = true;
      this.chkDirectionDifference.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkDirectionDifference.Name = "chkDirectionDifference";
      this.chkDirectionDifference.UseVisualStyleBackColor = true;
      this.chkDirectionDifference.CheckedChanged += new System.EventHandler(this.chkDirectionDifference_CheckedChanged);
      // 
      // txtDirectionDifference
      // 
      resources.ApplyResources(this.txtDirectionDifference, "txtDirectionDifference");
      this.txtDirectionDifference.Name = "txtDirectionDifference";
      this.txtDirectionDifference.Tag = "";
      this.txtDirectionDifference.TextChanged += new System.EventHandler(this.txtDirectionDifference_TextChanged);
      this.txtDirectionDifference.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDirectionDifference_KeyDown);
      this.txtDirectionDifference.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDirectionDifference_KeyPress);
      // 
      // lblDirectionUnits
      // 
      resources.ApplyResources(this.lblDirectionUnits, "lblDirectionUnits");
      this.lblDirectionUnits.Name = "lblDirectionUnits";
      // 
      // chkReportResults
      // 
      resources.ApplyResources(this.chkReportResults, "chkReportResults");
      this.chkReportResults.Checked = true;
      this.chkReportResults.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkReportResults.Name = "chkReportResults";
      this.chkReportResults.UseVisualStyleBackColor = true;
      // 
      // btnResetDefaults
      // 
      resources.ApplyResources(this.btnResetDefaults, "btnResetDefaults");
      this.btnResetDefaults.Name = "btnResetDefaults";
      this.btnResetDefaults.Tag = "Reset the values to the recommended common settings";
      this.btnResetDefaults.UseVisualStyleBackColor = true;
      this.btnResetDefaults.Click += new System.EventHandler(this.btnResetDefaults_Click);
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      resources.ApplyResources(this.button2, "button2");
      this.button2.Name = "button2";
      this.button2.UseVisualStyleBackColor = true;
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      resources.ApplyResources(this.button1, "button1");
      this.button1.Name = "button1";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // lblDirectionOffset
      // 
      resources.ApplyResources(this.lblDirectionOffset, "lblDirectionOffset");
      this.lblDirectionOffset.Name = "lblDirectionOffset";
      // 
      // lblDistanceUnits1
      // 
      resources.ApplyResources(this.lblDistanceUnits1, "lblDistanceUnits1");
      this.lblDistanceUnits1.Name = "lblDistanceUnits1";
      // 
      // txtSubtendedDist
      // 
      resources.ApplyResources(this.txtSubtendedDist, "txtSubtendedDist");
      this.txtSubtendedDist.Name = "txtSubtendedDist";
      this.txtSubtendedDist.TextChanged += new System.EventHandler(this.txtSubtendedDist_TextChanged);
      this.txtSubtendedDist.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSubtendedDist_KeyDown);
      this.txtSubtendedDist.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSubtendedDist_KeyPress);
      // 
      // txtDirectionOffset
      // 
      resources.ApplyResources(this.txtDirectionOffset, "txtDirectionOffset");
      this.txtDirectionOffset.Name = "txtDirectionOffset";
      this.txtDirectionOffset.TextChanged += new System.EventHandler(this.txtDirectionOffset_TextChanged);
      this.txtDirectionOffset.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDirectionOffset_KeyDown);
      this.txtDirectionOffset.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDirectionOffset_KeyPress);
      // 
      // lblAngleUnits
      // 
      resources.ApplyResources(this.lblAngleUnits, "lblAngleUnits");
      this.lblAngleUnits.Name = "lblAngleUnits";
      // 
      // optManualEnteredDirnOffset
      // 
      resources.ApplyResources(this.optManualEnteredDirnOffset, "optManualEnteredDirnOffset");
      this.optManualEnteredDirnOffset.Name = "optManualEnteredDirnOffset";
      this.optManualEnteredDirnOffset.UseVisualStyleBackColor = true;
      this.optManualEnteredDirnOffset.CheckedChanged += new System.EventHandler(this.optManualEnteredDirnOffset_CheckedChanged);
      // 
      // optComputeDirnOffset
      // 
      resources.ApplyResources(this.optComputeDirnOffset, "optComputeDirnOffset");
      this.optComputeDirnOffset.Checked = true;
      this.optComputeDirnOffset.Name = "optComputeDirnOffset";
      this.optComputeDirnOffset.TabStop = true;
      this.optComputeDirnOffset.UseVisualStyleBackColor = true;
      // 
      // chkSubtendedDistance
      // 
      resources.ApplyResources(this.chkSubtendedDistance, "chkSubtendedDistance");
      this.chkSubtendedDistance.Name = "chkSubtendedDistance";
      this.chkSubtendedDistance.UseVisualStyleBackColor = true;
      this.chkSubtendedDistance.CheckedChanged += new System.EventHandler(this.chkSubtendedDistance_CheckedChanged);
      // 
      // label1
      // 
      resources.ApplyResources(this.label1, "label1");
      this.label1.Name = "label1";
      // 
      // btnGetOffsetFromEditor
      // 
      resources.ApplyResources(this.btnGetOffsetFromEditor, "btnGetOffsetFromEditor");
      this.btnGetOffsetFromEditor.Name = "btnGetOffsetFromEditor";
      this.btnGetOffsetFromEditor.Tag = "Get the direction offset from the Editing options";
      this.btnGetOffsetFromEditor.UseVisualStyleBackColor = true;
      this.btnGetOffsetFromEditor.Click += new System.EventHandler(this.btnGetOffsetFromEditor_Click);
      this.btnGetOffsetFromEditor.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.btnGetOffsetFromEditor_HelpRequested);
      this.btnGetOffsetFromEditor.MouseHover += new System.EventHandler(this.btnGetOffsetFromEditor_MouseHover);
      // 
      // InverseDirectionDLG
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.btnGetOffsetFromEditor);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.chkSubtendedDistance);
      this.Controls.Add(this.optComputeDirnOffset);
      this.Controls.Add(this.optManualEnteredDirnOffset);
      this.Controls.Add(this.lblAngleUnits);
      this.Controls.Add(this.txtDirectionOffset);
      this.Controls.Add(this.lblDirectionOffset);
      this.Controls.Add(this.lblDistanceUnits1);
      this.Controls.Add(this.txtSubtendedDist);
      this.Controls.Add(this.chkReportResults);
      this.Controls.Add(this.btnResetDefaults);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.chkDirectionDifference);
      this.Controls.Add(this.txtDirectionDifference);
      this.Controls.Add(this.lblDirectionUnits);
      this.Controls.Add(this.lblRecalc);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.HelpButton = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "InverseDirectionDLG";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Load += new System.EventHandler(this.InverseDirectionDLG_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblRecalc;
    internal System.Windows.Forms.CheckBox chkDirectionDifference;
    internal System.Windows.Forms.TextBox txtDirectionDifference;
    internal System.Windows.Forms.Label lblDirectionUnits;
    internal System.Windows.Forms.CheckBox chkReportResults;
    private System.Windows.Forms.Button btnResetDefaults;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label lblDirectionOffset;
    internal System.Windows.Forms.Label lblDistanceUnits1;
    private System.Windows.Forms.Label label1;
    internal System.Windows.Forms.Label lblAngleUnits;
    internal System.Windows.Forms.TextBox txtDirectionOffset;
    internal System.Windows.Forms.RadioButton optManualEnteredDirnOffset;
    internal System.Windows.Forms.RadioButton optComputeDirnOffset;
    internal System.Windows.Forms.TextBox txtSubtendedDist;
    internal System.Windows.Forms.CheckBox chkSubtendedDistance;
    private System.Windows.Forms.Button btnGetOffsetFromEditor;
  }
}