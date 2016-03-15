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
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;

namespace ParcelFabricQualityControl
{
  public class DistanceInverse : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSetParcels;
    private IQueryFilter m_pQF;
    private string m_sReport;
    private string sUnderline = Environment.NewLine + "---------------------------------------------------------------------------------------------------------------" + Environment.NewLine;
    private Dictionary<int, double> m_dict_DiffToReport = new Dictionary<int, double>();
    private string m_sLineCount;
    private int m_iTotalLineCount;
    private int m_iExcludedLineCount;
    private string m_sParcelCount;
    private bool m_bShowReport=false;
    private bool m_bShowProgressor = false;
    private bool m_bNoUpdates=false;
    private string m_sScaleMethod;
    private string m_sHeight_Or_ElevationLayer;
    private string m_stxtDistDifference;
    private string m_sUnit="meters";
    private double m_dAverageCombinedScaleFactor=1.0;

    public DistanceInverse()
    {
    }

    protected override void OnClick()
    {

      m_sReport = "Distance Inverse Report:";
      m_bNoUpdates = false;
      IEditor m_pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");

      if (m_pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing first, and try again.", "Start Editing", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }
      
      IArray PolygonLyrArr;
      IMap pMap = m_pEd.Map;
      ICadastralFabric pCadFabric = null;

      //if we're in an edit session then grab the target fabric
      if (m_pEd.EditState == esriEditState.esriStateEditing)
        pCadFabric = pCadEd.CadastralFabric;

      if (pCadFabric == null)
      {//find the first fabric in the map
          MessageBox.Show
            ("No Parcel Fabric found in the workspace you're editing.\r\nPlease re-start editing on a workspace with a fabric, and try again.",
            "No Fabric found", MessageBoxButtons.OK, MessageBoxIcon.Information);
          return;
      }

      Utilities Utils = new Utilities();

      if (!Utils.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTParcels, out PolygonLyrArr))
        return;
      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadEd;
      if (pCadPacMan.PacketOpen)
      {
        MessageBox.Show("The Distance Inverse does not work when the parcel is open.\r\nPlease close the parcel and try again.",
          "Distance Inverse", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      ISpatialReference pMapSpatRef = m_pEd.Map.SpatialReference;
      IGeoDataset pGeoDS = (IGeoDataset)pCadEd.CadastralFabric;
      ISpatialReference pFabricSpatRef = pGeoDS.SpatialReference;
      
      IProjectedCoordinateSystem2 pPCS = null;
      IActiveView pActiveView = ArcMap.Document.ActiveView;

      double dMetersPerUnit = 1;
      int m_ParcelsWithLinesOnlyListCount = 0;
      bool bFabricIsInGCS = !(pFabricSpatRef is IProjectedCoordinateSystem2);
      
      IEditProperties2 pEditorProps2 = (IEditProperties2)m_pEd;
      InverseDistanceDlg InverseDistanceDialog = new InverseDistanceDlg(pEditorProps2, pMap);

      if (pMapSpatRef == null)
        InverseDistanceDialog.lblDistanceUnits1.Text = "<unknown units>";
      else if (pMapSpatRef is IProjectedCoordinateSystem2)
      {
        pPCS = (IProjectedCoordinateSystem2)pMapSpatRef;
        string sUnit=pPCS.CoordinateUnit.Name;
        if (sUnit.Contains("Foot") && sUnit.Contains("US"))
          sUnit = "U.S. Feet";
        InverseDistanceDialog.lblDistanceUnits1.Text = sUnit;
        dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
        if (dMetersPerUnit < 1)
          m_sUnit = "feet";
      }

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedEdit = false;
      IWorkspace pWS = null;
      ITable pParcelsTable = null;
      ITable pLinesTable = null;
      IProgressDialog2 pProgressorDialog = null;
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);
      var pTool = ArcMap.Application.CurrentTool;
      try
      {
        IFeatureLayer pFL = (IFeatureLayer)PolygonLyrArr.get_Element(0);
        IDataset pDS = (IDataset)pFL.FeatureClass;
        pWS = pDS.Workspace;

        if (!Utils.SetupEditEnvironment(pWS, pCadFabric, m_pEd, out bIsFileBasedGDB,
          out bIsUnVersioned, out bUseNonVersionedEdit))
        {
          return;
        }

        ICadastralSelection pCadaSel = (ICadastralSelection)pCadEd;
        IEnumGSParcels pEnumGSParcels = pCadaSel.SelectedParcels;// need to get the parcels before trying to get the parcel count: BUG workaround
        IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
        ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;


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

        if (pCadaSel.SelectedParcelCount == 0 && pSelSet.Count == 0 && iCntLineSelection == 0)
        {
          MessageBox.Show("Please select some fabric parcels and try again.", "No Selection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          m_bShowReport = false;
          return;
        }

        ArcMap.Application.CurrentTool = null;

        //Display the dialog
        DialogResult pDialogResult = InverseDistanceDialog.ShowDialog();
        if (pDialogResult != DialogResult.OK)
        {
          m_bShowReport = false;
          return;
        }

        bool bInverseAll = !InverseDistanceDialog.chkDistanceDifference.Checked;
        bool bApplyManuallyEnteredScaleFactor = InverseDistanceDialog.chkApplyScaleFactor.Checked && InverseDistanceDialog.optUserEnteredScaleFactor.Checked;
        bool bCalculateScaleFactorFromHeight = InverseDistanceDialog.chkApplyScaleFactor.Checked && InverseDistanceDialog.optComputeForMe.Checked;

        double dEllipsoidalHeight = 0;
        double dToMetersHeightConversionFactor = 1;
        bool bManualEnteredHeight = (InverseDistanceDialog.cboScaleMethod.SelectedIndex == 0 && bCalculateScaleFactorFromHeight);
        bool bGetElevationFromLayer = (InverseDistanceDialog.cboScaleMethod.SelectedIndex == 1 && bCalculateScaleFactorFromHeight);
        bool bPass = false;
        if (bManualEnteredHeight)
        {
          bPass = Double.TryParse(InverseDistanceDialog.txtHeightParameter.Text, out dEllipsoidalHeight);
          if (InverseDistanceDialog.cboUnits.SelectedIndex == 1) //1=feet
            dEllipsoidalHeight = dEllipsoidalHeight * .3048;
          m_sHeight_Or_ElevationLayer = InverseDistanceDialog.txtHeightParameter.Text;
        }
        else if (bGetElevationFromLayer)
        {
          if (InverseDistanceDialog.cboUnits.SelectedIndex == 1) //1=feet
            dToMetersHeightConversionFactor = .3048;

          m_sHeight_Or_ElevationLayer = InverseDistanceDialog.txtElevationLyr.Text;
          if (InverseDistanceDialog.ElevationFeatureLayer == null || InverseDistanceDialog.ElevationFieldIndex==-1)
          {
            MessageBox.Show("Please select an elevation source and try again.", "Elevation Layer not found",
              MessageBoxButtons.OK, MessageBoxIcon.Information);
            m_bShowReport = false;
            return;
          }
        }

        if (bCalculateScaleFactorFromHeight)
          m_sScaleMethod = "Height: ";

        if (!bInverseAll)
          m_stxtDistDifference = InverseDistanceDialog.txtDistDifference.Text;

        double dScaleFactor = 1;
        if (bApplyManuallyEnteredScaleFactor)
        {
          bPass = Double.TryParse(InverseDistanceDialog.txtScaleFactor.Text, out dScaleFactor);
          m_dAverageCombinedScaleFactor = dScaleFactor;
        }

        double dDifference = 0;
        if (!bInverseAll)
          bPass = Double.TryParse(InverseDistanceDialog.txtDistDifference.Text, out dDifference);

        m_bShowReport = InverseDistanceDialog.chkReportResults.Checked;

        m_bShowProgressor = (pSelSet.Count > 10) || pCadaSel.SelectedParcelCount > 10;
        if (m_bShowProgressor)
        {
          m_pProgressorDialogFact = new ProgressDialogFactoryClass();
          m_pTrackCancel = new CancelTrackerClass();
          m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
          m_pStepProgressor.MinRange = 1;
          m_pStepProgressor.MaxRange = pCadaSel.SelectedParcelCount * 14; //(estimate 7 lines per parcel)
          m_pStepProgressor.StepValue = 1;
          pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        }

        m_pQF = new QueryFilterClass();
        string sPref; string sSuff;

        ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
        sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
        sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

        //====== need to do this for all the parcel sublayers in the map that are part of the target fabric
        //pEnumGSParcels should take care of this automatically
        //but need to do this for line sublayer

        if (m_bShowProgressor)
        {
          pProgressorDialog.ShowDialog();
          m_pStepProgressor.Message = "Collecting parcel data...";
        }

        //Add the OIDs of all the selected parcels into a new feature IDSet
        int tokenLimit =995;
        List<int> oidList = new List<int>();

        pLinesTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        pParcelsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);

        int idxLineCategory = pLinesTable.FindField("CATEGORY");
        string LineCategoryFldName = pLinesTable.Fields.get_Field(idxLineCategory).Name;

        int idxParcelID = pLinesTable.FindField("PARCELID");
        string ParcelIDFldName = pLinesTable.Fields.get_Field(idxParcelID).Name;

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
          int iDBId = pGSParcel.DatabaseId;
          if (!oidList.Contains(iDBId))
            oidList.Add(iDBId);

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

        Dictionary<int, int> dict_LinesToParcel = new Dictionary<int, int>();
        Dictionary<int, double> dict_LinesToInverseDistance = new Dictionary<int, double>();
        Dictionary<int, List<double>> dict_LinesToInverseCircularCurve = new Dictionary<int, List<double>>();
        Dictionary<int, List<int>> dict_LinesToRadialLinesPair = new Dictionary<int, List<int>>();
        List<int> lstParcelsWithCurves = new List<int>();
        List<double> lstCombinedScaleFactor = new List<double>();

        m_pFIDSetParcels = new FIDSetClass();
        m_pQF = new QueryFilterClass();

        if (oidList.Count() > 0)
        {
          List<string> sInClauseList0 = Utils.InClauseFromOIDsList(oidList, tokenLimit);
          foreach (string sInClause in sInClauseList0)
          {
            m_pQF.WhereClause = ParcelIDFldName + " IN (" + sInClause + ") AND (" +
                    LineCategoryFldName + " <> 4)";

            if (bGetElevationFromLayer)
              InverseLineDistancesByElevationLayer(m_pQF, pLinesTable, bFabricIsInGCS, pMapSpatRef, pFabricSpatRef, dMetersPerUnit, InverseDistanceDialog.ElevationFeatureLayer,
                InverseDistanceDialog.ElevationFieldIndex, dDifference, dToMetersHeightConversionFactor, bInverseAll, ref dict_LinesToParcel, ref dict_LinesToInverseDistance, 
                ref dict_LinesToInverseCircularCurve, ref dict_LinesToRadialLinesPair, ref lstParcelsWithCurves, m_pTrackCancel,
                ref m_dict_DiffToReport, ref m_iTotalLineCount,ref m_iExcludedLineCount, ref lstCombinedScaleFactor);
            else
              InverseLineDistances(m_pQF, pLinesTable, bFabricIsInGCS, pMapSpatRef, pFabricSpatRef, dMetersPerUnit, bApplyManuallyEnteredScaleFactor, dScaleFactor,
                dEllipsoidalHeight, dDifference, bInverseAll, ref dict_LinesToParcel, ref dict_LinesToInverseDistance, ref dict_LinesToInverseCircularCurve,
                ref dict_LinesToRadialLinesPair, ref lstParcelsWithCurves, m_pTrackCancel, ref m_dict_DiffToReport, ref m_iTotalLineCount, ref m_iExcludedLineCount, ref lstCombinedScaleFactor);

            if (m_bShowProgressor)
            {
              if (m_bShowReport)
                m_bShowReport = m_pTrackCancel.Continue();

              if (!m_pTrackCancel.Continue())
                return;
            }
          }
        }

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
          List<int> ParcelsWithLinesOnlyList = LineSelectionListParcelIds.Except(oidList).ToList();
          List<int> LinesWithNoParcelSelection = dict_LineSelection2ParcelIds.Where(x => ParcelsWithLinesOnlyList.Contains(x.Value)).Select(x => x.Key).ToList();

          m_ParcelsWithLinesOnlyListCount = ParcelsWithLinesOnlyList.Count;

          if (LinesWithNoParcelSelection.Count > 0)
          {
            List<string> sInClauseList1 = Utils.InClauseFromOIDsList(LinesWithNoParcelSelection, tokenLimit);
            foreach (string sInClause in sInClauseList1)
            {
              m_pQF.WhereClause = pLinesTable.OIDFieldName + " IN (" + sInClause + ")";

              if (bGetElevationFromLayer)
                InverseLineDistancesByElevationLayer(m_pQF, pLinesTable, bFabricIsInGCS, pMapSpatRef, pFabricSpatRef, dMetersPerUnit, InverseDistanceDialog.ElevationFeatureLayer,
                  InverseDistanceDialog.ElevationFieldIndex, dDifference,dToMetersHeightConversionFactor, bInverseAll, ref dict_LinesToParcel, ref dict_LinesToInverseDistance,
                  ref dict_LinesToInverseCircularCurve, ref dict_LinesToRadialLinesPair, ref lstParcelsWithCurves, m_pTrackCancel,
                  ref m_dict_DiffToReport, ref m_iTotalLineCount, ref m_iExcludedLineCount, ref lstCombinedScaleFactor);
              else
                InverseLineDistances(m_pQF, pLinesTable, bFabricIsInGCS, pMapSpatRef, pFabricSpatRef, dMetersPerUnit, bApplyManuallyEnteredScaleFactor, dScaleFactor,
                  dEllipsoidalHeight, dDifference, bInverseAll, ref dict_LinesToParcel, ref dict_LinesToInverseDistance, ref dict_LinesToInverseCircularCurve,
                  ref dict_LinesToRadialLinesPair, ref lstParcelsWithCurves, m_pTrackCancel, ref m_dict_DiffToReport, ref m_iTotalLineCount, 
                  ref m_iExcludedLineCount, ref lstCombinedScaleFactor);

              if (m_bShowProgressor)
              {
                if (m_bShowReport)
                  m_bShowReport = m_pTrackCancel.Continue();

                if (!m_pTrackCancel.Continue())
                  return;
              }
            }
          }
        }


        m_sParcelCount = (oidList.Count + m_ParcelsWithLinesOnlyListCount).ToString();

        if (m_pFIDSetParcels.Count() == 0)
        {
          m_bNoUpdates = true;
          m_sReport += sUnderline + "No parcels were updated.";
          return;
        }
        int[] pParcelIds = new int[m_pFIDSetParcels.Count()];

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
        //if we're in an enterprise then test for edit locks
        ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)pCadFabric;
        ILongArray pParcelsToLock = new LongArrayClass();
        List<int> lstParcelChanges = Utils.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, ref pParcelIds, m_pStepProgressor);
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
        pSchemaEd.ReleaseReadOnlyFields(pLinesTable, esriCadastralFabricTable.esriCFTLines); //release safety-catch

