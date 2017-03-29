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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//Added non-Esri references
using System.Runtime.InteropServices;

//Added Esri references
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoSurvey;

using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace ParcelEditHelper
{
  public partial class ParcelConstruction : ESRI.ArcGIS.Desktop.AddIns.Tool, IShapeConstructorTool, ISketchTool
  {
    private IEditor3 m_editor;
    private IEditEvents_Event m_editEvents;
    private IEditEvents5_Event m_editEvents5;
    private IEditSketch3 m_edSketch;
    private IShapeConstructor m_csc;
    private bool m_bsnap2sketch;

    public ParcelConstruction()
    {
      // Get the editor
      m_editor = ArcMap.Editor as IEditor3;
      m_editEvents = m_editor as IEditEvents_Event;
      m_editEvents5 = m_editor as IEditEvents5_Event;
    }

    protected override void OnUpdate()
    {
      //Enabled = ArcMap.Application != null;
      Enabled = (m_editor.EditState == esriEditState.esriStateEditing);
    }

    protected override void OnActivate()
    {
      m_edSketch = m_editor as IEditSketch3;
      m_editor.CurrentTask = null;
      m_edSketch.GeometryType = esriGeometryType.esriGeometryPolyline;

      IEditProperties4 pEdProps = m_editor as IEditProperties4;
      m_bsnap2sketch = pEdProps.SnapToSketch;
      pEdProps.SnapToSketch = true;

      m_csc = new StraightConstructorClass();
//      m_csc = new SketchConstructorClass();

      m_csc.Initialize(m_editor);
      m_edSketch.ShapeConstructor = m_csc;
      m_csc.Activate();

      // Setup events
      m_editEvents.OnSketchModified += OnSketchModified;
      m_editEvents5.OnShapeConstructorChanged += OnShapeConstructorChanged;
      m_editEvents.OnSketchFinished += OnSketchFinished;

    }

    protected override bool OnDeactivate()
    {
      IEditProperties4 pEdProps = m_editor as IEditProperties4;
      pEdProps.SnapToSketch = m_bsnap2sketch; //set it back to original setting

      m_editEvents.OnSketchModified -= OnSketchModified;
      m_editEvents5.OnShapeConstructorChanged -= OnShapeConstructorChanged;
      m_editEvents.OnSketchFinished -= OnSketchFinished;
      return true;
    }

    protected override void OnDoubleClick()
    {
      if (m_edSketch.Geometry == null)
        return;

      if (Control.ModifierKeys == Keys.Shift)
      {
        // Finish part
        ISketchOperation pso = new SketchOperation();
        pso.MenuString_2 = "Finish Sketch Part";
        pso.Start(m_editor);
        m_edSketch.FinishSketchPart();
        pso.Finish(null);
      }
      else
        m_edSketch.FinishSketch();
    }

    private void OnSketchModified()
    {
      if (IsShapeConstructorOkay(m_csc))
        m_csc.SketchModified();
    }

    private void OnShapeConstructorChanged()
    {
      // Activate a new constructor
      if (m_csc != null)
        m_csc.Deactivate();
      m_csc = null;
      m_csc = m_edSketch.ShapeConstructor;
      if (m_csc != null)
      {
        //Need these two lines or else the tool throws COMException if it is use immediatly after a save edits operation.
        m_edSketch.RefreshSketch();
        m_edSketch.GeometryType = esriGeometryType.esriGeometryPolyline;
        m_csc.Activate();
      }
    }

    private void OnSketchFinished()
    {
      CreateParcelFromSegmentCollection(m_edSketch.Geometry as ISegmentCollection,"<map>");
    }

   private void CreateParcelFromSegmentCollection(ISegmentCollection Segments, string PlanName)
    {
      int iCnt = Segments.SegmentCount;
      ISegment[] pSegmentArr = new ISegment[iCnt];
      for (int j = 0; j < iCnt; j++)
        pSegmentArr[j] = Segments.get_Segment(j);

      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;

      try
      {
        ICadastralPacketManager pCadPacketMan = (ICadastralPacketManager)pCadEd;
        bool bStartedWithPacketOpen = pCadPacketMan.PacketOpen;
        if (!bStartedWithPacketOpen)
          m_editor.StartOperation();

        //1. Start map edit session
        ICadastralMapEdit pCadMapEdit = (ICadastralMapEdit)pCadEd;
        pCadMapEdit.StartMapEdit(esriMapEditType.esriMEEmpty, "NewParcel", false);

        //2.	Get job packet
        ICadastralPacket pCadaPacket = pCadPacketMan.JobPacket;

        //3.	Create Plan (new)
        string sPlanName = PlanName;
        //first check to ensure plan is not already in the database.
        IGSPlan pGSPlan = FindFabricPlanByName(sPlanName, pCadEd);

        if (pGSPlan == null)
        {
          //if plan is null, it was not found and can be created
          pGSPlan = new GSPlanClass();
          // 3.a set values
          pGSPlan.Accuracy = 4;
          pGSPlan.Name = sPlanName;
        }

        //3.b Add the plan to the job packet
        ICadastralPlan pCadaPlan = (ICadastralPlan)pCadaPacket;
        pCadaPlan.AddPlan(pGSPlan);

        //4.	Create Parcel
        ICadastralParcel pCadaParcel = (ICadastralParcel)pCadaPacket;
        IGSParcel pNewGSParcel = new GSParcelClass();
        //Make sure that any extended attributes on the parcel have their default values set
        IGSAttributes pGSAttributes = (IGSAttributes)pNewGSParcel;
        if (pGSAttributes != null)
        {
          ICadastralObjectSetup pCadaObjSetup = (ICadastralObjectSetup)pParcEditorMan;
          pCadaObjSetup.AddExtendedAttributes(pGSAttributes);
          pCadaObjSetup.SetDefaultValues(pGSAttributes);
        }

        //4a.	Add the parcel to the packet. (do this before addlines)
        // - This will enable us to Acquire the parcel ID,
        // - Having the parcel attached to the packet allows InsertLine to function.  
        pCadaParcel.AddParcel(pNewGSParcel);
        pNewGSParcel.Lot = "NewParcel";
        pNewGSParcel.Type = 7;
        //4b.	Set Plan (created above)
        IGSPlan thePlan = pCadaPlan.GetPlan(sPlanName);
        pNewGSParcel.Plan = thePlan;
        //4c.	Insert GSLines (from new) into GSParcel
        //4d. To bypass join, you can create GSPoints and assign those point IDs to the GSLines. 
        ICadastralPoints pCadaPoints = (ICadastralPoints)pCadaPacket;
        IMetricUnitConverter pMetricUnitConv = (IMetricUnitConverter)pCadEd;

        //Set up the initial start point, POB

        IPoint pPt1 = Segments.get_Segment(0).FromPoint;

        IZAware pZAw = (IZAware)pPt1;
        pZAw.ZAware = true;
        pPt1.Z = 0; //defaulting to 0

        //Convert the point into metric units, and get a new (in-mem) point id
        IGSPoint pGSPointFrom = pMetricUnitConv.SetGSPoint(pPt1);
        pCadaPoints.AddPoint(pGSPointFrom);
        int iID1 = pGSPointFrom.Id;
        int iID1_Orig=iID1;

        int index = 0;
        IGSLine pGSLine = null;
        //++++++++++++ Add Courses ++++++++++++++
        int iID2 = -1;
        bool bIsLoop = (Math.Abs(pPt1.X - Segments.get_Segment(iCnt-1).ToPoint.X)) < 0.01 && 
          (Math.Abs(pPt1.Y - Segments.get_Segment(iCnt-1).ToPoint.Y)) < 0.01;

        IAngularConverter pAngConv = new AngularConverterClass();

        for (int j = 0; j < iCnt; j++)
        {
          pSegmentArr[j] = Segments.get_Segment(j);

          double dDir = 0; //radians north azimuth
          ILine pLineOrChord = new LineClass();
          pLineOrChord.PutCoords(pSegmentArr[j].FromPoint, pSegmentArr[j].ToPoint);

          if (pAngConv.SetAngle(pLineOrChord.Angle, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians))
            dDir=pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth,esriDirectionUnits.esriDURadians);

          double dDist = pLineOrChord.Length;
          double dRadius = 0;
          int iAccuracy = -1;
          int iUserLineType = -1;
          int iCategory = -1;

          if (pSegmentArr[j] is ICircularArc)
          { 
            ICircularArc pCircArc = pSegmentArr[j] as ICircularArc;
            dRadius = pCircArc.Radius;
            if (pCircArc.IsCounterClockwise)
              dRadius = dRadius * -1;
          }
          bool bComputeToPoint = (bIsLoop && (j < iCnt - 1)) || !bIsLoop;
          //From, Direction (NAz Radians), Distance (map's projection units), Radius
          pGSLine = CreateGSLine(pMetricUnitConv, pCadaPoints, ref pPt1,
          iID1, dDir, dDist, dRadius, iAccuracy, iUserLineType, iCategory, bComputeToPoint, out iID2);

          if (j<iCnt-1 || !bIsLoop)
            iID1 = iID2;
          else if ((j == iCnt - 1) && bIsLoop)
            pGSLine.ToPoint = iID1_Orig; //closing the traverse back to the POB

          iID2 = -1;
          
          //Add the line to the new parcel
          if (pGSLine != null)
            pNewGSParcel.InsertLine(++index, pGSLine);
        }


        //Add radial lines for circular curves
        pNewGSParcel.AddRadialLines();

        // 4.e then set join=true on the parcel.
        pNewGSParcel.Joined = true;

        //let the packet know that a change has been made
        pCadPacketMan.SetPacketModified(true);

        //save the new parcel 
        try
        {
          pCadMapEdit.StopMapEdit(true);
        }
        catch
        {
          if (!bStartedWithPacketOpen)
            m_editor.AbortOperation();
          return;
        }
        if (!bStartedWithPacketOpen)
          m_editor.StopOperation("New Parcel");
        pCadPacketMan.PartialRefresh();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }

    }

    private IGSLine CreateGSLine(IMetricUnitConverter MetricConversion, ICadastralPoints CadastralPoints,
      ref IPoint FromPointInToPointOut, int FromPointID, double Direction, double Distance,
      double Radius, int Accuracy, int UserLineType, int Category, bool ComputeToPoint, out int ToPointID)
    {
      //In this function, Radius == 0 means a straight line
      //If the radius is >0 or <0 then the line is a circular curve with Distance as the chord length
      //for curves Bearing means chord bearing
      //negative radius means a curve to the left, positive radius curve to the right
      //for no Accuracy, no Type, or no Category pass in -1
      //Bearing is in north azimuth radians

      IGSLine pLine = new GSLineClass();
      pLine.Bearing = Direction; //direction is in radians north azimuth
      double dConvertedDistance = 0;
      MetricConversion.ConvertDistance(esriCadastralUnitConversionType.esriCUCToMetric, Distance, ref dConvertedDistance);
      pLine.Distance = dConvertedDistance;  //needs to be in meters;

      if (Math.Abs(Radius) > 0)
      {
        MetricConversion.ConvertDistance(esriCadastralUnitConversionType.esriCUCToMetric, Radius, ref dConvertedDistance);
        pLine.Radius = dConvertedDistance;  //needs to be in meters;
      }

      pLine.FromPoint = FromPointID;
      pLine.ToPoint = -1;

      if (Accuracy > -1)
        pLine.Accuracy = Accuracy;
      if (UserLineType > -1)
        pLine.LineType = UserLineType;
      if (Category > -1)
        pLine.Category = (esriCadastralLineCategory)Category;

      //Make sure that any extended attributes on the line have their default values set
      IGSAttributes pGSAttributes = (IGSAttributes)pLine;
      if (pGSAttributes != null)
      {
        ICadastralObjectSetup pCadaObjSetup = (ICadastralObjectSetup)MetricConversion; //QI
        pCadaObjSetup.AddExtendedAttributes(pGSAttributes);
        pCadaObjSetup.SetDefaultValues(pGSAttributes);
      }

      //Compute the new end point for the line.
      //FromPointInToPointOut is in units of the map projection.
      ICurve pCurv = MetricConversion.GetSurveyedLine(pLine, CadastralPoints, false, FromPointInToPointOut);
      //pCurv is also in the units of the map projection. Convert the end point to metric units.

      FromPointInToPointOut = pCurv.ToPoint;//pass the new To point back out
      FromPointInToPointOut.Z = 0;
      IGSPoint pGSPointTo = MetricConversion.SetGSPoint(FromPointInToPointOut);
      if (ComputeToPoint)
      {
        CadastralPoints.AddPoint(pGSPointTo);
        pLine.ToPoint = pGSPointTo.Id;
        ToPointID = pLine.ToPoint;
      }
      else
        ToPointID = -1;

      if (pCurv is ICircularArc)
      {
        ICircularArc pCircArc = (ICircularArc)pCurv;
        IPoint pCtrPt = pCircArc.CenterPoint;
        IZAware pZAw = (IZAware)pCtrPt;
        pZAw.ZAware = true;
        pCtrPt.Z = 0;
        IGSPoint pGSCtrPt = MetricConversion.SetGSPoint(pCtrPt);
        CadastralPoints.AddPoint(pGSCtrPt);
        pLine.CenterPoint = pGSCtrPt.Id;
      }

      return pLine;
    }

    private IGSPlan FindFabricPlanByName(string PlanName, ICadastralEditor CadastralEditor)
    {
      ICursor pCur = null;
      try
      {
        ICadastralFabric pCadaFab = CadastralEditor.CadastralFabric;
        ITable pPlanTable = pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
        int iPlanNameFldID = pPlanTable.FindField("NAME");
        string PlanNameFld = pPlanTable.Fields.get_Field(iPlanNameFldID).Name;
        IQueryFilter pQF = new QueryFilter();
        pQF.WhereClause = PlanNameFld + "= '" + PlanName + "'";
        pQF.SubFields = pPlanTable.OIDFieldName + PlanNameFld;
        pCur = pPlanTable.Search(pQF, false);
        IRow pPlanRow = pCur.NextRow();
        IGSPlan pGSPlan = null;
        if (pPlanRow != null)
        {
          //Since plan was found, generate plan object from database:
          ICadastralFeatureGenerator pFeatureGenerator = new CadastralFeatureGenerator();
          pGSPlan = pFeatureGenerator.CreatePlanFromRow(CadastralEditor, pPlanRow);
          Marshal.ReleaseComObject(pPlanRow);
        }
        return pGSPlan;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
      finally
      {
        if (pCur != null)
          Marshal.ReleaseComObject(pCur);
      }
    }



  }

}
