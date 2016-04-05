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
    private string sUnderline = Environment.NewLine + "---------------------------------------------------------------------" + Environment.NewLine;

    protected override void OnClick()
    {
      //get the Cadastral Extension
      UID pUID = new UID();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      //get the parcel fabric
      ICadastralFabric pFab = pCadEd.CadastralFabric;

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
      List<int> oidLineListFromParcel = new List<int>();
      List<int> oidFabricPointListFromParcel = new List<int>();

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
              oidLineListFromParcel.Add(pGSLine.DatabaseId);
              pEnumGSLines.Next(ref pGSParcel, ref pGSLine);           
            }
            pGSParcel = pEnumGSParcels.Next();
          }
        }
        else
        {//get all parcels in the map extent
          ISpatialFilter pSpatFilt = new SpatialFilter();
          pSpatFilt.WhereClause = "";
          pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
          pSpatFilt.Geometry = ArcMap.Document.ActiveView.Extent;
          pFab = pCadEd.CadastralFabric;
          ITable pParcelTable = pFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
          ICursor pCur = pParcelTable.Search(pSpatFilt, false);
          IRow pRow = pCur.NextRow();
          while (pRow != null)
          {
            oidLineListFromParcel.Add(pRow.OID);
            Marshal.ReleaseComObject(pRow);
            pRow = pCur.NextRow();
          }
          Marshal.ReleaseComObject(pCur);
        }

        if (oidLineListFromParcel.Count == 0)
          return;
        //now use the list of Line ids to get the points used
        List<string> InClauses = UTIL.InClauseFromOIDsList(oidLineListFromParcel, 995);
        IQueryFilter pQuFilt = new QueryFilter();
        string sOID=pFabricPointsFeatureClass.OIDFieldName;
        ITable pFabricLinesTable = pFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        int iFromIDIdx = pFabricLinesTable.FindField("FROMPOINTID");
        int iToIDIdx = pFabricLinesTable.FindField("TOPOINTID");
        foreach (string InClause in InClauses)
        { 
          pQuFilt.WhereClause = sOID + " IN (" + InClause + ")";
          ICursor pCur2 = pFabricLinesTable.Search(pQuFilt, false) as ICursor;
          IRow pRow = pCur2.NextRow();
          while (pRow != null)
          {
            int iFrom = (int)pRow.get_Value(iFromIDIdx);
            int iTo = (int)pRow.get_Value(iToIDIdx);

            if (!oidFabricPointListFromParcel.Contains(iFrom))
              oidFabricPointListFromParcel.Add(iFrom);

            if (!oidFabricPointListFromParcel.Contains(iTo))
              oidFabricPointListFromParcel.Add(iTo);

            Marshal.ReleaseComObject(pRow);
            pRow = pCur2.NextRow();         
          }
          Marshal.ReleaseComObject(pCur2);
        }

        List<int> oidRefPointList = new List<int>();
        List<string> sInClauses2 = UTIL.InClauseFromOIDsList(oidFabricPointListFromParcel, 995);

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

      Dictionary<int, IPoint> dict_PointMatch = new Dictionary<int,IPoint>();
      List<int> oidList = new List<int>();
      List<int> oidRepeatList = new List<int>();
      bool bUseLines= (pReferenceFC.ShapeType==esriGeometryType.esriGeometryPolyline);
      IFeature pRefFeat = pReferenceFeatCur.NextFeature();

      while (pRefFeat != null)
      {

        if (!bUseLines)
        {
          IPoint pPoint = pRefFeat.ShapeCopy as IPoint;
          if (pPoint !=null)
          { 
            object obj = pRefFeat.get_Value(iRefField);
            if (obj != DBNull.Value && !pPoint.IsEmpty)
            {
              int iRefPoint = Convert.ToInt32(obj);
              if (!oidList.Contains(iRefPoint))
              {
                dict_PointMatch.Add(iRefPoint, pPoint);
                oidList.Add(iRefPoint);
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
          +"Ids should be unique:" + Environment.NewLine +
          sRepeatedIDList);
        return;
      }

      if (pReferenceFeatCur != null)
        Marshal.FinalReleaseComObject(pReferenceFeatCur);

      if (oidList.Count == 0)
      {
        MessageBox.Show("No reference features were detected." + Environment.NewLine + 
          "Please check configurations and try again.", "Move Fabric Points", MessageBoxButtons.OK, MessageBoxIcon.None);
        return;
      }

      List<string> sInClauses = UTIL.InClauseFromOIDsList(oidList,995);

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

      foreach (string InClause in sInClauses)
      {
        pLayerQueryF.WhereClause = sOIDFld + " IN (" +  InClause +")";
        IFeatureCursor pFeatCurs = pFabricPointsFeatureClass.Search(pLayerQueryF, false);
        IFeature pPointFeat = pFeatCurs.NextFeature();
        int iIsCtrPtIDX = pFabricPointsFeatureClass.FindField("CENTERPOINT");

        while (pPointFeat != null)
        {
          IGeometry pGeom = pPointFeat.ShapeCopy;
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

          IPoint pPt = (IPoint)dict_PointMatch[pPointFeat.OID];

          if (bWriteReport)
          {
            IPoint pFromPt = pGeom as IPoint;
            ILine pLine = new ESRI.ArcGIS.Geometry.Line();
            pLine.PutCoords(pFromPt, pPt);
            if (pLine.Length > ext_LyrMan.ReportTolerance)
            {
              pAngConv.SetAngle(pLine.Angle,esriDirectionType.esriDTPolar,esriDirectionUnits.esriDURadians);
              string sDirection = pAngConv.GetString(pEdProps.DirectionType,pEdProps.DirectionUnits,pEdProps.AngularUnitPrecision);
              string sPointID = pPointFeat.OID.ToString();
              sPointID = String.Format("{0,10}", sPointID);
              dict_LineReport.Add(pPointFeat.OID, "Point " + sPointID + sTab + pLine.Length.ToString("#.000") + ",  " + sDirection);
              dict_Length.Add(pPointFeat.OID,pLine.Length);
            }
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

      m_sReport += dict_Length.Count.ToString() + " points moved more than " + ext_LyrMan.ReportTolerance.ToString("#.000")
        + " " + sUnit + " :" + Environment.NewLine + Environment.NewLine;
      
      var sortedDict = from entry in dict_Length orderby entry.Value descending select entry;
      var pEnum = sortedDict.GetEnumerator();
      while (pEnum.MoveNext())
      {
        var pair = pEnum.Current;
        m_sReport += dict_LineReport[pair.Key];
        m_sReport += Environment.NewLine;
      }

      m_sReport += sUnderline;
      IArray PolygonLyrArr = new ESRI.ArcGIS.esriSystem.Array();
      if (!UTIL.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTParcels, out PolygonLyrArr))
        return;

      //Start the fabric point adjustment
      try
      {
        ext_LyrMan.TheEditor.StartOperation();
        pFabricPointUpdate.AdjustFabric(pTrkCan);
        pFabricPointUpdate.ClearAdjustedPoints();
      }
      catch (Exception ex)
      {
        ext_LyrMan.TheEditor.AbortOperation();
        COMException cEx = ex as COMException;
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
        ConfigDial.lblUnits2.Text = "<unknown units>";
      else if (pMapSpatRef is IProjectedCoordinateSystem2)
      {
        pPCS = (IProjectedCoordinateSystem2)pMapSpatRef;
        string sUnit = pPCS.CoordinateUnit.Name.ToLower();
        if (sUnit.Contains("foot") && sUnit.Contains("us"))
          sUnit = "U.S. Feet";
        else if (sUnit.Contains("meter"))
          sUnit = sUnit.ToLower() + "s";
        ConfigDial.lblUnits1.Text = sUnit;
        ConfigDial.lblUnits2.Text = sUnit;
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
