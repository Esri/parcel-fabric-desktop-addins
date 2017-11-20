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

using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;

namespace ParcelEditHelper
{
  public partial class dlgParcEditHelperOptions : Form
  {
    // Boolean flag used to determine when a character other than a number is entered.
    private bool nonNumberEntered = false;
    ParcelEditHelperExtension ext_ParcelEditHelper;
    //private static System.Windows.Forms.TextBox s_TxtFieldName;
    Utilities UTIL = new Utilities();
    public dlgParcEditHelperOptions()
    {
      InitializeComponent();

      if (ext_ParcelEditHelper == null)
        ext_ParcelEditHelper = ParcelEditHelperExtension.GetParcelEditHelperExtension();

      chkCopyDirection_CheckedChanged(null, null);
      try
      {
        //string sDesktopVers = UTIL.GetDesktopVersionFromRegistry();
        //if (sDesktopVers.Trim() == "")
        //  sDesktopVers = "Desktop10.0";
        //else
        //  sDesktopVers = "Desktop" + sDesktopVers;
        
        //string sValues = UTIL.ReadFromRegistry(RegistryHive.CurrentUser,
        //"Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        //  "Options", false);

        //string[] Values = sValues.Split(',');
        //string sVal = Values[0];
        //sFieldName = Values[1];
        //bUseFieldRecord = false;

        //if (sVal.Trim() != "")
        //  this.chkCopyDirection.Checked = ((sVal.ToUpper().Trim()) == "1");

        this.chkCopyDirection.Checked = ext_ParcelEditHelper.RecordToField;

        //bUseFieldRecord = this.chkCopyDirection.Checked;
        this.txtFldName.Text = ext_ParcelEditHelper.FieldName;

        //ext_ParcelEditHelper.FieldName = sFieldName;
        //ext_ParcelEditHelper.RecordToField = bUseFieldRecord;

        btnApply.Enabled = false;
      }
      catch { }
    }

    private void chkCopyDirection_CheckedChanged(object sender, EventArgs e)
    {
      txtFldName.Enabled = btnChkField.Enabled = chkCopyDirection.Checked;
      txtChanged();
    }

    private void txtFldName_TextChanged(object sender, EventArgs e)
    {
      txtChanged();
    }

    private void txtChanged()
    {
      btnApply.Enabled = txtFldName.TextLength > 0;
    }

    private void btnApply_Click(object sender, EventArgs e)
    {
      try
      {

        string sDesktopVers = UTIL.GetDesktopVersionFromRegistry();
        if (sDesktopVers.Trim() == "")
          sDesktopVers = "Desktop10.0";
        else
          sDesktopVers = "Desktop" + sDesktopVers;

        string sChk1 = "0";
        if (this.chkCopyDirection.Checked)
          sChk1 = "1";

        UTIL.WriteToRegistry(RegistryHive.CurrentUser, 
          "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
          "Options", sChk1 + "," + txtFldName.Text, false);

        if (ext_ParcelEditHelper == null)
          ext_ParcelEditHelper = ParcelEditHelperExtension.GetParcelEditHelperExtension();

        ext_ParcelEditHelper.RecordToField = (sChk1 == "1");
        ext_ParcelEditHelper.FieldName = txtFldName.Text;

        this.btnApply.Enabled = false;

      }
      catch{}
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      btnApply_Click(null, null);
    }

    private void btnChkField_Click(object sender, EventArgs e)
    {
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      bool bIsEditing = (ArcMap.Editor.EditState==ESRI.ArcGIS.Editor.esriEditState.esriStateEditing);
      
      ICadastralFabric pCadFab = pCadEd.CadastralFabric;
      IFeatureClass ParcelLinesFC = null;

      if (bIsEditing)
        ParcelLinesFC = pCadFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines) as IFeatureClass;
      else
      {
        UTIL.GetFabricFromMap(ParcelEditHelper.ArcMap.Document.ActiveView.FocusMap, out pCadFab);
        ParcelLinesFC = pCadFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines) as IFeatureClass;
      }

      if (pCadFab == null)
      {
        MessageBox.Show("Parcel fabric not found in the map.", "Check");
        return;
      }

      int iFldIdx = ParcelLinesFC.FindField(this.txtFldName.Text);

      if (iFldIdx == -1)
      {
        if (bIsEditing)
          MessageBox.Show("Field is not present. Stop editing, and click 'Check' again to be prompted to create the field, " +
            "or else manually create the string field using the Catalog window or ArcToolbox.","Check");
        else
        {
          DialogResult dialogRes = MessageBox.Show("Field is not present. Create string field called " + this.txtFldName.Text + " ?","Create field",MessageBoxButtons.YesNo);
          if (dialogRes == DialogResult.No)
            return;

          if (this.txtFldName.Text.Trim() == "")
            return;

          IField2 pFld = new FieldClass();
          IFieldEdit2 pFldEd = pFld as IFieldEdit2;
          pFldEd.Editable_2 = true;
          pFldEd.Name_2 = this.txtFldName.Text;
          pFldEd.Type_2 = esriFieldType.esriFieldTypeString;
          pFldEd.Length_2 = 50;
          ParcelLinesFC.AddField(pFld);
        }
      }
      else
      {
        IField pField = ParcelLinesFC.Fields.Field[iFldIdx];
        if (pField.Type != esriFieldType.esriFieldTypeString)
        {
          MessageBox.Show("A field called " + this.txtFldName.Text + " was found." + Environment.NewLine +
            "However, the field is not a Text field." + Environment.NewLine +
            "Please create or use a different Text field.","Check field",
            MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
        else
          MessageBox.Show("A field called " + this.txtFldName.Text + " was found.", "Check field");
      }

    }
  }
}
