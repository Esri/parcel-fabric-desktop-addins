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

      ICadastralFixedPoints pFixedPoints = pCadastralPts as ICadastralFixedPoints;
      IPointCalculation pPointCalc = new PointCalculationClass();

      if (pConstr == null)
        return;

      bool bLineSelectionSequence = false;
      IGSLine pParcelLine = null;
      IGSLine pLastSelectedGSLineInGrid = null;
      IMetricUnitConverter pMetricUnitConv = (IMetricUnitConverter)pCadEd;
      IGSPoint pStartPoint = null;
      IGSPoint pToPoint = null;
      List<int> lstPointIds = new List<int>();

      List<IVector3D> Traverse = new List<IVector3D>();

      pPointCalc = new PointCalculationClass();

      for (int i = 0; i < pConstr.LineCount; i++)
      {
        if (pConstr.GetLineSelection(i))
        {
          if (pConstr.GetLine(i, ref pParcelLine))
          {
            if (!bLineSelectionSequence) //first line
            {
              pStartPoint = pCadastralPts.GetPoint(pParcelLine.FromPoint);
              pToPoint = pCadastralPts.GetPoint(pParcelLine.ToPoint);
            }

            pPointCalc.AddLine(pParcelLine);

            pLastSelectedGSLineInGrid = pParcelLine;
            pToPoint = pCadastralPts.GetPoint(pParcelLine.ToPoint);
            lstPointIds.Add(pToPoint.Id);

          }
          bLineSelectionSequence = true;

          double dBear = pParcelLine.Bearing; //radians polar
          double dDist = pParcelLine.Distance;
          //          dSUMDistance += dDist;
          IVector3D vec = new Vector3DClass();
          vec.PolarSet(dBear, 0, dDist);
          Traverse.Add(vec);
        }
        else
        {
          if (bLineSelectionSequence && pConstr.GetLine(i, ref pParcelLine) && HasLineSelectionAfter(pConstr, i))
          //if there was a prior selection and this line is a complete line, and there is no later selection
          {
            MessageBox.Show("Please select a continous set of lines for closure.");
            return;
          }
        }
      }

      IPoint pStart = new PointClass();
      pStart.X = pStartPoint.X;
      pStart.Y = pStartPoint.Y;


      IGSPoint pClosingPoint = pCadastralPts.GetPoint(pLastSelectedGSLineInGrid.ToPoint);

      double dRatio = 0;

      pPointCalc.Rotation = 2.25 * Math.PI / 180; //radians
      pPointCalc.CalculatePoints(esriParcelAdjustmentType.esriParcelAdjustmentCompass, pStartPoint.Id, pStartPoint, pClosingPoint.Id, pClosingPoint, false);

      ITraverseClosure pClose = pPointCalc.Closure;
      //string sAdjustedPoints = "";
      for (int i = 0; i < pClose.CourseCount; i++)
      {
        double dAdjustedPointX = 0; double dAdjustedPointY = 0;
        IGSPoint pPt = pCadastralPts.GetPoint(lstPointIds[i]);
        dAdjustedPointY = pPointCalc.GetCalculatedPoint(lstPointIds[i], ref dAdjustedPointX);

        pPt.X = dAdjustedPointX;
        pPt.Y = dAdjustedPointY;

        pFixedPoints.SetFixedPoint(lstPointIds[i], true);

        //pMetricUnitConv.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, pAdjustedPoints[i].X, ref dAdjustedPointX);
        //pMetricUnitConv.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, pAdjustedPoints[i].Y, ref dAdjustedPointY);
        //sAdjustedPoints += lstPointIds[i].ToString() + ":  X: " + dAdjustedPointX.ToString("0.00") + "  Y: " + dAdjustedPointY.ToString("0.00") + Environment.NewLine;

      }

      double dMisclosureDistance = pClose.MisclosureDistance; double dMisclosureBearing = pClose.MisclosureDirection;
      IVector MiscloseVector = new Vector3DClass();

      //double dXDiff = MiscloseVector.XComponent;
      //double dYDiff = MiscloseVector.YComponent;

      //pMetricUnitConv.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, dMisclosureDistance, ref dMisclosureDistance);
      //pMetricUnitConv.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, dXDiff, ref dXDiff);
      //pMetricUnitConv.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, dYDiff, ref dYDiff);

      IAngularConverter pAngConv = new AngularConverterClass();
      pAngConv.SetAngle(dMisclosureBearing, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);
      string sMiscloseBearing = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);

      string sReport = "Closure:" + Environment.NewLine +
        "        error:  " + "1:" + dRatio.ToString("0") + Environment.NewLine +
        "        distance:  " + dMisclosureDistance.ToString("0.000") + Environment.NewLine +
        "        bearing:  " + sMiscloseBearing + Environment.NewLine;// +
      //"        xdist:  " + (dXDiff * -1).ToString("0.000") + Environment.NewLine +
      //"        ydist:  " + (dYDiff * -1).ToString("0.000") + Environment.NewLine;// +sAdjustedPoints;

      MessageBox.Show(sReport, "Closure Report");

    }
    
    protected override void OnUpdate()
    {
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
