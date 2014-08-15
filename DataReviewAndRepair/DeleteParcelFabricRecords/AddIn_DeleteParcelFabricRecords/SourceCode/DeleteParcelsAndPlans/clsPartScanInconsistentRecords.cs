/*
 Copyright 1995-2014 Esri

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
using System.IO;
using System.Text;
using Microsoft.Win32;


namespace DeleteSelectedParcels
{
  public class clsPartialScanInconsistentRecords : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public clsPartialScanInconsistentRecords()
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

      dlgChoosePartialScanType dlgScanOption = new dlgChoosePartialScanType();
      DialogResult dResult = dlgScanOption.ShowDialog();
      if (dResult == DialogResult.Cancel)
        return;

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
      ITable pControl = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);

      #endregion

      #region Check desktop release
      bool bIsDesktop100 = false;
      string sVersion = Application.ProductVersion;
      string[] VersionPart = sVersion.Split('.');
      if (VersionPart[0].Trim() == "10" && VersionPart[1].Trim() == "0")
        bIsDesktop100 = true;
      else
        bIsDesktop100 = false;
      #endregion

      IDataset pDS = (IDataset)pLines;
      IWorkspace pWS = pDS.Workspace;
      bool bIsFileBasedGDB = true;
      bool bIsUnVersioned = true;

      FabricUTILS.GetFabricPlatform(pWS, m_pCadaFab, out bIsFileBasedGDB,
        out bIsUnVersioned);


      string sPref; string sSuff;
      ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
      sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
      sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

      IQueryFilter pQuFilter = new QueryFilterClass();

      int m_CursorCnt = 0;
      string m_sErrMessage = "";
      pProgressorDialogFact = new ProgressDialogFactoryClass();
      pTrackCancel = new CancelTrackerClass();
      pStepProgressor = pProgressorDialogFact.Create(pTrackCancel, pApp.hWnd);
      IProgressDialog2 pProgressorDialog = (IProgressDialog2)pStepProgressor;
      pStepProgressor.MinRange = 1;
      pStepProgressor.StepValue = 1;
      pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

      try
      {
        string sMessage = "";
        bool bCanEdit = FabricUTILS.CanEditFabric(pWS, pLines, out sMessage);
        //Stopped doing a Start and Stop editing to check if it's running in an edit session...was causing memory issues.
        //updated the routine to instead check privileges on one of the fabric tables, and check if local db is being edited.
        if (!bCanEdit)
        {
          DialogResult dRes = MessageBox.Show(sMessage + Environment.NewLine + "You will be able to retrieve the report, but you will"
           + Environment.NewLine + "not be able to delete records." + Environment.NewLine + Environment.NewLine + "Would you like to continue?" + Environment.NewLine +
           "Click Yes to continue. Click No to exit.", 
           "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
          if (dRes == DialogResult.No)
            return;
        }

        if (dlgScanOption.optOrphanPoints.Checked == true)
        {
          #region Orphan Points Scan
          pStepProgressor.Message = "Counting point records...please wait.";

          //do the row count after message, as it takes time
          //int iLineRowCount = pLines.RowCount(null);
          int iPointRowCount = pPoints.RowCount(null);

          pStepProgressor.MaxRange = iPointRowCount * 3;//+iLineRowCount;
          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

          #region Points Table Cursor
          m_sErrMessage = "Searching fabric Point records.";
          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Show();
            pStepProgressor.Message = "Searching " + iPointRowCount.ToString() + " point records...";
          }
          bool bCont = true;

          pQuFilter.SubFields = pPoints.OIDFieldName;
          ICursor pCur = pPoints.Search(pQuFilter, false);
          Dictionary<int, bool> OrphanPointsList_DICT = new Dictionary<int, bool>();
          IRow pRow = pCur.NextRow();
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
            pRow = pCur.NextRow();
          }
          if (pCur != null)
            do { } while (Marshal.FinalReleaseComObject(pCur) > 0);

          if (!bCont)
            return;
          #endregion

          //string sWhereClause = sPref + pLines.OIDFieldName.ToString() + sSuff + "< 5000000";
          string sWhereClause = "";

          if (bShowProgressor)
            pStepProgressor.Message = "Searching lines for point references...";
          //pStepProgressor.Message = "Searching " + iLineRowCount.ToString() + " lines for point references...";

          if (!ProcessOrphanPointsFromLines(pLines, ref OrphanPointsList_DICT, sWhereClause, true, pTrackCancel))
            return;

          m_sErrMessage = "Add the Orphan point Dictionary elements to the orphan points List.";
          List<int> OrphanPointsList = new List<int>();
          List<KeyValuePair<int, bool>> list = OrphanPointsList_DICT.ToList();
          // Loop over list.
          foreach (KeyValuePair<int, bool> pair in list)
          {
            int iVal = pair.Key;
            OrphanPointsList.Add(iVal);
          }
          List<string> m_InClausePointsNotConnectedToLines;
          m_InClausePointsNotConnectedToLines = FabricUTILS.InClauseFromOIDsList(OrphanPointsList, iTokenMax);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          dlgInconsistentRecords dlgReportOrphanPoints = new dlgInconsistentRecords();
          dlgReportOrphanPoints.OrphanPoints = OrphanPointsList;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportOrphanPoints.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportOrphanPoints.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportOrphanPoints.VersionName = FabricUTILS.GetVersionName(pWS);

          string sPointOIDFieldName = sSuff + pPoints.OIDFieldName.ToString() + sPref;

          SetupDialogForReport(dlgReportOrphanPoints, "Orphan Points Report", "Save the report, or click Delete to remove these records:",
            "Points that are not connected to lines", sPointOIDFieldName, m_InClausePointsNotConnectedToLines, OrphanPointsList.Count);

          DialogResult dResult2 = dlgReportOrphanPoints.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;

          #region Delete Orphan Points
          m_sErrMessage = "Deleting orphan points";

          if (OrphanPointsList.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = OrphanPointsList.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClausePointsNotConnectedToLines[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
              {
                pProgressorDialog.ShowDialog();
                pStepProgressor2.Show();
                pStepProgressor2.Message = "Deleting points not connected to lines (orphan points)...";
              }

              if (!FabricUTILS.DeleteByInClause(pWS, pPoints, pPoints.Fields.get_Field(pPoints.FindField(pPoints.OIDFieldName)),
                m_InClausePointsNotConnectedToLines, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              //need to also take care of control points that have a reference to any of the deleted Orphan points
              ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
              int idxNameFldOnControl = pControl.FindField("POINTID");
              string ControlNameFldName = pControl.Fields.get_Field(idxNameFldOnControl).Name;

              if (bShowProgressor)
                pStepProgressor2.Message = "Resetting control references to points...";

              int iCnt = m_InClausePointsNotConnectedToLines.Count - 1;
              for (int z = 0; z <= iCnt; z++)
              {
                if ((m_InClausePointsNotConnectedToLines[z].Trim() == ""))
                  break;
                //cleanup associated control points, and associations where underlying points were deleted 
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
              FabricUTILS.StopEditing(pWS);

            }
          }
          #endregion

          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }
          #endregion
        }
        else if (dlgScanOption.optOrphanLines.Checked == true)
        {

          #region Orphan Lines Scan
          pStepProgressor.Message = "Counting parcel records...please wait.";

          //do the row count after message, as it takes time
          //int iLineRowCount = pLines.RowCount(null);
          int iParcelRowCount = pParcels.RowCount(null);

          pStepProgressor.MaxRange = iParcelRowCount * 6;//+iLineRowCount;
          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

          #region Parcel Table Cursor
          m_sErrMessage = "Searching fabric Parcel records.";
          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Show();
            pStepProgressor.Message = "Searching parcel records...";
          }
          bool bCont = true;
          pQuFilter.SubFields = pParcels.OIDFieldName;
          ICursor pCur = pParcels.Search(pQuFilter, false);
          Dictionary<int, bool> OrphanLinesList_DICT = new Dictionary<int, bool>();
          List<int> inParcelList = new List<int>();
          IRow pRow = pCur.NextRow();
          while (pRow != null)
          {
            m_CursorCnt++;
            int iRow = pRow.OID;
            inParcelList.Add(iRow);
            Marshal.ReleaseComObject(pRow);
            //after garbage collection, and before gettng the next row,
            //check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
              bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
            pRow = pCur.NextRow();
          }
          if (pCur != null)
            do { } while (Marshal.FinalReleaseComObject(pCur) > 0);

          if (!bCont)
            return;
          #endregion
          //now search the lines for parcelid references
          string sWhereClause = "";

          if (bShowProgressor)
            pStepProgressor.Message = "Searching lines for parcel references...";

          m_sErrMessage = "Add the Orphan line Dictionary elements to the orphan lines List.";
          List<int> OrphanLinesList = new List<int>();

          if (!ProcessOrphanLines(pLines, ref inParcelList, ref OrphanLinesList, sWhereClause, true, pTrackCancel))
            return;

          #region setup dialog
          List<string> m_InClauseLinesNotConnectedToParcels;
          m_InClauseLinesNotConnectedToParcels = FabricUTILS.InClauseFromOIDsList(OrphanLinesList, iTokenMax);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          dlgInconsistentRecords dlgReportOrphanLines = new dlgInconsistentRecords();
          dlgReportOrphanLines.OrphanLines = OrphanLinesList;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportOrphanLines.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportOrphanLines.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportOrphanLines.VersionName = FabricUTILS.GetVersionName(pWS);

          string sOIDFieldName = sPref + pLines.OIDFieldName.ToString() + sSuff;

          SetupDialogForReport(dlgReportOrphanLines, "Orphan Lines", "Save the report, or click Delete to remove these records:",
            "Lines that do not belong to parcels", sOIDFieldName, m_InClauseLinesNotConnectedToParcels, OrphanLinesList.Count);

          DialogResult dResult2 = dlgReportOrphanLines.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;
          #endregion

          #endregion

          #region Delete Orphan lines
          m_sErrMessage = "Deleting orphan lines";

          if (OrphanLinesList.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = OrphanLinesList.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClauseLinesNotConnectedToParcels[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
                pStepProgressor2.Message = "Deleting lines not connected to parcels (orphan lines)...";
              if (!FabricUTILS.DeleteByInClause(pWS, pLines, pLines.Fields.get_Field(pLines.FindField(pLines.OIDFieldName)),
                m_InClauseLinesNotConnectedToParcels, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              FabricUTILS.StopEditing(pWS);

            }
          }
          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }

          #endregion

        }
        else if (dlgScanOption.optSameFromTo.Checked == true)
        {

          #region Same To and From Points Scan
          pStepProgressor.Message = "Counting line records...please wait.";
          //do the row count after message, as it takes time
          int iLineRowCount = pLines.RowCount(null);

          pStepProgressor.MaxRange = iLineRowCount;
          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);
          if (bShowProgressor)
            pStepProgressor.Message = "Searching lines for records with identical To and From points...";

          m_sErrMessage = "Looking for same To and From points on lines.";
          List<int> SameFromAndToLinesList = new List<int>();
          string sWhereClause = "";

          //The following is FOR 10.0 Desktop Clients ONLY:
          if (bIsDesktop100)
          {
            if (!ProcessLinesWithSameFromToNoGeomTest(pLines, ref SameFromAndToLinesList, sWhereClause, true, pTrackCancel))
              return;
          }
          else
          {
            if (!ProcessLinesWithSameFromTo(pLines, ref SameFromAndToLinesList, sWhereClause, true, pTrackCancel))
              return;
          }

          #region setup dialog
          m_sErrMessage = "Getting In clause for Same From and To Points.";

          List<string> m_InClauseLinesWithSameFromTo;
          m_InClauseLinesWithSameFromTo = FabricUTILS.InClauseFromOIDsList(SameFromAndToLinesList, iTokenMax);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          dlgInconsistentRecords dlgReportLinesWithSameFromTo= new dlgInconsistentRecords();
          dlgReportLinesWithSameFromTo.LinesWithSameFromTo = SameFromAndToLinesList;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportLinesWithSameFromTo.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportLinesWithSameFromTo.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportLinesWithSameFromTo.VersionName = FabricUTILS.GetVersionName(pWS);

          string sOIDFieldName = sPref + pLines.OIDFieldName.ToString() + sSuff;

          SetupDialogForReport(dlgReportLinesWithSameFromTo, "Lines with same To and From", "Save the report, or click Delete to remove these records:",
            "Lines with same To and From points", sOIDFieldName, m_InClauseLinesWithSameFromTo, SameFromAndToLinesList.Count);

          DialogResult dResult2 = dlgReportLinesWithSameFromTo.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;
          #endregion

          #endregion

          #region Delete Lines with same To and From
          m_sErrMessage = "Deleting lines with same From and To";

          if (SameFromAndToLinesList.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = SameFromAndToLinesList.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClauseLinesWithSameFromTo[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
                pStepProgressor2.Message = "Deleting lines with same From and To points...";
              if (!FabricUTILS.DeleteByInClause(pWS, pLines, pLines.Fields.get_Field(pLines.FindField(pLines.OIDFieldName)),
                m_InClauseLinesWithSameFromTo, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              FabricUTILS.StopEditing(pWS);
            }
          }
          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }

          #endregion

        }
        else if (dlgScanOption.optOrphanLinePoints.Checked == true)
        {

          #region LinePoints Scan

          pStepProgressor.Message = "Counting line point records...please wait.";

          //do the row count after message, as it takes time

          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();
          int iLinePointRowCount = pLinePts.RowCount(null);
          int iLineRowCount = pLines.RowCount(null);
          pStepProgressor.MaxRange = iLinePointRowCount + iLineRowCount;

          #region Re-building the point list- Points Table Cursor
          m_sErrMessage = "Re-building fabric Point list.";
          pQuFilter.SubFields = pPoints.OIDFieldName;
          pQuFilter.WhereClause = "";
          ICursor pCur = pPoints.Search(pQuFilter, false);
          List<int> PointList = new List<int>();
          IRow pRow = pCur.NextRow();
          while (pRow != null)
          {
            int iRow = pRow.OID;
            PointList.Add(iRow);
            Marshal.ReleaseComObject(pRow);
            pRow = pCur.NextRow();
          }
          if (pCur != null)
            do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
          PointList.Sort();
          #endregion

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

          #region LinePoint Table Cursor
          m_sErrMessage = "Searching line point records.";
          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Show();
            pStepProgressor.Message = "Searching line point records...";
          }
          bool bCont = true;

          int iLinePtPointIdIdx = pLinePts.FindField("LINEPOINTID");
          int iLinePtFromPtIdIdx = pLinePts.FindField("FROMPOINTID");
          int iLinePtToPtIdIdx = pLinePts.FindField("TOPOINTID");

          string LinePointPtIdFldName = pLinePts.Fields.get_Field(iLinePtPointIdIdx).Name;
          string LinePointFromPtIdFldName = pLinePts.Fields.get_Field(iLinePtFromPtIdIdx).Name;
          string LinePointToPtIdFldName = pLinePts.Fields.get_Field(iLinePtToPtIdIdx).Name;

          pQuFilter.SubFields = pLinePts.OIDFieldName + "," + LinePointPtIdFldName + "," + LinePointFromPtIdFldName + "," + LinePointToPtIdFldName;
          pQuFilter.WhereClause = "";
          pCur = pLinePts.Search(pQuFilter, false);

          Dictionary<int, string> inLinePoint_DICT = new Dictionary<int, string>(iLinePointRowCount);
          m_sErrMessage = "";
          List<int> OrphanLinePointList = new List<int>();
          pRow = pCur.NextRow();
          while (pRow != null)
          {
            m_CursorCnt++;
            int iRow = pRow.OID;
            object obj = pRow.get_Value(iLinePtFromPtIdIdx);
            string sLPFrom = "";
            if (obj != DBNull.Value)
              sLPFrom = Convert.ToString(obj);

            obj = pRow.get_Value(iLinePtToPtIdIdx);
            string sLPTo = "";
            if (obj != DBNull.Value)
              sLPTo = Convert.ToString(obj);

            obj = pRow.get_Value(iLinePtPointIdIdx);
            int iLinePointID = -1;
            string sLPPtId = "";
            if (obj != DBNull.Value)
            {
              iLinePointID = (int)obj;
              sLPPtId = Convert.ToString(iLinePointID);
            }
            if (sLPFrom.Trim() == sLPTo.Trim() || sLPFrom.Trim() == sLPPtId.Trim() || sLPTo.Trim() == sLPPtId.Trim())
            {
              OrphanLinePointList.Add(iRow); //add corrupted line-point to the list
              pRow = pCur.NextRow();//and don't bother adding it to the dictionary
              continue;
            }
            else if (sLPFrom.Trim() == "" || sLPTo.Trim() == "" || sLPPtId.Trim() == "")
            {
              OrphanLinePointList.Add(iRow); //add corrupted line-point to the list 
              pRow = pCur.NextRow(); //and don't bother adding it to the dictionary
              continue;
            }
            else if (PointList.BinarySearch(iLinePointID) < 0)
            {
              OrphanLinePointList.Add(iRow);//the from and to's are handled off the lines
              pRow = pCur.NextRow(); //and don't bother adding it to the dictionary
              continue;
            }

            inLinePoint_DICT.Add(iRow, sLPFrom + ":" + sLPTo);
            Marshal.ReleaseComObject(pRow);
            //after garbage collection, and before gettng the next row,
            //check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
              bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
            pRow = pCur.NextRow();
          }
          if (pCur != null)
            do { } while (Marshal.FinalReleaseComObject(pCur) > 0);

          if (!bCont)
            return;
          #endregion
          //now search the line points for line references
          string sWhereClause = "";

          if (bShowProgressor)
            pStepProgressor.Message = "Searching for invalid line point references...";

          if (!ProcessOrphanLinePoints(pLines, ref inLinePoint_DICT, ref OrphanLinePointList, sWhereClause, true, pTrackCancel))
            return;

          #region setup dialog
          List<string> m_InClauseOrphanLinePoints;
          m_InClauseOrphanLinePoints = FabricUTILS.InClauseFromOIDsList(OrphanLinePointList, iTokenMax);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          dlgInconsistentRecords dlgReportOrphanLinePts = new dlgInconsistentRecords();
          dlgReportOrphanLinePts.OrphanLinePoints = OrphanLinePointList;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportOrphanLinePts.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportOrphanLinePts.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportOrphanLinePts.VersionName = FabricUTILS.GetVersionName(pWS);

          string sOIDFieldName = sPref + pLinePts.OIDFieldName.ToString() + sSuff;

          SetupDialogForReport(dlgReportOrphanLinePts, "Invalid Line points", "Save the report, or click Delete to remove these records:",
            "Line points with invalid point references", sOIDFieldName, m_InClauseOrphanLinePoints, OrphanLinePointList.Count);

          DialogResult dResult2 = dlgReportOrphanLinePts.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;
          #endregion

          #endregion

          #region Delete Line points
          m_sErrMessage = "Deleting line-points with invalid references";

          if (OrphanLinePointList.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = OrphanLinePointList.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClauseOrphanLinePoints[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
                pStepProgressor2.Message = "Deleting linepoints with invalid references...";
              if (!FabricUTILS.DeleteByInClause(pWS, pLinePts, pLinePts.Fields.get_Field(pLinePts.FindField(pLinePts.OIDFieldName)),
                m_InClauseOrphanLinePoints, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              FabricUTILS.StopEditing(pWS);
            }
          }
          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }

          #endregion

        }
        else if (dlgScanOption.optLinePointsDisplaced.Checked == true)
        {

          #region Displaced LinePoints Scan

          pStepProgressor.Message = "Counting line point records...please wait.";

          //do the row count after message, as it takes time

          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();
          int iLinePointRowCount = pLinePts.RowCount(null);
          double dMaxOffset=0;

          pStepProgressor.MaxRange = iLinePointRowCount;

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

          #region LinePoint Table Cursor
          m_sErrMessage = "Searching line point records.";
          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Show();
            pStepProgressor.Message = "Searching line point records...";
          }
          bool bCont = true;

          int iLinePtPointIdIdx = pLinePts.FindField("LINEPOINTID");
          int iLinePtFromIdIdx = pLinePts.FindField("FROMPOINTID");
          int iLinePtToIdIdx = pLinePts.FindField("TOPOINTID");

          int iLineOffsetIdx = pLinePts.FindField("LINEOFFSET");
          bool bHasOffset = (iLineOffsetIdx > -1);

          if (bHasOffset)
          {
            IFields pFlds = pLinePts.Fields;
            IField pFld = pFlds.get_Field(iLineOffsetIdx);
            bHasOffset = (pFld.Type == esriFieldType.esriFieldTypeDouble);
          }

          if (bHasOffset)
            bHasOffset = bCanEdit; //pretend there is no offset field if you are not able to edit

          IFeatureClass LP_FC = (IFeatureClass)pLinePts;
          IFeatureClass Pt_FC = (IFeatureClass)pPoints;

          string LinePointPtIdFldName = pLinePts.Fields.get_Field(iLinePtPointIdIdx).Name;
          pQuFilter.WhereClause = "";
          IFeatureCursor pFeatCur = LP_FC.Search(pQuFilter, false);

          List<int> DisplacedLinePointList = new List<int>();
          Dictionary<int, double> LinePointOffset_Lookup = new Dictionary<int, double>();
          IFeature pFeat = pFeatCur.NextFeature();
          double dSearch = 0.1;
          double dFabricXYTolerance = GetMaxShiftThreshold(m_pCadaFab);
          string sUnit = "";
  
          ISpatialFilter pSpatFilt = new SpatialFilterClass();
          if (pFeat != null)
          {
            IGeoDataset pGeoDS = (IGeoDataset)pFeat.Class;
            ISpatialReference pSR = pGeoDS.SpatialReference;
            IProjectedCoordinateSystem pPCS = (IProjectedCoordinateSystem)pSR;
            double dMetersPerUnit=pPCS.CoordinateUnit.MetersPerUnit;
            dFabricXYTolerance = ConvertDistanceFromMetersToFabricUnits(dFabricXYTolerance, m_pCadaFab, out sUnit, out dMetersPerUnit);
            ISpatialReferenceTolerance pSRTol = (ISpatialReferenceTolerance)pGeoDS.SpatialReference;
            if (pSRTol.XYToleranceValid == esriSRToleranceEnum.esriSRToleranceOK)
              dSearch = pSRTol.XYTolerance * Math.Sqrt(2) * 10;//root 2...x10 to account for sde oracle search error from small geometry
            pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
          }
          if (bShowProgressor)
            pStepProgressor.Message = "Searching for line points with displaced geometry...";

          while (pFeat != null)
          {
            m_CursorCnt++;
            int iFeatRow = pFeat.OID;
            m_sErrMessage = "Searching for displaced line points, OID: " + iFeatRow.ToString();
            object obj = pFeat.get_Value(iLinePtPointIdIdx);
            int iLinePointID = -1;
            if (obj != DBNull.Value)
              iLinePointID = (int)obj;

            //Now look for fabric points within XY tolerance of the line-point geometry
            IGeometry pGeom = pFeat.ShapeCopy;
            ITopologicalOperator topologicalOperator = (ITopologicalOperator)pGeom;
            IPolygon pBufferedLinePointGeometry = topologicalOperator.Buffer(dSearch) as IPolygon;
            if (pBufferedLinePointGeometry != null)
              pSpatFilt.Geometry = pBufferedLinePointGeometry;
            else
              continue;
            pSpatFilt.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;
            pSpatFilt.WhereClause = pPoints.OIDFieldName + " = " + iLinePointID.ToString();
            IFeatureCursor pPtCursor = Pt_FC.Search(pSpatFilt, false);
            IFeature pFabPoint = pPtCursor.NextFeature();
            if (pFabPoint == null) //if the fabric point is not found it's displaced
              DisplacedLinePointList.Add(iFeatRow);
            else
              Marshal.ReleaseComObject(pFabPoint);
            Marshal.ReleaseComObject(pPtCursor);

            //if there is a LineOffset double field, compute and write an offset
            if (bHasOffset)
            {
              pQuFilter.WhereClause = Pt_FC.OIDFieldName + " IN (" + pFeat.get_Value(iLinePtFromIdIdx).ToString() + "," + pFeat.get_Value(iLinePtToIdIdx).ToString() + ")";
              IFeatureCursor pFeatCur2 = Pt_FC.Search(pQuFilter, false);
              IFeature pFromPt = pFeatCur2.NextFeature();
              IFeature pToPt = pFeatCur2.NextFeature();
              if (pFromPt != null && pToPt != null)
              {
                IPoint pLinePointGeom = (IPoint)pFeat.ShapeCopy;
                IPoint pFromPtGeom = (IPoint)pFromPt.ShapeCopy;
                IPoint pToPtGeom = (IPoint)pToPt.ShapeCopy;

                if (pFromPtGeom.IsEmpty || pToPtGeom.IsEmpty)
                {
                  LinePointOffset_Lookup.Add(iFeatRow, -1);  //-1 indicates and empty from or to point geometry
                }
                else
                {
                  ILine pLine = new LineClass();
                  pLine.PutCoords(pFromPtGeom, pToPtGeom);
                  double dDistAlong = 0;
                  double dDistPerpendicular = 0;
                  bool bRightSide = false;
                  IPoint pOutPoint = null;
                  pLine.QueryPointAndDistance(esriSegmentExtension.esriExtendTangents, pLinePointGeom, false, pOutPoint, ref dDistAlong, ref dDistPerpendicular, ref bRightSide);
                  if (dDistPerpendicular > (dFabricXYTolerance))
                  {
                    LinePointOffset_Lookup.Add(iFeatRow, dDistPerpendicular);  //Add to the offset dictionary
                    dMaxOffset = dDistPerpendicular > dMaxOffset ? dDistPerpendicular : dMaxOffset; //update the max value
                  }
                }
                Marshal.ReleaseComObject(pFromPt);
                Marshal.ReleaseComObject(pToPt);
              }

              Marshal.FinalReleaseComObject(pFeatCur2);
            }

            Marshal.ReleaseComObject(pFeat);
            //after garbage collection, and before gettng the next row,
            //check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
              bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
            pFeat = pFeatCur.NextFeature();
          }
          if (pFeatCur != null)
            do { } while (Marshal.FinalReleaseComObject(pFeatCur) > 0);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          if (!bCont)
            return;

          #endregion

          #region Update the Offset field
          if (LinePointOffset_Lookup.Count > 0)
          {
            IProgressDialogFactory pProgressorDialogFact1 = new ProgressDialogFactoryClass();
            ITrackCancel pTrackCancel1 = new CancelTrackerClass();
            IStepProgressor pStepProgressor1 = pProgressorDialogFact.Create(pTrackCancel1, pApp.hWnd);
            IProgressDialog2 pProgressorDialog1 = (IProgressDialog2)pStepProgressor1;
            pStepProgressor1.MinRange = 1;
            pStepProgressor1.MaxRange = LinePointOffset_Lookup.Count;
            pStepProgressor1.StepValue = 1;
            pProgressorDialog1.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

            bShowProgressor = (pStepProgressor1 != null && pTrackCancel1 != null);

            try
            {
              m_sErrMessage = "Add the line offset Dictionary elements to the List.";
              List<int> LinePointOffsets = new List<int>();
              List<KeyValuePair<int, double>> list2 = LinePointOffset_Lookup.ToList();
              foreach (KeyValuePair<int, double> pair in list2)
              {
                int iVal = pair.Key;
                LinePointOffsets.Add(iVal);
              }

              List<string> m_InClauseLinePointOffsets;
              m_InClauseLinePointOffsets = FabricUTILS.InClauseFromOIDsList(LinePointOffsets, iTokenMax);

              //update line point offset values
              ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
              ITable LinePointsTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLinePoints);
              int idxOffsetFld = LinePointsTable.FindField("LINEOFFSET");
              string OffsetFldName = LinePointsTable.Fields.get_Field(idxOffsetFld).Name;
              
              if (bShowProgressor)
              {
                pProgressorDialog1.ShowDialog();
                pStepProgressor1.Show();
                pStepProgressor1.Message = "Updating line offset field values on line points...";
              }

              //Start editing
              bool bStartEditingSuccess = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
              if (!bStartEditingSuccess)
                return;

              int iCnt = m_InClauseLinePointOffsets.Count - 1;
              for (int z = 0; z <= iCnt; z++)
              {
                if ((m_InClauseLinePointOffsets[z].Trim() == ""))
                  break;
                //cleanup associated control points, and associations where underlying points were deleted 
                pQuFilter.SubFields = "";
                pQuFilter.WhereClause = LinePointsTable.OIDFieldName + " IN (" + m_InClauseLinePointOffsets[z] + ")";
                pSchemaEd.ReleaseReadOnlyFields(LinePointsTable, esriCadastralFabricTable.esriCFTLinePoints); //release safety-catch
                if (!ProcessUpdateLinePointOffset(LinePointsTable, LinePointOffset_Lookup, pQuFilter, bIsUnVersioned, bShowProgressor, pTrackCancel1))
                {
                  pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLinePoints);//set safety back on
                  FabricUTILS.AbortEditing(pWS);
                  return;
                }
              }
              pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLinePoints);//set safety back on
              FabricUTILS.StopEditing(pWS);
            }
            finally
            {
              if (pProgressorDialog1 != null)
                pProgressorDialog1.HideDialog();

              if (pStepProgressor1 != null)
                pStepProgressor1.Hide();
            }
          }
          #endregion

          #region setup dialog
          List<string> m_InClauseDisplacedLinePoints;
          m_InClauseDisplacedLinePoints = FabricUTILS.InClauseFromOIDsList(DisplacedLinePointList, iTokenMax);

          dlgInconsistentRecords dlgReportDisplacedLinePts = new dlgInconsistentRecords();
          dlgReportDisplacedLinePts.DisplacedLinePoints = DisplacedLinePointList;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportDisplacedLinePts.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportDisplacedLinePts.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportDisplacedLinePts.VersionName = FabricUTILS.GetVersionName(pWS);

          string sOIDFieldName = sPref + pLinePts.OIDFieldName.ToString() + sSuff;

          SetupDialogForReport(dlgReportDisplacedLinePts, "Displaced Line points", "Save the report, or click Delete to remove these records:",
            "Line points with displaced geometry", sOIDFieldName, m_InClauseDisplacedLinePoints, DisplacedLinePointList.Count);

          string sNote = "";
          if (bHasOffset)
            sNote = "Offsets greater than the fabric XY tolerance were written to the 'LINEOFFSET' field." + Environment.NewLine +
          "The largest offset is " + dMaxOffset.ToString("F3") + " (" + sUnit.ToLower() + ")";
          dlgReportDisplacedLinePts.txtInClauseReport.Text += sNote;

          DialogResult dResult2 = dlgReportDisplacedLinePts.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;
          #endregion

          #endregion

          #region Delete displaced Line points
          m_sErrMessage = "Deleting line-points with displaced geometry";

          if (DisplacedLinePointList.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess3 = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess3)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = DisplacedLinePointList.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClauseDisplacedLinePoints[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
                pStepProgressor2.Message = "Deleting linepoints with displaced geometry...";
              if (!FabricUTILS.DeleteByInClause(pWS, pLinePts, pLinePts.Fields.get_Field(pLinePts.FindField(pLinePts.OIDFieldName)),
                m_InClauseDisplacedLinePoints, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              FabricUTILS.StopEditing(pWS);
            }
          }
          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }

          #endregion

        }
        else if (dlgScanOption.optNoLineParcels.Checked == true)
        {

          #region Parcels with no lines Scan
          pStepProgressor.Message = "Counting parcel records...please wait.";

          //do the row count after message, as it takes time
          //int iLineRowCount = pLines.RowCount(null);
          int iParcelRowCount = pParcels.RowCount(null);

          pStepProgressor.MaxRange = iParcelRowCount * 6;//+iLineRowCount;
          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

          #region Parcel Table Cursor
          m_sErrMessage = "Searching fabric Parcel records.";
          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Show();
            pStepProgressor.Message = "Searching parcel records...";
          }
          bool bCont = true;
          int idxPlanIDFld = pParcels.FindField("PLANID");
          string PlanIDFldName = pParcels.Fields.get_Field(idxPlanIDFld).Name;
          int idxConstructionFldName = pParcels.FindField("CONSTRUCTION");
          string ConstructionFldName = pParcels.Fields.get_Field(idxConstructionFldName).Name;

          pQuFilter.SubFields = pParcels.OIDFieldName + "," + PlanIDFldName + "," + ConstructionFldName;
          m_sErrMessage += pQuFilter.SubFields;

          pQuFilter.WhereClause = "NOT(" + PlanIDFldName + "=-1 AND " + ConstructionFldName + "=1)";
          ICursor pCur = pParcels.Search(pQuFilter, false);
          Dictionary<int, bool> NoLinesList_DICT = new Dictionary<int, bool>();
          IRow pRow = pCur.NextRow();
          while (pRow != null)
          {
            m_CursorCnt++;
            int iRow = pRow.OID;

            bool bIsConstruction = false;
            object obj = pRow.get_Value(idxConstructionFldName);
            if (obj != DBNull.Value)
            {
              int iVal = (int)obj;
              bIsConstruction = (iVal == 1);
            }

            NoLinesList_DICT.Add(iRow, bIsConstruction);
            Marshal.ReleaseComObject(pRow);
            //after garbage collection, and before gettng the next row,
            //check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
              bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
            pRow = pCur.NextRow();
          }
          if (pCur != null)
            do { } while (Marshal.FinalReleaseComObject(pCur) > 0);

          if (!bCont)
            return;
          #endregion
          //now search the lines for parcelid references
          string sWhereClause = "";

          if (bShowProgressor)
            pStepProgressor.Message = "Searching lines for parcel references...";

          if (!ProcessNoLinesInParcels(pLines, ref NoLinesList_DICT, sWhereClause, true, pTrackCancel))
            return;

          #region setup dialog
          m_sErrMessage = "Add parcels with no lines Dictionary elements to the no lines List.";
          List<int> NoParcelLinesList = new List<int>();
          List<KeyValuePair<int, bool>> list = NoLinesList_DICT.ToList();
          // Loop over list.
          foreach (KeyValuePair<int, bool> pair in list)
          {
            int iVal = pair.Key;
            if (!NoLinesList_DICT[iVal]) //if it's not a construction
              NoParcelLinesList.Add(iVal);
          }

          List<string> m_InClauseParcelsWithNoLines;
          m_InClauseParcelsWithNoLines = FabricUTILS.InClauseFromOIDsList(NoParcelLinesList, iTokenMax);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          dlgInconsistentRecords dlgReportNoLinesOnParcels = new dlgInconsistentRecords();
          dlgReportNoLinesOnParcels.ParcelsWithNoLines = NoParcelLinesList;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportNoLinesOnParcels.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportNoLinesOnParcels.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportNoLinesOnParcels.VersionName = FabricUTILS.GetVersionName(pWS);

          string sOIDFieldName = sPref + pParcels.OIDFieldName.ToString() + sSuff;

          SetupDialogForReport(dlgReportNoLinesOnParcels, "Parcels with No Lines", "Save the report, or click Delete to remove these records:",
            "Parcels that have no lines", sOIDFieldName, m_InClauseParcelsWithNoLines, NoParcelLinesList.Count);

          DialogResult dResult2 = dlgReportNoLinesOnParcels.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;
          #endregion

          #endregion

          #region Delete Parcels with no lines
          m_sErrMessage = "Deleting parcels with no lines";

          if (NoParcelLinesList.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess2 = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess2)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = NoParcelLinesList.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClauseParcelsWithNoLines[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
                pStepProgressor2.Message = "Deleting parcels with no lines...";
              if (!FabricUTILS.DeleteByInClause(pWS, pParcels, pParcels.Fields.get_Field(pParcels.FindField(pParcels.OIDFieldName)),
                m_InClauseParcelsWithNoLines, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              FabricUTILS.StopEditing(pWS);
            }
          }
          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }

          #endregion
        
        }
        else if (dlgScanOption.optInvalidVectors.Checked == true)
        {

          #region Invalid vectors Scan

          pStepProgressor.Message = "Counting vector records...please wait.";

          //do the row count after message, as it takes time
          int iVectorRowCount = pVectors.RowCount(null);
          List<int> InvalidVectors = new List<int>();
          pStepProgressor.MaxRange = iVectorRowCount;
          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

          #region Vector Table Cursor
          m_sErrMessage = "Searching vector records.";
          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Show();
            pStepProgressor.Message = "Searching " + iVectorRowCount.ToString() + " vector records...";
          }

          bool bCont = true;
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
          pQuFilter.WhereClause = "";
          ICursor pCur = pVectors.Search(pQuFilter, false);
          IRow pRow = pCur.NextRow();
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
            pRow = pCur.NextRow();
          }

          if (pCur != null)
            do { } while (Marshal.FinalReleaseComObject(pCur) > 0);

          if (!bCont)
            return;

          //now remove repeats
          List<int> temp5 = InvalidVectors.Distinct().ToList();
          InvalidVectors = temp5.ToList();
          temp5.Clear();
          temp5 = null;

          #endregion

          #region setup dialog
          List<string> m_InClauseInvalidVectors;
          m_InClauseInvalidVectors = FabricUTILS.InClauseFromOIDsList(InvalidVectors, iTokenMax);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          dlgInconsistentRecords dlgReportInvalidVectors = new dlgInconsistentRecords();
          dlgReportInvalidVectors.InvalidVectors = InvalidVectors;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportInvalidVectors.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportInvalidVectors.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportInvalidVectors.VersionName = FabricUTILS.GetVersionName(pWS);

          string sOIDFieldName = sPref + pVectors.OIDFieldName.ToString() + sSuff;

          SetupDialogForReport(dlgReportInvalidVectors, "Invalid Vectors", "Save the report, or click Delete to remove these records:",
            "Invalid vectors", sOIDFieldName, m_InClauseInvalidVectors, InvalidVectors.Count);

          DialogResult dResult2 = dlgReportInvalidVectors.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;
          #endregion

          #endregion

          #region Delete Invalid Vectors
          m_sErrMessage = "Deleting invalid vectors";

          if (InvalidVectors.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = InvalidVectors.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClauseInvalidVectors[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
                pStepProgressor2.Message = "Deleting invalid vectors...";
              if (!FabricUTILS.DeleteByInClause(pWS, pVectors, pVectors.Fields.get_Field(pVectors.FindField(pVectors.OIDFieldName)),
                m_InClauseInvalidVectors, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              FabricUTILS.StopEditing(pWS);
            }
          }
          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }

          #endregion
        
        }
        else if (dlgScanOption.optInvalidFCAssocs.Checked == true)
        {

          #region Invalid feature class associations Scan

          pStepProgressor.Message = "Counting adjustment level records...please wait.";

          //do the row count after message, as it takes time
          int iAdjustmentLvlRowCount = pAdjLevels.RowCount(null);

          pStepProgressor.MaxRange = iAdjustmentLvlRowCount;
          m_pStartTime = new TimeClass();
          m_pStartTime.SetFromCurrentLocalTime();

          bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

          #region AdjustmentLevel Table Cursor
          List<int> pLevels = new List<int>();
          Hashtable pHashLevels = new Hashtable();

          m_sErrMessage = "Adjustment records query: Search";
          if (bShowProgressor)
            pStepProgressor.Message = "Searching " + iAdjustmentLvlRowCount.ToString() + " adjustment level records...";
          bool bCont = true;

          int iFC = pAdjLevels.FindField("FeatureClassID");
          string FeatClassIDFldName = pAdjLevels.Fields.get_Field(iFC).Name;

          pQuFilter.SubFields = pAdjLevels.OIDFieldName + "," + FeatClassIDFldName;
          ICursor pCur = pAdjLevels.Search(pQuFilter, false);
          IRow pRow = pCur.NextRow();

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
            pRow = pCur.NextRow();
          }
          if (pCur != null)
            do { } while (Marshal.FinalReleaseComObject(pCur) > 0);

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

          #region setup dialog
          List<string> m_InClauseInvalidFeatClassAssocs;
          m_InClauseInvalidFeatClassAssocs = FabricUTILS.InClauseFromOIDsList(pLevelInvalidLevels, iTokenMax);

          if (pProgressorDialog != null)
            pProgressorDialog.HideDialog();

          if (pStepProgressor != null)
            pStepProgressor.Hide();

          dlgInconsistentRecords dlgReportInvalidFCAssocs = new dlgInconsistentRecords();
          dlgReportInvalidFCAssocs.InvalidFeatureClassAssociations = pLevelInvalidLevels;
          m_pEndTime = new TimeClass();
          m_pEndTime.SetFromCurrentLocalTime();
          ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);
          dlgReportInvalidFCAssocs.ProcessTime = HowLong.Hours.ToString("00") + "h "
                    + HowLong.Minutes.ToString("00") + "m "
                    + HowLong.Seconds.ToString("00") + "s";

          string sFabricName = pWS.PathName + "\\" + pDS.Name;
          string sFabricName2 = sFabricName.ToLower();
          dlgReportInvalidFCAssocs.FabricName = sFabricName.Remove(sFabricName2.LastIndexOf("_lines"));
          dlgReportInvalidFCAssocs.VersionName = FabricUTILS.GetVersionName(pWS);

          string sOIDFieldName = sPref + pAdjLevels.OIDFieldName.ToString() + sSuff;

          SetupDialogForReport(dlgReportInvalidFCAssocs, "Invalid feature class associations", "Save the report, or click Delete to remove these records:",
            "Invalid feature class associations", sOIDFieldName, m_InClauseInvalidFeatClassAssocs, pLevelInvalidLevels.Count);

          DialogResult dResult2 = dlgReportInvalidFCAssocs.ShowDialog();
          if (dResult2 == DialogResult.Cancel)
            return;
          #endregion

          #endregion

          #region Delete Invalid feature class associations
          m_sErrMessage = "Deleting invalid feature class associations";

          if (pLevelInvalidLevels.Count == 0)
            return;

          //Start editing
          bool bStartEditingSuccess = FabricUTILS.StartEditing(pWS, bIsUnVersioned);
          if (!bStartEditingSuccess)
            return;

          IProgressDialogFactory pProgressorDialogFact2 = new ProgressDialogFactoryClass();
          ITrackCancel pTrackCancel2 = new CancelTrackerClass();
          IStepProgressor pStepProgressor2 = pProgressorDialogFact.Create(pTrackCancel2, pApp.hWnd);
          IProgressDialog2 pProgressorDialog2 = (IProgressDialog2)pStepProgressor2;
          pStepProgressor2.MinRange = 1;
          pStepProgressor2.MaxRange = pLevelInvalidLevels.Count;
          pStepProgressor2.StepValue = 1;
          pProgressorDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

          bShowProgressor = (pStepProgressor2 != null && pTrackCancel2 != null);

          try
          {
            if (m_InClauseInvalidFeatClassAssocs[0].Trim() != "")
            {//setup progressor dialog for Delete
              if (bShowProgressor)
                pStepProgressor2.Message = "Deleting invalid feature class associations...";

              if (!FabricUTILS.DeleteByInClause(pWS, pAdjLevels, pAdjLevels.Fields.get_Field(pAdjLevels.FindField(pAdjLevels.OIDFieldName)),
                m_InClauseInvalidFeatClassAssocs, !bIsUnVersioned, pStepProgressor2, pTrackCancel2))
              {
                FabricUTILS.AbortEditing(pWS);
                return;
              }
              FabricUTILS.StopEditing(pWS);
            }
          }
          finally
          {
            if (pProgressorDialog2 != null)
              pProgressorDialog2.HideDialog();

            if (pStepProgressor2 != null)
              pStepProgressor2.Hide();
          }
          #endregion
        
        }
      }
      catch (Exception ex)
      {
        FabricUTILS.AbortEditing(pWS);
        MessageBox.Show(ex.Message + Environment.NewLine + m_sErrMessage + Environment.NewLine +
          "Row Counter:" + m_CursorCnt.ToString(), "Focused Fabric Scan");
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
        pProgressorDialog = null;
      }
      #endregion

    }

    private double GetMaxShiftThreshold(ICadastralFabric pFab)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric2 pDECadaFab = (IDECadastralFabric2)pDEDS;
      double d_retVal = pDECadaFab.MaximumShiftThreshold;
      return d_retVal;
    }

    private double ConvertDistanceFromMetersToFabricUnits(double InDistance, ICadastralFabric InFabric, out string UnitString, out double MetersPerMapUnit)
    {
      IGeoDataset pGeoFab = (IGeoDataset)InFabric;
      IProjectedCoordinateSystem2 pPCS;
      ILinearUnit pMapLU;
      double dMetersPerMapUnit = 1;

      UnitString = "";

      MetersPerMapUnit = dMetersPerMapUnit;
      ISpatialReference2 pSpatRef = (ISpatialReference2)pGeoFab.SpatialReference;
      if (pSpatRef == null)
        return InDistance;

      if (pSpatRef is IProjectedCoordinateSystem2)
      {
        pPCS = (IProjectedCoordinateSystem2)pSpatRef;
        pMapLU = pPCS.CoordinateUnit;
        dMetersPerMapUnit = pMapLU.MetersPerUnit;
        MetersPerMapUnit = dMetersPerMapUnit;
        double dRes = InDistance / dMetersPerMapUnit;
        UnitString = pMapLU.Name;
        return dRes;
      }
      UnitString = "meters";
      return InDistance;
    }
    
    private void SetupDialogForReport(dlgInconsistentRecords dlg, string Title, string Info1, string Info2, string ObjectIDName, List<string> InClause, int Count)
    {
      //--------------------------------------------------------------------
      //INCONSISTENT FABRIC RECORDS REPORT
      //--------------------------------------------------------------------
 
      //Points that are not connected to lines (7 inconsistent records)
      //--------------------------------------------------------------------
      //In clause for ObjectIDs:
      //[OBJECTID] IN (86,88,89,90,91,92,93)

      dlg.label1.Text = Info1;
      dlg.Text = Title;
      dlg.btnSaveReport.Size = dlg.btnSelectAll.Size;
      dlg.btnSaveReport2.Location = dlg.btnSelectAll.Location;
      dlg.btnSaveReport.Visible = false;
      dlg.btnSaveReport2.Visible = true;
      dlg.btnCancel.Location = dlg.btnBack.Location;
      dlg.btnBack.Visible = false;
      dlg.btnDelete.Size = dlg.btnSelectAll.Size;
      dlg.btnDelete.Location = dlg.btnNext.Location;
      dlg.btnNext.Visible = false;

      dlg.btnDeselectAll.Visible = false;
      dlg.txtInClauseReport.Size = dlg.checkedListBox1.Size;
      dlg.txtInClauseReport.Location = dlg.checkedListBox1.Location;
      dlg.checkedListBox1.Visible = false;
      dlg.tvOrphanRecs.Visible = false;
      dlg.txtInClauseReport.Size = dlg.checkedListBox1.Size;
      dlg.txtInClauseReport.Visible = true;
      dlg.btnDelete.Visible = true;
      dlg.OIDFieldName = ObjectIDName;

      string temp = "Process time:" + dlg.ProcessTime + Environment.NewLine + Info2;
      Info2 = temp + Environment.NewLine + "(" + Count.ToString() + " inconsistent records)";

      WriteToTxtInclauseReport(dlg, InClause, Info2, ObjectIDName);
    }

    private bool ProcessUpdateLinePointOffset(ITable LinePointTable, Dictionary<int, double> LinePointLookup, IQueryFilter QueryFilter, bool Unversioned,
      bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      ICursor pLinePtCurs = null;
      try
      {
        ITableWrite pTableWr = (ITableWrite)LinePointTable;//used for unversioned table
        IRow pLinePointFeat = null;


        if (Unversioned)
          pLinePtCurs = pTableWr.UpdateRows(QueryFilter, false);
        else
          pLinePtCurs = LinePointTable.Update(QueryFilter, false);

        pLinePointFeat = pLinePtCurs.NextRow();
        Int32 iLineOffsetIDX = pLinePtCurs.Fields.FindField("LINEOFFSET");
        
        if (iLineOffsetIDX < 0)
          return false;

        bool bCont = true;
        while (pLinePointFeat != null)
        {//loop through all of the line points, and if any of the linepoint id values are in the lookup dictionary, then update offset value from the 
          //number stored in the dictionary
          int iKey = pLinePointFeat.OID;
          double dOffset = LinePointLookup[iKey];
          pLinePointFeat.set_Value(iLineOffsetIDX, dOffset);
          if (Unversioned)
            pLinePtCurs.UpdateRow(pLinePointFeat);
          else
            pLinePointFeat.Store();

          Marshal.ReleaseComObject(pLinePointFeat); //garbage collection

          //after garbage collection, and before getting the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;

          pLinePointFeat = pLinePtCurs.NextRow();
        }

        return bCont;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Problem updating line point offsets: " + Convert.ToString(ex.Message));
        return false;
      }
      finally
      {
           Marshal.ReleaseComObject(pLinePtCurs); //garbage collection
      }

    }

    private bool ProcessOrphanPointsFromLines(ITable linesTable, ref Dictionary<int, bool> inPointList, string WhereClause, 
      bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;

      //thePointList contains all the point oids.
      //search the lines table and remove the From and To oids from thePointList

      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iToPtID = linesTable.FindField("TOPOINTID");
        int iFromPtID = linesTable.FindField("FROMPOINTID");
        int iCtrPtID = linesTable.FindField("CENTERPOINTID");

        string ToPtIDFldName = linesTable.Fields.get_Field(iToPtID).Name;
        string FromPtIDFldName = linesTable.Fields.get_Field(iFromPtID).Name;
        string CtrPtIDFldName = linesTable.Fields.get_Field(iCtrPtID).Name;

        pQuFilter.SubFields = linesTable.OIDFieldName + "," + ToPtIDFldName + "," +
          FromPtIDFldName + "," + CtrPtIDFldName;

        pCur = linesTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iThisID = (int)pRow.get_Value(iToPtID);
          inPointList.Remove(iThisID);

          int iThisID2 = (int)pRow.get_Value(iFromPtID);
          inPointList.Remove(iThisID2);

          int iThisID3 = -1;
          object obj = pRow.get_Value(iCtrPtID);
          if (obj != DBNull.Value)
          {
            iThisID3 = (int)obj;
            inPointList.Remove(iThisID3);
          }

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
        MessageBox.Show(ex.Message, "Detect Orphan Points");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private bool ProcessNoLinesInParcels(ITable linesTable, ref Dictionary<int, bool> inParcelList, string WhereClause,
      bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;

      //thePointList contains all the point oids.
      //search the lines table and remove the From and To oids from thePointList

      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iParcelID = linesTable.FindField("PARCELID");

        string ParcelIDFldName = linesTable.Fields.get_Field(iParcelID).Name;
        pQuFilter.SubFields = linesTable.OIDFieldName + "," + ParcelIDFldName;

        pCur = linesTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iThisID = (int)pRow.get_Value(iParcelID);
          inParcelList.Remove(iThisID);
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
        MessageBox.Show(ex.Message, "Detect no-line parcels");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private bool ProcessOrphanLines(ITable linesTable, ref List<int> inParcelList, ref List<int> OrphanLinesList, string WhereClause,
      bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;
      inParcelList.Sort();
      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iParcelID = linesTable.FindField("PARCELID");
        string ParcelIDFldName = linesTable.Fields.get_Field(iParcelID).Name;
        
        pQuFilter.SubFields = linesTable.OIDFieldName + "," + ParcelIDFldName;

        pCur = linesTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iThisID4 = (int)pRow.get_Value(iParcelID);

          if (inParcelList.BinarySearch(iThisID4) < 0)
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
        MessageBox.Show(ex.Message, "Detect Orphan Lines");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private bool ProcessLinesWithSameFromTo(ITable fromTable, ref List<int> theSameFromToList, string WhereClause, bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;

      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iToPtID = fromTable.FindField("TOPOINTID");
        int iFromPtID = fromTable.FindField("FROMPOINTID");
        int iCtrPtID = fromTable.FindField("CENTERPOINTID");
        int iDistanceFld = fromTable.FindField("DISTANCE");
        int iDensifyType = fromTable.FindField("DENSIFYTYPE");
        bool bHasDensifyTypeFld = (iDensifyType >= 0);

        string ToPtIDFldName = fromTable.Fields.get_Field(iToPtID).Name;
        string FromPtIDFldName = fromTable.Fields.get_Field(iFromPtID).Name;
        string CtrPtIDFldName = fromTable.Fields.get_Field(iCtrPtID).Name;
        string DistanceFldName = fromTable.Fields.get_Field(iDistanceFld).Name;
        string DensifyTypeFldName = "";
        if (bHasDensifyTypeFld)
          DensifyTypeFldName = fromTable.Fields.get_Field(iDensifyType).Name;

        IFeatureClass pFC = (IFeatureClass)fromTable;

        pQuFilter.SubFields = pFC.ShapeFieldName + "," + fromTable.OIDFieldName + "," + ToPtIDFldName + "," +
          FromPtIDFldName + "," + DistanceFldName + "," + CtrPtIDFldName;

        if (bHasDensifyTypeFld)
          pQuFilter.SubFields += "," + DensifyTypeFldName;

        pCur = fromTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iThisID = (int)pRow.get_Value(iToPtID);
          int iThisID2 = (int)pRow.get_Value(iFromPtID);

          //same from and to
          //exclude linestrings, for same from and to points
          bool bIsLinestring = false;

          if (bHasDensifyTypeFld)
          {
            object obj = pRow.get_Value(iDensifyType);
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
              bHasOneSegment = (pSegColl.SegmentCount == 1); //final confirmation
            }
            object obj = pRow.get_Value(iDistanceFld);
            double dDistance = 0;
            if (obj != DBNull.Value)
              dDistance = Convert.ToDouble(obj);
            if (dDistance < 0.5 || bHasOneSegment)
              theSameFromToList.Add(iRow);
          }

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
        MessageBox.Show(ex.Message, "Detect Lines with same From and To");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private bool ProcessLinesWithSameFromToNoGeomTest(ITable fromTable, ref List<int> theSameFromToList, string WhereClause, bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;

      try
      {
        pQuFilter.WhereClause = WhereClause;
        int iToPtID = fromTable.FindField("TOPOINTID");
        int iFromPtID = fromTable.FindField("FROMPOINTID");
        int iCtrPtID = fromTable.FindField("CENTERPOINTID");
        int iDistanceFld = fromTable.FindField("DISTANCE");
        string DensifyTypeFldName = "";
        int iDensifyType = fromTable.FindField("DENSIFYTYPE");
        bool bHasDensifyTypeFld = (iDensifyType >= 0);

        string ToPtIDFldName = fromTable.Fields.get_Field(iToPtID).Name;
        string FromPtIDFldName = fromTable.Fields.get_Field(iFromPtID).Name;
        string CtrPtIDFldName = fromTable.Fields.get_Field(iCtrPtID).Name;
        string DistanceFldName = fromTable.Fields.get_Field(iDistanceFld).Name;

        if (bHasDensifyTypeFld)
          DensifyTypeFldName = fromTable.Fields.get_Field(iDensifyType).Name;

        //IFeatureClass pFC = (IFeatureClass)fromTable;
        string sSubFlds = fromTable.OIDFieldName + "," + ToPtIDFldName + "," +
          FromPtIDFldName + "," + DistanceFldName + "," + CtrPtIDFldName;

        if (bHasDensifyTypeFld)
          DensifyTypeFldName = "," + fromTable.Fields.get_Field(iDensifyType).Name;

        pQuFilter.SubFields = sSubFlds + DensifyTypeFldName;

        pCur = fromTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;
        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iThisID = (int)pRow.get_Value(iToPtID);
          int iThisID2 = (int)pRow.get_Value(iFromPtID);

          //same from and to
          //exclude linestrings, for same from and to points
          bool bIsLinestring = false; //this code assumes no linestrings

          if (bHasDensifyTypeFld)
          {
            object obj = pRow.get_Value(iDensifyType);
            if (obj != DBNull.Value)
            {
              int i = (int)obj;
              if (i > 0)
                bIsLinestring = true;
            }
          }

          if (iThisID == iThisID2 && !bIsLinestring)
              theSameFromToList.Add(iRow);

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
        MessageBox.Show(ex.Message, "Detect Lines with same From and To");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private bool ProcessOrphanLinePoints(ITable fromTable, ref Dictionary<int, string> inLinePoint_DICT, ref List<int> OrphanLinePointList, string WhereClause, bool bShowProgressor, ITrackCancel pTrackCancel)
    {
      bool bCont = true;
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pCur = null;
      try
      {
        //convert the values to a list
        List<string> DictionaryValuesList = new List<string>();
        Dictionary<int, String>.ValueCollection valCollection = inLinePoint_DICT.Values;
        foreach (string val in valCollection)
          DictionaryValuesList.Add(val);

        DictionaryValuesList.Sort();
        //now remove repeats
        List<string> temp5 = DictionaryValuesList.Distinct().ToList();
        DictionaryValuesList = temp5.ToList();
        temp5.Clear();
        temp5 = null;

        pQuFilter.WhereClause = WhereClause;
        int iToPtID_Idx = fromTable.FindField("TOPOINTID");
        int iFromPtID_Idx = fromTable.FindField("FROMPOINTID");

        string ToPtIDFldName = fromTable.Fields.get_Field(iToPtID_Idx).Name;
        string FromPtIDFldName = fromTable.Fields.get_Field(iFromPtID_Idx).Name;

        pQuFilter.SubFields = fromTable.OIDFieldName + "," + ToPtIDFldName + "," + FromPtIDFldName;

        pCur = fromTable.Search(pQuFilter, false); //do this with separate cursors to handle large datasets
        int iCursorCnt = 0;

        IRow pRow = pCur.NextRow();
        while (pRow != null)
        {
          iCursorCnt++;
          int iRow = pRow.OID;
          int iToPtID = (int)pRow.get_Value(iToPtID_Idx);
          int iFromPtID = (int)pRow.get_Value(iFromPtID_Idx);

          string sLineRef = iFromPtID.ToString() + ":" + iToPtID.ToString();
          DictionaryValuesList.Remove(sLineRef);

          string sLineRefRev = iToPtID.ToString() + ":" + iFromPtID.ToString();
          DictionaryValuesList.Remove(sLineRefRev);

          Marshal.FinalReleaseComObject(pRow);
          //after garbage collection, and before getting the next row,
          //check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
            bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          pRow = pCur.NextRow();
        }
        if (!bCont)
          return bCont;
        
        //now the remaining list items need to be added to the orphan line points list
        foreach (string sValue in DictionaryValuesList)
        {
          var keysWithMatchingValues = inLinePoint_DICT.Where(p => p.Value == sValue).Select(p => p.Key);
          foreach(var key in keysWithMatchingValues) //should not be repeats, but loop anyway
            OrphanLinePointList.Add(key);
        }

        return bCont;
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Detect Inconsistent line points");
        return false;
      }

      finally
      {
        if (pCur != null)
          do { } while (Marshal.FinalReleaseComObject(pCur) > 0);
      }
    }

    private void WriteToTxtInclauseReport(dlgInconsistentRecords dlg, List<string> InClause, string Info, string OIDFieldName)
    {
      //Points that are not connected to lines (7 inconsistent records)
      //--------------------------------------------------------------------
      dlg.txtInClauseReport.Text = Info + Environment.NewLine;
      dlg.txtInClauseReport.Text += "----------------------------------------------------------------------------------------" + Environment.NewLine;
      if (InClause.Count() > 0)
        {
          foreach (string s in InClause)
          {
            if (s.Trim() == "")
              continue;
            dlg.txtInClauseReport.Text += "In clause for ObjectIDs:" + Environment.NewLine;
            dlg.txtInClauseReport.Text += OIDFieldName + " IN (" + s + ")" + Environment.NewLine;
            dlg.txtInClauseReport.Text += "----------------------------------------------------------------------------------------" + Environment.NewLine;
          }
        }
    }

    protected override void OnUpdate()
    {
    }

  }
}
