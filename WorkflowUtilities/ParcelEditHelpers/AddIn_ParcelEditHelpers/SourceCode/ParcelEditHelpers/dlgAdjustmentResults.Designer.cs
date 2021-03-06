﻿namespace ParcelEditHelper
{
  partial class dlgAdjustmentResults
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
      this.tvwResults = new System.Windows.Forms.TreeView();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnAccep = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnResultOptions = new System.Windows.Forms.Button();
      this.txtReport = new System.Windows.Forms.TextBox();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tvwResults
      // 
      this.tvwResults.CheckBoxes = true;
      this.tvwResults.Location = new System.Drawing.Point(313, 27);
      this.tvwResults.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tvwResults.Name = "tvwResults";
      this.tvwResults.Size = new System.Drawing.Size(143, 148);
      this.tvwResults.TabIndex = 0;
      this.tvwResults.Visible = false;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnCancel.Location = new System.Drawing.Point(362, 0);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(117, 32);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnAccep
      // 
      this.btnAccep.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnAccep.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnAccep.Location = new System.Drawing.Point(479, 0);
      this.btnAccep.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnAccep.Name = "btnAccep";
      this.btnAccep.Size = new System.Drawing.Size(117, 32);
      this.btnAccep.TabIndex = 1;
      this.btnAccep.Text = "Accept";
      this.btnAccep.UseVisualStyleBackColor = true;
      this.btnAccep.Click += new System.EventHandler(this.btnAccep_Click);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnResultOptions);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnAccep);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 475);
      this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(596, 32);
      this.panel1.TabIndex = 3;
      // 
      // btnResultOptions
      // 
      this.btnResultOptions.Dock = System.Windows.Forms.DockStyle.Left;
      this.btnResultOptions.Location = new System.Drawing.Point(0, 0);
      this.btnResultOptions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnResultOptions.Name = "btnResultOptions";
      this.btnResultOptions.Size = new System.Drawing.Size(49, 32);
      this.btnResultOptions.TabIndex = 3;
      this.btnResultOptions.Text = "V";
      this.btnResultOptions.UseVisualStyleBackColor = true;
      this.btnResultOptions.Click += new System.EventHandler(this.btnResultOptions_Click);
      // 
      // txtReport
      // 
      this.txtReport.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtReport.HideSelection = false;
      this.txtReport.Location = new System.Drawing.Point(0, 0);
      this.txtReport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.txtReport.Multiline = true;
      this.txtReport.Name = "txtReport";
      this.txtReport.ReadOnly = true;
      this.txtReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtReport.Size = new System.Drawing.Size(596, 475);
      this.txtReport.TabIndex = 4;
      this.txtReport.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.txtReport.WordWrap = false;
      this.txtReport.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtReport_MouseClick);
      // 
      // dlgAdjustmentResults
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(596, 507);
      this.Controls.Add(this.txtReport);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.tvwResults);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "dlgAdjustmentResults";
      this.Text = "Fabric Adjustment Results";
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TreeView tvwResults;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnAccep;
    private System.Windows.Forms.Panel panel1;
    internal System.Windows.Forms.TextBox txtReport;
    private System.Windows.Forms.Button btnResultOptions;
  }
}