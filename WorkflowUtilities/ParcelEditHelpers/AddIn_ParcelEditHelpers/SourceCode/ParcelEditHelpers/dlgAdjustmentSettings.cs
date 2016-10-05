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

namespace ParcelEditHelper
{
  public partial class dlgAdjustmentSettings : Form
  {
    public dlgAdjustmentSettings()
    {
      InitializeComponent();
      LoadValuesFromRegistry();
    }

    private bool nonNumberEntered = false;
    
    private void LoadValuesFromRegistry()
    {
      Utilities FabUTILS = new Utilities();
      lblUnits1.Text=FabUTILS.UnitNameFromSpatialReference(ArcMap.Document.ActiveView.FocusMap.SpatialReference);
      lblUnits2.Text = lblUnits3.Text = lblUnits5.Text = lblUnits6.Text 
        = lblUnits7.Text = lblUnits8.Text = lblUnits9.Text = lblUnits1.Text;

      try
      {
        string sVersion = FabUTILS.GetDesktopVersionFromRegistry();

      string sSelectedPage= FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSASettingsSelectedPage", false);

      this.tabControl1.SelectedIndex=Convert.ToInt32(sSelectedPage);

       string sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSADistanceToleranceReport", true);
       if (sVal.Trim()!="")
         this.txtDistTolerance.Text =sVal;

      sVal =  FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSABearingToleranceReport", false);
      if (sVal.Trim()!="")
        this.txtBearingTolerance.Text=sVal;

      sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSALinePointsOffsetToleranceReport", true);
      if (sVal.Trim()!="")
        this.txtLinePtsOffsetTolerance.Text=sVal;

      sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAClosePointsToleranceReport", true);
       if (sVal.Trim()!="")
         this.txtClosePointsTolerance.Text=sVal;

      sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSANumberRepeatCountIteration", false);
      if (sVal.Trim() != "")
        this.numRepeatCount.Value= Convert.ToDecimal(sVal);

      sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAConvergenceValue", true);
      if (sVal.Trim() != "")
        this.txtConvergenceValue.Text=sVal;

      sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSADivergenceValue", true);
      if (sVal.Trim() != "")
        this.txtDivergenceValue.Text=sVal;

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAReportType", false);
      if (sVal.Trim() != "")
        this.cboReportType.SelectedIndex=Convert.ToInt32(sVal);

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSABrowseFilePath", false);
      if (sVal.Trim() != "")
        this.txtBrowseFilePath.Text=sVal;

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkBendLinesOn", false);
      if (sVal.Trim() != "")
        this.chkBendLines.Checked=((sVal.ToUpper().Trim())=="TRUE");

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSABendLinesTolerance", true);
      if (sVal.Trim() != "")
        this.txtBendLinesTolerance.Text=sVal;

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkIncludeDependentLines", false);
      if (sVal.Trim() != "")
        this.chkIncludeDependentLines.Checked = ((sVal.ToUpper().Trim()) == "TRUE");

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkSnapLinePointsToLines",false);
      if (sVal.Trim() != "")
        this.chkSnapLinePointsToLines.Checked = ((sVal.ToUpper().Trim()) == "TRUE");

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSASnapLinePtsToLinesTolerance", true);
      if (sVal.Trim() != "")
        this.txtSnapLinePointTolerance.Text=sVal;

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkStraightenRoadFrontages", false);
      if (sVal.Trim() != "")
        this.chkStraightenRoadFrontages.Checked = ((sVal.ToUpper().Trim()) == "TRUE");

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAStraightenRoadFrontagesOffset", true);
      if (sVal.Trim() != "")
        this.txtStraightRoadOffsetTolerance.Text=sVal;

       sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAStraightenRoadFrontagesAngle", false);
      if (sVal.Trim() != "")
        this.txtStraightRoadAngleTolerance.Text=sVal;
       }
       catch(Exception ex)
       {
         MessageBox.Show(ex.Message,"Fabric Adjustment Settings");
       }
       finally
       {
         FabUTILS=null;
       }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Utilities FabUTILS = new Utilities();
      ////check the strings to make sure they are doubles
      double dVal=0;
      string sVersion = FabUTILS.GetDesktopVersionFromRegistry();
      int iSelectedPage =this.tabControl1.SelectedIndex;
      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSASettingsSelectedPage", iSelectedPage.ToString(), false);

      if(Double.TryParse(this.txtDistTolerance.Text, out dVal))
      {
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSADistanceToleranceReport",this.txtDistTolerance.Text,true);
        
        //need to set the Text Box to the same value in the AdjustmentDockWindow
        AdjustmentDockWindow.SetTextOnDistanceTolerance(this.txtDistTolerance.Text);

      }
      if (Double.TryParse(this.txtBearingTolerance.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSABearingToleranceReport", this.txtBearingTolerance.Text,false);

      if (Double.TryParse(this.txtLinePtsOffsetTolerance.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSALinePointsOffsetToleranceReport", this.txtLinePtsOffsetTolerance.Text,true);

      if (Double.TryParse(this.txtClosePointsTolerance.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSAClosePointsToleranceReport", this.txtClosePointsTolerance.Text, true);

      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSANumberRepeatCountIteration", this.numRepeatCount.Value.ToString(),false);

      if (Double.TryParse(this.txtConvergenceValue.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSAConvergenceValue", this.txtConvergenceValue.Text,true);

      if (Double.TryParse(this.txtDivergenceValue.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSADivergenceValue", this.txtDivergenceValue.Text,true);

      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAReportType", this.cboReportType.SelectedIndex.ToString(),false);

      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSABrowseFilePath", this.txtBrowseFilePath.Text,false);

      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkBendLinesOn", (this.chkBendLines.Checked).ToString(),false);

      if (Double.TryParse(this.txtBendLinesTolerance.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSABendLinesTolerance", this.txtBendLinesTolerance.Text,true);

      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkIncludeDependentLines", (this.chkIncludeDependentLines.Checked).ToString(),false);

      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkSnapLinePointsToLines", (this.chkSnapLinePointsToLines.Checked).ToString(),false);

      if (Double.TryParse(this.txtSnapLinePointTolerance.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSASnapLinePtsToLinesTolerance", this.txtSnapLinePointTolerance.Text,true);

      FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
      "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
      "LSAChkStraightenRoadFrontages", (this.chkStraightenRoadFrontages.Checked).ToString(),false);

      if (Double.TryParse(this.txtStraightRoadOffsetTolerance.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSAStraightenRoadFrontagesOffset", this.txtStraightRoadOffsetTolerance.Text,true);

      if (Double.TryParse(this.txtStraightRoadAngleTolerance.Text, out dVal))
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSAStraightenRoadFrontagesAngle", this.txtStraightRoadAngleTolerance.Text,false);

    }

    private void txtBoxHandleKeyDown(object sender, KeyEventArgs e)
    {
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

    private void txtDistTolerance_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtBoxHandleKeyPress(object sender, KeyPressEventArgs e)
    {
      // Check for the flag being set in the KeyDown event.
      if (nonNumberEntered == true)
      {
        // Stop the character from being entered into the control since it is non-numerical.
        e.Handled = true;
      }
    }

    private void txtDistTolerance_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender,e);
    }

    private void txtBearingTolerance_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtBearingTolerance_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender,e);
    }

    private void txtLinePtsOffsetTolerance_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtLinePtsOffsetTolerance_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void txtClosePointsTolerance_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtClosePointsTolerance_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void txtConvergenceValue_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtConvergenceValue_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void txtDivergenceValue_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtDivergenceValue_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {

    }

    private void chkBendLines_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void txtBendLinesTolerance_TextChanged(object sender, EventArgs e)
    {

    }

    private void chkSnapLinePointsToLines_CheckedChanged(object sender, EventArgs e)
    {

    }

    //public dlgAdjustmentSettings hook { get; set; }
  }
}
