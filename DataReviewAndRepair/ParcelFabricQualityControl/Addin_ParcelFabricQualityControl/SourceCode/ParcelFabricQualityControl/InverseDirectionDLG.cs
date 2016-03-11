using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;

namespace ParcelFabricQualityControl
{
  public partial class InverseDirectionDLG : Form
  {
    static public string AssemblyDirectory
    {
      get
      {
        string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return System.IO.Path.GetDirectoryName(path);
      }
    }
    private IEditProperties2 m_EditorProperties;
    private static System.Windows.Forms.ToolTip m_ToolTip1 = null;

    public InverseDirectionDLG(IEditProperties2 EditorProperties)
    {
      InitializeComponent();
      m_ToolTip1 = new System.Windows.Forms.ToolTip();
      m_EditorProperties = EditorProperties;
      Utilities Utils = new Utilities();

      string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
      if (sDesktopVers.Trim() == "")
        sDesktopVers = "Desktop10.1";
      else
        sDesktopVers = "Desktop" + sDesktopVers;
      string sValues =
      Utils.ReadFromRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral",
        "AddIn.FabricQualityControl_InverseDirection");
      
      IAngularConverter pAngConv = new AngularConverterClass();
      if (m_EditorProperties != null)
      {
        esriDirectionUnits pUnits = m_EditorProperties.DirectionUnits;
        string sAngleUnits = pUnits.ToString();
        sAngleUnits = sAngleUnits.Replace("esriDU", "");
        sAngleUnits = sAngleUnits.Replace("Minutes", " Minutes ");
        sAngleUnits = sAngleUnits.Replace("Decimal", "Decimal ");
        this.lblAngleUnits.Text = sAngleUnits;
      }
      
      if (sValues.Trim() == "")
        return;
      string[] Values = sValues.Split(',');
      try
      {
        string sTxt1 = Values[1];
        if (m_EditorProperties != null)
        {
          int iPrec = m_EditorProperties.AngularUnitPrecision;
          pAngConv.SetString(Values[1], esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees); //registry always stores in DD
          sTxt1 = pAngConv.GetString(esriDirectionType.esriDTNorthAzimuth, m_EditorProperties.DirectionUnits, m_EditorProperties.AngularUnitPrecision);
        }

        this.optManualEnteredDirnOffset.Checked = (Values[0].Trim() == "True");
        this.txtDirectionOffset.Text = sTxt1;
        this.optComputeDirnOffset.Checked = (Values[2].Trim() == "True");
        this.chkDirectionDifference.Checked = (Values[3].Trim() == "True");
        this.txtDirectionDifference.Text = Values[4];
        this.chkSubtendedDistance.Checked = (Values[5].Trim() == "True");
        this.txtSubtendedDist.Text = Values[6];
        this.chkReportResults.Checked = (Values[7].Trim() == "True");
      }
      catch
      { }

    }

    private void chkDirectionDifference_CheckedChanged(object sender, EventArgs e)
    {
      txtDirectionDifference.Enabled = chkDirectionDifference.Checked;
    }

    private void chkSubtendedDistance_CheckedChanged(object sender, EventArgs e)
    {
      txtSubtendedDist.Enabled = chkSubtendedDistance.Checked;
    }

    private void optManualEnteredDirnOffset_CheckedChanged(object sender, EventArgs e)
    {
      btnGetOffsetFromEditor.Enabled = txtDirectionOffset.Enabled  = optManualEnteredDirnOffset.Checked;
    }

    private void optComputeDirnOffset_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void btnGetOffsetFromEditor_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/GetOffsetFromEditor.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = btnGetOffsetFromEditor.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void InverseDirectionDLG_Load(object sender, EventArgs e)
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

      string sBool0 = this.optManualEnteredDirnOffset.Checked.ToString();
      string sTxt1 = this.txtDirectionOffset.Text;

      IAngularConverter pAngConv = new AngularConverterClass();
      if (!pAngConv.SetString(sTxt1, esriDirectionType.esriDTNorthAzimuth, m_EditorProperties.DirectionUnits))
        sTxt1 = "0";
      else
        sTxt1 = pAngConv.GetString(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees, 6);//always write as decimal deg
      string sBool2 = this.optComputeDirnOffset.Checked.ToString();
      string sBool3 = this.chkDirectionDifference.Checked.ToString();
      string sTxt4 = this.txtDirectionDifference.Text;
      string sBool5 = this.chkSubtendedDistance.Checked.ToString();
      string sTxt6 = this.txtSubtendedDist.Text;
      string sBool7 = this.chkReportResults.Checked.ToString();

      Utils.WriteToRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" +
        sDesktopVers + "\\ArcMap\\Cadastral", "AddIn.FabricQualityControl_InverseDirection",
        sBool0 + "," + sTxt1 + "," + sBool2 + "," + sBool3 + "," + sTxt4 + "," + sBool5 + "," + sTxt6 + "," + sBool7);

    }

    private void btnGetOffsetFromEditor_MouseHover(object sender, EventArgs e)
    {
      m_ToolTip1.SetToolTip(this.btnGetOffsetFromEditor, btnGetOffsetFromEditor.Tag.ToString());
    }

    private void btnGetOffsetFromEditor_Click(object sender, EventArgs e)
    {
      if (m_EditorProperties == null)
        return;
      IAngularConverter pAngConv = new AngularConverterClass();
      double dCorr = m_EditorProperties.AngularCorrectionOffset;
      pAngConv.SetAngle(dCorr, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);
      string sCorr = pAngConv.GetString(esriDirectionType.esriDTNorthAzimuth, m_EditorProperties.DirectionUnits, m_EditorProperties.AngularUnitPrecision);
      this.txtDirectionOffset.Text = sCorr;
    }

    private void btnResetDefaults_Click(object sender, EventArgs e)
    {
      optComputeDirnOffset.Checked = true;
      IAngularConverter pAngConv = new AngularConverterClass();
      pAngConv.SetAngle(0,m_EditorProperties.DirectionType, m_EditorProperties.DirectionUnits);
      txtDirectionOffset.Text = pAngConv.GetString(m_EditorProperties.DirectionType, m_EditorProperties.DirectionUnits,m_EditorProperties.AngularUnitPrecision);
      chkDirectionDifference.Checked = chkSubtendedDistance.Checked = chkReportResults.Checked = true;
      txtDirectionDifference.Text = "180";
      txtSubtendedDist.Text = lblDistanceUnits1.Text.ToLower().Contains("meter")? "0.1":"0.3";
    }
  }
}
