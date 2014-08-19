/*
 Copyright 1995-2014 ESRI

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

using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FabricPlanTools
{
  public class AddFlagField : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    IApplication m_pApp;
    private ICadastralFabric m_pCadaFab;

    public AddFlagField()
    {
    }

    protected override void OnClick()
    {
      m_pApp = (IApplication)ArcMap.Application;
      if (m_pApp == null)
        //if the app is null then could be running from ArcCatalog
        m_pApp = (IApplication)ArcCatalog.Application;

      if (m_pApp == null)
      {
        MessageBox.Show("Could not access the application.", "No Application found");
        return;
      }
      IGxApplication pGXApp = (IGxApplication)m_pApp;
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


      Utils FabricUTILS = new Utils();

      ITable pTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
      IDataset pDS = (IDataset)pTable;
      IWorkspace pWS = pDS.Workspace;

      //Do a Start and Stop editing to make sure we're not running in an edit session
      if (!FabricUTILS.StartEditing(pWS, true))
      {//if start editing fails then bail
        if (pUnk != null)
          Marshal.ReleaseComObject(pUnk);
        Cleanup(pTable, pWS);
        FabricUTILS = null;
        return;
      }
      FabricUTILS.StopEditing(pWS);

      bool bAddedField = false;

      if (FabricUTILS.GetFabricVersion((ICadastralFabric2)m_pCadaFab) < 2)
      {
        bAddedField = FabricUTILS.CadastralTableAddFieldV1(m_pCadaFab, esriCadastralFabricTable.esriCFTPlans, esriFieldType.esriFieldTypeInteger,
          "KeepOnMerge", "KeepOnMerge", 1);
      }
      else
      {
        bAddedField = FabricUTILS.CadastralTableAddField(m_pCadaFab, esriCadastralFabricTable.esriCFTPlans, esriFieldType.esriFieldTypeInteger,
           "KeepOnMerge", "KeepOnMerge", 1);
      }

      if (bAddedField)
        MessageBox.Show("Plan-merge helper field 'KeepOnMerge' added.", "Add Field");
      else
        MessageBox.Show("Field 'KeepOnMerge' could not be added." + Environment.NewLine + "The field may already exist.",
          "Add Field");

      if (bAddedField)
      { 
        //if the field was added succesfully, add the Yes/No domain
        IDomain pDom = new CodedValueDomainClass();
        try
        {
          IWorkspaceDomains2 pWSDoms = (IWorkspaceDomains2)pWS;
          pDom.FieldType = esriFieldType.esriFieldTypeInteger;
          pDom.Name = "Flag for Keep on Plan Merge";
          pDom.Description = "Flag for Keep on Plan Merge";
          ICodedValueDomain pCVDom = (ICodedValueDomain)pDom;
          //pCVDom.AddCode(0, "No");
          pCVDom.AddCode(1, "Keep On Merge");
          pWSDoms.AddDomain(pDom);
        }
        catch (COMException ex)
        {
          MessageBox.Show(ex.ErrorCode.ToString());
        }

        //Get the field
        int iFld = pTable.FindField("KeepOnMerge");
        if (iFld>=0)
        {
          IField pFld = pTable.Fields.get_Field(iFld);
        // Check that the field and domain have the same field type.
          if (pFld.Type == pDom.FieldType)
          {
            // Cast the feature class to the ISchemaLock and IClassSchemaEdit interfaces.
            ISchemaLock schemaLock = (ISchemaLock)pTable;
            IClassSchemaEdit classSchemaEdit = (IClassSchemaEdit)pTable;

            // Attempt to get an exclusive schema lock.
            try
            {
              // Lock the class and alter the domain.
              schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
              classSchemaEdit.AlterDomain("KeepOnMerge", pDom);
              Console.WriteLine("The domain was successfully assigned.");
            }
            catch (COMException exc)
            {
              // Handle the exception in a way appropriate for the application.
              Console.WriteLine(exc.Message);
            }
            finally
            {
              // Set the schema lock to be a shared lock.
              schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
            }
          }

        }

        if (pDom != null)
          Marshal.ReleaseComObject(pDom);
      }
      Cleanup(pTable, pWS);
      FabricUTILS = null;
    }

    protected override void OnUpdate()
    {
    }

    private void Cleanup(ITable PlansTable, IWorkspace Workspace)
    {

      if (PlansTable != null)
      {
        do { }
        while (Marshal.ReleaseComObject(PlansTable) > 0);
      }

      if (Workspace != null)
      {
        do { }
        while (Marshal.ReleaseComObject(Workspace) > 0);
      }

      }
    }

  }

