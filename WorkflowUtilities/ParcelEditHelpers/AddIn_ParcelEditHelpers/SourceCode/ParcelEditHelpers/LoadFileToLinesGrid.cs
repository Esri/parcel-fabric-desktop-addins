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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;

using System.Runtime.InteropServices;


namespace ParcelEditHelper
{
  public class LoadFileToLinesGrid : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    int m_count = 1;
    double m_dScaleFactor = -1;
    enum FileType { COGOToolbarTraverse, CommaDelimited };
    public LoadFileToLinesGrid()
    {
    }

    protected override void OnClick()
    {
      FileType ft = FileType.COGOToolbarTraverse; //start out assuming it is a cogo traverse file
      m_count = 1;
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      ICadastralFabric pCadFabric = pCadEd.CadastralFabric;
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)pCadEd;
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;
      IParcelConstruction pTrav = pParcEditorMan.ParcelConstruction;

      if (!(pParcEditorMan.InTraverseEditMode) && !(pParcEditorMan.InConstructionEditMode))
      {//if this is not a construction or a new parcel, then get out.
        MessageBox.Show("Please create a new parcel or new construction first, and then try again.");
        return;
      }

      //Make sure the lines grid is selected
      Utilities UTILS = new Utilities();
      UTILS.SelectCadastralPropertyPage((ICadastralExtensionManager)pCadExtMan, "lines");

      IParcelConstruction3 pTrav3 = (IParcelConstruction3)pTrav;
      IGSParcel pParcel = null;
      try
      {
        pParcel = pTrav.Parcel;
      }
      catch (COMException error)
      {
        MessageBox.Show(error.Message.ToString());
        return;
      }
      //go get a traverse file
      // Display .Net dialog for File selection.
      OpenFileDialog openFileDialog = new OpenFileDialog();
      // Set File Filter
      openFileDialog.Filter = "Traverse file (*.txt)|*.txt|Comma-delimited(*.csv)|*.csv|All Files|*.*";
      // Disable multi-select
      openFileDialog.Multiselect = false;
      // Don't need to Show Help
      openFileDialog.ShowHelp = false;
      // Set Dialog Title
      openFileDialog.Title = "Load file";
      openFileDialog.FilterIndex = 2;
      // Display Open File Dialog
      if (openFileDialog.ShowDialog() != DialogResult.OK)
      {
        openFileDialog = null;
        return;
      }

      TextReader tr = null;
      try
      {
        tr = new StreamReader(openFileDialog.FileName);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }

      string sCourse = "";
      int iCount = 0;
      string[] sFileLine = new string[0]; //define as dynamic array 
      //initialize direction units and format
      esriDirectionType enumDirectionType = esriDirectionType.esriDTQuadrantBearing;
      esriDirectionUnits enumDirectionUnits = esriDirectionUnits.esriDUDegreesMinutesSeconds;

      //initialize start and end points
      IPoint StartPoint = new PointClass();
      IPoint EndPoint = new PointClass();
      bool IsLoopTraverse = false;

      //fill the array with the lines from the file
      while (sCourse != null)
      {
        sCourse = tr.ReadLine();
        try
        {
          if (sCourse.Trim().Length >= 1) //test for empty lines
          {
            RedimPreserveString(ref sFileLine, 1);
            sFileLine[iCount] = sCourse;
          }
          iCount++;
          sCourse = sCourse.ToLower();
          if (sCourse.Contains("dt"))
          {
            if (sCourse.Contains("qb"))
              enumDirectionType = esriDirectionType.esriDTQuadrantBearing;
            else if (sCourse.Contains("na"))
              enumDirectionType = esriDirectionType.esriDTNorthAzimuth;
            else if (sCourse.Contains("sa"))
              enumDirectionType = esriDirectionType.esriDTSouthAzimuth;
            else if (sCourse.Contains("p"))
              enumDirectionType = esriDirectionType.esriDTPolar;
          }
          if (sCourse.Contains("du"))
          {
            if (sCourse.Contains("dms"))
              enumDirectionUnits = esriDirectionUnits.esriDUDegreesMinutesSeconds;
            else if (sCourse.Contains("dd"))
              enumDirectionUnits = esriDirectionUnits.esriDUDecimalDegrees;
            else if (sCourse.Contains("g"))
              enumDirectionUnits = esriDirectionUnits.esriDUGons;
            else if (sCourse.Contains("r"))
              enumDirectionUnits = esriDirectionUnits.esriDURadians;
          }
          if (sCourse.Contains("sp"))
          {//start point
            string[] XY = sCourse.Split(' ');
            double x = Convert.ToDouble(XY[1]);
            double y = Convert.ToDouble(XY[2]);
            StartPoint.PutCoords(x, y);
          }

          if (sCourse.Contains("ep"))
          {//end point
            string[] XY = sCourse.Split(' ');
            double x = Convert.ToDouble(XY[1]);
            double y = Convert.ToDouble(XY[2]);
            EndPoint.PutCoords(x, y);
          }

          if (sCourse.Contains("tometricfactor"))
          {//this handles the comma-separated file case
            string[] sScaleFactor = sCourse.Split(',');
            m_dScaleFactor = Convert.ToDouble(sScaleFactor[1]);
          }
        }
        catch { }
      }
      tr.Close(); //close the file and release resources
      string sFileExt = System.IO.Path.GetExtension(openFileDialog.FileName.TrimEnd());
      if ((sFileExt.ToLower() == ".csv") && (sFileLine[0].Contains(",")))
      {//if it's a comma-delimited file
        ft = FileType.CommaDelimited;
      }

