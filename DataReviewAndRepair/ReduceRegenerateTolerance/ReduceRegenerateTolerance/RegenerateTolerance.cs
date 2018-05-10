using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using ESRI.ArcGIS.ArcCatalog;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Framework;

namespace ReduceRegenerateTolerance
{
  public class RegenerateTolerance : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public RegenerateTolerance()
    {
    }

    protected override void OnClick()
    {
      ICadastralFabric pCadaFab = null;
      IApplication pApp = (IApplication)ArcMap.Application;
      if (pApp == null)
        pApp = (IApplication)ArcCatalog.Application; //if the app is null then try ArcCatalog

      if (pApp == null)
      {
        MessageBox.Show("Could not access the application.", "No Application found");
        return;
      }
      stdole.IUnknown pUnk = null;
      try
      {
        IGxApplication pGXApp = (IGxApplication)pApp;
        pUnk = (stdole.IUnknown)pGXApp.SelectedObject.InternalObjectName.Open();
        if (!(pUnk is ICadastralFabric))
        {
          MessageBox.Show("Please select a parcel fabric in the Catalog window and try again.", "Not a parcel fabric");
          return;
        }
        pCadaFab = pUnk as ICadastralFabric;
      }
      catch (Exception ex)
      {
        MessageBox.Show("There was a problem opening this fabric." + Environment.NewLine +
          ex.Message);
        return;
      }

      int iV = GetFabricVersion(pCadaFab);
      bool bCanDo = (iV >= 3);

      if (!bCanDo)
      {
        MessageBox.Show("This extended property is not available with this version of the fabric's schema." +
          Environment.NewLine + "Please upgrade the fabric and try again.", "Extended Properties");
        return;
      }

      bool bIsReducedRegenerateTolerance = GetReducedRegenerateTolerance(pCadaFab);
      string sCurrentState_ON_or_OFF =  bIsReducedRegenerateTolerance ? "REDUCED":"NORMAL";
      string sProposedtState_ON_or_OFF = bIsReducedRegenerateTolerance ? "NORMAL": "REDUCED";

      if (MessageBox.Show("The regenerate tolerance is currently set to " 
        + sCurrentState_ON_or_OFF + Environment.NewLine 
        + "Would you like to set it to " + sProposedtState_ON_or_OFF + " ?" + Environment.NewLine + Environment.NewLine
        + "Set the REDUCED property to regenerate the fabric with a tolerance 100 times smaller than the " 
        + "default XY tolerance of the dataset." + Environment.NewLine
        + "Use this REDUCED tolerance for the first regenerate after a data load or after an append to the fabric." 
        + "Then reset it back to NORMAL for subsequent standard editing workflows."
        , "Regenerate Tolerance"
        , MessageBoxButtons.YesNo, MessageBoxIcon.Question)==DialogResult.Yes)
      {
        SetReducedRegenerateTolerance(pCadaFab, !bIsReducedRegenerateTolerance);
      }

    }

    private int CountPropertySetItems(ICadastralFabric pFab, esriCadastralPropertySetType PropertySetType)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric3 pDECadaFab3 = (IDECadastralFabric3)pDEDS;
      IPropertySet pPropSetEdSettings = null;
      pDECadaFab3.GetPropertySet(PropertySetType, out pPropSetEdSettings);
      return pPropSetEdSettings.Count;
    }

    private int GetFabricVersion(ICadastralFabric pFab)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric2 pDECadaFab2 = (IDECadastralFabric2)pDEDS;
      int x = pDECadaFab2.Version;
      return x;
    }


    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }

    void SetReducedRegenerateTolerance(ICadastralFabric pFab, bool IsReducedTolerance)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric3 pDECadaFab3 = (IDECadastralFabric3)pDEDS;
      IPropertySet pPropSetEdSettings = null;
      pDECadaFab3.GetPropertySet(esriCadastralPropertySetType.esriCadastralPropSetEditSettings, out pPropSetEdSettings);
      pPropSetEdSettings.SetProperty("esriReduceRegenerateTolerance", IsReducedTolerance);
      pDECadaFab3.SetPropertySet(esriCadastralPropertySetType.esriCadastralPropSetEditSettings, pPropSetEdSettings);
 
      //Update the schema
      ICadastralFabricSchemaEdit pSchemaEd = (ICadastralFabricSchemaEdit)pFab;
      IDECadastralFabric pDECadaFab = (IDECadastralFabric)pDECadaFab3;
      pSchemaEd.UpdateSchema(pDECadaFab);
    }

    bool GetReducedRegenerateTolerance(ICadastralFabric pFab)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric3 pDECadaFab = (IDECadastralFabric3)pDEDS;

      IPropertySet pPropSetTol = null;
      pDECadaFab.GetPropertySet(esriCadastralPropertySetType.esriCadastralPropSetEditSettings, out pPropSetTol);

      object retVal = null;
      try
      {
        retVal = pPropSetTol.GetProperty("esriReduceRegenerateTolerance");
      }
      catch
      {
        return false; //default value
      }
      bool b_retVal = Convert.ToBoolean(retVal);
      return b_retVal;
    }


    double GetMinScaleTolerance(ICadastralFabric pFab)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric3 pDECadaFab = (IDECadastralFabric3)pDEDS;

      IPropertySet pPropSetTol = null;
      pDECadaFab.GetPropertySet(esriCadastralPropertySetType.esriCadastralPropSetEditSettings, out pPropSetTol);

      object retVal = null;
      try
      {
        retVal = pPropSetTol.GetProperty("esriMinScaleTolerance");
      }
      catch
      {
        Marshal.ReleaseComObject(pDEDS);
        Marshal.ReleaseComObject(pPropSetTol);
        return 1.2; //default value
      }
      double d_retVal = Convert.ToDouble(retVal);
      return d_retVal;
    }

    void SetMinScaleTolerance(ICadastralFabric pFab, double ScaleTolerance)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric3 pDECadaFab3 = (IDECadastralFabric3)pDEDS;
      IPropertySet pPropSetEdSettings = null;
      pDECadaFab3.GetPropertySet(esriCadastralPropertySetType.esriCadastralPropSetEditSettings, out pPropSetEdSettings);
      pPropSetEdSettings.SetProperty("esriMinScaleTolerance", ScaleTolerance);
      pDECadaFab3.SetPropertySet(esriCadastralPropertySetType.esriCadastralPropSetEditSettings, pPropSetEdSettings);

      //Update the schema
      ICadastralFabricSchemaEdit pSchemaEd = (ICadastralFabricSchemaEdit)pFab;
      IDECadastralFabric pDECadaFab = (IDECadastralFabric)pDECadaFab3;
      pSchemaEd.UpdateSchema(pDECadaFab);
    }

  }

}
