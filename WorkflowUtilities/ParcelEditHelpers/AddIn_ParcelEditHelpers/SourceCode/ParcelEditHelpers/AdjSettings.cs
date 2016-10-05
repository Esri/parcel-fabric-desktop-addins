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
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using Microsoft.Win32;
namespace ParcelEditHelper
{
  public class AdjSettings : ESRI.ArcGIS.Desktop.AddIns.Button
  {

    public static AdjustmentDockWindow xx;
    public AdjSettings()
    {
    }

    protected override void OnClick()
    {
      //read values from the registry, values are in meters, need to convert
      GetFabricAdjustmentSettings();
    }

    public void GetFabricAdjustmentSettings()
    {
      try
      {
      Utilities FabUtils= new Utilities();
      string sLSABearingTolerance=
      FabUtils.ReadFromRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\Desktop10.1\\ArcMap\\Cadastral", "LSABearingTolerance",false);

      string sLSAListingDetailLevel=
      FabUtils.ReadFromRegistry(RegistryHive.CurrentUser, 
      "Software\\ESRI\\Desktop10.1\\ArcMap\\Cadastral", "LSAListingDetailLevel", false);

      dlgAdjustmentSettings pAdjustmentSettingsDialog = new dlgAdjustmentSettings();



      //Display the dialog
      DialogResult pDialogResult = pAdjustmentSettingsDialog.ShowDialog();
      
      if (pDialogResult != DialogResult.OK)
        return;

      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    
    }

    protected override void OnUpdate()
    {
    }

  }
}
