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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;
using Microsoft.Win32;

namespace FabricPointMoveToFeature
{
  public class MovePointsToTarget : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public MovePointsToTarget()
    {
    }
    LayerManager ext_LyrMan = null;
    private string m_sReport;
    private string sUnderline = Environment.NewLine + "-----------------------------------------------------------------------------------" + Environment.NewLine;

    protected override void OnClick()
    {
      //get the Cadastral Extension
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
 
      //get the parcel fabric
      ICadastralFabric pFab = pCadEd.CadastralFabric;

      //get minimum merge tolerance
      string sTemp;
      double dMetersPerUnit;
      double dMinimumMergeTolerance = GetMaxShiftThreshold(pFab); //this is always in meters, so convert to fabric units:
      dMinimumMergeTolerance = ConvertDistanceFromMetersToFabricUnits(dMinimumMergeTolerance, pFab, out sTemp, out dMetersPerUnit);

      if (!ext_LyrMan.MergePoints)
        ext_LyrMan.MergePointTolerance = dMinimumMergeTolerance;

      ext_LyrMan.MergePointTolerance = ext_LyrMan.MergePointTolerance < dMinimumMergeTolerance ? dMinimumMergeTolerance : ext_LyrMan.MergePointTolerance;

      IFeatureClass pFabricPointsFeatureClass = null;
      if (pFab != null)
        pFabricPointsFeatureClass = (IFeatureClass)pFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
      else
        return;

      //get the reference layer feature class
      IFeatureLayer pReferenceLayer= LayerDropdown.GetFeatureLayer();
      IFeatureLayerDefinition pFeatLyrDef = (IFeatureLayerDefinition)pReferenceLayer;
      IQueryFilter pLayerQueryF = new QueryFilter();
      pLayerQueryF.WhereClause = pFeatLyrDef.DefinitionExpression;
      IFeatureClass pReferenceFC = pReferenceLayer.FeatureClass;

      ext_LyrMan = LayerManager.GetExtension();

      IEditProperties2 pEdProps=ext_LyrMan.TheEditor as IEditProperties2;
      string sFldName = ext_LyrMan.PointFieldName;

      double dReportTol = 0;
      dReportTol = ext_LyrMan.ReportTolerance;
      bool bWriteReport = ext_LyrMan.ShowReport;
      
      char sTab = Convert.ToChar(9);

      IFields2 pFlds = pReferenceFC.Fields as IFields2;
      int iRefField = pFlds.FindField(sFldName);
      if (iRefField > 0)
      {//double check to make sure it's a long
        IField2 pFld = pFlds.get_Field(iRefField) as IField2;
        if (pFld.Type != esriFieldType.esriFieldTypeInteger)
          iRefField = -1;
      }

      if (iRefField == -1)
      {
        MessageBox.Show("Reference field not found. Please check the configuration, and try again.", "Move Fabric Points", MessageBoxButtons.OK, MessageBoxIcon.None);
          return;
      }

      IFeatureCursor pReferenceFeatCur = null;
      bool bUseExtent = false;
      List<int> oidLinesList = new List<int>();
      List<int> oidFabricPointListFromLines = new List<int>();
      List<string> InClausesForLines = null;

      IQueryFilter pQuFilt = new QueryFilter();
      ITable pFabricLinesTable = pFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
      int iFromIDIdx = pFabricLinesTable.FindField("FROMPOINTID");
      int iToIDIdx = pFabricLinesTable.FindField("TOPOINTID");
      int iCtrPtIDIdx = pFabricLinesTable.FindField("CENTERPOINTID");

      Utilities UTIL = new Utilities();

      if (ext_LyrMan.SelectionsUseReferenceFeatures)
      {
        IFeatureSelection featureSelection = pReferenceLayer as IFeatureSelection;
        ISelectionSet selectionSet = featureSelection.SelectionSet;

        if (ext_LyrMan.SelectionsPromptForChoicesWhenNoSelection)
        {
          DialogResult dRes = DialogResult.Yes;
          if (selectionSet.Count == 0)
            dRes = MessageBox.Show("There are no reference features selected." + Environment.NewLine + 
              "Do you want to use the map extent?" + Environment.NewLine + Environment.NewLine +
              "Click 'Yes' to move points to reference features in the map extent." + Environment.NewLine +
            "Click 'No' to Cancel the operation.", "Process data in Map Extent?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
          
          if (dRes != DialogResult.Yes)
            return;
          
          bUseExtent = selectionSet.Count == 0;
        }

        ICursor cursor;
        if (!bUseExtent)
        {
          selectionSet.Search(pLayerQueryF, false, out cursor);
          pReferenceFeatCur = cursor as IFeatureCursor;
        }
        else
        {
          ISpatialFilter pSpatFilt = new SpatialFilter();
          pSpatFilt.WhereClause = pLayerQueryF.WhereClause;
          pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
          pSpatFilt.Geometry = ArcMap.Document.ActiveView.Extent;
          pReferenceFeatCur = pReferenceFC.Search(pSpatFilt, false);
        }
      }
      else if (ext_LyrMan.SelectionsUseParcels)
      { 
        //Get point ids from selected parcels
        ICadastralSelection pCadaSel = pCadEd as ICadastralSelection;
        IEnumGSParcels pEnumGSParcels = pCadaSel.SelectedParcels;// need to get the parcels before trying to get the parcel count: BUG workaround
        if (ext_LyrMan.SelectionsPromptForChoicesWhenNoSelection)
        {
          DialogResult dRes = DialogResult.Yes;
          if (pCadaSel.SelectedParcelCount == 0)
            dRes = MessageBox.Show("There are no parcels selected." + Environment.NewLine +
              "Do you want to use the map extent?" + Environment.NewLine + Environment.NewLine +
              "Click 'Yes' to move points to reference features in the map extent." + Environment.NewLine +
            "Click 'No' to Cancel the operation.", "Process data in Map Extent?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

          if (dRes != DialogResult.Yes)
            return;

          bUseExtent = pCadaSel.SelectedParcelCount == 0;
        }

        if (!bUseExtent)
        {
          //Get point ids from selected parcels
          pEnumGSParcels.Reset();
          IGSParcel pGSParcel = pEnumGSParcels.Next();
          while (pGSParcel != null)
          {
            IEnumGSLines pEnumGSLines = pGSParcel.GetParcelLines(null, false);
            pEnumGSLines.Reset();
            IGSLine pGSLine = null; 
            pEnumGSLines.Next(ref pGSParcel, ref pGSLine);
            while (pGSLine != null)
            {
              oidLinesList.Add(pGSLine.DatabaseId);
              pEnumGSLines.Next(ref pGSParcel, ref pGSLine);           
            }
            pGSParcel = pEnumGSParcels.Next();
          }
        }
        else
        {//get all lines in the map extent
          ISpatialFilter pSpatFilt = new SpatialFilter();
          pSpatFilt.WhereClause = "";
          pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
          pSpatFilt.Geometry = ArcMap.Document.ActiveView.Extent;
          pFab = pCadEd.CadastralFabric;
          ITable pLinesTable = pFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
          ICursor pCur = pLinesTable.Search(pSpatFilt, false);
          IRow pRow = pCur.NextRow();
          while (pRow != null)
          {
            oidLinesList.Add(pRow.OID);
            Marshal.ReleaseComObject(pRow);
            pRow = pCur.NextRow();
          }
          Marshal.ReleaseComObject(pCur);
        }

        if (oidLinesList.Count == 0)
          return;
        //now use the list of Line ids to get the points used
        InClausesForLines = UTIL.InClauseFromOIDsList(oidLinesList, 995);
        string sOID = pFabricLinesTable.OIDFieldName;

        foreach (string InClause in InClausesForLines)
        { 
          pQuFilt.WhereClause = sOID + " IN (" + InClause + ")";
          ICursor pCur2 = pFabricLinesTable.Search(pQuFilt, false) as ICursor;
          IRow pRow = pCur2.NextRow();
          while (pRow != null)
          {
            int iFrom = (int)pRow.get_Value(iFromIDIdx);
            int iTo = (int)pRow.get_Value(iToIDIdx);

            if (!oidFabricPointListFromLines.Contains(iFrom))
              oidFabricPointListFromLines.Add(iFrom);

            if (!oidFabricPointListFromLines.Contains(iTo))
              oidFabricPointListFromLines.Add(iTo);

            Marshal.ReleaseComObject(pRow);
            pRow = pCur2.NextRow();         
          }
          Marshal.ReleaseComObject(pCur2);
        }

        List<int> oidRefPointList = new List<int>();
        List<string> sInClauses2 = UTIL.InClauseFromOIDsList(oidFabricPointListFromLines, 995);

        foreach (string sIn in sInClauses2)
        {
          pQuFilt.WhereClause = sFldName+" IN (" + sIn + ")";
          IFeatureCursor pFeatCur2 = pReferenceFC.Search(pQuFilt, false);
          IFeature pFeat = pFeatCur2.NextFeature();
          while (pFeat!=null)
          {
            oidRefPointList.Add(pFeat.OID);
            Marshal.ReleaseComObject(pFeat);
            pFeat = pFeatCur2.NextFeature();
          }
          Marshal.ReleaseComObject(pFeatCur2);
        }
        int[] oidPointListFromParcels = oidRefPointList.ToArray();
        if (oidRefPointList.Count == 0)
          return;

        pReferenceFeatCur = pReferenceFC.GetFeatures(oidPointListFromParcels, false);

      }
      else if (ext_LyrMan.SelectionsIgnore)
        pReferenceFeatCur = pReferenceFC.Search(pLayerQueryF, false);

      if (pReferenceFeatCur == null)
      {
        MessageBox.Show("Null cursor detected.");
        return;
      }

      Dictionary<int, IPoint> dict_PointMatchLookup = new Dictionary<int, IPoint>();
      Dictionary<int, string> dict_TargetPoints = new Dictionary<int, string>();
      Dictionary<int, List<int>> dict_MergePointMapperList = new Dictionary<int, List<int>>();
      Dictionary<int, int> dict_MergePointMapper = new Dictionary<int, int>();

      List<int> oidList = new List<int>();
      List<int> oidRepeatList = new List<int>();
      bool bUseLines = (pReferenceFC.ShapeType==esriGeometryType.esriGeometryPolyline);
      IFeature pRefFeat = pReferenceFeatCur.NextFeature();

      while (pRefFeat != null)
      {
        if (!bUseLines)
        {
          IPoint pPoint = pRefFeat.Shape as IPoint;
          if (pPoint !=null)
          { 
            object obj = pRefFeat.get_Value(iRefField);
            if (obj != DBNull.Value && !pPoint.IsEmpty)
            {
              int iRefPoint = Convert.ToInt32(obj);
              if (!oidList.Contains(iRefPoint))
              {
                dict_PointMatchLookup.Add(iRefPoint, pPoint);
                oidList.Add(iRefPoint);

                string sXY= PointXYAsSingleIntegerInterleave(pPoint, 3);
                string sXY2=sXY.Remove(sXY.Length-8);
                dict_TargetPoints.Add(iRefPoint, sXY2);

              }
              else
              {
                if(!oidRepeatList.Contains(iRefPoint))
                  oidRepeatList.Add(iRefPoint);
              }
            }
          }
        }

        Marshal.ReleaseComObject(pRefFeat);
        pRefFeat = pReferenceFeatCur.NextFeature();
      }
      
      if (pReferenceFeatCur != null)
        Marshal.FinalReleaseComObject(pReferenceFeatCur);

      if (oidRepeatList.Count > 0)
      {
        string sRepeatedIDList = "";
        foreach (int i in oidRepeatList)
        {
          sRepeatedIDList += Convert.ToInt64(i) + Environment.NewLine;
          if (i > 10) //only show first 10 repeats
            break;
        }
        MessageBox.Show("There is more than one reference with the same id." + Environment.NewLine 
          + "Ids should be unique:" + Environment.NewLine +
          sRepeatedIDList,"Ids not unique");
        return;
      }

      //check the source points for overlaps or close points
      Dictionary<string, List<int>> ClosePoints = dict_TargetPoints.GroupBy(r => r.Value)
                        .ToDictionary(t => t.Key, t => t.Select(r => r.Key).ToList());

      //remove the points that have a unique location
      var PointsToRemove = ClosePoints.Where(kvp => kvp.Value.Count <= 1).Select(kvp => kvp.Key).ToArray();
      foreach (var key in PointsToRemove)
        ClosePoints.Remove(key);

      //Find all lines that have a To or From id with these close points

      
      //go through the close points and test nearness to other points

      //build point merge mapping and line id list
      int iMappedSourceItem = 0;
      Dictionary<int, List<int>> InitialMergePointMapper = new Dictionary<int, List<int>>();

      List<int> FlatListClosePoints = new List<int>();

      foreach (KeyValuePair<string, List<int>> item in ClosePoints)
      {
        List<int> lItem = item.Value;
        int iCnt = 0;
        foreach (int i in lItem)
        {
          if (iCnt == 0)
          {//first one keeps the source point and initializes Merge map list
            List<int> lstMappedItem = new List<int>();
            InitialMergePointMapper.Add(i, lstMappedItem);
            iMappedSourceItem = i;
          }
          else
          {
            List<int> lstMappedItem2 = InitialMergePointMapper[iMappedSourceItem];
            lstMappedItem2.Add(i);
          }
          iCnt++;
          if (!FlatListClosePoints.Contains(i))
            FlatListClosePoints.Add(i);
        }
      }
      
      List<string> sInClauses = null;
      Dictionary<int, Int32> dict_LineIDFromToHash = new Dictionary<int,Int32>();
      List<int> lstAffectedLines = new List<int>();
      pQuFilt = new QueryFilter();

      if (FlatListClosePoints.Count > 0)
      {
        string sOID = pFabricPointsFeatureClass.OIDFieldName;
        IFeatureClass pFabricLinesFC = pFabricLinesTable as IFeatureClass; 
        
        string sFromPointID = pFabricLinesFC.Fields.get_Field(iFromIDIdx).Name;
        string sToPointID = pFabricLinesFC.Fields.get_Field(iToIDIdx).Name;
        string sCenterPointID = pFabricLinesFC.Fields.get_Field(iCtrPtIDIdx).Name;

        sInClauses = UTIL.InClauseFromOIDsList(FlatListClosePoints, 995);
        foreach (string sInClause in sInClauses)
        {
          //pQuFilt.WhereClause = sFromPointID + " IN (" + sInClause + ")" + " OR " + sToPointID + " IN (" + sInClause + ")";
          // OR on InClauses has performance problems on some DB platforms, so using separate cursors here.
          pQuFilt.WhereClause = sFromPointID + " IN (" + sInClause + ")";
          IFeatureCursor pFeatCurs = pFabricLinesFC.Search(pQuFilt, false);
          IFeature pLineFeat = pFeatCurs.NextFeature();

          while (pLineFeat != null)
          {
            //create from/to hash
            int iFrom = (int)pLineFeat.get_Value(iFromIDIdx);
            int iTo = (int)pLineFeat.get_Value(iToIDIdx);

            Int32 iHashCode = iFrom.GetHashCode() ^ iTo.GetHashCode();
            //the same hash code will be returned for [from -> to] as for [to -> from]
            //this is OK for this hash lookup.
            dict_LineIDFromToHash.Add(pLineFeat.OID, iHashCode);

            Marshal.ReleaseComObject(pLineFeat);
            pLineFeat = pFeatCurs.NextFeature(); ;
          }

          if (pFeatCurs != null)
            Marshal.FinalReleaseComObject(pFeatCurs);

          pQuFilt.WhereClause = sToPointID + " IN (" + sInClause + ")";
          pFeatCurs = pFabricLinesFC.Search(pQuFilt, false);
          pLineFeat = pFeatCurs.NextFeature();

          while (pLineFeat != null)
          {
            if (!dict_LineIDFromToHash.ContainsKey(pLineFeat.OID))
            {
              //create from/to hash
              int iFrom = (int)pLineFeat.get_Value(iFromIDIdx);
              int iTo = (int)pLineFeat.get_Value(iToIDIdx);

              Int32 iHashCode = iFrom.GetHashCode() ^ iTo.GetHashCode();
              //the same hash code will be returned for [from -> to] as for [to -> from]
              //this is OK for this hash lookup.
              dict_LineIDFromToHash.Add(pLineFeat.OID, iHashCode);
            }
            Marshal.ReleaseComObject(pLineFeat);
            pLineFeat = pFeatCurs.NextFeature(); ;
          }

          if (pFeatCurs != null)
            Marshal.FinalReleaseComObject(pFeatCurs);

          lstAffectedLines = dict_LineIDFromToHash.Keys.ToList();

          pQuFilt.WhereClause = sCenterPointID + " IN (" + sInClause + ")";
          pFeatCurs = pFabricLinesFC.Search(pQuFilt, false);
          pLineFeat = pFeatCurs.NextFeature();

          while (pLineFeat != null)
          {
            if (lstAffectedLines.Contains(pLineFeat.OID))
              continue;

            lstAffectedLines.Add(pLineFeat.OID);

            Marshal.ReleaseComObject(pLineFeat);
            pLineFeat = pFeatCurs.NextFeature(); ;
          }

          if (pFeatCurs != null)
            Marshal.FinalReleaseComObject(pFeatCurs);

        }
      
        //now check, for each of the mergepoint mapping items, if there is a line between them
        //and update the list by removing that reference
        foreach (KeyValuePair<int, List<int>> item in InitialMergePointMapper)
        { 
          List<int> listItem = item.Value;
          List<int> newList = new List<int>();
          foreach (int i in listItem)
          {
            Int32 iHash = item.Key.GetHashCode() ^ i.GetHashCode();
            //get distance between points
            IPoint pPoint = dict_PointMatchLookup[item.Key];
            IProximityOperator pProximOp = pPoint as IProximityOperator;
            double dDist = pProximOp.ReturnDistance(dict_PointMatchLookup[i]);
            if (!dict_LineIDFromToHash.ContainsValue(iHash))
            {
              newList.Add(i); //new list for point pairs without a line between them

              if (dDist < ext_LyrMan.MergePointTolerance)
                dict_MergePointMapper.Add(i, item.Key);
            }
            else
            { //there's a line between 2 points in the merge list
              if (dDist < ext_LyrMan.MergePointTolerance)
              {
                MessageBox.Show("References will result in one or more collapsed lines. Please check" + Environment.NewLine +
                  "for close points that reference each end of the same line.", "Move Fabric Points", MessageBoxButtons.OK, MessageBoxIcon.None);
                  return;
              }
            }
          }
          dict_MergePointMapperList.Add(item.Key, newList);
        }
        sInClauses.Clear(); //clear in clause for next use
      }

      if (oidList.Count == 0)
      {
        MessageBox.Show("No reference features were detected." + Environment.NewLine + 
          "Please check configurations and try again.", "Move Fabric Points", MessageBoxButtons.OK, MessageBoxIcon.None);
        return;
      }

      sInClauses = UTIL.InClauseFromOIDsList(oidList,995);

      ICadastralFabricUpdate pFabricPointUpdate = (ICadastralFabricUpdate)pFab;
      ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pFab;

      ITrackCancel pTrkCan = new CancelTracker();
      // Create and display the Progress Dialog
      IProgressDialogFactory pProDlgFact = new ProgressDialogFactoryClass();
      IProgressDialog2 pProDlg = pProDlgFact.Create(pTrkCan, ArcMap.Application.hWnd) as IProgressDialog2;
      //Set the properties of the Progress Dialog
      pProDlg.CancelEnabled = false;
      pProDlg.Description = "Moving fabric points to reference layer...";
      pProDlg.Title = "Moving Points";
      pProDlg.Animation = esriProgressAnimationTypes.esriProgressGlobe;
      string sOIDFld = pFabricPointsFeatureClass.OIDFieldName;
      IAngularConverter pAngConv = new AngularConverter();
      IMap pMap = ext_LyrMan.TheEditor.Map;
      string sUnit = "<unknown units>";
      Dictionary<int, double> dict_Length = new Dictionary<int,double>();
      Dictionary<int, string> dict_LineReport = new Dictionary<int, string>();
      
      if (bWriteReport)
      {
        ISpatialReference pMapSpatRef = pMap.SpatialReference;
        IProjectedCoordinateSystem2 pPCS = null;

        m_sReport = "Fabric Point Move to Feature:" + sUnderline;
        if (pMapSpatRef != null)
        {
          if (pMapSpatRef is IProjectedCoordinateSystem2)
          {
            pPCS = (IProjectedCoordinateSystem2)pMapSpatRef;
            sUnit = pPCS.CoordinateUnit.Name.ToLower();
            if (sUnit.Contains("foot") && sUnit.Contains("us"))
              sUnit = "U.S. Feet";
            else if (sUnit.Contains("meter"))
              sUnit = sUnit.ToLower() + "s";
          }
        }
      }

      //find the line points based on the list of OIDs
      ITable pFabricLPsTable = pFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLinePoints);
      int iLinePtIDIdx = pFabricLPsTable.FindField("LINEPOINTID");

      Dictionary<int, int[]> dict_AffectedLinePointsLookup = new Dictionary<int, int[]>();
      List<int> lstAffectedLinePoints = new List<int>();

      ////first level line point correction
      List<int> lstNewPoints1 = null;
      List<int> lstDirectlyReferencedLinePoints = new List<int>();

      UpdateLinePointPositions(pFabricLPsTable, ref lstAffectedLinePoints, ref dict_AffectedLinePointsLookup,
        oidList, out lstNewPoints1, ref lstDirectlyReferencedLinePoints);
      
      List<int> lstNewPoints2 = null;
      List<int> lstNewPoints3 = null;
      List<int> lstNewPoints4 = null;
      List<int> lstNewPoints5 = null;
      List<int> lstNewPoints6 = null;
      
      ////second cascade level for downstream affected line points
      //for the affected line points, do another search using those ID's in From and To

      List<int> lstTemp = new List<int>();

      if (lstNewPoints1 != null)
      {
        if (lstNewPoints1.Count > 0)
        {
          UpdateLinePointPositions(pFabricLPsTable, ref lstAffectedLinePoints, ref dict_AffectedLinePointsLookup,
            lstNewPoints1, out lstNewPoints2, ref lstTemp);
        }
      }

      ////third cascade level for downstream affected line points
      if (lstNewPoints2 != null)
      {
        if (lstNewPoints2.Count > 0)
        {
          UpdateLinePointPositions(pFabricLPsTable, ref lstAffectedLinePoints, ref dict_AffectedLinePointsLookup,
            lstNewPoints2, out lstNewPoints3, ref lstTemp);
        }
      }

      ////fourth cascade level for downstream affected line points
      if (lstNewPoints3 != null)
      {
        if (lstNewPoints3.Count > 0)
        {
          UpdateLinePointPositions(pFabricLPsTable, ref lstAffectedLinePoints, ref dict_AffectedLinePointsLookup,
            lstNewPoints3, out lstNewPoints4, ref lstTemp);
        }
      }

      ////fifth cascade level for downstream affected line points
      if (lstNewPoints4 != null)
      {
        if (lstNewPoints4.Count > 0)
        {
          UpdateLinePointPositions(pFabricLPsTable, ref lstAffectedLinePoints, ref dict_AffectedLinePointsLookup,
            lstNewPoints4, out lstNewPoints5, ref lstTemp);
        }
      }

      ////sixth cascade level for downstream affected line points
      if (lstNewPoints5 != null)
      {
        if (lstNewPoints5.Count > 0)
        {
          UpdateLinePointPositions(pFabricLPsTable, ref lstAffectedLinePoints, ref dict_AffectedLinePointsLookup,
            lstNewPoints5, out lstNewPoints6, ref lstTemp);
        }
      }

      lstTemp = null;

      //now remove the line points that are direct references
      string sOid_Conflicts = "";
      foreach (int i in lstDirectlyReferencedLinePoints)
      {
        sOid_Conflicts += i.ToString() + ", ";
        dict_PointMatchLookup.Remove(i);
      }

      foreach (string InClause in sInClauses)
      {
        pLayerQueryF.WhereClause = sOIDFld + " IN (" +  InClause +")";
        IFeatureCursor pFeatCurs = pFabricPointsFeatureClass.Search(pLayerQueryF, false);
        IFeature pPointFeat = pFeatCurs.NextFeature();
        int iIsCtrPtIDX = pFabricPointsFeatureClass.FindField("CENTERPOINT");

        while (pPointFeat != null)
        {
          IGeometry pGeom = pPointFeat.Shape;
          if (pGeom == null)
          {
            Marshal.ReleaseComObject(pPointFeat);
            pPointFeat = pFeatCurs.NextFeature();
            continue;
          }
          if (pGeom.IsEmpty)
          {
            Marshal.ReleaseComObject(pPointFeat);
            pPointFeat = pFeatCurs.NextFeature();
            continue;
          }

          if (!dict_PointMatchLookup.ContainsKey(pPointFeat.OID))
          {
            Marshal.ReleaseComObject(pPointFeat);
            pPointFeat = pFeatCurs.NextFeature();
            continue;
          }

          IPoint pPt = (IPoint)dict_PointMatchLookup[pPointFeat.OID];

          if (dict_MergePointMapper.ContainsKey(pPointFeat.OID))
            pPt = dict_PointMatchLookup[dict_MergePointMapper[pPointFeat.OID]];

          IProximityOperator pProx = pPt as IProximityOperator;
          double dProximityDistance = pProx.ReturnDistance(pGeom);
          if (dProximityDistance <= ext_LyrMan.MinimumMoveTolerance && ext_LyrMan.TestForMinimumMove)
          {
            Marshal.ReleaseComObject(pPointFeat);
            pPointFeat = pFeatCurs.NextFeature();
            continue;
          }

          if (bWriteReport)
          {
            IPoint pFromPt = pGeom as IPoint;
            ILine pLine = new ESRI.ArcGIS.Geometry.Line();
            pLine.PutCoords(pFromPt, pPt);
            pAngConv.SetAngle(pLine.Angle, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
            string sDirection = pAngConv.GetString(pEdProps.DirectionType, pEdProps.DirectionUnits, pEdProps.AngularUnitPrecision);
            string sPointID = pPointFeat.OID.ToString();
            sPointID = String.Format("{0,10}", sPointID);
            dict_LineReport.Add(pPointFeat.OID, "Point " + sPointID + sTab + dProximityDistance.ToString("#.000") + ",  " + sDirection);
            dict_Length.Add(pPointFeat.OID, dProximityDistance);
          }

          int iVal = -1;
          object Attr_val = pPointFeat.get_Value(iIsCtrPtIDX);

          if (Attr_val != DBNull.Value)
            iVal = Convert.ToInt32(pPointFeat.get_Value(iIsCtrPtIDX));
          else
            iVal = 0;

          bool bIsCenterPoint = (iVal != 0);

          pFabricPointUpdate.AddAdjustedPoint(pPointFeat.OID, pPt.X, pPt.Y, bIsCenterPoint);

          Marshal.ReleaseComObject(pPointFeat);
          pPointFeat = pFeatCurs.NextFeature();
        }
        if (pFeatCurs != null)
          Marshal.FinalReleaseComObject(pFeatCurs);
      }

      //we have a dictionary of line points, so update points for line points
      //

      List<string> sInclauses = UTIL.InClauseFromOIDsList(lstAffectedLinePoints, 995);
      IFeatureClass pFabricLPsFeatClass = pFabricLPsTable as IFeatureClass;
      foreach (string sInCl in sInclauses)
      {
        if (sInCl.Trim() == "")
          continue;
        pQuFilt.WhereClause = pFabricLPsTable.OIDFieldName + " IN (" + sInCl + ")";
        IFeatureCursor pFeatCurs = pFabricLPsFeatClass.Search(pQuFilt, false);
        IFeature pLinePointFeat = pFeatCurs.NextFeature();
        while (pLinePointFeat != null)
        {
          //first get the original from and to points
          int[] iFrTo = dict_AffectedLinePointsLookup[(int)pLinePointFeat.get_Value(iLinePtIDIdx)];

          IFeature pFromPtFeat = pFabricPointsFeatureClass.GetFeature(iFrTo[0]);
          IFeature pToPtFeat = pFabricPointsFeatureClass.GetFeature(iFrTo[1]);

          IPoint pLinePt = pLinePointFeat.Shape as IPoint;

          IProximityOperator pProx = pLinePt as IProximityOperator;
          double dDist1 = pProx.ReturnDistance(pFromPtFeat.Shape);
          double dDist2 = pProx.ReturnDistance(pToPtFeat.Shape);
          double dRatio = dDist1 / (dDist1 + dDist2);

          //now get the new positions of the points from the reference point locations initialized to the fabric positions
          IPoint pUpdatedFromPtLocation=pFromPtFeat.Shape as IPoint;
          IPoint pUpdatedToPtLocation=pToPtFeat.Shape as IPoint;


          //if the id is present then set the point to the reference geom
          if (dict_PointMatchLookup.ContainsKey(iFrTo[0]))
            pUpdatedFromPtLocation = dict_PointMatchLookup[iFrTo[0]];

          if (dict_PointMatchLookup.ContainsKey(iFrTo[1]))
            pUpdatedToPtLocation = dict_PointMatchLookup[iFrTo[1]];

          ILine pLine = new LineClass();
          pLine.PutCoords(pUpdatedFromPtLocation, pUpdatedToPtLocation);
          IConstructPoint pConstPt = new PointClass();
          pConstPt.ConstructAlong(pLine as ICurve, esriSegmentExtension.esriExtendAtFrom, dRatio, true);
          int iOID = (int)pLinePointFeat.get_Value(iLinePtIDIdx);
          if (!dict_PointMatchLookup.ContainsKey(iOID))
            dict_PointMatchLookup.Add(iOID,pConstPt as IPoint);
          pFabricPointUpdate.AddAdjustedPoint(iOID, (pConstPt as IPoint).X, (pConstPt as IPoint).Y, false);

          Marshal.ReleaseComObject(pLinePointFeat);
          pLinePointFeat = pFeatCurs.NextFeature();
        }
        if (pFeatCurs != null)
          Marshal.FinalReleaseComObject(pFeatCurs);

      }


      string sQualifier = ext_LyrMan.TestForMinimumMove ? "more than " + ext_LyrMan.MinimumMoveTolerance.ToString("#.000")
        + " " + sUnit + " :" : " :";
      if (dict_Length.Count > 0)
        m_sReport += dict_Length.Count.ToString() + " point(s) moved " + sQualifier + Environment.NewLine + Environment.NewLine;
      else
        m_sReport += "No points were moved." + Environment.NewLine;

      var sortedDict = from entry in dict_Length orderby entry.Value descending select entry;
      var pEnum = sortedDict.GetEnumerator();
      while (pEnum.MoveNext())
      {
        var pair = pEnum.Current;
        m_sReport += dict_LineReport[pair.Key];
        m_sReport += Environment.NewLine;
      }

      m_sReport += sUnderline;

      if (lstDirectlyReferencedLinePoints.Count > 0 && dict_Length.Count>0)
      {
        m_sReport += "The references for " + lstDirectlyReferencedLinePoints.Count.ToString() + " point(s) were not applied because"
           + Environment.NewLine + "they are line points." + Environment.NewLine + Environment.NewLine + "These line points were held fixed or were moved to lines."
           + Environment.NewLine + "(Points: " + sOid_Conflicts.TrimEnd().TrimEnd(',') + ")" + sUnderline;
      }
      else if (lstDirectlyReferencedLinePoints.Count > 0 && dict_Length.Count==0)
      {
        m_sReport += "The references for " + lstDirectlyReferencedLinePoints.Count.ToString() + " point(s) were not applied because"
           + Environment.NewLine + "they are line points." + Environment.NewLine + Environment.NewLine + "(Points: " + sOid_Conflicts.TrimEnd().TrimEnd(',') + ")" + sUnderline;
      }


      IArray PolygonLyrArr = new ESRI.ArcGIS.esriSystem.Array();
      if (!UTIL.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTParcels, out PolygonLyrArr))
        return;

      //Start the fabric point adjustment
      try
      {
        ext_LyrMan.TheEditor.StartOperation();
        pFabricPointUpdate.AdjustFabric(pTrkCan);
        pFabricPointUpdate.ClearAdjustedPoints();


        if (dict_MergePointMapper.Count>0)
        {
          InClausesForLines=UTIL.InClauseFromOIDsList(lstAffectedLines, 995);
          List<int> lstOrphanPoints = new List<int>();
          //update the ToPoint id and FromPointId references on lines 
          pSchemaEd.ReleaseReadOnlyFields(pFabricLinesTable, esriCadastralFabricTable.esriCFTLines);
          foreach (string InClause in InClausesForLines)
          {
            pQuFilt.WhereClause = pFabricPointsFeatureClass.OIDFieldName +" IN (" + InClause + ")";
            ICursor pCur2 = pFabricLinesTable.Update(pQuFilt, false) as ICursor;
            IRow pRow = pCur2.NextRow();
            while (pRow != null)
            {
              int iFrom = (int)pRow.get_Value(iFromIDIdx);
              if (dict_MergePointMapper.ContainsKey(iFrom))
              {
                int iVal=dict_MergePointMapper[iFrom];
                pRow.set_Value(iFromIDIdx, iVal);
                if (!lstOrphanPoints.Contains(iFrom)) 
                  lstOrphanPoints.Add(iFrom); //iFrom is an orphan
              }

              int iTo = (int)pRow.get_Value(iToIDIdx);
              if (dict_MergePointMapper.ContainsKey(iTo))
              {
                int iVal = dict_MergePointMapper[iTo];
                pRow.set_Value(iToIDIdx, iVal);
                if (!lstOrphanPoints.Contains(iTo))
                  lstOrphanPoints.Add(iTo); //iTo is an orphan
              }

              object obj = pRow.get_Value(iCtrPtIDIdx);
              if (obj != DBNull.Value)
              {
                int iCtrPoint = (int)obj;
                if (dict_MergePointMapper.ContainsKey(iCtrPoint))
                {
                  int iVal = dict_MergePointMapper[iCtrPoint];
                  pRow.set_Value(iCtrPtIDIdx, iVal);
                  if (!lstOrphanPoints.Contains(iCtrPoint))
                    lstOrphanPoints.Add(iCtrPoint); //iCtrPoint is an orphan
                }
              }

              pRow.Store();

              Marshal.ReleaseComObject(pRow);
              pRow = pCur2.NextRow();
            }
            Marshal.ReleaseComObject(pCur2);
          }
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);

          if (lstOrphanPoints.Count > 0)
          {
            List<string> lstInClauseOrphanPoints = UTIL.InClauseFromOIDsList(lstOrphanPoints, 995);
            IField pField = pFabricPointsFeatureClass.Fields.get_Field(pFabricPointsFeatureClass.FindField(pFabricPointsFeatureClass.OIDFieldName));
            UTIL.DeleteByInClause(ext_LyrMan.TheEditor.EditWorkspace, (ITable)pFabricPointsFeatureClass, pField,
              lstInClauseOrphanPoints, false, null, pTrkCan);
          }
        }

        dict_PointMatchLookup.Clear();
        dict_TargetPoints.Clear();
        dict_MergePointMapperList.Clear();
        dict_MergePointMapper.Clear();
        dict_LineIDFromToHash.Clear();
        FlatListClosePoints.Clear();
        ClosePoints.Clear();
        InitialMergePointMapper.Clear();
        if (InClausesForLines != null)
          InClausesForLines.Clear();
        sInClauses.Clear();
        lstAffectedLines.Clear();
        lstDirectlyReferencedLinePoints.Clear();
        oidList.Clear();
        oidRepeatList.Clear();

      }
      catch (Exception ex)
      {
        ext_LyrMan.TheEditor.AbortOperation();
        COMException cEx = ex as COMException;
        m_sReport = ex.Message;
        m_sReport += Environment.NewLine + ex.Message;
      }
      finally
      {
        if (pProDlg != null)
          pProDlg.HideDialog();

        ext_LyrMan.TheEditor.StopOperation("Update Fabric Points");
        IActiveView pActiveView = ArcMap.Document.ActiveView;
        for (int j=0; j < PolygonLyrArr.Count;j++)
          pActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, PolygonLyrArr.get_Element(j), pActiveView.Extent);

        if (bWriteReport)
        {
          //m_sReport += Environment.NewLine + " *** BETA *** " + sUnderline;
          ReportDLG ReportDialog = new ReportDLG();
          ReportDialog.textBox1.Text = m_sReport;
          ReportDialog.ShowDialog();
        }

      }
    }

    protected void UpdateLinePointPositions(ITable pFabricLPsTable, ref List<int> lstAffectLPs, ref Dictionary<int, int[]> dict_AffectedLinePointsLookup, 
      List<int> lstDownstreamLinePoints, out List<int> NewAffectedPoints, ref List<int> DirectlyReferencedLinePoints)
    {
      NewAffectedPoints = new List<int>();
      Utilities UTIL = new Utilities();
      List<string> sInClauses = UTIL.InClauseFromOIDsList(lstDownstreamLinePoints, 995);

      int iFromID_LP_Idx = pFabricLPsTable.FindField("FROMPOINTID");
      string sFromIDLP = pFabricLPsTable.Fields.get_Field(iFromID_LP_Idx).Name;

      int iToID_LP_Idx = pFabricLPsTable.FindField("TOPOINTID");
      string sToIDLP = pFabricLPsTable.Fields.get_Field(iToID_LP_Idx).Name;

      int iLinePtIDIdx = pFabricLPsTable.FindField("LINEPOINTID");
      string sLinePtID = pFabricLPsTable.Fields.get_Field(iLinePtIDIdx).Name;

      IQueryFilter pQuFilt = new QueryFilter();

      foreach (string InClause in sInClauses)
      {
        pQuFilt.WhereClause = sFromIDLP + " IN (" + InClause + ")";
        ICursor pCurs = pFabricLPsTable.Search(pQuFilt, false);
        IRow pLinePoint = pCurs.NextRow();
        while (pLinePoint != null)
        {
          if (!lstAffectLPs.Contains(pLinePoint.OID))
          {
            lstAffectLPs.Add(pLinePoint.OID);
            NewAffectedPoints.Add((int)pLinePoint.get_Value(iLinePtIDIdx));
          }

          int iFromID_LP = (int)pLinePoint.get_Value(iFromID_LP_Idx);
          int iToID_LP = (int)pLinePoint.get_Value(iToID_LP_Idx);

          int[] FromTo = new int[2] { iFromID_LP, iToID_LP };
          int i = (int)pLinePoint.get_Value(iLinePtIDIdx);

          if (!dict_AffectedLinePointsLookup.ContainsKey(i))
            dict_AffectedLinePointsLookup.Add(i, FromTo);

          Marshal.ReleaseComObject(pLinePoint);
          pLinePoint = pCurs.NextRow();
        }
        Marshal.ReleaseComObject(pCurs);

        pQuFilt.WhereClause = sToIDLP + " IN (" + InClause + ")";
        pCurs = pFabricLPsTable.Search(pQuFilt, false);
        pLinePoint = pCurs.NextRow();
        while (pLinePoint != null)
        {
          if (!dict_AffectedLinePointsLookup.ContainsKey((int)pLinePoint.get_Value(iLinePtIDIdx)))
          {
            if (!lstAffectLPs.Contains(pLinePoint.OID))
            {
              lstAffectLPs.Add(pLinePoint.OID);
              NewAffectedPoints.Add((int)pLinePoint.get_Value(iLinePtIDIdx));
            }
            int[] FromTo = new int[2] { (int)pLinePoint.get_Value(iFromID_LP_Idx), (int)pLinePoint.get_Value(iToID_LP_Idx) };
            dict_AffectedLinePointsLookup.Add((int)pLinePoint.get_Value(iLinePtIDIdx), FromTo);
          }
          Marshal.ReleaseComObject(pLinePoint);
          pLinePoint = pCurs.NextRow();
        }
        Marshal.ReleaseComObject(pCurs);

        pQuFilt.WhereClause = sLinePtID + " IN (" + InClause + ")";
        pCurs = pFabricLPsTable.Search(pQuFilt, false);
        pLinePoint = pCurs.NextRow();
        while (pLinePoint != null)
        {
          //we need to build a list of LinePoints that have direct point references, as this is an exclusion set
          if (!DirectlyReferencedLinePoints.Contains(pLinePoint.OID))
            DirectlyReferencedLinePoints.Add((int)pLinePoint.get_Value(iLinePtIDIdx));
          Marshal.ReleaseComObject(pLinePoint);
          pLinePoint = pCurs.NextRow();
       
        }
        Marshal.ReleaseComObject(pCurs);
      
      }

    }

    protected string PointXYAsSingleIntegerInterleave(IPoint point, int iPrecision)
    {

      //string sZeroFormat = new String('#', iPrecision);
      string sZeroFormat = new String('0', iPrecision);
      string sFormat1Precision = "0." + sZeroFormat;
      string sX = point.X.ToString(sFormat1Precision);
      string sY = point.Y.ToString(sFormat1Precision);

      int iLength = sX.Length > sY.Length ? sX.Length : sY.Length;

      string sFormat = new String('0', iLength-iPrecision-1) + "." + sZeroFormat;

      Char[] chrArrayX = point.X.ToString(sFormat).ToCharArray();
      Char[] chrArrayY = point.Y.ToString(sFormat).ToCharArray();

      char[] chars = new char[iLength*2];
      for (int i = 0; i < sX.Length; i++)
      {
        chars[i*2] = chrArrayX[i];
        chars[i*2+1] = chrArrayY[i];

      
      }
      string sInterleaved = new string(chars);

      sInterleaved = sInterleaved.Replace("..", "");

      return sInterleaved;

    }

    protected double GetMaxShiftThreshold(ICadastralFabric pFab)
    {
      IDatasetComponent pDSComponent = (IDatasetComponent)pFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric2 pDECadaFab = (IDECadastralFabric2)pDEDS;
      double d_retVal = pDECadaFab.MaximumShiftThreshold;
      return d_retVal;
    }

    protected double ConvertDistanceFromMetersToFabricUnits(double InDistance, ICadastralFabric InFabric, out string UnitString, out double MetersPerMapUnit)
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

    protected override void OnUpdate()
    {
      if (ext_LyrMan==null)
        ext_LyrMan = LayerManager.GetExtension();
      Enabled = ext_LyrMan.TheEditor.EditState != esriEditState.esriStateNotEditing;
    }
  }

  public class MovePtsToTargetConfig : ESRI.ArcGIS.Desktop.AddIns.Button
  {

    public MovePtsToTargetConfig()
    {

    }

    protected override void OnClick()
    {
      LayerManager ext_LyrMan = LayerManager.GetExtension();
      ConfigurationDLG ConfigDial = new ConfigurationDLG();

      IMap pMap = ArcMap.Document.FocusMap;
      ISpatialReference pMapSpatRef = pMap.SpatialReference;
      IProjectedCoordinateSystem2 pPCS = null;

  //  #region Get Feature Layers
      UID pId = new UID();
      pId.Value = "{E156D7E5-22AF-11D3-9F99-00C04F6BC78E}";
      IEnumLayer pEnumLyr= pMap.get_Layers(pId,true);
      pEnumLyr.Reset();
      IFeatureLayer pFeatLayer = pEnumLyr.Next() as IFeatureLayer;
      ConfigDial.cboFldChoice.Items.Clear();
      while (pFeatLayer != null)
      {
        if (pFeatLayer is ICadastralFabricSubLayer2)
        {
          pFeatLayer = pEnumLyr.Next() as IFeatureLayer;
          continue;
        }
        if (pFeatLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
        {
          IFeatureClass pFC = pFeatLayer.FeatureClass;
          IFields2 pFlds = pFC.Fields as IFields2;
          for (int i = 0; i < pFlds.FieldCount; i++)
          {
            if (pFlds.get_Field(i).Editable && pFlds.get_Field(i).Type == esriFieldType.esriFieldTypeInteger)
              ConfigDial.cboFldChoice.Items.Add(pFlds.get_Field(i).Name);
          }
        }
        
        pFeatLayer = pEnumLyr.Next() as IFeatureLayer;
      }
   // #endregion

      if (pMapSpatRef == null)
        ConfigDial.lblUnits1.Text = ConfigDial.lblUnits2.Text = ConfigDial.lblUnits3.Text = "<unknown units>";
      else if (pMapSpatRef is IProjectedCoordinateSystem2)
      {
        pPCS = (IProjectedCoordinateSystem2)pMapSpatRef;
        string sUnit = pPCS.CoordinateUnit.Name.ToLower();
        if (sUnit.Contains("foot") && sUnit.Contains("us"))
          sUnit = "U.S. Feet";
        else if (sUnit.Contains("meter"))
          sUnit = sUnit.ToLower() + "s";
        ConfigDial.lblUnits1.Text = ConfigDial.lblUnits2.Text = ConfigDial.lblUnits3.Text = sUnit;
      }

      #region Get last page used from the registry
      Utilities Utils = new Utilities();
      string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
      if (sDesktopVers.Trim() == "")
        sDesktopVers = "Desktop10.4";
      else
        sDesktopVers = "Desktop" + sDesktopVers;
      string sTabPgIdx = "";
      if (sDesktopVers.Trim() != "")
        sTabPgIdx = Utils.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral",
        "AddIn.FabricPointMoveToFeatureConfigurationLastPageUsed");

      if (sTabPgIdx.Trim() == "")
        sTabPgIdx = "0";
      #endregion

      ConfigDial.tbConfiguration.SelectedIndex = Convert.ToInt32(sTabPgIdx);

      DialogResult dlgRes= ConfigDial.ShowDialog();
      if (ConfigDial.txtMinimumMove.Text.Trim() == "")
        ConfigDial.txtMinimumMove.Text = "0";
      if (dlgRes == DialogResult.OK)
      {
        ext_LyrMan.UseLines = ConfigDial.optLines.Checked;
        ext_LyrMan.PointFieldName = ConfigDial.cboFldChoice.Text;
        ext_LyrMan.TestForMinimumMove = ConfigDial.chkMinimumMove.Checked;
        ext_LyrMan.MinimumMoveTolerance = Convert.ToDouble(ConfigDial.txtMinimumMove.Text);
        ext_LyrMan.ReportTolerance = Convert.ToDouble(ConfigDial.txtReportTolerance.Text);
        ext_LyrMan.ShowReport = ConfigDial.chkReport.Checked;
        ext_LyrMan.MergePointTolerance = Convert.ToDouble(ConfigDial.txtMergeTolerance.Text);
        ext_LyrMan.MergePoints = ConfigDial.chkPointMerge.Checked; 
        ext_LyrMan.SelectionsIgnore = ConfigDial.optMoveAllFeaturesNoSelection.Checked;
        ext_LyrMan.SelectionsUseReferenceFeatures = ConfigDial.optMoveBasedOnSelectedFeatures.Checked;
        ext_LyrMan.SelectionsUseParcels = ConfigDial.optMoveBasedOnSelectedParcels.Checked;
        ext_LyrMan.SelectionsPromptForChoicesWhenNoSelection = ConfigDial.chkPromptForSelection.Checked;
      }

      //now refresh the layer dropdown
      if (pMap == null)//if it's still null then bail
        return;
      LayerDropdown.FillComboBox(pMap);

      ArcMap.Application.CurrentTool = null;
    }
    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }
  }

}
