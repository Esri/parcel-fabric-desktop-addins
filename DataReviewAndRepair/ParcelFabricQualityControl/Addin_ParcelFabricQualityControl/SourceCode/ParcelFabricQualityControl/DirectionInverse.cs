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
    private string sUnderline = Environment.NewLine + "---------------------------------------------------------------------" + Environment.NewLine;
    private string m_sLineCount;
    private string m_sParcelCount;
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

      if (m_pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing first, and try again.", "Start Editing");
        return;
      }

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

      IEditProperties2 pEditorProps2 = (IEditProperties2)m_pEd;

      InverseDirectionDLG InverseDirectionDialog = new InverseDirectionDLG(pEditorProps2);
      IArray PolygonLyrArr;
      IMap pMap = m_pEd.Map;
      ICadastralFabric pCadFabric = null;
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
      }

      IAngularConverter pAngConv = new AngularConverterClass();
      Utilities Utils = new Utilities();

      if (!Utils.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTParcels, out PolygonLyrArr))
        return;

      //if we're in an edit session then grab the target fabric
      if (m_pEd.EditState == esriEditState.esriStateEditing)
        pCadFabric = pCadEd.CadastralFabric;

      if (pCadFabric == null)
      {//find the first fabric in the map
        if (!Utils.GetFabricFromMap(pMap, out pCadFabric))
        {
          MessageBox.Show
            ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }
      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedDelete = false;
      IWorkspace pWS = null;
      ITable pParcelsTable = null;
      ITable pLinesTable = null;
      IProgressDialog2 pProgressorDialog = null;
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      try
      {
       //Get the selection of parcels
        IFeatureLayer pFL = (IFeatureLayer)PolygonLyrArr.get_Element(0);

        IDataset pDS = (IDataset)pFL.FeatureClass;
        pWS = pDS.Workspace;

        if (!Utils.SetupEditEnvironment(pWS, pCadFabric, m_pEd, out bIsFileBasedGDB,
          out bIsUnVersioned, out bUseNonVersionedDelete))
        {
          return;
        }

        ICadastralSelection pCadaSel = (ICadastralSelection)pCadEd;
        IEnumGSParcels pEnumGSParcels = pCadaSel.SelectedParcels;// need to get the parcels before trying to get the parcel count: BUG workaround
        IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
        ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;

        if (pCadaSel.SelectedParcelCount == 0 && pSelSet.Count == 0)
        {
          MessageBox.Show("Please select some fabric parcels and try again.", "No Selection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          return;
        }

        //Display the dialog
        DialogResult pDialogResult = InverseDirectionDialog.ShowDialog();
        if (pDialogResult != DialogResult.OK)
          return;

        m_bShowProgressor = (pSelSet.Count > 10 || pCadaSel.SelectedParcelCount > 10);
        if (m_bShowProgressor)
        {
          m_pProgressorDialogFact = new ProgressDialogFactoryClass();
          m_pTrackCancel = new CancelTrackerClass();
          m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
          m_pStepProgressor.MinRange = 1;
          m_pStepProgressor.MaxRange = pCadaSel.SelectedParcelCount * 7; //(estimate 7 lines per parcel)
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
        bool bCont = true;
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
            bCont = m_pTrackCancel.Continue();
            if (!bCont)
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

        if (!bCont)
        {
          //AbortEdits(bUseNonVersionedDelete, m_pEd, pWS);
          return;
        }

        ICursor pCursor = null;
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
        m_sParcelCount = oidList.Count().ToString();

        int idxCENTERPTID = pLinesTable.FindField("CENTERPOINTID");
        int idxPARCELID = pLinesTable.FindField("PARCELID");
        int idxFROMPTID = pLinesTable.FindField("FROMPOINTID");
        int idxTOPOINTID = pLinesTable.FindField("TOPOINTID");
        int idxDISTANCEID = pLinesTable.FindField("DISTANCE");
        int idxRADIUSID = pLinesTable.FindField("RADIUS");

        List<string> sInClauseList = Utils.InClauseFromOIDsList(oidList, tokenLimit);
        foreach (string sInClause in sInClauseList)
        {
          m_pQF.WhereClause = ParcelIDFldName + " IN (" + sInClause + ") AND (" +
                  LineCategoryFldName + " <> 4)";

          pCursor = pLinesTable.Search(m_pQF, false);
          IRow pLineRecord = pCursor.NextRow();
          while (pLineRecord != null)
          {
            IFeature pFeat = (IFeature)pLineRecord;
            IGeometry pGeom = pFeat.ShapeCopy;
            if (pGeom != null)
            {
              if (!pGeom.IsEmpty)
              {
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
                  if (pSegColl.SegmentCount == 1)
                  {
                    ICircularArc pCirc = pSeg as ICircularArc;
                    if (!pCirc.IsLine)
                    {
                      double dChordDist = Convert.ToDouble(pFeat.get_Value(idxDISTANCEID));
                      double dRadius = Convert.ToDouble(pFeat.get_Value(idxRADIUSID));

                      //compute circular arc to get the central angle parameter, 
                      //use *attribute* radius and *attribute* chord distance
                      IConstructCircularArc pConstrArc = new CircularArcClass();
                      pConstrArc.ConstructBearingRadiusChord(pSeg.FromPoint, 0, (dRadius<0), Math.Abs(dRadius),dChordDist, pCirc.IsMinor);
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
                double dAttributeDirection = (double)pLineRecord.get_Value(idxDirection);
                dict_LinesToRecordDirection.Add(pFeat.OID, dAttributeDirection);
                double dCorrectedDirection = dAttributeDirection; //initialize as the same
                IPolyline pPolyline = (IPolyline)pGeom;
                IPoint pPt1 = pPolyline.FromPoint;
                IPoint pPt2 = pPolyline.ToPoint;
                ILine pLine = new LineClass();
                pLine.PutCoords(pPt1, pPt2);
                pAngConv.SetAngle(pLine.Angle,esriDirectionType.esriDTPolar,esriDirectionUnits.esriDURadians);
                double dInverseDirn=pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth,esriDirectionUnits.esriDUDecimalDegrees);
                dict_LinesToInverseDirection.Add(pFeat.OID,dInverseDirn);
                dict_LinesToShapeDistance.Add(pFeat.OID, pLine.Length);
                double dOffset=dAttributeDirection-dInverseDirn+360;
                dict_LinesToDirectionOffset.Add(pFeat.OID, dOffset);
                int fId = (int)pFeat.get_Value(idxParcelID);
                List<int> ll = dict_ParcelLinesListLookup[fId];
                ll.Add(pFeat.OID);
                dict_LinesToParcel.Add(pFeat.OID, fId);
              }
            }
            Marshal.ReleaseComObject(pLineRecord);
            pLineRecord = pCursor.NextRow();
          }

          Marshal.ReleaseComObject(pCursor);
        }

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
          dict_LinesToComputedDirection, dict_LinesToShapeDistance, dDirectionOffset, dBearingOffsetTolerance, dDistOffsetTolerance, ref m_pFIDSetParcels);

        int[] pParcelIds = new int[m_pFIDSetParcels.Count()];
        ILongArray pParcelsToLock = new LongArrayClass();
        Utils.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, ref pParcelIds, m_pStepProgressor);

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
        //ILongArray pParcelsToLock = new LongArrayClass();
        //Utils.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, ref pParcelIds, m_pStepProgressor);
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
        sInClauseList = Utils.InClauseFromOIDsList(lstOidsOfLines, tokenLimit);

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
        if (bUseNonVersionedDelete)
        {
          if (!Utils.StartEditing(pWS, bIsUnVersioned))
            return;
        }

        ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        pSchemaEd.ReleaseReadOnlyFields(pLinesTable, esriCadastralFabricTable.esriCFTLines); //release safety-catch

        //now go through the lines to update
        foreach (string sInClause in sInClauseList)
        {
          m_pQF.WhereClause = pLinesTable.OIDFieldName + " IN (" + sInClause + ")";
          if (!Utils.UpdateTableByDictionaryLookup(pLinesTable, m_pQF, "BEARING", bIsUnVersioned, dict_LinesToComputedDirection))
          {
            m_bNoUpdates = true;
            m_sReport += sUnderline + "ERROR occurred updating direction values. No parcel lines were updated.";
            AbortEdits(bIsUnVersioned, m_pEd, pWS);
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on
            return;
          }
        }

        //next get the radial lines and update the radial bearings if needed.
        if (lstParcelsWithCurves.Count > 0)
        {
          sInClauseList.Clear();
          sInClauseList = Utils.InClauseFromOIDsList(lstParcelsWithCurves, tokenLimit);
          foreach (string sInClause in sInClauseList)
          {
            m_pQF.WhereClause = ParcelIDFldName + " IN (" + sInClause + ") AND (" +
                    LineCategoryFldName + " = 4)";
            if (!Utils.UpdateRadialLineBearingsByDictionaryLookups(pLinesTable, m_pQF, bIsUnVersioned,
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
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on

        //now run through the parcels id list and update misclose and ShapeStdErr m_pFIDSetParcels
        Dictionary<int, List<double>> UpdateSysFieldsLookup = Utils.ReComputeParcelSystemFieldsFromLines(pCadEd, pMap.SpatialReference, 
          (IFeatureClass)pParcelsTable, pParcelIds);

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
        Utils.UpdateParcelSystemFieldsByLookup(pParcelsTable, UpdateSysFieldsLookup, bIsUnVersioned);
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);//set safety back on

        m_sLineCount = dict_LinesToComputedDirection.Count.ToString();
        m_pEd.StopOperation("Inversed directions on " + m_sLineCount + " lines");

        //IGeoDatabaseBridge2 geodatabaseBridge2 = new GeoDatabaseHelperClass();
        //geodatabaseBridge2.RemoveList(pSelSet, UpdateList);


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
        if (pProgressorDialog != null)
          pProgressorDialog.HideDialog();

        if (m_bShowReport)
        {
          if (!m_bNoUpdates)
          {
            m_sReport += sUnderline + m_pFIDSetParcels.Count().ToString() + " out of " + m_sParcelCount + " selected parcels updated.";
            m_sReport += Environment.NewLine + m_sLineCount + " parcel line direction attributes recalculated." + sUnderline;
          }
          m_sReport += Environment.NewLine + " *** BETA *** " + sUnderline;
          ReportDLG ReportDialog = new ReportDLG();
          ReportDialog.txtReport.Text = m_sReport;
          ReportDialog.ShowDialog();
        }

        RefreshMap(pActiveView, PolygonLyrArr);
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

        if (bUseNonVersionedDelete)
        {
          pCadEd.CadastralFabricLayer = null;
          PolygonLyrArr = null;
        }

        if (pMouseCursor != null)
          pMouseCursor.SetCursor(0);

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
      double DirectionOffset, double BearingTolerance, double DistanceTolerance, ref IFIDSet AffectedParcelsFIDs)
    {
      double dSum = 0;
      double dLongestLineOffset = DirectionOffset; //initialize the correction to incoming DirectionOffset value
      bool bUseDirectionOffset = (DirectionOffset!=-1440);
     
      IAngularConverter pAngConv = new AngularConverterClass();
      Utilities UTILS = new Utilities();
      foreach (var key in dict_ParcelLinesListLookup.Keys)
      {
        List<int> ParcelLinesList = dict_ParcelLinesListLookup[key];

        if (!bUseDirectionOffset)
        {
          Dictionary<int, int> dict_Lookup1 = new Dictionary<int, int>();
          int jj = ParcelLinesList.Count;
          int kk = 0;
          double[] d = new double[jj];
          double[] len = new double[jj];
          foreach (int ll in ParcelLinesList)
          {
            double dd = dict_LinesToDirectionOffset[ll];
            dict_Lookup1.Add(kk, ll);
            len[kk] = dict_LinesToShapeDistance[ll];
            d[kk++] = dd;
            dSum += dd;
          }
          double dMean = 0; double dStdDev = 0;
          double dRange = 0; int iOutliers = 0;

          UTILS.GetStatistics(d, dSum, 1, out dMean, out dStdDev, out dRange, out iOutliers);

          double dlBound = dMean - dStdDev;// -BearingTolerance; this bearing tolerance test should be applied afterwards
          double dUBound = dMean + dStdDev;// +BearingTolerance;
          double dLongest = 0;
          dLongestLineOffset = 0;
          iOutliers = 0;//recount
          List<double> GoodLinesList = new List<double>();

          for (int i = 0; i < d.GetLength(0); i++)
          {
            int v = dict_Lookup1[i];
            if (d[i] < dlBound || d[i] > dUBound)
            {//these are the outliers
              iOutliers++;
              bool bExists;
              AffectedParcelsFIDs.Find(key, out bExists);
              if (!bExists)
                AffectedParcelsFIDs.Add(key);
            }
            else
            {
              if (len[i] > dLongest)
              {
                dLongest = len[i];
                dLongestLineOffset = d[i];
              }

              GoodLinesList.Add(d[i]);
              //ParcelLinesList.Remove(v);//exclude this if applying correction to all lines
            }
          }

          jj = GoodLinesList.Count;
          kk = 0;
          dSum = 0;
          double[] e = new double[jj];

          foreach (double goodDirection in GoodLinesList)
          {
            e[kk++] = goodDirection;
            dSum += goodDirection;
          }
          UTILS.GetStatistics(e, dSum, 1, out dMean, out dStdDev, out dRange, out iOutliers);
        }
        else
        {
          bool bExists;
          AffectedParcelsFIDs.Find(key, out bExists);
          if (!bExists)
            AffectedParcelsFIDs.Add(key);        
        }

        //now use the good mean to apply to the corrected directions
        //alternative uses the longest line's correction
        foreach (int i in ParcelLinesList)
        {
          //double dOffset = dict_LinesToDirectionOffset[i];
          //double dNewComputed = dict_LinesToInverseDirection[i] + dMean;
          double dOriginalDirection = dict_LinesToRecordDirection[i];
          double dNewComputed = dict_LinesToInverseDirection[i] + dLongestLineOffset;
          double dAngleDiff = Math.Abs(dOriginalDirection + 360 - dNewComputed);
          if (dAngleDiff >= 360)
            dAngleDiff = dAngleDiff - 360;
          double dLateralEffectOfAngleDiff = Math.Abs(Math.Tan(dAngleDiff * Math.PI / 180) * dict_LinesToShapeDistance[i]);
          bool bBothUnchecked=(BearingTolerance < 0 && DistanceTolerance < 0);
          //negative Tolerance means the option is unchecked, so no filter, add lines

          if (  bBothUnchecked  || ((dAngleDiff > BearingTolerance) && BearingTolerance > 0)
                                || ((dLateralEffectOfAngleDiff > DistanceTolerance) && DistanceTolerance > 0) )
          {
            pAngConv.SetAngle(dNewComputed, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
            dNewComputed = pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
            dict_LinesToComputedDirection.Add(i, dNewComputed);
            continue;//go the next one if this is already added, otherwise try next test
          }
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
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, ParcelLayers.get_Element(z), ActiveView.Extent);
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ParcelLayers.get_Element(z), ActiveView.Extent);
          }
        }
      }
      catch
      { }
    }

  }
}
