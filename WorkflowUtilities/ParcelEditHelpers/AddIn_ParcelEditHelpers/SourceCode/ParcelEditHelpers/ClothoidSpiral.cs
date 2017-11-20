using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace ParcelEditHelper
{
  public class ClothoidSpiral : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    ParcelEditHelperExtension m_ParcelEditHelperExtension = null;
    public ClothoidSpiral()
    {
      m_ParcelEditHelperExtension = ParcelEditHelperExtension.GetParcelEditHelperExtension();
    }

    protected override void OnClick()
    {
      IEditor pEd = (IEditor)ArcMap.Editor;
      IEditor2 pEd2 = (IEditor2)ArcMap.Editor;

      IEditProperties pEdProps1 = pEd as IEditProperties;
      IEditProperties2 pEdProps2 = pEd as IEditProperties2;

      IEditSketch2 pSketch2 = ArcMap.Editor as IEditSketch2;
      ISegmentCollection pSegColl = pSketch2.Geometry as ISegmentCollection;

      double dLineDirection = 0;

      ISketchTool sketchTool = ArcMap.Application.CurrentTool.Command as ISketchTool;
      if (sketchTool.Constraint == esriSketchConstraint.esriConstraintAngle)
        dLineDirection = sketchTool.AngleConstraint;
      else
      {
        ILine pRubberBandLine = new LineClass();
        pRubberBandLine.PutCoords(pSketch2.LastPoint, pEd2.Location);
        dLineDirection = pRubberBandLine.Angle;
      }

      IAngularConverter pAngConv = new AngularConverterClass();
      pAngConv.SetAngle(dLineDirection, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);     

      int iSegCnt = pSegColl.SegmentCount;
      dlgSpiralParameters SpiralEntryDialog = new dlgSpiralParameters();

      string sBearing = pAngConv.GetString(pEdProps2.DirectionType, pEdProps2.DirectionUnits, pEdProps2.AngularUnitPrecision);
      SpiralEntryDialog.txtDirection.Text = sBearing;
      //Display the dialog
      DialogResult pDialogResult = SpiralEntryDialog.ShowDialog();

      esriCurveDensifyMethod DensifyMethod = esriCurveDensifyMethod.esriCurveDensifyByAngle; //default
      double dDensifyParameter = 2 * Math.PI / 180; //2 degrees //default

      if (SpiralEntryDialog.optCustomDensification.Checked)
      {
        DensifyMethod = (esriCurveDensifyMethod)SpiralEntryDialog.cboDensificationType.SelectedIndex;
        if (DensifyMethod == esriCurveDensifyMethod.esriCurveDensifyByAngle)
          dDensifyParameter = Convert.ToDouble(SpiralEntryDialog.numAngleDensification.Value) * Math.PI / 180;
        else
        {
          if(!Double.TryParse(SpiralEntryDialog.txtDensifyValue.Text,out dDensifyParameter))
            dDensifyParameter = 2;
        }
      }

      if (pDialogResult != DialogResult.OK)
        return;

      if (SpiralEntryDialog.txtStartRadius.Text.ToLower().Trim() == "infinity" && SpiralEntryDialog.txtEndRadius.Text.ToLower().Trim() == "infinity")
        return;


      double dSpiralRadius1 = Double.MaxValue; //default to infinity
      double dFromCurvature = 0;
      if (SpiralEntryDialog.txtStartRadius.Text.ToLower() != "infinity")
      {
        if (Double.TryParse(SpiralEntryDialog.txtStartRadius.Text, out dSpiralRadius1))
          dFromCurvature = 1 / dSpiralRadius1;
        else
          return;
      }

      double dSpiralRadius2= Double.MaxValue; //default to infinity
      double dToCurvature = 0;
      if (SpiralEntryDialog.txtEndRadius.Text.ToLower() != "infinity")
      {
        if (Double.TryParse(SpiralEntryDialog.txtEndRadius.Text, out dSpiralRadius2))
          dToCurvature = 1 / dSpiralRadius2;
        else
          return;
      }

      bool bIsCCW = (dSpiralRadius1 > dSpiralRadius2) ? SpiralEntryDialog.optRight.Checked : SpiralEntryDialog.optLeft.Checked;

      bool bSpecialCaseCircularArc = (dSpiralRadius1 == dSpiralRadius2);

      if (!pAngConv.SetString(SpiralEntryDialog.txtDirection.Text, pEdProps2.DirectionType, pEdProps2.DirectionUnits))
        return;

      double dNorthAzimuthRadians = pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);

      IVector3D pVec = new Vector3DClass();
      pVec.PolarSet(dNorthAzimuthRadians, 0, 500);

      IPoint pTangentPoint = new PointClass();
      pTangentPoint.PutCoords(pSketch2.LastPoint.X + pVec.XComponent, pSketch2.LastPoint.Y + pVec.YComponent);

      //double dStreamingTol = pEdProps1.StreamTolerance;
      //if (dStreamingTol == 0)
      //  dStreamingTol = 0.001 * 5000; //metric

      //double dSpiralOffsetGeometryPrecision = 0.001 * 250; //metric 0.25 m 

      IPolyline6 theSpiralPolyLine = null;
      double dExitTangent = 0;

      if (SpiralEntryDialog.cboPathLengthParameter.SelectedIndex == 0)
      {
        double dSpiralArcLength;
        if (!Double.TryParse(SpiralEntryDialog.txtPathLengthParameter.Text, out dSpiralArcLength))
          return;

        if (bSpecialCaseCircularArc)
        {
          ILine pInTangentLine = new LineClass();
          pInTangentLine.PutCoords(pSketch2.LastPoint, pTangentPoint);

          ISegment pTangentSegment = (ISegment)pInTangentLine;
          IConstructCircularArc2 pCircArcConstr = new ESRI.ArcGIS.Geometry.CircularArcClass() as IConstructCircularArc2;
          pCircArcConstr.ConstructTangentRadiusArc(pTangentSegment,false, bIsCCW, dSpiralRadius1, dSpiralArcLength);
          ICircularArc pArcSegment = pCircArcConstr as ICircularArc;

          //Get chord Line from tangent curve constructor
          ILine pChordLine = new LineClass();
          pChordLine.PutCoords(pArcSegment.FromPoint, pArcSegment.ToPoint);
          double dPolarRadians = pChordLine.Angle; //to get the chord azimuth

          pCircArcConstr.ConstructBearingRadiusArc(pSketch2.LastPoint, dPolarRadians, bIsCCW, dSpiralRadius1, dSpiralArcLength);

          dExitTangent = pArcSegment.ToAngle + Math.PI/2;

          ISegmentCollection segCollection = new PolylineClass() as ISegmentCollection;
          object obj = Type.Missing;
          segCollection.AddSegment((ISegment)pArcSegment, ref obj, ref obj);
          theSpiralPolyLine = segCollection as IPolyline6;

        }
        else
          theSpiralPolyLine = ConstructSpiralbyLength(pSketch2.LastPoint, pTangentPoint, dFromCurvature, dToCurvature, bIsCCW, dSpiralArcLength,
            DensifyMethod, dDensifyParameter, out dExitTangent);
      }

      if (SpiralEntryDialog.cboPathLengthParameter.SelectedIndex == 1)
      {
        if(!pAngConv.SetString(SpiralEntryDialog.txtPathLengthParameter.Text, esriDirectionType.esriDTPolar, pEdProps2.DirectionUnits))
          return;
        double dSpiralDeltaAngle=pAngConv.GetAngle(esriDirectionType.esriDTPolar,esriDirectionUnits.esriDURadians);

        if (bSpecialCaseCircularArc)
        {
          ILine pInTangentLine = new LineClass();
          pInTangentLine.PutCoords(pSketch2.LastPoint, pTangentPoint);

          ISegment pTangentSegment = (ISegment)pInTangentLine;
          IConstructCircularArc2 pCircArcConstr = new ESRI.ArcGIS.Geometry.CircularArcClass() as IConstructCircularArc2;
          pCircArcConstr.ConstructTangentRadiusAngle(pTangentSegment, false, bIsCCW, dSpiralRadius1, dSpiralDeltaAngle);
          ICircularArc pArcSegment = pCircArcConstr as ICircularArc;

          //Get chord Line from tangent curve constructor
          ILine pChordLine = new LineClass();
          pChordLine.PutCoords(pArcSegment.FromPoint, pArcSegment.ToPoint);
          double dPolarRadians = pChordLine.Angle; //to get the chord azimuth

          pCircArcConstr.ConstructBearingRadiusAngle(pSketch2.LastPoint, dPolarRadians, bIsCCW, dSpiralRadius1, dSpiralDeltaAngle);

          dExitTangent = pArcSegment.ToAngle + Math.PI / 2;

          ISegmentCollection segCollection = new PolylineClass() as ISegmentCollection;
          object obj = Type.Missing;
          segCollection.AddSegment((ISegment)pArcSegment, ref obj, ref obj);
          theSpiralPolyLine = segCollection as IPolyline6;
        }
        else
          theSpiralPolyLine = ConstructSpiralbyDeltaAngle(pSketch2.LastPoint, pTangentPoint, dFromCurvature, dToCurvature, bIsCCW, dSpiralDeltaAngle,
            DensifyMethod, dDensifyParameter, out dExitTangent);
      }

      if (theSpiralPolyLine == null)
      {
        MessageBox.Show("A spiral could not be created with the entered parameters.");
        return;
      }

      ISegmentCollection pSpiralSegCollection = theSpiralPolyLine as ISegmentCollection;
      //Start a sketch operation and insert the new envelope into the sketch
      ISketchOperation2 sketchOp = new SketchOperationClass();
      sketchOp.Start(ArcMap.Editor);
      sketchOp.MenuString = "Add Spiral";
      pSegColl.AddSegmentCollection(pSpiralSegCollection);
      IGeometry geom = pSegColl as IGeometry;
      pSketch2.Geometry = geom;
      
      //set the angle constraint to the exit tangent of the spiral
      sketchTool.Constraint = esriSketchConstraint.esriConstraintAngle;
      sketchTool.AngleConstraint = dExitTangent;

      sketchOp.Finish(ArcMap.Document.ActiveView.Extent, esriSketchOperationType.esriSketchOperationGeneral, null);

    }

    private IPolyline6 ConstructSpiralbyLength(IPoint theFromPoint, IPoint theTangentpoint, double theFromCurvature, 
      double theToCurvature, bool isCCW, double theSpiralLength, esriCurveDensifyMethod DensifyMethod, double theCurveDensity, out double ExitTangent)
    {
      //the parameter name "curvature" is the inverse of the radius. Infinity radius is 0 radius.
      ExitTangent = 0;
      IPolyline6 thePolyLine = new PolylineClass() as IPolyline6;

      try
      {
        IGeometryEnvironment4 theGeometryEnvironment = new GeometryEnvironmentClass();
        IConstructClothoid TheSpiralConstruction = theGeometryEnvironment as IConstructClothoid;
        thePolyLine = TheSpiralConstruction.ConstructClothoidByLength(theFromPoint, theTangentpoint, isCCW, 
          theFromCurvature, theToCurvature, theSpiralLength, DensifyMethod, theCurveDensity) as IPolyline6;

        //now use the Query on the same Spiral, to get the precise exit tangent
        double dCurvature;
        double dSplitLength;
        double dSplitAngle;
        ILine pExitTangentLine;
        if (thePolyLine != null)
        {
          TheSpiralConstruction.ConstructSplitClothoidByLength(thePolyLine.ToPoint, theFromPoint,
          theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
          out dCurvature, out dSplitLength, out dSplitAngle, out pExitTangentLine);
          ExitTangent = pExitTangentLine.Angle; //returns polar azimuth in radians
        }

        #region tests

        //TEST +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //IAngularConverter pAngConv = new AngularConverterClass();
        //double dCurvatureTEST = 0;
        //double dSplitLengthTEST = 0;
        //double dSplitAngleTEST = 0;
        //double ExitTangentTEST = 0;
        //ILine pExitTangentLineTEST = null;

        ////IPoint theFromPoint = new PointClass();
        ////theFromPoint.PutCoords(2356762.676, 1919302.7);
        ////theFromPoint.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        ////IPoint theTangentpoint = new PointClass();
        ////theTangentpoint.PutCoords(2356409.122, 1918949.146);
        ////theTangentpoint.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //IPoint TESTPOINT1 = new PointClass();
        //TESTPOINT1.PutCoords(2356868.410, 1918327.316);
        //TESTPOINT1.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //IPoint TESTPOINT2 = new PointClass();
        //TESTPOINT2.PutCoords(2356876.899, 1918316.005);
        //TESTPOINT2.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //IPoint TESTPOINT3 = new PointClass();
        //TESTPOINT3.PutCoords(2356884.312, 1918317.906);
        //TESTPOINT3.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //IPoint TESTPOINT4 = new PointClass();
        //TESTPOINT4.PutCoords(2356872.308, 1918333.903);
        //TESTPOINT4.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;


        //IPoint TESTPOINTA_Centroid = new PointClass();
        //TESTPOINTA_Centroid.PutCoords(2356633.395, 1918788.139);
        //TESTPOINTA_Centroid.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //IPoint TESTPOINTB_Centroid_OnCurve = new PointClass();
        //TESTPOINTB_Centroid_OnCurve.PutCoords(2356633.396, 1918788.358);
        //TESTPOINTB_Centroid_OnCurve.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //IPoint TESTPOINTC_OutsideOffsetTransferredToCurve = new PointClass();
        //TESTPOINTC_OutsideOffsetTransferredToCurve.PutCoords(2356511.754, 1919017.119);
        //TESTPOINTC_OutsideOffsetTransferredToCurve.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //IPoint TESTPOINTD_OutsideOffsetPoint = new PointClass();
        //TESTPOINTD_OutsideOffsetPoint.PutCoords(2355963.185,1919377.363);
        //TESTPOINTD_OutsideOffsetPoint.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;


        //IPoint ComputedEndPOINT = new PointClass();
        //ComputedEndPOINT.PutCoords(2356878.31, 1918325.904);
        //ComputedEndPOINT.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;

        //string sReport = "";

        //if (thePolyLine != null)
        //{

        //  TheSpiralConstruction.ConstructSplitClothoidByLength(thePolyLine.ToPoint, theFromPoint,
        //  theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //  out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //  ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //  pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //  string sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //  pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //  string sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //  sReport += ("To Point " +Environment.NewLine + "------------------" + Environment.NewLine +
        //    "Curvature at To point          : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //    "Radius at To point          : " + (1/dCurvatureTEST).ToString("0.00000") + Environment.NewLine +
        //    "Length along curve at To point : " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //    "Central angle  at To  point     : " + sSplitCentralAngle + Environment.NewLine +
        //    "Tangent bearing at To point    : " + sTangentBearing + Environment.NewLine
        //    );



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //        TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINT1, theFromPoint,
        //theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //        ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //        pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //        sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //        pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //        sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //        sReport += ("TESTPOINT1" +Environment.NewLine + "---" + Environment.NewLine +
        //          "Curvature at query point          : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //          "Length along curve at query point : " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //          "Central angle  at query point     : " + sSplitCentralAngle + Environment.NewLine +
        //          "Tangent bearing at query point    : " + sTangentBearing + Environment.NewLine
        //          );


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //        TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINT2, theFromPoint,
        //theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //        ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //        pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //        sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //        pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //        sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //        sReport += ("TESTPOINT2" +Environment.NewLine + "---" + Environment.NewLine +
        //          "Curvature at query point          : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //          "Length along curve at query point : " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //          "Central angle  at query point     : " + sSplitCentralAngle + Environment.NewLine +
        //          "Tangent bearing at query point    : " + sTangentBearing + Environment.NewLine
        //          );


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //        TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINT3, theFromPoint,
        //theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //        ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //        pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //        sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //        pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //        sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //        sReport += ("TESTPOINT3" +Environment.NewLine + "---" + Environment.NewLine +
        //          "Curvature at query point          : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //          "Length along curve at query point : " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //          "Central angle  at query point     : " + sSplitCentralAngle + Environment.NewLine +
        //          "Tangent bearing at query point    : " + sTangentBearing + Environment.NewLine
        //          );


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINT4, theFromPoint,
        //  theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //  out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //sReport += ("TESTPOINT4" +Environment.NewLine + "---" + Environment.NewLine +
        //  "Curvature at query point          : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //  "Length along curve at query point : " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //  "Central angle  at query point     : " + sSplitCentralAngle + Environment.NewLine +
        //  "Tangent bearing at query point    : " + sTangentBearing + Environment.NewLine
        //  );


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINTA_Centroid, theFromPoint,
        //  theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //  out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //sReport += ("A " +Environment.NewLine + "------------------" + Environment.NewLine +
        //  "Curvature; query point A         : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //  "Radius; query point A         : " + (1/dCurvatureTEST).ToString("0.00000") + Environment.NewLine +
        //  "Length along curve; query point A: " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //  "Central angle; query point A    : " + sSplitCentralAngle + Environment.NewLine +
        //  "Tangent bearing; query point A   : " + sTangentBearing + Environment.NewLine
        //  );


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINTB_Centroid_OnCurve, theFromPoint,
        //  theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //  out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //sReport += ("B " +Environment.NewLine + "---" + Environment.NewLine +
        //  "Curvature; query point B         : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //  "Radius; query point B         : " + (1/dCurvatureTEST).ToString("0.00000") + Environment.NewLine +
        //  "Length along curve; query point B : " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //  "Central angle; query point B    : " + sSplitCentralAngle + Environment.NewLine +
        //  "Tangent bearing; query point B   : " + sTangentBearing + Environment.NewLine
        //  );


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINTC_OutsideOffsetTransferredToCurve, theFromPoint,
        //  theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //  out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //sReport += ("C " +Environment.NewLine + "---" + Environment.NewLine +
        //  "Curvature; query point C         : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //  "Radius; query point C         : " + (1/dCurvatureTEST).ToString("0.00000") + Environment.NewLine +
        //  "Length along curve; query point C: " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //  "Central angle ; query point C    : " + sSplitCentralAngle + Environment.NewLine +
        //  "Tangent bearing; query point C   : " + sTangentBearing + Environment.NewLine
        //  );


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  TheSpiralConstruction.ConstructSplitClothoidByLength(TESTPOINTD_OutsideOffsetPoint, theFromPoint,
        //    theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralLength,
        //    out dCurvatureTEST, out dSplitLengthTEST, out dSplitAngleTEST, out pExitTangentLineTEST);
        //  ExitTangentTEST = pExitTangentLineTEST.Angle; //returns polar azimuth in radians

        //  pAngConv.SetAngle(ExitTangentTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        //  sTangentBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //  pAngConv.SetAngle(dSplitAngleTEST, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians); //returns polar azimuth in radians
        //  sSplitCentralAngle = pAngConv.GetString(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);
        //  sReport += ("D " +Environment.NewLine + "---------------" + Environment.NewLine +
        //    "Curvature; query point D         : " + dCurvatureTEST.ToString("0.00000") + Environment.NewLine +
        //    "Radius; query point D         : " + (1/dCurvatureTEST).ToString("0.00000") + Environment.NewLine +
        //    "Length along curve; query point D: " + dSplitLengthTEST.ToString("0.00") + Environment.NewLine +
        //    "Central angle; query point D    : " + sSplitCentralAngle + Environment.NewLine +
        //    "Tangent bearing; query point D   : " + sTangentBearing + Environment.NewLine
        //    );

        //  MessageBox.Show(sReport, "Query Point Results");

        //}

        //END TEST ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        #endregion

        return thePolyLine;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
    }


    private IPolyline6 ConstructSpiralbyDeltaAngle(IPoint theFromPoint, IPoint theTangentpoint, double theFromCurvature,
  double theToCurvature, bool isCCW, double theSpiralDeltaAngle, esriCurveDensifyMethod DensifyMethod, double theCurveDensity, out double ExitTangent)
    {
      //the parameter name "curvature" is actually the inverse of the radius. Infinity radius is 0 curvature.
      ExitTangent = 0;
      IPolyline6 thePolyLine = new PolylineClass() as IPolyline6;

      try
      {
        IGeometryEnvironment4 theGeometryEnvironment = new GeometryEnvironmentClass();
        IConstructClothoid TheSpiralConstruction = theGeometryEnvironment as IConstructClothoid;
        thePolyLine = TheSpiralConstruction.ConstructClothoidByAngle(theFromPoint, theTangentpoint, isCCW, 
          theFromCurvature, theToCurvature, theSpiralDeltaAngle, DensifyMethod, theCurveDensity) as IPolyline6;

        //now use the Query on the same Spiral, to get the precise exit tangent
        double dCurvature;
        double dSplitLength;
        double dSplitAngle;
        ILine pExitTangentLine;

        if (thePolyLine != null)
        {
          TheSpiralConstruction.ConstructSplitClothoidByAngle(thePolyLine.ToPoint, theFromPoint,
            theTangentpoint, isCCW, theFromCurvature, theToCurvature, theSpiralDeltaAngle,
            out dCurvature, out dSplitLength, out dSplitAngle, out pExitTangentLine);

          ExitTangent = pExitTangentLine.Angle; //returns polar azimuth in radians
        }

        return thePolyLine; 
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
    }

    protected override void OnUpdate()
    {
    }
  }
}
