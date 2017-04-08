/*
 Copyright 1995-2017 Esri

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
  public partial class InterpolateZDlg : Form
  {
    private static IEditProperties2 m_EditorProperties;
    private static Dictionary<int, string> m_ElevationLayer2FieldNames;
    private static List<string> m_ElevationLayerNames;

    private static Dictionary<int, string> m_TinLayer2FieldNames;
    private static List<string> m_TinLayerNames;

    private static Dictionary<int, string> m_RasterLayer2FieldNames;
    private static List<string> m_RasterLayerNames;

    private static IMap m_Map;
    private static int m_ElevationFieldIndex = -1;
    private static int m_RasterBandIndexOnDEM = -1;

    private static IFeatureLayer m_ElevationFeatureLayer = null;
    private static ITinLayer2 m_TIN_Layer = null;
    private static IRasterLayer m_DEM_RasterLayer = null;

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

    public ITinLayer2 TINLayer
    {
      get
      {
        return m_TIN_Layer;
      }
    }

    public IRasterLayer DEMRasterLayer
    {
      get
      {
        return m_DEM_RasterLayer;
      }
    }


    public int ElevationFieldIndex
    {
      get
      {
        return m_ElevationFieldIndex;
      }
    }

    public int RasterBandIndexOnDEM
    {
      get
      {
        return m_RasterBandIndexOnDEM;
      }
    }


    private bool nonNumberEntered = false;

    public InterpolateZDlg(IEditProperties2 EditorProperties, IMap TheMap)
    {
      InitializeComponent();

      m_ToolTip1 = new System.Windows.Forms.ToolTip();

      m_EditorProperties = EditorProperties;
      m_Map = TheMap;

      m_TinLayer2FieldNames = new Dictionary<int, string>();
      m_TinLayerNames = new List<string>();

      Utilities Utils = new Utilities();
      Utils.getTINLayer(ref m_TinLayer2FieldNames, ref m_TinLayerNames);

      m_RasterLayer2FieldNames = new Dictionary<int, string>();
      m_RasterLayerNames = new List<string>();
      Utils.getRasterDEMLayer(ref m_RasterLayer2FieldNames, ref m_RasterLayerNames);

      cboLayerNameTIN.Items.Add("<None>");
      cboLayerNameTIN.SelectedIndex=0;

      cboLayerNameDEM.Items.Add("<None>");
      cboLayerNameDEM.SelectedIndex = 0;

      if (m_TinLayerNames.Count > 0)
        foreach (string sLayer in m_TinLayerNames)
          cboLayerNameTIN.Items.Add(sLayer);

      if (m_RasterLayerNames.Count > 0)
        foreach (string sLayer in m_RasterLayerNames)
          cboLayerNameDEM.Items.Add(sLayer);


      string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
      if (sDesktopVers.Trim() == "")
        sDesktopVers = "Desktop10.1";
      else
        sDesktopVers = "Desktop" + sDesktopVers;
      string sValues =
      Utils.ReadFromRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral",
        "AddIn.FabricQualityControl_InterpolateZ");
      if (sValues.Trim() == "")
        sValues = "True,False,0,m,1,<None>,False,0,True"; //Defaults
      string[] Values = sValues.Split(',');
      int k = 0;

      try
      {
        Point p = new Point();
        txtHeightParameter.Location = txtElevationLyr.Location;
        p.X = txtHeightParameter.Location.X + 10 + txtHeightParameter.Width;
        p.Y = txtHeightParameter.Location.Y;
        cboUnits.Location = p;


        cboUnits.SelectedItem = "m"; //default to meters, but change later if needed
        txtHeightParameter.Visible = false;
        cboUnits.Visible = txtHeightParameter.Visible;
        
        btnChange.Top = txtElevationLyr.Top;
        btnChange.Left = txtElevationLyr.Right + 3;

        this.optClearElevations.Checked = (Values[0].Trim() == "True");
        this.optAssignZValues.Checked = (Values[1].Trim() == "True");
        this.txtHeightParameter.Text =Values[2];
        m_sInitialUnit = m_sUnit = Values[3].Trim();
        if (m_sUnit != "m")
          cboUnits.SelectedItem = m_sUnit;
        try {this.cboElevationSource.SelectedIndex = Convert.ToInt32(Values[4]);}
        catch { this.cboElevationSource.SelectedIndex = 0; }
        this.txtElevationLyr.Text = Values[5];

        this.chkElevationDifference.Checked = (Values[6].Trim() == "True");
        this.txtElevationDifference.Text = Values[7];

        this.chkReportResults.Checked = (Values[8].Trim() == "True");

        if (this.cboElevationSource.SelectedIndex == 1 && m_TinLayerNames.Count > 0) //TIN layer
          this.txtElevationLyr.Text = m_TinLayerNames[0];
        else if (this.cboElevationSource.SelectedIndex == 2 && m_RasterLayerNames.Count > 0) //DEM layer
          this.txtElevationLyr.Text = m_RasterLayerNames[0];


        if (this.cboElevationSource.SelectedIndex > 0)
          this.button1.Enabled = !txtElevationLyr.Text.Contains("<None>");

        btnUnits.Visible = (cboElevationSource.SelectedIndex == 1) && !txtElevationLyr.Text.Contains("<None>");
        optClearElevations_CheckedChanged(null, null);

      }
      catch
      {
        this.cboElevationSource.SelectedIndex = 0;
      
      }

    }

    private void InterpolateZDlg_Load(object sender, EventArgs e)
    {

      lblHeightInput.Enabled = true;
      btnUnits.Enabled = txtElevationLyr.Enabled = true;
    }

    private void optClearElevations_CheckedChanged(object sender, EventArgs e)
    {
      cboElevationSource.Enabled  =txtElevationLyr.Enabled=btnUnits.Enabled=btnChange.Enabled  = 
        cboUnits.Enabled = txtHeightParameter.Enabled = lblHeightInput.Enabled =! optClearElevations.Checked;

      chkElevationDifference.Enabled = txtElevationDifference.Enabled = !optClearElevations.Checked;
      if (optClearElevations.Checked)
        button1.Enabled = true;
      else
        optAssignZValues_CheckedChanged(null, null);
    }

    private void optAssignZValues_CheckedChanged(object sender, EventArgs e)
    {
      chkElevationDifference.Enabled = lblElevationUnits.Enabled = cboElevationSource.Enabled = optAssignZValues.Checked;
      txtElevationDifference.Enabled = chkElevationDifference.Checked;
      cboElevationSource_SelectedIndexChanged(sender, e);
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

      string sBool0 = this.optClearElevations.Checked.ToString();
      string sBool1 = this.optAssignZValues.Checked.ToString();
      string sTxt2 = this.txtHeightParameter.Text;
      string sUnits3 = this.cboUnits.SelectedItem.ToString();
      string sElevSrc4 = this.cboElevationSource.Text.Trim();
      string sTxt5 = this.txtElevationLyr.Text.Trim();
      string sBool6 = this.chkElevationDifference.Checked.ToString();
      string sTxt7 = this.txtElevationDifference.Text.Trim();
      string sBool8 = this.chkReportResults.Checked.ToString();
      

      Utils.WriteToRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" +
        sDesktopVers + "\\ArcMap\\Cadastral", "AddIn.FabricQualityControl_InterpolateZ",
        sBool0 + "," + sBool1 + "," + sTxt2 + "," + sUnits3 + "," + sElevSrc4 + "," + sTxt5 + "," + sBool6 + "," + sTxt7 + "," + sBool8);

      try
      {
        string[] sElevFldInLayer = sTxt5.TrimStart('[').Replace("] in layer ", ",").Split(',');
        if (sElevFldInLayer.Length == 2 || sElevFldInLayer.Length == 1)
        {
          int iLayerIdx = -1;
          if (sElevFldInLayer.Length == 2)
            iLayerIdx = m_ElevationLayer2FieldNames.Keys.ToArray()[m_ElevationLayerNames.FindIndex(a => a == sElevFldInLayer[1])];

          IEnumLayer LyrEnum = m_Map.get_Layers(null, true); //use same paraamters as original load to ensure matching index and layer position
          LyrEnum.Reset();
          int iLyrPos = 1; //layer position starting at 1
          ILayer pLyr = LyrEnum.Next();
          while (pLyr != null)
          {
            if (iLayerIdx == iLyrPos)
            {
              if (pLyr is IFeatureLayer)
              {
                IFeatureLayer pFeatLyr = pLyr as IFeatureLayer; //should be a feature layer, by original load position 
                m_ElevationFieldIndex = pFeatLyr.FeatureClass.Fields.FindField(sElevFldInLayer[0]);
                if (m_ElevationFieldIndex > -1)
                  m_ElevationFeatureLayer = pFeatLyr;
                break;
              }
            }
            else if (pLyr is IRasterLayer)
            {
              if (pLyr.Name == sElevFldInLayer[0])
              {
                m_DEM_RasterLayer = pLyr as IRasterLayer; //should be a Raster layer, by original load position
                break;
              }
            }

            else if (pLyr is ITinLayer2)
            {
              if (pLyr.Name == sElevFldInLayer[0])
              {
                m_TIN_Layer = pLyr as ITinLayer2; //should be a TIN layer, by original load position
                break;
              }
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
      cboUnits.BringToFront();
      cboUnits.SelectedItem = m_sUnit;
    }

    private void btnUnits_MouseHover(object sender, EventArgs e)
    {
      m_ToolTip1.SetToolTip(this.btnUnits, "Units: " + m_sUnit);
    }

    private void cboElevationSource_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cboElevationSource.SelectedIndex == 1 && m_TinLayerNames.Count > 0)
        txtElevationLyr.Text = m_TinLayerNames[0];
      else if (cboElevationSource.SelectedIndex == 2 && m_RasterLayerNames.Count > 0)
        txtElevationLyr.Text = m_RasterLayerNames[0];
      else
        txtElevationLyr.Text = "<None>";

      txtHeightParameter.Visible = cboUnits.Visible = cboElevationSource.SelectedIndex == 0;
      if (cboElevationSource.SelectedIndex == 0)
      {
        lblHeightInput.Text = "Height:";
        Point p = new Point();
        txtHeightParameter.Location = txtElevationLyr.Location;
        p.X = txtHeightParameter.Location.X + 10 + txtHeightParameter.Width;
        p.Y = txtHeightParameter.Location.Y;
        cboUnits.Location = p;
      }
      else
        lblHeightInput.Text = "Layer:";
      btnChange.Visible = txtElevationLyr.Visible = (cboElevationSource.SelectedIndex != 0);
      
      button1.Enabled = ((cboElevationSource.SelectedIndex != 0) && !txtElevationLyr.Text.Contains("<None>")) || 
               ((cboElevationSource.SelectedIndex == 0) && (txtHeightParameter.Text.Trim().Length > 0));

      if (chkElevationDifference.Checked && optAssignZValues.Checked)
        button1.Enabled = txtElevationDifference.TextLength > 0;

      btnUnits.Visible = (cboElevationSource.SelectedIndex != 0) && optAssignZValues.Checked && !txtElevationLyr.Text.Contains("<None>");
      btnChange.Enabled = !txtElevationLyr.Text.Contains("<None>");

    }

    private void cboUnits_DropDownClosed(object sender, EventArgs e)
    {
      cboUnits.Visible = !btnUnits.Visible;
      m_sUnit = cboUnits.SelectedItem.ToString();
    }

    private void btnChange_Click(object sender, EventArgs e)
    {
      cboLayerNameTIN.Width = cboLayerNameDEM.Width = txtElevationLyr.Width;
      cboLayerNameTIN.Location = cboLayerNameDEM.Location = txtElevationLyr.Location;
      if (cboElevationSource.SelectedIndex == 1)
      {
        cboLayerNameTIN.BringToFront();
        cboLayerNameTIN.Visible = true;
        cboLayerNameTIN.DroppedDown = true;
      }
      else if (cboElevationSource.SelectedIndex == 2)
      {
        cboLayerNameDEM.BringToFront();
        cboLayerNameDEM.Visible = true;
        cboLayerNameDEM.DroppedDown = true;
      }
    }

    private void cboLayerNameTIN_SelectedIndexChanged(object sender, EventArgs e)
    {
      txtElevationLyr.Text = "   " + cboLayerNameTIN.SelectedItem.ToString();
      button1.Enabled = !txtElevationLyr.Text.Contains("<None>");
      btnUnits.Visible = !txtElevationLyr.Text.Contains("<None>");
    }

    private void cboLayerNameTIN_DropDownClosed(object sender, EventArgs e)
    {
      cboLayerNameTIN.Visible = false;
    }

    private void cboLayerNameTIN_MouseHover(object sender, EventArgs e)
    {
      m_ToolTip1.SetToolTip(this.btnChange, "Change Elevation Source");
    }

    private void cboLayerNameDEM_SelectedIndexChanged(object sender, EventArgs e)
    {
      txtElevationLyr.Text = "   " + cboLayerNameDEM.SelectedItem.ToString();
      button1.Enabled = !txtElevationLyr.Text.Contains("<None>");
      btnUnits.Visible = !txtElevationLyr.Text.Contains("<None>");
    }

    private void cboLayerNameDEM_DropDownClosed(object sender, EventArgs e)
    {
      cboLayerNameDEM.Visible = false;
    }

    private void cboLayerNameDEM_MouseHover(object sender, EventArgs e)
    {
      m_ToolTip1.SetToolTip(this.btnChange, "Change Elevation Source");
    }

    private void txtHeightParameter_TextChanged(object sender, EventArgs e)
    {
      txtChanged();
    }

    private void txtChanged()
    {
      button1.Enabled = (txtHeightParameter.TextLength > 0);
    }

    private void txtBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      // Check for the flag being set in the KeyDown event.
      if (nonNumberEntered == true)
      {
        // Stop the character from being entered into the control since it is non-numerical.
        e.Handled = true;
        return;
      }
    }

    private void txtBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyValue == 110 || e.KeyValue == 190)
      {
        //test for other cases of 2 decimal points
        TextBox txtB = (TextBox)sender;
        string txtBxString = "." + txtB.Text;
        double d = 0;
        if (!Double.TryParse(txtBxString, out d))
        {
          nonNumberEntered = true;
          return;
        }
      }

      // Initialize the flag to false.
      nonNumberEntered = false;
      // Determine whether the keystroke is a number from the top of the keyboard.
      if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
      {
        // Determine whether the keystroke is a number from the keypad.
        if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
        {
          // Determine whether the keystroke is a backspace.
          if ((e.KeyCode != Keys.Back) && (e.KeyCode != Keys.Decimal) && (e.KeyCode != Keys.OemPeriod))
          {
            // A non-numerical keystroke was pressed.
            // Set the flag to true and evaluate in KeyPress event.
            nonNumberEntered = true;
          }
        }
      }
    }

    private void txtHeightParameter_KeyDown(object sender, KeyEventArgs e)
    {
      txtBox_KeyDown(sender,e);
    }

    private void txtHeightParameter_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBox_KeyPress(sender, e);
    }

    private void txtElevationDifference_TextChanged(object sender, EventArgs e)
    {
      button1.Enabled = (txtElevationDifference.TextLength > 0);
    }

    private void chkElevationDifference_CheckedChanged(object sender, EventArgs e)
    {
      txtElevationDifference.Enabled = chkElevationDifference.Checked;
      button1.Enabled = (chkElevationDifference.Checked && txtElevationDifference.TextLength > 0) || !chkElevationDifference.Checked;
      if (button1.Enabled)
        button1.Enabled = (txtHeightParameter.Enabled && txtHeightParameter.TextLength > 0);

    }

    private void txtElevationDifference_KeyDown(object sender, KeyEventArgs e)
    {
      txtBox_KeyDown(sender, e);
    }

    private void txtElevationDifference_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBox_KeyPress(sender, e);
    }

    private void cboUnits_SelectedIndexChanged(object sender, EventArgs e)
    {
      lblElevationUnits.Text = cboUnits.Text;
    }

  }
}
