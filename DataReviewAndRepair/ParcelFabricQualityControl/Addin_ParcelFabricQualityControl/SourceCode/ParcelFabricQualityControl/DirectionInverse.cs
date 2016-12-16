﻿/*
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

using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
namespace ParcelFabricQualityControl
{
  public class DirectionInverse : ESRI.ArcGIS.Desktop.AddIns.Button
  {

    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSetParcels;
    private IQueryFilter m_pQF;
    private string m_sReport;
    private string sUnderline = Environment.NewLine + "---------------------------------------------------------------------------------------------------------------" + Environment.NewLine;
    private Dictionary<int, double[]> m_dict_DiffToReport = new Dictionary<int, double[]>();
    private string m_sLineCount;
    private string m_sParcelCount;
    private string m_sUnit = "meters";
    private bool m_bShowReport = false;
    private bool m_bNoUpdates = false;
    private bool m_bShowProgressor = false;

    public DirectionInverse()
    {
    }

    protected override void OnClick()
    {
      m_bNoUpdates = false;
      m_sReport = "Direction Inverse Report:";
      IEditor m_pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");

      if (m_pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing first, and try again.", "Start Editing", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      ICadastralFabric pCadFabric = null;

      //if we're in an edit session then grab the target fabric
      if (m_pEd.EditState == esriEditState.esriStateEditing)
        pCadFabric = pCadEd.CadastralFabric;

      if (pCadFabric == null)
      {//find the first fabric in the map
        MessageBox.Show
          ("No Parcel Fabric found in the workspace you're editing.\r\nPlease re-start editing on a workspace with a fabric, and try again.", 
            "No fabric found", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }
      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadEd;
      if (pCadPacMan.PacketOpen)
      {
        MessageBox.Show("The Direction Inverse does not work when the parcel is open.\r\nPlease close the parcel and try again.",
          "Direction Inverse", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      IEditProperties2 pEditorProps2 = (IEditProperties2)m_pEd;

      InverseDirectionDLG InverseDirectionDialog = new InverseDirectionDLG(pEditorProps2);
      IArray PolygonLyrArr;
      IMap pMap = m_pEd.Map;
      ISpatialReference pSpatRef = m_pEd.Map.SpatialReference;
      IProjectedCoordinateSystem2 pPCS = null;
      IActiveView pActiveView = ArcMap.Document.ActiveView;

      double dMetersPerUnit = 1;

      if (pSpatRef == null)
        InverseDirectionDialog.lblDistanceUnits1.Text = "<unknown units>";
      else if (pSpatRef is IProjectedCoordinateSystem2)
      {
        pPCS = (IProjectedCoordinateSystem2)pSpatRef;
        string sUnit = pPCS.CoordinateUnit.Name;
        if (sUnit.Contains("Foot") && sUnit.Contains("US"))
          sUnit = "U.S. Feet";
        InverseDirectionDialog.lblDistanceUnits1.Text = sUnit;
        dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
        if (dMetersPerUnit < 1)
          m_sUnit = "feet";
      }

      IAngularConverter pAngConv = new AngularConverterClass();
      Utilities Utils = new Utilities();

      if (!Utils.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTParcels, out PolygonLyrArr))
        return;

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedEdit = false;
      IWorkspace pWS = null;
      ITable pParcelsTable = null;
      ITable pLinesTable = null;
      IProgressDialog2 pProgressorDialog = null;
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);
      var pTool = ArcMap.Application.CurrentTool;
      ArcMap.Application.CurrentTool = null;

      try
      {
       //Get the selection of parcels
        IFeatureLayer pFL0 = (IFeatureLayer)PolygonLyrArr.get_Element(0);

        IDataset pDS = (IDataset)pFL0.FeatureClass;
        pWS = pDS.Workspace;

        if (!Utils.SetupEditEnvironment(pWS, pCadFabric, m_pEd, out bIsFileBasedGDB,
          out bIsUnVersioned, out bUseNonVersionedEdit))
        {
          return;
        }

        ICadastralSelection pCadaSel = (ICadastralSelection)pCadEd;
        IEnumGSParcels pEnumGSParcels = pCadaSel.SelectedParcels;// need to get the parcels before trying to get the parcel count: BUG workaround

        //also need to check for a line selection
        IArray LineLayerArray;
        if (!Utils.GetFabricSubLayers(ArcMap.Document.ActiveView.FocusMap, esriCadastralFabricTable.esriCFTLines,
           true, pCadEd.CadastralFabric, out LineLayerArray))
          return;

        // get the line selection; this code sample uses first line layer for the target fabric (first element)
        int iCntLineSelection = 0;
        for (int k = 0; k < LineLayerArray.Count; k++)
        {
          ISelectionSet2 LineSelection =
          Utils.GetSelectionFromLayer(LineLayerArray.get_Element(k) as ICadastralFabricSubLayer);
          iCntLineSelection += LineSelection.Count;
        }

        //if (pCadaSel.SelectedParcelCount == 0 && pSelSet0.Count == 0 && iCntLineSelection == 0)
        if (pCadaSel.SelectedParcelCount == 0 && iCntLineSelection == 0)
        {
          MessageBox.Show("Please select some fabric parcels and try again.", "No Selection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          m_bShowReport = false;
          return;
        }

        //Display the dialog
        DialogResult pDialogResult = InverseDirectionDialog.ShowDialog();
        if (pDialogResult != DialogResult.OK)
        {
          m_bShowReport = false;
          return;
        }
        m_bShowProgressor = (pCadaSel.SelectedParcelCount > 10);
        if (m_bShowProgressor)
        {
          m_pProgressorDialogFact = new ProgressDialogFactoryClass();
          m_pTrackCancel = new CancelTrackerClass();
          m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
          m_pStepProgressor.MinRange = 1;
          m_pStepProgressor.MaxRange = pCadaSel.SelectedParcelCount * 14; //(estimate 7 lines per parcel 2 loops)
          m_pStepProgressor.StepValue = 1;
          pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        }
        m_bShowReport = InverseDirectionDialog.chkReportResults.Checked;

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
        int tokenLimit = 995;
        List<int> oidList = new List<int>();

        pLinesTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        pParcelsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);

        Dictionary<int, List<int>> dict_ParcelLinesListLookup = new Dictionary<int, List<int>>();

        pEnumGSParcels.Reset();
        IGSParcel pGSParcel = pEnumGSParcels.Next();
        while (pGSParcel != null)
        {
          //Check if the cancel button was pressed. If so, stop process   
          if (m_bShowProgressor)
          {
            if (!m_pTrackCancel.Continue())
              break;
          }
          int iDBId=pGSParcel.DatabaseId;
          if (!oidList.Contains(iDBId))
          {
            oidList.Add(iDBId);
            List<int> LinesList = new List<int>();
            dict_ParcelLinesListLookup.Add(pGSParcel.DatabaseId, LinesList);
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
        
        if (m_bShowProgressor)
        {
          if (m_bShowReport)
            m_bShowReport = m_pTrackCancel.Continue();

          if (!m_pTrackCancel.Continue())
            return;
        }

        int idxLineCategory = pLinesTable.FindField("CATEGORY");
        string LineCategoryFldName = pLinesTable.Fields.get_Field(idxLineCategory).Name;

        int idxParcelID = pLinesTable.FindField("PARCELID");
        string ParcelIDFldName = pLinesTable.Fields.get_Field(idxParcelID).Name;

        int idxDirection = pLinesTable.FindField("BEARING");
        Dictionary<int, int> dict_LinesToParcel = new Dictionary<int, int>();
        Dictionary<int, double> dict_LinesToRecordDirection = new Dictionary<int, double>();
        Dictionary<int, double> dict_LinesToComputedDirection = new Dictionary<int, double>();
        Dictionary<int, double> dict_LinesToInverseDirection = new Dictionary<int, double>();
        Dictionary<int, double> dict_LinesToDirectionOffset = new Dictionary<int, double>();
        Dictionary<int, double> dict_LinesToShapeDistance = new Dictionary<int, double>();
        Dictionary<int, double> dict_LinesToComputedDelta = new Dictionary<int, double>();
        Dictionary<int, List<int>> dict_LinesToRadialLinesPair = new Dictionary<int, List<int>>();
        List<int> lstParcelsWithCurves = new List<int>();

        m_pQF = new QueryFilterClass();
        m_pFIDSetParcels = new FIDSetClass();
        
        if (oidList.Count > 0)
        {
          List<string> sInClauseList = Utils.InClauseFromOIDsList(oidList, tokenLimit);
          foreach (string sInClause in sInClauseList)
          {
            m_pQF.WhereClause = ParcelIDFldName + " IN (" + sInClause + ") AND (" +
                    LineCategoryFldName + " <> 4)";

            ISpatialReference pMapSpatRef = m_pEd.Map.SpatialReference;

            InverseLineDirections(m_pQF, pLinesTable, pMapSpatRef, dict_ParcelLinesListLookup, ref lstParcelsWithCurves, ref dict_LinesToRadialLinesPair,
              ref dict_LinesToComputedDelta, ref dict_LinesToRecordDirection, ref dict_LinesToInverseDirection, ref dict_LinesToShapeDistance,
              ref dict_LinesToDirectionOffset, ref dict_LinesToParcel);
          }
        }

        int m_ParcelsWithLinesOnlyListCount = 0;
        List<int> ExcludeList = new List<int>();

        if (iCntLineSelection > 0)
        {
          ICursor pCursor;
          //if there's a line selection, then grab the parcel ids
          Dictionary<int, int> dict_LineSelection2ParcelIds = new Dictionary<int, int>();
          m_pQF.WhereClause = pLinesTable.Fields.get_Field(idxLineCategory).Name + " <> 4";
          for (int k = 0; k < LineLayerArray.Count; k++)
          {
            ISelectionSet2 LineSelection =
                Utils.GetSelectionFromLayer(LineLayerArray.get_Element(k) as ICadastralFabricSubLayer);
            LineSelection.Search(m_pQF, false, out pCursor);
            IRow pRow = pCursor.NextRow();
            while (pRow != null)
            {
              int ParcelID = (int)pRow.get_Value(idxParcelID);
              if (!dict_LineSelection2ParcelIds.ContainsKey(pRow.OID))
                dict_LineSelection2ParcelIds.Add(pRow.OID, ParcelID);
              Marshal.ReleaseComObject(pRow);
              pRow = pCursor.NextRow();
            }
            Marshal.ReleaseComObject(pCursor);
          }

          List<int> LineSelectionListParcelIds = dict_LineSelection2ParcelIds.Values.Distinct().ToList();
          List<int> ParcelsWithOnlyLinesSelected = LineSelectionListParcelIds.Except(oidList).ToList();
          List<int> LinesWithNoParcelSelection = dict_LineSelection2ParcelIds.Where(x => ParcelsWithOnlyLinesSelected.Contains(x.Value)).Select(x => x.Key).ToList();

          m_ParcelsWithLinesOnlyListCount = ParcelsWithOnlyLinesSelected.Count;

          if (LinesWithNoParcelSelection.Count > 0)
          {
            List<string> sInClauseList1 = Utils.InClauseFromOIDsList(ParcelsWithOnlyLinesSelected, tokenLimit);
            for (int ii = 0; ii < ParcelsWithOnlyLinesSelected.Count;ii++)
              if (!dict_ParcelLinesListLookup.ContainsKey(ParcelsWithOnlyLinesSelected[ii]))
                dict_ParcelLinesListLookup.Add(ParcelsWithOnlyLinesSelected[ii], new List<int>());
            
            ISpatialReference pMapSpatRef = m_pEd.Map.SpatialReference;

            foreach (string sInClause in sInClauseList1)
            {
              m_pQF.WhereClause = pLinesTable.Fields.get_Field(idxParcelID).Name + " IN (" + sInClause + ") AND " + pLinesTable.Fields.get_Field(idxLineCategory).Name + "<> 4";
              List<int> AllLinesOfNonSelectedParcels = InverseLineDirections(m_pQF, pLinesTable, pMapSpatRef, dict_ParcelLinesListLookup, ref lstParcelsWithCurves, ref dict_LinesToRadialLinesPair,
                ref dict_LinesToComputedDelta, ref dict_LinesToRecordDirection, ref dict_LinesToInverseDirection, ref dict_LinesToShapeDistance,
                ref dict_LinesToDirectionOffset, ref dict_LinesToParcel);

              //line exclusion list is the [non-selected set of lines] of the [parcels that are not selected but that have some of their lines selected]
              ExcludeList.AddRange(AllLinesOfNonSelectedParcels.Except(LinesWithNoParcelSelection).ToList());
            }
          }
        }

        m_sParcelCount = (oidList.Count + m_ParcelsWithLinesOnlyListCount).ToString();

        double dDirectionOffset = -1440;//initialize as a -ve multiple of 360.
        if (InverseDirectionDialog.optManualEnteredDirnOffset.Checked)
        {
          if (pAngConv.SetString(InverseDirectionDialog.txtDirectionOffset.Text, esriDirectionType.esriDTNorthAzimuth, pEditorProps2.DirectionUnits))
            dDirectionOffset=pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth,esriDirectionUnits.esriDUDecimalDegrees);
        }

        double dBearingOffsetTolerance = -0.0001;
        if (InverseDirectionDialog.chkDirectionDifference.Checked)
        {
          Double.TryParse(InverseDirectionDialog.txtDirectionDifference.Text, out dBearingOffsetTolerance);
          dBearingOffsetTolerance = dBearingOffsetTolerance / 3600;
        }
        
        double dDistOffsetTolerance = -0.0001;
        if (InverseDirectionDialog.chkSubtendedDistance.Checked)
        {
          Double.TryParse(InverseDirectionDialog.txtSubtendedDist.Text, out dDistOffsetTolerance);
        }

        LineBearingAnalysis(dict_ParcelLinesListLookup,dict_LinesToRecordDirection, dict_LinesToDirectionOffset, dict_LinesToInverseDirection, 
          dict_LinesToComputedDirection, dict_LinesToShapeDistance, dDirectionOffset, dBearingOffsetTolerance, dDistOffsetTolerance, ExcludeList, 
          ref m_pFIDSetParcels, ref m_dict_DiffToReport);

        int[] pParcelIds = new int[m_pFIDSetParcels.Count()];
        ILongArray pParcelsToLock = new LongArrayClass();
        List<int> lstParcelChanges= Utils.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, ref pParcelIds, m_pStepProgressor);

        if (m_pFIDSetParcels.Count() == 0 || dict_LinesToComputedDirection.Count==0)
        {
          m_bNoUpdates = true;
          m_sReport += sUnderline + "No parcels were updated.";
          return;
        }

        #region Create Cadastral Job
        string sTime = "";
        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          //see if parcel locks can be obtained on the selected parcels. First create a job.
          DateTime localNow = DateTime.Now;
          sTime = Convert.ToString(localNow);
          ICadastralJob pJob = new CadastralJobClass();
          pJob.Name = sTime;
          pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
          pJob.Description = "Inverse lines on selected parcels";
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
        #endregion

        #region Test for Edit Locks
        ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)pCadFabric;
        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          pFabLocks.LockingJob = sTime;
          ILongArray pLocksInConflict = null;
          ILongArray pSoftLcksInConflict = null;

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
        #endregion


        List<int> lstOidsOfLines = dict_LinesToComputedDirection.Keys.ToList<int>(); //linq
        List<string> sInClauseList2 = Utils.InClauseFromOIDsList(lstOidsOfLines, tokenLimit);

        if (m_pEd.EditState == esriEditState.esriStateEditing)
        {
          try
          {
            m_pEd.StartOperation();
          }
          catch
          {
            m_pEd.AbortOperation();//abort any open edit operations and try again
            m_pEd.StartOperation();
          }
        }
        if (bUseNonVersionedEdit)
        {
          if (!Utils.StartEditing(pWS, bIsUnVersioned))
            return;
        }

        ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        pSchemaEd.ReleaseReadOnlyFields(pLinesTable, esriCadastralFabricTable.esriCFTLines); //release read-only fields
        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Updating line directions...";
        //now go through the lines to update
        foreach (string sInClause in sInClauseList2)
        {
          m_pQF.WhereClause = pLinesTable.OIDFieldName + " IN (" + sInClause + ")";
          if (!Utils.UpdateTableByDictionaryLookup(pLinesTable, m_pQF, "BEARING", false, 
            dict_LinesToComputedDirection, ref pSchemaEd, m_pStepProgressor, m_pTrackCancel))
          {
            if (m_bShowReport && m_pTrackCancel!=null)
              m_bShowReport=m_pTrackCancel.Continue();
            m_bNoUpdates = true;
            m_sReport += sUnderline + "ERROR occurred updating direction values. No parcel lines were updated.";
            AbortEdits(bIsUnVersioned, m_pEd, pWS);
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set fields back to read-only
            return;
          }

        
        }
  
        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Updating radial lines...";
        //next get the radial lines and update the radial bearings if needed.
        if (lstParcelsWithCurves.Count > 0)
        {
          sInClauseList2.Clear();
          sInClauseList2 = Utils.InClauseFromOIDsList(lstParcelsWithCurves, tokenLimit);
          foreach (string sInClause in sInClauseList2)
          {
            m_pQF.WhereClause = ParcelIDFldName + " IN (" + sInClause + ") AND (" +
                    LineCategoryFldName + " = 4)";
            if (!Utils.UpdateRadialLineBearingsByDictionaryLookups(pLinesTable, m_pQF, false,
              dict_LinesToComputedDirection, dict_LinesToRadialLinesPair, dict_LinesToComputedDelta))
            {
              m_bNoUpdates = true;
              m_sReport += sUnderline + "ERROR occurred updating direction values. No parcel lines were updated.";
              AbortEdits(bIsUnVersioned, m_pEd, pWS);
              pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on
              return;
            }
          }
        }
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set fields back to read-only
        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Updating parcel system fields...";

        //now run through the parcels id list and update misclose and ShapeStdErr m_pFIDSetParcels
        IFIDSet pRegenIds = new FIDSetClass();
        Dictionary<int, List<double>> UpdateSysFieldsLookup = Utils.ReComputeParcelSystemFieldsFromLines(pCadEd, pMap.SpatialReference,
          (IFeatureClass)pParcelsTable, pParcelIds, ref pRegenIds, m_pStepProgressor);

        int iLineCount = dict_LinesToComputedDirection.Count();
        if (iLineCount==0)
        {
          m_bNoUpdates = true;
          m_sReport += sUnderline + "No direction values out of tolerances.";
          m_sReport += Environment.NewLine + "No parcel lines were updated.";

          AbortEdits(bIsUnVersioned, m_pEd, pWS);
          return;
        }

        //Use this update dictionary to update the parcel System fields
        pSchemaEd.ReleaseReadOnlyFields(pParcelsTable, esriCadastralFabricTable.esriCFTParcels);
        Utils.UpdateParcelSystemFieldsByLookup(pParcelsTable, UpdateSysFieldsLookup, false);
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);//set fields back to read-only

        if (pRegenIds.Count() > 0)
        {
          //this is a fall-back for when UpdateSysFields failed
          ICadastralFabricRegeneration pRegenFabric = new CadastralFabricRegenerator();
          #region regenerator enum
          // enum esriCadastralRegeneratorSetting 
          // esriCadastralRegenRegenerateGeometries         =   1 
          // esriCadastralRegenRegenerateMissingRadials     =   2, 
          // esriCadastralRegenRegenerateMissingPoints      =   4, 
          // esriCadastralRegenRemoveOrphanPoints           =   8, 
          // esriCadastralRegenRemoveInvalidLinePoints      =   16, 
          // esriCadastralRegenSnapLinePoints               =   32, 
          // esriCadastralRegenRepairLineSequencing         =   64, 
          // esriCadastralRegenRepairPartConnectors         =   128  
          // By default, the bitmask member is 0 which will only regenerate geometries. 
          // (equivalent to passing in regeneratorBitmask = 1) 
          #endregion
          pRegenFabric.CadastralFabric = pCadFabric;
          pRegenFabric.RegeneratorBitmask = 7 + 64 + 128;
          if(m_pStepProgressor !=null)
            m_pStepProgressor.Message = "Regenerating " + pRegenIds.Count().ToString() + " parcels...";
          pRegenFabric.RegenerateParcels(pRegenIds, false, m_pTrackCancel);
        }

        m_sLineCount = dict_LinesToComputedDirection.Count.ToString();
        m_pEd.StopOperation("Inversed directions on " + m_sLineCount + " lines");

        if (lstParcelChanges.Count() > 0)
          for (int hh = 0; hh < PolygonLyrArr.Count; hh++)
            Utils.SelectByFIDList((IFeatureLayer)PolygonLyrArr.get_Element(hh), lstParcelChanges, esriSelectionResultEnum.esriSelectionResultSubtract);
        
        if (m_bShowReport)
        {
          if (m_bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            m_pStepProgressor.Message = "Generating report. Please wait...";
          }
          if (!m_bNoUpdates)
          {
            m_sReport += sUnderline + m_pFIDSetParcels.Count().ToString() + " out of " + m_sParcelCount + " selected parcels updated.";
            m_sReport += Environment.NewLine + m_sLineCount + " parcel line direction attributes recalculated.";

            m_sReport += sUnderline + "Line OID\t\tDirection Difference\t\tLateral Effect" + Environment.NewLine + "\t\t(shape - attribute)\t\t" + "    (" + m_sUnit + ")" + sUnderline;
            //list sorted by lateral distance difference
            var sortedDict = from entry in m_dict_DiffToReport orderby entry.Value[1] descending select entry;
            var pEnum = sortedDict.GetEnumerator();
            while (pEnum.MoveNext())
            {
              var pair = pEnum.Current;
              string sAngle = "--";
              if (pAngConv.SetAngle(m_dict_DiffToReport[pair.Key][0], esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDecimalDegrees))
              {
                sAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, pEditorProps2.DirectionUnits, pEditorProps2.AngularUnitPrecision);
                if (sAngle.Contains("."))
                  sAngle += "°";
                else
                {
                  int i = sAngle.IndexOf("-");
                  int j = sAngle.LastIndexOf("-");
                  sAngle = sAngle.Substring(0, i) + "° " + sAngle.Replace("-", "' ").Substring(j - 1) + Char.ConvertFromUtf32(34);
                }
              }
              m_sReport += String.Format("   {0,-15}\t{1,-20}\t{2,20:0.000}", pair.Key.ToString(), sAngle, m_dict_DiffToReport[pair.Key][1])
                + Environment.NewLine;

              if (m_bShowProgressor)
              {
                pProgressorDialog.CancelEnabled=false;
                if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                  m_pStepProgressor.Step();
              }

            }
          }
        }

      }
      catch (Exception ex)
      {
        if (m_pEd != null)
          AbortEdits(bIsUnVersioned, m_pEd, pWS);
        m_sReport += sUnderline + "Error:  " + ex.Message;
        m_bNoUpdates = true;
        MessageBox.Show(ex.Message);
      }

      finally
      {
        if (m_bShowReport)
        {
          ReportDLG ReportDialog = new ReportDLG();
          ReportDialog.txtReport.Text = m_sReport;
          ReportDialog.ShowDialog();
        }
        ArcMap.Application.CurrentTool = pTool;
        
        m_pStepProgressor = null;
        m_pTrackCancel = null;

        if (pProgressorDialog != null)
          pProgressorDialog.HideDialog();

        RefreshMap(pActiveView, PolygonLyrArr);
        ICadastralExtensionManager pCExMan = (ICadastralExtensionManager)pCadEd;
        IParcelPropertiesWindow2 pPropW = (IParcelPropertiesWindow2)pCExMan.ParcelPropertiesWindow;
        pPropW.RefreshAll();
        //update the TOC
        IMxDocument mxDocument = (ESRI.ArcGIS.ArcMapUI.IMxDocument)(ArcMap.Application.Document);
        for (int i = 0; i < mxDocument.ContentsViewCount; i++)
        {
          IContentsView pCV = (IContentsView)mxDocument.get_ContentsView(i);
          pCV.Refresh(null);
        }

        if (bUseNonVersionedEdit)
        {
          pCadEd.CadastralFabricLayer = null;
          PolygonLyrArr = null;
        }

        if (pMouseCursor != null)
          pMouseCursor.SetCursor(0);

        m_dict_DiffToReport.Clear();
        Utils = null;
      }

    }

    protected override void OnUpdate()
    {
    }

    private void AbortEdits(bool bUseNonVersionedDelete, IEditor pEd, IWorkspace pWS)
    {
      Utilities FabricUTILS = new Utilities();
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

    private void LineBearingAnalysis(Dictionary<int, List<int>> dict_ParcelLinesListLookup, Dictionary<int, double> dict_LinesToRecordDirection,
      Dictionary<int, double> dict_LinesToDirectionOffset, Dictionary<int, double> dict_LinesToInverseDirection, 
      Dictionary<int, double> dict_LinesToComputedDirection, Dictionary<int, double> dict_LinesToShapeDistance,
      double DirectionOffset, double BearingTolerance, double DistanceTolerance, List<int> ExcludeList, ref IFIDSet AffectedParcelsFIDs, 
      ref Dictionary<int, double[]> m_dict_DiffToReport)
    {
      double dSum = 0;
      double dLongestLineOffset = DirectionOffset; //initialize the correction to incoming DirectionOffset value
      bool bUseDirectionOffset = (DirectionOffset!=-1440);

      IAngularConverter pAngConv = new AngularConverterClass();
      Utilities UTILS = new Utilities();
      bool bIsAffected = false;
      foreach (var key in dict_ParcelLinesListLookup.Keys)
      {
        List<int> ParcelLinesList = dict_ParcelLinesListLookup[key];
        if (ParcelLinesList.Count == 0)
          continue; //if the parcel has no lines then skip
        if (!bUseDirectionOffset)
        {
          Dictionary<int, int> dict_Lookup1 = new Dictionary<int, int>();
          int jj = ParcelLinesList.Count;
          int kk = 0;
          double[] dirOffsets = new double[jj];
          double[] lnDistances = new double[jj];
          foreach (int ll in ParcelLinesList)
          {
            double dd = dict_LinesToDirectionOffset[ll];
            dict_Lookup1.Add(kk, ll);
            lnDistances[kk] = dict_LinesToShapeDistance[ll];
            dirOffsets[kk++] = dd;
            dSum += dd;
          }

          bool bHasPotentialOutliers=false;
          for (int i = 1; i < dirOffsets.GetLength(0); i++)
          {
            bHasPotentialOutliers = (Math.Abs(dirOffsets[i - 1] - dirOffsets[i]) > 0.0001);
            if (bHasPotentialOutliers)
              break;
          }

          double dMedian = UTILS.GetMedian(dirOffsets);
          double dMAD = UTILS.GetMedianDeviationOfTheMean(dirOffsets, dSum);
          double dLongest = 0;
          dLongestLineOffset = 0;
          List<double> GoodLinesList = new List<double>();

          for (int i = 0; i < dirOffsets.GetLength(0); i++)
          {
            double dModZScore = 0.6745 * Math.Abs(dirOffsets[i] - dMedian) / dMAD; 
            //see comments in function GetMedianDeviationOfTheMean(dirOffsets,dSum);
            if (dModZScore <= 0.25 )//&& !bHasPotentialOutliers)
            {
              if (lnDistances[i] > dLongest)
              {
                dLongest = lnDistances[i];
                dLongestLineOffset = dirOffsets[i];
              }
              GoodLinesList.Add(dirOffsets[i]);
            }
          }

          if (dLongestLineOffset >= 360)
            dLongestLineOffset = dLongestLineOffset - 360;

          //jj = GoodLinesList.Count;
          //kk = 0;
          //dSum = 0;
          //double[] e = new double[jj];

          //foreach (double goodDirection in GoodLinesList)
          //{
          //  e[kk++] = goodDirection;
          //  dSum += goodDirection;
          //}
        }

        //apply the exclusion list
        ParcelLinesList = ParcelLinesList.Except(ExcludeList).ToList();

        //now use the longest line's correction to apply to the directions
        IVector3D vecOriginalDirection = new Vector3DClass();
        IVector3D vecInverseDirection = new Vector3DClass();
        IVector3D vecAxis = new Vector3DClass();
        vecAxis.SetComponents(0, 0, 10);
        foreach (int i in ParcelLinesList)
        {
          vecOriginalDirection.PolarSet(dict_LinesToRecordDirection[i] * Math.PI / 180, 0, 1);
          vecInverseDirection.PolarSet(dict_LinesToInverseDirection[i] * Math.PI / 180, 0, 1);
          vecInverseDirection.Rotate(-dLongestLineOffset * Math.PI / 180, vecAxis);// -dMean; //alternative use the mean

          double dNewComputed = vecInverseDirection.Azimuth * 180 / Math.PI;
          double dAngleDiff = Math.Abs(dict_LinesToRecordDirection[i] - dNewComputed);
          if (dAngleDiff >= 180)
            dAngleDiff = Math.Abs(dAngleDiff - 360);

          double dLateralEffectOfAngleDiff = Math.Abs(Math.Tan(dAngleDiff * Math.PI / 180) * dict_LinesToShapeDistance[i]);
          bool bBothUnchecked = (BearingTolerance < 0 && DistanceTolerance < 0);
          //negative Tolerance means the option is unchecked, so no filter, add lines

          if (bBothUnchecked || ((dAngleDiff > BearingTolerance) && BearingTolerance > 0)
                                || ((dLateralEffectOfAngleDiff > DistanceTolerance) && DistanceTolerance > 0))
          {
            bIsAffected = true;
            pAngConv.SetAngle(dNewComputed, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
            dNewComputed = pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
            dict_LinesToComputedDirection.Add(i, dNewComputed);

            double[] dAngleDiffAndLateralOffset = new double[2];
            dAngleDiffAndLateralOffset[0] = dAngleDiff;
            dAngleDiffAndLateralOffset[1] = dLateralEffectOfAngleDiff;
            m_dict_DiffToReport.Add(i, dAngleDiffAndLateralOffset);
          }
        }
        if (bIsAffected)
        {
          bool bExists;
          AffectedParcelsFIDs.Find(key, out bExists);
          if (!bExists)
            AffectedParcelsFIDs.Add(key);
        }
      }
    }

    private void RefreshMap(IActiveView ActiveView, IArray ParcelLayers)
    {
      try
      {
        for (int z = 0; z <= ParcelLayers.Count - 1; z++)
        {
          if (ParcelLayers.get_Element(z) != null)
          {
            //IFeatureSelection pFeatSel = (IFeatureSelection)ParcelLayers.get_Element(z);
            //pFeatSel.Clear();//refreshes the parcel explorer
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection | esriViewDrawPhase.esriViewGeography, ParcelLayers.get_Element(z), ActiveView.Extent);
          }
        }
      }
      catch
      { }
    }

    private List<int> InverseLineDirections(IQueryFilter m_pQF, ITable pLinesTable, ISpatialReference pMapSpatRef, Dictionary<int, List<int>> dict_ParcelLinesListLookup, ref List<int> lstParcelsWithCurves, 
            ref Dictionary<int, List<int>> dict_LinesToRadialLinesPair, ref Dictionary<int, double> dict_LinesToComputedDelta, ref Dictionary<int, double> dict_LinesToRecordDirection, 
            ref Dictionary<int, double> dict_LinesToInverseDirection, ref Dictionary<int, double> dict_LinesToShapeDistance,
            ref Dictionary<int, double> dict_LinesToDirectionOffset, ref Dictionary<int, int> dict_LinesToParcel)
    {
      #region Inverse Directions

      List<int> ListOfLinesForThisProcess = new List<int>();

      int idxCENTERPTID = pLinesTable.FindField("CENTERPOINTID");
      int idxPARCELID = pLinesTable.FindField("PARCELID");
      int idxFROMPTID = pLinesTable.FindField("FROMPOINTID");
      int idxTOPOINTID = pLinesTable.FindField("TOPOINTID");
      int idxDISTANCEID = pLinesTable.FindField("DISTANCE");
      int idxRADIUSID = pLinesTable.FindField("RADIUS");
      int idxDirection = pLinesTable.FindField("BEARING");

      IAngularConverter pAngConv = new AngularConverterClass();
      IVector3D vecAttributeDirection = new Vector3DClass();
      IVector3D vecGeometryDirection = new Vector3DClass();
      ILine pLine = new LineClass();

      ICursor pCursor = pLinesTable.Search(m_pQF, false);
      IRow pLineRecord = pCursor.NextRow();
      while (pLineRecord != null)
      {
        IFeature pFeat = (IFeature)pLineRecord;
        IGeometry pGeom = pFeat.Shape;
        if (pGeom != null)
        {
          if (!pGeom.IsEmpty)
          {

            pGeom.Project(pMapSpatRef);

            int iCtrPtID = -1;
            object dVal = pFeat.get_Value(idxCENTERPTID);
            if (dVal != DBNull.Value)
            {
              iCtrPtID = Convert.ToInt32(dVal);
              int iParcelID = Convert.ToInt32(pFeat.get_Value(idxPARCELID));
              int iFromID = Convert.ToInt32(pFeat.get_Value(idxFROMPTID));
              int iToID = Convert.ToInt32(pFeat.get_Value(idxTOPOINTID));

              ISegmentCollection pSegColl = (ISegmentCollection)pGeom;
              ISegment pSeg = pSegColl.get_Segment(0);
              if (pSegColl.SegmentCount == 1 && pSeg is ICircularArc)
              {
                ICircularArc pCirc = pSeg as ICircularArc;
                if (!pCirc.IsLine)
                {
                  double dChordDist = Convert.ToDouble(pFeat.get_Value(idxDISTANCEID));
                  double dRadius = Convert.ToDouble(pFeat.get_Value(idxRADIUSID));

                  //compute circular arc to get the central angle parameter, 
                  //use *attribute* radius and *attribute* chord distance
                  IConstructCircularArc pConstrArc = new CircularArcClass();
                  bool bCurveConstructPass = true;
                  try {pConstrArc.ConstructBearingRadiusChord(pSeg.FromPoint, 0, (dRadius < 0), Math.Abs(dRadius), dChordDist, pCirc.IsMinor);}
                  catch {bCurveConstructPass = false;}
                  if (bCurveConstructPass)
                  {
                    ICircularArc pCircArc = pConstrArc as ICircularArc;
                    IAngularConverter pAngCon = new AngularConverterClass();
                    pAngCon.SetAngle(Math.Abs(pCircArc.CentralAngle), esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
                    double dCentralAngle = pAngCon.GetAngle(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDecimalDegrees);
                    List<int> lstRadialPairIdentity = new List<int>();

                    if (!lstParcelsWithCurves.Contains(iParcelID))
                      lstParcelsWithCurves.Add(iParcelID);

                    lstRadialPairIdentity.Add(iParcelID);
                    lstRadialPairIdentity.Add(iCtrPtID);
                    lstRadialPairIdentity.Add(iFromID); //from point of radial line as curve start
                    lstRadialPairIdentity.Add(iToID); //from point of radial line as curve end
                    lstRadialPairIdentity.Add(Convert.ToInt32(dRadius < 0)); //store info about curve clockwise or not. TRUE=1 = CCW
                    dict_LinesToRadialLinesPair.Add(pFeat.OID, lstRadialPairIdentity);
                    dict_LinesToComputedDelta.Add(pFeat.OID, dCentralAngle);
                  }
                }
              }
            }
            double dAttributeDirection = (double)pLineRecord.get_Value(idxDirection);
            dict_LinesToRecordDirection.Add(pFeat.OID, dAttributeDirection);
            vecAttributeDirection.PolarSet(dAttributeDirection * Math.PI / 180, 0, 1);

            double dCorrectedDirection = dAttributeDirection; //initialize as the same
            IPolyline pPolyline = (IPolyline)pGeom;
            IPoint pPt1 = pPolyline.FromPoint;
            IPoint pPt2 = pPolyline.ToPoint;
            pLine.PutCoords(pPt1, pPt2);
            pAngConv.SetAngle(pLine.Angle, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
            double dInverseDirn = pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
            dict_LinesToInverseDirection.Add(pFeat.OID, dInverseDirn);
            dict_LinesToShapeDistance.Add(pFeat.OID, pLine.Length);
            vecGeometryDirection.PolarSet(dInverseDirn * Math.PI / 180, 0, 1);
            
            double dOffset =dAttributeDirection - dInverseDirn;
            if ((dOffset)<180)
              dOffset = dOffset + 360;

            dict_LinesToDirectionOffset.Add(pFeat.OID, dOffset);
            int fId = (int)pFeat.get_Value(idxPARCELID);
            List<int> ll = dict_ParcelLinesListLookup[fId];
            ll.Add(pFeat.OID);
            dict_LinesToParcel.Add(pFeat.OID, fId);
            ListOfLinesForThisProcess.Add(pFeat.OID);
          }
        }
        Marshal.ReleaseComObject(pLineRecord);
        pLineRecord = pCursor.NextRow();
      }
      Marshal.ReleaseComObject(pCursor);
      return ListOfLinesForThisProcess;
      #endregion
    }

  }
}
