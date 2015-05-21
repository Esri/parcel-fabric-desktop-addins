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
  public class clsDeleteSelectedConnxLines : ESRI.ArcGIS.Desktop.AddIns.Button
  {
//    private IFIDSet m_pFIDSetConnxLines;
    private IFIDSet m_pFIDSetParcels;

    private IQueryFilter m_pQF;

    public clsDeleteSelectedConnxLines()
    {
    }

    protected override void OnClick()
    {
      bool bShowProgressor = false;
      IStepProgressor pStepProgressor = null;
      //Create a CancelTracker.
      ITrackCancel pTrackCancel = null;
      IProgressDialogFactory pProgressorDialogFact;

      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      //first get the selected line features
      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadExtMan;
      if (pCadPacMan.PacketOpen)
      {
        MessageBox.Show("The Delete Connection Line command cannot be used when there is an open job.\r\nPlease finish or discard the open job, and try again.",
          "Delete Selected Connection Lines");
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

      if (pCadFabric == null)
      {//find the first fabric in the map
        if (!FabricUTILS.GetFabricFromMap(pMap, out pCadFabric))
        {
          MessageBox.Show
            ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }

      IArray CFLineLayers = new ArrayClass();

      if (!(FabricUTILS.GetLineLayersFromFabric(pMap, pCadFabric, out CFLineLayers)))
        return; //no fabric sublayers available for the targeted fabric

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedDelete = false;
      IWorkspace pWS = null;

      ITable pControlTable = null;
      ITable pPointsTable = null;
      ITable pLinesTable = null;
      ITable pParcelsTable = null;

      try
      {
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
        IFeatureLayer pFL = (IFeatureLayer)CFLineLayers.get_Element(0);
        IDataset pDS = (IDataset)pFL.FeatureClass;
        pWS = pDS.Workspace;

        if (!FabricUTILS.SetupEditEnvironment(pWS, pCadFabric, pEd, out bIsFileBasedGDB,
          out bIsUnVersioned, out bUseNonVersionedDelete))
          return;

        //loop through each line layer and
        //Get the selection of lines
        int iCnt = 0;
        int iTotalSelectionCount = 0;
        for (; iCnt < CFLineLayers.Count; iCnt++)
        {
          pFL = (IFeatureLayer)CFLineLayers.get_Element(iCnt);
          IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
          ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;
          iTotalSelectionCount += pSelSet.Count;
        }

        if (iTotalSelectionCount == 0)
        {
          MessageBox.Show("Please select some fabric connection lines and try again.", "No Selection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          if (bUseNonVersionedDelete)
          {
            pCadEd.CadastralFabricLayer = null;
            CFLineLayers = null;
          }
          return;
        }

        bShowProgressor = (iTotalSelectionCount > 10);

        if (bShowProgressor)
        {
          pProgressorDialogFact = new ProgressDialogFactoryClass();
          pTrackCancel = new CancelTrackerClass();
          pStepProgressor = pProgressorDialogFact.Create(pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)pStepProgressor;
          pStepProgressor.MinRange = 1;
          pStepProgressor.MaxRange = iTotalSelectionCount * 2;
          pStepProgressor.StepValue = 1;
          pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        }

        m_pFIDSetParcels = new FIDSetClass();
        List<int> SelectedConnectionLines = new List<int>();
        pLinesTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        pParcelsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
        pControlTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);
        pPointsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);

        ICursor pCursor = null;
        int idxToPointID = pLinesTable.FindField("TOPOINTID");
        string ToPointIDFldName = pLinesTable.Fields.get_Field(idxToPointID).Name;

        int idxFromPointID = pLinesTable.FindField("FROMPOINTID");
        string FromPointIDFldName = pLinesTable.Fields.get_Field(idxFromPointID).Name;

        int idxParcelID = pLinesTable.FindField("PARCELID");
        string ParcelIDFldName = pLinesTable.Fields.get_Field(idxParcelID).Name;

        int idxLineCategory = pLinesTable.FindField("CATEGORY");
        string LineCategoryFldName = pLinesTable.Fields.get_Field(idxLineCategory).Name;

        int idxSequenceID = pLinesTable.FindField("SEQUENCE");
        string SequenceIDFldName = pLinesTable.Fields.get_Field(idxSequenceID).Name;
        
        m_pQF = new QueryFilterClass();
        m_pQF.WhereClause = "(" + LineCategoryFldName + " = 1 OR " + LineCategoryFldName + " = 2 OR " +
                                     LineCategoryFldName + " = 3 OR " + LineCategoryFldName + " = 6)";

        IGeoDataset pGeoDS = (IGeoDataset)pLinesTable;
        ISpatialReference spatialRef = pGeoDS.SpatialReference;


        ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
        string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
        string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

        //loop through each line layer and build the LineID_To_PtFromID Dictionary
        //build the ParcelID list using the parcel id on the selected lines
        Dictionary<int, int> dict_LineID_ToPtID = new Dictionary<int,int>();
        Dictionary<int, int> dict_LineID_FromPtID = new Dictionary<int, int>();
        Dictionary<int, int> dict_LineID_Sequence = new Dictionary<int, int>();
        Dictionary<int, int> dict_LineToParcelReference = new Dictionary<int,int>();

        List<int> ParcelIDs = new List<int>();

        iCnt = 0;
        for (; iCnt < CFLineLayers.Count; iCnt++)
        {
          //clear out the dictionary from the last line layer process
          dict_LineID_ToPtID.Clear();
          dict_LineID_FromPtID.Clear();
          dict_LineID_Sequence.Clear();
          dict_LineToParcelReference.Clear();
          pFL = (IFeatureLayer)CFLineLayers.get_Element(iCnt);
          IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
          ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;

          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Message = "Collecting connection line data...";
          }

          //Add all the OIDs of the selected lines into a new feature IDSet

          bool bCont = true;

          pSelSet.Search(m_pQF, false, out pCursor);
          IFeatureCursor pLinesFeatCurs = (IFeatureCursor)pCursor;
          IFeature pLineFeat = pLinesFeatCurs.NextFeature();

          while (pLineFeat != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
            {
              bCont = pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            bool bExists = false;
            //m_pFIDSetConnxLines.Find(pLineFeat.OID, out bExists);
            bExists=SelectedConnectionLines.Contains(pLineFeat.OID);
            if (!bExists)
            {
              int iToPt = (int)pLineFeat.get_Value(idxToPointID);
              int iFromPt = (int)pLineFeat.get_Value(idxFromPointID);
              int iLineCategory = (int)pLineFeat.get_Value(idxLineCategory);
              int iParcelID = (int)pLineFeat.get_Value(idxParcelID);
              
              SelectedConnectionLines.Add(pLineFeat.OID);
              m_pFIDSetParcels.Find(iParcelID, out bExists);
              if(!bExists)
              {
                m_pFIDSetParcels.Add(iParcelID);
                ParcelIDs.Add(iParcelID);
              }

              if (iLineCategory == 6)
              {
                dict_LineID_ToPtID.Add(pLineFeat.OID, -1 * iToPt);
                dict_LineID_FromPtID.Add(pLineFeat.OID, -1 * iFromPt);
              }
              else
              {
                dict_LineID_ToPtID.Add(pLineFeat.OID, iToPt);
                dict_LineID_FromPtID.Add(pLineFeat.OID, iFromPt);
              }

            }
            Marshal.ReleaseComObject(pLineFeat); //garbage collection
            pLineFeat = pLinesFeatCurs.NextFeature();

            if (bShowProgressor)
            {
              if (pStepProgressor.Position < pStepProgressor.MaxRange)
                pStepProgressor.Step();
            }
          }
          Marshal.ReleaseComObject(pCursor); //garbage collection
          if (!bCont)
          {
            SelectedConnectionLines.Clear();
            AbortEdits(bUseNonVersionedDelete, pEd, pWS); //this abort is here in case there were edits in the prior iteration, when there are multiple line layers
            return;
          }

          if (SelectedConnectionLines.Count == 0)
            continue; //if there is no selection in this layer then continue

          //collecting connection line records for trace
          if (bShowProgressor)
            pStepProgressor.Message = "Tracing downstream connection lines...";

          #region Get Job Locks

          string sTime = "";
          if (!bIsUnVersioned && !bIsFileBasedGDB)
          {
            //see if parcel locks can be obtained on the selected parcels. First create a job.
            DateTime localNow = DateTime.Now;
            sTime = Convert.ToString(localNow);
            ICadastralJob pJob = new CadastralJobClass();
            pJob.Name = sTime;
            pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
            pJob.Description = "Delete selected connection lines";
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

            FabricUTILS.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, pStepProgressor);
            if (bShowProgressor && !bIsFileBasedGDB)
              pStepProgressor.Message = "Testing for edit locks on parcels...";

            try
            {
              pFabLocks.AcquireLocks(pParcelsToLock, true, ref pLocksInConflict, ref pSoftLcksInConflict);
            }
            catch (COMException pCOMEx)
            {
              if (pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_LOCK_ALREADY_EXISTS ||
                pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_CURRENTLY_EDITED)
              {
                MessageBox.Show("Edit Locks could not be acquired on all selected parcel connection lines.");
                // since the operation is being aborted, release any locks that were acquired
                pFabLocks.UndoLastAcquiredLocks();
              }
              else
                MessageBox.Show(pCOMEx.Message + Environment.NewLine + Convert.ToString(pCOMEx.ErrorCode));

              return;
            }
          }
          #endregion

          //now get *all* the other lines for the same parcels that have the connection lines selected, and then create a forward star object
          int tokenLimit = 995;
          List<string> InClauseList = FabricUTILS.InClauseFromOIDsList(ParcelIDs, tokenLimit);
          m_pQF.WhereClause = "";
          string sOIDFldName = pParcelsTable.OIDFieldName;
          IArray pParcelFeatArr = new ArrayClass();
          foreach (string InClause in InClauseList)
          {
            if (InClause == "")
              continue;
            m_pQF.WhereClause = sOIDFldName + " IN (" + InClause + ")";
            IFeatureCursor pParcelsFeatCurs = (IFeatureCursor)pParcelsTable.Search(m_pQF,false);
            IFeature pParcelFeat = pParcelsFeatCurs.NextFeature();
            while (pParcelFeat!=null)
            {
              pParcelFeatArr.Add(pParcelFeat);
              Marshal.ReleaseComObject(pParcelFeat); //garbage collection
              pParcelFeat = pParcelsFeatCurs.NextFeature();
            }
            Marshal.ReleaseComObject(pParcelsFeatCurs); //garbage collection
          }

          if (pParcelFeatArr.Count==0)
            continue;

          ICadastralFeatureGenerator pFeatureGenerator = new CadastralFeatureGeneratorClass();
          IEnumGSParcels pEnumGSParcels = pFeatureGenerator.CreateParcelsFromFeatures(pCadEd, pParcelFeatArr, true);
          pEnumGSParcels.Reset();

          IEnumCELines pCELines = new EnumCELinesClass();
          IEnumGSLines pEnumGSLines = (IEnumGSLines)pCELines;
          IGSParcel pGSParcel = pEnumGSParcels.Next();
          int iSequ=pLinesTable.FindField("Sequence");
          while (pGSParcel != null)
          {
            IEnumGSLines pGSLinesInner = pGSParcel.GetParcelLines(null, false);
            pGSLinesInner.Reset();
            IGSParcel pTemp=null;
            IGSLine pGSLine=null;
            pGSLinesInner.Next(ref pTemp, ref pGSLine);
            while (pGSLine != null)
            {
              ICadastralFeature cf = (ICadastralFeature)pGSLine;
              IRow rr = cf.Row;
              int ii=(int)rr.get_Value(iSequ);
              dict_LineID_Sequence.Add(rr.OID,ii);
              dict_LineToParcelReference.Add(rr.OID, pGSParcel.DatabaseId);
              pCELines.Add(pGSLine);
              pGSLinesInner.Next(ref pTemp, ref pGSLine);
            }
            pGSParcel = pEnumGSParcels.Next();
          }

          IParcelLineFunctions3 ParcelLineFx = new ParcelFunctionsClass();
          IGSForwardStar pFwdStar = ParcelLineFx.CreateForwardStar(pEnumGSLines);
          
          //forward star object is now created for all the selected lines, so we can loop through each
          //selected line and use its nodes to add to the lines delete list
          List<int> LineIdList = new List<int>();
          foreach (int k in SelectedConnectionLines)
            LineIdList.Add(k);//make a copy

          foreach (int i in SelectedConnectionLines)
          {
            int iFromNode = dict_LineID_FromPtID[i];
            int iToNode = dict_LineID_ToPtID[i];
            if (iFromNode < 0 || iToNode < 0)
            {//if it's an origin connection then switch the starting from and to
              iFromNode = Math.Abs(iToNode);
              iToNode = Math.Abs(dict_LineID_FromPtID[i]);
            }

            if (!TraceFabricLines(ref pFwdStar, iToNode, ref LineIdList, 0,false))
            {//if trace didn't work, remove the selected line from the line id list, so it doesn't get deleted.
              LineIdList.Remove(i);
              continue;
            }

          }
          SelectedConnectionLines.Clear(); //done using this list in this iteration
          #region Re-sequence lines and remove gaps
          //check the list of lines that we're about to delete, and remove them from the sequence list
          foreach (int LineID in LineIdList)
            dict_LineID_Sequence.Remove(LineID);

          List<string> StringList = new List<string>();
          foreach (var pair in dict_LineID_Sequence)
          {
            string s1= dict_LineToParcelReference[pair.Key].ToString();
            string s2 = s1 + "," + pair.Key.ToString() + "," + pair.Value.ToString("0000000");
            StringList.Add(s2);
          }
          StringList.Sort();
          string sCheck = "";
          List<List<string>> t = new List<List<string>>();
          List<string> sInnerList = null;
          foreach (string s in StringList)
          {
            string[] splitVals = s.Split(',');
            if (sCheck != splitVals[0])
            {
              sInnerList = new List<string>();
              t.Add(sInnerList);
            }
            sInnerList.Add(splitVals[2] +","+splitVals[1]);
            sCheck = splitVals[0];
          }

          //now find the gaps
          foreach (List<string> LineSequence in t)
          {
            LineSequence.Sort();

            //first check to see if there are any gaps to process
            string[] FirstRecord = LineSequence[0].Split(',');
            string[] LastRecord = LineSequence[LineSequence.Count - 1].Split(',');
            int iLowest = Convert.ToInt32(FirstRecord[0]);
            int iHighest = Convert.ToInt32(LastRecord[0]);
            bool bLowNumberIsHigh = (iLowest >= 3);
            if ((iHighest - iLowest == (LineSequence.Count-1)) && !bLowNumberIsHigh)
            {
              LineSequence.Clear();
              continue;
            }

            for (int i = LineSequence.Count - 1; i >= 0; i--)
            {
              if (i == 0)
                continue;
              string[] SplitValsA = LineSequence[i].Split(',');
              string[] SplitValsB = LineSequence[i-1].Split(',');
              int iA = Convert.ToInt32(SplitValsA[0]);
              int iB = Convert.ToInt32(SplitValsB[0]);
              if (iA - 1 != iB)
              {
                int iDiff = iA - iB;
                LineSequence[i] += "," + iDiff.ToString();
              }
            }

            int k=0;
            int iFirstGap = 0;
            int iCumulativeSUM = 0;

            if (bLowNumberIsHigh)
              iCumulativeSUM = iLowest-1;

            while (k < LineSequence.Count)
            {
              string[] SplitValsA = LineSequence[k].Split(',');
              bool bGap = (SplitValsA.GetLength(0) == 3);
              if (iCumulativeSUM == 0 && bGap)
                iFirstGap = k;
              int iB = Convert.ToInt32(SplitValsA[0]);
              if(bGap)
                iCumulativeSUM += (Convert.ToInt32(SplitValsA[2]) - 1);
              int iD = iB - iCumulativeSUM;
              if(iCumulativeSUM > 0)
                LineSequence[k] = SplitValsA[1] + "," + iD.ToString();
              else
                LineSequence[k] = null;
              k++;
            }
            if (iFirstGap > 1)
              LineSequence.RemoveRange(0, iFirstGap-1);

          }

          for (int i = t.Count - 1; i >= 0; i--)
          {
            List<string> lst = t[i];
            if (lst.Count <= 1)
              t.RemoveAt(i);
          }

          dict_LineID_Sequence.Clear();
          //rebuild the lineid-to-sequence dictionary with the new sequences
          List<string> sInClauseForSequences = new List<string>();
          string sInClause4ThisSeq = "";
          //tokenLimit = 5;
          iCnt = 0;
          foreach(List<string> lst in t)
          {
            foreach (string s in lst)
            {
              if (s == null)
                continue;
              string[] s2 = s.Split(',');
              int iLineID = Convert.ToInt32(s2[0]);
              int iSeqID = Convert.ToInt32(s2[1]);

              if (iCnt == tokenLimit)
              {
                sInClause4ThisSeq += ")";
                sInClauseForSequences.Add(sInClause4ThisSeq);
                sInClause4ThisSeq = "";
                iCnt = 0;
              }

              if (sInClause4ThisSeq.Length == 0)
                sInClause4ThisSeq = "(" + s2[0];
              else
                sInClause4ThisSeq += "," + s2[0];
              iCnt++;
             
              
              dict_LineID_Sequence.Add(iLineID, iSeqID);
            }
          }
          sInClause4ThisSeq += ")";
          if(sInClause4ThisSeq.Length>2)
            sInClauseForSequences.Add(sInClause4ThisSeq);
          #endregion

          IFIDSet pFIDSetConnxLines = new FIDSetClass();
          foreach (int j in LineIdList)
            pFIDSetConnxLines.Add(j);
          
          if (bUseNonVersionedDelete)
          {
            if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
            {
              if (bUseNonVersionedDelete)
                pCadEd.CadastralFabricLayer = null;
              return;
            }
          }

          //first delete all the connection line records
          if (bShowProgressor)
            pStepProgressor.Message = "Deleting connection lines...";

          bool bSuccess = true;
          //Make a union of the line feature geometry envelopes
          IGeometryBag pGeomBag = new GeometryBagClass();
          IGeometryCollection pGeomColl = (IGeometryCollection)pGeomBag;

          if (!bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsByFIDSetReturnGeomCollection(pLinesTable, pFIDSetConnxLines, pStepProgressor, pTrackCancel, ref pGeomColl);
          if (bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsUnversioned(pWS, pLinesTable, pFIDSetConnxLines, pStepProgressor, pTrackCancel);
          if (!bSuccess)
          {
            AbortEdits(bUseNonVersionedDelete, pEd, pWS);
            return;
          }

          //now need to resequence lines to close any gaps in the sequence resulting from line deletion
          #region resequence lines
          ICadastralFabricSchemaEdit2 pSchemaEdA = (ICadastralFabricSchemaEdit2)pCadFabric;
          int idxSequFldOnLines = pLinesTable.FindField("SEQUENCE");
          string SequenceFldName = pLinesTable.Fields.get_Field(idxSequFldOnLines).Name;
          string OIDFldName = pLinesTable.OIDFieldName;

          IQueryFilter pQuFilterA = new QueryFilterClass();

          if (bShowProgressor)
            pStepProgressor.Message = "Resequencing lines ...";
          foreach (string sInClause in sInClauseForSequences)
          {
            if (sInClause.Trim() == "")
              continue;
            pQuFilterA.SubFields = OIDFldName + "," + SequenceFldName;
            pQuFilterA.WhereClause = OIDFldName + " IN " + sInClause;
            pSchemaEdA.ReleaseReadOnlyFields(pLinesTable, esriCadastralFabricTable.esriCFTLines); //release safety-catch
            if (!FabricUTILS.UpdateTableByDictionaryLookup(pLinesTable, pQuFilterA, "sequence",
              bIsUnVersioned,dict_LineID_Sequence))
            {
              pSchemaEdA.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
          pSchemaEdA.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on
          sInClauseForSequences.Clear();
          #endregion

          //now need to delete newly orphaned points using the unioned geometry of envelopes to filter the point/line search
          #region Delete newly orphaned points
          //first delete all the connection line records
          string ObjectID = pPointsTable.OIDFieldName;
          List<string> InClausePointRefsOnLines = new List<string>();
          if (pGeomColl.GeometryCount > 0)
          {
            ITopologicalOperator4 pUnionedEnvelopes = null;
            pUnionedEnvelopes = new PolygonClass();
            pUnionedEnvelopes.ConstructUnion((IEnumGeometry)pGeomBag);
            ISpatialFilter pSpatFilter = new SpatialFilterClass();
            pSpatFilter.Geometry = (IGeometry)pUnionedEnvelopes;
            pSpatFilter.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;
            pSpatFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            ISpatialIndex spatialIndex = (ISpatialIndex)pGeomBag;
            spatialIndex.AllowIndexing = true;
            spatialIndex.Invalidate();
            IFeatureCursor pLinesFeatCurs2 = (IFeatureCursor)pLinesTable.Search(pSpatFilter, false);

            //IColor pColor = new RgbColorClass();
            //pColor.RGB = System.Drawing.Color.Blue.ToArgb();
            //IScreenDisplay pScreenDisplay = ArcMap.Document.ActiveView.ScreenDisplay;
            //FabricUTILS.FlashGeometry(pSpatFilter.Geometry, pScreenDisplay, pColor, 5, 100);

            IFeature pLineFeat2 = pLinesFeatCurs2.NextFeature();
            HashSet<int> LinePointReferenceHashSet = new HashSet<int>();
            bool bFound = false;
            while (pLineFeat2 != null)
            {
              if (bShowProgressor)
                pStepProgressor.Message = "Refreshing line collection...oid: " + pLineFeat2.OID.ToString();

              int i = (int)pLineFeat2.get_Value(idxFromPointID);
              bFound = LinePointReferenceHashSet.Contains(i);
              if (!bFound)
                LinePointReferenceHashSet.Add(i);

              i = (int)pLineFeat2.get_Value(idxToPointID);
              bFound = LinePointReferenceHashSet.Contains(i);
              if (!bFound)
                LinePointReferenceHashSet.Add(i);

              Marshal.ReleaseComObject(pLineFeat2); //garbage collection
              pLineFeat2 = pLinesFeatCurs2.NextFeature();
            }
            Marshal.ReleaseComObject(pLinesFeatCurs2); //garbage collection

            if (bShowProgressor)
              pStepProgressor.Message = "Checking for orphan points...";
            List<int> OrphanPointsList = new List<int>();
            //also make sure the point is not radial point
            //int iCenterPtFld=  pPointsTable.FindField("CenterPoint");
            //string sCenterPtFld = pPointsTable.Fields.get_Field(iCenterPtFld).Name;
            //pSpatFilter.WhereClause = sCenterPtFld + " NOT IN (1)"; //this isn't fail proof because the center point can also be 0 if connected to a non radial line.
            spatialIndex.Invalidate();
            IFeatureCursor pPointFeatCurs = (IFeatureCursor)pPointsTable.Search(pSpatFilter, false);
            IFeature pPointFeat = pPointFeatCurs.NextFeature();

            while (pPointFeat != null)
            {
              bFound = LinePointReferenceHashSet.Contains(pPointFeat.OID);
              if (!bFound)
              {
                OrphanPointsList.Add(pPointFeat.OID);
                if (bShowProgressor)
                  pStepProgressor.Message = "Deleting orphan points...oid: " + pPointFeat.OID.ToString();
              }
              Marshal.ReleaseComObject(pPointFeat); //garbage collection
              pPointFeat = pPointFeatCurs.NextFeature();
            }
            Marshal.ReleaseComObject(pPointFeatCurs); //garbage collection
            LinePointReferenceHashSet.Clear();

            if (OrphanPointsList.Count > 0)
            {
              InClausePointRefsOnLines = FabricUTILS.InClauseFromOIDsList(OrphanPointsList, tokenLimit);
              //Now test to make sure the point is not a valid radial point, because the center point id can also be null or 0 if connected to a non radial line
              //Search orphan points and if any have a centerpointid on a **line** then exclude it from the orphan point list
              int iCenterPointID = pLinesTable.FindField("centerpointid");
              string sCenterPointIDFld = pLinesTable.Fields.get_Field(iCenterPointID).Name;
              foreach (string sInClause in InClausePointRefsOnLines)
              {
                m_pQF.WhereClause = sCenterPointIDFld + " IN (" + sInClause + ")";
                ICursor pLinesFeatCurs3 = pLinesTable.Search(m_pQF, false);
                IRow pRow = pLinesFeatCurs3.NextRow();
                while (pRow != null)
                {
                  int iCtrPtID = (int)pRow.get_Value(iCenterPointID);
                  OrphanPointsList.Remove(iCtrPtID);
                  Marshal.ReleaseComObject(pRow);
                  pRow = pLinesFeatCurs3.NextRow();
                }
                Marshal.ReleaseComObject(pLinesFeatCurs3);
              }
              //now rebuild the inclause with the updated list
              InClausePointRefsOnLines = FabricUTILS.InClauseFromOIDsList(OrphanPointsList, tokenLimit);

              OrphanPointsList.Clear();

              if (!FabricUTILS.DeleteByInClause(pWS, pPointsTable, pPointsTable.Fields.get_Field(pPointsTable.FindField(pPointsTable.OIDFieldName)),
                    InClausePointRefsOnLines, !bIsUnVersioned, pStepProgressor, pTrackCancel))
              {
                FabricUTILS.AbortEditing(pWS);
                InClausePointRefsOnLines.Clear();
                return;
              }
            }
            else
            {
              if (bUseNonVersionedDelete)
                FabricUTILS.StopEditing(pWS);

              if (pEd.EditState == esriEditState.esriStateEditing)
                pEd.StopOperation("Delete Connection Lines");
              return;
            }
          }
          else
          {
            if (bUseNonVersionedDelete)
              FabricUTILS.StopEditing(pWS);

            if (pEd.EditState == esriEditState.esriStateEditing)
              pEd.StopOperation("Delete Connection Lines");
            return;
          }
          #endregion

          //now need to update the control point references
          #region Update Control Point References

          //need to also take care of control points that have a reference to any of the deleted Orphan points
          ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
          int idxNameFldOnControl = pControlTable.FindField("POINTID");
          string ControlNameFldName = pControlTable.Fields.get_Field(idxNameFldOnControl).Name;
          IQueryFilter pQuFilter = new QueryFilterClass();

          int idxActiveIDX = pControlTable.Fields.FindField("ACTIVE");
          string ActiveFldName = pControlTable.Fields.get_Field(idxActiveIDX).Name;

          if (bShowProgressor)
            pStepProgressor.Message = "Resetting control references on points ...";

          foreach (string z in InClausePointRefsOnLines)
          {
            if (z.Trim() == "")
              break;
            //cleanup associated control points, and associations where underlying points were deleted 
            pQuFilter.SubFields = ControlNameFldName + "," + ActiveFldName;
            pQuFilter.WhereClause = ControlNameFldName + " IN (" + z + ")";
            pSchemaEd.ReleaseReadOnlyFields(pControlTable, esriCadastralFabricTable.esriCFTControl); //release safety-catch
            if (!FabricUTILS.ResetControlAssociations(pControlTable, pQuFilter, bIsUnVersioned))
            {
              pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
              FabricUTILS.AbortEditing(pWS);
              return;
            }
          }
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
          InClausePointRefsOnLines.Clear();

          #endregion

        }

        if (bUseNonVersionedDelete)
          FabricUTILS.StopEditing(pWS);

        if (pEd.EditState == esriEditState.esriStateEditing)
          pEd.StopOperation("Delete Connection Lines");

      }
      catch (Exception ex)
      {
        AbortEdits(bUseNonVersionedDelete, pEd, pWS);
        MessageBox.Show(ex.Message);
        return;
      }
      finally
      {
        RefreshMap(pActiveView, CFLineLayers);
        ICadastralExtensionManager pCExMan = (ICadastralExtensionManager)pCadExtMan;
        IParcelPropertiesWindow2 pPropW = (IParcelPropertiesWindow2)pCExMan.ParcelPropertiesWindow;
        pPropW.RefreshAll();
        //update the TOC
        IMxDocument mxDocument = (ESRI.ArcGIS.ArcMapUI.IMxDocument)(ArcMap.Application.Document);
        for (int i = 0; i < mxDocument.ContentsViewCount; i++)
        {
          IContentsView pCV = (IContentsView)mxDocument.get_ContentsView(i);
          pCV.Refresh(null);
        }

        if (pProgressorDialog != null)
          pProgressorDialog.HideDialog();

        if (bUseNonVersionedDelete)
        {
          pCadEd.CadastralFabricLayer = null;
          CFLineLayers = null;
        }

        if (pMouseCursor != null)
          pMouseCursor.SetCursor(0);
              #region Final Cleanup
      
        FabricUTILS = null;
      
      #endregion
      }

    }

    private bool TraceFabricLines(ref IGSForwardStar FwdStar, int StartNodeId, ref List<int> LineIdList, int iInfinityChecker, bool SequenceHasBoundaryToPoint)
    {
      iInfinityChecker++;
      if (iInfinityChecker > 20000)
        return false;
      //(This is a self-calling function.) In this context 20000 downstream connection lines is like infinity, 
      //so exit gracefully, and avoid probable endless loop. 
      //Possible cause of endless loop? Corrupted data; example, a line with the same from and to point id
      try
      {
        ILongArray iLngArr = FwdStar.get_ToNodes(StartNodeId); 
        //get_ToNodes returns an array of radiated points, not "TO" points in the fabric data model sense
        int iCnt2 = 0;
        iCnt2 = iLngArr.Count;
        IGSLine pGSLine = null;
        for (int i = 0; i < iCnt2; i++)
        {
          int i2 = iLngArr.get_Element(i);
          bool bIsReversed=FwdStar.GetLine(StartNodeId, i2, ref pGSLine);
          int iDBId = pGSLine.DatabaseId;
          bool bIsForwardConnection = (pGSLine.Category == esriCadastralLineCategory.esriCadastralLineConnection ||
                  pGSLine.Category == esriCadastralLineCategory.esriCadastralLinePreciseConnection);
          bool bIsOriginConnection = pGSLine.Category == esriCadastralLineCategory.esriCadastralLineOriginConnection;
          bool bIsRadialLine = pGSLine.Category == esriCadastralLineCategory.esriCadastralLineRadial;
          bool bIsPartConnection = pGSLine.Category == esriCadastralLineCategory.esriCadastralLinePartConnection;
          if ((!bIsReversed && bIsForwardConnection) || (bIsReversed && bIsOriginConnection))
          {//if the line is a standard connection and running with same orientation as GetLine function
            //---OR---if the line is an origin connection and running with opposite orientation as GetLine function
            if (!LineIdList.Contains(iDBId) && !SequenceHasBoundaryToPoint)
              LineIdList.Add(iDBId);
            else
              continue;
            if (!TraceFabricLines(ref FwdStar, i2, ref LineIdList, iInfinityChecker, SequenceHasBoundaryToPoint))
              return false;
          }
          else if (!bIsForwardConnection && !bIsOriginConnection && !bIsRadialLine && !bIsPartConnection)
          {
            SequenceHasBoundaryToPoint = true;
            continue;
          }
        }
        return true;
      }
      catch
      {
        return false;
      }
    }

    protected override void OnUpdate()
    {
      CustomizelHelperExtension v = CustomizelHelperExtension.GetExtension();
      this.Enabled = v.CommandIsEnabled;

      if (!this.Enabled)
        this.Enabled = v.MapHasUnversionedFabric;
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

    private void RefreshMap(IActiveView ActiveView, IArray LineLayers)
    {
      try
      {
        for (int z = 0; z <= LineLayers.Count - 1; z++)
        {
          if (LineLayers.get_Element(z) != null)
          {
            IFeatureSelection pFeatSel = (IFeatureSelection)LineLayers.get_Element(z);
            pFeatSel.Clear();//refreshes the parcel explorer
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, LineLayers.get_Element(z), ActiveView.Extent);
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, LineLayers.get_Element(z), ActiveView.Extent);
          }
        }
      }
      catch
      { }
    }

    private bool IsMultiPart(IGeometry geometry)
    {
      var geometryCollection = geometry as IGeometryCollection;
      return geometryCollection != null && geometryCollection.GeometryCount > 1;
    }

    private void AddSegmentToPolyline(ISegment inAddSegment, ref ISegmentCollection segCollection)
    {
      object obj = Type.Missing;
      segCollection.AddSegment(inAddSegment, ref obj, ref obj);
    }

  }
}
