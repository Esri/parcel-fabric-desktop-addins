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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;

namespace FabricPointMoveToFeature
{
  public class MovePointsToTarget : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public MovePointsToTarget()
    {
    }

    protected override void OnClick()
    {
      //get the Cadastral Extension
      UID pUID = new UID();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);
      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      //get the parcel fabric
      ICadastralFabric pFab = pCadEd.CadastralFabric;

      if (pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing first, and try again.", "Move Fabric Points", MessageBoxButtons.OK, MessageBoxIcon.None);
        return;
      }
      IFeatureClass pFabricPointsFeatureClass = null;
      if (pFab != null)
        pFabricPointsFeatureClass = (IFeatureClass)pFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
      else
        return;
      //IDataset pDS = (IDataset)pFabricPointsFeatureClass;
      //IWorkspace pWS = pDS.Workspace;

      //get the reference layer feature class
      IFeatureLayer pReferenceLayer= LayerDropdown.GetFeatureLayer();
      IFeatureLayerDefinition pFeatLyrDef = (IFeatureLayerDefinition)pReferenceLayer;
      IQueryFilter pLayerQueryF = new QueryFilter();
      pLayerQueryF.WhereClause = pFeatLyrDef.DefinitionExpression;
      IFeatureClass pReferenceFC = pReferenceLayer.FeatureClass;
      //Get first field of type Long: reference layer
      IFields2 pFlds = pReferenceFC.Fields as IFields2;
      
      int iFldCnt = pFlds.FieldCount;
      int iRefField = -1;
      for (int i = 0; i < iFldCnt; i++)
      {
        if (pFlds.get_Field(i).Type == esriFieldType.esriFieldTypeInteger)
        {
          iRefField = i;
          break;
        }
      }

      IFeatureCursor pReferenceFeatCur = pReferenceFC.Search(pLayerQueryF, false);
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
        MessageBox.Show("No reference features were detected." +Environment.NewLine+"Please check configurations and try again.", "Move Fabric Points", MessageBoxButtons.OK, MessageBoxIcon.None);
        return;
      }

      Utilities UTIL = new Utilities();
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
      string sOIDFld = pReferenceFC.OIDFieldName;
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
      IMap pMap = pEd.Map;
      IArray PolygonLyrArr = new ESRI.ArcGIS.esriSystem.Array();
      if (!UTIL.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTParcels, out PolygonLyrArr))
        return;

      //Start the fabric point adjustment
      try
      {
        pEd.StartOperation();
        pFabricPointUpdate.AdjustFabric(pTrkCan);
        pFabricPointUpdate.ClearAdjustedPoints();
      }
      catch (Exception ex)
      {
        pEd.AbortOperation();
        COMException cEx = ex as COMException;
      }
      finally
      {
        if (pProDlg != null)
          pProDlg.HideDialog();

        pEd.StopOperation("Update Fabric Points");
        IActiveView pActiveView = ArcMap.Document.ActiveView;
        for (int j=0; j < PolygonLyrArr.Count;j++)
          pActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, PolygonLyrArr.get_Element(j), pActiveView.Extent);
      
      }
    }
    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
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

      ISpatialReference pMapSpatRef = ArcMap.Document.FocusMap.SpatialReference;
      IProjectedCoordinateSystem2 pPCS = null;

      if (pMapSpatRef == null)
        ConfigDial.lblUnits.Text = "<unknown units>";
      else if (pMapSpatRef is IProjectedCoordinateSystem2)
      {
        pPCS = (IProjectedCoordinateSystem2)pMapSpatRef;
        string sUnit = pPCS.CoordinateUnit.Name;
        if (sUnit.Contains("Foot") && sUnit.Contains("US"))
          sUnit = "U.S. Feet";
        else
          sUnit = sUnit.ToLower() + "s";
        ConfigDial.lblUnits.Text = sUnit;
      }

      DialogResult dlgRes= ConfigDial.ShowDialog();
      if (dlgRes == DialogResult.OK)
      {
        ext_LyrMan.UseLines = ConfigDial.optLines.Checked;
      }

      //now refresh the layer dropdown
      IMap pMap = ArcMap.Document.FocusMap;
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
