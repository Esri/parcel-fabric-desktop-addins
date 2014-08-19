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
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace DeleteSelectedParcels
{
  public class TruncateFabricTables : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    IApplication m_pApp;
    private ICadastralFabric m_pCadaFab;
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSet;
    private enum FabricClassGroup
    {
      ControlPoints = 0,
      PlansParcelsLinesPointsLinePoints = 1,
      CadastralJobs = 2,
      FeatureAdjustmentVectors = 3,
      Accuracy = 4
    }
    public TruncateFabricTables()
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

      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      clsFabricUtils FabricUTILS = new clsFabricUtils();
      IProgressDialog2 pProgressorDialog = null;

      ITable pTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
      IDataset pDS = (IDataset)pTable;
      IWorkspace pWS = pDS.Workspace;
      bool bIsFileBasedGDB = true;
      bool bIsUnVersioned = true;

      FabricUTILS.GetFabricPlatform(pWS, m_pCadaFab, out bIsFileBasedGDB,
        out bIsUnVersioned);

      if (!bIsFileBasedGDB && !bIsUnVersioned)
      {
        MessageBox.Show("Truncate operates on non-versioned fabrics."
          + Environment.NewLine +
          "Please unversion the fabric and try again.", "Tables are versioned");
        return;
      }


      //Do a Start and Stop editing to make sure truncate it not running within an edit session
      if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
      {//if start editing fails then bail
        Cleanup(pProgressorDialog, pMouseCursor);
        return;
      }
      FabricUTILS.StopEditing(pWS);

      dlgTruncate pTruncateDialog = new dlgTruncate();
      IArray TableArray = new ESRI.ArcGIS.esriSystem.ArrayClass();
      pTruncateDialog.TheFabric = m_pCadaFab;
      pTruncateDialog.TheTableArray = TableArray;

      //Display the dialog
      DialogResult pDialogResult = pTruncateDialog.ShowDialog();

      if (pDialogResult != DialogResult.OK)
      {
        pTruncateDialog = null;
        if (TableArray != null)
        {
          TableArray.RemoveAll();
        }
        return;
      }

      m_pProgressorDialogFact = new ProgressDialogFactoryClass();
      m_pTrackCancel = new CancelTrackerClass();
      m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, m_pApp.hWnd);
      pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
      m_pStepProgressor.MinRange = 0;
      m_pStepProgressor.MaxRange = pTruncateDialog.DropRowCount;
      m_pStepProgressor.StepValue = 1;
      pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
      bool bSuccess = false;
      try
      {
        //Work on the table array
        pTable = null;

        m_pFIDSet = new FIDSetClass();
        for (int i = 0; i <= TableArray.Count - 1; i++)
        {
          if (TableArray.get_Element(i) is ITable)
          {
            pTable = (ITable)TableArray.get_Element(i);
            IDataset pDataSet = (IDataset)pTable;
            //Following code uses the truncate method
            //***
            if (pTable is IFeatureClass)
            {
              ITableWrite2 pTableWr = (ITableWrite2)pTable;
              m_pStepProgressor.Message = "Deleting all rows in " + pDataSet.Name;
              int RowCnt=pTable.RowCount(null);
              pTableWr.Truncate();
              m_pStepProgressor.MaxRange -= RowCnt;              
            }
          }
         }
        }
        catch(COMException ex)
        {
          MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.ErrorCode));
          Cleanup(pProgressorDialog, pMouseCursor);
          return;
        }

      //do the loop again, this time within the edit transaction and using the delete function for the chosen tables
      try
      {
        //Start an Edit Transaction
        if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
        {//if start editing fails then bail
          Cleanup(pProgressorDialog, pMouseCursor);
          return;
        }
        for (int i = 0; i <= TableArray.Count - 1; i++)
        {
          if (TableArray.get_Element(i) is ITable)
          {
            pTable = (ITable)TableArray.get_Element(i);
            IDataset pDataSet = (IDataset)pTable;

            if (pTable is IFeatureClass)
            {
            }
            else
            {
              //The following code is in place to workaround a limitation of truncate for fabric classes 
              //without a shapefield. It uses an alternative method for removing all the rows 
              //with the Delete function.
              //General note: This method could be used exclusively, without needing the truncate method.
              //One advantage is that it allows the option to cancel the whole 
              //operation using the cancel tracker. Truncate is faster, but is problematic if
              //the truncate fails, and leaves a partially deleted fabric. For example, if the 
              //lines table is deleted but the points table truncate fails, the fabric would be in a 
              //corrupt state.
              //****

              m_pFIDSet.SetEmpty();
              string sName = pDataSet.Name.ToUpper().Trim();
              m_pStepProgressor.Message = "Loading rows from " + pDataSet.Name;

              if (sName.EndsWith("_PLANS"))
              {//for Plans table make sure the default plan is not deleted
                IQueryFilter pQF = new QueryFilterClass();
                string sPref; string sSuff;
                ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
                sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
                sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);
                string sFieldName = "NAME";
                //pQF.WhereClause = sPref + sFieldName + sSuff + " <> '<map>'";
                pQF.WhereClause = sFieldName + " <> '<map>'";
                if (!BuildFIDSetFromTable(pTable, pQF, ref m_pFIDSet))
                {
                  FabricUTILS.AbortEditing(pWS);
                  Cleanup(pProgressorDialog, pMouseCursor);
                  return;
                }

              }
              else
              {
                if (!BuildFIDSetFromTable(pTable, null, ref m_pFIDSet))
                {
                  FabricUTILS.AbortEditing(pWS);
                  Cleanup(pProgressorDialog, pMouseCursor);
                  return;
                }
              }

              if (m_pFIDSet.Count() == 0)
                continue;

              m_pStepProgressor.Message = "Deleting all rows in " + pDataSet.Name;
              bSuccess = FabricUTILS.DeleteRowsUnversioned(pWS, pTable, m_pFIDSet,
                m_pStepProgressor, m_pTrackCancel);
              if (!bSuccess)
              {
                FabricUTILS.AbortEditing(pWS);
                Cleanup(pProgressorDialog, pMouseCursor);
                return;
              }
            }
          }
        }

        //now need to Fix control-to-point associations if one table was truncated 
        //and the other was not
        if (pTruncateDialog.TruncateControl && !pTruncateDialog.TruncateParcelsLinesPoints)
        {
          IQueryFilter pQF = new QueryFilterClass();
          string sPref; string sSuff;
          ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
          sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
          sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);
          string sFieldName = "NAME";
          //NAME IS NOT NULL AND (NAME <>'' OR NAME <>' ')
          //pQF.WhereClause = sPref + sFieldName + sSuff + " IS NOT NULL AND (" +
          //  sPref + sFieldName + sSuff + "<>'' OR " + sPref + sFieldName + sSuff + " <>' ')";
          pQF.WhereClause = sFieldName + " IS NOT NULL AND (" + sFieldName + "<>'' OR " + sFieldName + " <>' ')";

          ITable PointTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
          ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
          pSchemaEd.ReleaseReadOnlyFields(PointTable, esriCadastralFabricTable.esriCFTPoints);

          if (!FabricUTILS.ResetPointAssociations(PointTable, pQF, true))
          {
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);
            FabricUTILS.AbortEditing(pWS);
            Cleanup(pProgressorDialog, pMouseCursor);
            return;
          }
          
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);

        }

        else if (pTruncateDialog.TruncateParcelsLinesPoints && !pTruncateDialog.TruncateControl)
        {
          IQueryFilter pQF = new QueryFilterClass();
          string sPref; string sSuff;
          ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
          sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
          sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);
          //POINTID >=0 AND POINTID IS NOT NULL
          string sFieldName = "POINTID";
          //pQF.WhereClause = sPref + sFieldName + sSuff + " IS NOT NULL AND " + 
          //  sPref + sFieldName + sSuff + " >=0";
          pQF.WhereClause = sFieldName + " IS NOT NULL AND " + sFieldName + " >=0";

          ITable ControlTable=m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);
          ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
          pSchemaEd.ReleaseReadOnlyFields(ControlTable, esriCadastralFabricTable.esriCFTControl);
          if (!FabricUTILS.ResetControlAssociations(ControlTable, null, true))
          {
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);
            FabricUTILS.AbortEditing(pWS);
            Cleanup(pProgressorDialog, pMouseCursor);
            return;
          }
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);
        }

        //now need to re-assign default accuracy table values, if the option was checked
        if (pTruncateDialog.ResetAccuracyTableDefaults)
        {
          double dCat1 = FabricUTILS.ConvertMetersToFabricUnits(0.001, m_pCadaFab);
          double dCat2 = FabricUTILS.ConvertMetersToFabricUnits(0.01, m_pCadaFab);
          double dCat3 = FabricUTILS.ConvertMetersToFabricUnits(0.02, m_pCadaFab);
          double dCat4 = FabricUTILS.ConvertMetersToFabricUnits(0.05, m_pCadaFab);
          double dCat5 = FabricUTILS.ConvertMetersToFabricUnits(0.2, m_pCadaFab);
          double dCat6 = FabricUTILS.ConvertMetersToFabricUnits(1, m_pCadaFab);
          double dCat7 = FabricUTILS.ConvertMetersToFabricUnits(10, m_pCadaFab);

          ITable pAccTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTAccuracy);
          int idxBrgSD = pAccTable.FindField("BrgSD");
          int idxDistSD = pAccTable.FindField("DistSD");
          int idxPPM = pAccTable.FindField("PPM");
          int idxCategory = pAccTable.FindField("Category");
          int idxDescription = pAccTable.FindField("Description");
          ITableWrite2 pTableWr = (ITableWrite2)pAccTable;
          ICursor pCur = pTableWr.InsertRows(false);

          IRowBuffer pRowBuff = pAccTable.CreateRowBuffer();

          //write category 1
          pRowBuff.set_Value(idxCategory, 1);
          pRowBuff.set_Value(idxBrgSD, 5);
          pRowBuff.set_Value(idxDistSD, dCat1);
          pRowBuff.set_Value(idxPPM, 5);
          pRowBuff.set_Value(idxDescription, "1 - Highest");
          pCur.InsertRow(pRowBuff);

          //write category 2
          pRowBuff.set_Value(idxCategory, 2);
          pRowBuff.set_Value(idxBrgSD, 30);
          pRowBuff.set_Value(idxDistSD, dCat2);
          pRowBuff.set_Value(idxPPM, 25);
          pRowBuff.set_Value(idxDescription, "2 - After 1980");
          pCur.InsertRow(pRowBuff);

          //write category 3
          pRowBuff.set_Value(idxCategory, 3);
          pRowBuff.set_Value(idxBrgSD, 60);
          pRowBuff.set_Value(idxDistSD, dCat3);
          pRowBuff.set_Value(idxPPM, 50);
          pRowBuff.set_Value(idxDescription, "3 - 1908 to 1980");
          pCur.InsertRow(pRowBuff);

          //write category 4
          pRowBuff.set_Value(idxCategory, 4);
          pRowBuff.set_Value(idxBrgSD, 120);
          pRowBuff.set_Value(idxDistSD, dCat4);
          pRowBuff.set_Value(idxPPM, 125);
          pRowBuff.set_Value(idxDescription, "4 - 1881 to 1907");
          pCur.InsertRow(pRowBuff);

          //write category 5
          pRowBuff.set_Value(idxCategory, 5);
          pRowBuff.set_Value(idxBrgSD, 300);
          pRowBuff.set_Value(idxDistSD, dCat5);
          pRowBuff.set_Value(idxPPM, 125);
          pRowBuff.set_Value(idxDescription, "5 - Before 1881");
          pCur.InsertRow(pRowBuff);

          //write category 6
          pRowBuff.set_Value(idxCategory, 6);
          pRowBuff.set_Value(idxBrgSD, 3600);
          pRowBuff.set_Value(idxDistSD, dCat6);
          pRowBuff.set_Value(idxPPM, 1000);
          pRowBuff.set_Value(idxDescription, "6 - 1800");
          pCur.InsertRow(pRowBuff);

          //write category 7
          pRowBuff.set_Value(idxCategory, 7);
          pRowBuff.set_Value(idxBrgSD, 6000);
          pRowBuff.set_Value(idxDistSD, dCat7);
          pRowBuff.set_Value(idxPPM, 5000);
          pRowBuff.set_Value(idxDescription, "7 - Lowest");
          pCur.InsertRow(pRowBuff);

          pCur.Flush();
          if (pRowBuff != null)
            Marshal.ReleaseComObject(pRowBuff);
          if (pCur != null)
            Marshal.ReleaseComObject(pCur);
        }

        //now need to cleanup the IDSequence table if ALL the tables were truncated
        if(pTruncateDialog.TruncateControl&&
           pTruncateDialog.TruncateParcelsLinesPoints &&
           pTruncateDialog.TruncateJobs &&
           pTruncateDialog.TruncateAdjustments)
        {
          IWorkspace2 pWS2=(IWorkspace2)pWS;
          IDataset TheFabricDS=(IDataset)m_pCadaFab;
          string sFabricName=TheFabricDS.Name;
          string sName = sFabricName + "_IDSequencer";
          bool bExists=pWS2.get_NameExists(esriDatasetType.esriDTTable, sName);
          IFeatureWorkspace pFWS=(IFeatureWorkspace)pWS;
          ITable pSequencerTable;
          if (bExists)
          {
            pSequencerTable = pFWS.OpenTable(sName);
            IFIDSet pFIDSet= new FIDSetClass();
            if (BuildFIDSetFromTable(pSequencerTable, null, ref pFIDSet))
              FabricUTILS.DeleteRowsUnversioned(pWS, pSequencerTable, pFIDSet, null, null);
          }
        }

        Cleanup(pProgressorDialog, pMouseCursor);

        if (TableArray != null)
        {
          TableArray.RemoveAll();
        }
        FabricUTILS.StopEditing(pWS);
      }
      catch (Exception ex)
      {
        FabricUTILS.AbortEditing(pWS);
        Cleanup(pProgressorDialog, pMouseCursor);
        MessageBox.Show(Convert.ToString(ex.Message));
      }
    }

    private bool BuildFIDSetFromTable(ITable TheTable, IQueryFilter QueryFilter, ref IFIDSet FIDSet)
    {
      ICursor pCur = TheTable.Search(QueryFilter, false);
      try
      {
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          FIDSet.Add(pRow.OID);
          Marshal.ReleaseComObject(pRow);
          pRow = pCur.NextRow();
        }
        Marshal.ReleaseComObject(pCur);
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.ErrorCode));
        if (pCur!=null)
          Marshal.ReleaseComObject(pCur);
        return false;
      }
    }

    private void Cleanup(IProgressDialog2 ProgressorDialog, IMouseCursor MouseCursor)
    {
      MouseCursor.SetCursor(0);

      if (!(ProgressorDialog == null))
      {
        ProgressorDialog.HideDialog();
      }
      ProgressorDialog = null;
      m_pStepProgressor = null;
    }
  }
}
