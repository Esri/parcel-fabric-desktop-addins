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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

//Added non-Esri references
using System.Windows.Forms;
using System.Runtime.InteropServices;

//Added Esri references
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;

namespace ParcelEditHelper
{
  public class ConstructionTraverse : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public ConstructionTraverse()
    {
    }

    protected override void OnClick()
    {
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;

      ICadastralPacketManager pCadPacketMan = (ICadastralPacketManager)pCadEd;
      //bool bStartedWithPacketOpen = pCadPacketMan.PacketOpen;

      if (pParcEditorMan == null)
        return;

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      if (pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing and try again.");
        return;
      }

      IParcelConstruction pConstr = pParcEditorMan.ParcelConstruction;
      IParcelConstruction4 pConstr4 = pConstr as IParcelConstruction4;
      ICadastralPoints pCadastralPts = pConstr4 as ICadastralPoints;
      ICadastralEditorSettings2 pCadastralEditorSettings2 = pCadEd as ICadastralEditorSettings2;

      ICadastralFixedPoints pFixedPoints = pCadastralPts as ICadastralFixedPoints;
      IPointCalculation pPointCalc = new PointCalculationClass();

      if (pConstr == null)
        return;

      IGSLine pParcelLine = null;
      IMetricUnitConverter pMetricUnitConv = (IMetricUnitConverter)pCadEd;
      IGSPoint pStartPoint = null;
      //IGSPoint pToPoint = null;
      List<int> lstPointIds = new List<int>();

      List<IVector3D> Traverse = new List<IVector3D>();

      //get rotation here
      IParcelConstructionData pConstrData = pConstr4.ConstructionData;
      IConstructionParentParcels pConstructionParentParcels = pConstrData as IConstructionParentParcels;

      ICadastralUndoRedo pCadUndoRedo = pConstr as ICadastralUndoRedo;
      try
      {
        int iParcelID = -1;
        pConstructionParentParcels.GetParentParcel(0, ref iParcelID);

        ICadastralParcel pCadaParcel = pCadPacketMan.JobPacket as ICadastralParcel;
        IGSParcel pGSParcel = pCadaParcel.GetParcel(iParcelID);
        //if in measurement view then rotation is 0
        double TheRotation = 0;
        if (!pCadastralEditorSettings2.MeasurementView)
          TheRotation = pGSParcel.Rotation;//radians

        pPointCalc.Rotation = TheRotation;
        IGSPoint pClosingPoint = null;

        #region simple method as fall-back
        bool bUseSimpleStackSelection = false;
        if (bUseSimpleStackSelection)
        {
          //bool bLineSelectionSequence = false;
          //IGSLine pLastSelectedGSLineInGrid = null;
          //for (int i = 0; i < pConstr.LineCount; i++)
          //{
          //  if (pConstr.GetLineSelection(i))
          //  {
          //    if (pConstr.GetLine(i, ref pParcelLine))
          //    {
          //      if (!bLineSelectionSequence) //first line
          //      {
          //        pStartPoint = pCadastralPts.GetPoint(pParcelLine.FromPoint);
          //        pToPoint = pCadastralPts.GetPoint(pParcelLine.ToPoint);
          //      }

          //      pPointCalc.AddLine(pParcelLine);

          //      pLastSelectedGSLineInGrid = pParcelLine;
          //      pToPoint = pCadastralPts.GetPoint(pParcelLine.ToPoint);
          //      lstPointIds.Add(pToPoint.Id);

          //    }
          //    bLineSelectionSequence = true;

          //    double dBear = pParcelLine.Bearing; //Azimuth of IVector3D is north azimuth radians zero degrees north
          //    double dDist = pParcelLine.Distance;
          //    IVector3D vec = new Vector3DClass();
          //    vec.PolarSet(dBear, 0, dDist); ////Azimuth of IVector3D is north azimuth radians zero degrees north
          //    Traverse.Add(vec);
          //  }
          //  else
          //  {
          //    if (bLineSelectionSequence && pConstr.GetLine(i, ref pParcelLine) && HasLineSelectionAfter(pConstr, i))
          //    //if there was a prior selection and this line is a complete line, and there is no later selection
          //    {
          //      MessageBox.Show("Please select a continuous set of lines for closure.");
          //      return;
          //    }
          //  }
          //}
          //pClosingPoint = pCadastralPts.GetPoint(pLastSelectedGSLineInGrid.ToPoint);
        }
        else
        #endregion

        {//build a forward star for the selected lines
          IEnumCELines pCELines = new EnumCELinesClass();
          IEnumGSLines pEnumGSLines = (IEnumGSLines)pCELines;
          ILongArray pLongArray = new LongArrayClass();
          int iFirstToNode = -1;
          for (int i = 0; i < pConstr.LineCount; i++)
          {
            if (pConstr.GetLineSelection(i))
            {
              if (pConstr.GetLine(i, ref pParcelLine))
              {
                if (iFirstToNode < 0)
                  iFirstToNode = pParcelLine.ToPoint;
                pLongArray.Add(i);
                pCELines.Add(pParcelLine);
              }
            }
          }
          IParcelLineFunctions3 ParcelLineFx = new ParcelFunctionsClass();
          IGSForwardStar pFwdStar = ParcelLineFx.CreateForwardStar(pEnumGSLines);
          //forward star object is now created for all the selected lines, 
          //need to first re-sequence the lines, and test for branching and discontinuity

          int iBranches = 0; int iTracedLines = 0;
          int iLoops = 0; int iTerminals = 0;
          List<int> LineIDList = new List<int>();
          List<int> FromList = new List<int>();
          List<int> ToList = new List<int>();
          List<string> FromToLine = new List<string>();

          bool bTraceSucceeded = TraceLines(ref pFwdStar, iFirstToNode, ref iBranches, ref iTracedLines,
            ref iLoops, ref iTerminals, ref FromToLine, ref FromList, ref ToList, 0);

          if (iBranches > 0)
          {
            MessageBox.Show("Please select a continuous set of lines for closure." + Environment.NewLine +
              "Line selection should not have branches.", "Traverse");
            return;
          }

          if (iTracedLines < pLongArray.Count)
          {
            MessageBox.Show("Please select a continuous set of lines for closure." + Environment.NewLine +
              "Selected Lines should be connected in a single sequence without branches.", "Traverse");
            return;
          }

          LineIDList.Clear();
          FromList.Clear();
          ToList.Clear();
          pLongArray.RemoveAll();

          pCadUndoRedo.StartUndoRedoSession("Adjust Traverse");

          if (iLoops == 0)
          {
            //re-sequence using TraceLines function based on either end point, because the order of
            //selected construction lines in grid don't control start or end point
            FromToLine.Clear();
            int iTerminus = -1;
            iTracedLines = 0;
            iBranches = 0;
            iLoops = 0; iTerminals = 0;
            FindTerminusForSequence(ref pFwdStar, iFirstToNode, ref iTerminus, 0);
            if (iTerminus == -1)
            {
              pCadUndoRedo.WriteUndoRedoSession(false);
              return;
            }
            TraceLines(ref pFwdStar, iTerminus, ref iBranches, ref iTracedLines, ref iLoops, ref iTerminals,
              ref FromToLine, ref FromList, ref ToList, 0);
          }

          List<IVector3D> SequencedTraverse = new List<IVector3D>();
          IGSLine pGSLineInPath = null;
          foreach (string s in FromToLine)
          {
            string[] sFromTo = s.Split(',');
            int iFrom = Convert.ToInt32(sFromTo[0]);
            int iTo = Convert.ToInt32(sFromTo[1]);

            bool bReversed = pFwdStar.GetLine(iFrom, iTo, ref pGSLineInPath);
            if (bReversed)
            {
              IGSLine pGSLine180 = new GSLineClass();
              pGSLine180.FromPoint = pGSLineInPath.ToPoint;
              pGSLine180.ToPoint = pGSLineInPath.FromPoint;
              pGSLine180.Bearing = pGSLineInPath.Bearing + Math.PI;
              pGSLine180.Distance = pGSLineInPath.Distance;

              IVector3D vec180 = new Vector3DClass();
              vec180.PolarSet(pGSLine180.Bearing, 0, pGSLine180.Distance); //Azimuth of IVector3D is north azimuth radians zero degrees north
              Traverse.Add(vec180);
              lstPointIds.Add(pGSLine180.ToPoint);
              pPointCalc.AddLine(pGSLine180);
            }
            else
            {
              double dBear = pGSLineInPath.Bearing;
              double dDist = pGSLineInPath.Distance;
              IVector3D vec = new Vector3DClass();
              vec.PolarSet(dBear, 0, dDist); //Azimuth of IVector3D is north azimuth radians zero degrees north
              Traverse.Add(vec);
              lstPointIds.Add(pGSLineInPath.ToPoint);
              pPointCalc.AddLine(pGSLineInPath);
            }

            if (pStartPoint == null)
            {
              if (bReversed)
                pStartPoint = pCadastralPts.GetPoint(pGSLineInPath.ToPoint);
              else
                pStartPoint = pCadastralPts.GetPoint(pGSLineInPath.FromPoint);
            }

            if (bReversed)
              pClosingPoint = pCadastralPts.GetPoint(pGSLineInPath.FromPoint);
            else
              pClosingPoint = pCadastralPts.GetPoint(pGSLineInPath.ToPoint);
          }
        }

        IPoint pStart = new PointClass();
        pStart.X = pStartPoint.X;
        pStart.Y = pStartPoint.Y;

        string sAdjustMethod = "Compass";
        esriParcelAdjustmentType eAdjMethod = esriParcelAdjustmentType.esriParcelAdjustmentCompass;

        if (pCadastralEditorSettings2.ParcelAdjustment == esriParcelAdjustmentType.esriParcelAdjustmentNone ||
          pCadastralEditorSettings2.ParcelAdjustment == esriParcelAdjustmentType.esriParcelAdjustmentCompass)
          eAdjMethod = esriParcelAdjustmentType.esriParcelAdjustmentCompass;
        else if (pCadastralEditorSettings2.ParcelAdjustment == esriParcelAdjustmentType.esriParcelAdjustmentCrandall)
        {
          sAdjustMethod = "Crandall";
          eAdjMethod = pCadastralEditorSettings2.ParcelAdjustment;
        }
        else if (pCadastralEditorSettings2.ParcelAdjustment == esriParcelAdjustmentType.esriParcelAdjustmentTransit)
        {
          sAdjustMethod = "Transit";
          eAdjMethod = pCadastralEditorSettings2.ParcelAdjustment;
        }

        pPointCalc.CalculatePoints(eAdjMethod, pStartPoint.Id, pStartPoint, pClosingPoint.Id, pClosingPoint, true);
        ITraverseClosure pClose = pPointCalc.Closure;
        List<string> lstCoursesFromTo = new List<string>();
        List<IVector3D> AdjustedTraverse = new List<IVector3D>();
        double dAdjustedPointX = 0; double dAdjustedPointY = 0;
        double dPreviousPointX = 0; double dPreviousPointY = 0;

        for (int i = 0; i < pClose.CourseCount; i++)
        {
          IGSPoint pPt = pCadastralPts.GetPoint(lstPointIds[i]);
          dAdjustedPointY = pPointCalc.GetCalculatedPoint(lstPointIds[i], ref dAdjustedPointX);
          string sFromTo = "";
          IVector3D pAdjustedLine = new Vector3DClass();
          if (i == 0)
          {
            sFromTo = pStartPoint.Id.ToString() + "-" + lstPointIds[i].ToString();
            pAdjustedLine.SetComponents(dAdjustedPointX - pStartPoint.X, dAdjustedPointY - pStartPoint.Y, 0);
          }
          else
          {
            sFromTo = lstPointIds[i - 1].ToString() + "-" + lstPointIds[i].ToString();
            pAdjustedLine.SetComponents(dAdjustedPointX - dPreviousPointX, dAdjustedPointY - dPreviousPointY, 0);
          }
          lstCoursesFromTo.Add(sFromTo);

          IVector3D Z_Axis = new Vector3DClass();
          Z_Axis.SetComponents(0, 0, 100);

          pAdjustedLine.Rotate(TheRotation, Z_Axis);
          AdjustedTraverse.Add(pAdjustedLine);

          dPreviousPointX = dAdjustedPointX;
          dPreviousPointY = dAdjustedPointY;

          pPt.X = dAdjustedPointX;
          pPt.Y = dAdjustedPointY;

          if (!pCadastralEditorSettings2.MeasurementView)
            pFixedPoints.SetFixedPoint(lstPointIds[i], true);
        }

        double dMisclosureDistance = pClose.MisclosureDistance; double dMisclosureBearing = pClose.MisclosureDirection;
        IVector MiscloseVector = new Vector3DClass();

        IEditProperties2 pEdProps = pEd as IEditProperties2;

        IAngularConverter pAngConv = new AngularConverterClass();
        pAngConv.SetAngle(dMisclosureBearing, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);
        int iPrec = 7;
        if (pConstr.Parcel.Plan.AngleUnits == esriDirectionUnits.esriDUDegreesMinutesSeconds)
          iPrec = 0;

        string sMiscloseBearing = pAngConv.GetString(pEdProps.DirectionType, pEdProps.DirectionUnits, iPrec);

        Utilities UTIL = new Utilities();
        string sRatio = "High Accuracy";

        if (pClose.RelativeErrorRatio < 10000)
          sRatio = "1:" + pClose.RelativeErrorRatio.ToString("0");

        if (dMisclosureDistance >= 0.001)
          sMiscloseBearing = UTIL.FormatDirectionDashesToDegMinSecSymbols(sMiscloseBearing);
        else
          sMiscloseBearing = "----";

        ICadastralUnitConversion pCadUnitConverter = new CadastralUnitConversionClass();
        double dMetersPerUnit = pCadUnitConverter.ConvertDouble(1, pConstr.Parcel.Plan.DistanceUnits, esriCadastralDistanceUnits.esriCDUMeter);

        string sReport = "Closure:" + Environment.NewLine +
          "        error:  " + sRatio + Environment.NewLine +
          "        distance:  " + (dMisclosureDistance / dMetersPerUnit).ToString("0.000") + Environment.NewLine +
          "        bearing:  " + sMiscloseBearing + Environment.NewLine +
          "        xdist:  " + (pClose.MisclosureX / dMetersPerUnit).ToString("0.000") + Environment.NewLine +
          "        ydist:  " + (pClose.MisclosureY / dMetersPerUnit).ToString("0.000") + Environment.NewLine +
          "        courses: " + (pClose.CourseCount) + Environment.NewLine +
          Environment.NewLine + "Adjustment:" + Environment.NewLine +
          "        method: " + sAdjustMethod;

        dlgTraverseResults dlgTraverseResults = new dlgTraverseResults();
        AddTraverseInfoToGrid(pConstr.Parcel.Plan, dlgTraverseResults.dataGridView1, Traverse, AdjustedTraverse, lstCoursesFromTo);
        dlgTraverseResults.txtMiscloseReport.Text = sReport;
        DialogResult dRes = dlgTraverseResults.ShowDialog();
        if (dRes == DialogResult.Cancel)
        {
          //since we cancelled, set the points back
          foreach (int i in lstPointIds)
            pFixedPoints.SetFixedPoint(i, false);
          pCadUndoRedo.WriteUndoRedoSession(false);
        }
        else
          pCadUndoRedo.WriteUndoRedoSession(true);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message,"Traverse");
        pCadUndoRedo.WriteUndoRedoSession(false);
      }
    }

    private bool TraceLines(ref IGSForwardStar FwdStar, int StartNodeId, ref int BranchCount, 
      ref int TracedLinesCount, ref int LoopCount, ref int TerminusCount, ref List<string> FromToLine,
      ref List<int> FromList, ref List<int> ToList, int iInfinityChecker)
    {
      iInfinityChecker++; 
      if (iInfinityChecker > 5000)
        return false;
      //(This is a self-calling function.) In this context 5000 downstream lines is like infinity, 
      //so exit gracefully, and avoid probable endless loop. 
      //Possible cause of endless loop? Corrupted data; example, a line with the same from and to point id
      IVector3D vect = new Vector3DClass();
      try
      {
        ILongArray iLngArr = FwdStar.get_ToNodes(StartNodeId);
        //get_ToNodes returns an array of radiated points, not "TO" points in the fabric data model sense
        int iCnt2 = 0;
        iCnt2 = iLngArr.Count;

        if (iCnt2 == 1)
          TerminusCount++;

        IGSLine pGSLine = null;
        for (int i = 0; i < iCnt2; i++)
        {
          int i2 = iLngArr.get_Element(i);

          string sFromTo = StartNodeId.ToString() + "," + i2.ToString();
          string sToFrom = i2.ToString() + "," + StartNodeId.ToString();

          if (FromToLine.Contains(sFromTo) || FromToLine.Contains(sToFrom))
          {
            if (FromToLine.Contains(sFromTo))
              LoopCount++;
            continue;
          }

          if (iCnt2 > 2)
            BranchCount++;

          TracedLinesCount++;

          FromToLine.Add(StartNodeId.ToString() + "," + i2.ToString());
          
          bool bIsReversed = FwdStar.GetLine(StartNodeId, i2, ref pGSLine);
          if (bIsReversed)
          {
            FromList.Add(-StartNodeId);
            ToList.Add(-i2);
            vect.PolarSet(pGSLine.Bearing + Math.PI, 0, pGSLine.Distance); //Azimuth of IVector3D is north azimuth radians zero degrees north
          }
          else
          {
            FromList.Add(StartNodeId);
            ToList.Add(i2);
            vect.PolarSet(pGSLine.Bearing, 0, pGSLine.Distance); //Azimuth of IVector3D is north azimuth radians zero degrees north
          }

          if (!TraceLines(ref FwdStar, i2, ref BranchCount, ref TracedLinesCount,ref LoopCount, ref TerminusCount,
             ref FromToLine,ref FromList, ref ToList, iInfinityChecker))
            return false;
        }

        return true;
      }
      catch
      {
        return false;
      }
    }

    private void FindTerminusForSequence(ref IGSForwardStar FwdStar, int StartNodeId, 
      ref int TerminusPoint, int iInfinityChecker)
    {
      if (TerminusPoint > -1)
        return;

      //iInfinityChecker++;
      if (iInfinityChecker++ > 5000)
        return;

      ILongArray iLngArr = FwdStar.get_ToNodes(StartNodeId);
      int iCnt2 = 0;
      iCnt2 = iLngArr.Count;
      IGSLine pGSLine = null;
      for (int i = 0; i < iCnt2; i++)
      {
        int i2 = iLngArr.get_Element(i);
        bool bIsReversed = FwdStar.GetLine(StartNodeId, i2, ref pGSLine);

        if (iCnt2 == 1) //this is a terminus/end-line
          TerminusPoint = StartNodeId;

        FindTerminusForSequence(ref FwdStar, i2, ref TerminusPoint, iInfinityChecker);
      }
    }
    
    protected void AddTraverseInfoToGrid(IGSPlan Plan, DataGridView TraverseGrid, List<IVector3D> Traverse, 
      List<IVector3D> AdjustedTraverse, List<string> FromToList)
    {
      ICadastralUnitConversion pCadUnitConverter = new CadastralUnitConversionClass();
      double dMetersPerUnit = pCadUnitConverter.ConvertDouble(1, Plan.DistanceUnits, esriCadastralDistanceUnits.esriCDUMeter);
      IAngularConverter pAngConv = new AngularConverterClass();
      Utilities Utils = new Utilities();

      List<string> lstAdjustedCourses = new List<string>();
      List<string> lstResiduals = new List<string>();

      foreach (IVector3D vect in AdjustedTraverse)
      {
        pAngConv.SetAngle(vect.Azimuth, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);

        int iPrec = 7;
        if (Plan.AngleUnits == esriDirectionUnits.esriDUDegreesMinutesSeconds)
          iPrec = 0;
        string sBearing = pAngConv.GetString(Plan.DirectionFormat, Plan.AngleUnits, iPrec);
        string sAdjusted = Utils.FormatDirectionDashesToDegMinSecSymbols(sBearing) + ", " + (vect.Magnitude / dMetersPerUnit).ToString("0.000");

        lstAdjustedCourses.Add(sAdjusted);

      }

      int i = 0;

      foreach (IVector3D vect in Traverse)
      {
        pAngConv.SetAngle(vect.Azimuth, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);

        int iPrec = 7;
        if(Plan.AngleUnits == esriDirectionUnits.esriDUDegreesMinutesSeconds)
          iPrec=0;
        string sBearing = pAngConv.GetString(Plan.DirectionFormat, Plan.AngleUnits, iPrec);
        string sDescription = Utils.FormatDirectionDashesToDegMinSecSymbols(sBearing) + ", " + (vect.Magnitude/dMetersPerUnit).ToString("0.000");

        string sDistResidual= ((vect.Magnitude-AdjustedTraverse[i].Magnitude)/dMetersPerUnit).ToString("0.000");

        //get the angle difference between the vectors
        IVector3D vec1 = new Vector3DClass();
        vec1.PolarSet(vect.Azimuth, 0, 1);

        IVector3D vec2 = new Vector3DClass();
        vec2.PolarSet(AdjustedTraverse[i].Azimuth, 0, 1);

        double dAngleDifference = Math.Acos(vec1.DotProduct(vec2));
        pAngConv.SetAngle(dAngleDifference, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        string sAngResidual = pAngConv.GetString(esriDirectionType.esriDTPolar, Plan.AngleUnits, iPrec);
        sAngResidual = Utils.FormatDirectionDashesToDegMinSecSymbols(sAngResidual);

        TraverseGrid.Rows.Add(FromToList[i], sDescription, lstAdjustedCourses[i], sAngResidual + ", " + sDistResidual);
        i++;
      }
    }

    protected override void OnUpdate()
    {
    }

    private void NetworkAnalysis(List<int> FromList, List<int> ToList, out int Loops)
    {
      int iTerminalPoints = 0;
      int iBranches = 0;
      int iCorners = 0;

      Loops = 0;

      List<int> l1 = new List<int>();
      foreach (int i in FromList)
        l1.Add(Math.Abs(i));

      List<int> l2 = new List<int>();
      foreach (int i in ToList)
        l2.Add(Math.Abs(i));

      l1.AddRange(l2);
      l1.Sort();

      var groupNodes = l1.GroupBy(item => item);
      foreach (IGrouping<int, int> nodeGroup in groupNodes)
      {
        if (nodeGroup.Count() == 1)
          iTerminalPoints++;
        else if((nodeGroup.Count() == 2))
          iCorners++;
        else if ((nodeGroup.Count() > 2))
          iBranches++;
      }
    }
    protected bool HasLineSelectionAfter(IParcelConstruction ConstructionLines, int AfterLineIndex)
    {
      IGSLine pParcelLine = null;
      for (int i = AfterLineIndex + 1; i < ConstructionLines.LineCount; i++)
      {
        if (ConstructionLines.GetLineSelection(i))
        {
          if (ConstructionLines.GetLine(i, ref pParcelLine))
            return true;
        }
      }
      return false;
    }
    private IPoint[] BowditchAdjust(List<IVector3D> TraverseCourses, IPoint StartPoint, IPoint EndPoint, out IVector3D MiscloseVector, out double Ratio)
    {
      MiscloseVector = null;
      double dSUM = 0;
      Ratio = 10000;
      MiscloseVector = GetClosingVector(TraverseCourses, StartPoint, EndPoint, out dSUM) as IVector3D;
      //Azimuth of IVector3D is north azimuth radians zero degrees north

      if (MiscloseVector.Magnitude > 0.001)
        Ratio = dSUM / MiscloseVector.Magnitude;

      if (Ratio > 10000)
        Ratio = 10000;

      double dRunningSum = 0;
      IPoint[] TraversePoints = new IPoint[TraverseCourses.Count]; //from control
      for (int i = 0; i < TraverseCourses.Count; i++)
      {
        IPoint toPoint = new PointClass();
        IVector3D vec = TraverseCourses[i];
        dRunningSum += vec.Magnitude;

        double dScale = (dRunningSum / dSUM);
        double dXCorrection = MiscloseVector.XComponent * dScale;
        double dYCorrection = MiscloseVector.YComponent * dScale;

        toPoint.PutCoords(StartPoint.X + vec.XComponent, StartPoint.Y + vec.YComponent);
        StartPoint.PutCoords(toPoint.X, toPoint.Y); //re-set the start point to the one just added

        IPoint pAdjustedPoint = new PointClass();
        pAdjustedPoint.PutCoords(toPoint.X - dXCorrection, toPoint.Y - dYCorrection);
        TraversePoints[i] = pAdjustedPoint;
      }
      return TraversePoints;
    }
    private IVector GetClosingVector(List<IVector3D> TraverseCourses, IPoint StartPoint, IPoint EndPoint, out double SUMofLengths)
    {
      IVector SumVec = null;
      SUMofLengths = 0;
      for (int i = 0; i < TraverseCourses.Count - 1; i++)
      {
        if (i == 0)
        {
          SUMofLengths = TraverseCourses[0].Magnitude + TraverseCourses[1].Magnitude;
          SumVec = TraverseCourses[0].AddVector(TraverseCourses[1]);
        }
        else
        {
          IVector3D SumVec3D = SumVec as IVector3D;
          SUMofLengths += TraverseCourses[i + 1].Magnitude;
          SumVec = SumVec3D.AddVector(TraverseCourses[i + 1]);
        }
      }

      double dCalcedEndX = StartPoint.X + SumVec.ComponentByIndex[0];
      double dCalcedEndY = StartPoint.Y + SumVec.ComponentByIndex[1];

      IVector3D CloseVector3D = new Vector3DClass();
      CloseVector3D.SetComponents(dCalcedEndX - EndPoint.X, dCalcedEndY - EndPoint.Y, 0);

      IVector CloseVector = CloseVector3D as IVector;
      return CloseVector;
    }

  }
}
