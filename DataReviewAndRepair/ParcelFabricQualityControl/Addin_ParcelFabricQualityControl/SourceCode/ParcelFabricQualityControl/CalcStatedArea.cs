/*
 Copyright 1995-2021 Esri

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
  public class CalcStatedArea : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSetParcels;
    private IQueryFilter m_pQF;
    private bool m_bShowProgressor = false;
    //private bool m_bNoUpdates=false;
    //private string m_sEntryMethod;
    //private string m_sHeight_Or_ElevationLayer;
    //private string m_stxtElevDifference;
    //private string m_sUnit="meters";

    public CalcStatedArea()
    {
    }

    protected override void OnClick()
    {
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
        MessageBox.Show("The Area Calculation does not work when the parcel is open.\r\nPlease close the parcel and try again.",
          "Calculate Stated Area", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      IActiveView pActiveView = ArcMap.Document.ActiveView;

      CalcStatedAreaDLG CalcStatedArea = new CalcStatedAreaDLG();
      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedEdit = false;
      IWorkspace pWS = null;
      ITable pParcelsTable = null;
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

        if (pCadaSel.SelectedParcelCount == 0 && pSelSet.Count == 0 )
        {
          MessageBox.Show("Please select some fabric parcels and try again.", "No Selection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          return;
        }

        ArcMap.Application.CurrentTool = null;

        //Display the dialog
        DialogResult pDialogResult = CalcStatedArea.ShowDialog();
        if (pDialogResult != DialogResult.OK)
        {
          return;
        }

        m_bShowProgressor = (pSelSet.Count > 10) || pCadaSel.SelectedParcelCount > 10;
        if (m_bShowProgressor)
        {
          m_pProgressorDialogFact = new ProgressDialogFactoryClass();
          m_pTrackCancel = new CancelTrackerClass();
          m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
          m_pStepProgressor.MinRange = 1;
          m_pStepProgressor.MaxRange = pCadaSel.SelectedParcelCount * 3; //(3 runs through the selection)
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
        int tokenLimit = 995;
        List<int> oidList = new List<int>();
        Dictionary<int, string> dict_ParcelSelection2CalculatedArea = new Dictionary<int, string>();

        pParcelsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);

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
          if (!m_pTrackCancel.Continue())
            return;
        }


        string sSuffixUnit = CalcStatedArea.txtSuffix.Text;
        int iAreaPrec = (int)CalcStatedArea.numDecPlaces.Value;

        double dSqMPerUnit = 1;
        if (CalcStatedArea.cboAreaUnit.FindStringExact("Acres") == CalcStatedArea.cboAreaUnit.SelectedIndex)
          dSqMPerUnit = 4046.86;

        if (CalcStatedArea.cboAreaUnit.FindStringExact("Hectares") == CalcStatedArea.cboAreaUnit.SelectedIndex)
          dSqMPerUnit = 10000;

        if (CalcStatedArea.cboAreaUnit.FindStringExact("Square Feet") == CalcStatedArea.cboAreaUnit.SelectedIndex)
          dSqMPerUnit = 0.09290304;

        if (CalcStatedArea.cboAreaUnit.FindStringExact("Square Feet US") == CalcStatedArea.cboAreaUnit.SelectedIndex)
          dSqMPerUnit = 0.09290341;

        if (m_bShowProgressor)
        {
          pProgressorDialog.ShowDialog();
          m_pStepProgressor.Message = "Computing areas...";
        }
        List<string> sInClauseList0 = null;
        m_pQF = new QueryFilterClass();
        if (oidList.Count() > 0)
        {
          sInClauseList0 = Utils.InClauseFromOIDsList(oidList, tokenLimit);
          foreach (string sInClause in sInClauseList0)
          {
            m_pQF.WhereClause = pParcelsTable.OIDFieldName + " IN (" + sInClause + ")";
            CalculateStatedArea(m_pQF, pParcelsTable, pCadEd, m_pEd.Map.SpatialReference, dSqMPerUnit, sSuffixUnit, 
              iAreaPrec, ref dict_ParcelSelection2CalculatedArea, m_pTrackCancel);

            if (m_bShowProgressor)
            {
              if (!m_pTrackCancel.Continue())
                return;
            }       
          }
        }
        else
          return;


        if (m_bShowProgressor)
        {
          if (!m_pTrackCancel.Continue())
            return;
        }
        #region Create Cadastral Job
        //string sTime = "";
        //if (!bIsUnVersioned && !bIsFileBasedGDB)
        //{
        //  //see if parcel locks can be obtained on the selected parcels. First create a job.
        //  DateTime localNow = DateTime.Now;
        //  sTime = Convert.ToString(localNow);
        //  ICadastralJob pJob = new CadastralJobClass();
        //  pJob.Name = sTime;
        //  pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
        //  pJob.Description = "Interpolate Z values on selected features";
        //  try
        //  {
        //    Int32 jobId = pCadFabric.CreateJob(pJob);
        //  }
        //  catch (COMException ex)
        //  {
        //    if (ex.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_ALREADY_EXISTS)
        //    {
        //      MessageBox.Show("Job named: '" + pJob.Name + "', already exists");
        //    }
        //    else
        //    {
        //      MessageBox.Show(ex.Message);
        //    }
        //    return;
        //  }
        //}
        #endregion

        //ILongArray pTempParcelsLongArray = new LongArrayClass();
        //List<int> lstParcelChanges = Utils.FIDsetToLongArray(m_pFIDSetParcels, ref pTempParcelsLongArray, ref pParcelIds, m_pStepProgressor);

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

        //ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        //pSchemaEd.ReleaseReadOnlyFields(pParcelsTable, esriCadastralFabricTable.esriCFTParcels);
        int idxParcelStatedArea = pParcelsTable.FindField("STATEDAREA");
        string ParcelStatedAreaFieldName = pParcelsTable.Fields.get_Field(idxParcelStatedArea).Name;
        if (m_bShowProgressor)
        {
        //  pProgressorDialog.ShowDialog();
          m_pStepProgressor.Message = "Updating parcel areas...";
        }

        IRow pTheFeatRow = null;
        ICursor pUpdateCursor = null;
        foreach (string sInClause in sInClauseList0)
        {
          m_pQF.WhereClause = pParcelsTable.OIDFieldName + " IN (" + sInClause + ")";

          if (bUseNonVersionedEdit)
          {
            ITableWrite pTableWr = (ITableWrite)pParcelsTable; //used for unversioned table
            pUpdateCursor = pTableWr.UpdateRows(m_pQF, false);
          }
          else
            pUpdateCursor = pParcelsTable.Update(m_pQF, false);

          pTheFeatRow = pUpdateCursor.NextRow();
          while (pTheFeatRow != null)
          {
            string sAreaString = dict_ParcelSelection2CalculatedArea[pTheFeatRow.OID];
            pTheFeatRow.set_Value(idxParcelStatedArea, sAreaString);
            pTheFeatRow.Store();
            Marshal.ReleaseComObject(pTheFeatRow); //garbage collection
            if (m_bShowProgressor)
            {
              if (!m_pTrackCancel.Continue())
                break;
            }
            pTheFeatRow = pUpdateCursor.NextRow();
          }
          Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
          if (m_bShowProgressor)
          {
            if (!m_pTrackCancel.Continue())
              break;
          }
        }
        m_pEd.StopOperation("Calculate Stated Area");
        //pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);//set fields back to read-only
      }
      catch(Exception ex)
      {
        if (m_pEd != null)
          AbortEdits(bIsUnVersioned, m_pEd, pWS);
        MessageBox.Show(ex.Message);
      }
      finally
      {
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
        
        // refresh the attributes dialog
        ISelectionEvents pSelEvents = (ISelectionEvents)m_pEd.Map;
        pSelEvents.SelectionChanged();

        if (bUseNonVersionedEdit)
        {
          pCadEd.CadastralFabricLayer = null;
          PolygonLyrArr = null;
        }

        if (pMouseCursor != null)
          pMouseCursor.SetCursor(0);
      }
    }


    protected override void OnUpdate()
    {
    }

    private void CalculateStatedArea(IQueryFilter m_pQF, ITable pParcelsTable, ICadastralEditor pCadEd, ISpatialReference pMapSR,
             double SquareMetersPerUnitFactor, string Suffix, int DecimalPlaces, ref Dictionary<int, string> dict_ParcelSelection2CalculatedArea, ITrackCancel pTrackCancel)
    {
      bool bTrackCancel = (pTrackCancel != null);
      //ILine pLine = new LineClass();

      ICursor pCursor = pParcelsTable.Search(m_pQF, false);
      IRow pParcelRecord = pCursor.NextRow();
      Utilities Utils = new Utilities();

      IArray pParcelFeatArr = new ArrayClass();
      IGeoDataset pGeoDS = (IGeoDataset)((IFeatureClass)pParcelsTable).FeatureDataset;
      ISpatialReference pFabricSR = pGeoDS.SpatialReference;
      IProjectedCoordinateSystem pPCS = null;
      double dMetersPerUnit = 1;
      bool bFabricIsInGCS = !(pFabricSR is IProjectedCoordinateSystem);
      if (!bFabricIsInGCS)
      {
        pPCS = (IProjectedCoordinateSystem)pFabricSR;
        dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
      }
      else
      {
        pPCS = (IProjectedCoordinateSystem)pMapSR;
        dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
      }

      //for each parcel record
      while (pParcelRecord != null)
      {
        IFeature pFeat = (IFeature)pParcelRecord;
        IGeometry pGeom = pFeat.Shape;
        pParcelFeatArr.Add(pFeat);

        Marshal.ReleaseComObject(pParcelRecord);
        if (bTrackCancel)
          if (!pTrackCancel.Continue())
            break;

        pParcelRecord = pCursor.NextRow();
      }
      Marshal.ReleaseComObject(pCursor);

      ICadastralFeatureGenerator pFeatureGenerator = new CadastralFeatureGeneratorClass();
      IEnumGSParcels pEnumGSParcels = pFeatureGenerator.CreateParcelsFromFeatures(pCadEd, pParcelFeatArr, true);

      pEnumGSParcels.Reset();
      Dictionary<int, int> dict_ParcelAndStartPt = new Dictionary<int, int>();
      Dictionary<int, IPoint> dict_PointID2Point = new Dictionary<int, IPoint>();

      //dict_PointID2Point -->> this lookup makes an assumption that the fabric TO point geometry is at the same location as the line *geometry* endpoint
      IParcelLineFunctions3 ParcelLineFx = new ParcelFunctionsClass();

      IGSParcel pGSParcel = pEnumGSParcels.Next();
      int iFromPtIDX = -1;
      int iToPtIDX = -1;
      int iParcelIDX = -1;
      int iIsMajorFldIdx = -1;

      while (pGSParcel != null)
      {
        IEnumCELines pCELines = new EnumCELinesClass();
        IEnumGSLines pEnumGSLines = (IEnumGSLines)pCELines;

        IEnumGSLines pGSLinesInner = pGSParcel.GetParcelLines(null, false);
        pGSLinesInner.Reset();
        IGSParcel pTemp = null;
        IGSLine pGSLine = null;
        ICadastralFeature pCF = (ICadastralFeature)pGSParcel;
        int iParcID = pCF.Row.OID;
        pGSLinesInner.Next(ref pTemp, ref pGSLine);
        bool bStartPointAdded = false;
        int iFromPtID = -1;
        while (pGSLine != null)
        {
          if (pGSLine.Category == esriCadastralLineCategory.esriCadastralLineBoundary ||
            pGSLine.Category == esriCadastralLineCategory.esriCadastralLineRoad ||
            pGSLine.Category == esriCadastralLineCategory.esriCadastralLinePartConnection)
          {
            pCELines.Add(pGSLine);
            ICadastralFeature pCadastralLineFeature = (ICadastralFeature)pGSLine;
            IFeature pLineFeat = (IFeature)pCadastralLineFeature.Row;
            if (iFromPtIDX == -1)
              iFromPtIDX = pLineFeat.Fields.FindField("FROMPOINTID");
            if (iToPtIDX == -1)
              iToPtIDX = pLineFeat.Fields.FindField("TOPOINTID");
            if (iParcelIDX == -1)
              iParcelIDX = pLineFeat.Fields.FindField("PARCELID");
            if (iIsMajorFldIdx == -1)
              iIsMajorFldIdx = pLineFeat.Fields.FindField("ISMAJOR");

            if (!bStartPointAdded)
            {
              iFromPtID = (int)pLineFeat.get_Value(iFromPtIDX);
              bStartPointAdded = true;
            }
            IPolyline pPolyline = (IPolyline)pLineFeat.ShapeCopy;
            //if (bFabricIsInGCS)
            pPolyline.Project(pPCS);
            //dict_PointID2Point -->> this lookup makes an assumption that the fabric TO point geometry is at the same location as the line *geometry* endpoint
            int iToPtID = (int)pLineFeat.get_Value(iToPtIDX);
            //first make sure the point is not already added
            if (!dict_PointID2Point.ContainsKey(iToPtID))
              dict_PointID2Point.Add(iToPtID, pPolyline.ToPoint);
          }
          pGSLinesInner.Next(ref pTemp, ref pGSLine);
        }

        if (pGSParcel.Unclosed)
        {//skip unclosed parcels
          pGSParcel = pEnumGSParcels.Next();
          continue;
        }

        IGSForwardStar pFwdStar = ParcelLineFx.CreateForwardStar(pEnumGSLines);
        //forward star is created for this parcel, now ready to find misclose for the parcel
        List<int> LineIdsList = new List<int>();
        List<IVector3D> TraverseCourses = new List<IVector3D>();
        List<int> FabricPointIDList = new List<int>();
        List<double> RadiusList = new List<double>();
        List<bool> IsMajorList = new List<bool>();

        bool bPass = false;
        if (!bFabricIsInGCS)
          bPass = Utils.GetParcelTraverseEx(ref pFwdStar, iIsMajorFldIdx, iFromPtID, dMetersPerUnit,
            ref LineIdsList, ref TraverseCourses, ref FabricPointIDList, ref RadiusList, ref IsMajorList, 0, -1, -1, false);
        else
          bPass = Utils.GetParcelTraverseEx(ref pFwdStar, iIsMajorFldIdx, iFromPtID, dMetersPerUnit * dMetersPerUnit,
            ref LineIdsList, ref TraverseCourses, ref FabricPointIDList, ref RadiusList, ref IsMajorList, 0, -1, -1, false);
        //List<double> SysValList = new List<double>();
        IVector3D MiscloseVector = null;
        IPoint[] FabricPoints = new IPoint[FabricPointIDList.Count];//from control

        int f = 0;
        foreach (int j in FabricPointIDList)
          FabricPoints[f++] = dict_PointID2Point[j];

        double dRatio = 10000;
        double dArea = 0;
        f = FabricPointIDList.Count - 1;
        IPoint[] AdjustedTraversePoints = Utils.BowditchAdjustEx(TraverseCourses, FabricPoints[f], FabricPoints[f],
             RadiusList, IsMajorList, out MiscloseVector, out dRatio, out dArea);

        if (MiscloseVector == null)
        {//skip if vector closure failed
          pGSParcel = pEnumGSParcels.Next();
          continue;
        }

        dArea *=  (dMetersPerUnit * dMetersPerUnit); //convert to square meters first
        dArea /= SquareMetersPerUnitFactor; //convert to the given unit equivalent

        string sFormattedArea = Math.Round(dArea,DecimalPlaces).ToString() + Suffix;
        dict_ParcelSelection2CalculatedArea.Add(pGSParcel.DatabaseId, sFormattedArea);

        pGSParcel = pEnumGSParcels.Next();

        //if (bShowProgressor)
        //{
        //  if (pStepProgressor.Position < pStepProgressor.MaxRange)
        //    pStepProgressor.Step();
        //}
        }
      }

    private void RefreshMap(IActiveView ActiveView, IArray ParcelLayers)
    {
      try
      {
        for (int z = 0; z <= ParcelLayers.Count - 1; z++)
        {
          if (ParcelLayers.get_Element(z) != null)
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection | esriViewDrawPhase.esriViewGeography, ParcelLayers.get_Element(z), ActiveView.Extent);
        }
      }
      catch
      { }
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

  }
}