      //Test for loop traverse
      if (!(EndPoint.IsEmpty))
      {
        if (EndPoint.Compare(StartPoint) == 0)
          IsLoopTraverse = true;
        else
          IsLoopTraverse = false;
      }

      //get highest point id number in grid, and get the to point id on the last line.
      IGSLine pParcelLine = null;
      int iFirstToNode = -1;
      int iLastToNode = -1;
      int iHighestPointID = -1;
      for (int i = 0; i < pTrav.LineCount; i++)
      {
        if (pTrav.GetLine(i, ref pParcelLine))
        {
          if (iFirstToNode < 0)
            iFirstToNode = pParcelLine.ToPoint;
          iLastToNode = pParcelLine.ToPoint;
          iHighestPointID = iHighestPointID < pParcelLine.ToPoint ? pParcelLine.ToPoint : iHighestPointID;
          iHighestPointID = iHighestPointID < pParcelLine.FromPoint ? pParcelLine.FromPoint : iHighestPointID;
        }
      }

      ICadastralUndoRedo pCadUndoRedo = pTrav as ICadastralUndoRedo;
      pCadUndoRedo.StartUndoRedoSession("Load Lines From File");
      try
      {
        IGSLine pLine = null;
        int iLinecount = iCount - 1;
        ISegment pExitTangent = null;
        for (iCount = 0; iCount <= iLinecount; iCount++)
        {
          if (ft == FileType.COGOToolbarTraverse)
          {
            {
              pLine = null;

              ICircularArc pCircArc;//need to use this in the test to handle curves greater than 180
              pLine = CreateGSLine(sFileLine[iCount], enumDirectionUnits, enumDirectionType,
                      pExitTangent, out pExitTangent, out pCircArc);
              //exit tangent from the previous course is the new entry tangent for the next course

              if (pLine != null)
              {
                if (pCircArc == null)
                {// straight line
                  //if this is the last course then set the to point to 1
                  if ((iCount == iLinecount) && IsLoopTraverse)
                    pLine.ToPoint = 1;
                  pTrav.InsertGridRow(-1, pLine);
                }
                else
                {//some post-processing needed to figure out if a 180 curve needs to be split
                  if (Math.Abs(pCircArc.CentralAngle) < (Math.PI - 0.000001))//some tolerance for being close to 180
                  { //this curve is OK
                    //if this is the last course then set the to point to 1
                    if ((iCount == iLinecount) && IsLoopTraverse)
                      pLine.ToPoint = 1;
                    pTrav.InsertGridRow(-1, pLine);
                  }
                  else
                  {//curve is greater than or equal to 180, special treatment for GSE needed to split curve into 2 parts
                    //first decrement the count
                    m_count -= 1;
                    ISegment pFullSegment = (ISegment)pCircArc; ISegment pFirstHalf; ISegment pSecondHalf;
                    pFullSegment.SplitAtDistance(0.5, true, out pFirstHalf, out pSecondHalf);
                    IConstructCircularArc2 pCircArcConstr1 = new CircularArcClass();
                    ICircularArc pCircArc1 = (ICircularArc)pCircArcConstr1;
                    pCircArcConstr1.ConstructEndPointsRadius(pFirstHalf.FromPoint,
                            pFirstHalf.ToPoint, pCircArc.IsCounterClockwise, pCircArc.Radius, true);
                    ILine2 pTangentLine = new LineClass();
                    pCircArc1.QueryTangent(esriSegmentExtension.esriExtendTangentAtTo, 0, false, 100, pTangentLine);
                    string sTangentBearing = PolarRadians_2_DirectionString(pTangentLine.Angle,
                            enumDirectionType, enumDirectionUnits);
                    sTangentBearing = sTangentBearing.Replace(" ", "");
                    string sHalfDelta = Radians_2_Angle(Math.Abs(pCircArc1.CentralAngle), enumDirectionUnits);
                    string sSide = pCircArc.IsCounterClockwise ? " L " : " R ";

                    ISegment EntryTangent = (ISegment)pTangentLine;

                    //construct the string for the first piece
                    // looks similar to this: NC R 500 D 181-59-59 T N59-59-59W L
                    string sFirstCurve = "NC R " + Convert.ToString(pCircArc.Radius)
                          + " D " + sHalfDelta + " T " + sTangentBearing + sSide;

                    IGSLine pLineFirstCurve = CreateGSLine(sFirstCurve, enumDirectionUnits, enumDirectionType, pExitTangent,
                            out pExitTangent, out pCircArc);
                    pTrav.InsertGridRow(-1, pLineFirstCurve);

                    ICircularArc pCircArc2 = (ICircularArc)pCircArcConstr1;
                    pCircArcConstr1.ConstructEndPointsRadius(pSecondHalf.FromPoint,
                            pSecondHalf.ToPoint, pCircArc.IsCounterClockwise, pCircArc.Radius, true);
                    pCircArc2.QueryTangent(esriSegmentExtension.esriExtendTangentAtTo, 0, false, 100, pTangentLine);
                    sTangentBearing = PolarRadians_2_DirectionString(pTangentLine.Angle, enumDirectionType, enumDirectionUnits);
                    sTangentBearing = sTangentBearing.Replace(" ", "");
                    //construct the string for the second piece
                    // looks similar to this: NC R 500 D 181-59-59 T N59-59-59W L
                    string sSecondCurve = "NC R " + Convert.ToString(pCircArc.Radius)
                          + " D " + sHalfDelta + " T " + sTangentBearing + sSide;

                    IGSLine pLineSecondCurve = CreateGSLine(sSecondCurve, enumDirectionUnits, enumDirectionType, pExitTangent,
                            out pExitTangent, out pCircArc);
                    //if this is the last course then set the to point to 1
                    if ((iCount == iLinecount) && IsLoopTraverse)
                      pLine.ToPoint = 1;
                    pTrav.InsertGridRow(-1, pLineSecondCurve);
                  }
                }
              }
            }
          }
          else//this is comma-separated version of the grid, so do the following
          {
            pLine = null;
            //apply a point id number offset if there are existing lines in the grid.
            if (iHighestPointID > -1 && iCount >= 3)
            {
              string[] sTraverseCourse = sFileLine[iCount].Split(',');
              if (iCount == 3)
              {
                sTraverseCourse[0] = iLastToNode.ToString();
                sTraverseCourse[5] = (Convert.ToInt32(sTraverseCourse[5]) + iHighestPointID).ToString();
              }
              else if (iCount > 3)
              {
                sTraverseCourse[0] = (Convert.ToInt32(sTraverseCourse[0]) + iHighestPointID).ToString();
                sTraverseCourse[5] = (Convert.ToInt32(sTraverseCourse[5]) + iHighestPointID).ToString();
              }

              sFileLine[iCount] = sTraverseCourse[0];
              int iAttCount = sTraverseCourse.GetLength(0);
              for (int j = 1; j < iAttCount; j++)
                sFileLine[iCount] += "," + sTraverseCourse[j];
            }

            pLine = CreateGSLineFromCommaSeparatedString(sFileLine[iCount], enumDirectionUnits, enumDirectionType);

            if (pLine != null)
              pTrav.InsertGridRow(-1, pLine);
          }
        }
        pTrav3.UpdateGridFromGSLines(false);
        IParcelConstruction2 pConstr2 = (IParcelConstruction2)pTrav3; //hidden interface
        pConstr2.RecalculatePoints(); //explicit recalculate needed on a construction
        pParcel.Modified();
        pParcEditorMan.Refresh();
        pCadUndoRedo.WriteUndoRedoSession(true);
        sFileLine = null;
        openFileDialog = null;
      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message,"Load Lines From File");
        pCadUndoRedo.WriteUndoRedoSession(false);
      }
    }

    private IGSLine CreateGSLine(String inCourse, esriDirectionUnits inDirectionUnits,
            esriDirectionType inDirectionType, ISegment EntryTangent,
            out ISegment ExitTangent, out ICircularArc outCircularArc)
    {//
      string[] sCourse = inCourse.Split(' ');
      Utilities UTILS = new Utilities();
      double dToMeterUnitConversion = UTILS.ToMeterUnitConversion();
      // ISegment ExitTangent = null;
      string item = (string)sCourse.GetValue(0);
      if (item.ToLower() == "dd")
      {//Direction distance -- straight line course
        IGSLine pLine = new GSLineClass();
        double dBear = DirectionString_2_NorthAzimuth((string)sCourse.GetValue(1), inDirectionType, inDirectionUnits);
        pLine.Bearing = dBear;
        pLine.Distance = Convert.ToDouble(sCourse.GetValue(2)) * dToMeterUnitConversion;
        pLine.FromPoint = m_count;
        m_count += 1;
        pLine.ToPoint = m_count;
        //Now define the tangent exit segment
        //in case it's needed for the next course (TC tangent curve, or Angle deflection)
        double dPolar = DirectionString_2_PolarRadians((string)sCourse.GetValue(1),
                inDirectionType, inDirectionUnits);
        IPoint pFromPt = new PointClass();
        pFromPt.PutCoords(1000, 1000);
        IConstructPoint2 pToPt = new PointClass();
        pToPt.ConstructAngleDistance(pFromPt, dPolar, 100);
        ILine ExitTangentLine = new LineClass();
        ExitTangentLine.PutCoords(pFromPt, (IPoint)pToPt);
        ExitTangent = (ISegment)ExitTangentLine;
        outCircularArc = null;
        Marshal.ReleaseComObject(pFromPt);
        Marshal.ReleaseComObject(pToPt);
        return pLine;
      }
      if (item.ToLower() == "ad")
      {//Angle deflection distance
        IGSLine pLine = new GSLineClass();
        double dDeflAngle = Angle_2_Radians((string)sCourse.GetValue(1), inDirectionUnits);
        //now need to take the previous tangent segment, reverse its orientation and 
        //add +ve clockwise to get the bearing
        ILine calcLine = (ILine)EntryTangent;
        double dBear = PolarRAD_2_SouthAzimuthRAD(calcLine.Angle) + dDeflAngle;
        pLine.Bearing = dBear;
        pLine.Distance = Convert.ToDouble(sCourse.GetValue(2)) * dToMeterUnitConversion;
        pLine.FromPoint = m_count;
        m_count += 1;
        pLine.ToPoint = m_count;
        //Now define the tangent exit segment
        //in case it's needed for the next course (TC tangent curve, or Angle deflection)
        IPoint pFromPt = new PointClass();
        pFromPt.PutCoords(1000, 1000);
        IConstructPoint2 pToPt = new PointClass();
        double dPolar = NorthAzimuthRAD_2_PolarRAD(dBear);
        pToPt.ConstructAngleDistance(pFromPt, dPolar, 100);
        ILine ExitTangentLine = new LineClass();
        ExitTangentLine.PutCoords(pFromPt, (IPoint)pToPt);
        ExitTangent = (ISegment)ExitTangentLine;
        outCircularArc = null;
        Marshal.ReleaseComObject(pFromPt);
        Marshal.ReleaseComObject(pToPt);

        return pLine;
      }
      else if ((item.ToLower() == "nc") || (item.ToLower() == "tc"))
      {
        double dChordlength;
        double dChordBearing;
        ICircularArc pArc = ConstructCurveFromString(inCourse, EntryTangent,
                inDirectionType, inDirectionUnits, out dChordlength, out dChordBearing);
        try
        {
          IGSLine pLine = new GSLineClass();
          pLine.Bearing = PolarRAD_2_NorthAzimuthRAD(dChordBearing);
          pLine.Radius = pArc.Radius * dToMeterUnitConversion;//convert to meters
          if (pArc.IsCounterClockwise) { pLine.Radius = pLine.Radius * -1; }
          pLine.Distance = dChordlength * dToMeterUnitConversion; //convert to meters
          pLine.FromPoint = m_count;
          m_count += 1;
          pLine.ToPoint = m_count;
          ILine pTangentLine = new LineClass();
          pArc.QueryTangent(esriSegmentExtension.esriExtendTangentAtTo, 1, true, 100, pTangentLine);
          //pass the exit tangent back out for use as next entry tangent
          ExitTangent = (ISegment)pTangentLine;
          outCircularArc = pArc;
          return pLine;
        }
        catch { }
      }
      outCircularArc = null;
      ExitTangent = null;
      return null;
    }
    private IGSLine CreateGSLineFromCommaSeparatedString(String inCourse, esriDirectionUnits inDirectionUnits,
            esriDirectionType inDirectionType)
    {
      string[] sCourse = inCourse.Split(',');
      int iUpperBnd = sCourse.GetUpperBound(0);
      double dToMeterUnitConversion = m_dScaleFactor;
      if (m_dScaleFactor == -1)
      {//if the scale factor wasn't found in the file for some reason
        Utilities UTILS = new Utilities();
        dToMeterUnitConversion = UTILS.ToMeterUnitConversion();
      }
      if (iUpperBnd <= 4) { return null; }
      IGSLine pLine = new GSLineClass();
      string sLineCat = "";
      string sLineUserType = "";
      string sAccCat = "";
      string sFromPt = (string)sCourse.GetValue(0);//from point
      //string sTemplate = (string)sCourse.GetValue(1);//line template
      string sDirection = (string)sCourse.GetValue(1);//direction
      string sDistance = (string)sCourse.GetValue(2);//distance
      string sRadius = (string)sCourse.GetValue(3);//radius
      string sChord = (string)sCourse.GetValue(4);//chord
      string sToPt = (string)sCourse.GetValue(5);//to point
      if (iUpperBnd >= 6)
        sLineCat = (string)sCourse.GetValue(6); //line category
      if (iUpperBnd >= 7)
        sLineUserType = (string)sCourse.GetValue(7); //line user type
      if (iUpperBnd >= 8)
        sAccCat = (string)sCourse.GetValue(8); //accuracy category

      int iFromPt = -1; //from point
      int iToPt = -1; //to point
      int iLineCat = -1;//line category
      int iLineUserType = -1;//line user type
      int iAccCat = -1;//accuracy
      double dDistance = -123456789;//distance
      double dChord = -123456789;//chord
      double dRadius = -123456789;//radius

      try
      {
        iFromPt = Convert.ToInt32(sFromPt); //from point
        iToPt = Convert.ToInt32(sToPt); //to point
        if (sLineCat.Trim().Length > 0)
          iLineCat = Convert.ToInt32(sLineCat); //line category
        if (sLineUserType.Trim().Length > 0)
          iLineUserType = Convert.ToInt32(sLineUserType); //line user type
        if (sAccCat.Trim().Length > 0)
          iAccCat = Convert.ToInt32(sAccCat); //accuracy
        if (sDistance.Trim().Length > 0)
          dDistance = Convert.ToDouble(sDistance); //distance
        if (sChord.Trim().Length > 0)
          dChord = Convert.ToDouble(sChord); //chord
        if (sRadius.Trim().Length > 0)
          dRadius = Convert.ToDouble(sRadius); //radius
      }
      catch
      {
        return null;
      }
      double dBear = DirectionString_2_NorthAzimuth(sDirection, inDirectionType, inDirectionUnits);
      pLine.Bearing = dBear;
      if (dDistance != -123456789)
        pLine.Distance = dDistance * dToMeterUnitConversion;
      if (dChord != -123456789)
        pLine.Distance = dChord * dToMeterUnitConversion;
      if (dRadius != -123456789)
        pLine.Radius = dRadius * dToMeterUnitConversion;
      pLine.FromPoint = iFromPt;
      pLine.ToPoint = iToPt;
      if (iAccCat > -1)
        pLine.Accuracy = iAccCat;
      if (iLineUserType > -1)
        pLine.LineType = iLineUserType;
      if (iLineCat > -1)
        pLine.Category = (esriCadastralLineCategory)iLineCat;
      return pLine;
    }

    private ICircularArc ConstructCurveFromString(string inString, ISegment ExitTangentFromPreviousCourse,
          esriDirectionType inDirectionType, esriDirectionUnits inDirectionUnits,
          out double outChordLength, out double outChordBearing)
    {//
      IConstructCircularArc pConstArc = new CircularArcClass();
      ICircularArc pArc = (ICircularArc)pConstArc;
      IPoint pPt = new PointClass();
      pPt.PutCoords(1000, 1000);
      //initialize the curve params
      bool bHasRadius = false; double dRadius = -1;
      bool bHasChord = false; double dChord = -1;
      bool bHasArc = false; double dArcLength = -1;
      bool bHasDelta = false; double dDelta = -1;
      bool bCCW = false; //assume curve to right unless proven otherwise
      //now initialize bearing types for non-tangent curves
      bool bHasRadialBearing = false; double dRadialBearing = -1;
      bool bHasChordBearing = false; double dChordBearing = -1;
      bool bHasTangentBearing = false; double dTangentBearing = -1;
      ISegment EntryTangentSegment = null;

      int iItemPosition = 0;

      string[] sCourse = inString.Split(' ');
      int UpperBound = sCourse.GetUpperBound(0);
      bool bIsTangentCurve = (((string)sCourse.GetValue(0)).ToLower() == "tc");
      foreach (string item in sCourse)
      {
        if (item == null)
          break;
        if ((item.ToLower() == "r") && (!bHasRadius) && (iItemPosition <= 3))
        {// this r is for radius
          dRadius = Convert.ToDouble(sCourse.GetValue(iItemPosition + 1));
          bHasRadius = true; //found a radius
        }
        if ((item.ToLower()) == "c" && (!bHasChord) && (iItemPosition <= 3))
        {// this c is for chord length
          dChord = Convert.ToDouble(sCourse.GetValue(iItemPosition + 1));
          bHasChord = true; //found a chord length
        }
        if ((item.ToLower()) == "a" && (!bHasArc) && (iItemPosition <= 3))
        {// this a is for arc length
          dArcLength = Convert.ToDouble(sCourse.GetValue(iItemPosition + 1));
          bHasArc = true; //found an arc length
        }
        if ((item.ToLower()) == "d" && (!bHasDelta) && (iItemPosition <= 3))
        {// this d is for delta or central angle
          dDelta = Angle_2_Radians((string)sCourse.GetValue(iItemPosition + 1), inDirectionUnits);
          bHasDelta = true; //found a central angle
        }
        if ((item.ToLower()) == "r" && (!bHasRadialBearing) && (iItemPosition > 3) && (UpperBound > 5))
        {// this r is for radial bearing
          try
          {
            dRadialBearing = DirectionString_2_PolarRadians((string)sCourse.GetValue(iItemPosition + 1), inDirectionType, inDirectionUnits);
            if (!(dRadialBearing == -999)) { bHasRadialBearing = true; } //found a radial bearing
          }
          catch { }//this will catch case of final R meaning a curve right and not radial bearing
        }
        if ((item.ToLower()) == "c" && (!bHasChordBearing) && (iItemPosition > 3))
        {// this c is for chord bearing
          dChordBearing = DirectionString_2_PolarRadians((string)sCourse.GetValue(iItemPosition + 1), inDirectionType, inDirectionUnits);
          bHasChordBearing = true; //found a chord bearing
        }
        if ((item.ToLower()) == "t" && (!bHasTangentBearing) && (iItemPosition > 3))
        {// this t is for tangent bearing
          dTangentBearing = DirectionString_2_PolarRadians((string)sCourse.GetValue(iItemPosition + 1), inDirectionType, inDirectionUnits);
          bHasTangentBearing = true; //found a tangent bearing
          IConstructPoint2 pToPt = new PointClass();
          pToPt.ConstructAngleDistance(pPt, dTangentBearing, 100);
          ILine EntryTangentLine = new LineClass();
          EntryTangentLine.PutCoords(pPt, (IPoint)pToPt);
          EntryTangentSegment = (ISegment)EntryTangentLine;
        }

        if ((item.ToLower()) == "l")
          // this l is for defining a curve to the left
          bCCW = true;
        iItemPosition += 1;
      }

      if (!(bIsTangentCurve)) //non-tangent curve
      {//chord bearing
        if (bHasRadius && bHasChord && bHasChordBearing)
        {
          try
          {
            pConstArc.ConstructBearingRadiusChord(pPt, dChordBearing, bCCW, dRadius, dChord, true);
          }
          catch { };
        }

        if (bHasRadius && bHasArc && bHasChordBearing)
        {
          try
          {
            pConstArc.ConstructBearingRadiusArc(pPt, dChordBearing, bCCW, dRadius, dArcLength);
          }
          catch { };
        }

        if (bHasRadius && bHasDelta && bHasChordBearing)
        {
          try
          {
            pConstArc.ConstructBearingRadiusAngle(pPt, dChordBearing, bCCW, dRadius, dDelta);
          }
          catch { };
        }

        if (bHasChord && bHasDelta && bHasChordBearing)
        {
          try
          {
            pConstArc.ConstructBearingAngleChord(pPt, dChordBearing, bCCW, dDelta, dChord);
          }
          catch { };
        }

        if (bHasChord && bHasArc && bHasChordBearing)
        {
          try
          {
            pConstArc.ConstructBearingChordArc(pPt, dChordBearing, bCCW, dChord, dArcLength);
          }
          catch { };
        }

        //tangent bearing
        if (bHasRadius && bHasChord && bHasTangentBearing)
        {
          try
          {
            pConstArc.ConstructTangentRadiusChord(EntryTangentSegment, false, bCCW, dRadius, dChord);
          }
          catch { };
        }

        if (bHasRadius && bHasArc && bHasTangentBearing)
        {
          try
          {
            pConstArc.ConstructTangentRadiusArc(EntryTangentSegment, false, bCCW, dRadius, dArcLength);
          }
          catch { };
        }
        if (bHasRadius && bHasDelta && bHasTangentBearing)
        {
          try
          {
            pConstArc.ConstructTangentRadiusAngle(EntryTangentSegment, false, bCCW, dRadius, dDelta);
          }
          catch { };
        }

        if (bHasChord && bHasDelta && bHasTangentBearing)
        {
          try
          {
            pConstArc.ConstructTangentAngleChord(EntryTangentSegment, false, bCCW, dDelta, dChord);
          }
          catch { };
        }
        if (bHasChord && bHasArc && bHasTangentBearing)
        {
          try
          {
            pConstArc.ConstructTangentChordArc(EntryTangentSegment, false, bCCW, dChord, dArcLength);
          }
          catch { };
        }

        //radial bearing
        if (bHasRadialBearing)
        {
          //need to convert radial bearing to tangent bearing by adding/subtracting 90 degrees
          double dTanBear = 0;
          if (bCCW)
            dTanBear = dRadialBearing - (Angle_2_Radians("90", esriDirectionUnits.esriDUDecimalDegrees));
          else
            dTanBear = dRadialBearing + (Angle_2_Radians("90", esriDirectionUnits.esriDUDecimalDegrees));
          IConstructPoint2 pToPt = new PointClass();
          pToPt.ConstructAngleDistance(pPt, dTanBear, 100);
          ILine EntryTangentLine = new LineClass();
          EntryTangentLine.PutCoords(pPt, (IPoint)pToPt);
          EntryTangentSegment = (ISegment)EntryTangentLine;
        }

        if (bHasRadius && bHasChord && bHasRadialBearing)
        {
          try
          {
            pConstArc.ConstructTangentRadiusChord(EntryTangentSegment, false, bCCW, dRadius, dChord);
          }
          catch { };
        }
        if (bHasRadius && bHasArc && bHasRadialBearing)
        {
          try
          {
            pConstArc.ConstructTangentRadiusArc(EntryTangentSegment, false, bCCW, dRadius, dArcLength);
          }
          catch { };
        }
        if (bHasRadius && bHasDelta && bHasRadialBearing)
        {
          try
          {
            pConstArc.ConstructTangentRadiusAngle(EntryTangentSegment, false, bCCW, dRadius, dDelta);
          }
          catch { };
        }
        if (bHasChord && bHasDelta && bHasRadialBearing)
        {
          try
          {
            pConstArc.ConstructTangentAngleChord(EntryTangentSegment, false, bCCW, dDelta, dChord);
          }
          catch { };
        }
        if (bHasChord && bHasArc && bHasRadialBearing)
        {
          try
          {
            pConstArc.ConstructTangentChordArc(EntryTangentSegment, false, bCCW, dChord, dArcLength);
          }
          catch { };
        }
      }
      else
      { //tangent curve
        if (bHasRadius && bHasChord)
        {
          try
          {
            pConstArc.ConstructTangentRadiusChord(ExitTangentFromPreviousCourse, false, bCCW, dRadius, dChord);
          }
          catch { };
        }
        if (bHasRadius && bHasArc)
        {
          try
          {
            pConstArc.ConstructTangentRadiusArc(ExitTangentFromPreviousCourse, false, bCCW, dRadius, dArcLength);
          }
          catch { };
        }
        if (bHasRadius && bHasDelta)
        {
          try
          {
            pConstArc.ConstructTangentRadiusAngle(ExitTangentFromPreviousCourse, false, bCCW, dRadius, dDelta);
          }
          catch { };
        }
        if (bHasChord && bHasDelta)
        {
          try
          {
            pConstArc.ConstructTangentAngleChord(ExitTangentFromPreviousCourse, false, bCCW, dDelta, dChord);
          }
          catch { };
        }
        if (bHasChord && bHasArc)
        {
          try
          {
            pConstArc.ConstructTangentChordArc(ExitTangentFromPreviousCourse, false, bCCW, dChord, dArcLength);
          }
          catch { };
        }
      }
      ILine pLine = new LineClass();
      try
      {
        pLine.PutCoords(pArc.FromPoint, pArc.ToPoint);
      }
      catch
      {
        outChordLength = -1; outChordBearing = -1;
        return null;
      }
      outChordLength = pLine.Length;
      outChordBearing = pLine.Angle;
      return pArc;
    }

    private double DirectionString_2_NorthAzimuth(String inBearing, esriDirectionType ConvertFromDirectionType,
     esriDirectionUnits ConvertFromDirectionUnits)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetString(inBearing, ConvertFromDirectionType, ConvertFromDirectionUnits))
      {
        double result = pAng.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);
        return result;
      }
      else
        return -999;
    }

    private double PolarRAD_2_NorthAzimuthRAD(double inBearing)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetAngle(inBearing, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians))
      {
        double result = pAng.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);
        return result;
      }
      else
        return -999;
    }

    private double NorthAzimuthRAD_2_PolarRAD(double inBearing)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetAngle(inBearing, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians))
      {
        double result = pAng.GetAngle(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        return result;
      }
      else
        return -999;
    }

    private double PolarRAD_2_SouthAzimuthRAD(double inBearing)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetAngle(inBearing, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians))
      {
        double result = pAng.GetAngle(esriDirectionType.esriDTSouthAzimuth, esriDirectionUnits.esriDURadians);
        return result;
      }
      else
        return -999;
    }

    private double DirectionString_2_PolarRadians(String inDirection, esriDirectionType ConvertFromDirectionType,
      esriDirectionUnits ConvertFromDirectionUnits)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetString(inDirection, ConvertFromDirectionType, ConvertFromDirectionUnits))
      {
        double result = pAng.GetAngle(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        return result;
      }
      else
        return -999;
    }

    private string PolarRadians_2_DirectionString(double inDirection, esriDirectionType ConvertToDirectionType,
      esriDirectionUnits ConvertToDirectionUnits)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetAngle(inDirection, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians))
      {
        int iPrec = 7;
        string result = pAng.GetString(ConvertToDirectionType, ConvertToDirectionUnits, iPrec);
        return result;
      }
      else
        return null;
    }

    private double Angle_2_Radians(String inAngle, esriDirectionUnits inAngleUnits)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetString(inAngle, esriDirectionType.esriDTPolar, inAngleUnits))
      {
        double result = pAng.GetAngle(esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
        return result;
      }
      else
        return -999;
    }

    private string Radians_2_Angle(double inAngle, esriDirectionUnits outAngleUnits)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetAngle(inAngle, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians))
      {
        string result = pAng.GetString(esriDirectionType.esriDTPolar, outAngleUnits, 7);
        return result;
      }
      else
        return null;
    }

    private string[] RedimPreserveString(ref string[] x, int ResizeIncrement)
    {
      string[] Temp1 = new string[x.GetLength(0) + ResizeIncrement];
      if (x != null)
        System.Array.Copy(x, Temp1, x.Length);
      x = Temp1;
      Temp1 = null;
      return x;
    }






    protected override void OnUpdate()
    {
    }
  }
}
