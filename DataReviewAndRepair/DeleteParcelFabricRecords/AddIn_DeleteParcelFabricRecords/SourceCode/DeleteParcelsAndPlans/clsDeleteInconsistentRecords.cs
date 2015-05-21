/*
 Copyright 1995-2015 Esri

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
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace DeleteSelectedParcels
{
  public class clsDeleteInconsistentRecords : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    struct LARGE_INTEGER
    {
      [FieldOffset(0)]
      public Int64 QuadPart;
      [FieldOffset(0)]
      public UInt32 LowPart;
      [FieldOffset(4)]
      public Int32 HighPart;
    }

    public clsDeleteInconsistentRecords()
    {
    }

    protected override void OnClick()
    {
      ITime m_pEndTime;
      ITime m_pStartTime;
      IApplication pApp;
      ICadastralFabric m_pCadaFab;
      IStepProgressor pStepProgressor;
      //Create a CancelTracker.
      ITrackCancel pTrackCancel;
      IProgressDialogFactory pProgressorDialogFact;
      int iTokenMax = 995;

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

      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      clsFabricUtils FabricUTILS = new clsFabricUtils();

      #region Get Fabric Tables
      ITable pParcels = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
      ITable pLines = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
      ITable pPoints = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
      ITable pLinePts = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLinePoints);
      ITable pVectors = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTVectors);
      ITable pAdjLevels = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLevels);
      
      IDataset pDS = (IDataset)pParcels;
      IWorkspace pWS = pDS.Workspace;

      //Stopped doing a Start and Stop editing to check if it's running in an edit session...was causing memory issues.
      //updated the routine to instead check privileges on one of the fabric tables, and check for protected version, and check if local db is being edited.

      string sMessage = "";
      bool bCanEdit = FabricUTILS.CanEditFabric(pWS, pLines, out sMessage);

      if (!bCanEdit)
      {
        DialogResult dRes = MessageBox.Show(sMessage + Environment.NewLine + "You will be able to retrieve the report, but you will"
         + Environment.NewLine + "not be able to delete records." + Environment.NewLine + Environment.NewLine + "Would you like to continue?" + Environment.NewLine +
         "Click Yes to continue. Click No to exit.",
         "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (dRes == DialogResult.No)
          return;
      }

      int iParcelRowCount = pParcels.RowCount(null);
      int iLineRowCount = pLines.RowCount(null);
      int iPointRowCount = pPoints.RowCount(null);
      int iLinePointRowCount = pLinePts.RowCount(null);
      int iVectorRowCount = pVectors.RowCount(null);
      int iLevelRowCount = pAdjLevels.RowCount(null);

      int iTotalRowCount = iParcelRowCount + iLineRowCount + iPointRowCount + iLinePointRowCount + iVectorRowCount + iLevelRowCount;

      ITable pAccuracy = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTAccuracy);
      ITable pAdjustments = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTAdjustments);
      ITable pControl = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);
      ITable pJobObjects= m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTJobObjects);
      ITable pJobs = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTJobs);
      ITable pPlans= m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);

      int iAccuracyRowCount = pAccuracy.RowCount(null);
      int iAdjustmentsRowCount = pAdjustments.RowCount(null);
      int iControlRowCount = pControl.RowCount(null);
      int iJobObjectsRowCount = pJobObjects.RowCount(null);
      int iJobRowCount = pJobs.RowCount(null);
      int iPlanRowCount = pPlans.RowCount(null);

      #endregion

      #region Set up Cancel Tracker for Search
      pProgressorDialogFact = new ProgressDialogFactoryClass();
      pTrackCancel = new CancelTrackerClass();
      pStepProgressor = pProgressorDialogFact.Create(pTrackCancel, pApp.hWnd);
      IProgressDialog2 pProgressorDialog = (IProgressDialog2)pStepProgressor;
      pStepProgressor.MinRange = 1;
      pStepProgressor.MaxRange = iTotalRowCount + iPointRowCount; //adding an extra point row count because it's done twice
      pStepProgressor.StepValue = 1;
      pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
      pProgressorDialog.ShowDialog();
      pStepProgressor.Message = "Searching all " + iTotalRowCount.ToString() + " fabric records...";
      #endregion

      bool bIsFileBasedGDB = true;
      bool bIsUnVersioned = true;

      FabricUTILS.GetFabricPlatform(pWS, m_pCadaFab, out bIsFileBasedGDB,
        out bIsUnVersioned);

      List<int> LinesWithSameFromTo = new List<int>();
      List<int> OrphanLinePointsList = new List<int>();
      IQueryFilter pQuFilter = new QueryFilterClass();
      List<int> pLevels = new List<int>();
      Hashtable pHashLevels = new Hashtable();
      List<int> InvalidVectors = new List<int>();

      List<string> m_InClausePointsNotConnectedToLines;
      List<string> m_InClauseLinesNotPartOfParcels;
      List<string> m_InClauseParcelsHaveNoLines;
      List<string> m_InClauseLinePointsWithMissingPointRefs;
      List<string> m_InClauseLinesWithSameFromAndTo;
      List<string> m_InClauseInvalidFeatureAdjustmentVectors;
      List<string> m_InClauseInvalidFeatureClassAssociations;

      int m_CursorCnt = 0;
      string m_sErrMessage = "";
      try
      {
        m_pStartTime = new TimeClass();
        m_pStartTime.SetFromCurrentLocalTime();

        bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

        #region Points Table Cursor
        m_sErrMessage = "Searching fabric Point records.";
        if (bShowProgressor)
          pStepProgressor.Message = "Searching " + iPointRowCount.ToString() + " fabric point records...";

        bool bCont = true;
        pQuFilter.WhereClause = "";
        pQuFilter.SubFields = pPoints.OIDFieldName;
        ICursor pPointCur = pPoints.Search(pQuFilter, false);
        Dictionary<int, bool> OrphanPointsList_DICT = new Dictionary<int, bool>(); //Dictionary used here for faster Removal of elements later
        //List<int> OrphanPointsList = new List<int>();
        IRow pRow = pPointCur.NextRow();
        while (pRow != null)
        {
          m_CursorCnt++;
          int iRow = pRow.OID;
          OrphanPointsList_DICT.Add(iRow, false);
          Marshal.ReleaseComObject(pRow);
          //after garbage collection, and before gettng the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pPointCur.NextRow();
        }
        if (pPointCur != null)
          do { } while (Marshal.FinalReleaseComObject(pPointCur) > 0);

        if (!bCont)
          return;
        #endregion

        #region Parcels Table Cursor
        m_sErrMessage = "Searching fabric Parcels records.";

        if (bShowProgressor)
          pStepProgressor.Message = "Searching " + iParcelRowCount.ToString() + " fabric parcel records...";
        bCont = true;

        m_sErrMessage = "Parcels records query:";

        int idxPlanIDFld = pParcels.FindField("PLANID");
        string PlanIDFldName = pParcels.Fields.get_Field(idxPlanIDFld).Name;
        int idxConstructionFldName = pParcels.FindField("CONSTRUCTION");
        string ConstructionFldName = pParcels.Fields.get_Field(idxConstructionFldName).Name;

        pQuFilter.SubFields = pParcels.OIDFieldName + "," + PlanIDFldName + "," + ConstructionFldName;
        m_sErrMessage += pQuFilter.SubFields;

        m_sErrMessage = "Parcels records query:";
        //Exclude the System Construction parcel. Risky to delete it. TODO: research.
        //pQuFilter.WhereClause = "NOT(" + sPref + "PLANID" + sSuff + "=-1 AND " + sPref + "CONSTRUCTION" + sSuff + "=1)";
        pQuFilter.WhereClause = "NOT(" + PlanIDFldName + "=-1 AND " + ConstructionFldName + "=1)";
        //5/27/2014: The system construction parcel is used/defined by (PLANID=-1).
        //It does not have any lines, and is just used/created on NON remote dbs as a way to hold onto
        //the last OID for databases that reset the OID back to 1 when ALL parcels are deleted
        //therefore exclude System Construction from the parcels list

        m_sErrMessage += pQuFilter.WhereClause;

        Dictionary<int, bool> ParcelsWithNoLinesList_DICT = new Dictionary<int, bool>();
        List<int> ParcelIDs = new List<int>(iParcelRowCount);

        m_CursorCnt = 0;
        ICursor pParcelCur = pParcels.Search(pQuFilter, false);
        m_sErrMessage = "Parcels records query: Search";
        pRow = pParcelCur.NextRow();
        while (pRow != null)
        {
          m_CursorCnt++;
          int iRow = pRow.OID;
          ParcelIDs.Add(iRow);
          bool bIsConstruction = false;
          object obj = pRow.get_Value(idxConstructionFldName);
          if(obj!=DBNull.Value)
          {
            int iVal = (int)obj; //Convert.ToInt32(obj);
            bIsConstruction=(iVal==1);
          }
          ParcelsWithNoLinesList_DICT.Add(iRow, bIsConstruction);
          Marshal.ReleaseComObject(pRow);
          //after garbage collection, and before gettng the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pParcelCur.NextRow();
        }
        if (pParcelCur != null)
          do { } while (Marshal.FinalReleaseComObject(pParcelCur) > 0);

        if (!bCont)
          return;
        pQuFilter.SubFields = "";
        pQuFilter.WhereClause = "";
        #endregion

        #region Lines Table Cursor
        string sPref; string sSuff;
        ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
        sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
        sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

        if (bShowProgressor)
          pStepProgressor.Message = "Searching " + iLineRowCount.ToString() + " fabric line records...";
        m_sErrMessage = "Searching fabric line records.";

        List<string> sFromToPair = new List<string>(iLineRowCount);
        List<int> OrphanLinesList = new List<int>();
        //List<int> ConnectedToUnjoined = new List<int>();
        Dictionary<int,long> FromToPair_DICT = new Dictionary<int,long>();
        List<Int64> FromToPair = new List<long>();

        #region out of memory code...to Research
        //TODO: The following code consumes more resources than the code that follows even though it is technically the same.
        //The code throws a memory exception, hence the "hard-coded" repeated calls to the AddToIntegerListsFromTable3 function that follows. 
        //Is the for loop not releasing resources between each iteration?
        //int iLineBlock=5000000;
        //int iRem=0;
        //int iHowManyLineBlocks= Math.DivRem(iLineRowCount,iLineBlock, out iRem);
        //string sWhereClausePref = sPref + pLines.OIDFieldName.ToString() + sSuff;
        //string sWhereClause = "";
        //if (iLineBlock > iLineRowCount)
        //{ 
        //  sWhereClause = sWhereClausePref + ">= 0";
        //  bCont = AddToIntegerListsFromTable3(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref LinesWithSameFromTo,
        //  ref sFromToPair, sWhereClause, bShowProgressor, pTrackCancel);
        //  if (!bCont)
        //    return;
        //}
        //else
        //{
        //  for (int j = 0; j <= iHowManyLineBlocks; j++)
        //  {
        //    sWhereClause = "";
        //    if (j == 0)
        //      sWhereClause = sWhereClausePref + "< " + iLineBlock.ToString();
        //    else if (j == iHowManyLineBlocks)
        //      sWhereClause = sWhereClausePref + ">= " + (j * iLineBlock).ToString();
        //    else
        //      sWhereClause = sWhereClausePref + ">= " + (j * iLineBlock).ToString() + " AND " +
        //        sWhereClausePref + "< " + ((j + 1) * iLineBlock).ToString();

        //    bCont = AddToIntegerListsFromTable3(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref LinesWithSameFromTo,
        //      ref sFromToPair, sWhereClause, bShowProgressor, pTrackCancel);
        //    if (!bCont)
        //      return;
        //  }
        //}
        #endregion
        
        //string sAns = FabricUTILS.RegValue(RegistryHive.LocalMachine, "SOFTWARE\\ESRI\\Desktop10.0", "32Bit");
        //bool bIs32Bit = (sAns == "TRUE");

        //sAns = FabricUTILS.RegValue(RegistryHive.LocalMachine, "SOFTWARE\\ESRI\\Desktop10.0", "64Bit");
        //bool bIs64Bit = (sAns == "TRUE");

        //if (!bIs64Bit)
        //{
        //  sAns = FabricUTILS.RegValue(RegistryHive.LocalMachine, "SOFTWARE\\ESRI\\Desktop10.1", "64Bit");
        //  bIs64Bit = (sAns == "TRUE");
        //}

        //if (bIs64Bit)
        //  bIs32Bit = false;
        //else
        //  bIs32Bit = true;

        bool bRegkey = false;
        //bool bRegkey = true;

        if (!bRegkey)
        #region well-tested method without using the Int64 fromtopair key
        {
          ParcelIDs.Sort();

          #region Check desktop release
          bool bIsDesktop100 = false;
          string sVersion = Application.ProductVersion;
          string[] VersionPart = sVersion.Split('.');
          if (VersionPart[0].Trim() == "10" && VersionPart[1].Trim() == "0")
            bIsDesktop100 = true;
          else
            bIsDesktop100 = false;
          #endregion

          if (bIsDesktop100)
          {
            #region Process without geom tests
            string sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + "< 5000000";
            bCont = AddToIntegerListsFromTableNoGeomTESTS(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 0;

            sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 5000000 AND "
              + sPref + pLines.OIDFieldName.ToString() + sSuff + "< 10000000";
            bCont = AddToIntegerListsFromTableNoGeomTESTS(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 5000000;

            sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 10000000 AND "
              + sPref + pLines.OIDFieldName.ToString() + sSuff + "< 15000000";
            bCont = AddToIntegerListsFromTableNoGeomTESTS(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 10000000;

            sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 15000000";
            bCont = AddToIntegerListsFromTableNoGeomTESTS(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 15000000;
            #endregion
          }
          else
          {
            #region Process with checks on line geometry (fails on 10.0 clients)
            string sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + "< 5000000";
            bCont = AddToIntegerListsFromTable3(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 0;

            sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 5000000 AND "
              + sPref + pLines.OIDFieldName.ToString() + sSuff + "< 10000000";
            bCont = AddToIntegerListsFromTable3(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 5000000;

            sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 10000000 AND "
              + sPref + pLines.OIDFieldName.ToString() + sSuff + "< 15000000";
            bCont = AddToIntegerListsFromTable3(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 10000000;

            sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 15000000";
            bCont = AddToIntegerListsFromTable3(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref ParcelIDs,
            ref LinesWithSameFromTo, ref sFromToPair, ref OrphanLinesList, sWhereClause, bShowProgressor, pTrackCancel);
            if (!bCont)
              return;
            m_CursorCnt = 15000000;
            #endregion
          }
        }
        #endregion
        else
        #region alternative newer method using FromToPairDictionary and combined-Int64 key of 2 Int32's
        //{//this method is aimed at reducing the large memory footprint of the FromToPair string-based list
        //  //however it appears to use more memory than the well-tested approach and fails with out-of-mem-message
        //  string sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + "< 5000000";
        //  bCont = AddToIntegerListsFromTable4(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref LinesWithSameFromTo,
        //    ref FromToPair, sWhereClause, bShowProgressor, pTrackCancel);
        //  if (!bCont)
        //    return;
        //  m_CursorCnt = 0;

        //  sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 5000000 AND "
        //    + sPref + pLines.OIDFieldName.ToString() + sSuff + "< 10000000";
        //  bCont = AddToIntegerListsFromTable4(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref LinesWithSameFromTo,
        //    ref FromToPair, sWhereClause, bShowProgressor, pTrackCancel);
        //  if (!bCont)
        //    return;
        //  m_CursorCnt = 5000000;

        //  sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 10000000 AND "
        //    + sPref + pLines.OIDFieldName.ToString() + sSuff + "< 15000000";
        //  bCont = AddToIntegerListsFromTable4(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref LinesWithSameFromTo,
        //    ref FromToPair, sWhereClause, bShowProgressor, pTrackCancel);
        //  if (!bCont)
        //    return;
        //  m_CursorCnt = 10000000;

        //  sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + ">= 15000000";
        //  bCont = AddToIntegerListsFromTable4(pLines, ref OrphanPointsList_DICT, ref ParcelsWithNoLinesList_DICT, ref LinesWithSameFromTo,
        //    ref FromToPair, sWhereClause, bShowProgressor, pTrackCancel);
        //  if (!bCont)
        //    return;
        //  m_CursorCnt = 15000000;
        //}

        #endregion

        m_sErrMessage = "Add the Orphan point Dictionary elements to the orphan points List.";
        List<int> OrphanPointsList = new List<int>();
        List<KeyValuePair<int, bool>> list = OrphanPointsList_DICT.ToList();
        // Loop over list.
        foreach (KeyValuePair<int, bool> pair in list)
        {
          int iVal = pair.Key;
          OrphanPointsList.Add(iVal);
        }

        OrphanPointsList_DICT.Clear();
        OrphanPointsList_DICT = null;

        m_sErrMessage = "Add the remaining parcel Dictionary elements to the parcels with no lines List.";
        List<int> ParcelsWithNoLinesList = new List<int>();
        List<KeyValuePair<int, bool>> list2 = ParcelsWithNoLinesList_DICT.ToList();
        // Loop over list.
        foreach (KeyValuePair<int, bool> pair in list2)
        {
          int iVal = pair.Key;
          if (!ParcelsWithNoLinesList_DICT[iVal]) //if it's not a construction
            ParcelsWithNoLinesList.Add(iVal);
        }
       
        #endregion

        //TODO: need research reversing this process, and collecting the line-points first, 
        //and then detecting orphan line-points when looping through lines

        #region Re-building the point list- Points Table Cursor
        m_sErrMessage = "Re-building fabric Point list.";
        if (bShowProgressor)
          pStepProgressor.Message = "Preparing for line-point search...";

        bCont = true;

        pQuFilter.SubFields = pPoints.OIDFieldName;
        pQuFilter.WhereClause = "";
        ICursor pPointsCur2 = pPoints.Search(pQuFilter, false);
        List<int> PointList = new List<int>();
        pRow = pPointsCur2.NextRow();
        while (pRow != null)
        {
          int iRow = pRow.OID;
          PointList.Add(iRow);
          Marshal.ReleaseComObject(pRow);
          //after garbage collection, and before gettng the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pPointsCur2.NextRow();
        }
        if (pPointsCur2 != null)
          do { } while (Marshal.FinalReleaseComObject(pPointsCur2) > 0);

        if (!bCont)
          return;
        #endregion
         PointList.Sort();
       
        #region LinePoints Table Cursor
        m_sErrMessage = "Searching fabric LinePoints records.";
        if (bShowProgressor)
          pStepProgressor.Message = "Searching " + iLinePointRowCount.ToString() + " fabric linepoint records...";

        bCont = true;
        
        int iLinePtPointIdIdx = pLinePts.FindField("LINEPOINTID");
        int iLinePtFromPtIdIdx = pLinePts.FindField("FROMPOINTID");
        int iLinePtToPtIdIdx = pLinePts.FindField("TOPOINTID");

        string LinePointPtIdFldName = pLinePts.Fields.get_Field(iLinePtPointIdIdx).Name;
        string LinePointFromPtIdFldName = pLinePts.Fields.get_Field(iLinePtFromPtIdIdx).Name;
        string LinePointToPtIdFldName = pLinePts.Fields.get_Field(iLinePtToPtIdIdx).Name;

        pQuFilter.SubFields = pLinePts.OIDFieldName + "," + LinePointPtIdFldName + "," + LinePointFromPtIdFldName + "," + LinePointToPtIdFldName;

        m_sErrMessage = "Sort sFromToPair";
        if (!bRegkey)
          sFromToPair.Sort();
        else
          FromToPair.Sort();

        m_CursorCnt = 0;
        ICursor pLPCur = pLinePts.Search(pQuFilter, false);
        pRow = pLPCur.NextRow();
        while (pRow != null)
        {
          m_CursorCnt++;
          int iThisID = (int)pRow.get_Value(iLinePtPointIdIdx);
          int iFromPtID = (int)pRow.get_Value(iLinePtFromPtIdIdx);
          int iToPtID = (int)pRow.get_Value(iLinePtToPtIdIdx);

          if (!bRegkey)
          {
            string sFromID = iFromPtID.ToString();
            string sToID = iToPtID.ToString();
            string sThisID = iThisID.ToString();
            string sLineRef = sFromID + ":" + sToID;
            string sLineRefRev = sToID + ":" + sFromID;
            if ((sFromToPair.BinarySearch(sLineRef) < 0) && (sFromToPair.BinarySearch(sLineRefRev) < 0))
              OrphanLinePointsList.Add(pRow.OID);
            else if (sFromID==sToID || sFromID==sThisID || sToID==sThisID)
              OrphanLinePointsList.Add(pRow.OID);
            else if (PointList.BinarySearch(iThisID) < 0)
              OrphanLinePointsList.Add(pRow.OID);//the from and to's are handled off the lines
            }
          else
          {
            //create a key for the line's from/to id combination
            //fwd
            uint u1 = (uint)iFromPtID;
            uint u2 = (uint)iToPtID;
            ulong unsignedkey = (((ulong)u1) << 32) | u2;
            long qFwdKey = (long)unsignedkey;
            //reverse
            uint u3 = (uint)iToPtID;
            uint u4 = (uint)iFromPtID;
            ulong unsignedkey2 = (((ulong)u3) << 32) | u4;
            long qRevKey = (long)unsignedkey2;

            if ((FromToPair.BinarySearch(qFwdKey) < 0) && (FromToPair.BinarySearch(qRevKey) < 0))
              OrphanLinePointsList.Add(pRow.OID);
          }

          Marshal.ReleaseComObject(pRow);
          //after garbage collection, and before getting the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pLPCur.NextRow();
        }
        if (pLPCur != null)
          do { } while (Marshal.FinalReleaseComObject(pLPCur) > 0);

        if (!bCont)
          return;

        //now remove repeats and sort
        List<int> temp = OrphanLinePointsList.Distinct().ToList();
        OrphanLinePointsList = temp.ToList();
        temp.Clear();
        temp = null;

        #endregion

        #region Vector Table Cursor

        m_sErrMessage = "Vector records query: Search";
        if (bShowProgressor)
          pStepProgressor.Message = "Searching " + iVectorRowCount.ToString() + " fabric vector records...";
        bCont = true;
        int idxFromX = pVectors.FindField("FROMX");
        int idxFromY = pVectors.FindField("FROMY");
        int idxToX = pVectors.FindField("TOX");
        int idxToY = pVectors.FindField("TOY");
        int idxAdjLvl = pVectors.FindField("ADJLEVEL");

        string FromXFldName = pVectors.Fields.get_Field(idxFromX).Name;
        string FromYFldName = pVectors.Fields.get_Field(idxFromY).Name;
        string ToXFldName = pVectors.Fields.get_Field(idxToX).Name;
        string ToYFldName = pVectors.Fields.get_Field(idxToY).Name;
        string AdjLvlFldName = pVectors.Fields.get_Field(idxAdjLvl).Name;

        pQuFilter.SubFields = pVectors.OIDFieldName + "," + FromXFldName + "," + FromYFldName + ","
                + ToXFldName + "," + ToYFldName + "," + AdjLvlFldName;

        ICursor pVectCur = pVectors.Search(pQuFilter, false);
        pRow = pVectCur.NextRow();
        while (pRow != null)
        {
          m_CursorCnt++;
          int iRow = pRow.OID;
          object obj = pRow.get_Value(idxAdjLvl);
          if (obj == DBNull.Value)
            InvalidVectors.Add(iRow);
          else
          {
            int iVal = (int)obj;
            if (iVal < 0)
              InvalidVectors.Add(iRow);
          }
          try
          {
            obj = pRow.get_Value(idxToX);
            Convert.ToDouble(obj);
            object obj2 = pRow.get_Value(idxToY);
            Convert.ToDouble(obj2);
            object obj3 = pRow.get_Value(idxFromX);
            Convert.ToDouble(obj3);
            object obj4 = pRow.get_Value(idxFromY);
            Convert.ToDouble(obj4);
          }
          catch
          {//if getting any of the x or y values fail, then the vector is bad.
            InvalidVectors.Add(iRow);
          }
          Marshal.ReleaseComObject(pRow);
          //after garbage collection, and before gettng the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pVectCur.NextRow();
        }

        if (pVectCur != null)
          do { } while (Marshal.FinalReleaseComObject(pVectCur) > 0);

        if (!bCont)
          return;

        //now remove repeats
        List<int> temp5 = InvalidVectors.Distinct().ToList();
        InvalidVectors = temp5.ToList();
        temp5.Clear();
        temp5 = null;

        #endregion

        #region AdjustmentLevel Table Cursor
        m_sErrMessage = "Adjustment records query: Search";
        if (bShowProgressor)
          pStepProgressor.Message = "Searching " + iLevelRowCount.ToString() + " adjustment level records...";
        bCont = true;

        int iFC = pAdjLevels.FindField("FeatureClassID");
        string FeatClassIDFldName = pAdjLevels.Fields.get_Field(iFC).Name;

        pQuFilter.SubFields = pAdjLevels.OIDFieldName + "," + FeatClassIDFldName;
        ICursor pAdjLvlCur = pAdjLevels.Search(pQuFilter, false);
        pRow = pAdjLvlCur.NextRow();

        while (pRow != null)
        {
          int iFCID = (int)pRow.get_Value(iFC);
          if (iFCID > 0)
          {
            int iOID = -1;
            iOID = pRow.OID;
            if (!pLevels.Contains(iFCID))
            {
              pLevels.Add(iFCID);
              pHashLevels.Add(iFCID, (int)iOID);
            }
          }
          Marshal.FinalReleaseComObject(pRow);
          //after garbage collection, and before gettng the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pAdjLvlCur.NextRow();
        }
        if (pAdjLvlCur != null)
          do { } while (Marshal.FinalReleaseComObject(pAdjLvlCur) > 0);

        if (!bCont)
          return;

        //Get the feature class id's
        #region Refine the list and remove valid feature class ids
        if (pLevels.Count() > 0)
        {
          //run through standalone feature classes
          IEnumDataset pEnumDS = pWS.get_Datasets(esriDatasetType.esriDTFeatureClass);
          pEnumDS.Reset();

          IFeatureClass pFC = (IFeatureClass)pEnumDS.Next();
          while (pFC != null)
          {
            if (pLevels.Contains(pFC.FeatureClassID))
              pLevels.Remove(pFC.FeatureClassID);
            pFC = (IFeatureClass)pEnumDS.Next();
          }

          //run through feature classes in feature datasets
          pEnumDS = pWS.get_Datasets(esriDatasetType.esriDTFeatureDataset);
          pEnumDS.Reset();

          IFeatureClassContainer pFCCont = (IFeatureClassContainer)pEnumDS.Next();

          while (pFCCont != null)
          {
            IEnumFeatureClass pEnumFC = pFCCont.Classes;
            pEnumFC.Reset();
            pFC = pEnumFC.Next();
            while (pFC != null)
            {
              Debug.WriteLine(pFC.FeatureClassID.ToString());
              if (pLevels.Contains(pFC.FeatureClassID))
                pLevels.Remove(pFC.FeatureClassID);
              pFC = pEnumFC.Next();
            }
            pFCCont = (IFeatureClassContainer)pEnumDS.Next();
          }
        }
        #endregion

        #region build an FIDSet if there are levels to delete
        List<int> pLevelInvalidLevels = new List<int>();

        if (pLevels.Count > 0)
        {
          //made it here so some invalid levels exist
          foreach (int i in pLevels)
          {
            int j = (int)pHashLevels[i];//temp
            Debug.WriteLine(j.ToString());
            pLevelInvalidLevels.Add((int)pHashLevels[i]);
          }
        }
        pHashLevels.Clear();
        pHashLevels = null;
        #endregion

        #endregion

        #region Final query for orphan lines (Commented out- NOT Implemented - reverted to original method)
        //This query was not reliabe under circumstances where there was limited RAM on a 32-bit machine.
        //Very risky query, and retruned 800K lines for deletion, when the correct number was 50K
        //if (bShowProgressor)
        //  pStepProgressor.Message = "Searching for inconsistent records...";

        //m_sErrMessage = "Searching for Orphan lines: ";

        //// Find any lines which are not associated with a valid parcel.
        //// Use a SQL query similar to
        //// SELECT * FROM xxxx  WHERE
        //// ParcelID not in (select OBJECTID from cedata.CEDATA.EncinitasFabric_Parcels)

        //int iParcelID = pLines.FindField("PARCELID");
        //string ParcelIDFldName = pLines.Fields.get_Field(iParcelID).Name;
        //string ParcelTableName = pDS.Name;

        //pQuFilter.WhereClause = ParcelIDFldName + " not in (select " + pParcels.OIDFieldName + " from " + ParcelTableName + ")";
        //pQuFilter.SubFields = pLines.OIDFieldName;
        //m_sErrMessage += pQuFilter.WhereClause;
        //List<int> OrphanLinesList = new List<int>();
        //if (!FabricUTILS.GetListFromQuery(pLines, pQuFilter, out OrphanLinesList, bShowProgressor, pTrackCancel))
        //  return;

        #endregion

        if (pProgressorDialog != null)
          pProgressorDialog.HideDialog();
        pProgressorDialog = null;

        #region Assign dialog properties
        dlgInconsistentRecords pInconsistenciesDialog = new dlgInconsistentRecords();
        pInconsistenciesDialog.OrphanPoints = OrphanPointsList;
        pInconsistenciesDialog.LinesWithSameFromTo = LinesWithSameFromTo;
        pInconsistenciesDialog.OrphanLines = OrphanLinesList;
        pInconsistenciesDialog.OrphanLinePoints = OrphanLinePointsList;
        pInconsistenciesDialog.ParcelsWithNoLines = ParcelsWithNoLinesList;
        pInconsistenciesDialog.InvalidVectors = InvalidVectors;
        pInconsistenciesDialog.InvalidFeatureClassAssociations = pLevelInvalidLevels;

        pInconsistenciesDialog.ParcelCount = iParcelRowCount;
        pInconsistenciesDialog.LineCount = iLineRowCount;
        pInconsistenciesDialog.PointCount = iPointRowCount;
        pInconsistenciesDialog.LinePointCount = iLinePointRowCount;
        pInconsistenciesDialog.VectorCount = iVectorRowCount;
        pInconsistenciesDialog.LevelsCount = iLevelRowCount;

        pInconsistenciesDialog.AccuracyCategoryCount = iAccuracyRowCount;
        pInconsistenciesDialog.AdjustmentsCount = iAdjustmentsRowCount;
        pInconsistenciesDialog.ControlPointCount = iControlRowCount;
        pInconsistenciesDialog.JobObjectCount = iJobObjectsRowCount;
        pInconsistenciesDialog.JobCount = iJobRowCount;
        pInconsistenciesDialog.PlanCount = iPlanRowCount;
        pInconsistenciesDialog.OIDFieldName = sPref + pParcels.OIDFieldName + sSuff;

        m_pEndTime = new TimeClass();
        m_pEndTime.SetFromCurrentLocalTime();
        ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
        pInconsistenciesDialog.ProcessTime = HowLong.Hours.ToString("00") + "h "
                  + HowLong.Minutes.ToString("00") + "m "
                  + HowLong.Seconds.ToString("00") + "s";

        string sFabricName = pWS.PathName + "\\" + pDS.Name;
        string sFabricName2 = sFabricName.ToLower();
        pInconsistenciesDialog.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_parcel"));
        pInconsistenciesDialog.VersionName = FabricUTILS.GetVersionName(pWS);

        #endregion

        #region display the dialog and act on button press
        //Display the dialog
        DialogResult pDialogResult = pInconsistenciesDialog.ShowDialog();

        if (pDialogResult != DialogResult.OK)
        {
          pInconsistenciesDialog = null;
          return;
        }

        int iDeleteCount = 0;
        if (pInconsistenciesDialog.CheckInvalidFeatureAdjustmentVectors)
          iDeleteCount += InvalidVectors.Count;
        if (pInconsistenciesDialog.CheckLinesWithSameFromAndTo)
          iDeleteCount += LinesWithSameFromTo.Count;
        if (pInconsistenciesDialog.CheckPointsNotConnectedToLines)
          iDeleteCount += OrphanPointsList.Count;
        if (pInconsistenciesDialog.CheckLinesNotPartOfParcels)
          iDeleteCount += OrphanLinesList.Count;
        if (pInconsistenciesDialog.CheckLinePointsWithNoFabricPoints)
          iDeleteCount += OrphanLinePointsList.Count;
        if (pInconsistenciesDialog.CheckParcelsHaveNoLines)
          iDeleteCount += ParcelsWithNoLinesList.Count;
        if (pInconsistenciesDialog.CheckInvalidFeatureClassAssociations)
          iDeleteCount += pLevelInvalidLevels.Count;
        #endregion

        #region Set up Cancel Tracker
        if (bShowProgressor)
        {
          pStepProgressor = pProgressorDialogFact.Create(pTrackCancel, pApp.hWnd);
          pProgressorDialog = (IProgressDialog2)pStepProgressor;
          pStepProgressor.MinRange = 1;
          pStepProgressor.MaxRange = iDeleteCount;
          pStepProgressor.StepValue = 1;
          pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
          pProgressorDialog.ShowDialog();
          //pStepProgressor.Message = "Searching all " + iTotalRowCount.ToString() + " fabric records...";
        }
        #endregion

        #region DeleteInvalidRecords

        if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
          return;

        #region DeleteVectors
        m_sErrMessage = "Deleting invalid vectors";
        if (pInconsistenciesDialog.CheckInvalidFeatureAdjustmentVectors)
        {
          m_InClauseInvalidFeatureAdjustmentVectors = FabricUTILS.InClauseFromOIDsList(InvalidVectors, iTokenMax);
          if (m_InClauseInvalidFeatureAdjustmentVectors[0].Trim() != "")
          {//setup progressor dialog for Delete
            if (bShowProgressor)
              pStepProgressor.Message = "Deleting invalid vectors...";

            if (!FabricUTILS.DeleteByInClause(pWS, pVectors, pVectors.Fields.get_Field(pVectors.FindField(pVectors.OIDFieldName)),
              m_InClauseInvalidFeatureAdjustmentVectors, !bIsUnVersioned, pStepProgressor, pTrackCancel))
            {
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
        }
        #endregion

        #region DeleteInvalidAssociations
        m_sErrMessage = "Deleting invalid feature class associations";
        if (pInconsistenciesDialog.CheckInvalidFeatureClassAssociations)
        {
          m_InClauseInvalidFeatureClassAssociations = FabricUTILS.InClauseFromOIDsList(pLevelInvalidLevels, iTokenMax);
          if (m_InClauseInvalidFeatureClassAssociations[0].Trim() != "")
          {//setup progressor dialog for Delete
            if (bShowProgressor)
              pStepProgressor.Message = "Deleting invalid feature class associations...";

            if (!FabricUTILS.DeleteByInClause(pWS, pAdjLevels, pAdjLevels.Fields.get_Field(pAdjLevels.FindField(pAdjLevels.OIDFieldName)),
              m_InClauseInvalidFeatureClassAssociations, !bIsUnVersioned, pStepProgressor, pTrackCancel))
            {
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
        }
        #endregion

        #region DeleteSameFromTo
        m_sErrMessage = "Deleting same from to";
        if (pInconsistenciesDialog.CheckLinesWithSameFromAndTo)
        {
          m_InClauseLinesWithSameFromAndTo = FabricUTILS.InClauseFromOIDsList(LinesWithSameFromTo, iTokenMax);
          if (m_InClauseLinesWithSameFromAndTo[0].Trim() != "")
          {//setup progressor dialog for Delete
            if (bShowProgressor)
              pStepProgressor.Message = "Deleting lines with same From and To points...";

            if (!FabricUTILS.DeleteByInClause(pWS, pLines, pLines.Fields.get_Field(pLines.FindField(pLines.OIDFieldName)),
              m_InClauseLinesWithSameFromAndTo, !bIsUnVersioned, pStepProgressor, pTrackCancel))
            {
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
        }
        #endregion

        #region DeleteOrphanPoints
        m_sErrMessage = "Deleting orphan points";
        if (pInconsistenciesDialog.CheckPointsNotConnectedToLines)
        {
          m_InClausePointsNotConnectedToLines = FabricUTILS.InClauseFromOIDsList(OrphanPointsList, iTokenMax);
          if (m_InClausePointsNotConnectedToLines[0].Trim() != "")
          {//setup progressor dialog for Delete
            if (bShowProgressor)
              pStepProgressor.Message = "Deleting points not connected to lines (orphan points)...";

            if (!FabricUTILS.DeleteByInClause(pWS, pPoints, pPoints.Fields.get_Field(pPoints.FindField(pPoints.OIDFieldName)),
              m_InClausePointsNotConnectedToLines, !bIsUnVersioned, pStepProgressor, pTrackCancel))
            {
              FabricUTILS.AbortEditing(pWS);
              return;
            }
            //need to also take care of control points that have a reference to any of the deleted Orphan points
            ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
            int idxNameFldOnControl = pControl.FindField("POINTID");
            string ControlNameFldName = pControl.Fields.get_Field(idxNameFldOnControl).Name;

            int idxActiveIDX = pControl.Fields.FindField("ACTIVE");
            string ActiveFldName = pControl.Fields.get_Field(idxActiveIDX).Name;

            if (bShowProgressor)
              pStepProgressor.Message = "Resetting control references to points ...";

            int iCnt = m_InClausePointsNotConnectedToLines.Count - 1;
            for (int z = 0; z <= iCnt; z++)
            {
              if ((m_InClausePointsNotConnectedToLines[z].Trim() == ""))
                break;
              //cleanup associated control points, and associations where underlying points were deleted 
              pQuFilter.SubFields = ControlNameFldName + "," + ActiveFldName;
              pQuFilter.WhereClause = ControlNameFldName + " IN (" + m_InClausePointsNotConnectedToLines[z] + ")";
              pSchemaEd.ReleaseReadOnlyFields(pControl, esriCadastralFabricTable.esriCFTControl); //release safety-catch
              if (!FabricUTILS.ResetControlAssociations(pControl, pQuFilter, bIsUnVersioned))
              {
                pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
                FabricUTILS.AbortEditing(pWS);
                return;
              }
            }
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on

          }
        }
        #endregion

        #region DeleteOrphanLines
        m_sErrMessage = "Deleting orphan lines";
        if (pInconsistenciesDialog.CheckLinesNotPartOfParcels)
        {
          m_InClauseLinesNotPartOfParcels = FabricUTILS.InClauseFromOIDsList(OrphanLinesList, iTokenMax);
          if (m_InClauseLinesNotPartOfParcels[0].Trim() != "")
          {//setup progressor dialog for Delete
            if (bShowProgressor)
              pStepProgressor.Message = "Deleting lines not connected to parcels (orphan lines)...";
            if (!FabricUTILS.DeleteByInClause(pWS, pLines, pLines.Fields.get_Field(pLines.FindField(pLines.OIDFieldName)),
              m_InClauseLinesNotPartOfParcels, !bIsUnVersioned, pStepProgressor, pTrackCancel))
            {
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
        }
        #endregion

        #region DeleteOrphanLinePoints
        m_sErrMessage = "Deleting orphan line points";
        if (pInconsistenciesDialog.CheckLinePointsWithNoFabricPoints)
        {
          m_InClauseLinePointsWithMissingPointRefs = FabricUTILS.InClauseFromOIDsList(OrphanLinePointsList, iTokenMax);
          if (m_InClauseLinePointsWithMissingPointRefs[0].Trim() != "")
          {
            if (bShowProgressor)
              pStepProgressor.Message = "Deleting parcels that have no lines...";

            if (!FabricUTILS.DeleteByInClause(pWS, pLinePts, pLinePts.Fields.get_Field(pLinePts.FindField(pLinePts.OIDFieldName)),
              m_InClauseLinePointsWithMissingPointRefs, !bIsUnVersioned, pStepProgressor, pTrackCancel))
            {
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
        }
        #endregion

        #region DeleteParcelsWithNoLines
        m_sErrMessage = "Deleting parcels with no lines";
        if (pInconsistenciesDialog.CheckParcelsHaveNoLines)
        {
          m_InClauseParcelsHaveNoLines = FabricUTILS.InClauseFromOIDsList(ParcelsWithNoLinesList, iTokenMax);
          if (m_InClauseParcelsHaveNoLines[0].Trim() != "")
          {
            if (bShowProgressor)
              pStepProgressor.Message = "Deleting parcels that have no lines...";
            if (!FabricUTILS.DeleteByInClause(pWS, pParcels, pParcels.Fields.get_Field(pParcels.FindField(pParcels.OIDFieldName)),
              m_InClauseParcelsHaveNoLines, !bIsUnVersioned, pStepProgressor, pTrackCancel))
            {
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
        }
        #endregion

        if (bShowProgressor)
          pStepProgressor.Message = "Saving changes...please wait.";

        #endregion

        if (!(pProgressorDialog == null))
          pProgressorDialog.HideDialog();
        pProgressorDialog = null;
        FabricUTILS.StopEditing(pWS);
      }
      catch (Exception ex)
      {
        FabricUTILS.AbortEditing(pWS);
        MessageBox.Show(ex.Message + Environment.NewLine + m_sErrMessage + Environment.NewLine + 
          "Row Counter:" + m_CursorCnt.ToString(), "Delete Inconsistent Records");
      }

      #region Cleanup
      finally
      {
        if (pProgressorDialog != null)
          pProgressorDialog.HideDialog();

        if (pMouseCursor != null)
          pMouseCursor.SetCursor(0);
        if (pStepProgressor != null)
          pStepProgressor.Hide();

        pStepProgressor = null;
        pHashLevels = null;

      }
      #endregion
    }

    private bool AddToIntegerListsFromTable3(ITable fromTable, ref Dictionary<int, bool> thePointList, 
      ref Dictionary<int, bool> theParcelList_DICT, ref List<int> theParcelList, ref List<int> theList2, ref List<string> FromToPair, 
      ref List<int> OrphanLinesList,string WhereClause, bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;

      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iParcelID = fromTable.FindField("PARCELID");
        int iToPtID = fromTable.FindField("TOPOINTID");
        int iFromPtID = fromTable.FindField("FROMPOINTID");
        int iCtrPtID = fromTable.FindField("CENTERPOINTID");
        int iDistanceFld = fromTable.FindField("DISTANCE");
        int iDensifyType = fromTable.FindField("DENSIFYTYPE");
        //int iCategoryFld = fromTable.FindField("CATEGORY");

        bool bHasDensifyTypeFld = (iDensifyType >= 0);

        string ParcelIDFldName = fromTable.Fields.get_Field(iParcelID).Name;
        string ToPtIDFldName = fromTable.Fields.get_Field(iToPtID).Name;
        string FromPtIDFldName = fromTable.Fields.get_Field(iFromPtID).Name;
        string CtrPtIDFldName = fromTable.Fields.get_Field(iCtrPtID).Name;
        //string CategoryFldName = fromTable.Fields.get_Field(iCategoryFld).Name;
        string DistanceFldName = fromTable.Fields.get_Field(iDistanceFld).Name;
        string DensifyTypeFldName = "";
        if (bHasDensifyTypeFld)
          DensifyTypeFldName=fromTable.Fields.get_Field(iDensifyType).Name;

        IFeatureClass pFC = (IFeatureClass)fromTable;

        pQuFilter.SubFields = pFC.ShapeFieldName + "," + fromTable.OIDFieldName + "," + ParcelIDFldName + "," + ToPtIDFldName + "," +
          FromPtIDFldName + "," + DistanceFldName + "," + CtrPtIDFldName; // +"," + CategoryFldName; // +"," + DensifyTypeFldName;

        if (bHasDensifyTypeFld)
            pQuFilter.SubFields +=  "," + DensifyTypeFldName;

        pCur = fromTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iThisID = (int)pRow.get_Value(iToPtID);
          bool bWasRemoved1 = thePointList.Remove(iThisID);

          int iThisID2 = (int)pRow.get_Value(iFromPtID);
          bool bWasRemoved2=thePointList.Remove(iThisID2);

          //Code started for case of emptying geometry on points that are used by unjoined lines
          //not currently implemented, as side effects are benign
          //int iCat = (int)pRow.get_Value(iCategoryFld);
          //bool bRadial = (iCat==4);

          //if ((bWasRemoved1 || bWasRemoved2) && !bRadial)
          //{
          //  IFeature pFeat = null;
          //  pFeat = (IFeature)pRow;
          //  IGeometry pGeom = pFeat.ShapeCopy;
          //  if (pGeom != null)
          //  {
          //    if (pGeom.IsEmpty)
          //    { //Unjoined lines list
          //      ConnectedToUnjoined.Add(iThisID);
          //      ConnectedToUnjoined.Add(iThisID2);
          //    }
          //  }
          //}

          FromToPair.Add(iThisID2.ToString() + ":" + iThisID.ToString());//

          int iThisID3 = -1;
          object obj = pRow.get_Value(iCtrPtID);
          if (obj != DBNull.Value)
          {
            iThisID3 = (int)obj;
            thePointList.Remove(iThisID3);
          }

          //same from and to
          //exclude linestrings, for same from and to points
          bool bIsLinestring = false;

          if (bHasDensifyTypeFld)
          {
            obj = pRow.get_Value(iDensifyType);
            if (obj != DBNull.Value)
            {
              int i = (int)obj;
              if (i > 0)
                bIsLinestring = true;
            }
          }

          if (iThisID == iThisID2 && !bIsLinestring)
          {
            IFeature pFeat = null;
            pFeat = (IFeature)pRow;
            IGeometry pGeom = pFeat.ShapeCopy;
            ISegmentCollection pSegColl = null;
            bool bHasOneSegment = false;//set it false first then prove otherwise
            bHasOneSegment = (pGeom != null);//first make sure geom is not null, first +ve
            if (bHasOneSegment)
              bHasOneSegment = !pGeom.IsEmpty; //make sure geom is not empty, second +ve
            if (bHasOneSegment)
            {
              pSegColl = (ISegmentCollection)pGeom;
              bHasOneSegment=(pSegColl.SegmentCount==1); //final confirmation
            }
            obj = pRow.get_Value(iDistanceFld);
            double dDistance=0;
            if (obj != DBNull.Value)
              dDistance = Convert.ToDouble(obj);
            if (dDistance < 0.5 || bHasOneSegment)
              theList2.Add(iRow);
          }

          //remove parcel ids that are referenced in the lines list
          int iThisID4 = (int)pRow.get_Value(iParcelID);
          theParcelList_DICT.Remove(iThisID4);

          if (theParcelList.BinarySearch(iThisID4)<0)
            OrphanLinesList.Add(iRow);

          Marshal.FinalReleaseComObject(pRow);
          
          //after garbage collection, and before getting the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pCur.NextRow();
        }

        return bCont;
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Delete Inconsistent Records");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private bool AddToIntegerListsFromTableNoGeomTESTS(ITable fromTable, ref Dictionary<int, bool> thePointList,
  ref Dictionary<int, bool> theParcelList_DICT, ref List<int> theParcelList, ref List<int> theList2, ref List<string> FromToPair,
  ref List<int> OrphanLinesList, string WhereClause, bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;

      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iParcelID = fromTable.FindField("PARCELID");
        int iToPtID = fromTable.FindField("TOPOINTID");
        int iFromPtID = fromTable.FindField("FROMPOINTID");
        int iCtrPtID = fromTable.FindField("CENTERPOINTID");
        int iDistanceFld = fromTable.FindField("DISTANCE");
        int iDensifyType = fromTable.FindField("DENSIFYTYPE");
        bool bHasDensifyTypeFld = (iDensifyType >= 0);

        string ParcelIDFldName = fromTable.Fields.get_Field(iParcelID).Name;
        string ToPtIDFldName = fromTable.Fields.get_Field(iToPtID).Name;
        string FromPtIDFldName = fromTable.Fields.get_Field(iFromPtID).Name;
        string CtrPtIDFldName = fromTable.Fields.get_Field(iCtrPtID).Name;
        string DistanceFldName = fromTable.Fields.get_Field(iDistanceFld).Name;
        string DensifyTypeFldName = "";
        if (bHasDensifyTypeFld)
          DensifyTypeFldName = fromTable.Fields.get_Field(iDensifyType).Name;

        string sSubFlds = fromTable.OIDFieldName + "," + ParcelIDFldName + "," + ToPtIDFldName + "," +
          FromPtIDFldName + "," + DistanceFldName + "," + CtrPtIDFldName; // +"," + DensifyTypeFldName;

        if (bHasDensifyTypeFld)
          sSubFlds += "," + DensifyTypeFldName;

        pQuFilter.SubFields = sSubFlds;

        pCur = fromTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iThisID = (int)pRow.get_Value(iToPtID);
          thePointList.Remove(iThisID);

          int iThisID2 = (int)pRow.get_Value(iFromPtID);
          thePointList.Remove(iThisID2);

          FromToPair.Add(iThisID2.ToString() + ":" + iThisID.ToString());//

          int iThisID3 = -1;
          object obj = pRow.get_Value(iCtrPtID);
          if (obj != DBNull.Value)
          {
            iThisID3 = (int)obj;
            thePointList.Remove(iThisID3);
          }

          //same from and to
          //exclude linestrings, for same from and to points
          bool bIsLinestring = false;

          if (bHasDensifyTypeFld)
          {
            obj = pRow.get_Value(iDensifyType);
            if (obj != DBNull.Value)
            {
              int i = (int)obj;
              if (i > 0)
                bIsLinestring = true;
            }
          }

          if (iThisID == iThisID2 && !bIsLinestring)
            theList2.Add(iRow);

          //remove parcel ids that are referenced in the lines list
          int iThisID4 = (int)pRow.get_Value(iParcelID);
          theParcelList_DICT.Remove(iThisID4);

          if (theParcelList.BinarySearch(iThisID4) < 0)
            OrphanLinesList.Add(iRow);

          Marshal.FinalReleaseComObject(pRow);

          //after garbage collection, and before getting the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pCur.NextRow();
        }

        return bCont;
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Delete Inconsistent Records");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private bool AddToIntegerListsFromTable4(ITable fromTable, ref Dictionary<int, bool> thePointList,
      ref Dictionary<int, bool> theParcelList, ref List<int> theList2, ref List<long> FromToPair,
      string WhereClause, bool bShowProgressor, ITrackCancel pTrackCancel)
    {

      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;

      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iParcelID = fromTable.FindField("PARCELID");
        int iToPtID = fromTable.FindField("TOPOINTID");
        int iFromPtID = fromTable.FindField("FROMPOINTID");
        int iCtrPtID = fromTable.FindField("CENTERPOINTID");
        int iDistanceFld = fromTable.FindField("DISTANCE");
        int iDensifyType = fromTable.FindField("DENSIFYTYPE");
        bool bHasDensifyTypeFld = (iDensifyType >= 0);

        string ParcelIDFldName = fromTable.Fields.get_Field(iParcelID).Name;
        string ToPtIDFldName = fromTable.Fields.get_Field(iToPtID).Name;
        string FromPtIDFldName = fromTable.Fields.get_Field(iFromPtID).Name;
        string CtrPtIDFldName = fromTable.Fields.get_Field(iCtrPtID).Name;
        string DistanceFldName = fromTable.Fields.get_Field(iDistanceFld).Name;
        string DensifyTypeFldName = "";
        if (bHasDensifyTypeFld)
          DensifyTypeFldName=fromTable.Fields.get_Field(iDensifyType).Name;

        IFeatureClass pFC = (IFeatureClass)fromTable;

        pQuFilter.SubFields = pFC.ShapeFieldName + "," + fromTable.OIDFieldName + "," + ParcelIDFldName + "," + ToPtIDFldName + "," +
          FromPtIDFldName + "," + DistanceFldName + "," + CtrPtIDFldName; // +"," + DensifyTypeFldName;

        if (bHasDensifyTypeFld)
            pQuFilter.SubFields +=  "," + DensifyTypeFldName;

        pCur = fromTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iThisID = (int)pRow.get_Value(iToPtID);
          thePointList.Remove(iThisID);

          int iThisID2 = (int)pRow.get_Value(iFromPtID);
          thePointList.Remove(iThisID2);

          //FromToPair.Add(iThisID2.ToString() + ":" + iThisID.ToString());//

          //create a key for the line's from/to id combination
          uint u1 = (uint)iThisID;
          uint u2 = (uint)iThisID2;
          ulong unsignedKey = (((ulong)u1) << 32) | u2;
          long qKey = (long)unsignedKey;
          FromToPair.Add(qKey);

          ////test going back
          //ulong unsignedKey2 = (ulong)qKey;
          //uint lowBits = (uint)(unsignedKey & 0xffffffffUL);
          //uint highBits = (uint)(unsignedKey >> 32);
          //int i1 = (int)highBits;
          //int i2 = (int)lowBits;


          int iThisID3 = -1;
          object obj = pRow.get_Value(iCtrPtID);
          if (obj != DBNull.Value)
          {
            iThisID3 = (int)obj;
            thePointList.Remove(iThisID3);
          }

          //same from and to
          //exclude linestrings, for same from and to points
          bool bIsLinestring = false;

          if (bHasDensifyTypeFld)
          {
            obj = pRow.get_Value(iDensifyType);
            if (obj != DBNull.Value)
            {
              int i = (int)obj;
              if (i > 0)
                bIsLinestring = true;
            }
          }

          if (iThisID == iThisID2 && !bIsLinestring)
          {
            IFeature pFeat = null;
            pFeat = (IFeature)pRow;
            IGeometry pGeom = pFeat.ShapeCopy;
            ISegmentCollection pSegColl = null;
            bool bHasOneSegment = false;//set it false first then prove otherwise
            bHasOneSegment = (pGeom != null);//first make sure geom is not null, first +ve
            if (bHasOneSegment)
              bHasOneSegment = !pGeom.IsEmpty; //make sure geom is not empty, second +ve
            if (bHasOneSegment)
            {
              pSegColl = (ISegmentCollection)pGeom;
              bHasOneSegment=(pSegColl.SegmentCount==1); //final confirmation
            }
            obj = pRow.get_Value(iDistanceFld);
            double dDistance=0;
            if (obj != DBNull.Value)
              dDistance = Convert.ToDouble(obj);
            if (dDistance < 0.5 || bHasOneSegment)
              theList2.Add(pRow.OID);
          }

          //remove parcel ids that are referenced in the lines list
          int iThisID4 = (int)pRow.get_Value(iParcelID);
          theParcelList.Remove(iThisID4);

          Marshal.FinalReleaseComObject(pRow);
          //after garbage collection, and before getting the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pCur.NextRow();
        }
        ////rebuild the dictionaries to get back the space from all the removed point ids and parcel ids
        //TODO: need to figure out how to get back RAM resources after removing items from the dictionary.
        //The following code does not work.
        //if (iCursorCnt > 10000)
        //{
        //  thePointList = new Dictionary<int, bool>(thePointList);
        //  theParcelList = new Dictionary<int, bool>(theParcelList);
        //}
        return bCont;
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Delete Inconsistent Records");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    protected override void OnUpdate()
    {
    }

  }
 
}
