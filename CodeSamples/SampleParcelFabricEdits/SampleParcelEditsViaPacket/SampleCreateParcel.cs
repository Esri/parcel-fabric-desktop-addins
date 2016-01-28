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

//Add-in provided import library references
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

//Added non-Esri references
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

//Added Esri references
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;

namespace SampleParcelEditsViaPacket
{
  public class SampleCreateParcel : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public SampleCreateParcel()
    {
    }

    protected override void OnClick()
    {
      // **SAMPLE CODE NOTE**
      // the following code show the mechanics of creating a new parcel using the ICadastralMapEdit interface
      // there is no user interface to enter parcel record data, and the parcel line records are hard -coded
      // for the purposes of this sample code. The center of the map extent is used as the point of beginning 
      // for the parcel.
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      if (pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing and try again.");
        return;
      }

      try
      {
        ICadastralPacketManager pCadPacketMan = (ICadastralPacketManager)pCadEd;
        bool bStartedWithPacketOpen = pCadPacketMan.PacketOpen;
        if (!bStartedWithPacketOpen)
          pEd.StartOperation();

        //1. Start map edit session
        ICadastralMapEdit pCadMapEdit = (ICadastralMapEdit)pCadEd;
        pCadMapEdit.StartMapEdit(esriMapEditType.esriMEEmpty, "NewParcel", false);

        //2.	Get job packet
        ICadastralPacket pCadaPacket = pCadPacketMan.JobPacket;

        //3.	Create Plan (new)
        string sPlanName = "My New Plan";
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
        //This sample code starts from the middle of the map, and defines 4 lines of a parcel
        //The first course is a straight line, the other 3 courses are circular arcs
        IArea pArea = (IArea)ArcMap.Document.ActiveView.Extent;
        IPoint pPt1 = pArea.Centroid;

        IZAware pZAw = (IZAware)pPt1;
        pZAw.ZAware = true;
        pPt1.Z = 0; //defaulting to 0

        //Convert the point into metric units, and get a new (in-mem) point id
        IGSPoint pGSPointFrom = pMetricUnitConv.SetGSPoint(pPt1);
        pCadaPoints.AddPoint(pGSPointFrom);
        int iID1 = pGSPointFrom.Id;

        int index = 0;
        //++++++++++++ Course 1 ++++++++++++++
        int iID2 = -1;
        //From, Direction (NAz Radians), Distance (map's projection units), Radius
        IGSLine pGSLine = CreateGSLine(pMetricUnitConv, pCadaPoints, ref pPt1,
          iID1, 0, 100, 0, -1, -1, -1, true, out iID2);
        //Add the line to the new parcel
        if (pGSLine != null)
          pNewGSParcel.InsertLine(++index, pGSLine);

        //++++++++++++ Course 2 ++++++++++++++
        int iID3 = -1;
        pGSLine = CreateGSLine(pMetricUnitConv, pCadaPoints, ref pPt1,
          iID2, (Math.PI / 2), 100, -80, -1, -1, -1, true, out iID3);
        if (pGSLine != null)
          pNewGSParcel.InsertLine(++index, pGSLine);

        //++++++++++++ Course 3 ++++++++++++++
        int iID4 = -1;
        pGSLine = CreateGSLine(pMetricUnitConv, pCadaPoints, ref pPt1,
          iID3, Math.PI, 100, 80, -1, -1, -1, true, out iID4);
        if (pGSLine != null)
          pNewGSParcel.InsertLine(++index, pGSLine);

        //++++++++++++ Course 4 ++++++++++++++
        //close back to point of beginning
        int i = -1;
        pGSLine = CreateGSLine(pMetricUnitConv, pCadaPoints, ref pPt1,
          iID4, (3 * Math.PI / 2), 100, 200, -1, -1, -1, false, out i);
        pGSLine.ToPoint = iID1; //closing the traverse back to the POB
        if (pGSLine != null)
          pNewGSParcel.InsertLine(++index, pGSLine);

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
            pEd.AbortOperation();
          return;
        }
        if (!bStartedWithPacketOpen)
          pEd.StopOperation("New Parcel");
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
    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }
  }

}
