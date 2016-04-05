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

using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DeleteSelectedParcels
{
  
  class NativeMethods
  {
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern internal bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
    }
  }

  public class clsDeleteOrphansTool : ESRI.ArcGIS.Desktop.AddIns.Tool
  {
    private string m_sDebug = "";
    private string m_CommUID = "";
    ICadastralExtensionManager2 m_pCadExtMan = null;
    IEditor m_pEd = null;

    public clsDeleteOrphansTool()
    {
    }

    protected override bool OnDeactivate()
    {    
      return true;
    }

    protected override void OnActivate()
    {
      clsFabricUtils FabricUTILS = new clsFabricUtils();
      string sCommName = ""; string sCommUID = ""; string sCommBarUID = "";
      FabricUTILS.InfoFromCurrentTool(out sCommBarUID, out sCommUID, out sCommName);
      m_CommUID = sCommUID;
      FabricUTILS = null;
    }

    protected override void OnUpdate()
    {
      CustomizelHelperExtension v =CustomizelHelperExtension.GetExtension();
      if (m_pEd==null)
        m_pEd = v.TheEditor;
      if(m_pCadExtMan==null)
        m_pCadExtMan = v.TheCadastralExtensionManager;      

      bool bEnabled = (m_pEd.EditState == esriEditState.esriStateEditing);

      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)m_pCadExtMan;
      if (pCadPacMan.PacketOpen)
        this.Enabled = false;
      else
        this.Enabled = bEnabled;

      v.CommandIsEnabled = this.Enabled;
    }

    #region ITool Members
    protected override void OnMouseDown(MouseEventArgs arg)
    {
      IFeatureLayer pPointLayer = null;
      IFeatureLayer pLineLayer = null;
      IArray pParcelLayers = null;
      IFeatureLayer pControlLayer = null;
      IFeatureLayer pLinePointLayer = null;

      double dXYTol = 0.003;
      clsFabricUtils FabricUTILS = new clsFabricUtils();
      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

      //first get the extension
      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadExtMan;
      if (pCadPacMan.PacketOpen)
      {
        FabricUTILS.ExecuteCommand(m_CommUID);
        MessageBox.Show("This tool cannot be used when there is an open parcel, construction, or job.\r\nPlease complete or discard the open items, and try again.");
        return;
      }

      ICadastralFabric pCadFabric = null;

      //if we're in an edit session then grab the target fabric
      if (pEd.EditState == esriEditState.esriStateEditing)
        pCadFabric = pCadEd.CadastralFabric;
      else
      {
        FabricUTILS.ExecuteCommand(m_CommUID);
         MessageBox.Show("This tool works on a parcel fabric in an edit session.\r\nPlease start editing and try again.");
         return;
      }

      if (pCadFabric == null)
      {//find the first fabric in the map
        if (!FabricUTILS.GetFabricFromMap(ArcMap.Document.ActiveView.FocusMap, out pCadFabric))
        {
          FabricUTILS.ExecuteCommand(m_CommUID);
          MessageBox.Show("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }

      IGeoDataset pGeoDS = (IGeoDataset)pCadFabric;
      ISpatialReferenceTolerance pSpatRefTol = (ISpatialReferenceTolerance)pGeoDS.SpatialReference;
      if (pSpatRefTol.XYToleranceValid == esriSRToleranceEnum.esriSRToleranceOK)
        dXYTol = pSpatRefTol.XYTolerance;

      IMouseCursor pMouseCursor = null;
      ISpatialFilter pSpatFilt = null; //spatial filter query hinging off the dragged rectangle selection
      IQueryFilter pQuFilter = null; //used for the non-spatial query for radial lines, as they have no geometry
      IRubberBand2 pRubberRect = null;
      IGeometryBag pGeomBag = null;
      ITopologicalOperator5 pUnionedPolyine = null;
      IPolygon pBufferedToolSelectGeometry=null; //geometry used to search for parents
      IFIDSet pLineFIDs = null; //the FIDSet used to collect the lines that'll be deleted
      IFIDSet pPointFIDs = null; // the FIDSet used to collect the points that'll be deleted
      IFIDSet pLinePointFIDs = null; // the FIDSet used to collect the line-points that'll be deleted
      List<int> pDeletedLinesPoints = new List<int>(); //list used to stage the ids for points that are referenced by lines
      List<int> pUsedPoints = new List<int>(); //list used to collect pointids that are referenced by existing lines
      List<int> CtrPointIDList = new List<int>(); //list for collecting the ids of center points
      List<int> pParcelsList =new List<int>(); //used only to avoid adding duplicates to IN clause string for, based on ties to radial lines
      List<int> pOrphanPointsList = new List<int>(); //list of orphan points defined from radial ines
      List<int> pPointsInsideBoxList = new List<int>();//list of parcels that exist and that intersect the drag-box
      List<string> sFromToPair =new List<string>();//list of from/to pairs for manging line points
      List<int> pLineToParcelIDRef = new List<int>();//list of parcel id refs stored on lines

      IInvalidArea3 pInvArea = null;

      IFeatureClass pLines = null;
      IFeatureClass pPoints = null;
      IFeatureClass pParcels = null;
      IFeatureClass pLinePoints = null;
      IWorkspace pWS = null;

      try
      {
        #region define the rubber envelope geometry
        pRubberRect = new RubberEnvelopeClass();
        IGeometry ToolSelectEnvelope = pRubberRect.TrackNew(ArcMap.Document.ActiveView.ScreenDisplay, null);

        if (ToolSelectEnvelope == null)
          return;    

        ISegmentCollection pSegmentColl = new PolygonClass();
        pSegmentColl.SetRectangle(ToolSelectEnvelope.Envelope);
        IPolygon ToolSelectGeometry = (IPolygon)pSegmentColl;

        if (pCadFabric == null)
          return;

        pMouseCursor = new MouseCursorClass();
        pMouseCursor.SetCursor(2);

        #endregion

        FabricUTILS.GetFabricSubLayersFromFabric(ArcMap.Document.ActiveView.FocusMap, pCadFabric,
    out pPointLayer, out pLineLayer, out pParcelLayers, out pControlLayer,
    out pLinePointLayer);

        #region get tables and field indexes
        pLines = (IFeatureClass)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        pPoints = (IFeatureClass)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
        pParcels = (IFeatureClass)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
        pLinePoints = (IFeatureClass)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLinePoints);
        string sPref = "";
        string sSuff = "";

        int iCtrPtFldIDX = pLines.FindField("CENTERPOINTID");
        int iFromPtFldIDX = pLines.FindField("FROMPOINTID");
        int iToPtFldIDX = pLines.FindField("TOPOINTID");
        int iCatFldIDX = pLines.FindField("CATEGORY");
        int iParcelIDX = pLines.FindField("PARCELID");
        int iDistanceIDX = pLines.FindField("DISTANCE");


        pWS = pLines.FeatureDataset.Workspace;
        ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
        sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
        sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

        pSpatFilt = new SpatialFilterClass();
        pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
        pSpatFilt.Geometry = ToolSelectGeometry;
        #endregion

        #region center point
        //need to make sure that center points are correctly handled for cases where the center point
        //is inside the select box, but the curve itself is not. The following is used to build an
        //IN CLAUSE for All the center points that are found within the tool's select geometry.
        int iIsCtrPtIDX = pPoints.FindField("CENTERPOINT");
        int iCount = 0;
        int iCntCtrPoint = 0;
        string sCtrPntIDList1 = "";

        IFeatureCursor pFeatCursPoints = pPoints.Search(pSpatFilt, false);
        IFeature pFeat7 = pFeatCursPoints.NextFeature();
        while (pFeat7 != null)
        {
          iCount++;
          int iVal = -1;
          object Attr_val = pFeat7.get_Value(iIsCtrPtIDX);

          if (Attr_val != DBNull.Value)
            iVal = Convert.ToInt32(pFeat7.get_Value(iIsCtrPtIDX));

          if (iVal == 1)
          {
            if (sCtrPntIDList1.Trim() == "")
              sCtrPntIDList1 += pFeat7.OID.ToString();
            else
              sCtrPntIDList1 += "," + pFeat7.OID.ToString();
            iCntCtrPoint++;
          }
          pPointsInsideBoxList.Add(pFeat7.OID);//used to check for orphan linepoints 
          pOrphanPointsList.Add(pFeat7.OID); //this gets whittled down till only "pure" orphan points remain 
          Marshal.ReleaseComObject(pFeat7);
          pFeat7 = pFeatCursPoints.NextFeature();
        }
        Marshal.FinalReleaseComObject(pFeatCursPoints);
        #endregion

        #region create convex hull of lines
        //get the lines that intersect the search box and build a 
        //polygon search geometry being the convex hull of those lines.
        IFeatureCursor pFeatCursLines = pLines.Search(pSpatFilt, false);
        pGeomBag = new GeometryBagClass();
        IGeometryCollection pGeomColl = (IGeometryCollection)pGeomBag;
        IFeature pFeat1 = pFeatCursLines.NextFeature();
        m_sDebug = "Add lines to Geometry Collection.";
        string sParcelRefOnLines = "";
        object missing = Type.Missing;
        while (pFeat1 != null)
        {
          int iVal = (int)pFeat1.get_Value(iFromPtFldIDX);
          string sFromTo = iVal.ToString() + ":";

          if (pOrphanPointsList.Contains(iVal))//Does this need to be done...will remove fail if it's not there?
            pOrphanPointsList.Remove(iVal);//does this need to be in the if block?

          iVal = (int)pFeat1.get_Value(iToPtFldIDX);
          sFromTo += iVal.ToString();
          if (pOrphanPointsList.Contains(iVal))
            pOrphanPointsList.Remove(iVal);

          sFromToPair.Add(sFromTo);
          pGeomColl.AddGeometry(pFeat1.ShapeCopy, missing, missing);

          if (sParcelRefOnLines.Trim() == "")
            sParcelRefOnLines = pFeat1.get_Value(iParcelIDX).ToString();
          else
            sParcelRefOnLines += "," + pFeat1.get_Value(iParcelIDX).ToString();

          Marshal.ReleaseComObject(pFeat1);
          pFeat1 = pFeatCursLines.NextFeature();
        }
        Marshal.FinalReleaseComObject(pFeatCursLines);

        #endregion

        #region Add Center Points for curves outside map extent

        if (iCntCtrPoint > 999)
        {
          throw new InvalidOperationException("The Delete Orphans tool works with smaller amounts of data." + Environment.NewLine +
            "Please try again, by selecting fewer fabric lines and points. (More than 1000 center points returned.)");
        }

        //If there is no line geometry found, and there are also no points found, then nothing to do...
        if (pGeomColl.GeometryCount == 0 && iCount == 0)
          return;

        //Radial lines have no geometry so there is a special treatment for those; 
        //that special treatment takes two forms, 
        //1. if a circular arc is selected and it turns out that it is an orphan line, then we
        //need to take down its radial lines, and its center point as well.
        //2. if a center point is selected, we need to check if it's an orphan, by searching for its parent.
        //The parent parcel can easily be well beyond the query rectangle, so the original 
        //search rectangle is buffered by the largest found radius distance, to make sure that all 
        //parent parcels are "find-able."

        //The radial lines themselves are also needed; Get the radial lines from the Center Points
        //CtrPt is always TO point, so find lines CATEGORY = 4 AND TOPOINT IN ()
        string sRadialLineListParcelID = "";
        string sRadialLinesID = "";
        string sRadialLinePoints = "";

        double dRadiusBuff = 0;
        pQuFilter = new QueryFilterClass();
        //Find all the radial lines based on the search query
        if (sCtrPntIDList1.Trim() != "")
        {
          pQuFilter.WhereClause = "CATEGORY = 4 AND TOPOINTID IN (" + sCtrPntIDList1 + ")";

          //add all the *references* to Parcel ids for the radial lines, 
          //and add the ID's of the lines
          IFeatureCursor pFeatCursLines8 = pLines.Search(pQuFilter, false);
          IFeature pFeat8 = pFeatCursLines8.NextFeature();
          while (pFeat8 != null)
          {
            object Attr_val = pFeat8.get_Value(iDistanceIDX);
            double dVal = 0;
            if (Attr_val != DBNull.Value)
              dVal = Convert.ToDouble(Attr_val);
            dRadiusBuff = dRadiusBuff > dVal ? dRadiusBuff : dVal;
            int iVal = Convert.ToInt32(pFeat8.get_Value(iParcelIDX));
            if (!pParcelsList.Contains(iVal))
            {
              if (sRadialLineListParcelID.Trim() == "")
                sRadialLineListParcelID += Convert.ToString(iVal);
              else
                sRadialLineListParcelID += "," + Convert.ToString(iVal);
            }
            pParcelsList.Add(iVal);

            //pOrphanPointsList is used for "Pure Orphan point" detection
            //meaning that these are points that do not have ANY line, not even an orphan line.
            iVal = (int)pFeat8.get_Value(iFromPtFldIDX);
            if (pOrphanPointsList.Contains(iVal))
              pOrphanPointsList.Remove(iVal);

            iVal = (int)pFeat8.get_Value(iToPtFldIDX);
            if (pOrphanPointsList.Contains(iVal))
              pOrphanPointsList.Remove(iVal);

            if (sRadialLinesID.Trim() == "")
              sRadialLinesID += Convert.ToString(iVal);
            else
              sRadialLinesID += "," + Convert.ToString(iVal);

            //Add from point to list
            if (sRadialLinePoints.Trim() == "")
              sRadialLinePoints += Convert.ToString(pFeat8.get_Value(iFromPtFldIDX));
            else
              sRadialLinePoints += "," + Convert.ToString(pFeat8.get_Value(iFromPtFldIDX));
            //Add To point to list
            sRadialLinePoints += "," + Convert.ToString(pFeat8.get_Value(iToPtFldIDX));

            Marshal.ReleaseComObject(pFeat8);
            pFeat8 = pFeatCursLines8.NextFeature();
          }
          Marshal.FinalReleaseComObject(pFeatCursLines8);

          //create a polygon goeometry that is a buffer of the selection rectangle expanded
          //to the greatest radius of all the radial lines found.
          ITopologicalOperator topologicalOperator = (ITopologicalOperator)ToolSelectGeometry;
          pBufferedToolSelectGeometry = topologicalOperator.Buffer(dRadiusBuff) as IPolygon;
        }
        else
        {
          pQuFilter.WhereClause = "";
        }
        #endregion

        #region OrphanLines

        if (pGeomColl.GeometryCount != 0)
        {
          pUnionedPolyine = new PolylineClass();
          pUnionedPolyine.ConstructUnion((IEnumGeometry)pGeomBag);
          ITopologicalOperator pTopoOp = (ITopologicalOperator)pUnionedPolyine;
          IGeometry pConvexHull = pTopoOp.ConvexHull();
          //With this convex hull, do a small buffer, 
          //theis search geometry is used as a spatial query on the parcel polygons 
          //and also on the parcel lines, to build IN Clauses
          pTopoOp = (ITopologicalOperator)pConvexHull;
          IGeometry pBufferedConvexHull = pTopoOp.Buffer(10 * dXYTol);
          if (pBufferedToolSelectGeometry != null)
          {
            pTopoOp = (ITopologicalOperator)pBufferedToolSelectGeometry;
            IGeometry pUnionPolygon = pTopoOp.Union(pBufferedConvexHull);
            pSpatFilt.Geometry = pUnionPolygon;
          }
          else
            pSpatFilt.Geometry = pBufferedConvexHull;
        }
        else
        {
          if (pQuFilter.WhereClause.Trim() == "" && pBufferedToolSelectGeometry == null)
            pSpatFilt.Geometry = ToolSelectGeometry;
          else
            pSpatFilt.Geometry = pBufferedToolSelectGeometry;
        }

        IColor pColor = new RgbColorClass();
        pColor.RGB = System.Drawing.Color.Blue.ToArgb();
        IScreenDisplay pScreenDisplay = ArcMap.Document.ActiveView.ScreenDisplay;
        FabricUTILS.FlashGeometry(pSpatFilt.Geometry, pScreenDisplay, pColor, 5, 100);

        pSpatFilt.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;

        m_sDebug = "Searching Parcels table.";
        pInvArea = new InvalidAreaClass();
        IFeatureCursor pFeatCursParcels = pParcels.Search(pSpatFilt, false);
        IFeature pFeat2 = pFeatCursParcels.NextFeature();
        string sParcelIDList = "";
        iCount = 0;
        //create the "NOT IN" CLAUSE for parcels that exist in the DB and that are within the search area
        //Will be used as a search on lines to get the Orphan Lines

        while (pFeat2 != null)
        {
          iCount++;
          if (sParcelIDList.Trim() == "")
            sParcelIDList += pFeat2.OID.ToString();
          else
            sParcelIDList += "," + pFeat2.OID.ToString();

          Marshal.ReleaseComObject(pFeat2);
          if (iCount > 999)
            break;
          pFeat2 = pFeatCursParcels.NextFeature();
        }
        Marshal.FinalReleaseComObject(pFeatCursParcels);

        //if we have more than 999 in clause tokens, there will be problems on Oracle.
        //Since this is an interactive tool, we expect it not to be used on a large amount of data.
        //for this reason, the following message is displayed if more than 999 parcels are returned in this query.
        //TODO: for the future this can be made to work on larger sets of data.
        if (iCount > 999)
        {
          throw new InvalidOperationException("The Delete Orphans tool works with smaller amounts of data." + Environment.NewLine +
            "Please try again, by selecting fewer fabric lines and points. (More than 1000 parcels returned.)");
        }

        m_sDebug = "Building the used points list.";
        //This first pass contains all references to points found within the parent parcel search buffer
        //Later, points are removed from this list 
        IFeatureCursor pFeatCursLargerLineSet = pLines.Search(pSpatFilt, false);
        IFeature pFeat3 = pFeatCursLargerLineSet.NextFeature();
        while (pFeat3 != null)
        {
          iCount++;
          object Attr_val = pFeat3.get_Value(iCtrPtFldIDX);
          if (Attr_val != DBNull.Value)
            pUsedPoints.Add(Convert.ToInt32(Attr_val)); //add center point

          int iVal = (int)pFeat3.get_Value(iFromPtFldIDX);
          pUsedPoints.Add(iVal);//add from point

          iVal = (int)pFeat3.get_Value(iToPtFldIDX);
          pUsedPoints.Add(iVal);//add to point

          Marshal.ReleaseComObject(pFeat3);
          pFeat3 = pFeatCursLargerLineSet.NextFeature();
        }
        Marshal.FinalReleaseComObject(pFeatCursLargerLineSet);

        //pUsedPoints list is at this stage, references to points for all lines found within the search area.
        //use the IN clause of the parcel ids to search for lines within 
        //the original search box, and that are also orphans that do not have a parent parcel.
        pSpatFilt.WhereClause = "";
        pSpatFilt.Geometry = ToolSelectGeometry;
        pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
        pSpatFilt.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;

        IFeatureCursor pFeatCursor = null;
        if (pGeomColl.GeometryCount == 0)
        {
          if (sParcelIDList.Trim().Length > 0 && sCtrPntIDList1.Trim().Length > 0)
          {
            pQuFilter.WhereClause = "(PARCELID NOT IN (" + sParcelIDList + 
              ")) AND (CATEGORY = 4 AND TOPOINTID IN (" + sCtrPntIDList1 + "))";
            pFeatCursor = pLines.Search(pQuFilter, false);
          }
          else if (sParcelIDList.Trim().Length == 0 && sCtrPntIDList1.Trim().Length > 0)
          {
            pQuFilter.WhereClause = "CATEGORY = 4 AND TOPOINTID IN (" + sCtrPntIDList1 + ")";
            pFeatCursor = pLines.Search(pQuFilter, false);
          }
        }
        else
        {//do a spatial query
          if (sParcelIDList.Trim().Length > 0)
            pSpatFilt.WhereClause = "PARCELID NOT IN (" + sParcelIDList + ")";
          else
            pSpatFilt.WhereClause = "";
          pFeatCursor = pLines.Search(pSpatFilt, false);
        }

        m_sDebug = "Collecting lines to be deleted.";

        //start collecting the lines that need to be deleted
        iCount = 0;
        int iCtrPointCount = 0;
        string sCtrPointIDList = "";
        string sLineParcelIDReference = "";
        //Feature cursor is lines that are NOT IN the ParcelIDList

        if (pFeatCursor != null)
        {
          pLineFIDs = new FIDSetClass();
          IFeature pFeat4 = pFeatCursor.NextFeature();
          while (pFeat4 != null)
          {
            iCount++;
            pLineFIDs.Add(pFeat4.OID);
            int iParcRef = Convert.ToInt32(pFeat4.get_Value(iParcelIDX));
            if (sLineParcelIDReference.Trim() == "")
              sLineParcelIDReference = iParcRef.ToString();
            else
            {
              if (!pLineToParcelIDRef.Contains(iParcRef))
                sLineParcelIDReference += "," + iParcRef.ToString();
            }
            pLineToParcelIDRef.Add(iParcRef);
            pInvArea.Add((IObject)pFeat4);
            //now for this line, get it's points
            //first add the center point reference if there is one
            object Attr_val = pFeat4.get_Value(iCtrPtFldIDX);
            if (Attr_val != DBNull.Value)
            {
              iCtrPointCount++;
              int iCtrPointID = Convert.ToInt32(Attr_val);
              pDeletedLinesPoints.Add(iCtrPointID); //add this line's center point
              pUsedPoints.Remove(iCtrPointID);

              if (sCtrPointIDList.Trim() == "")
                sCtrPointIDList = iCtrPointID.ToString();
              else
              {
                if (CtrPointIDList.Contains(iCtrPointID))
                  iCtrPointCount--;
                else
                  sCtrPointIDList += "," + iCtrPointID.ToString();
              }
              CtrPointIDList.Add(iCtrPointID);//to keep track of repeats
            }
            //and also add the FROM and TO point references if they exist
            int iVal = (int)pFeat4.get_Value(iFromPtFldIDX);
            pDeletedLinesPoints.Add(iVal);//add FROM point
            if (pGeomColl.GeometryCount > 0)
              pUsedPoints.Remove(iVal);

            iVal = (int)pFeat4.get_Value(iToPtFldIDX);
            pDeletedLinesPoints.Add(iVal);//add TO point
            if (pGeomColl.GeometryCount > 0)
              pUsedPoints.Remove(iVal);

            Marshal.ReleaseComObject(pFeat4);
            if (iCtrPointCount > 999)
              break;
            pFeat4 = pFeatCursor.NextFeature();
          }
          Marshal.FinalReleaseComObject(pFeatCursor);
        }
        if (iCtrPointCount > 999)
        {
          throw new InvalidOperationException("The Delete Orphans tool works with smaller amounts of data." + Environment.NewLine +
              "Please try again, by selecting fewer fabric lines and points. (More than 1000 center points returned.)");
        }

        m_sDebug = "Adding orphan radial lines to list.";

        if (sCtrPointIDList.Trim().Length > 0)
        {
          //add the Radial lines at each end of the curves using the collected CtrPtIDs
          //CtrPt is always TO point, so find lines CATEGORY = 4 AND TOPOINT IN ()

          pQuFilter.WhereClause = "CATEGORY = 4 AND TOPOINTID IN (" + sCtrPointIDList + ")";
          pFeatCursor = pLines.Search(pQuFilter, false);
          IFeature pFeat5 = pFeatCursor.NextFeature();
          while (pFeat5 != null)
          {
            pLineFIDs.Add(pFeat5.OID);
            int iParcRef = Convert.ToInt32(pFeat5.get_Value(iParcelIDX));
            pLineToParcelIDRef.Add(iParcRef);
            if (sLineParcelIDReference.Trim() == "")
              sLineParcelIDReference = iParcRef.ToString();
            else
            {
              if (!pLineToParcelIDRef.Contains(iParcRef))
                sLineParcelIDReference += "," + iParcRef.ToString();
            }
            Marshal.ReleaseComObject(pFeat5);
            pFeat5 = pFeatCursor.NextFeature();
          }
          Marshal.FinalReleaseComObject(pFeatCursor);
        }
        else
        {
          pQuFilter.WhereClause = "";
        }

        //refine the DeletedLinesPoints list
        foreach (int i in pUsedPoints)
        {
          if (pDeletedLinesPoints.Contains(i))
          { do { } while (pDeletedLinesPoints.Remove(i));}
        }

        //add the points to a new FIDSet
        pPointFIDs = new FIDSetClass();
        foreach (int i in pDeletedLinesPoints)
          pPointFIDs.Add(i);

        #endregion

        #region OrphanPoints
        //We already know which points to delete based on taking down the orphan lines.
        //We need to still add to the points FIDSet those points that are "pure" ophan points
        //as defined for the pOrphanPointsList variable.
        //and add the orphan points to the points FIDSet
        foreach (int i in pOrphanPointsList)
        {
          bool bFound = false;
          pPointFIDs.Find(i, out bFound);
          if (!bFound)
            pPointFIDs.Add(i);
        }
        #endregion

        #region orphan Line points
        //next check for orphan line-points
        //the line-point is deleted if there is no underlying point
        //or if the from and to point references do not exist.
        pSpatFilt.WhereClause = "";
        pSpatFilt.Geometry = ToolSelectGeometry;
        IFeatureCursor pFeatCursLinePoints = pLinePoints.Search(pSpatFilt, false);
        IFeature pLPFeat = pFeatCursLinePoints.NextFeature();
        int iLinePtPointIdIdx = pLinePoints.FindField("LINEPOINTID");
        int iLinePtFromPtIdIdx = pLinePoints.FindField("FROMPOINTID");
        int iLinePtToPtIdIdx = pLinePoints.FindField("TOPOINTID");

        pLinePointFIDs = new FIDSetClass();
        while (pLPFeat != null)
        {
          bool bExistsA = true;

          bool bExists1 = true;
          bool bExists2 = true;
          bool bExists3 = true;

          int iVal = (int)pLPFeat.get_Value(iLinePtPointIdIdx);
          pPointFIDs.Find(iVal, out bExists1);
          if (!pPointsInsideBoxList.Contains(iVal))
            bExistsA = false;

          iVal = (int)pLPFeat.get_Value(iLinePtFromPtIdIdx);
          string sFrom = iVal.ToString();
          pPointFIDs.Find(iVal, out bExists2);

          iVal = (int)pLPFeat.get_Value(iLinePtToPtIdIdx);
          string sTo = iVal.ToString();
          pPointFIDs.Find(iVal, out bExists3);
          
          int iOID = pLPFeat.OID;

          if (bExists1 || bExists2 || bExists3)
            pLinePointFIDs.Add(iOID);

          if (!sFromToPair.Contains(sFrom + ":" + sTo) && !sFromToPair.Contains(sTo + ":" + sFrom))
          {
            pLinePointFIDs.Find(iOID, out bExists1);
            if(!bExists1)
              pLinePointFIDs.Add(iOID);
          }

          //if (!bExistsA || !bExistsB || !bExistsC)
          if (!bExistsA)
          {
            bool bFound = true;
            pLinePointFIDs.Find(iOID, out bFound);
            if (!bFound)
              pLinePointFIDs.Add(iOID);
          }
          pPointsInsideBoxList.Contains(iVal);

          Marshal.ReleaseComObject(pLPFeat);
          pLPFeat = pFeatCursLinePoints.NextFeature();
        }

        Marshal.FinalReleaseComObject(pFeatCursLinePoints);
        #endregion

        #region Refine the lines that are on the delete list
        //next step is to refine and double-check to make sure that the lines that are on the delete list
        //do not have a parcel record somewhere else (not spatially connected to the line) (example unjoined, or bad geom)
        string sFreshlyFoundParcels = "";
        if (sLineParcelIDReference.Trim() != "")
        {
          pQuFilter.WhereClause = sPref + pParcels.OIDFieldName + sSuff + " IN (" + sLineParcelIDReference + ")";
          pFeatCursor = pParcels.Search(pQuFilter, false);
          IFeature pFeat6 = pFeatCursor.NextFeature();
          while (pFeat6 != null)
          {
            int iOID = pFeat6.OID;
            if (sFreshlyFoundParcels.Trim() == "")
              sFreshlyFoundParcels = iOID.ToString();
            else
              sFreshlyFoundParcels += "," + iOID.ToString();
            Marshal.ReleaseComObject(pFeat6);
            pFeat6 = pFeatCursor.NextFeature();
          }
          Marshal.FinalReleaseComObject(pFeatCursor);
          
          if (sFreshlyFoundParcels.Trim()!="")
          {
            pQuFilter.WhereClause = "PARCELID IN (" + sFreshlyFoundParcels + ")";
            pFeatCursor = pLines.Search(pQuFilter, false);
            IFeature pFeat9 = pFeatCursor.NextFeature();
            while (pFeat9 != null)
            {
              int iOID = pFeat9.OID;
              bool bIsHere = false;
              pLineFIDs.Delete(iOID);
              int iVal = Convert.ToInt32(pFeat9.get_Value(iFromPtFldIDX));
              pPointFIDs.Find(iVal, out bIsHere);
              if (bIsHere)
                pPointFIDs.Delete(iVal);

              iVal = Convert.ToInt32(pFeat9.get_Value(iToPtFldIDX));
              pPointFIDs.Find(iVal, out bIsHere);
              if (bIsHere)
                pPointFIDs.Delete(iVal);

              Marshal.ReleaseComObject(pFeat9);
              pFeat9 = pFeatCursor.NextFeature();
            }
            Marshal.FinalReleaseComObject(pFeatCursor);
          }
        }
        #endregion

        #region Make sure the points on the delete list are not part of a construction
        //For post 10.0, Make sure the points on the delete list are not part of a construction if they are then null geometry
        //pQuFilter.WhereClause=pLines.LengthField.Name + " = 0 AND CATEGORY <> 4";
        //IFeatureCursor pFeatCursLines101 = pLines.Search(pQuFilter, false);
        //this would open a new cursor and do a query on the entire 
        #endregion

        #region report results and do edits
        dlgReport Report = new dlgReport();
        //Display the dialog
        System.Drawing.Color BackColorNow = Report.textBox1.BackColor;
        if (iCount == 0 && pPointFIDs.Count() == 0 && pLinePointFIDs.Count() == 0)
        {
          Report.textBox1.BackColor = System.Drawing.Color.LightGreen;
          Report.textBox1.Text = "Selected area has no orphan lines or points.";
        }
        else
        {
          int iCount1 = 0;
          int iCount2 = 0;
          int iCount3 = 0;
          if (pLineFIDs != null)
            iCount1 = pLineFIDs.Count();

          if (pPointFIDs != null)
            iCount2 = pPointFIDs.Count();

          if (pLinePointFIDs != null)
            iCount3 = pLinePointFIDs.Count();

          iCount = iCount1 + iCount2 + iCount3;
          if (iCount > 0)
          {
            pEd.StartOperation();
            FabricUTILS.DeleteRowsByFIDSet((ITable)pLines, pLineFIDs, null, null);
            FabricUTILS.DeleteRowsByFIDSet((ITable)pPoints, pPointFIDs, null, null);
            if (pPointFIDs.Count() > 0)
            {
              //now need to update the control points associated with any deleted points.
              ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
              ITable pControlTable = pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);
              int idxNameFldOnControl = pControlTable.FindField("POINTID");
              string ControlNameFldName = pControlTable.Fields.get_Field(idxNameFldOnControl).Name;
              int i;
              List<int> pPointFIDList= new List<int>();
              pPointFIDs.Reset();
              pPointFIDs.Next(out i);
              while (i > -1)
              {
                pPointFIDList.Add(i);
                pPointFIDs.Next(out i);
              }
              List<string> InClausePointsNotConnectedToLines = FabricUTILS.InClauseFromOIDsList(pPointFIDList, 995);
              pQuFilter.WhereClause = ControlNameFldName + " IN (" + InClausePointsNotConnectedToLines[0] + ")";
              pSchemaEd.ReleaseReadOnlyFields(pControlTable, esriCadastralFabricTable.esriCFTControl); //release safety-catch
              if (!FabricUTILS.ResetControlAssociations(pControlTable, pQuFilter, false))
              {
                pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
                pEd.AbortOperation();
                return;
              }
              pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl);//set safety back on
            }
            pQuFilter.WhereClause = "";
            FabricUTILS.DeleteRowsByFIDSet((ITable)pLinePoints, pLinePointFIDs, null, null);
            pEd.StopOperation("Delete " + iCount.ToString() + " orphans");          
            Report.textBox1.Text = "Deleted:";
            if (iCount1 > 0)
              Report.textBox1.Text += Environment.NewLine + iCount1.ToString() + " orphaned lines";
            if (iCount2 > 0)
              Report.textBox1.Text += Environment.NewLine + iCount2.ToString() + " orphaned points";
            if (iCount3 > 0)
              Report.textBox1.Text += Environment.NewLine + iCount3.ToString() + " orphaned line points";
          }
          if (sFreshlyFoundParcels.Trim() != "")
          {
            if (Report.textBox1.Text.Trim() != "")
              Report.textBox1.Text += Environment.NewLine;
            Report.textBox1.Text +="Info: Line(s) that you selected are not directly" +
              Environment.NewLine + "touching a parent parcel geometry. Check parcels with OIDs:" +
              sFreshlyFoundParcels;
          }
        }
        IArea pArea = (IArea)ToolSelectGeometry;
        if (pArea.Area > 0)
          SetDialogLocationAtPoint(Report, pArea.Centroid);

        DialogResult pDialogResult = Report.ShowDialog();
        Report.textBox1.BackColor = BackColorNow;
        pInvArea.Display = ArcMap.Document.ActiveView.ScreenDisplay;
        pInvArea.Invalidate((short)esriScreenCache.esriAllScreenCaches);

        if (pPointLayer != null)
          ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography,
            pPointLayer, ArcMap.Document.ActiveView.Extent);
        #endregion

      }

      catch (Exception ex)
      {
        if (pEd != null)
          pEd.AbortOperation();
        MessageBox.Show(ex.Message + Environment.NewLine + m_sDebug, "Delete Orphans Tool");
        this.OnDeactivate();
      }

      #region Final Cleanup
      finally
      {
        pDeletedLinesPoints.Clear();
        pDeletedLinesPoints = null;
        pUsedPoints.Clear();
        pUsedPoints=null;
        CtrPointIDList.Clear();
        CtrPointIDList = null;
        pParcelsList.Clear();
        pParcelsList = null;
        pOrphanPointsList.Clear();
        pOrphanPointsList = null;
        pPointsInsideBoxList.Clear();
        pPointsInsideBoxList = null;
        pLineToParcelIDRef.Clear();
        pLineToParcelIDRef = null;
        sFromToPair.Clear();
        sFromToPair = null;

        FabricUTILS = null;
      }
      #endregion
    
    }

    void SetDialogLocationAtPoint(Form TheDialog, IPoint ThePoint)
    {
      int iX = 0; int iY = 0;
      int iX2 = 0; int iY2 = 0;
      IDisplayTransformation pDispTr = ArcMap.Document.ActiveView.ScreenDisplay.DisplayTransformation;
      pDispTr.FromMapPoint(ThePoint, out iX, out iY);

      IntPtr hWnd = (IntPtr)ArcMap.Document.ActiveView.ScreenDisplay.hWnd;

      NativeMethods.RECT rect = new NativeMethods.RECT();
      if (NativeMethods.GetWindowRect(hWnd, ref rect))
      {
        iX2 = rect.Left;
        iY2 = rect.Top;
      }
      int iLeft = iX + iX2 - (TheDialog.Width/2);
      int iTop = iY + iY2 - (TheDialog.Height / 2);

      TheDialog.StartPosition = FormStartPosition.Manual;
      TheDialog.Location = new System.Drawing.Point(iLeft,iTop);
    }

    #endregion
  }

}
