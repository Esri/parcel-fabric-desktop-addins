using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using ESRI.ArcGIS.Editor;

namespace ParcelFabricQualityControl
{
  public partial class InverseDistanceDlg : Form
  {
    private IEditProperties2 m_EditorProperties;
    public InverseDistanceDlg(IEditProperties2 EditorProperties)
    {
      InitializeComponent();
      m_EditorProperties = EditorProperties;

      Utilities Utils = new Utilities();

      string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
      if (sDesktopVers.Trim() == "")
        sDesktopVers = "Desktop10.1";
      else
        sDesktopVers = "Desktop" + sDesktopVers;
      string sValues =
      Utils.ReadFromRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral",
        "AddIn.FabricQualityControl_InverseDistance");
      if (sValues.Trim() == "")
        return;
      string[] Values = sValues.Split(',');

      try
      {
        Point p = new Point();
        txtHeightParameter.Location = txtServiceURL.Location;
        p.X = txtHeightParameter.Location.X + 10 + txtHeightParameter.Width;
        p.Y = txtHeightParameter.Location.Y;
        cboUnits.Location = p;
        cboUnits.SelectedItem = "m"; //default to meters, but change later if needed
        txtHeightParameter.Visible = false;
        cboUnits.Visible = txtHeightParameter.Visible;
        this.chkDistanceDifference.Checked = (Values[0].Trim() == "True");
        this.txtDistDifference.Text = Values[1];
        this.chkApplyScaleFactor.Checked = (Values[2].Trim() == "True");
        this.optUserEnteredScaleFactor.Checked = (Values[3].Trim() == "True");
        this.txtScaleFactor.Text = string.Format(Values[4], "0.000000000000000");
        this.optComputeForMe.Checked = (Values[5].Trim() == "True");
        this.cboScaleMethod.SelectedIndex = Convert.ToInt32(Values[6]);
        this.txtHeightParameter.Text = Values[7];
        this.txtServiceURL.Text = Values[8];
        this.chkReportResults.Checked = (Values[9].Trim() == "True");

        string sUnit = Values[10].Trim();
        if (sUnit!="m")
          cboUnits.SelectedItem = sUnit;

      }
      catch
      {}
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void lblDistanceDiff_Click(object sender, EventArgs e)
    {

    }

    private void txtDistDifferance_TextChanged(object sender, EventArgs e)
    {

    }

    private void txtServiceURL_TextChanged(object sender, EventArgs e)
    {

    }

    private void txtServiceURL_MouseHover(object sender, EventArgs e)
    {
      System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
      ToolTip1.SetToolTip(this.txtServiceURL, txtServiceURL.Tag.ToString());
    }

    private void txtDistDifferance_MouseHover(object sender, EventArgs e)
    {
      string sTip = txtDistDifference.Tag.ToString().Replace("/",Environment.NewLine);
      System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
      ToolTip1.SetToolTip(this.txtDistDifference, sTip);
    }

    private void optUserEnteredScaleFactor_CheckedChanged(object sender, EventArgs e)
    {
      cboScaleMethod.Enabled = btnChange.Enabled = optComputeForMe.Checked;
      txtHeightParameter.Enabled = optComputeForMe.Checked;
      cboUnits.Enabled = txtHeightParameter.Enabled;
    }

    private void chkDistanceDifference_CheckedChanged(object sender, EventArgs e)
    {
      txtDistDifference.Enabled = chkDistanceDifference.Checked;
    }

    private void chkApplyScaleFactor_CheckedChanged(object sender, EventArgs e)
    {
      panel1.Enabled = chkApplyScaleFactor.Checked;
    }

    private void optComputeForMe_CheckedChanged(object sender, EventArgs e)
    {
      cboScaleMethod.Enabled = btnChange.Enabled = optComputeForMe.Checked;
      txtScaleFactor.Enabled = btnGetScaleFromEditor.Enabled = !cboScaleMethod.Enabled;
      txtHeightParameter.Enabled = optComputeForMe.Enabled;
      cboUnits.Enabled = optComputeForMe.Enabled;
    }

    private void InverseDistanceDlg_Load(object sender, EventArgs e)
    {
      cboScaleMethod.SelectedItem = "Entered mean sea-level";
      txtHeightParameter.Enabled = true;
      //lblDistanceUnits2.Text = lblDistanceUnits1.Text;
    }

    private void cboScaleMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cboScaleMethod.SelectedIndex == 0)
      {
        txtHeightParameter.Location = txtServiceURL.Location;
        txtHeightParameter.Visible = true;
        cboUnits.Visible = true;
        cboUnits.Enabled = txtHeightParameter.Enabled;
        Point p = new Point();
        p.X=txtHeightParameter.Location.X+10+txtHeightParameter.Width;
        p.Y=txtHeightParameter.Location.Y;
        cboUnits.Location = p;
        cboUnits.SelectedItem = "m";
        txtServiceURL.Visible = false;
        btnChange.Visible = false;
      }
      else
      {
        txtServiceURL.Location = txtHeightParameter.Location;
        txtServiceURL.Visible = true;
        txtHeightParameter.Visible = false;
        cboUnits.Visible = false;
        btnChange.Visible = true;
      }
    }

    private void btnChange_Click(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
      //write the key
      Utilities Utils = new Utilities();
      string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
      if (sDesktopVers.Trim() == "")
        sDesktopVers = "Desktop10.1";
      else
        sDesktopVers = "Desktop" + sDesktopVers;


      string sBool0 = this.chkDistanceDifference.Checked.ToString();
      string sTxt1=this.txtDistDifference.Text;
      string sBool2=this.chkApplyScaleFactor.Checked.ToString();
      string sBool3=this.optUserEnteredScaleFactor.Checked.ToString();
      string sTxt4=this.txtScaleFactor.Text;
      string sBool5=this.optComputeForMe.Checked.ToString();
      string sBool6=this.cboScaleMethod.SelectedIndex.ToString();
      string sTxt7=this.txtHeightParameter.Text;
      string sTxt8=this.txtServiceURL.Text;
      string sBool9 = this.chkReportResults.Checked.ToString();
      string sUnits10 = this.cboUnits.SelectedItem.ToString();

      Utils.WriteToRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" +
        sDesktopVers + "\\ArcMap\\Cadastral", "AddIn.FabricQualityControl_InverseDistance",
        sBool0 + "," + sTxt1 + "," + sBool2 + "," + sBool3 + "," + sTxt4 + "," + sBool5 + "," + sBool6 + "," + sTxt7 + "," + sTxt8 + "," + sBool9 + "," + sUnits10);
    }

    private void btnGetOffsetFromEditor_Click(object sender, EventArgs e)
    {
      if (m_EditorProperties == null)
        return;
      double dScale = m_EditorProperties.DistanceCorrectionFactor;
      string sScale = dScale.ToString("0.000000000");
      this.txtScaleFactor.Text = sScale;
    }

    private void btnGetScaleFromEditor_MouseHover(object sender, EventArgs e)
    {
      System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
      ToolTip1.SetToolTip(this.btnGetScaleFromEditor, btnGetScaleFromEditor.Tag.ToString());
    }
  }
}
