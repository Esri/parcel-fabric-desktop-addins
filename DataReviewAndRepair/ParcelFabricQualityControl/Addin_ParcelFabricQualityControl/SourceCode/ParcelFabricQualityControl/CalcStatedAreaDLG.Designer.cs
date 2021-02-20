namespace ParcelFabricQualityControl
{
    partial class CalcStatedAreaDLG
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
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalcStatedAreaDLG));
          this.cboAreaUnit = new System.Windows.Forms.ComboBox();
          this.btnCancel = new System.Windows.Forms.Button();
          this.btnCalculate = new System.Windows.Forms.Button();
          this.lblAreaUnit = new System.Windows.Forms.Label();
          this.lblAreaSuffix = new System.Windows.Forms.Label();
          this.txtSuffix = new System.Windows.Forms.TextBox();
          this.label1 = new System.Windows.Forms.Label();
          this.numDecPlaces = new System.Windows.Forms.NumericUpDown();
          ((System.ComponentModel.ISupportInitialize)(this.numDecPlaces)).BeginInit();
          this.SuspendLayout();
          // 
          // cboAreaUnit
          // 
          this.cboAreaUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.cboAreaUnit.FormattingEnabled = true;
          this.cboAreaUnit.Items.AddRange(new object[] {
            resources.GetString("cboAreaUnit.Items"),
            resources.GetString("cboAreaUnit.Items1"),
            resources.GetString("cboAreaUnit.Items2"),
            resources.GetString("cboAreaUnit.Items3"),
            resources.GetString("cboAreaUnit.Items4")});
          resources.ApplyResources(this.cboAreaUnit, "cboAreaUnit");
          this.cboAreaUnit.Name = "cboAreaUnit";
          this.cboAreaUnit.SelectedIndexChanged += new System.EventHandler(this.cboAreaUnit_SelectedIndexChanged);
          // 
          // btnCancel
          // 
          this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          resources.ApplyResources(this.btnCancel, "btnCancel");
          this.btnCancel.Name = "btnCancel";
          this.btnCancel.UseVisualStyleBackColor = true;
          // 
          // btnCalculate
          // 
          this.btnCalculate.DialogResult = System.Windows.Forms.DialogResult.OK;
          resources.ApplyResources(this.btnCalculate, "btnCalculate");
          this.btnCalculate.Name = "btnCalculate";
          this.btnCalculate.UseVisualStyleBackColor = true;
          this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
          // 
          // lblAreaUnit
          // 
          resources.ApplyResources(this.lblAreaUnit, "lblAreaUnit");
          this.lblAreaUnit.Name = "lblAreaUnit";
          // 
          // lblAreaSuffix
          // 
          resources.ApplyResources(this.lblAreaSuffix, "lblAreaSuffix");
          this.lblAreaSuffix.Name = "lblAreaSuffix";
          // 
          // txtSuffix
          // 
          resources.ApplyResources(this.txtSuffix, "txtSuffix");
          this.txtSuffix.Name = "txtSuffix";
          // 
          // label1
          // 
          resources.ApplyResources(this.label1, "label1");
          this.label1.Name = "label1";
          // 
          // numDecPlaces
          // 
          resources.ApplyResources(this.numDecPlaces, "numDecPlaces");
          this.numDecPlaces.Name = "numDecPlaces";
          this.numDecPlaces.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
          // 
          // CalcStatedAreaDLG
          // 
          resources.ApplyResources(this, "$this");
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.Controls.Add(this.numDecPlaces);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.txtSuffix);
          this.Controls.Add(this.lblAreaSuffix);
          this.Controls.Add(this.lblAreaUnit);
          this.Controls.Add(this.btnCalculate);
          this.Controls.Add(this.btnCancel);
          this.Controls.Add(this.cboAreaUnit);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
          this.MaximizeBox = false;
          this.MinimizeBox = false;
          this.Name = "CalcStatedAreaDLG";
          this.ShowInTaskbar = false;
          ((System.ComponentModel.ISupportInitialize)(this.numDecPlaces)).EndInit();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Label lblAreaUnit;
        private System.Windows.Forms.Label lblAreaSuffix;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.ComboBox cboAreaUnit;
        internal System.Windows.Forms.TextBox txtSuffix;
        internal System.Windows.Forms.NumericUpDown numDecPlaces;
    }
}