        List<int> lstOidsOfLines = dict_LinesToInverseDistance.Keys.ToList<int>(); //linq
        List<int> lstOidsOfCurves = dict_LinesToInverseCircularCurve.Keys.ToList<int>(); //linq

        lstOidsOfLines.AddRange(lstOidsOfCurves);
        List<string> sInClauseList2 = Utils.InClauseFromOIDsList(lstOidsOfLines, tokenLimit);

        if (m_bShowProgressor)
          m_pStepProgressor.Message = "Updating line distances...";

        //now go through the lines to update
        foreach (string sInClause in sInClauseList2)
        {
          m_pQF.WhereClause = pLinesTable.OIDFieldName + " IN (" + sInClause + ")";
          if (!Utils.UpdateCOGOByDictionaryLookups(pLinesTable, m_pQF, bIsUnVersioned,
            dict_LinesToInverseDistance, dict_LinesToInverseCircularCurve, m_pStepProgressor, m_pTrackCancel))
          {
            if(m_bShowProgressor)
              if (m_bShowReport)
                m_bShowReport = m_pTrackCancel.Continue();
            m_bNoUpdates = true;
            m_sReport += sUnderline + "ERROR occurred updating distance records. No parcel lines were updated.";
            AbortEdits(bIsUnVersioned, m_pEd, pWS);
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on
            return;
          }
        }

