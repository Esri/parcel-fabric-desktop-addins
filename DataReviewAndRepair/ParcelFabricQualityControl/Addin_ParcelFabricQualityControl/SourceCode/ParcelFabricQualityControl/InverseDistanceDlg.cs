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
    private static Dictionary<int, IArray> m_ElevationLayer2Fields;
    private static Dictionary<int, string> m_ElevationLayer2FieldNames;
    private static List<string> m_ElevationLayerNames;

    private static IMap m_Map;
    public InverseDistanceDlg(IEditProperties2 EditorProperties, IMap TheMap)
    {
      InitializeComponent();
      m_EditorProperties = EditorProperties;
      m_Map = TheMap;

      m_ElevationLayer2Fields = new Dictionary<int, IArray>();
      m_ElevationLayer2FieldNames = new  Dictionary<int, string>();
      m_ElevationLayerNames = new List<string>();
      getElevationTableAndField(ref m_ElevationLayer2Fields, ref m_ElevationLayer2FieldNames, ref m_ElevationLayerNames);

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
        
        this.txtElevationLyr.Text = Values[8];
        this.chkReportResults.Checked = (Values[9].Trim() == "True");

        string sUnit = Values[10].Trim();
        if (sUnit!="m")
          cboUnits.SelectedItem = sUnit;

      }
      catch
      {}
    }

    private bool getElevationTableAndField(ref Dictionary<int, IArray> FeatureLayerIDAndFieldArray,
                                    ref Dictionary<int, string> FeatureLayerIDAndFieldNamesList, ref List<string> ElevationLayerNames)
    {
      int FieldIndex = -1;
      IMap map = ArcMap.Document.FocusMap;
      // get the elevation feature layers in the focus map
      UID uid = new UID();
      uid.Value = "{40A9E885-5533-11d0-98BE-00805F7CED21}";
      int iLayerPos = 0;
      IEnumLayer enumLayers = map.get_Layers(uid, true);
      IFeatureLayer pFeatLayer = enumLayers.Next() as IFeatureLayer;

      while (pFeatLayer != null)
      {
        if (pFeatLayer is ICadastralFabricSubLayer2)
        {
          pFeatLayer = enumLayers.Next() as IFeatureLayer;
          continue;
        }
        ILayerFields pLyrFlds = pFeatLayer as ILayerFields;
        iLayerPos++;//use the TOC index
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
              IArray FldArray = null;
              if (!FeatureLayerIDAndFieldArray.ContainsKey(iLayerPos))
              {
                FldArray = new ESRI.ArcGIS.esriSystem.ArrayClass();
                FldArray.Add(pLyrFlds.get_Field(i));
                FeatureLayerIDAndFieldArray.Add(iLayerPos, FldArray);
                FeatureLayerIDAndFieldNamesList.Add(iLayerPos, sFieldName);
                ElevationLayerNames.Add(pFeatLayer.Name);
              }
              else
              {
                FldArray = FeatureLayerIDAndFieldArray[iLayerPos];
                FldArray.Add(pLyrFlds.get_Field(i));
                FeatureLayerIDAndFieldNamesList[iLayerPos] += "," + sFieldName;
              }

              FieldIndex = i;
            }
          }
        }
        pFeatLayer = enumLayers.Next() as IFeatureLayer;
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
      System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
      ToolTip1.SetToolTip(this.txtDistDifference, sTip);
    }

    private void optUserEnteredScaleFactor_CheckedChanged(object sender, EventArgs e)
    {
      cboElevField.Visible = false;
      cboScaleMethod.Enabled = btnChange.Enabled = optComputeForMe.Checked;
      txtHeightParameter.Enabled = optComputeForMe.Checked;
      cboUnits.Enabled = txtHeightParameter.Enabled;

      lblHeightInput.Enabled = txtHeightParameter.Enabled;
      txtElevationLyr.Enabled = txtHeightParameter.Enabled;

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

    }

    private void InverseDistanceDlg_Load(object sender, EventArgs e)
    {
      txtHeightParameter.Enabled = true;
      lblHeightInput.Enabled = true;
      txtElevationLyr.Enabled = true;
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
        cboUnits.SelectedItem = "m";
        btnChange.Visible = lblHeightInput.Visible = txtElevationLyr.Visible = false;
        button1.Enabled = true;

        txtElevationLyr.Enabled = true;
        lblHeightInput.Enabled = true;
      }
      else
      {
        Point p = new Point();
        p.X = txtHeightParameter.Location.X + lblHeightInput.Width;
        p.Y = txtHeightParameter.Location.Y;
        txtElevationLyr.Location = p;

        lblHeightInput.Visible= txtElevationLyr.Visible = true;
        txtHeightParameter.Visible = false;
        cboUnits.Visible = false;
        btnChange.Visible = true;
        button1.Enabled = txtElevationLyr.Text.Contains("in layer:");
      }
    }

    private void btnChange_Click(object sender, EventArgs e)
    {
      cboElevField.Width = txtElevationLyr.Width;
      cboElevField.Location=txtElevationLyr.Location;
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
      string sTxt8=this.txtElevationLyr.Text;
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

    private void button3_Click(object sender, EventArgs e)
    {
      chkDistanceDifference.Checked = true;
      txtDistDifference.Text = "0.5";
      chkReportResults.Checked = true;

    }

    private void cboElevField_SelectedIndexChanged(object sender, EventArgs e)
    {
      txtElevationLyr.Text = cboElevField.SelectedItem.ToString();
      cboElevField.Visible = false;
      button1.Enabled = txtElevationLyr.Text.Contains("in layer:");
    }

    private void txtElevationLyr_MouseHover(object sender, EventArgs e)
    {
      System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
      ToolTip1.SetToolTip(this.txtElevationLyr, txtElevationLyr.Text);
    }

    private void cboElevField_DropDownClosed(object sender, EventArgs e)
    {
      cboElevField.Visible = false;
    }
  }
}
