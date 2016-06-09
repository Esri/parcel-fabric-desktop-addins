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
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FabricPointMoveToFeature
{
  public partial class ConfigurationDLG : Form
  {
    public ConfigurationDLG()
    {
      InitializeComponent();

      Utilities FabUtils = new Utilities();

      string sDesktopVers = FabUtils.GetDesktopVersionFromRegistry();
      if (sDesktopVers.Trim() == "")
        sDesktopVers = "Desktop10.0";
      else
        sDesktopVers = "Desktop" + sDesktopVers;
      string sValues =
      FabUtils.ReadFromRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral",
        "AddIn.FabricPointMoveToFeature");
      if (sValues.Trim() == "")
        return;

      try
      {
        string[] Values = sValues.Split(',');
        this.optLines.Checked = (Values[0].Trim() == "1");
        this.cboFldChoice.Text = (Values[1].Trim());
        this.chkMinimumMove.Checked = (Values[2].Trim() == "1");
        this.txtMinimumMove.Text = Values[3].Trim();
        this.chkReport.Checked = (Values[4].Trim() == "1");
        this.txtReportTolerance.Text = Values[5].Trim();
        this.optMoveAllFeaturesNoSelection.Checked = (Values[6].Trim() == "1");
        this.optMoveBasedOnSelectedFeatures.Checked = (Values[7].Trim() == "1");
        this.optMoveBasedOnSelectedParcels.Checked = (Values[8].Trim() == "1");
        this.chkPromptForSelection.Checked = (Values[9].Trim() == "1");
        this.chkPointMerge.Checked = (Values[10].Trim() == "1");
        this.txtMergeTolerance.Text = Values[11].Trim();

        optLines_CheckedChanged(null, null);
        optAllFeaturesNoSelection_CheckedChanged(null,null);
        chkReport_CheckedChanged(null, null);
        chkMinimumMove_CheckedChanged(null, null);
        chkPointMerge_CheckedChanged(null,null);
      }
      catch
      { }

    }
    // Boolean flag used to determine when a character other than a number is entered.
    private bool nonNumberEntered = false;
    private void ConfigurationDLG_Load(object sender, EventArgs e)
    {

    }

    private void chkReport_CheckedChanged(object sender, EventArgs e)
    {
      txtReportTolerance.Enabled = chkReport.Checked;
    }

    private void optLines_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      try
      {
        string sOpt1 = "0";
        if (this.optLines.Checked)
          sOpt1 = "1";

        string sFldName1 = Convert.ToString(this.cboFldChoice.SelectedItem);
        if (sFldName1.Trim() == "")
          sFldName1 = this.cboFldChoice.Text;
        //=======================
        string sChk1 = "0";
        if (this.chkMinimumMove.Checked)
          sChk1 = "1";

        string sVal1 = this.txtMinimumMove.Text;
        if (sVal1.Trim() == "")
          sVal1 = "0.00";

        string sChk2 = "0";
        if (this.chkReport.Checked)
          sChk2 = "1";

        string sVal2 = this.txtReportTolerance.Text;
        if (sVal2.Trim() == "")
          sVal2 = "0.00";

        string sOpt2 = "0";
        if (this.optMoveAllFeaturesNoSelection.Checked)
          sOpt2 = "1";

        string sOpt3 = "0";
        if (this.optMoveBasedOnSelectedFeatures.Checked)
          sOpt3 = "1";

        string sOpt4 = "0";
        if (this.optMoveBasedOnSelectedParcels.Checked)
          sOpt4 = "1";

        string sChk3 = "0";
        if (this.chkPromptForSelection.Checked)
          sChk3 = "1";

        string sChk4 = "0";
        if (this.chkPointMerge.Checked)
          sChk4 = "1";

        string sVal4 = this.txtMergeTolerance.Text;
        if (sVal4.Trim() == "")
          sVal4 = "0.00";

        //write the key
        Utilities Utils = new Utilities();
        string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
        if (sDesktopVers.Trim() == "")
          sDesktopVers = "Desktop10.4";
        else
          sDesktopVers = "Desktop" + sDesktopVers;

        Utils.WriteToRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" +
          sDesktopVers + "\\ArcMap\\Cadastral", "AddIn.FabricPointMoveToFeature",
          sOpt1 + "," + sFldName1 + "," + sChk1 + "," + sVal1 + "," + sChk2 + "," + sVal2
          + "," + sOpt2 + "," + sOpt3 + "," + sOpt4 + "," + sChk3 + "," + sChk4 + "," + sVal4);

        string sTabPgIdx = this.tbConfiguration.SelectedIndex.ToString();

        Utils.WriteToRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" +
          sDesktopVers + "\\ArcMap\\Cadastral", "AddIn.FabricPointMoveToFeatureConfigurationLastPageUsed",
          sTabPgIdx);
      }
      catch
      {

      }

    }

    private void optPoints_CheckedChanged(object sender, EventArgs e)
    {
      cboFldChoice.Enabled = (optPoints.Checked);
    }

    private void txtReportTolerance_KeyDown(object sender, KeyEventArgs e)
    {
      txtBox_KeyDown(sender, e);
    }

    private void txtReportTolerance_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBox_KeyPress(sender, e);
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

    private void cboFldChoice_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void optAllFeaturesNoSelection_CheckedChanged(object sender, EventArgs e)
    {
      chkPromptForSelection.Enabled = !optMoveAllFeaturesNoSelection.Checked;
      if (optMoveAllFeaturesNoSelection.Checked)
        chkPromptForSelection.Checked = false;
    }

    private void chkMinimumMove_CheckedChanged(object sender, EventArgs e)
    {
      txtMinimumMove.Enabled = chkMinimumMove.Checked;
    }

    private void txtMinimumMove_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBox_KeyPress(sender, e);
    }

    private void txtMinimumMove_KeyDown(object sender, KeyEventArgs e)
    {
      txtBox_KeyDown(sender, e);
    }

    private void chkPointMerge_CheckedChanged(object sender, EventArgs e)
    {
      txtMergeTolerance.Enabled = chkPointMerge.Checked;
    }

    private void txtMergeTolerance_KeyDown(object sender, KeyEventArgs e)
    {
      txtBox_KeyDown(sender, e);
    }

    private void txtMergeTolerance_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBox_KeyPress(sender, e);
    }

  }
}
