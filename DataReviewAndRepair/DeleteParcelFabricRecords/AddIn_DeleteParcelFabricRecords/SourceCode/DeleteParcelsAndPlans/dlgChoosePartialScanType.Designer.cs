namespace DeleteSelectedParcels
{
  partial class dlgChoosePartialScanType
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.optLinePointsDisplaced = new System.Windows.Forms.RadioButton();
            this.optInvalidFCAssocs = new System.Windows.Forms.RadioButton();
            this.optInvalidVectors = new System.Windows.Forms.RadioButton();
            this.optNoLineParcels = new System.Windows.Forms.RadioButton();
            this.optOrphanLinePoints = new System.Windows.Forms.RadioButton();
            this.optOrphanLines = new System.Windows.Forms.RadioButton();
            this.optSameFromTo = new System.Windows.Forms.RadioButton();
            this.optOrphanPoints = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Choose the type of records to scan:";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(154, 194);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnStart.Location = new System.Drawing.Point(261, 194);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(101, 23);
            this.btnStart.TabIndex = 9;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.optLinePointsDisplaced);
            this.panel1.Controls.Add(this.optInvalidFCAssocs);
            this.panel1.Controls.Add(this.optInvalidVectors);
            this.panel1.Controls.Add(this.optNoLineParcels);
            this.panel1.Controls.Add(this.optOrphanLinePoints);
            this.panel1.Controls.Add(this.optOrphanLines);
            this.panel1.Controls.Add(this.optSameFromTo);
            this.panel1.Controls.Add(this.optOrphanPoints);
            this.panel1.Location = new System.Drawing.Point(19, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(332, 141);
            this.panel1.TabIndex = 10;
            this.panel1.MouseEnter += new System.EventHandler(this.panel1_MouseEnter);
            // 
            // optLinePointsDisplaced
            // 
            this.optLinePointsDisplaced.AutoSize = true;
            this.optLinePointsDisplaced.Location = new System.Drawing.Point(3, 95);
            this.optLinePointsDisplaced.Name = "optLinePointsDisplaced";
            this.optLinePointsDisplaced.Size = new System.Drawing.Size(192, 17);
            this.optLinePointsDisplaced.TabIndex = 14;
            this.optLinePointsDisplaced.TabStop = true;
            this.optLinePointsDisplaced.Text = "Line points with displaced geometry";
            this.optLinePointsDisplaced.UseVisualStyleBackColor = true;
            this.optLinePointsDisplaced.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optLinePointsDisplaced_HelpRequested);
            // 
            // optInvalidFCAssocs
            // 
            this.optInvalidFCAssocs.AutoSize = true;
            this.optInvalidFCAssocs.Location = new System.Drawing.Point(3, 164);
            this.optInvalidFCAssocs.Name = "optInvalidFCAssocs";
            this.optInvalidFCAssocs.Size = new System.Drawing.Size(180, 17);
            this.optInvalidFCAssocs.TabIndex = 13;
            this.optInvalidFCAssocs.Text = "Invalid feature class associations";
            this.optInvalidFCAssocs.UseVisualStyleBackColor = true;
            this.optInvalidFCAssocs.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optInvalidFCAssocs_HelpRequested);
            // 
            // optInvalidVectors
            // 
            this.optInvalidVectors.AutoSize = true;
            this.optInvalidVectors.Location = new System.Drawing.Point(3, 141);
            this.optInvalidVectors.Name = "optInvalidVectors";
            this.optInvalidVectors.Size = new System.Drawing.Size(184, 17);
            this.optInvalidVectors.TabIndex = 12;
            this.optInvalidVectors.Text = "Invalid feature adjustment vectors";
            this.optInvalidVectors.UseVisualStyleBackColor = true;
            this.optInvalidVectors.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optInvalidVectors_HelpRequested);
            // 
            // optNoLineParcels
            // 
            this.optNoLineParcels.AutoSize = true;
            this.optNoLineParcels.Location = new System.Drawing.Point(3, 118);
            this.optNoLineParcels.Name = "optNoLineParcels";
            this.optNoLineParcels.Size = new System.Drawing.Size(147, 17);
            this.optNoLineParcels.TabIndex = 11;
            this.optNoLineParcels.Text = "Parcels that have no lines";
            this.optNoLineParcels.UseVisualStyleBackColor = true;
            this.optNoLineParcels.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optNoLineParcels_HelpRequested);
            // 
            // optOrphanLinePoints
            // 
            this.optOrphanLinePoints.AutoSize = true;
            this.optOrphanLinePoints.Location = new System.Drawing.Point(3, 72);
            this.optOrphanLinePoints.Name = "optOrphanLinePoints";
            this.optOrphanLinePoints.Size = new System.Drawing.Size(210, 17);
            this.optOrphanLinePoints.TabIndex = 10;
            this.optOrphanLinePoints.Text = "Line points with invalid point references";
            this.optOrphanLinePoints.UseVisualStyleBackColor = true;
            this.optOrphanLinePoints.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optOrphanLinePoints_HelpRequested);
            // 
            // optOrphanLines
            // 
            this.optOrphanLines.AutoSize = true;
            this.optOrphanLines.Location = new System.Drawing.Point(3, 49);
            this.optOrphanLines.Name = "optOrphanLines";
            this.optOrphanLines.Size = new System.Drawing.Size(192, 17);
            this.optOrphanLines.TabIndex = 9;
            this.optOrphanLines.Text = "Lines that do not belong to a parcel";
            this.optOrphanLines.UseVisualStyleBackColor = true;
            this.optOrphanLines.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optOrphanLines_HelpRequested);
            // 
            // optSameFromTo
            // 
            this.optSameFromTo.AutoSize = true;
            this.optSameFromTo.Location = new System.Drawing.Point(3, 26);
            this.optSameFromTo.Name = "optSameFromTo";
            this.optSameFromTo.Size = new System.Drawing.Size(212, 17);
            this.optSameFromTo.TabIndex = 8;
            this.optSameFromTo.Text = "Lines with the same To and From points";
            this.optSameFromTo.UseVisualStyleBackColor = true;
            this.optSameFromTo.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optSameFromTo_HelpRequested);
            // 
            // optOrphanPoints
            // 
            this.optOrphanPoints.AutoSize = true;
            this.optOrphanPoints.Checked = true;
            this.optOrphanPoints.Location = new System.Drawing.Point(3, 3);
            this.optOrphanPoints.Name = "optOrphanPoints";
            this.optOrphanPoints.Size = new System.Drawing.Size(201, 17);
            this.optOrphanPoints.TabIndex = 7;
            this.optOrphanPoints.TabStop = true;
            this.optOrphanPoints.Text = "Points that are not connected to lines";
            this.optOrphanPoints.UseVisualStyleBackColor = true;
            this.optOrphanPoints.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.optOrphanPoints_HelpRequested);
            // 
            // dlgChoosePartialScanType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(485, 271);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "dlgChoosePartialScanType";
            this.Text = "Focused Fabric Scan";
            this.Load += new System.EventHandler(this.dlgChoosePartialScanType_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Panel panel1;
    internal System.Windows.Forms.RadioButton optInvalidFCAssocs;
    internal System.Windows.Forms.RadioButton optInvalidVectors;
    internal System.Windows.Forms.RadioButton optNoLineParcels;
    internal System.Windows.Forms.RadioButton optOrphanLinePoints;
    internal System.Windows.Forms.RadioButton optOrphanLines;
    internal System.Windows.Forms.RadioButton optSameFromTo;
    internal System.Windows.Forms.RadioButton optOrphanPoints;
    internal System.Windows.Forms.RadioButton optLinePointsDisplaced;
  }
}