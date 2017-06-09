/*
 Copyright 1995-2017 Esri

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
  public class InterpolateZ : ESRI.ArcGIS.Desktop.AddIns.Button
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
    private Dictionary<string, double> m_dict_ControlName2HeightDiff = new Dictionary<string, double>();
    private string m_sLineCount;
    private int m_iTotalParcelCount;
    private int m_iTotalLineCount;
    private int m_iTotalPointCount;
    private int m_iTotalControlCount;
    private int m_iExcludedPointCount;
    private int m_iExcludedControlCount;
    private string m_sParcelCount;
    private bool m_bShowReport=false;
    private bool m_bShowProgressor = false;
    private bool m_bNoUpdates=false;
    private string m_sEntryMethod;
    private string m_sHeight_Or_ElevationLayer;
    private string m_stxtElevDifference;
    private string m_sUnit="meters";
    private double m_dAverageElevation = 0.0;
    public InterpolateZ()
    {
    }

    protected override void OnClick()
    {
      m_sReport = "Interpolate Z Report:";
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
        MessageBox.Show("Interpolate Z does not work when the parcel is open.\r\nPlease close the parcel and try again.",
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
      InterpolateZDlg InterpolateZDialog = new InterpolateZDlg(pEditorProps2, pMap);

      if (pMapSpatRef != null)
      {
        if (pMapSpatRef is IProjectedCoordinateSystem2)
        {
          pPCS = (IProjectedCoordinateSystem2)pMapSpatRef;
          string sUnit = pPCS.CoordinateUnit.Name;
          if (sUnit.Contains("Foot") && sUnit.Contains("US"))
            sUnit = "U.S. Feet";
          dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
          if (dMetersPerUnit < 1)
            m_sUnit = "feet";
        }
      }

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedEdit = false;
      IWorkspace pWS = null;
      ITable pParcelsTable = null;
      ITable pLinesTable = null;
      ITable pControlTable = null;
      ITable pPointsTable = null;
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


        //also need to check for a control point selection
        IArray ControlLayerArray;
        if (!Utils.GetFabricSubLayers(ArcMap.Document.ActiveView.FocusMap, esriCadastralFabricTable.esriCFTControl,
           true, pCadEd.CadastralFabric, out ControlLayerArray))
          return;

        // get the line selection; this code sample uses first line layer for the target fabric (first element)
        int iCntControlSelection = 0;
        for (int k = 0; k < ControlLayerArray.Count; k++)
        {
          ISelectionSet2 ControlSelection =
          Utils.GetSelectionFromLayer(ControlLayerArray.get_Element(k) as ICadastralFabricSubLayer);
          iCntControlSelection += ControlSelection.Count;
        }

        if (pCadaSel.SelectedParcelCount == 0 && pSelSet.Count == 0 && iCntLineSelection == 0 && iCntControlSelection == 0)
        {
          MessageBox.Show("Please select some fabric records and try again.", "No Selection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          m_bShowReport = false;
          return;
        }

        ArcMap.Application.CurrentTool = null;

        //Display the dialog
        DialogResult pDialogResult = InterpolateZDialog.ShowDialog();
        if (pDialogResult != DialogResult.OK)
        {
          m_bShowReport = false;
          return;
        }

        bool bClearAllElevation = InterpolateZDialog.optClearElevations.Checked;
        bool bAssignElevations = InterpolateZDialog.optAssignZValues.Checked;
        bool bManualEnteredHeight = (InterpolateZDialog.cboElevationSource.SelectedIndex == 0 && bAssignElevations);
        bool bGetElevationFromTIN = (InterpolateZDialog.cboElevationSource.SelectedIndex == 1 && bAssignElevations);
        bool bGetElevationFromDEM = (InterpolateZDialog.cboElevationSource.SelectedIndex == 2 && bAssignElevations);

        bool bTestElevationDifference = InterpolateZDialog.chkElevationDifference.Checked && InterpolateZDialog.optAssignZValues.Checked;

        double dElevationDiffTest = 0;
        if (bTestElevationDifference)
        {
          dElevationDiffTest = Double.Parse(InterpolateZDialog.txtElevationDifference.Text);
          m_stxtElevDifference = InterpolateZDialog.txtElevationDifference.Text;
        }
        else
          dElevationDiffTest = -999.9;

        double dEllipsoidalHeight = 0;

        bool bPass = false;
        if (bManualEnteredHeight)
        {
          bPass = Double.TryParse(InterpolateZDialog.txtHeightParameter.Text, out dEllipsoidalHeight);
          m_sHeight_Or_ElevationLayer = InterpolateZDialog.txtHeightParameter.Text;
        }
        else if (bGetElevationFromTIN)
        {
          m_sHeight_Or_ElevationLayer = InterpolateZDialog.txtElevationLyr.Text;
          if (InterpolateZDialog.TINLayer == null)
          {
            MessageBox.Show("Please select an elevation source and try again.", "TIN Layer not found",
              MessageBoxButtons.OK, MessageBoxIcon.Information);
            m_bShowReport = false;
            return;
          }
        }
        else if (bGetElevationFromDEM)
        {
          m_sHeight_Or_ElevationLayer = InterpolateZDialog.txtElevationLyr.Text;
          if (InterpolateZDialog.DEMRasterLayer == null)
          {
            MessageBox.Show("Please select an elevation source and try again.", "DEM Layer not found",
              MessageBoxButtons.OK, MessageBoxIcon.Information);
            m_bShowReport = false;
            return;
          }
        }

        m_bShowReport = InterpolateZDialog.chkReportResults.Checked;

        double dSourceMetersPerUnit = 1;
        if (InterpolateZDialog.cboUnits.SelectedIndex == 1) //1=feet
          dSourceMetersPerUnit = 0.3048;

        double dScaleFactor = dSourceMetersPerUnit / dMetersPerUnit;

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
        int tokenLimit = 995;
        List<int> oidList = new List<int>();

        pPointsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
        pLinesTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        pParcelsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
        pControlTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);

        int idxLineCategory = pLinesTable.FindField("CATEGORY");
        string LineCategoryFldName = pLinesTable.Fields.get_Field(idxLineCategory).Name;

        int idxParcelID = pLinesTable.FindField("PARCELID");
        string ParcelIDFldName = pLinesTable.Fields.get_Field(idxParcelID).Name;

        int idxControlPointName = pControlTable.FindField("NAME");
        string ControlNameFldName = pControlTable.Fields.get_Field(idxControlPointName).Name;

        int idxControlPointHeight = pControlTable.FindField("Z");
        string ControlZFldName = pControlTable.Fields.get_Field(idxControlPointHeight).Name;


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
        Dictionary<int, IPoint[]> dict_LinesToInterpolateHeight = new Dictionary<int, IPoint[]>();
        Dictionary<string, double> dict_ZSurfaceSamples = new Dictionary<string, double>();

        Dictionary<int, double> dict_ControlSelection2AttributeHeights = new Dictionary<int, double>();

        List<int> lstPoints = new List<int>();

        m_iExcludedPointCount = 0;
        m_iExcludedControlCount = 0;
        m_iTotalLineCount = m_iTotalParcelCount = m_iTotalControlCount = 0;
        m_dict_DiffToReport.Clear();
        m_dict_ControlName2HeightDiff.Clear();

        m_pFIDSetParcels = new FIDSetClass();
        m_pQF = new QueryFilterClass();

        if (oidList.Count() > 0)
        {
          List<string> sInClauseList0 = Utils.InClauseFromOIDsList(oidList, tokenLimit);
          foreach (string sInClause in sInClauseList0)
          {
            m_pQF.WhereClause = ParcelIDFldName + " IN (" + sInClause + ") AND (" +
                    LineCategoryFldName + " <> 4)";
            if (bClearAllElevation)
            {
              InterpolateZOnLines(m_pQF, pLinesTable, null, -999.9, dScaleFactor, ref dict_LinesToParcel, ref dict_LinesToInterpolateHeight,
                ref dict_ZSurfaceSamples, ref lstPoints, dElevationDiffTest, ref m_dict_DiffToReport, m_pTrackCancel);
              
            }
            else
            {
              if (bGetElevationFromTIN)
              {
                if (dEllipsoidalHeight == -999.9 && InterpolateZDialog.TINLayer == null)
                {
                  MessageBox.Show("Please select an elevation source and try again.", "TIN Layer not found",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                  m_bShowReport = false;
                  return;
                }
                InterpolateZOnLines(m_pQF, pLinesTable, InterpolateZDialog.TINLayer, dEllipsoidalHeight, dScaleFactor, ref dict_LinesToParcel,
                  ref dict_LinesToInterpolateHeight, ref dict_ZSurfaceSamples, ref lstPoints, dElevationDiffTest, ref m_dict_DiffToReport, m_pTrackCancel);
              }
              else if (bGetElevationFromDEM)
              {
                if (dEllipsoidalHeight == -999.9 && InterpolateZDialog.DEMRasterLayer == null)
                {
                  MessageBox.Show("Please select an elevation source and try again.", "DEM Layer not found",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                  m_bShowReport = false;
                  return;
                }
                InterpolateZOnLines(m_pQF, pLinesTable, InterpolateZDialog.DEMRasterLayer, dEllipsoidalHeight, dScaleFactor, ref dict_LinesToParcel,
                  ref dict_LinesToInterpolateHeight, ref dict_ZSurfaceSamples, ref lstPoints, dElevationDiffTest, ref m_dict_DiffToReport, m_pTrackCancel);
              }
              else
                InterpolateZOnLines(m_pQF, pLinesTable, null, dEllipsoidalHeight, dScaleFactor, ref dict_LinesToParcel, ref dict_LinesToInterpolateHeight,
                  ref dict_ZSurfaceSamples, ref lstPoints, dElevationDiffTest, ref m_dict_DiffToReport, m_pTrackCancel);
            }
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
              if (bGetElevationFromTIN)
                InterpolateZOnLines(m_pQF, pLinesTable, InterpolateZDialog.TINLayer, dEllipsoidalHeight, dScaleFactor, ref dict_LinesToParcel,
                  ref dict_LinesToInterpolateHeight, ref dict_ZSurfaceSamples, ref lstPoints, dElevationDiffTest, ref m_dict_DiffToReport, m_pTrackCancel);
              else if (bGetElevationFromDEM)
                InterpolateZOnLines(m_pQF, pLinesTable, InterpolateZDialog.DEMRasterLayer, dEllipsoidalHeight, dScaleFactor, ref dict_LinesToParcel,
                  ref dict_LinesToInterpolateHeight, ref dict_ZSurfaceSamples, ref lstPoints, dElevationDiffTest, ref m_dict_DiffToReport, m_pTrackCancel);
              else
                InterpolateZOnLines(m_pQF, pLinesTable, null, dEllipsoidalHeight, dScaleFactor, ref dict_LinesToParcel, ref dict_LinesToInterpolateHeight,
                  ref dict_ZSurfaceSamples, ref lstPoints, dElevationDiffTest, ref m_dict_DiffToReport, m_pTrackCancel); 

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


        if (iCntControlSelection > 0 & !bClearAllElevation)
        {
          ICursor pCursor;
          //if there's a control selection, then grab the control point names
          m_pQF.WhereClause = "";//pControlTable.Fields.get_Field(idx).Name + " = 0";
          for (int k = 0; k < ControlLayerArray.Count; k++)
          {
            ISelectionSet2 ControlSelection =
                Utils.GetSelectionFromLayer(ControlLayerArray.get_Element(k) as ICadastralFabricSubLayer);
            ControlSelection.Search(m_pQF, false, out pCursor);
            IRow pRow = pCursor.NextRow();
            while (pRow != null)
            {
              if (!dict_ControlSelection2AttributeHeights.ContainsKey(pRow.OID))
              {
                double dCurrentControlHeight = 0;
                object obj = pRow.get_Value(idxControlPointHeight);
                if (obj != DBNull.Value)
                  dCurrentControlHeight = (double)obj;

                double dIncomingHeight = dEllipsoidalHeight;
                IPoint pControlLocation = (pRow as IFeature).Shape as IPoint;

                if (bGetElevationFromDEM || bGetElevationFromTIN)
                {
                  ILayer SurfaceLayer = null;
                  if (bGetElevationFromDEM)
                    SurfaceLayer = InterpolateZDialog.DEMRasterLayer;
                  else if (bGetElevationFromTIN)
                    SurfaceLayer = InterpolateZDialog.TINLayer;

                  if (!Utils.GetElevationAtLocationOnSurface(SurfaceLayer, pControlLocation, out dIncomingHeight))
                  {
                    Marshal.ReleaseComObject(pRow);
                    pRow = pCursor.NextRow();
                    continue;
                  }
                  if (dIncomingHeight == 0)
                  {
                    Marshal.ReleaseComObject(pRow);
                    pRow = pCursor.NextRow();
                    continue;                
                  }
                }

                dIncomingHeight = Math.Round(dIncomingHeight * dScaleFactor, 4);

                string sControlName = " ";
                obj = pRow.get_Value(idxControlPointName);
                if (obj != DBNull.Value)
                  sControlName = (string)obj;


                if (bTestElevationDifference)
                {
                  if (Math.Abs(dIncomingHeight - dCurrentControlHeight) >= dElevationDiffTest)
                  {
                    dict_ControlSelection2AttributeHeights.Add(pRow.OID, dIncomingHeight);
                    m_dict_ControlName2HeightDiff.Add(sControlName, (dIncomingHeight - dCurrentControlHeight));
                  }
                  else
                    m_iExcludedControlCount++;
                }
                else
                  dict_ControlSelection2AttributeHeights.Add(pRow.OID, dIncomingHeight);

              }
              Marshal.ReleaseComObject(pRow);
              pRow = pCursor.NextRow();
            }
            Marshal.ReleaseComObject(pCursor);
          }
        
        }

        m_sParcelCount = (oidList.Count + m_ParcelsWithLinesOnlyListCount).ToString();

        if (m_pFIDSetParcels.Count() == 0 && dict_ControlSelection2AttributeHeights.Count == 0)
        {
          m_bNoUpdates = true;
          m_sReport += sUnderline + "No records were updated.";
          return;
        }

        int[] pParcelIds = new int[m_pFIDSetParcels.Count()];

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

        ILongArray pTempParcelsLongArray = new LongArrayClass();
        List<int> lstParcelChanges = Utils.FIDsetToLongArray(m_pFIDSetParcels, ref pTempParcelsLongArray, ref pParcelIds, m_pStepProgressor);

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
        pSchemaEd.ReleaseReadOnlyFields(pControlTable, esriCadastralFabricTable.esriCFTControl);
        if (dict_ControlSelection2AttributeHeights.Count > 0)
        {
          List<int> lstControlIds = new List<int>(dict_ControlSelection2AttributeHeights.Keys);
          List<string> sInClauseList3 = Utils.InClauseFromOIDsList(lstControlIds, tokenLimit);
          foreach (string sInClause in sInClauseList3)
          {
            m_pQF.WhereClause = pControlTable.OIDFieldName + " IN (" + sInClause + ")";
            if(!UpdateControlZsFromDictionary(pControlTable, m_pQF, bIsUnVersioned, ref dict_ControlSelection2AttributeHeights, m_pStepProgressor, m_pTrackCancel))
            {
              if (m_bShowProgressor && m_pTrackCancel != null)
                if (m_bShowReport)
                  m_bShowReport = m_pTrackCancel.Continue();
              m_bNoUpdates = true;
              m_sReport += sUnderline + "ERROR occurred updating elevations on Control. No records were updated.";
              AbortEdits(bIsUnVersioned, m_pEd, pWS);
              pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
              return;
            }
          }
        }

        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set fields back to read-only
        
        if (m_pFIDSetParcels.Count() > 0)
        {
          pSchemaEd.ReleaseReadOnlyFields(pLinesTable, esriCadastralFabricTable.esriCFTLines); //release read-only fields

          List<int> lstOidsOfLines = dict_LinesToInterpolateHeight.Keys.ToList<int>(); //linq
          List<string> sInClauseList2 = Utils.InClauseFromOIDsList(lstOidsOfLines, tokenLimit);

          if (m_bShowProgressor)
            m_pStepProgressor.Message = "Updating Z values ...";

          //now go through the lines to update
          foreach (string sInClause in sInClauseList2)
          {
            m_pQF.WhereClause = pLinesTable.OIDFieldName + " IN (" + sInClause + ")";
            if (!UpdateFeatureZsByDictionaryLookups(pLinesTable, m_pQF, bClearAllElevation, dElevationDiffTest * dScaleFactor, bIsUnVersioned, 
              dict_LinesToInterpolateHeight, m_pStepProgressor, m_pTrackCancel))
            {
              if (m_bShowProgressor && m_pTrackCancel != null)
                if (m_bShowReport)
                  m_bShowReport = m_pTrackCancel.Continue();
              m_bNoUpdates = true;
              m_sReport += sUnderline + "ERROR occurred updating elevations on records. No records were updated.";
              AbortEdits(bIsUnVersioned, m_pEd, pWS);
              pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on
              return;
            }
          }

          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set fields back to read-only

          pSchemaEd.ReleaseReadOnlyFields(pPointsTable, esriCadastralFabricTable.esriCFTPoints);

          List<int> lstCenterPointIds = new List<int>();

          if (lstPoints.Count > 0)
          {
            List<string> sInClauseList0 = Utils.InClauseFromOIDsList(lstPoints, tokenLimit);
            foreach (string sInClause in sInClauseList0)
            {
              m_pQF.WhereClause = pPointsTable.OIDFieldName + " IN (" + sInClause + ")";

              if (!UpdatePointZsBySurfaceSamples(pPointsTable, m_pQF, bIsUnVersioned, ref lstCenterPointIds, dict_ZSurfaceSamples, 
                ref pSchemaEd, m_pStepProgressor, m_pTrackCancel))
              {
                if (m_bShowProgressor && m_pTrackCancel != null)
                  if (m_bShowReport)
                    m_bShowReport = m_pTrackCancel.Continue();
                m_bNoUpdates = true;
                m_sReport += sUnderline + "ERROR occurred updating elevations on records. No records were updated.";
                AbortEdits(bIsUnVersioned, m_pEd, pWS);
                pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);//set safety back on
                return;
              }
            }
          }

          //The center point locations are not interpolated since the interpolation depends on line geometry.
          //The center points are updated after interpolating and comparing with existing Z value
          if (lstCenterPointIds.Count > 0) 
          {
            List<string> sInClauseList0 = Utils.InClauseFromOIDsList(lstCenterPointIds, tokenLimit);
            foreach (string sInClause in sInClauseList0)
            {
              m_pQF.WhereClause = pPointsTable.OIDFieldName + " IN (" + sInClause + ")";

              ILayer SurfaceLayer = null;
              if (bGetElevationFromDEM)
                SurfaceLayer = InterpolateZDialog.DEMRasterLayer;
              else if (bGetElevationFromTIN)
                SurfaceLayer = InterpolateZDialog.TINLayer;

              if (!bManualEnteredHeight)
                dEllipsoidalHeight = -999.9;

              if (!bTestElevationDifference)
                dElevationDiffTest = -999.9;

              if (!UpdatePointZsDirectFromSurface(pPointsTable, m_pQF, bIsUnVersioned, SurfaceLayer, dElevationDiffTest, ref m_dict_DiffToReport, dEllipsoidalHeight, dScaleFactor, 
                ref pSchemaEd, m_pStepProgressor, m_pTrackCancel))
              {
                if (m_bShowProgressor && m_pTrackCancel != null)
                  if (m_bShowReport)
                    m_bShowReport = m_pTrackCancel.Continue();
                m_bNoUpdates = true;
                m_sReport += sUnderline + "ERROR occurred updating elevations on records. No records were updated.";
                AbortEdits(bIsUnVersioned, m_pEd, pWS);
                pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);//set safety back on
                return;
              }
            }
          }

          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);//set fields back to read-only

          lstOidsOfLines.Clear();
          lstPoints.Clear();

          pSchemaEd.ReleaseReadOnlyFields(pParcelsTable, esriCadastralFabricTable.esriCFTParcels);

          if (lstParcelChanges.Count > 0)
          {
            List<string> sInClauseList1 = Utils.InClauseFromOIDsList(lstParcelChanges, tokenLimit);
            foreach (string sInClause in sInClauseList1)
            {
              m_pQF.WhereClause = pParcelsTable.OIDFieldName + " IN (" + sInClause + ")";

              if (!UpdateFeatureZsBySurfaceSamples(pParcelsTable, m_pQF, dElevationDiffTest * dScaleFactor, bIsUnVersioned, dict_ZSurfaceSamples, m_pStepProgressor, m_pTrackCancel))
              {
                if (m_bShowProgressor && m_pTrackCancel != null)
                  if (m_bShowReport)
                    m_bShowReport = m_pTrackCancel.Continue();
                m_bNoUpdates = true;
                m_sReport += sUnderline + "ERROR occurred updating elevations on records. No records were updated.";
                AbortEdits(bIsUnVersioned, m_pEd, pWS);
                pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);//set fields back to read-only
                return;
              }
            }
          }

          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);//set fields back to read-only

          #region Regenerate Fabric
        if (pParcelIds.Count() > 0)
        {

          //IFIDSet pRegenIds = new FIDSetClass();
          //foreach (int j in pParcelIds)
          //  pRegenIds.Add(j);

          //ICadastralFabricRegeneration pRegenFabric = new CadastralFabricRegenerator();
          //#region regenerator enum
          //// enum esriCadastralRegeneratorSetting 
          //// esriCadastralRegenRegenerateGeometries         =   1 
          //// esriCadastralRegenRegenerateMissingRadials     =   2, 
          //// esriCadastralRegenRegenerateMissingPoints      =   4, 
          //// esriCadastralRegenRemoveOrphanPoints           =   8, 
          //// esriCadastralRegenRemoveInvalidLinePoints      =   16, 
          //// esriCadastralRegenSnapLinePoints               =   32, 
          //// esriCadastralRegenRepairLineSequencing         =   64, 
          //// esriCadastralRegenRepairPartConnectors         =   128  
          //// By default, the bitmask member is 0 which will only regenerate geometries. 
          //// (equivalent to passing in regeneratorBitmask = 1) 
          //#endregion
          //pRegenFabric.CadastralFabric = pCadFabric;
          //pRegenFabric.RegeneratorBitmask =  7 + 64 + 128;
          //if (m_pStepProgressor != null)
          //  m_pStepProgressor.Message = "Regenerating " + pRegenIds.Count().ToString() + " parcels...";
          //pRegenFabric.RegenerateParcels(pRegenIds, false, m_pTrackCancel); 


          // instead of requiring a parcel regen (that avoids flattening of circular arcs), 
          //research changing GeomBridge.ReplacePoints to GeomBridge.ReplaceSegments
          //Current solution: use updatepoint on the pointcollection instead of the geometry bridge replace points.
          //the documentation indicates geometry bridge it's not needed for update points.
        }
        #endregion
        }

        if (m_iTotalPointCount > 0 || m_iTotalLineCount > 0 || m_iTotalControlCount > 0)
          m_pEd.StopOperation("Interpolate elevations on " + m_iTotalPointCount.ToString() + " points." + Environment.NewLine +
            m_iTotalLineCount.ToString() + " lines." + Environment.NewLine + m_iTotalControlCount.ToString() + " control points.");
        else
          m_pEd.AbortOperation();

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
            if (m_stxtElevDifference != null)
              m_sReport += sUnderline + "Excluded " + m_iExcludedPointCount.ToString() + " points with elevation differences less than " + m_stxtElevDifference + " " + m_sUnit + ".";

            if (m_stxtElevDifference != null)
              m_sReport += Environment.NewLine + "Excluded " + m_iExcludedControlCount.ToString() + " control points with elevation differences less than " + m_stxtElevDifference + " " + m_sUnit + ".";

            m_sReport += sUnderline + m_iTotalParcelCount.ToString() + " parcel(s) updated.";
            m_sReport += Environment.NewLine + m_iTotalLineCount.ToString() + " line elevation(s) interpolated.";
            m_sReport += Environment.NewLine + m_iTotalPointCount.ToString() + " point elevation(s) interpolated.";
            m_sReport += Environment.NewLine + m_iTotalControlCount.ToString() + " control point elevation(s) interpolated.";

            if (m_sEntryMethod != null)
              m_sReport += m_sEntryMethod + m_sHeight_Or_ElevationLayer;


            if (m_dict_ControlName2HeightDiff.Count > 0)
              m_sReport += sUnderline + "Control Name\tDifference (" + m_sUnit + ")" + Environment.NewLine + "\t\t" + "new Z - initial Z" + sUnderline;

            //list sorted by distance difference
            var sortedDict0 = from entry in m_dict_ControlName2HeightDiff orderby entry.Value descending select entry;
            var pEnum0 = sortedDict0.GetEnumerator();
            while (pEnum0.MoveNext())
            {
              var pair = pEnum0.Current;
              m_sReport += String.Format("   {0,-15}\t{1,12:0.000}", pair.Key.ToString(), m_dict_ControlName2HeightDiff[pair.Key])
                + Environment.NewLine;

              if (m_bShowProgressor)
              {
                pProgressorDialog.CancelEnabled = false;
                if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                  m_pStepProgressor.Step();
              }
            }

            //if (m_dict_ControlName2HeightDiff.Count == 0)
            //  m_sReport += sUnderline;

            if (m_dict_DiffToReport.Count > 0 && m_dict_ControlName2HeightDiff.Count == 0)
              m_sReport += sUnderline + "Point OID\t\tDifference (" + m_sUnit + ")" + Environment.NewLine + "\t\t" + "new Z - initial Z" + sUnderline;
            else if(m_dict_DiffToReport.Count > 0 && m_dict_ControlName2HeightDiff.Count > 0)
              m_sReport += Environment.NewLine + "Point OID" + sUnderline;

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
        }

      }
      catch(Exception ex)
      {
        if (m_pEd != null)
          AbortEdits(bIsUnVersioned, m_pEd, pWS);
        m_sReport += sUnderline + "Error:  " + ex.Message;
        m_bNoUpdates = true;
        MessageBox.Show(ex.Message);
      }
      finally
      {
        m_iTotalParcelCount = m_iTotalLineCount = m_iTotalPointCount = m_iTotalControlCount = m_iExcludedPointCount = 0;
        m_sEntryMethod = m_sLineCount = m_sParcelCount = m_sHeight_Or_ElevationLayer = m_stxtElevDifference = null;

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


    public bool UpdateFeatureZsByDictionaryLookups(ITable TheTable, IQueryFilter QueryFilter, bool ClearZs, double HeightDifferenceTolerance, bool Unversioned, Dictionary<int, IPoint[]> LookupLines, 
       IStepProgressor pStepProgressor, ITrackCancel pTrackCancel)
    {
      try
      {
        bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

        IRow pTheFeatRow = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
        {
          ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        }
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeatRow = pUpdateCursor.NextRow();
        
        bool bCont = true;
        IGeometryBridge2 GeomBridge = new GeometryEnvironmentClass();

        while (pTheFeatRow != null)
        {
          //Check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
          {
            bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
          }

          //loop through all of the features, lookup the object id, then write the shape with new z values to the 
          //feature's geometry
          
          IPoint[] TheUpdatePointArray = LookupLines[pTheFeatRow.OID];
          IFeature pTheFeat = pTheFeatRow as IFeature;
          IPointCollection4 pPointColl = pTheFeat.Shape as IPointCollection4;
          
          bool bUpdate = false;

          int iPtCnt = pPointColl.PointCount;
          for (int i = 0; i < iPtCnt; i++)
          {
            IPoint pPoint=pPointColl.get_Point(i);
            IPoint[] replacePointArray = new IPoint[1]; //just 1 point at a time, to avoid error on multi-parts

            double dZ = 0;

            if (!ClearZs)
            {
              dZ = GetZFromXYZPointArray(TheUpdatePointArray, pPoint, 0.01);
              if (Double.IsNaN(dZ))
                dZ = 0;
            }

            if (Math.Abs(pPoint.Z - dZ) > HeightDifferenceTolerance)
              bUpdate = true;

            IPoint pUpdatedPoint = new PointClass();
            pUpdatedPoint.X = pPoint.X;
            pUpdatedPoint.Y = pPoint.Y;
            pUpdatedPoint.Z = dZ;

            replacePointArray[0] = pUpdatedPoint;
            //GeomBridge.ReplacePoints(pPointColl, i, 1, ref replacePointArray); //this flattens curve segments, call regenerate on parcels to restore curve.
            //also research using replacesegments method instead. Regenerate fabric drops z's on parcel polygons.

            pPointColl.UpdatePoint(i, pUpdatedPoint);

          }

          if (bUpdate)
          {
            pTheFeat.Shape = pPointColl as IGeometry;
            pTheFeat.Store();
            if (pTheFeat.Shape is IPolygon)
              m_iTotalParcelCount++;
            else
              m_iTotalLineCount++;
          }

          string sGeomType = " line ";
          if (pTheFeat.Shape is IPolygon)
            sGeomType = " parcel ";
          if (bShowProgressor)
          {
            if (pStepProgressor.Position < pStepProgressor.MaxRange)
              pStepProgressor.Step();
            else
              pStepProgressor.Message = "Updating"+ sGeomType +"(id): " + pTheFeat.OID.ToString();
          }

          Marshal.ReleaseComObject(pTheFeatRow); //garbage collection
          pTheFeatRow = pUpdateCursor.NextRow();
        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return bCont;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating Elevation on feature: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }
    

    public bool UpdateFeatureZsBySurfaceSamples(ITable TheTable, IQueryFilter QueryFilter, double HeightDifferenceTolerance, bool Unversioned, Dictionary<string, double> SurfaceSamplePoints, 
      IStepProgressor pStepProgressor, ITrackCancel pTrackCancel)
    {
      string sFeatOID = "";
      try
      {
        bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

        IRow pTheFeatRow = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
        {
          ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        }
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeatRow = pUpdateCursor.NextRow();

        bool bCont = true;

        IGeometryBridge2 GeomBridge = new GeometryEnvironmentClass();

        while (pTheFeatRow != null)
        {
          //Check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
          {
            bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
          }
          sFeatOID = pTheFeatRow.OID.ToString();
          //loop through all of the features, lookup the Z from the sampled surface, then write the shape with new z values to the 
          //feature's geometry
          IFeature pTheFeat = pTheFeatRow as IFeature;
          IPointCollection4 pPointColl = pTheFeat.Shape as IPointCollection4;
          int iPtCnt = pPointColl.PointCount;
          bool bUpdate = false;
          for (int i = 0; i < iPtCnt; i++)
          {
            IPoint pPoint = pPointColl.get_Point(i);

            IPoint[] replacePointArray = new IPoint[1]; //just 1 point at a time, to avoid error on multi-parts

            double dZ =0;
            string NamedSampleLocation = PointXYAsSingleIntegerInterleave(pPoint, 2);
            if (SurfaceSamplePoints.ContainsKey(NamedSampleLocation))
            {
              bUpdate = true; //at least one point on the geometry has new update data and needs to be changed
              dZ = SurfaceSamplePoints[NamedSampleLocation];
            }
            if (Double.IsNaN(dZ))
              dZ = 0;
            IPoint pUpdatedPoint = new PointClass();
            pUpdatedPoint.X = pPoint.X;
            pUpdatedPoint.Y = pPoint.Y;
            pUpdatedPoint.Z = dZ;

            replacePointArray[0] = pUpdatedPoint;
            //GeomBridge.ReplacePoints(pPointColl, i, 1, ref replacePointArray);

            pPointColl.UpdatePoint(i, pUpdatedPoint);

          }

          if (bUpdate)
          {
            pTheFeat.Shape = pPointColl as IGeometry;
            pTheFeat.Store();

            if (pTheFeat.Shape is IPolygon)
              m_iTotalParcelCount++;
            else
              m_iTotalLineCount++;

          }

          string sGeomType = " line ";
          if (pTheFeat.Shape is IPolygon)
            sGeomType = " parcel ";
          if (bShowProgressor)
          {
            if (pStepProgressor.Position < pStepProgressor.MaxRange)
              pStepProgressor.Step();
            else
              pStepProgressor.Message = "Updating" + sGeomType + "(id): " + pTheFeat.OID.ToString();
          }

          Marshal.ReleaseComObject(pTheFeatRow); //garbage collection
          pTheFeatRow = pUpdateCursor.NextRow();

        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return bCont;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating Elevation on feature: " + Convert.ToString(ex.ErrorCode) + Environment.NewLine +
          "OID: " + sFeatOID + Environment.NewLine + ex.Message);
        return false;
      }
    }


    public bool UpdatePointZsDirectFromSurface(ITable TheTable, IQueryFilter QueryFilter, bool Unversioned, ILayer SurfaceLayer, double HeightDifferenceTolerance, ref Dictionary<int, double> dictElevDifference,
      double dEllipsoidalHeightFromSource, double ScaleFactor, ref ICadastralFabricSchemaEdit2 SchemaEdit, IStepProgressor pStepProgressor, ITrackCancel pTrackCancel)
    {
      try
      {
        bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

        bool bCompareHeightDifference = HeightDifferenceTolerance != -999.9;

        int idxPointZField = TheTable.FindField("Z");

        IRow pTheFeatRow = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
        {
          ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        }
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeatRow = pUpdateCursor.NextRow();

        bool bCont = true;
        Utilities Utils = new Utilities();

        while (pTheFeatRow != null)
        {
          //Check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
          {
            bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
          }

          //loop through all of the features, lookup the object id, then write the shape with new z values to the 
          //feature's geometry
          
          IFeature pTheFeat = pTheFeatRow as IFeature;
          IPoint pPoint = pTheFeat.Shape as IPoint;

          double dZ = dEllipsoidalHeightFromSource;
          if (Utils.GetElevationAtLocationOnSurface(SurfaceLayer, pPoint, out dZ))
            dZ = Math.Round(dZ * ScaleFactor,4);

          if (dZ < -3000) //keep null flag as -999.9
            dZ = -999.9;

          bool bDoTheWork = false;

          if (bCompareHeightDifference)
          {
            if (Math.Abs(dZ - pPoint.Z) > HeightDifferenceTolerance)
            {
              double dDiff = dZ - pPoint.Z;
              if (pPoint.Z < -999)
                dDiff = dZ;

              if (bCompareHeightDifference)
              {
                try { dictElevDifference.Add(pTheFeat.OID, dDiff); } //Note: getting point id based on geom direction on line.
                catch { }
              }

              bDoTheWork = true;
            }
            else
            {
              m_iExcludedPointCount++;
              bDoTheWork = false; 
            }
          }
          else
            bDoTheWork = true;

          if (bDoTheWork)
          {
            IPoint pUpdatedPoint = new PointClass();
            IZAware pZAw = pUpdatedPoint as IZAware;
            pZAw.ZAware = true;
            pUpdatedPoint.X = pPoint.X;
            pUpdatedPoint.Y = pPoint.Y;
            if (dZ == -999.9)
              pUpdatedPoint.Z = 0;
            else
              pUpdatedPoint.Z = dZ;

            if (dZ == -999.9)
              pTheFeat.set_Value(idxPointZField, DBNull.Value);
            else
              pTheFeat.set_Value(idxPointZField, dZ);

            pTheFeat.Shape = pUpdatedPoint as IGeometry;
            m_iTotalPointCount++;
            pTheFeat.Store();
          }

          if (bShowProgressor)
          {
            if (pStepProgressor.Position < pStepProgressor.MaxRange)
              pStepProgressor.Step();
            else
              pStepProgressor.Message = "Updating point (id): " + pTheFeat.OID.ToString();
          }

          Marshal.ReleaseComObject(pTheFeatRow); //garbage collection
          pTheFeatRow = pUpdateCursor.NextRow();
        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return bCont;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating Elevation on feature: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }


    public bool UpdateControlZsFromDictionary(ITable TheTable, IQueryFilter QueryFilter, bool Unversioned, ref Dictionary<int, double> dictControlToHeights,  IStepProgressor pStepProgressor, ITrackCancel pTrackCancel)
    {
      try
      {
        bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

        int idxPointZField = TheTable.FindField("Z");

        IRow pTheFeatRow = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
        {
          ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        }
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeatRow = pUpdateCursor.NextRow();

        bool bCont = true;

        while (pTheFeatRow != null)
        {
          //Check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
          {
            bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
          }

          //loop through all of the features, lookup the object id, then write the shape with new z values to the 
          //feature's geometry

          IFeature pTheFeat = pTheFeatRow as IFeature;
          IPoint pPoint = pTheFeat.Shape as IPoint;

          IPoint pUpdatedPoint = new PointClass();
          IZAware pZAw = pUpdatedPoint as IZAware;
          pZAw.ZAware = true;
          pUpdatedPoint.X = pPoint.X;
          pUpdatedPoint.Y = pPoint.Y;
          pUpdatedPoint.Z = dictControlToHeights[pTheFeat.OID];

          pTheFeat.set_Value(idxPointZField, dictControlToHeights[pTheFeat.OID]);

          pTheFeat.Shape = pUpdatedPoint as IGeometry;
          m_iTotalControlCount++;
          pTheFeat.Store();
          
          if (bShowProgressor)
          {
            if (pStepProgressor.Position < pStepProgressor.MaxRange)
              pStepProgressor.Step();
            else
              pStepProgressor.Message = "Updating control (id): " + pTheFeat.OID.ToString();
          }

          Marshal.ReleaseComObject(pTheFeatRow); //garbage collection
          pTheFeatRow = pUpdateCursor.NextRow();
        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return bCont;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating Elevation on control: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }



    public bool UpdatePointZsBySurfaceSamples(ITable TheTable, IQueryFilter QueryFilter, bool Unversioned, ref List<int> lstCenterPointIds, Dictionary<string, double> SurfaceSamplePoints,
  ref ICadastralFabricSchemaEdit2 SchemaEdit, IStepProgressor pStepProgressor, ITrackCancel pTrackCancel)
    {
      string sFeatOID = "";
      try
      {
        bool bShowProgressor = (pStepProgressor != null && pTrackCancel != null);

        int idxPointZField = TheTable.FindField("Z");
        int idxCenterPoint = TheTable.FindField("CENTERPOINT");

        IRow pTheFeatRow = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
        {
          ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        }
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeatRow = pUpdateCursor.NextRow();

        bool bCont = true;

        while (pTheFeatRow != null)
        {
          //Check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
          {
            bCont = pTrackCancel.Continue();
            if (!bCont)
              break;
          }

          //loop through all of the features, lookup the Z from the sampled surface, then write the shape with new z values to the 
          //feature's geometry
          sFeatOID = pTheFeatRow.OID.ToString();
          IFeature pTheFeat = pTheFeatRow as IFeature;
          IPoint pPoint = pTheFeat.Shape as IPoint;

          object obj = pTheFeatRow.get_Value(idxCenterPoint);
          if (obj != DBNull.Value)
          {
            if ((int)obj == 1)
            {
              int iOID = pTheFeat.OID;
              if (!lstCenterPointIds.Contains(iOID))
                lstCenterPointIds.Add(iOID);
            }
          }
          else
          {
            string NamedSampleLocation = PointXYAsSingleIntegerInterleave(pPoint, 2);
            if (SurfaceSamplePoints.ContainsKey(NamedSampleLocation))
            {
              double dZ = SurfaceSamplePoints[NamedSampleLocation];
              IPoint pUpdatedPoint = new PointClass();
              IZAware pZAw = pUpdatedPoint as IZAware;
              pZAw.ZAware = true;
              pUpdatedPoint.X = pPoint.X;
              pUpdatedPoint.Y = pPoint.Y;
              pUpdatedPoint.Z = dZ;
              if (Double.IsNaN(dZ))
              {
                pUpdatedPoint.Z = 0; //NaN z values cause error on geometry store
                pTheFeat.set_Value(idxPointZField, DBNull.Value);
              }
              else
                pTheFeat.set_Value(idxPointZField, dZ);
              pTheFeat.Shape = pUpdatedPoint as IGeometry;

              m_iTotalPointCount++;
              pTheFeat.Store();
            }
            else
              m_iExcludedPointCount++;
          }

          if (bShowProgressor)
          {
            if (pStepProgressor.Position < pStepProgressor.MaxRange)
              pStepProgressor.Step();
            else
              pStepProgressor.Message = "Updating point (id): " + pTheFeat.OID.ToString();
          }

          Marshal.ReleaseComObject(pTheFeatRow); //garbage collection
          pTheFeatRow = pUpdateCursor.NextRow();
        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return bCont;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating Elevation on feature: " + Convert.ToString(ex.ErrorCode) + Environment.NewLine +
          "OID: " + sFeatOID + Environment.NewLine + ex.Message);
        return false;
      }
    }


    double GetZFromXYZPointArray(IPoint[] TheUpdatePointArray, IPoint SourcePoint, double dTol)
    {
      double dX = SourcePoint.X;
      double dY = SourcePoint.Y;
      int iCnt = TheUpdatePointArray.Count();
      for (int i=0; i < iCnt; i++)
      {
        IPoint pPt = TheUpdatePointArray[i];
        if ((Math.Abs(pPt.X - dX) < dTol) &&  (Math.Abs(pPt.Y - dY) < dTol))
        {
          return pPt.Z;
        }
      }
      return -999.9;
    }

    private void InterpolateZOnLines(IQueryFilter m_pQF, ITable pLinesTable, ILayer SurfaceLayer, double dEllipsoidalHeight, double ScaleFactor,
      ref Dictionary<int, int> dict_LinesToParcel, ref Dictionary<int, IPoint[]> dict_LinesToInterpolateHeight,
      ref Dictionary<string, double> dict_ZSurfaceSamples, ref List<int> lst_Points, double HeightDifferenceTolerance, ref Dictionary<int, double> dictElevDifference, ITrackCancel pTrackCancel)
    {
      bool bTrackCancel = (pTrackCancel != null);

      bool bCompareHeightDifference = HeightDifferenceTolerance != -999.9;

      int idxParcelID = pLinesTable.FindField("PARCELID");
      int idxFromPointID = pLinesTable.FindField("FROMPOINTID");
      int idxToPointID = pLinesTable.FindField("TOPOINTID");
      int idxCenterPtId = pLinesTable.FindField("CENTERPOINTID");

      string ParcelIDFldName = pLinesTable.Fields.get_Field(idxParcelID).Name;

      ILine pLine = new LineClass();

      ICursor pCursor = pLinesTable.Search(m_pQF, false);
      IRow pLineRecord = pCursor.NextRow();
      Utilities Utils = new Utilities();

      //for each line record
      while (pLineRecord != null)
      {
        IFeature pFeat = (IFeature)pLineRecord;
        IGeometry pGeom = pFeat.Shape;
        IZAware pZAw = pGeom as IZAware;
        int fromPointId = (int)pFeat.get_Value(idxFromPointID);
        int toPointId = (int)pFeat.get_Value(idxToPointID);

        if (!lst_Points.Contains(fromPointId))
          lst_Points.Add(fromPointId);

        if (!lst_Points.Contains(toPointId))
          lst_Points.Add(toPointId);
        
        object obj = pFeat.get_Value(idxCenterPtId);
        if (obj != DBNull.Value)
        {
          int ctrPointId = Convert.ToInt32(obj);
          if (!lst_Points.Contains(ctrPointId))
            lst_Points.Add(ctrPointId);
        }

        if (pGeom != null)
        {
          if (!pGeom.IsEmpty)
          {
            if (pZAw.ZAware)
            {
              IPointCollection4 pPointColl = (IPointCollection4)pGeom;
              int iCnt = pPointColl.PointCount;
              IPoint[] pPointArr = new IPoint[iCnt];
              for (int j = 0; j < iCnt; j++)
                pPointArr[j] = pPointColl.get_Point(j);

              int parcelId = (int)pFeat.get_Value(idxParcelID);
              bool bExists;
              m_pFIDSetParcels.Find(parcelId, out bExists);
              if (!bExists)
                m_pFIDSetParcels.Add(parcelId);

              for (int i = 0; i < iCnt; i++)
              {
                IPoint pPt1 = pPointArr[i];

                double dZ = dEllipsoidalHeight;
                if (Utils.GetElevationAtLocationOnSurface(SurfaceLayer, pPt1, out dZ))
                  dZ = Math.Round(dZ * ScaleFactor,4);

                if (dZ < -3000) //keep null flag as -999.9
                  dZ = -999.9;

                string s = PointXYAsSingleIntegerInterleave(pPt1, 2);

                if (bCompareHeightDifference)
                {
                  if (Math.Abs(dZ - pPt1.Z) > HeightDifferenceTolerance)
                  {
                    double dDiff = dZ - pPt1.Z;
                    if (pPt1.Z < -999)
                      dDiff = dZ;

                    int iOID = -1;
                    if (i == 0) iOID = fromPointId;
                    else iOID = toPointId;

                    if (bCompareHeightDifference)
                    {
                      try { dictElevDifference.Add(iOID, dDiff); } //Note: getting point id based on geom direction on line.
                      catch { }
                    }

                    pPt1.Z = dZ;
                    pPointArr[i] = pPt1;
                    try { dict_ZSurfaceSamples.Add(s, dZ); } catch { } //will error frequently because of repeat coordinates
                  }
                }
                else
                {
                  if (dZ == -999.9)
                    pPt1.Z=Double.NaN;
                  else
                    pPt1.Z = dZ;
                  pPointArr[i] = pPt1;
                  try { dict_ZSurfaceSamples.Add(s, pPt1.Z); } catch { } //will error frequently because of repeat coordinates
                }
              }

              dict_LinesToInterpolateHeight.Add(pFeat.OID, pPointArr);
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

    private string PointXYAsSingleIntegerInterleave(IPoint point, int iPrecision)
    {
      //string sZeroFormat = new String('#', iPrecision);
      string sZeroFormat = new String('0', iPrecision);
      string sFormat1Precision = "0." + sZeroFormat;
      string sX = point.X.ToString(sFormat1Precision);
      string sY = point.Y.ToString(sFormat1Precision);

      int iLength = sX.Length > sY.Length ? sX.Length : sY.Length;

      if (sX.Length < iLength)
        sX = sX.PadLeft(iLength, '0');

      if (sY.Length < iLength)
        sY = sY.PadLeft(iLength, '0');

      Char[] chrArrayX = sX.ToCharArray();
      Char[] chrArrayY = sY.ToCharArray();


      //string sFormat = new String('0', iLength-iPrecision-1) + "." + sZeroFormat;

      //Char[] chrArrayX = point.X.ToString(sFormat).ToCharArray();
      //Char[] chrArrayY = point.Y.ToString(sFormat).ToCharArray();

      char[] chars = new char[iLength * 2];
      for (int i = 0; i < sX.Length; i++)
      {
        chars[i * 2] = chrArrayX[i];
        chars[i * 2 + 1] = chrArrayY[i];
      }
      string sInterleaved = new string(chars);

      sInterleaved = sInterleaved.Replace("..", "");

      return sInterleaved;

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
  
  }
}
