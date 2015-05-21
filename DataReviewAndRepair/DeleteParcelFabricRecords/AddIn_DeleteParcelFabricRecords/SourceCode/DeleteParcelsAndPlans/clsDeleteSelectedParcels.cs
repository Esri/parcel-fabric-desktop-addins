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

using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.GeoSurvey;
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
using Microsoft.Win32;

namespace DeleteSelectedParcels
{
  public class clsDeleteSelectedParcels : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IFIDSet m_pFIDSetLines;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSetParcels;
    private IFIDSet m_pFIDSetPoints;
    private IFIDSet m_pEmptyGeoms;
    private IQueryFilter m_pQF;
    private bool m_bShowProgressor;
    private bool bMoreThan995UnjoinedParcels=false;
    private IFeatureLayer CFPointLayer = null;
    private IFeatureLayer CFLineLayer = null;
    private IFeatureLayer CFControlLayer = null;
    private IFeatureLayer CFLinePointLayer = null;
    public clsDeleteSelectedParcels()
    {
 
    }

    protected override void OnClick()
    {
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      //first get the selected parcel features
      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadExtMan;
      if (pCadPacMan.PacketOpen)
      {
        MessageBox.Show("The Delete Parcels command cannot be used when there is an open job.\r\nPlease finish or discard the open job, and try again.",
          "Delete Selected Parcels");
        return;
      }

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

      IActiveView pActiveView = ArcMap.Document.ActiveView;
      IMap pMap = pActiveView.FocusMap;
      ICadastralFabric pCadFabric = null;
      clsFabricUtils FabricUTILS = new clsFabricUtils();
      IProgressDialog2 pProgressorDialog = null;

      //if we're in an edit session then grab the target fabric
      if (pEd.EditState == esriEditState.esriStateEditing)  
        pCadFabric = pCadEd.CadastralFabric;

      if(pCadFabric==null)
      {//find the first fabric in the map
        if (!FabricUTILS.GetFabricFromMap(pMap, out pCadFabric))
        {
          MessageBox.Show
            ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }

      IArray CFParcelLayers = new ArrayClass();

      if (!(FabricUTILS.GetFabricSubLayersFromFabric(pMap, pCadFabric, out CFPointLayer, out CFLineLayer,
          out CFParcelLayers, out CFControlLayer, out CFLinePointLayer)))
        {
          return; //no fabric sublayers available for the targeted fabric
        }

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedDelete = false;
      IWorkspace pWS = null;
      ICadastralFabricLayer pCFLayer = null;
      ITable pParcelsTable = null;
      ITable pLinesTable = null;
      ITable pLinePtsTable = null;
      ITable pPointsTable = null;
      ITable pControlTable = null;

      try
      {
        //Get the selection of parcels
        IFeatureLayer pFL = (IFeatureLayer)CFParcelLayers.get_Element(0);

        IDataset pDS = (IDataset)pFL.FeatureClass;
        pWS = pDS.Workspace;

        if (!FabricUTILS.SetupEditEnvironment(pWS, pCadFabric, pEd, out bIsFileBasedGDB,
          out bIsUnVersioned, out bUseNonVersionedDelete))
        {
          return;
        }
        if (bUseNonVersionedDelete)
        {
          pCFLayer = new CadastralFabricLayerClass();
          pCFLayer.CadastralFabric = pCadFabric;
          pCadEd.CadastralFabricLayer = pCFLayer;//NOTE: Need to set this back to NULL when done.
        }

        ICadastralSelection pCadaSel = (ICadastralSelection)pCadEd;

        IEnumGSParcels pEnumGSParcels = pCadaSel.SelectedParcels;// need to get the parcels before trying to get the parcel count: BUG workaround

        IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
        ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;

        bMoreThan995UnjoinedParcels = (pSelSet.Count > pCadaSel.SelectedParcelCount); //used for bug workaround

        if (pCadaSel.SelectedParcelCount == 0 && pSelSet.Count == 0)
        {
          MessageBox.Show("Please select some fabric parcels and try again.", "No Selection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          pMouseCursor.SetCursor(0);
          if (bUseNonVersionedDelete)
          {
            pCadEd.CadastralFabricLayer = null;
            CFParcelLayers = null;
            CFPointLayer = null;
            CFLineLayer = null;
            CFControlLayer = null;
            CFLinePointLayer = null;
          }
          return;
        }

        if (bMoreThan995UnjoinedParcels)
          m_bShowProgressor = (pSelSet.Count > 10);
        else
          m_bShowProgressor = (pCadaSel.SelectedParcelCount > 10);

        if (m_bShowProgressor)
        {
          m_pProgressorDialogFact = new ProgressDialogFactoryClass();
          m_pTrackCancel = new CancelTrackerClass();
          m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
          m_pStepProgressor.MinRange = 1;
          if (bMoreThan995UnjoinedParcels)
            m_pStepProgressor.MaxRange = pSelSet.Count * 18; //(estimate 7 lines per parcel, 4 pts per parcel, 3 line points per parcel, and there is a second loop on parcel list)
          else
            m_pStepProgressor.MaxRange = pCadaSel.SelectedParcelCount * 18; //(estimate 7 lines per parcel, 4 pts per parcel, 3 line points per parcel, and there is a second loop on parcel list)
          m_pStepProgressor.StepValue = 1;
          pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        }

        m_pQF = new QueryFilterClass();
        string sPref; string sSuff;

        ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
        sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
        sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

        //====== need to do this for all the parcel sublayers in the map that are part of the target fabric

        if (m_bShowProgressor)
        {
          pProgressorDialog.ShowDialog();
          m_pStepProgressor.Message = "Collecting parcel data...";
        }

        //Add the OIDs of all the selected parcels into a new feature IDSet
        string[] sOIDList = { "(" };
        int tokenLimit = 995;
        bool bCont = true;
        int j = 0;
        int iCounter = 0;

        m_pFIDSetParcels = new FIDSetClass();

        //===================== start bug workaraound for 10.0 client ===================
        if (bMoreThan995UnjoinedParcels)
        {
          ICursor pCursor = null;
          pSelSet.Search(null, false, out pCursor);//code deletes all selected parcels
          IFeatureCursor pParcelFeatCurs = (IFeatureCursor)pCursor;
          IFeature pParcFeat = pParcelFeatCurs.NextFeature();

          while (pParcFeat != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (m_bShowProgressor)
            {
              bCont = m_pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            bool bExists = false;
            m_pFIDSetParcels.Find(pParcFeat.OID, out bExists);
            if (!bExists)
            {
              m_pFIDSetParcels.Add(pParcFeat.OID);

              if (iCounter <= tokenLimit)
              {
                sOIDList[j] = sOIDList[j] + Convert.ToString(pParcFeat.OID) + ",";
                iCounter++;
              }
              else
              {//maximum tokens reached
                sOIDList[j] = sOIDList[j].Trim();
                iCounter = 0;
                //set up the next OIDList
                j++;
                FabricUTILS.RedimPreserveString(ref sOIDList, 1);
                sOIDList[j] = "(";
                sOIDList[j] = sOIDList[j] + Convert.ToString(pParcFeat.OID) + ",";
              }
            }
            Marshal.ReleaseComObject(pParcFeat); //garbage collection
            pParcFeat = pParcelFeatCurs.NextFeature();

            if (m_bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
          }
          Marshal.ReleaseComObject(pCursor); //garbage collection
          //===================== end bug workaraound for 10.0 client ===================
        }
        else //===the following code path is preferred======
        {
          pEnumGSParcels.Reset();
          IGSParcel pGSParcel = pEnumGSParcels.Next();
          while (pGSParcel != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (m_bShowProgressor)
            {
              bCont = m_pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            m_pFIDSetParcels.Add(pGSParcel.DatabaseId);
            if (iCounter <= tokenLimit)
            {
              sOIDList[j] = sOIDList[j] + Convert.ToString(pGSParcel.DatabaseId) + ",";
              iCounter++;
            }
            else
            {//maximum tokens reached
              sOIDList[j] = sOIDList[j].Trim();
              iCounter = 0;
              //set up the next OIDList
              j++;
              FabricUTILS.RedimPreserveString(ref sOIDList, 1);
              sOIDList[j] = "(";
              sOIDList[j] = sOIDList[j] + Convert.ToString(pGSParcel.DatabaseId) + ",";
            }
            Marshal.ReleaseComObject(pGSParcel); //garbage collection
            pGSParcel = pEnumGSParcels.Next();
            if (m_bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
          }
          Marshal.ReleaseComObject(pEnumGSParcels); //garbage collection
        }

        if (!bCont)
        {
          AbortEdits(bUseNonVersionedDelete, pEd, pWS);
          return;
        }

        string sTime = "";
        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          //see if parcel locks can be obtained on the selected parcels. First create a job.
          DateTime localNow = DateTime.Now;
          sTime = Convert.ToString(localNow);
          ICadastralJob pJob = new CadastralJobClass();
          pJob.Name = sTime;
          pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
          pJob.Description = "Delete selected parcels";
          try
          {
            Int32 jobId = pCadFabric.CreateJob(pJob);
          }
          catch (COMException ex)
          {
            if (ex.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_ALREADY_EXISTS)
            {
              MessageBox.Show("Job named: '" + pJob.Name + "', already exists");
            }
            else
            {
              MessageBox.Show(ex.Message);
            }
            return;
          }
        }

        //if we're in an enterprise then test for edit locks
        ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)pCadFabric;
        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          pFabLocks.LockingJob = sTime;
          ILongArray pLocksInConflict = null;
          ILongArray pSoftLcksInConflict = null;

          ILongArray pParcelsToLock = new LongArrayClass();

          FabricUTILS.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, m_pStepProgressor);
          if (m_bShowProgressor && !bIsFileBasedGDB)
            m_pStepProgressor.Message = "Testing for edit locks on parcels...";

          try
          {
            pFabLocks.AcquireLocks(pParcelsToLock, true, ref pLocksInConflict, ref pSoftLcksInConflict);
          }
          catch (COMException pCOMEx)
          {
            if (pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_LOCK_ALREADY_EXISTS ||
              pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_CURRENTLY_EDITED)
            {
              MessageBox.Show("Edit Locks could not be acquired on all selected parcels.");
              // since the operation is being aborted, release any locks that were acquired
              pFabLocks.UndoLastAcquiredLocks();
            }
            else
              MessageBox.Show(pCOMEx.Message + Environment.NewLine + Convert.ToString(pCOMEx.ErrorCode));

            return;
          }
        }

        //Build an IDSet of lines for the parcel to be deleted, and build an IDSet of the points for those lines
        m_pFIDSetLines = new FIDSetClass();
        m_pFIDSetPoints = new FIDSetClass();
        if (pEd.EditState == esriEditState.esriStateEditing)
        {
          try
          {
            pEd.StartOperation();
          }
          catch
          {
            pEd.AbortOperation();//abort any open edit operations and try again
            pEd.StartOperation();
          }
        }
        if (bUseNonVersionedDelete)
        {
          if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
            return;
        }

        //first delete all the parcel records
        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Deleting parcels...";

        bool bSuccess = true;
        pParcelsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
        pLinesTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        pLinePtsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLinePoints);
        pPointsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
        pControlTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);

        if (!bUseNonVersionedDelete)
          bSuccess = FabricUTILS.DeleteRowsByFIDSet(pParcelsTable, m_pFIDSetParcels, m_pStepProgressor, m_pTrackCancel);
        if (bUseNonVersionedDelete)
          bSuccess = FabricUTILS.DeleteRowsUnversioned(pWS, pParcelsTable,
              m_pFIDSetParcels, m_pStepProgressor, m_pTrackCancel);

        if (!bSuccess)
        {
          if (!bIsUnVersioned)
            pFabLocks.UndoLastAcquiredLocks();

          AbortEdits(bUseNonVersionedDelete,pEd,pWS);

          if (!bIsUnVersioned)
          {
            //check version and if the Cancel button was not clicked and we're higher than 
            //version 10.0, then re-try the delete with the core delete command
            string sVersion = Application.ProductVersion;
            int iErrCode = FabricUTILS.LastErrorCode;
            if (!sVersion.StartsWith("10.0") && iErrCode == -2147217400)
              FabricUTILS.ExecuteCommand("{B0A62C1C-7FAE-457A-AB25-A966B7254EF6}");
          }
          return;
        }

        //next need to use an in clause for lines, so ...
        string[] sPointOIDList = { "" };
        int iCnt = 0;
        int iTokenCnt = 0;
        int iStepCnt = 1;
        //...for each item in the sOIDList array
        foreach (string inClause in sOIDList)
        {
          ICursor pLineCurs = FabricUTILS.GetCursorFromCommaSeparatedOIDList(pLinesTable, inClause, "PARCELID");
          IRow pRow = pLineCurs.NextRow();
          Int32 iFromPt = pLinesTable.Fields.FindField("FROMPOINTID");
          Int32 iToPt = pLinesTable.Fields.FindField("TOPOINTID");

          while (pRow != null)
          {
            if (iTokenCnt >= tokenLimit)
            {
              FabricUTILS.RedimPreserveString(ref sPointOIDList, 1);
              iTokenCnt = 0;
              iCnt++;
            }

            m_pFIDSetLines.Add(pRow.OID);
            Int32 i = (Int32)pRow.get_Value(iFromPt);
            if (i > -1)
            {
              bool bExists = false;
              m_pFIDSetPoints.Find(i, out bExists);
              if (!bExists)
              {
                m_pFIDSetPoints.Add(i);
                sPointOIDList[iCnt] = sPointOIDList[iCnt] + Convert.ToString(i) + ",";
                iTokenCnt++;
              }
            }
            i = (Int32)pRow.get_Value(iToPt);
            if (i > -1)
            {
              bool bExists = false;
              m_pFIDSetPoints.Find(i, out bExists);
              if (!bExists)
              {
                m_pFIDSetPoints.Add(i);
                sPointOIDList[iCnt] = sPointOIDList[iCnt] + Convert.ToString(i) + ",";
                iTokenCnt++;
              }
            }
            Marshal.ReleaseComObject(pRow); //garbage collection
            pRow = pLineCurs.NextRow();
          }
          Marshal.ReleaseComObject(pLineCurs); //garbage collection

          //delete line records based on the selected parcels
          string sMessage = "Deleting lines...";
          int iSetCnt = sOIDList.GetLength(0);
          if (iSetCnt > 1)
            sMessage += "Step " + Convert.ToString(iStepCnt) + " of " + Convert.ToString(iSetCnt);
          if (m_bShowProgressor)
            m_pStepProgressor.Message = sMessage;
          if (!bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsByFIDSet(pLinesTable, m_pFIDSetLines, m_pStepProgressor, m_pTrackCancel);
          if (bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsUnversioned(pWS, pLinesTable, m_pFIDSetLines, m_pStepProgressor, m_pTrackCancel);
          if (!bSuccess)
          {
            if (!bIsUnVersioned)
              pFabLocks.UndoLastAcquiredLocks();

            AbortEdits(bUseNonVersionedDelete, pEd, pWS);
            return;
          }

          //delete the line points for the deleted parcels
          //build the list of the line points that need to be deleted.
          //IFeatureClass pFeatCLLinePoints = CFLinePointLayer.FeatureClass;
          string NewInClause = "";
          //remove trailing comma
          if ((inClause.Substring(inClause.Length - 1, 1)) == ",")
            NewInClause = inClause.Substring(0, inClause.Length - 1);

          //m_pQF.WhereClause = (sPref + "parcelid" + sSuff).Trim() + " IN " + NewInClause + ")";
          m_pQF.WhereClause = "PARCELID IN " + NewInClause + ")";
          ICursor pLinePointCurs = pLinePtsTable.Search(m_pQF, false);
          IRow pLinePointFeat = pLinePointCurs.NextRow();

          //Build an IDSet of linepoints for parcels to be deleted 
          IFIDSet pFIDSetLinePoints = new FIDSetClass();

          while (pLinePointFeat != null)
          {
            pFIDSetLinePoints.Add(pLinePointFeat.OID);
            Marshal.ReleaseComObject(pLinePointFeat); //garbage collection
            pLinePointFeat = pLinePointCurs.NextRow();
          }

          //===========deletes linepoints associated with parcels
          iSetCnt = sOIDList.GetLength(0);
          sMessage = "Deleting line-points...";
          if (iSetCnt > 1)
            sMessage += "Step " + Convert.ToString(iStepCnt) + " of " + Convert.ToString(iSetCnt);
          if (m_bShowProgressor)
            m_pStepProgressor.Message = sMessage;

          if (!bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsByFIDSet(pLinePtsTable,
              pFIDSetLinePoints, m_pStepProgressor, m_pTrackCancel);
          if (bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsUnversioned(pWS, pLinePtsTable,
                pFIDSetLinePoints, m_pStepProgressor, m_pTrackCancel);
          if (!bSuccess)
          {
            if (!bIsUnVersioned)
              pFabLocks.UndoLastAcquiredLocks();

            AbortEdits(bUseNonVersionedDelete, pEd, pWS);
            
            if (pLinePointCurs!=null)
              Marshal.ReleaseComObject(pLinePointCurs); //garbage

            return;
          }

          ///////==============================================

          Marshal.ReleaseComObject(pLinePointCurs); //garbage
          iStepCnt++;
        }

        //now need to get points that should not be deleted, because they are used by lines that are not deleted.
        //first search for the remaining lines. Any that have from/to points that are in the point fidset are the points 
        //that should stay
        IFIDSet pFIDSetNullGeomLinePtFrom = new FIDSetClass();
        IFIDSet pFIDSetNullGeomLinePtTo = new FIDSetClass();
        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Updating point delete list...";

        for (int z = 0; z <= iCnt; z++)
        {
          //remove trailing comma
          if ((sPointOIDList[z].Trim() == ""))
            break;
          if ((sPointOIDList[z].Substring(sPointOIDList[z].Length - 1, 1)) == ",")
            sPointOIDList[z] = sPointOIDList[z].Substring(0, sPointOIDList[z].Length - 1);
        }

        //string TheWhereClause = "(" + (sPref + "frompointid" + sSuff).Trim() + " IN (";

        //UpdateDeletePointList(ref sPointOIDList, ref m_pFIDSetPoints, "frompointid",
        //  TheWhereClause, pLinesTable, out pFIDSetNullGeomLinePtFrom);

        //TheWhereClause = "(" + (sPref + "topointid" + sSuff).Trim() + " IN (";

        //UpdateDeletePointList(ref sPointOIDList, ref m_pFIDSetPoints, "topointid",
        //  TheWhereClause, pLinesTable, out pFIDSetNullGeomLinePtTo);

        string TheWhereClause = "(FROMPOINTID IN (";

        UpdateDeletePointList(ref sPointOIDList, ref m_pFIDSetPoints, "FROMPOINTID",
          TheWhereClause, pLinesTable, out pFIDSetNullGeomLinePtFrom);

        TheWhereClause = "(TOPOINTID IN (";

        UpdateDeletePointList(ref sPointOIDList, ref m_pFIDSetPoints, "TOPOINTID",
          TheWhereClause, pLinesTable, out pFIDSetNullGeomLinePtTo);

        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Deleting points...";

        if (!bUseNonVersionedDelete)
        {
          bSuccess = FabricUTILS.DeleteRowsByFIDSet(pPointsTable, m_pFIDSetPoints,
            m_pStepProgressor, m_pTrackCancel);
        }
        if (bUseNonVersionedDelete)
          bSuccess = FabricUTILS.DeleteRowsUnversioned(pWS, pPointsTable, m_pFIDSetPoints,
             m_pStepProgressor, m_pTrackCancel);
        if (!bSuccess)
        {
          if (!bIsUnVersioned)
            pFabLocks.UndoLastAcquiredLocks();

          AbortEdits(bUseNonVersionedDelete, pEd, pWS);
          return;
        }

        //====Phase 2 of line-point delete. Remove the Line-points that no longer have underlying points.
        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Deleting line-points...";
        for (int z = 0; z <= iCnt; z++)
        {
          if ((sPointOIDList[z].Trim() == ""))
            continue;
          //remove line points where underlying points were deleted 
          bSuccess = FabricUTILS.DeleteByQuery(pWS, pLinePtsTable, pLinePtsTable.Fields.get_Field(pLinePtsTable.FindField("LinePointID")),
            sPointOIDList, !bIsUnVersioned, m_pStepProgressor, m_pTrackCancel);
          if (!bSuccess)
          {
            if (!bIsUnVersioned)
              pFabLocks.UndoLastAcquiredLocks();

            AbortEdits(bUseNonVersionedDelete, pEd, pWS);
            return;
          }
        }

        //=====

        //Empty geometry on points that are floating points on unjoined parcels
        m_pEmptyGeoms = new FIDSetClass();
        FabricUTILS.IntersectFIDSetCommonIDs(pFIDSetNullGeomLinePtTo, pFIDSetNullGeomLinePtFrom, out m_pEmptyGeoms);

        ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        pSchemaEd.ReleaseReadOnlyFields(pPointsTable, esriCadastralFabricTable.esriCFTPoints); //release safety-catch
        if (!bUseNonVersionedDelete)
          FabricUTILS.EmptyGeometries((IFeatureClass)pPointsTable, m_pEmptyGeoms);
        else
          FabricUTILS.EmptyGeometriesUnversioned(pWS, CFPointLayer.FeatureClass, m_pEmptyGeoms);

        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);//set safety back on

        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Resetting control point associations...";

        for (int z = 0; z <= iCnt; z++)
        {
          if ((sPointOIDList[z].Trim() == ""))
            break;
          //cleanup associated control points, and associations where underlying points were deleted 
          //m_pQF.WhereClause = (sPref + "pointid" + sSuff).Trim() + " IN (" + sPointOIDList[z] + ")";
          m_pQF.WhereClause = "POINTID IN (" + sPointOIDList[z] + ")";
          pSchemaEd.ReleaseReadOnlyFields(pControlTable, esriCadastralFabricTable.esriCFTControl); //release safety-catch
          if (!FabricUTILS.ResetControlAssociations(pControlTable, m_pQF, bUseNonVersionedDelete))
          {
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
          }
        }
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
 
        if (bUseNonVersionedDelete)
          FabricUTILS.StopEditing(pWS);

        if (pEd.EditState == esriEditState.esriStateEditing)
          pEd.StopOperation("Delete parcels");

        //clear selection, to make sure the parcel explorer is updated and refreshed properly
        if (pFeatSel != null && bMoreThan995UnjoinedParcels)
          pFeatSel.Clear();
      }

      catch (Exception ex)
      {
        if (bUseNonVersionedDelete)
          FabricUTILS.AbortEditing(pWS);

        if (pEd != null)
        {
          if (pEd.EditState == esriEditState.esriStateEditing)
            pEd.AbortOperation();
        }

        MessageBox.Show(ex.Message);
        return;
      }
      finally
      {
        RefreshMap(pActiveView, CFParcelLayers, CFPointLayer, CFLineLayer, CFControlLayer, CFLinePointLayer);
        //update the TOC
        IMxDocument mxDocument = (ESRI.ArcGIS.ArcMapUI.IMxDocument)(ArcMap.Application.Document);
        for (int i = 0; i < mxDocument.ContentsViewCount; i++)
        {
          IContentsView pCV = (IContentsView)mxDocument.get_ContentsView(i);
          pCV.Refresh(null);
        }

        if (pMouseCursor != null)
          pMouseCursor.SetCursor(0);

        m_pStepProgressor = null;
        if (!(pProgressorDialog == null))
          pProgressorDialog.HideDialog();
        pProgressorDialog = null;

        if (bUseNonVersionedDelete)
        {
          pCadEd.CadastralFabricLayer = null;
          CFParcelLayers = null;
          CFPointLayer = null;
          CFLineLayer = null;
          CFControlLayer = null;
          CFLinePointLayer = null;
        }
      }
    }

    protected override void OnUpdate()
    {
      CustomizelHelperExtension v = CustomizelHelperExtension.GetExtension();
      this.Enabled = v.CommandIsEnabled;

      if (!this.Enabled)
        this.Enabled = v.MapHasUnversionedFabric;
    }

    private void RefreshMap(IActiveView ActiveView, IArray ParcelLayers, IFeatureLayer PointLayer,
      IFeatureLayer LineLayer, IFeatureLayer ControlLayer, IFeatureLayer LinePointLayer)
    {
      try
      {
        for (int z = 0; z <= ParcelLayers.Count - 1; z++)
        {
          if (ParcelLayers.get_Element(z) != null)
          {
            IFeatureSelection pFeatSel = (IFeatureSelection)ParcelLayers.get_Element(z);
            pFeatSel.Clear();//refreshes the parcel explorer
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, ParcelLayers.get_Element(z), ActiveView.Extent);
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ParcelLayers.get_Element(z), ActiveView.Extent);
          }
        }
        if (PointLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, PointLayer, ActiveView.Extent);
        if (LineLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, LineLayer, ActiveView.Extent);
        if (ControlLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ControlLayer, ActiveView.Extent);
        if (LinePointLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, LinePointLayer, ActiveView.Extent);
      }
      catch
      {}
      }

    private bool UpdateDeletePointList(ref string[] sPointOIDList, ref IFIDSet m_pFIDSetPoints, string FieldName, 
      string WhereClauseLHS, ITable pLinesTable, out IFIDSet FIDSetNullGeomLine)
    {
      try
      {
        IFIDSet FIDSetNullGeomLine2 = new FIDSetClass();
        int iCnt = sPointOIDList.GetLength(0) - 1;
        for (int z = 0; z <= iCnt; z++)
        {
          if ((sPointOIDList[z].Trim() == ""))
            break;

          m_pQF.WhereClause = WhereClauseLHS + sPointOIDList[z] + "))";

          IFeatureCursor pRemainingLinesCurs = (IFeatureCursor)pLinesTable.Search(m_pQF, false);
          int iFromOrToPt2 = pRemainingLinesCurs.Fields.FindField(FieldName);

          IFeature pRow2 = pRemainingLinesCurs.NextFeature();

          while (pRow2 != null)
          {
            int i = (int)pRow2.get_Value(iFromOrToPt2);
            if (i > -1)
            {
              IGeometry pGeom = pRow2.Shape;
              if (pGeom != null)
              {
                if (pGeom.IsEmpty)
                  FIDSetNullGeomLine2.Add(i);
              }
              m_pFIDSetPoints.Delete(i);
              //also need to remove these from the sPointOIDList in-clause string
              string sToken1 = Convert.ToString(i) + ",";
              string sToken2 = "," + Convert.ToString(i) + ",";
              string sToken3 = "," + Convert.ToString(i);

              if (sPointOIDList[z].Contains(sToken1))
                sPointOIDList[z] = sPointOIDList[z].Replace(sToken1, "");//replace token for oid with a null string

              if (sPointOIDList[z].Contains(sToken2))
                sPointOIDList[z] = sPointOIDList[z].Replace(sToken2, ",");//replace token for oid with a null string

              if (sPointOIDList[z].Contains(sToken3))
                sPointOIDList[z] = sPointOIDList[z].Replace(sToken3, "");//replace token for oid with a null string

            }
            Marshal.ReleaseComObject(pRow2); //garbage collection
            pRow2 = pRemainingLinesCurs.NextFeature();
          }
          Marshal.ReleaseComObject(pRemainingLinesCurs); //garbage collection
          //remove trailing comma
          if ((sPointOIDList[z].Substring(sPointOIDList[z].Length - 1, 1)) == ",")
            sPointOIDList[z] = sPointOIDList[z].Substring(0, sPointOIDList[z].Length - 1);
        }
        FIDSetNullGeomLine = FIDSetNullGeomLine2;
        return true;
      }
      catch
      {
        FIDSetNullGeomLine = null;
        return false;
      }
    }

    private void AbortEdits(bool bUseNonVersionedDelete, IEditor pEd, IWorkspace pWS)
    {
      clsFabricUtils FabricUTILS = new clsFabricUtils();
      if (bUseNonVersionedDelete)
        FabricUTILS.AbortEditing(pWS);
      else
      {
        if (pEd != null)
        {
          if (pEd.EditState == esriEditState.esriStateEditing)
            pEd.AbortOperation();
        }
      }
    }

    //private void Cleanup(IEditor Editor, IProgressDialog2 ProgressorDialog, IMouseCursor MouseCursor)
    //{
    //  try
    //  {
    //    if (MouseCursor != null)
    //      MouseCursor.SetCursor(0);

    //    if (Editor != null)
    //    {
    //      if (Editor.EditState == esriEditState.esriStateEditing)
    //        Editor.AbortOperation();
    //    }

    //    if (!(ProgressorDialog == null))
    //    {
    //      ProgressorDialog.HideDialog();
    //    }
    //    ProgressorDialog = null;
    //  }
    //  catch (Exception ex)
    //  {
    //    MessageBox.Show(ex.Message.ToString());
    //  }
    //}

  }
}
