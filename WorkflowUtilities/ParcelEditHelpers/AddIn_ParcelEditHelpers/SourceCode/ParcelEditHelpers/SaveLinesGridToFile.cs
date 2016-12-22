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
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.CadastralUI;
//using ESRI.ArcGIS.Cadastral;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;
//using ESRI.ArcGIS.CartoUI;
//using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;


namespace ParcelEditHelper
{
  public class SaveLinesGridToFile : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public SaveLinesGridToFile()
    {
    }

    protected override void OnClick()
    {
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      ICadastralFabric pCadFabric = pCadEd.CadastralFabric;
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)pCadEd;
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;
      IParcelConstruction pTrav = pParcEditorMan.ParcelConstruction;

      //Test for the visibility of the parcel details window
      IDockableWindowManager pDocWinMgr = (IDockableWindowManager)ArcMap.Application;
      UID pUID = new UIDClass();
      pUID.Value = "{28531B78-7C42-4785-805D-2A7EC8879EA1}";//ArcID.ParcelDetails
      IDockableWindow pParcelDet = pDocWinMgr.GetDockableWindow(pUID);

      if (!pParcelDet.IsVisible())
      {
        MessageBox.Show("The Parcel Details window is not visible. \r\nThere is no data to save.");
        return;
      }

      //Make sure the lines grid is selected
      Utilities UTILS = new Utilities();
      UTILS.SelectCadastralPropertyPage((ICadastralExtensionManager)pCadExtMan, "lines");

      //test to make sure there is data there to be saved
      IParcelConstruction3 pConstr = (IParcelConstruction3)pTrav;
      IGSParcel pParcel = null;

      //
      try
      {
        pParcel = pTrav.Parcel;
      }
      catch (COMException err)
      {
        MessageBox.Show(err.Message + Environment.NewLine + "ERROR: Select a parcel or add lines to the grid. \r\nThere is no data to save. ");
        return;
      }
      //define the file that needs to be saved
      // Display .Net dialog for File saving.
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      // Set File Filter
      saveFileDialog.Filter = "Comma-delimited(*.csv)|*.csv|All Files|*.*";
      saveFileDialog.FilterIndex = 1;
      saveFileDialog.RestoreDirectory = true;
      // Warn on overwrite
      saveFileDialog.OverwritePrompt = true;
      // Don't need to Show Help
      saveFileDialog.ShowHelp = false;
      // Set Dialog Title
      saveFileDialog.Title = "Save file";

      // Display Open File Dialog
      if (saveFileDialog.ShowDialog() != DialogResult.OK)
      {
        saveFileDialog = null;
        return;
      }
      TextWriter tw = null;
      try
      {
        tw = new StreamWriter(saveFileDialog.FileName);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }

      try
      {

        IGSPlan pPlan = pTrav.Parcel.Plan;
        int iDF = (int)pPlan.DirectionFormat;
        switch (iDF)
        {
          case (int)esriDirectionType.esriDTNorthAzimuth:
            tw.WriteLine("DT,NA");
            break;
          case (int)esriDirectionType.esriDTPolar:
            tw.WriteLine("DT,P");
            break;
          case (int)esriDirectionType.esriDTQuadrantBearing:
            tw.WriteLine("DT,QB");
            break;
          case (int)esriDirectionType.esriDTSouthAzimuth:
            tw.WriteLine("DT,SA");
            break;
          default:
            tw.WriteLine("DT,NA");
            break;
        }

        int iAU = (int)pPlan.AngleUnits;
        switch (iAU)
        {
          case (int)esriDirectionUnits.esriDUDecimalDegrees:
            tw.WriteLine("DU,DD");
            break;
          case (int)esriDirectionUnits.esriDUDegreesMinutesSeconds:
            tw.WriteLine("DU,DMS");
            break;
          case (int)esriDirectionUnits.esriDUGons:
          case (int)esriDirectionUnits.esriDUGradians:
            tw.WriteLine("DU,G");
            break;
          case (int)esriDirectionUnits.esriDURadians:
            tw.WriteLine("DU,R");
            break;
          default:
            tw.WriteLine("DU,R");
            break;
        }

        ICadastralUnitConversion pUnitConv = new CadastralUnitConversionClass();
        double dMetricConversion = pUnitConv.ConvertDouble(1, pPlan.DistanceUnits, esriCadastralDistanceUnits.esriCDUMeter);
        string sLU = Convert.ToString(dMetricConversion);

        tw.WriteLine("ToMetricFactor," + sLU);

        IEnumGSLines pGSLines = pTrav.GetLines();
        pGSLines.Reset();
        IGSLine pGSLine = null;
        IGSParcel pGSParcel = null;
        pGSLines.Next(ref pGSParcel, ref pGSLine);
        while (pGSLine != null)
        {
          int iFromPt = pGSLine.FromPoint; //from point
          int iToPt = pGSLine.ToPoint; //to point
          int iLineCat = (int)pGSLine.Category;//line category
          if (iLineCat == 4)
          {
            pGSLines.Next(ref pGSParcel, ref pGSLine);
            continue;//ignore radial lines
          }
          int iLineUserType = pGSLine.LineType;//line user type
          int iAccCat = pGSLine.Accuracy;//accuracy
          double dDistance = pGSLine.Distance;//distance
          double dChord = pGSLine.Distance;//chord
          double dRadius = pGSLine.Radius;//radius

          string sLineCat = Convert.ToString(iLineCat);
          string sLineUserType = Convert.ToString(iLineUserType);

          if (iLineUserType > 2140000000)
            sLineUserType = "";

          string sAccCat = Convert.ToString(iAccCat);
          string sFromPt = Convert.ToString(iFromPt);//from point

          //following need conversion
          string sDirection = NorthAzRadians_2_DirectionString(pGSLine.Bearing, pPlan.DirectionFormat, pPlan.AngleUnits);//direction
          string sDistance = Convert.ToString(dDistance / dMetricConversion);//distance
          string sRadius = "";
          string sChord = "";

          if (dRadius != 123456789)
          {//circular curve
            sRadius = Convert.ToString(dRadius / dMetricConversion);//radius
            sChord = Convert.ToString(dDistance / dMetricConversion);//chord
            sDistance = "";//distance is replaced with the chord distance
          }

          string sToPt = Convert.ToString(iToPt);//to point      
          //write the line
          tw.WriteLine(sFromPt + "," + sDirection + "," + sDistance + "," + sRadius + "," + sChord + "," + sToPt + ","
            + sLineCat + "," + sLineUserType + "," + sAccCat);

          pGSLines.Next(ref pGSParcel, ref pGSLine);
        }
      }
      catch(Exception ex)
      { 
        MessageBox.Show(ex.Message);
      }
      finally
      {
        tw.Close();
        saveFileDialog = null;
      }
    }

    private string NorthAzRadians_2_DirectionString(double inDirection, esriDirectionType ConvertToDirectionType,
  esriDirectionUnits ConvertToDirectionUnits)
    {
      IAngularConverter pAng = new AngularConverter();
      if (pAng.SetAngle(inDirection, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians))
      {
        int iPrec = 7;
        string result = pAng.GetString(ConvertToDirectionType, ConvertToDirectionUnits, iPrec);
        Marshal.ReleaseComObject(pAng);
        return result;
      }
      else
      {
        Marshal.ReleaseComObject(pAng);
        return null;
      }
    }

    protected override void OnUpdate()
    {
    }
  }
}