        lstOidsOfLines.Clear();

        //next get the radial lines and update the distances to match the curve radius, if needed.
        if (lstParcelsWithCurves.Count > 0)
        {
          if (m_bShowProgressor)
            m_pStepProgressor.Message = "Updating radial lines...";
          List<string> sInClauseList3 = Utils.InClauseFromOIDsList(lstParcelsWithCurves, tokenLimit);
          foreach (string sInClause in sInClauseList3)
          {
            m_pQF.WhereClause = ParcelIDFldName + " IN (" + sInClause + ") AND (" +
                    LineCategoryFldName + " = 4)";
            if (!Utils.UpdateRadialLineDistancesByDictionaryLookups(pLinesTable, m_pQF, bIsUnVersioned,
              dict_LinesToInverseCircularCurve, dict_LinesToRadialLinesPair))
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
        Dictionary<int, List<double>> UpdateSysFieldsLookup = Utils.ReComputeParcelSystemFieldsFromLines(pCadEd, pMapSpatRef,
          (IFeatureClass)pParcelsTable, pParcelIds, ref pRegenIds, m_pStepProgressor);

        //Use this update dictionary to update the parcel records
        pSchemaEd.ReleaseReadOnlyFields(pParcelsTable, esriCadastralFabricTable.esriCFTParcels);
        Utils.UpdateParcelSystemFieldsByLookup(pParcelsTable, UpdateSysFieldsLookup, bIsUnVersioned);
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
          if (m_pStepProgressor!=null)
            m_pStepProgressor.Message = "Regenerating " + pRegenIds.Count().ToString() + " parcels...";
          pRegenFabric.RegenerateParcels(pRegenIds, false, m_pTrackCancel);
        }

        m_sLineCount = dict_LinesToInverseDistance.Count.ToString();

        m_pEd.StopOperation("Inversed distances on " + m_sLineCount + " lines");

        if (lstCombinedScaleFactor.Count() > 0)
          m_dAverageCombinedScaleFactor = lstCombinedScaleFactor.Average();

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
            m_sReport += Environment.NewLine + m_sLineCount + " out of " + m_iTotalLineCount.ToString() + " line distance attributes recalculated.";
            if (m_stxtDistDifference != null)
              m_sReport += Environment.NewLine + "Excluded " + m_iExcludedLineCount.ToString() + " lines with differences less than " + m_stxtDistDifference + " " + m_sUnit + ".";

            m_sReport += sUnderline;
            if (m_sScaleMethod != null)
              m_sReport += m_sScaleMethod + m_sHeight_Or_ElevationLayer;

            m_sReport += Environment.NewLine + "Average scale factor: " + m_dAverageCombinedScaleFactor.ToString("0.000000000000");
            m_sReport += sUnderline + "Line OID\t\tDifference (" + m_sUnit + ")" + Environment.NewLine + "\t\t" + "(shape - attribute)" + sUnderline;
            //list sorted by distance difference
            var sortedDict = from entry in m_dict_DiffToReport orderby entry.Value descending select entry;
            var pEnum = sortedDict.GetEnumerator();
            while (pEnum.MoveNext())
            {
              var pair = pEnum.Current;
              m_sReport += String.Format("   {0,-15}\t{1,12:0.000}", pair.Key.ToString(), m_dict_DiffToReport[pair.Key])
                + Environment.NewLine;

              if (m_bShowProgressor)
              {
                pProgressorDialog.CancelEnabled = false;
                if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                  m_pStepProgressor.Step();
              }
            
            }
          }

