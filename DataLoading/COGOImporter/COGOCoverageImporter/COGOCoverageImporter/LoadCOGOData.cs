using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Cadastral;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.CatalogUI;

namespace COGOCoverageImporter
{
  public class LoadCOGOData : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public LoadCOGOData()
    {
    }

    protected override void OnClick()
    {

      IApplication pApp;
      ICadastralFabric m_pCadaFab;

      #region Get Fabric

      pApp = (IApplication)ArcMap.Application;
      if (pApp == null)
        //if the app is null then could be running from ArcCatalog
        pApp = (IApplication)ArcCatalog.Application;

      if (pApp == null)
      {
        MessageBox.Show("Could not access the application.", "No Application found");
        return;
      }

      IGxApplication pGXApp = (IGxApplication)pApp;
      stdole.IUnknown pUnk = null;
      try
      {
        pUnk = (stdole.IUnknown)pGXApp.SelectedObject.InternalObjectName.Open();
      }
      catch (COMException ex)
      {
        if (ex.ErrorCode == (int)fdoError.FDO_E_DATASET_TYPE_NOT_SUPPORTED_IN_RELEASE ||
            ex.ErrorCode == -2147220944)
          MessageBox.Show("The dataset is not supported in this release.", "Could not open the dataset");
        else
          MessageBox.Show(ex.ErrorCode.ToString(), "Could not open the dataset");
        return;
      }

      if (pUnk is ICadastralFabric)
        m_pCadaFab = (ICadastralFabric)pUnk;
      else
      {
        MessageBox.Show("Please select a parcel fabric and try again.", "Not a parcel fabric");
        return;
      }
      #endregion


      IName pName = pGXApp.SelectedObject.InternalObjectName as IName;
      ICadastralFabricName pCFName = pName as ICadastralFabricName;

      IFabricImporterUI pFabImporterUI = new FabricCogoImporterUIClass();
      pFabImporterUI.CadastralFabric = pCFName;
      pFabImporterUI.DoModal(pApp.hWnd);

      //ArcMap.Application.CurrentTool = null;
    }
    protected override void OnUpdate()
    {
      //Enabled = ArcMap.Application != null;
    }
  }

}
