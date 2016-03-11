/*
 Copyright 1995-2016 Esri

 All rights reserved under the copyright laws of the United States.

 You may freely redistribute and use this sample code, with or without modification.

 Disclaimer: THE SAMPLE CODE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED 
 WARRANTIES, INCLUDING THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
 FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESRI OR 
 CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 INTERRUPTION) SUSTAINED BY YOU OR A THIRD PARTY, HOWEVER CAUSED AND ON ANY 
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT ARISING IN ANY 
 WAY OUT OF THE USE OF THIS SAMPLE CODE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 SUCH DAMAGE.

 For additional information contact: Environmental Systems Research Institute, Inc.

 Attn: Contracts Dept.

 380 New York Street

 Redlands, California, U.S.A. 92373 

 Email: contracts@esri.com
*/

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
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace ParcelFabricQualityControl
{
  public partial class InverseDistanceDlg : Form
  {
    private static IEditProperties2 m_EditorProperties;
    private static Dictionary<int, string> m_ElevationLayer2FieldNames;
    private static List<string> m_ElevationLayerNames;

    private static IMap m_Map;
    private static int m_ElevationFieldIndex = -1;
    private static IFeatureLayer m_ElevationFeatureLayer = null;
    private static System.Windows.Forms.ToolTip m_ToolTip1 = null;
    private static string m_sUnit = "m";
    private static string m_sInitialUnit = "m";

    public IFeatureLayer ElevationFeatureLayer
    {
      get
      {
        return m_ElevationFeatureLayer;
      }

    }

    public int ElevationFieldIndex
    {
      get
      {
        return m_ElevationFieldIndex;
      }
    }

    public InverseDistanceDlg(IEditProperties2 EditorProperties, IMap TheMap)
    {
      InitializeComponent();

      m_ToolTip1 = new System.Windows.Forms.ToolTip();

      m_EditorProperties = EditorProperties;
      m_Map = TheMap;

      m_ElevationLayer2FieldNames = new  Dictionary<int, string>();
      m_ElevationLayerNames = new List<string>();
      getElevationTableAndField(ref m_ElevationLayer2FieldNames, ref m_ElevationLayerNames);

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
      int k = 0;
      foreach (string FieldsList in m_ElevationLayer2FieldNames.Values)
      {
        string[] FieldName = FieldsList.Split(',');
        string sLayerName= m_ElevationLayerNames[k++];
        for (int i = 0; i < FieldName.Length; i++)
        {
          string sElevationSource = String.Format("[{0}] in layer: {1}", FieldName[i], sLayerName);
          cboElevField.Items.Add(sElevationSource);
        }
      }
      
      try
      {
        Point p = new Point();
        txtHeightParameter.Location = txtElevationLyr.Location;
        p.X = txtHeightParameter.Location.X + 10 + txtHeightParameter.Width;
        p.Y = txtHeightParameter.Location.Y;
        cboUnits.Location = p;

        Point p2 = new Point();
        p2.X = lblHeightInput.Location.X;
        p2.Y = txtElevationLyr.Location.Y;
        txtHeightParameter.Location = p2;

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
        
        this.txtElevationLyr.Text = "   " + Values[8];
        this.chkReportResults.Checked = (Values[9].Trim() == "True");

        m_sInitialUnit = m_sUnit = Values[10].Trim();
        if (m_sUnit != "m")
          cboUnits.SelectedItem = m_sUnit;

      }
      catch
      {}
    }

    private bool getElevationTableAndField(ref Dictionary<int, string> FeatureLayerIDAndFieldNamesList, ref List<string> ElevationLayerNames)
    {
      IMap map = ArcMap.Document.FocusMap;
      // get the elevation layers in the focus map
      int iLayerPos = 0; //relying on layer index
      IEnumLayer enumLayers = map.get_Layers(null, true);
      ILayer pLayer = enumLayers.Next();

      while (pLayer != null)
      {
        iLayerPos++;//use the TOC index
        if (pLayer is ICadastralFabricSubLayer2)
        {
          pLayer = enumLayers.Next();
          continue;
        }
        if (!(pLayer is IFeatureLayer))
        {//filter for feature layers only
          pLayer = enumLayers.Next();
          continue;
        }

        ILayerFields pLyrFlds = pLayer as ILayerFields;
        for (int i = 0; i < pLyrFlds.FieldCount; i++)
        {
          if (pLyrFlds.get_Field(i).Type == esriFieldType.esriFieldTypeDouble)
          {
            IFieldInfo pFldInfo = pLyrFlds.get_FieldInfo(i);
            if (!pFldInfo.Visible)
              continue;

            string sFieldName = pLyrFlds.get_Field(i).Name;
            if (sFieldName.ToLower().Contains("elevation") || sFieldName.ToLower() == ("z") || sFieldName.ToLower().Contains("height"))
            {
              if (!FeatureLayerIDAndFieldNamesList.ContainsKey(iLayerPos))
              {
                FeatureLayerIDAndFieldNamesList.Add(iLayerPos, sFieldName);
                ElevationLayerNames.Add(pLayer.Name);
              }
              else
                FeatureLayerIDAndFieldNamesList[iLayerPos] += "," + sFieldName;
            }
          }
        }
        pLayer = enumLayers.Next();
      }
      return false;
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

    private void txtDistDifferance_MouseHover(object sender, EventArgs e)
    {
      string sTip = txtDistDifference.Tag.ToString().Replace("/",Environment.NewLine);
      m_ToolTip1.SetToolTip(this.txtDistDifference, sTip);
    }

    private void optUserEnteredScaleFactor_CheckedChanged(object sender, EventArgs e)
    {
      cboElevField.Visible = false;
      cboScaleMethod.Enabled = btnChange.Enabled = optComputeForMe.Checked;

      txtHeightParameter.Enabled = optComputeForMe.Checked;
      cboUnits.Enabled = txtHeightParameter.Enabled;

      lblHeightInput.Enabled = txtHeightParameter.Enabled;
      btnUnits.Enabled = txtElevationLyr.Enabled = txtHeightParameter.Enabled;
      btnUnits.Visible = (cboScaleMethod.SelectedIndex == 1) && optComputeForMe.Checked;
    }

    private void chkDistanceDifference_CheckedChanged(object sender, EventArgs e)
    {
      txtDistDifference.Enabled = chkDistanceDifference.Checked;
    }

    private void chkApplyScaleFactor_CheckedChanged(object sender, EventArgs e)
    {
      panel1.Enabled = chkApplyScaleFactor.Checked;
      txtScaleFactor.Enabled = optUserEnteredScaleFactor.Checked;
      cboScaleMethod.Enabled = !optUserEnteredScaleFactor.Checked;
    }

    private void optComputeForMe_CheckedChanged(object sender, EventArgs e)
    {
      cboScaleMethod.Enabled = btnChange.Enabled = optComputeForMe.Checked;
      txtScaleFactor.Enabled = btnGetScaleFromEditor.Enabled = !cboScaleMethod.Enabled;
      txtHeightParameter.Enabled = optComputeForMe.Enabled;
      cboUnits.Enabled = optComputeForMe.Enabled;
      btnUnits.Visible = (cboScaleMethod.SelectedIndex == 1) && optComputeForMe.Checked;
    }

    private void InverseDistanceDlg_Load(object sender, EventArgs e)
    {
      txtHeightParameter.Enabled = true;
      lblHeightInput.Enabled = true;
      btnUnits.Enabled = txtElevationLyr.Enabled = true;
      button1.Enabled = true;
    }

    private void cboScaleMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cboScaleMethod.SelectedIndex == 0)
      {
        txtHeightParameter.Visible = true;
        cboUnits.Visible = true;
        cboUnits.Enabled = txtHeightParameter.Enabled;
        Point p2 = new Point();
        p2.X=txtHeightParameter.Location.X+10+txtHeightParameter.Width;
        p2.Y=txtHeightParameter.Location.Y;
        cboUnits.Location = p2;
        cboUnits.SelectedItem = m_sUnit;
        btnChange.Visible = lblHeightInput.Visible = txtElevationLyr.Visible = false;
        button1.Enabled = true;
        lblHeightInput.Enabled = true;
        btnUnits.Visible = false;
      }
      else
      {
        Point p = new Point();
        p.X = txtHeightParameter.Location.X + lblHeightInput.Width;
        p.Y = txtHeightParameter.Location.Y;
        txtElevationLyr.Location = p;

        Point p2 = new Point();
        p2.X = txtElevationLyr.Location.X;
        p2.Y = txtElevationLyr.Location.Y;
        btnUnits.Location =p2;

        lblHeightInput.Visible = btnUnits.Visible = txtElevationLyr.Visible = true;
        txtHeightParameter.Visible = false;
        cboUnits.SelectedItem = m_sUnit;
        cboUnits.Visible = false;
        btnChange.Visible = true;
        button1.Enabled = txtElevationLyr.Text.Contains("in layer:");
      }
    }

    private void btnChange_Click(object sender, EventArgs e)
    {
      cboElevField.Width = txtElevationLyr.Width;
      cboElevField.Location=txtElevationLyr.Location;
      cboElevField.BringToFront();
      cboElevField.Visible = true;
      cboElevField.DroppedDown = true;
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
      string sTxt8=this.txtElevationLyr.Text.Trim();
      string sBool9 = this.chkReportResults.Checked.ToString();
      string sUnits10 = this.cboUnits.SelectedItem.ToString();

      Utils.WriteToRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" +
        sDesktopVers + "\\ArcMap\\Cadastral", "AddIn.FabricQualityControl_InverseDistance",
        sBool0 + "," + sTxt1 + "," + sBool2 + "," + sBool3 + "," + sTxt4 + "," + sBool5 + "," + sBool6 + "," + sTxt7 + "," + sTxt8 + "," + sBool9 + "," + sUnits10);

      try
      {
        string[] sElevFldInLayer = sTxt8.TrimStart('[').Replace("] in layer: ", ",").Split(',');
        if (sElevFldInLayer.Length == 2)
        {
          int iLayerIdx = m_ElevationLayer2FieldNames.Keys.ToArray()[m_ElevationLayerNames.FindIndex(a => a == sElevFldInLayer[1])];
          IEnumLayer LyrEnum = m_Map.get_Layers(null, true); //use same paraamters as original load to ensure matching index and layer position
          LyrEnum.Reset();
          int iLyrPos = 1; //layer position starting at 1
          ILayer pLyr = LyrEnum.Next();
          while (pLyr!= null)
          {
            if (iLayerIdx==iLyrPos)
            {
              IFeatureLayer pFeatLyr = pLyr as IFeatureLayer; //should be a feature layer, by original load position 
              m_ElevationFieldIndex = pFeatLyr.FeatureClass.Fields.FindField(sElevFldInLayer[0]);
              if (m_ElevationFieldIndex > -1)
                m_ElevationFeatureLayer = pFeatLyr;
              break;
            }
            pLyr = LyrEnum.Next();
            iLyrPos++;
          }
        }
      }
      catch
      {
        m_ElevationFeatureLayer = null;
        m_ElevationFieldIndex = -1;
      }

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
      m_ToolTip1.SetToolTip(this.btnGetScaleFromEditor, btnGetScaleFromEditor.Tag.ToString());
    }

    private void button3_Click(object sender, EventArgs e)
    {
      optComputeForMe.Checked = true;
      cboScaleMethod.SelectedIndex= 0;
      cboUnits.SelectedItem = 0;
      chkDistanceDifference.Checked = chkReportResults.Checked = chkApplyScaleFactor.Checked = true;
      txtScaleFactor.Text = "1.0000000";
      txtHeightParameter.Text = "0.00";
      txtDistDifference.Text = "0.5";
      txtDistDifference.Text = lblDistanceUnits1.Text.ToLower().Contains("meter") ? "0.5" : "1.5";
    }

    private void cboElevField_SelectedIndexChanged(object sender, EventArgs e)
    {
      txtElevationLyr.Text = "   " + cboElevField.SelectedItem.ToString();
      cboElevField.Visible = false;
      button1.Enabled = txtElevationLyr.Text.Contains("in layer:");
    }

    private void txtElevationLyr_MouseHover(object sender, EventArgs e)
    {
      m_ToolTip1.SetToolTip(this.txtElevationLyr, txtElevationLyr.Text.Trim());
    }

    private void cboElevField_DropDownClosed(object sender, EventArgs e)
    {
      cboElevField.Visible = false;
    }

    private void btnUnits_MouseHover(object sender, EventArgs e)
    {
      m_ToolTip1.SetToolTip(this.btnUnits, "Units: " + m_sUnit);//cboUnits.SelectedItem.ToString());
    }

    private void btnUnits_Click(object sender, EventArgs e)
    {
      cboUnits.Enabled = txtElevationLyr.Enabled;
      Point p2 = new Point();
      p2.X = btnUnits.Location.X;
      p2.Y = btnUnits.Location.Y + btnUnits.Height;
      cboUnits.Location = p2;
      cboUnits.SelectedItem = m_sUnit;
      cboUnits.DroppedDown = true;
      cboUnits.Visible = true;
      cboUnits.SelectedItem = m_sUnit;
    }

    private void cboUnits_DropDownClosed(object sender, EventArgs e)
    {
      cboUnits.Visible = !btnUnits.Visible;
      m_sUnit=cboUnits.SelectedItem.ToString();
    }

    private void btnChange_MouseHover(object sender, EventArgs e)
    {
      m_ToolTip1.SetToolTip(this.btnChange, "Change Elevation Source");
    }

    private void button2_Click(object sender, EventArgs e)
    {
      //Cancel button, so reset the elevation unit on the combo box to the initial value
      cboUnits.SelectedItem = m_sInitialUnit;
    }
  }
}