          m_iTotalLineCount = m_iExcludedLineCount = 0;
          m_sScaleMethod = m_sLineCount = m_sParcelCount = m_sScaleMethod = m_sHeight_Or_ElevationLayer = m_stxtDistDifference = null;
          m_dAverageCombinedScaleFactor = 1.0;
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
          m_sReport += sUnderline;
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
      Enabled = ArcMap.Application != null;
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

    private void RefreshMap(IActiveView ActiveView, IArray ParcelLayers)
    {
      try
      {
        for (int z = 0; z <= ParcelLayers.Count - 1; z++)
        {
          if (ParcelLayers.get_Element(z) != null)
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection |esriViewDrawPhase.esriViewGeography, ParcelLayers.get_Element(z), ActiveView.Extent);
        }
      }
      catch
      { }
    }

    private void InverseLineDistances(IQueryFilter m_pQF, ITable pLinesTable, bool bFabricIsInGCS, ISpatialReference pMapSpatRef, ISpatialReference pFabricSpatRef,
      double dMetersPerUnit, bool bApplyManuallyEnteredScaleFactor, double dScaleFactor, double dEllipsoidalHeight, double dDifference, bool bInverseAll,
      ref Dictionary<int, int> dict_LinesToParcel, ref Dictionary<int, double> dict_LinesToInverseDistance, ref Dictionary<int, List<double>> dict_LinesToInverseCircularCurve,
      ref Dictionary<int, List<int>> dict_LinesToRadialLinesPair, ref List<int> lstParcelsWithCurves, ITrackCancel pTrackCancel, 
      ref Dictionary<int, double> dict_DiffToReport, ref int LineCount, ref int ExcludedLineCount, ref List<double> lstCombinedScaleFactor)
    {
      bool bTrackCancel = (pTrackCancel != null);

      int idxLineCategory = pLinesTable.FindField("CATEGORY");
      string LineCategoryFldName = pLinesTable.Fields.get_Field(idxLineCategory).Name;

      int idxParcelID = pLinesTable.FindField("PARCELID");
      string ParcelIDFldName = pLinesTable.Fields.get_Field(idxParcelID).Name;

      int idxDistance = pLinesTable.FindField("DISTANCE");
      int idxRadius = pLinesTable.FindField("RADIUS");
      int idxCenterPtId = pLinesTable.FindField("CENTERPOINTID");
      int idxFromPtId = pLinesTable.FindField("FROMPOINTID");
      int idxToPtId = pLinesTable.FindField("TOPOINTID");

      ILine pLine = new LineClass();

      ICursor pCursor = pLinesTable.Search(m_pQF, false);
      IRow pLineRecord = pCursor.NextRow();
      while (pLineRecord != null)
      {
        LineCount++;
        IFeature pFeat = (IFeature)pLineRecord;
        IGeometry pGeom = pFeat.Shape;
        if (pGeom != null)
        {
          if (!pGeom.IsEmpty)
          {
            //need to project to map's data frame
            if (bFabricIsInGCS)
              pGeom.Project(pMapSpatRef);

            double dAttributeDistance = (double)pLineRecord.get_Value(idxDistance);
            double dAttributeRadius = 0;
            double dGeometryRadius = 0;
            double dGeometryCentralAngle = 0;

            bool bIsCircularArc = false;
            bool bIsMinor = true;
            bool bIsCCW = true;

            object obj = pLineRecord.get_Value(idxRadius);
            if (obj != DBNull.Value)
              dAttributeRadius = Convert.ToDouble(obj);

            int iCtrPointID = -1;
            obj = pLineRecord.get_Value(idxCenterPtId);
            if (obj == DBNull.Value)
              dAttributeRadius = 0;
            else
              iCtrPointID = Convert.ToInt32(obj);

            if (dAttributeRadius != 0)
            {//has a cp ref and a radius attribute
              ISegmentCollection pSegColl = (ISegmentCollection)pGeom;
              ISegment pSeg = pSegColl.get_Segment(0);
              if (pSegColl.SegmentCount == 1 && pSeg is ICircularArc)
              {
                ICircularArc pCirc = pSeg as ICircularArc;
                if (!pCirc.IsLine)
                {
                  bIsCircularArc = true;
                  bIsMinor = pCirc.IsMinor;
                  dGeometryRadius = pCirc.Radius;
                  dGeometryCentralAngle = pCirc.CentralAngle;
                  if (bFabricIsInGCS)
                    dGeometryRadius = dGeometryRadius * dMetersPerUnit;
                  bIsCCW = pCirc.IsCounterClockwise;
                }
              }
            }

            Utilities Utils = new Utilities();
            double dCorrectedDist = dAttributeDistance; //initialize as the same

            IPolyline pPolyline = (IPolyline)pGeom;
            IPoint pPt1 = pPolyline.FromPoint;
            IPoint pPt2 = pPolyline.ToPoint;
            pLine.PutCoords(pPt1, pPt2);
            double dLineLength = pLine.Length;
            if (bApplyManuallyEnteredScaleFactor)
            {
              dCorrectedDist = dLineLength / dScaleFactor;
              if (bFabricIsInGCS)
                dCorrectedDist = dCorrectedDist * dMetersPerUnit;
            }
            else
              dCorrectedDist = Utils.InverseDistanceByGroundToGrid(pFabricSpatRef, pPt1, pPt2, dEllipsoidalHeight);

            lstCombinedScaleFactor.Add(dLineLength / dCorrectedDist);

            double dComputedDiff = Math.Abs(dCorrectedDist - dAttributeDistance);
            int parcelId = (int)pFeat.get_Value(idxParcelID);

            if (dComputedDiff > dDifference || bInverseAll)
            {
              bool bExists;
              m_pFIDSetParcels.Find(parcelId, out bExists);
              if (!bExists)
                m_pFIDSetParcels.Add(parcelId);
              if (!dict_LinesToParcel.ContainsKey(pFeat.OID))
              {
                dict_LinesToParcel.Add(pFeat.OID, parcelId);
                dict_LinesToInverseDistance.Add(pFeat.OID, dCorrectedDist);
                dict_DiffToReport.Add(pFeat.OID, dComputedDiff);
              }
            }
            else 
              ExcludedLineCount++;

            if (bIsCircularArc)
            {
              double dRadius = dAttributeRadius;
              if ((Math.Abs(Math.Abs(dAttributeRadius) - Math.Abs(dGeometryRadius)) > dDifference) || bInverseAll)
              {
                double dChordDist = dCorrectedDist;

                //use the corrected chord and keep the geometry central angle to re-compute the radius and arclength
                IConstructCircularArc pConstrArc = new CircularArcClass();
                if (bFabricIsInGCS)
                  pConstrArc.ConstructBearingAngleChord(pPt1, 0, bIsCCW, dGeometryCentralAngle, Math.Abs(dChordDist/dMetersPerUnit));
                else
                  pConstrArc.ConstructBearingAngleChord(pPt1, 0, bIsCCW, dGeometryCentralAngle, dChordDist);

                ICircularArc pCircArc = pConstrArc as ICircularArc;
                double dArcLength = pCircArc.Length;

                if (bIsCCW)
                  dRadius = pCircArc.Radius * -1;
                else
                  dRadius = pCircArc.Radius;

                IAngularConverter pAngCon = new AngularConverterClass();
                pAngCon.SetAngle(Math.Abs(pCircArc.CentralAngle), esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
                double dCentralAngle = pAngCon.GetAngle(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDecimalDegrees);
                //0:radius, 1:chord length, 2:arc length, 3:delta
                List<double> CurveParamList = new List<double>();
                CurveParamList.Add(dChordDist);
                CurveParamList.Add(dRadius);
                CurveParamList.Add(dArcLength);
                CurveParamList.Add(dCentralAngle);
                dict_LinesToInverseCircularCurve.Add(pFeat.OID, CurveParamList);

                List<int> lstRadialPairIdentity = new List<int>();

                if (!lstParcelsWithCurves.Contains(parcelId))
                  lstParcelsWithCurves.Add(parcelId);

                int iFromID = Convert.ToInt32(pFeat.get_Value(idxFromPtId));
                int iToID = Convert.ToInt32(pFeat.get_Value(idxToPtId));

                lstRadialPairIdentity.Add(parcelId);
                lstRadialPairIdentity.Add(iCtrPointID);
                lstRadialPairIdentity.Add(iFromID); //from point of radial line as curve start
                lstRadialPairIdentity.Add(iToID); //from point of radial line as curve end
                dict_LinesToRadialLinesPair.Add(pFeat.OID, lstRadialPairIdentity);
              }
            }
          }
        }
        Marshal.ReleaseComObject(pLineRecord);
    
        if (bTrackCancel)
          if (!pTrackCancel.Continue())
            break;

        pLineRecord = pCursor.NextRow();
      }
      Marshal.ReleaseComObject(pCursor);

    }

    private void InverseLineDistancesByElevationLayer(IQueryFilter m_pQF, ITable pLinesTable, bool bFabricIsInGCS, ISpatialReference pMapSpatRef, ISpatialReference pFabricSpatRef,
      double dMetersPerUnit, IFeatureLayer pElevationLayer, int iElevationFieldIndex, double dDifference, double ToMetersHeightConversionFactor, bool bInverseAll,
      ref Dictionary<int, int> dict_LinesToParcel, ref Dictionary<int, double> dict_LinesToInverseDistance, ref Dictionary<int, List<double>> dict_LinesToInverseCircularCurve,
      ref Dictionary<int, List<int>> dict_LinesToRadialLinesPair, ref List<int> lstParcelsWithCurves, ITrackCancel pTrackCancel,
      ref Dictionary<int, double> dict_DiffToReport, ref int LineCount, ref int ExcludedLineCount, ref List<double> lstCombinedScaleFactor)
    {
      bool bTrackCancel = (pTrackCancel != null);

      int idxLineCategory = pLinesTable.FindField("CATEGORY");
      string LineCategoryFldName = pLinesTable.Fields.get_Field(idxLineCategory).Name;

      int idxParcelID = pLinesTable.FindField("PARCELID");
      string ParcelIDFldName = pLinesTable.Fields.get_Field(idxParcelID).Name;

      int idxDistance = pLinesTable.FindField("DISTANCE");
      int idxRadius = pLinesTable.FindField("RADIUS");
      int idxCenterPtId = pLinesTable.FindField("CENTERPOINTID");
      int idxFromPtId = pLinesTable.FindField("FROMPOINTID");
      int idxToPtId = pLinesTable.FindField("TOPOINTID");
      string sElevFldName = pElevationLayer.FeatureClass.Fields.get_Field(iElevationFieldIndex).Name;

      //First find mean elevation from the elevation source to use as a fall-back default value in case other elevation queries fail. 
      double dDefaultElev = 0;
      IQueryFilter pQuFilter = new QueryFilterClass();
      pQuFilter.SubFields = sElevFldName;
      pQuFilter.WhereClause = sElevFldName + " IS NOT NULL";
      ICursor pElevCurs = pElevationLayer.FeatureClass.Search(pQuFilter, false) as ICursor;
      IDataStatistics pStats = new DataStatisticsClass();
      pStats.Field = sElevFldName;
      pStats.Cursor = pElevCurs;
      ESRI.ArcGIS.esriSystem.IStatisticsResults statResults = pStats.Statistics;
      dDefaultElev=statResults.Mean;

      ISpatialFilter pSpatFilter = new SpatialFilterClass();
      pSpatFilter.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;
      pSpatFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      pSpatFilter.SubFields = sElevFldName;

      ICursor pCursor = pLinesTable.Search(m_pQF, false);
      IRow pLineRecord = pCursor.NextRow();
      while (pLineRecord != null)
      {
        LineCount++;
        IFeature pFeat = (IFeature)pLineRecord;
        IGeometry pGeom = pFeat.Shape;
        if (pGeom != null)
        {
          if (!pGeom.IsEmpty)
          {
            //need to project to map's data frame
            if (bFabricIsInGCS)
              pGeom.Project(pMapSpatRef);

            double dAttributeDistance = (double)pLineRecord.get_Value(idxDistance);
            double dAttributeRadius = 0;
            double dGeometryRadius = 0;
            double dGeometryCentralAngle = 0;

            bool bIsCircularArc = false;
            bool bIsMinor = true;
            bool bIsCCW = true;

            object obj = pLineRecord.get_Value(idxRadius);
            if (obj != DBNull.Value)
              dAttributeRadius = Convert.ToDouble(obj);

            int iCtrPointID = -1;
            obj = pLineRecord.get_Value(idxCenterPtId);
            if (obj == DBNull.Value)
              dAttributeRadius = 0;
            else
              iCtrPointID = Convert.ToInt32(obj);

            if (dAttributeRadius != 0)
            {//has a cp ref and a radius attribute
              ISegmentCollection pSegColl = (ISegmentCollection)pGeom;
              ISegment pSeg = pSegColl.get_Segment(0);
              if (pSegColl.SegmentCount == 1 && pSeg is ICircularArc)
              {
                ICircularArc pCirc = pSeg as ICircularArc;
                if (!pCirc.IsLine)
                {
                  bIsCircularArc = true;
                  bIsMinor = pCirc.IsMinor;
                  dGeometryRadius = pCirc.Radius;
                  dGeometryCentralAngle = pCirc.CentralAngle;
                  if (bFabricIsInGCS)
                    dGeometryRadius = dGeometryRadius * dMetersPerUnit;
                  bIsCCW = pCirc.IsCounterClockwise;
                }
              }
            }

            Utilities Utils = new Utilities();

            double dCorrectedDist = dAttributeDistance; //initialize as the same

            IPolyline pPolyline = (IPolyline)pGeom;
            IPoint pPt1 = pPolyline.FromPoint;
            IPoint pPt2 = pPolyline.ToPoint;

            IConstructPoint MidPoint = new PointClass();
            MidPoint.ConstructAlong(pPolyline as ICurve, esriSegmentExtension.esriNoExtension, 0.5, true);

            //now get the ellipsoid height estimate from the source layer
            IBufferConstruction pBuff = new BufferConstructionClass();
            pSpatFilter.Geometry = pBuff.Buffer(MidPoint as IGeometry, 0.5);
            IFeatureCursor pElevCur = pElevationLayer.FeatureClass.Search(pSpatFilter,false);
            IFeature pElevationFeature = pElevCur.NextFeature();
            
            List<double> lstElevations = new List<double>();
            while (pElevationFeature != null)
            {
              double dElev = (pElevationFeature.get_Value(iElevationFieldIndex) is DBNull) ? 0 : (double)pElevationFeature.get_Value(iElevationFieldIndex);

              if (dElev!=0)
                lstElevations.Add((double)dElev);
              Marshal.ReleaseComObject(pElevationFeature);
              pElevationFeature = pElevCur.NextFeature();
            }
            Marshal.ReleaseComObject(pElevCur);

            double dEllipsoidalHeight = dDefaultElev; //initialize the height to the default
            if (lstElevations.Count > 0)
              dEllipsoidalHeight = lstElevations.Average();

            //This function expects metric height, so always convert to meters
            dCorrectedDist = Utils.InverseDistanceByGroundToGrid(pFabricSpatRef, pPt1, pPt2, dEllipsoidalHeight * ToMetersHeightConversionFactor);
            IProximityOperator pProxim = pPt1 as IProximityOperator;

            double dComputedDiff = Math.Abs(dCorrectedDist - dAttributeDistance);
            int parcelId = (int)pFeat.get_Value(idxParcelID);

            if (dComputedDiff > dDifference || bInverseAll)
            {
              bool bExists;
              m_pFIDSetParcels.Find(parcelId, out bExists);
              if (!bExists)
                m_pFIDSetParcels.Add(parcelId);
              if (!dict_LinesToParcel.ContainsKey(pFeat.OID))
              {
                dict_LinesToParcel.Add(pFeat.OID, parcelId);
                dict_LinesToInverseDistance.Add(pFeat.OID, dCorrectedDist);
                lstCombinedScaleFactor.Add(pProxim.ReturnDistance(pPt2) / dCorrectedDist);
                dict_DiffToReport.Add(pFeat.OID, dComputedDiff);
              }
            }
            else
              ExcludedLineCount++;

            if (bIsCircularArc)
            {
              double dRadius = dAttributeRadius;
              if ((Math.Abs(Math.Abs(dAttributeRadius) - Math.Abs(dGeometryRadius)) > dDifference) || bInverseAll)
              {
                double dChordDist = dCorrectedDist;

                //use the corrected chord and keep the geometry central angle to re-compute the radius and arclength
                IConstructCircularArc pConstrArc = new CircularArcClass();
                if (bFabricIsInGCS)
                  pConstrArc.ConstructBearingAngleChord(pPt1, 0, bIsCCW, dGeometryCentralAngle, Math.Abs(dChordDist / dMetersPerUnit));
                else
                  pConstrArc.ConstructBearingAngleChord(pPt1, 0, bIsCCW, dGeometryCentralAngle, dChordDist);

                ICircularArc pCircArc = pConstrArc as ICircularArc;
                double dArcLength = pCircArc.Length;

                if (bIsCCW)
                  dRadius = pCircArc.Radius * -1;
                else
                  dRadius = pCircArc.Radius;

                IAngularConverter pAngCon = new AngularConverterClass();
                pAngCon.SetAngle(Math.Abs(pCircArc.CentralAngle), esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
                double dCentralAngle = pAngCon.GetAngle(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDecimalDegrees);
                //0:radius, 1:chord length, 2:arc length, 3:delta
                List<double> CurveParamList = new List<double>();
                CurveParamList.Add(dChordDist);
                CurveParamList.Add(dRadius);
                CurveParamList.Add(dArcLength);
                CurveParamList.Add(dCentralAngle);
                dict_LinesToInverseCircularCurve.Add(pFeat.OID, CurveParamList);

                List<int> lstRadialPairIdentity = new List<int>();

                if (!lstParcelsWithCurves.Contains(parcelId))
                  lstParcelsWithCurves.Add(parcelId);

                int iFromID = Convert.ToInt32(pFeat.get_Value(idxFromPtId));
                int iToID = Convert.ToInt32(pFeat.get_Value(idxToPtId));

                lstRadialPairIdentity.Add(parcelId);
                lstRadialPairIdentity.Add(iCtrPointID);
                lstRadialPairIdentity.Add(iFromID); //from point of radial line as curve start
                lstRadialPairIdentity.Add(iToID); //from point of radial line as curve end
                dict_LinesToRadialLinesPair.Add(pFeat.OID, lstRadialPairIdentity);

              }
            }
          }
        }
        Marshal.ReleaseComObject(pLineRecord);

        if (bTrackCancel)
          if (!pTrackCancel.Continue())
            break;

        pLineRecord = pCursor.NextRow();
      }
      Marshal.ReleaseComObject(pCursor);

    }
 
  }

}
