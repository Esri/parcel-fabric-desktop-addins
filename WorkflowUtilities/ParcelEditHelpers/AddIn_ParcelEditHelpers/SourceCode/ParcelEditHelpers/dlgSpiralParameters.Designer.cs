namespace ParcelEditHelper
{
    partial class dlgSpiralParameters
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
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnCreate = new System.Windows.Forms.Button();
      this.btnDensifyOptions = new System.Windows.Forms.Button();
      this.lblStartRadius = new System.Windows.Forms.Label();
      this.txtStartRadius = new System.Windows.Forms.TextBox();
      this.lblEndRadius = new System.Windows.Forms.Label();
      this.txtEndRadius = new System.Windows.Forms.TextBox();
      this.cboPathLengthParameter = new System.Windows.Forms.ComboBox();
      this.txtPathLengthParameter = new System.Windows.Forms.TextBox();
      this.optLeft = new System.Windows.Forms.RadioButton();
      this.optRight = new System.Windows.Forms.RadioButton();
      this.txtDirection = new System.Windows.Forms.TextBox();
      this.cboDensificationType = new System.Windows.Forms.ComboBox();
      this.lblTangentDirection = new System.Windows.Forms.Label();
      this.txtDensifyValue = new System.Windows.Forms.TextBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.optCustomDensification = new System.Windows.Forms.RadioButton();
      this.optDefaultDensification = new System.Windows.Forms.RadioButton();
      this.numAngleDensification = new System.Windows.Forms.NumericUpDown();
      this.lblDegreeSymbol = new System.Windows.Forms.Label();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numAngleDensification)).BeginInit();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(46, 0);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(89, 29);
      this.btnCancel.TabIndex = 9;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnCreate
      // 
      this.btnCreate.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnCreate.Location = new System.Drawing.Point(141, 0);
      this.btnCreate.Name = "btnCreate";
      this.btnCreate.Size = new System.Drawing.Size(89, 29);
      this.btnCreate.TabIndex = 4;
      this.btnCreate.Text = "Create";
      this.btnCreate.UseVisualStyleBackColor = true;
      this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
      // 
      // btnDensifyOptions
      // 
      this.btnDensifyOptions.Location = new System.Drawing.Point(0, 0);
      this.btnDensifyOptions.Name = "btnDensifyOptions";
      this.btnDensifyOptions.Size = new System.Drawing.Size(33, 29);
      this.btnDensifyOptions.TabIndex = 12;
      this.btnDensifyOptions.TabStop = false;
      this.btnDensifyOptions.Text = "V";
      this.btnDensifyOptions.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnDensifyOptions.UseVisualStyleBackColor = true;
      this.btnDensifyOptions.Click += new System.EventHandler(this.btnDensifyOptions_Click);
      // 
      // lblStartRadius
      // 
      this.lblStartRadius.AutoSize = true;
      this.lblStartRadius.Location = new System.Drawing.Point(14, 52);
      this.lblStartRadius.Name = "lblStartRadius";
      this.lblStartRadius.Size = new System.Drawing.Size(63, 13);
      this.lblStartRadius.TabIndex = 0;
      this.lblStartRadius.Text = "Start radius:";
      // 
      // txtStartRadius
      // 
      this.txtStartRadius.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtStartRadius.Location = new System.Drawing.Point(139, 49);
      this.txtStartRadius.Name = "txtStartRadius";
      this.txtStartRadius.Size = new System.Drawing.Size(75, 20);
      this.txtStartRadius.TabIndex = 1;
      this.txtStartRadius.Text = "INFINITY";
      this.txtStartRadius.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtStartRadius.TextChanged += new System.EventHandler(this.txtStartRadius_TextChanged);
      this.txtStartRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtStartRadius_KeyDown);
      this.txtStartRadius.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtStartRadius_KeyPress);
      // 
      // lblEndRadius
      // 
      this.lblEndRadius.AutoSize = true;
      this.lblEndRadius.Location = new System.Drawing.Point(14, 78);
      this.lblEndRadius.Name = "lblEndRadius";
      this.lblEndRadius.Size = new System.Drawing.Size(60, 13);
      this.lblEndRadius.TabIndex = 2;
      this.lblEndRadius.Text = "End radius:";
      // 
      // txtEndRadius
      // 
      this.txtEndRadius.Location = new System.Drawing.Point(139, 75);
      this.txtEndRadius.Name = "txtEndRadius";
      this.txtEndRadius.Size = new System.Drawing.Size(75, 20);
      this.txtEndRadius.TabIndex = 2;
      this.txtEndRadius.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtEndRadius.TextChanged += new System.EventHandler(this.txtEndRadius_TextChanged);
      this.txtEndRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEndRadius_KeyDown);
      this.txtEndRadius.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtEndRadius_KeyPress);
      // 
      // cboPathLengthParameter
      // 
      this.cboPathLengthParameter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboPathLengthParameter.FormattingEnabled = true;
      this.cboPathLengthParameter.Items.AddRange(new object[] {
            "Arc length",
            "Delta angle"});
      this.cboPathLengthParameter.Location = new System.Drawing.Point(12, 112);
      this.cboPathLengthParameter.Name = "cboPathLengthParameter";
      this.cboPathLengthParameter.Size = new System.Drawing.Size(115, 21);
      this.cboPathLengthParameter.TabIndex = 4;
      this.cboPathLengthParameter.TabStop = false;
      // 
      // txtPathLengthParameter
      // 
      this.txtPathLengthParameter.Location = new System.Drawing.Point(139, 112);
      this.txtPathLengthParameter.Name = "txtPathLengthParameter";
      this.txtPathLengthParameter.Size = new System.Drawing.Size(75, 20);
      this.txtPathLengthParameter.TabIndex = 3;
      this.txtPathLengthParameter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtPathLengthParameter.TextChanged += new System.EventHandler(this.txtPathLengthParameter_TextChanged);
      this.txtPathLengthParameter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPathLengthParameter_KeyDown);
      this.txtPathLengthParameter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPathLengthParameter_KeyPress);
      // 
      // optLeft
      // 
      this.optLeft.AutoSize = true;
      this.optLeft.Checked = true;
      this.optLeft.Location = new System.Drawing.Point(131, 152);
      this.optLeft.Name = "optLeft";
      this.optLeft.Size = new System.Drawing.Size(43, 17);
      this.optLeft.TabIndex = 7;
      this.optLeft.TabStop = true;
      this.optLeft.Text = "Left";
      this.optLeft.UseVisualStyleBackColor = true;
      // 
      // optRight
      // 
      this.optRight.AutoSize = true;
      this.optRight.Location = new System.Drawing.Point(180, 152);
      this.optRight.Name = "optRight";
      this.optRight.Size = new System.Drawing.Size(50, 17);
      this.optRight.TabIndex = 8;
      this.optRight.Text = "Right";
      this.optRight.UseVisualStyleBackColor = true;
      // 
      // txtDirection
      // 
      this.txtDirection.Location = new System.Drawing.Point(139, 12);
      this.txtDirection.Name = "txtDirection";
      this.txtDirection.Size = new System.Drawing.Size(91, 20);
      this.txtDirection.TabIndex = 0;
      this.txtDirection.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // cboDensificationType
      // 
      this.cboDensificationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboDensificationType.FormattingEnabled = true;
      this.cboDensificationType.Items.AddRange(new object[] {
            "Equal segment lengths of",
            "Equal central angles of",
            "Minimize deviation to "});
      this.cboDensificationType.Location = new System.Drawing.Point(19, 55);
      this.cboDensificationType.Name = "cboDensificationType";
      this.cboDensificationType.Size = new System.Drawing.Size(144, 21);
      this.cboDensificationType.TabIndex = 13;
      this.cboDensificationType.TabStop = false;
      this.cboDensificationType.SelectedIndexChanged += new System.EventHandler(this.cboDensificationType_SelectedIndexChanged);
      // 
      // lblTangentDirection
      // 
      this.lblTangentDirection.AutoSize = true;
      this.lblTangentDirection.Location = new System.Drawing.Point(14, 15);
      this.lblTangentDirection.Name = "lblTangentDirection";
      this.lblTangentDirection.Size = new System.Drawing.Size(93, 13);
      this.lblTangentDirection.TabIndex = 11;
      this.lblTangentDirection.Text = "Tangent direction:";
      // 
      // txtDensifyValue
      // 
      this.txtDensifyValue.Location = new System.Drawing.Point(169, 56);
      this.txtDensifyValue.Name = "txtDensifyValue";
      this.txtDensifyValue.Size = new System.Drawing.Size(53, 20);
      this.txtDensifyValue.TabIndex = 15;
      this.txtDensifyValue.TabStop = false;
      this.txtDensifyValue.Text = "0.25";
      this.txtDensifyValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtDensifyValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDensifyValue_KeyDown);
      this.txtDensifyValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDensifyValue_KeyPress);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.lblDegreeSymbol);
      this.panel1.Controls.Add(this.numAngleDensification);
      this.panel1.Controls.Add(this.optDefaultDensification);
      this.panel1.Controls.Add(this.optCustomDensification);
      this.panel1.Controls.Add(this.txtDensifyValue);
      this.panel1.Controls.Add(this.cboDensificationType);
      this.panel1.Location = new System.Drawing.Point(8, 233);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(239, 96);
      this.panel1.TabIndex = 16;
      this.panel1.Visible = false;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.btnCreate);
      this.panel2.Controls.Add(this.btnCancel);
      this.panel2.Controls.Add(this.btnDensifyOptions);
      this.panel2.Location = new System.Drawing.Point(8, 186);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(239, 41);
      this.panel2.TabIndex = 17;
      // 
      // optCustomDensification
      // 
      this.optCustomDensification.AutoSize = true;
      this.optCustomDensification.Location = new System.Drawing.Point(8, 32);
      this.optCustomDensification.Name = "optCustomDensification";
      this.optCustomDensification.Size = new System.Drawing.Size(171, 17);
      this.optCustomDensification.TabIndex = 16;
      this.optCustomDensification.Text = "Customize densifcation method";
      this.optCustomDensification.UseVisualStyleBackColor = true;
      this.optCustomDensification.CheckedChanged += new System.EventHandler(this.optCustomDensification_CheckedChanged);
      // 
      // optDefaultDensification
      // 
      this.optDefaultDensification.AutoSize = true;
      this.optDefaultDensification.Checked = true;
      this.optDefaultDensification.Location = new System.Drawing.Point(8, 1);
      this.optDefaultDensification.Name = "optDefaultDensification";
      this.optDefaultDensification.Size = new System.Drawing.Size(159, 17);
      this.optDefaultDensification.TabIndex = 17;
      this.optDefaultDensification.Text = "Default densification method";
      this.optDefaultDensification.UseVisualStyleBackColor = true;
      this.optDefaultDensification.CheckedChanged += new System.EventHandler(this.optDefaultDensification_CheckedChanged);
      // 
      // numAngleDensification
      // 
      this.numAngleDensification.DecimalPlaces = 1;
      this.numAngleDensification.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
      this.numAngleDensification.Location = new System.Drawing.Point(179, 12);
      this.numAngleDensification.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numAngleDensification.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
      this.numAngleDensification.Name = "numAngleDensification";
      this.numAngleDensification.ReadOnly = true;
      this.numAngleDensification.Size = new System.Drawing.Size(43, 20);
      this.numAngleDensification.TabIndex = 18;
      this.numAngleDensification.TabStop = false;
      this.numAngleDensification.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numAngleDensification.Value = new decimal(new int[] {
            20,
            0,
            0,
            65536});
      this.numAngleDensification.Visible = false;
      // 
      // lblDegreeSymbol
      // 
      this.lblDegreeSymbol.AutoSize = true;
      this.lblDegreeSymbol.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblDegreeSymbol.Location = new System.Drawing.Point(220, 10);
      this.lblDegreeSymbol.Name = "lblDegreeSymbol";
      this.lblDegreeSymbol.Size = new System.Drawing.Size(15, 20);
      this.lblDegreeSymbol.TabIndex = 19;
      this.lblDegreeSymbol.Text = "°";
      this.lblDegreeSymbol.Visible = false;
      // 
      // dlgSpiralParameters
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(351, 343);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.lblTangentDirection);
      this.Controls.Add(this.txtPathLengthParameter);
      this.Controls.Add(this.lblStartRadius);
      this.Controls.Add(this.txtDirection);
      this.Controls.Add(this.txtStartRadius);
      this.Controls.Add(this.optRight);
      this.Controls.Add(this.lblEndRadius);
      this.Controls.Add(this.optLeft);
      this.Controls.Add(this.txtEndRadius);
      this.Controls.Add(this.cboPathLengthParameter);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "dlgSpiralParameters";
      this.Text = "Spiral";
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numAngleDensification)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnCreate;
    private System.Windows.Forms.Button btnDensifyOptions;
    private System.Windows.Forms.Label lblStartRadius;
    internal System.Windows.Forms.TextBox txtStartRadius;
    private System.Windows.Forms.Label lblEndRadius;
    internal System.Windows.Forms.TextBox txtEndRadius;
    internal System.Windows.Forms.ComboBox cboPathLengthParameter;
    internal System.Windows.Forms.TextBox txtPathLengthParameter;
    internal System.Windows.Forms.RadioButton optLeft;
    internal System.Windows.Forms.RadioButton optRight;
    internal System.Windows.Forms.TextBox txtDirection;
    internal System.Windows.Forms.ComboBox cboDensificationType;
    private System.Windows.Forms.Label lblTangentDirection;
    internal System.Windows.Forms.TextBox txtDensifyValue;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    internal System.Windows.Forms.RadioButton optDefaultDensification;
    internal System.Windows.Forms.RadioButton optCustomDensification;
    internal System.Windows.Forms.NumericUpDown numAngleDensification;
    private System.Windows.Forms.Label lblDegreeSymbol;
  }
}