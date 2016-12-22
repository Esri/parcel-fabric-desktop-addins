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
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;

namespace ParcelEditHelper
{
  class Utilities
  {
    //public RegistryFindKey()
    //{

    ////    '------------4/2/2012 ---------------
    ////'first get the service pack info to see if this code needs to run
    ////Dim sAns As String
    ////Dim bIsSP4 As Boolean = False
    ////Dim sErr As String = ""
    ////sAns = RegValue(RegistryHive.LocalMachine, "SOFTWARE\ESRI\Desktop10.0\CoreRuntime", "InstallVersion", sErr)
    ////bIsSP4 = (sAns = "10.0.4000")

    ////'also this workaround does not work in Manual Mode
    ////'check if there is a Manual Mode "modify" job active ===========
    //}

    public void SelectCadastralPropertyPage(ICadastralExtensionManager CadastralExtManager, string PageName)
    {//Set the property page to the lines grid [th]
      IParcelPropertiesWindow2 pPPWnd2 = (IParcelPropertiesWindow2)CadastralExtManager.ParcelPropertiesWindow;

      ICadastralEditorPages pPgs = (ICadastralEditorPages)pPPWnd2;
      ICadastralEditorPage pPg = null;

      int lPg = 0;
      for (lPg = 0; lPg <= (pPPWnd2.PageCount - 1); lPg++)
      {
        pPg = pPPWnd2.get_Page(lPg);
        if (pPg.PageName.ToLower() == PageName.ToLower())
        {
          pPgs.SelectPage(pPg);
          break;
        }
      }
    }

    public double ToMeterUnitConversion()
    {
      IMap pMap = ArcMap.Document.FocusMap;
      ISpatialReference2 pSpatRef = (ISpatialReference2)pMap.SpatialReference;

      if (!(pSpatRef == null))
      {
        try
        {
          IProjectedCoordinateSystem2 pPCS = (IProjectedCoordinateSystem2)pSpatRef;
          ILinearUnit pMapLU = pPCS.CoordinateUnit;
          return pMapLU.MetersPerUnit;
        }
        catch { }
      }
      return 1;
    }

    public List<string> ReadFabricAdjustmentSettingsFromRegistry(string Path)
    {
      try
      {
        RegistryKey regKeyAppRoot = Registry.CurrentUser.CreateSubKey(Path);
        string sValues = (string)regKeyAppRoot.GetValue("FabricAdjustmentAddIn");
        string[] Values = sValues.Split(',');
        List<string> RegKeyList = Values.ToList();
        return RegKeyList;
      }
      catch
      {
        return null;
      }
    }

    public string FormatDirectionDashesToDegMinSecSymbols(string Bearing)
    {
      Bearing = Bearing.Replace(" ", "");
      if(Bearing.EndsWith("E") || Bearing.EndsWith("e") || Bearing.EndsWith("W") || Bearing.EndsWith("w"))
        Bearing = Bearing.Insert(Bearing.Length -1, "\"");
      else
        Bearing = Bearing.Insert(Bearing.Length, "\"");
      int i = Bearing.LastIndexOf('-');
      Bearing = Bearing.Insert(i, "'");
      i = Bearing.IndexOf('-');
      Bearing = Bearing.Insert(i, "°");
      Bearing = Bearing.Replace("-", "");
      return Bearing;
    }

    public bool WriteToRegistry(RegistryHive Hive, string Path, string Name, string KeyValue, bool IsFabricLength)
    {
      IMetricUnitConverter pMetricUnitConv =null;
      if(IsFabricLength)
      { 
        UID pUID = new UIDClass();
        pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
        ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);
        pMetricUnitConv = (IMetricUnitConverter)pCadEd;
      }
      RegistryKey objParent = null;
      if (Hive == RegistryHive.ClassesRoot)
        objParent = Registry.ClassesRoot;

      if (Hive == RegistryHive.CurrentConfig)
        objParent = Registry.CurrentConfig;

      if (Hive == RegistryHive.CurrentUser)
        objParent = Registry.CurrentUser;

      if (Hive == RegistryHive.LocalMachine)
        objParent = Registry.LocalMachine;

      if (Hive == RegistryHive.PerformanceData)
        objParent = Registry.PerformanceData;

      if (Hive == RegistryHive.Users)
        objParent = Registry.Users;

      if (objParent != null)
      {
        if (pMetricUnitConv!=null)
        {
          double dVal;
          double dMetric=0;
          if(Double.TryParse(KeyValue, out dVal))
            pMetricUnitConv.ConvertDistance(esriCadastralUnitConversionType.esriCUCToMetric, dVal, ref dMetric);
          KeyValue=dMetric.ToString();
        }

        RegistryKey regKeyAppRoot = objParent.CreateSubKey(Path);
        regKeyAppRoot.SetValue(Name, KeyValue); 
        return true;
      }
      else
        return false;
    }

    public string ReadFromRegistry(RegistryHive Hive, string Key, string ValueName, bool ConvertFromMetric)
    {
      string sAns = "";
      RegistryKey objParent = null;

      IMetricUnitConverter pMetricUnitConv = null;
      if (ConvertFromMetric)
      {
        UID pUID = new UIDClass();
        pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
        ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);
        pMetricUnitConv = (IMetricUnitConverter)pCadEd;
      }

      if (Hive == RegistryHive.ClassesRoot)
        objParent = Registry.ClassesRoot;

      if (Hive == RegistryHive.CurrentConfig)
        objParent = Registry.CurrentConfig;

      if (Hive == RegistryHive.CurrentUser)
        objParent = Registry.CurrentUser;

      if (Hive == RegistryHive.LocalMachine)
        objParent = Registry.LocalMachine;

      if (Hive == RegistryHive.PerformanceData)
        objParent = Registry.PerformanceData;

      if (Hive == RegistryHive.Users)
        objParent = Registry.Users;

      if (objParent != null)
      {
        RegistryKey objSubKey = objParent.OpenSubKey(Key);
        //if it can't be found, object is not initialized
        object xx = null;
        if (objSubKey != null)
          xx = (objSubKey.GetValue(ValueName));
        if(xx != null)
          sAns=xx.ToString();

        if (pMetricUnitConv != null)
        {
          double dVal = 0;
          double dProjUnit=0;
          if(Double.TryParse(sAns,out dVal))
            pMetricUnitConv.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, dVal, ref dProjUnit);
          sAns = dProjUnit.ToString("0.000");
        }
      }
      return sAns;
    }

    public string GetDesktopVersionFromRegistry()
    {
      string sVersion = "";
      try
      {
        string s = ReadFromRegistry(RegistryHive.LocalMachine, "Software\\ESRI\\ArcGIS", "RealVersion",false);
        string[] Values = s.Split('.');
        sVersion = Values[0] + "." + Values[1];
        return sVersion;
      }
      catch (Exception)
      {
        return sVersion;
      }
    }

    private bool ComputeShapeDistortionParameters(IPoint[] FabricPoints, IPoint[] AdjustedTraversePoints, out double RotationInRadians,
      out double Scale, out double ShpStdErrX, out double ShpStdErrY)
    {
      Scale = 1;
      RotationInRadians = 0;
      ShpStdErrX = 0;
      ShpStdErrY = 0;
      IAffineTransformation2D3GEN affineTransformation2D = new AffineTransformation2DClass();
      try { affineTransformation2D.DefineConformalFromControlPoints(ref FabricPoints, ref AdjustedTraversePoints); }
      catch { return false; }

      RotationInRadians = affineTransformation2D.Rotation;
      Scale = affineTransformation2D.XScale;
      Scale = affineTransformation2D.YScale;
      Scale = 1 / Scale; //the inverse of this computed scale is stored

      int iLen = FabricPoints.GetLength(0) * 2;
      double[] inPoints = new double[iLen];
      double[] outPoints = new double[iLen];
      double[] adjTrav = new double[iLen];

      int x = 0;
      int y = 0;
      for (int j = 0; j < FabricPoints.GetLength(0); j++)
      {
        x = j * 2;
        y = x + 1;
        inPoints[x] = FabricPoints[j].X;//x
        inPoints[y] = FabricPoints[j].Y;//y

        adjTrav[x] = AdjustedTraversePoints[j].X;//x
        adjTrav[y] = AdjustedTraversePoints[j].Y;//y
      }

      affineTransformation2D.TransformPointsFF(esriTransformDirection.esriTransformForward, ref inPoints, ref outPoints);


      //now build a list of the diffs for x and y between transformed and the AdjustedTraverse Results

      int iLen2 = FabricPoints.GetLength(0);
      double[] errX = new double[iLen2];
      double[] errY = new double[iLen2];
      x = 0;
      y = 0;
      double dSUMX = 0;
      double dSUMY = 0;

      for (int j = 0; j < iLen2; j++)
      {
        x = j * 2;
        y = x + 1;
        errX[j] = adjTrav[x] - outPoints[x] + 100000;//x
        errY[j] = adjTrav[y] - outPoints[y] + 100000;//y
        dSUMX += errX[j];
        dSUMY += errY[j];
      }
      double dMean = 0; double dStdDevX = 0; double dStdDevY = 0; double dRange; int iOutliers = 0;
      GetStatistics(errX, dSUMX, 1, out dMean, out dStdDevX, out dRange, out iOutliers);
      GetStatistics(errY, dSUMY, 1, out dMean, out dStdDevY, out dRange, out iOutliers);
      ShpStdErrX = dStdDevX;
      ShpStdErrY = dStdDevY;

      return true;
    }

    public double InverseDistanceByGroundToGrid(ISpatialReference SpatRef, IPoint FromPoint, IPoint ToPoint, double EllipsoidalHeight)
    {

      bool bSpatialRefInGCS = !(SpatRef is IProjectedCoordinateSystem2);

      IZAware pZAw = (IZAware)FromPoint;
      pZAw.ZAware = true;
      FromPoint.Z = EllipsoidalHeight;

      pZAw = (IZAware)ToPoint;
      pZAw.ZAware = true;
      ToPoint.Z = EllipsoidalHeight;

      ICadastralGroundToGridTools pG2G = new CadastralDataToolsClass();

      double dDist1 = pG2G.Inverse3D(SpatRef, false, FromPoint, ToPoint);
      double dDist2 = pG2G.Inverse3D(SpatRef, true, FromPoint, ToPoint); //use true if in GCS

      if (bSpatialRefInGCS)
        dDist1 = dDist2;

      return dDist1;
    }
    public void GetStatistics(double[] InDoubleArray, double InSum, int InSigma1or2or3,
  out double Mean, out double StandardDeviation, out double Range, out int NumberOfOutliers)
    {//SUM is assumed to be readily computed during construction of the array, and avoids another loop here
      Mean = InSum / InDoubleArray.Length;
      double SumSquares = 0;
      double Smallest = 0;
      double Largest = 0;
      NumberOfOutliers = 0;
      for (int i = 0; i < InDoubleArray.Length; i++)
      {
        if (i == 0)
        {
          Smallest = InDoubleArray[i];
          Largest = Smallest;
        }
        else
        {
          if (InDoubleArray[i] > Largest)
            Largest = InDoubleArray[i];
          if (InDoubleArray[i] < Smallest)
            Smallest = InDoubleArray[i];
        }

        double d = InDoubleArray[i] - Mean;
        d = d * d;
        SumSquares += d;
      }

      StandardDeviation = Math.Sqrt(SumSquares / InDoubleArray.Length);
      Range = Largest - Smallest;
      //look for and count outliers within 1, 2 or 3 sigma
      if (InSigma1or2or3 <= 0 || InSigma1or2or3 > 3)
        InSigma1or2or3 = 3;
      double TestValue = InSigma1or2or3 * StandardDeviation;
      for (int i = 0; i < InDoubleArray.Length; i++)
      {
        if ((Math.Abs(InDoubleArray[i] - Mean)) > (TestValue))
          NumberOfOutliers++;
      }

    }

    public string UnitNameFromSpatialReference(ISpatialReference SpatialRef)
    {
      string sUnit = "Meters";
      if (SpatialRef == null)
        return sUnit;

      if (SpatialRef is IProjectedCoordinateSystem2)
      {
        IProjectedCoordinateSystem2 pPCS = (IProjectedCoordinateSystem2)SpatialRef;
        ILinearUnit pMapLU = pPCS.CoordinateUnit;
        sUnit=pMapLU.Name;
        sUnit=sUnit.Replace("Foot","feet");
        sUnit = sUnit.Replace("Meter", "meters");
      }

      return sUnit;
    }

    public void AddCommandToApplicationMenu(IApplication TheApplication, string AddThisCommandGUID,
  string ToThisMenuGUID, bool StartGroup, string PositionAfterThisCommandGUID, bool EndGroup)
    {
      if (AddThisCommandGUID.Trim() == "")
        return;

      if (ToThisMenuGUID.Trim() == "")
        return;

      //Then add items to the command bar:
      UID pCommandUID = new UIDClass(); // this references an ICommand 
      pCommandUID.Value = AddThisCommandGUID; // the ICommand to add

      //Assign the UID for the menu we want to add the custom button to
      UID MenuUID = new UIDClass();
      MenuUID.Value = ToThisMenuGUID;
      //Get the target menu as a ICommandBar
      ICommandBar pCmdBar = TheApplication.Document.CommandBars.Find(MenuUID) as ICommandBar;

      int iPos = pCmdBar.Count;

      //for the position string, if it's an empty string then default to end of menu.
      if (PositionAfterThisCommandGUID.Trim() != "")
      {
        UID pPositionCommandUID = new UIDClass();
        pPositionCommandUID.Value = PositionAfterThisCommandGUID;
        iPos = pCmdBar.Find(pPositionCommandUID).Index + 1;
        if (pPositionCommandUID != null)
          Marshal.ReleaseComObject(pPositionCommandUID);
      }

      ICommandItem pCmdItem = pCmdBar.Find(pCommandUID);

      //Check if it is already present on the context menu...
      if (pCmdItem == null)
      {
        pCmdItem = pCmdBar.Add(pCommandUID, iPos);
        pCmdItem.Group = StartGroup;
        pCmdItem.Refresh();
      }

      //if (EndGroup)
      //{
      //  if (pCmdBar.Count > iPos)
      //  { pCmdItem=pCmdBar.Find("")}
      //}

      if (pCommandUID != null)
        Marshal.ReleaseComObject(pCommandUID);
      if (MenuUID != null)
        Marshal.ReleaseComObject(MenuUID);

    }

    public void AddSpatialBookMark(IApplication application)
    {
      IMapDocument mapDoc = application.Document as IMapDocument;
      IMap map = mapDoc.ActiveView.FocusMap;
      IActiveView activeView = map as IActiveView;

      // Create a new bookmark and set its location to the focus map's current extent.
      IAOIBookmark areaOfInterest = new AOIBookmarkClass();
      areaOfInterest.Location = activeView.Extent;

      // Give the bookmark a name.
      areaOfInterest.Name = "Area of Interest Bookmark";

      // Add the bookmark to the map's bookmark collection. This adds the bookmark 
      // to the Bookmarks menu, which is accessible from the View menu.
      IMapBookmarks mapBookmarks = map as IMapBookmarks;
      mapBookmarks.AddBookmark(areaOfInterest);
    }

    private bool GetFabricSubLayersFromFabric(IMap Map, ICadastralFabric Fabric, out IFeatureLayer CFPointLayer, out IFeatureLayer CFLineLayer,
      out IArray CFParcelLayers, out IFeatureLayer CFControlLayer, out IFeatureLayer CFLinePointLayer)
    {
      ICadastralFabricLayer pCFLayer = null;
      ICadastralFabricSubLayer pCFSubLyr = null;
      ICompositeLayer pCompLyr = null;
      IArray CFParcelLayers2 = new ArrayClass();

      IDataset pDS = (IDataset)Fabric;
      IName pDSName = pDS.FullName;
      string FabricNameString = pDSName.NameString;

      long layerCount = Map.LayerCount;
      CFPointLayer = null; CFLineLayer = null; CFControlLayer = null; CFLinePointLayer = null;
      IFeatureLayer pParcelLayer = null;
      for (int idx = 0; idx <= (layerCount - 1); idx++)
      {
        ILayer pLayer = Map.get_Layer(idx);
        bool bIsComposite = false;
        if (pLayer is ICompositeLayer)
        {
          pCompLyr = (ICompositeLayer)pLayer;
          bIsComposite = true;
        }

        int iCompositeLyrCnt = 1;
        if (bIsComposite)
          iCompositeLyrCnt = pCompLyr.Count;

        for (int i = 0; i <= (iCompositeLyrCnt - 1); i++)
        {
          if (bIsComposite)
            pLayer = pCompLyr.get_Layer(i);
          if (pLayer is ICadastralFabricLayer)
          {
            pCFLayer = (ICadastralFabricLayer)pLayer;
            break;
          }
          if (pLayer is ICadastralFabricSubLayer)
          {
            pCFSubLyr = (ICadastralFabricSubLayer)pLayer;
            IDataset pDS2 = (IDataset)pCFSubLyr.CadastralFabric;
            IName pDSName2 = pDS2.FullName;
            if (pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTParcels)
            {
              pParcelLayer = (IFeatureLayer)pCFSubLyr;
              CFParcelLayers2.Add(pParcelLayer);
            }
            if (CFLineLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLines)
              CFLineLayer = (IFeatureLayer)pCFSubLyr;
            if (CFPointLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTPoints)
              CFPointLayer = (IFeatureLayer)pCFSubLyr;
            if (CFLinePointLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLinePoints)
              CFLinePointLayer = (IFeatureLayer)pCFSubLyr;
            if (CFControlLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTControl)
              CFControlLayer = (IFeatureLayer)pCFSubLyr;
          }
        }

        //Check that the fabric layer belongs to the requested fabric
        if (pCFLayer != null)
        {
          if (pCFLayer.CadastralFabric.Equals(Fabric))
          {
            CFPointLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRPoints);
            CFLineLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLines);
            pParcelLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRParcels);
            CFParcelLayers2.Add(pParcelLayer);
            CFControlLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRControlPoints);
            CFLinePointLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLinePoints);
          }
          CFParcelLayers = CFParcelLayers2;
          return true;
        }
      }
      //at the minimum, just need to make sure we have a parcel sublayer for the requested fabric
      if (pParcelLayer != null)
      {
        CFParcelLayers = CFParcelLayers2;
        return true;
      }
      else
      {
        CFParcelLayers = null;
        return false;
      }
    }

    private void RefreshMap(IActiveView ActiveView, IArray ParcelLayers, IFeatureLayer PointLayer,
  IFeatureLayer LineLayer, IFeatureLayer ControlLayer, IFeatureLayer LinePointLayer)
    {
      try
      {
        for (int z = 0; z <= ParcelLayers.Count - 1; z++)
        {
          if (ParcelLayers.get_Element(z) != null)
          {
            IFeatureSelection pFeatSel = (IFeatureSelection)ParcelLayers.get_Element(z);
            pFeatSel.Clear();//refreshes the parcel explorer
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, ParcelLayers.get_Element(z), ActiveView.Extent);
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ParcelLayers.get_Element(z), ActiveView.Extent);
          }
        }
        if (PointLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, PointLayer, ActiveView.Extent);
        if (LineLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, LineLayer, ActiveView.Extent);
        if (ControlLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ControlLayer, ActiveView.Extent);
        if (LinePointLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, LinePointLayer, ActiveView.Extent);
      }
      catch
      { }
    }

    public bool GetFabricSubLayers(IMap Map, esriCadastralFabricTable FabricSubClass, out IArray CFParcelFabSubLayers)
    {
      ICadastralFabricSubLayer pCFSubLyr = null;
      IArray CFParcelFabricSubLayers2 = new ArrayClass();
      IFeatureLayer pParcelFabricSubLayer = null;
      UID pId = new UIDClass();
      pId.Value = "{E156D7E5-22AF-11D3-9F99-00C04F6BC78E}";
      IEnumLayer pEnumLayer = Map.get_Layers(pId, true);
      pEnumLayer.Reset();
      ILayer pLayer = pEnumLayer.Next();
      while (pLayer != null)
      {
        if (pLayer is ICadastralFabricSubLayer)
        {
          pCFSubLyr = (ICadastralFabricSubLayer)pLayer;
          if (pCFSubLyr.CadastralTableType == FabricSubClass)
          {
            pParcelFabricSubLayer = (IFeatureLayer)pCFSubLyr;
            CFParcelFabricSubLayers2.Add(pParcelFabricSubLayer);
          }
        }
        pLayer = pEnumLayer.Next();
      }
      CFParcelFabSubLayers = CFParcelFabricSubLayers2;
      if (CFParcelFabricSubLayers2.Count > 0)
        return true;
      else
        return false;
    }

    public bool GetFabricFromMap(IMap InMap, out ICadastralFabric Fabric)
    {//this code assumes only one fabric in the map, and will get the first that it finds.
      //Used when not in an edit session. TODO: THis could return an array of fabrics
      Fabric = null;
      for (int idx = 0; idx <= (InMap.LayerCount - 1); idx++)
      {
        ILayer pLayer = InMap.get_Layer(idx);
        Fabric = GetFabricFromLayer(pLayer);
        if (Fabric != null)
          return true;
      }
      return false;
    }

    private ICadastralFabric GetFabricFromLayer(ILayer Layer)
    { //interogates a layer and returns it's source fabric if it is a fabric layer
      ICadastralFabric Fabric = null;
      ICompositeLayer pCompLyr = null;
      ICadastralFabricSubLayer pCFSubLyr = null;
      ICadastralFabricLayer pCFLayer = null;
      bool bIsComposite = false;

      if (Layer is ICompositeLayer)
      {
        pCompLyr = (ICompositeLayer)Layer;
        bIsComposite = true;
      }

      int iCount = 1;
      if (bIsComposite)
        iCount = pCompLyr.Count;

      for (int i = 0; i <= (iCount - 1); i++)
      {
        if (bIsComposite)
          Layer = pCompLyr.get_Layer(i);
        try
        {
          if (pCFLayer is ICadastralFabricLayer)
          {
            pCFLayer = (ICadastralFabricLayer)Layer;
            Fabric = pCFLayer.CadastralFabric;
            return Fabric;
          }
          else
          {
            pCFSubLyr = (ICadastralFabricSubLayer)Layer;
            Fabric = pCFSubLyr.CadastralFabric;
            return Fabric;
          }
        }
        catch
        {
          continue;//cast failed...not a fabric sublayer
        }
      }
      return Fabric;
    }
    
    public void FIDsetToLongArray(IFIDSet InFIDSet, ref ILongArray OutLongArray, ref int[] OutIntArray, IStepProgressor StepProgressor)
    {
      Int32 pfID = -1;
      InFIDSet.Reset();
      double dMax = InFIDSet.Count();
      int iMax = (int)(dMax);
      for (Int32 pCnt = 0; pCnt <= (InFIDSet.Count() - 1); pCnt++)
      {
        InFIDSet.Next(out pfID);
        OutLongArray.Add(pfID);
        OutIntArray[pCnt] = pfID;
        if (StepProgressor != null)
        {
          if (StepProgressor.Position < StepProgressor.MaxRange)
            StepProgressor.Step();
        }
      }
      return;
    }

    public void RefreshFabricLayers(IMap Map, ICadastralFabric Fabric)
    {
    IArray CFParcelLyrs;
    IFeatureLayer CFPtLyr;
    IFeatureLayer CFLineLyr;
    IFeatureLayer CFCtrlLyr;
    IFeatureLayer CFLinePtLyr;

    if (!GetFabricSubLayersFromFabric(Map,Fabric,out CFPtLyr, out CFLineLyr,
     out CFParcelLyrs, out CFCtrlLyr, out CFLinePtLyr))
      return;
    else
      RefreshMap(ArcMap.Document.ActiveView,CFParcelLyrs,CFPtLyr,CFLineLyr,CFCtrlLyr,CFLinePtLyr);
    }

    public List<string> InClauseFromOIDsList(List<int> ListOfOids, int TokenMax)
    {
      List<string> InClause = new List<string>();
      int iCnt = 0;
      int iIdx = 0;
      InClause.Add("");
      foreach (int i in ListOfOids)
      {
        if (iCnt == TokenMax)
        {
          InClause.Add("");
          iCnt = 0;
          iIdx++;
        }
        if (InClause[iIdx].Trim() == "")
          InClause[iIdx] = i.ToString();
        else
          InClause[iIdx] += "," + i.ToString();
        iCnt++;
      }
      return InClause;
    }

    public bool HasParallelCurveMatchFeatures(IFeatureClass FeatureClass, IPolycurve inPolycurve, string WhereClause,
      double AngleToleranceTangentCompareInDegrees, double OrthogonalSearchDistance,
       out int outFoundLinesCount, out int outFoundParallelCurvesCount, ref List<string> CurveInfoFromNeighbours)
    {
      outFoundLinesCount = 0;
      outFoundParallelCurvesCount = 0;

      ILine pOriginalChord = new Line();
      pOriginalChord.PutCoords(inPolycurve.FromPoint, inPolycurve.ToPoint);
      IVector3D vecOriginalSelected = new Vector3DClass();
      vecOriginalSelected.PolarSet(pOriginalChord.Angle, 0, 1);

      int idxRadius = FeatureClass.FindField("RADIUS");
      if (idxRadius == -1)
        return false;

      int idxCenterPointID = FeatureClass.FindField("CENTERPOINTID");
      if (idxCenterPointID == -1)
        return false;

      object val = null;

      IGeometryBag pGeomBag = new GeometryBagClass();
      IGeometryCollection pGeomColl = (IGeometryCollection)pGeomBag;

      IGeometry MultiPartPolyLine = new PolylineClass(); //qi
      IGeoDataset pGeoDS = (IGeoDataset)FeatureClass;
      ISpatialReference spatialRef = pGeoDS.SpatialReference;
      MultiPartPolyLine.SpatialReference = spatialRef;

      IGeometryCollection geometryCollection2 = MultiPartPolyLine as IGeometryCollection;

      ILine pNormalLine = new Line(); //new
      for (int i = -1; i < 2; i = i + 2)
      {
        double dOffset = OrthogonalSearchDistance * i;

        inPolycurve.QueryNormal(esriSegmentExtension.esriNoExtension, 0.5, true, dOffset, pNormalLine);
        ILine pThisLine = new Line();

        pThisLine.PutCoords(pNormalLine.FromPoint, pNormalLine.ToPoint);
        pGeomColl.AddGeometry(pThisLine);

        //Although each line is connected to the other, create a new path for each line 
        //this allows for future changes in case the input needs to be altered to separate paths.

        ISegmentCollection newPath = new PathClass();
        object obj = Type.Missing;
        newPath.AddSegment((ISegment)pThisLine, ref obj, ref obj);
        //The spatial reference associated with geometryCollection will be assigned to all incoming paths and segments.
        geometryCollection2.AddGeometry(newPath as IGeometry, ref obj, ref obj);
      }

      ISpatialFilter pSpatFilt = new SpatialFilter();
      pSpatFilt.WhereClause = WhereClause;
      pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      pSpatFilt.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;

      pSpatFilt.Geometry = pGeomBag;

      IFeatureCursor pFeatCursLines = null;
      try
      {
        pFeatCursLines = FeatureClass.Search(pSpatFilt, false);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return false;
      }

      IFeature pFeat = pFeatCursLines.NextFeature();
      while (pFeat != null)
      {
        IGeometry pFoundLineGeom = pFeat.ShapeCopy;

        //if the feature has no radius attribute, skip.
        double dRadius = 0;
        int iCtrPoint = -1;
        val = pFeat.get_Value(idxRadius);
        if (val == DBNull.Value)
          dRadius = 0;
        else
          dRadius = (double)val;

        if (dRadius == 0)
        {//null or zero radius so skip.
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }

        val = pFeat.get_Value(idxCenterPointID);
        if (val == DBNull.Value)
        {//null centrpointID so skip.
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }

        iCtrPoint = (int)val;

        ITopologicalOperator6 pTopoOp6 = (ITopologicalOperator6)MultiPartPolyLine;
        IGeometry pResultGeom = pTopoOp6.IntersectEx(pFoundLineGeom, false, esriGeometryDimension.esriGeometry0Dimension);
        if (pResultGeom == null)
        {
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }
        if (pResultGeom.IsEmpty)
        {
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }

        ISegmentCollection pFoundLineGeomSegs = pFoundLineGeom as ISegmentCollection;
        bool bHasCurves = false;
        pFoundLineGeomSegs.HasNonLinearSegments(ref bHasCurves);
        if (!bHasCurves)
        {
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }

        IPointCollection5 PtColl = (IPointCollection5)pResultGeom;

        if (PtColl.PointCount > 1)
        {
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }
        IPolycurve pPolyCurve4Tangent = pFoundLineGeom as IPolycurve;

        for (int j = 0; j < PtColl.PointCount; j++)
        {
          IPoint p = PtColl.get_Point(j);
          IPoint outPoint = new Point();
          double dDistanceAlong = 0;
          double dDistanceFromCurve = 0;
          bool bOffsetRight = true;

          //work out if the point is to the left or right of the original 
          inPolycurve.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, p, false, outPoint,
            ref dDistanceAlong, ref dDistanceFromCurve, ref bOffsetRight);

          ILine pTangent = new Line();
          dDistanceAlong = 0;
          dDistanceFromCurve = 0;
          bool bOnRight = true;

          pPolyCurve4Tangent.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, p, false, outPoint,
            ref dDistanceAlong, ref dDistanceFromCurve, ref bOnRight);
          pPolyCurve4Tangent.QueryTangent(esriSegmentExtension.esriNoExtension, dDistanceAlong, false, 100, pTangent);

          //compare the tangent bearing with the normal to check for orthogonality
          IVector3D vecTangent = new Vector3DClass();
          vecTangent.PolarSet(pTangent.Angle, 0, 1);

          IVector3D vecNormal = new Vector3DClass();
          vecNormal.PolarSet(pNormalLine.Angle, 0, 1);

          ILine pHitDistanceForRadiusDifference = new Line();
          pHitDistanceForRadiusDifference.PutCoords(pNormalLine.FromPoint, outPoint);
          double dRadiusDiff = pHitDistanceForRadiusDifference.Length;

          double dDotProd = vecTangent.DotProduct(vecNormal);
          double dAngleCheck = Math.Acos(dDotProd) * 180 / Math.PI; //in degrees
          dAngleCheck = Math.Abs(dAngleCheck - 90);

          if (dAngleCheck < AngleToleranceTangentCompareInDegrees)
          {
            //work out concavity orientation with respect to the original line using the radius sign and dot product
            dDotProd = vecOriginalSelected.DotProduct(vecTangent);
            double dTangentCheck = Math.Acos(dDotProd) * 180 / Math.PI; // in degrees
            //dTangentCheck at this point should be close to 0 or 180 degrees.
            outFoundLinesCount++;

            bool bIsConvex = ((dTangentCheck < 90 && dRadius < 0 && !bOffsetRight) ||
                              (dTangentCheck > 90 && dRadius > 0 && !bOffsetRight) ||
                              (dTangentCheck < 90 && dRadius > 0 && bOffsetRight) ||
                              (dTangentCheck > 90 && dRadius < 0 && bOffsetRight));

            double dUnitSignChange = 1;

            if (!bIsConvex)
              dUnitSignChange = -1;

            double dDerivedRadius = (Math.Abs(dRadius)) + dRadiusDiff * dUnitSignChange;

            dUnitSignChange = 1;
            //now compute inferred left/right for candidate
            if (bIsConvex && !bOffsetRight)
              dUnitSignChange = -1;

            if (!bIsConvex && bOffsetRight)
              dUnitSignChange = -1;

            dDerivedRadius = dDerivedRadius * dUnitSignChange;

            string sHarvestedCurveInfo = pFeat.OID.ToString() + "," + dDerivedRadius.ToString("#.000") + "," +
              iCtrPoint.ToString() + "," + dRadiusDiff.ToString("#.000");
            CurveInfoFromNeighbours.Add(sHarvestedCurveInfo);
          }
        }

        Marshal.ReleaseComObject(pFeat);
        pFeat = pFeatCursLines.NextFeature();
      }
      Marshal.FinalReleaseComObject(pFeatCursLines);

      bool bHasParallelCurveFeaturesNearby = (outFoundLinesCount > 0);

      return bHasParallelCurveFeaturesNearby;
    }

    public bool HasTangentCurveMatchFeatures(IFeatureClass FeatureClass, IPolycurve inPolycurve, string WhereClause,
  double AngleToleranceTangentCompareInDegrees, double StraightLinesBreakLessThanInDegrees, double MaximumDeltaInDegrees, double ExcludeTangentsShorterThan, 
      out int outFoundTangentCurvesCount, ref List<string> CurveInfoFromNeighbours)
    {
      outFoundTangentCurvesCount = 0;

      ILine pOriginalChord = new Line();
      pOriginalChord.PutCoords(inPolycurve.FromPoint, inPolycurve.ToPoint);
      IVector3D vecOriginalSelected = new Vector3DClass();
      vecOriginalSelected.PolarSet(pOriginalChord.Angle, 0, 1);

      int idxRadius = FeatureClass.FindField("RADIUS");
      if (idxRadius == -1)
        return false;

      int idxCenterPointID = FeatureClass.FindField("CENTERPOINTID");
      if (idxCenterPointID == -1)
        return false;

      object val = null;

      IGeometryBag pGeomBag = new GeometryBagClass();
      IGeometryCollection pGeomColl = (IGeometryCollection)pGeomBag;

      IGeometry MultiPartPolyLine = new PolylineClass(); //qi
      IGeoDataset pGeoDS = (IGeoDataset)FeatureClass;
      ISpatialReference spatialRef = pGeoDS.SpatialReference;
      MultiPartPolyLine.SpatialReference = spatialRef;

      IGeometryCollection geometryCollection2 = MultiPartPolyLine as IGeometryCollection;

      ILine pTangentLineAtEnd = new Line(); //new
      ILine pTangentLineAtStart = new Line(); //new
      object objMissing = Type.Missing;

      for (int i = 0; i < 2; i++)
      {
        ILine pThisLine = null;
        if (i == 0)
        {
          inPolycurve.QueryTangent(esriSegmentExtension.esriExtendAtTo, 1.0, true, 0.2, pTangentLineAtEnd);
          pThisLine = new Line();
          pThisLine.PutCoords(pTangentLineAtEnd.FromPoint, pTangentLineAtEnd.ToPoint);
          pGeomColl.AddGeometry(pThisLine);
        }
        else
        {
          inPolycurve.QueryTangent(esriSegmentExtension.esriExtendAtFrom, 0.0, true, 0.2, pTangentLineAtStart);
          pThisLine = new Line();
          pThisLine.PutCoords(pTangentLineAtStart.FromPoint, pTangentLineAtStart.ToPoint);
          pGeomColl.AddGeometry(pThisLine);
        }
        //Create a new path for each line.

        ISegmentCollection newPath = new PathClass();
        newPath.AddSegment((ISegment)pThisLine, ref objMissing, ref objMissing);
        //The spatial reference associated with geometryCollection will be assigned to all incoming paths and segments.
        geometryCollection2.AddGeometry(newPath as IGeometry, ref objMissing, ref objMissing);
      }

      //now buffer the lines
      IGeometryCollection outBufferedGeometryCol = new GeometryBagClass();
      for (int jj = 0; jj < geometryCollection2.GeometryCount; jj++)
      {
        IPath pPath = geometryCollection2.get_Geometry(jj) as IPath;
        IGeometryCollection pPolyL = new PolylineClass();
        pPolyL.AddGeometry((IGeometry)pPath);

        ITopologicalOperator topologicalOperator = (ITopologicalOperator)pPolyL;
        IPolygon pBuffer = topologicalOperator.Buffer(0.1) as IPolygon;
        outBufferedGeometryCol.AddGeometry(pBuffer, ref objMissing, ref objMissing);
      }
      ITopologicalOperator pUnionedBuffers = null;
      pUnionedBuffers = new PolygonClass() as ITopologicalOperator;
      pUnionedBuffers.ConstructUnion((IEnumGeometry)outBufferedGeometryCol);

      ISpatialFilter pSpatFilt = new SpatialFilter();
      pSpatFilt.WhereClause = WhereClause;
      pSpatFilt.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      pSpatFilt.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;

      pSpatFilt.Geometry = (IGeometry)pUnionedBuffers;

      IFeatureCursor pFeatCursLines = null;
      try
      {
        pFeatCursLines = FeatureClass.Search(pSpatFilt, false);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return false;
      }
      IVector3D vecFoundGeom = new Vector3DClass();
      IFeature pFeat = pFeatCursLines.NextFeature();
      bool bHasTangentStraightLineAtJunction = false;
      List<int> lstLargeBreak = new List<int>();

      while (pFeat != null)
      {
        IGeometry pFoundLineGeom = pFeat.ShapeCopy;
        IPolycurve pFoundLineAsPolyCurve = pFoundLineGeom as IPolycurve;
        int iRelativeOrientation = GetRelativeOrientation(pFoundLineAsPolyCurve, inPolycurve);
        //iRelativeOrientation == 1 --> closest points are original TO and found TO
        //iRelativeOrientation == 2 --> closest points are original TO and found FROM
        //iRelativeOrientation == 3 --> closest points are original FROM and found TO
        //iRelativeOrientation == 4 --> closest points are original FROM and found FROM

        //if the feature has no radius attribute, skip.
        double dRadius = 0;
        int iCtrPoint = -1;
        val = pFeat.get_Value(idxRadius);
        if (val == DBNull.Value)
          dRadius = 0;
        else
          dRadius = (double)val;
        
        val = pFeat.get_Value(idxCenterPointID);

        IPolycurve pPolyCurve = pFoundLineGeom as IPolycurve;

        ILine pFoundChordCandidate = new LineClass();
        pFoundChordCandidate.PutCoords(pPolyCurve.FromPoint, pPolyCurve.ToPoint);
        //first check for liklihood that subject line is supposed to stay straight, by
        //geometry chord bearing angle break test
        vecFoundGeom.PolarSet(pFoundChordCandidate.Angle, 0, 1);
        double dDotProd = vecFoundGeom.DotProduct(vecOriginalSelected);
        double dAngleCheck = Math.Acos(dDotProd) * 180 / Math.PI; //in degrees
        dAngleCheck = Math.Abs(dAngleCheck);
        double dLargeAngleBreakInDegrees = 3;
        if (dAngleCheck > dLargeAngleBreakInDegrees && dAngleCheck < (180 - dLargeAngleBreakInDegrees)) //large angle break non-tangent, greater than 3 degrees
        {
          if (!lstLargeBreak.Contains(iRelativeOrientation))
            lstLargeBreak.Add(iRelativeOrientation);
        }

        if ((dAngleCheck <= StraightLinesBreakLessThanInDegrees || (180 - dAngleCheck) < StraightLinesBreakLessThanInDegrees) 
            && val == DBNull.Value && dRadius == 0 && !(pPolyCurve.Length< ExcludeTangentsShorterThan))
        {
          if (lstLargeBreak.Contains(iRelativeOrientation))
            bHasTangentStraightLineAtJunction = true;
        }        
        
        if (val == DBNull.Value || dRadius == 0 || pPolyCurve.Length< ExcludeTangentsShorterThan)
        {//if the feature has a null centrpointID then skip.
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }

        if (Math.Abs(inPolycurve.Length / dRadius * 180 / Math.PI) > MaximumDeltaInDegrees)
        {
          //if the resulting curve would have a central angle more than MaximumDeltaInDegrees degrees then skip
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }

        iCtrPoint = (int)val;

        //if geometry of curve neighbour curves have been cracked then there can be more than one segment
        //however since all segments would be circular arcs, just need to test the first segment
        ISegmentCollection pFoundLineGeomSegs = pFoundLineGeom as ISegmentCollection;        
        ISegment pSeg = pFoundLineGeomSegs.get_Segment(0);
        if (!(pSeg is ICircularArc))
        {
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursLines.NextFeature();
          continue;
        }

        dRadius = (double)pFeat.get_Value(idxRadius);

        IVector3D vect1 = new Vector3DClass();
        IVector3D vect2 = new Vector3DClass();
        ILine tang = new Line();
        double dUnitSignChange = 1;
        if (iRelativeOrientation == 1) //closest points are original TO and found TO
        {
          dUnitSignChange = -1;
          vect1.PolarSet(pTangentLineAtEnd.Angle, 0, 1);
          pFoundLineAsPolyCurve.QueryTangent(esriSegmentExtension.esriExtendAtTo, 1.0, true, 1, tang);
          vect2.PolarSet(tang.Angle, 0, 1);
        }
        else if (iRelativeOrientation == 2)//closest points are original TO and found FROM
        {
          vect1.PolarSet(pTangentLineAtEnd.Angle, 0, 1);
          pFoundLineAsPolyCurve.QueryTangent(esriSegmentExtension.esriExtendAtFrom, 0.0, true, 1, tang);
          vect2.PolarSet(tang.Angle, 0, 1);
        }
        else if (iRelativeOrientation == 3)//closest points are original FROM and found TO
        {
          vect1.PolarSet(pTangentLineAtStart.Angle, 0, 1);
          pFoundLineAsPolyCurve.QueryTangent(esriSegmentExtension.esriExtendAtTo, 1.0, true, 1, tang);
          vect2.PolarSet(tang.Angle, 0, 1);
        }
        else if (iRelativeOrientation == 4)//closest points are original FROM and found FROM
        {
          dUnitSignChange = -1;
          vect1.PolarSet(pTangentLineAtStart.Angle, 0, 1);
          pFoundLineAsPolyCurve.QueryTangent(esriSegmentExtension.esriExtendAtFrom, 0.0, true, 1, tang);
          vect2.PolarSet(tang.Angle, 0, 1);
        }

        dDotProd = vect1.DotProduct(vect2);
        dAngleCheck = Math.Acos(dDotProd) * 180 / Math.PI; //in degrees
        dAngleCheck = Math.Abs(dAngleCheck);
        if (dAngleCheck < AngleToleranceTangentCompareInDegrees || (180 - dAngleCheck) < AngleToleranceTangentCompareInDegrees)
        {

          double dDerivedRadius = dRadius * dUnitSignChange;

          string sHarvestedCurveInfo = pFeat.OID.ToString() + "," + dDerivedRadius.ToString("#.000") + "," +
            iCtrPoint.ToString() + "," + "t";
          CurveInfoFromNeighbours.Add(sHarvestedCurveInfo);

          outFoundTangentCurvesCount++;
        }

        Marshal.ReleaseComObject(pFeat);
        pFeat = pFeatCursLines.NextFeature();
      }
      Marshal.FinalReleaseComObject(pFeatCursLines);

      if (bHasTangentStraightLineAtJunction)
        return false; //open to logic change to be less conservative

      bool bHasParallelCurveFeaturesNearby = (outFoundTangentCurvesCount > 0);

      return bHasParallelCurveFeaturesNearby;
    }

    private int GetRelativeOrientation(IPolycurve pFoundLineAsPolyCurve, IPolycurve inPolycurve)
    {
      //iRelativeOrientation == 1 --> closest points are original TO and found TO
      //iRelativeOrientation == 2 --> closest points are original TO and found FROM
      //iRelativeOrientation == 3 --> closest points are original FROM and found TO
      //iRelativeOrientation == 4 --> closest points are original FROM and found FROM

        Dictionary<int, double> dictSort2GetShortest = new Dictionary<int, double>();

        ILine pFoundTo_2_OriginalTo = new Line();
        pFoundTo_2_OriginalTo.PutCoords(pFoundLineAsPolyCurve.ToPoint, inPolycurve.ToPoint);
        dictSort2GetShortest.Add(1, pFoundTo_2_OriginalTo.Length);

        ILine pFoundFrom_2_OriginalTo = new Line();
        pFoundFrom_2_OriginalTo.PutCoords(pFoundLineAsPolyCurve.FromPoint, inPolycurve.ToPoint);
        dictSort2GetShortest.Add(2, pFoundFrom_2_OriginalTo.Length);

        ILine pFoundTo_2_OriginalFrom = new Line();
        pFoundTo_2_OriginalFrom.PutCoords(pFoundLineAsPolyCurve.ToPoint, inPolycurve.FromPoint);
        dictSort2GetShortest.Add(3, pFoundTo_2_OriginalFrom.Length);

        ILine pFoundFrom_2_OriginalFrom = new Line();
        pFoundFrom_2_OriginalFrom.PutCoords(pFoundLineAsPolyCurve.FromPoint, inPolycurve.FromPoint);
        dictSort2GetShortest.Add(4, pFoundFrom_2_OriginalFrom.Length);

        var sortedDict = from entry in dictSort2GetShortest orderby entry.Value ascending select entry;
        var pEnum = sortedDict.GetEnumerator();
        pEnum.MoveNext(); //get the first key for the shortest line
        var pair = pEnum.Current;
        int iOpt = pair.Key;
        return iOpt;
    }

    public bool SetupEditEnvironment(IWorkspace TheWorkspace, ICadastralFabric TheFabric, IEditor TheEditor,
      out bool IsFileBasedGDB, out bool IsUnVersioned, out bool UseNonVersionedEdit)
    {
      IsFileBasedGDB = false;
      IsUnVersioned = false;
      UseNonVersionedEdit = false;

      ITable pTable = TheFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);

      IsFileBasedGDB = (!(TheWorkspace.WorkspaceFactory.WorkspaceType == esriWorkspaceType.esriRemoteDatabaseWorkspace));

      if (!(IsFileBasedGDB))
      {
        IVersionedObject pVersObj = (IVersionedObject)pTable;
        IsUnVersioned = (!(pVersObj.IsRegisteredAsVersioned));
        pTable = null;
        pVersObj = null;
      }
      if (IsUnVersioned && !IsFileBasedGDB)
      {//
        DialogResult dlgRes = MessageBox.Show("Fabric is not registered as versioned." +
          "\r\n You will not be able to undo." +
          "\r\n Click 'OK' to delete permanently.",
          "Continue with delete?", MessageBoxButtons.OKCancel);
        if (dlgRes == DialogResult.OK)
        {
          UseNonVersionedEdit = true;
        }
        else if (dlgRes == DialogResult.Cancel)
        {
          return false;
        }
        //MessageBox.Show("The fabric tables are non-versioned." +
        //   "\r\n Please register as versioned, and try again.");
        //return false;
      }
      else if ((TheEditor.EditState == esriEditState.esriStateNotEditing))
      {
        MessageBox.Show("Please start editing first and try again.", "Delete",
          MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
      }
      return true;
    }

    public bool StartEditing(IWorkspace TheWorkspace, bool IsUnversioned)   // Start EditSession + create EditOperation
    {
      bool IsFileBasedGDB =
        (!(TheWorkspace.WorkspaceFactory.WorkspaceType == esriWorkspaceType.esriRemoteDatabaseWorkspace));

      IWorkspaceEdit pWSEdit = (IWorkspaceEdit)TheWorkspace;
      if (pWSEdit.IsBeingEdited())
      {
        MessageBox.Show("The workspace is being edited by another process.");
        return false;
      }

      if (!IsFileBasedGDB)
      {
        IMultiuserWorkspaceEdit pMUWSEdit = (IMultiuserWorkspaceEdit)TheWorkspace;
        try
        {
          if (pMUWSEdit.SupportsMultiuserEditSessionMode(esriMultiuserEditSessionMode.esriMESMNonVersioned) && IsUnversioned)
          {
            pMUWSEdit.StartMultiuserEditing(esriMultiuserEditSessionMode.esriMESMNonVersioned);
          }
          else if (pMUWSEdit.SupportsMultiuserEditSessionMode(esriMultiuserEditSessionMode.esriMESMVersioned) && !IsUnversioned)
          {
            pMUWSEdit.StartMultiuserEditing(esriMultiuserEditSessionMode.esriMESMVersioned);
          }

          else
          {
            return false;
          }
        }
        catch (COMException ex)
        {
          MessageBox.Show(ex.Message + "  " + Convert.ToString(ex.ErrorCode), "Start Editing");
          return false;
        }
      }
      else
      {
        try
        {
          pWSEdit.StartEditing(false);
        }
        catch (COMException ex)
        {
          MessageBox.Show(ex.Message + "  " + Convert.ToString(ex.ErrorCode), "Start Editing");
          return false;
        }
      }

      pWSEdit.DisableUndoRedo();
      try
      {
        pWSEdit.StartEditOperation();
      }
      catch
      {
        pWSEdit.StopEditing(false);
        return false;
      }
      return true;
    }

    public void ExecuteCommand(string CommUID)
    {
      if (CommUID.Trim() == "")
        return;
      ICommandBars pCommBars = ArcMap.Application.Document.CommandBars;
      IUID pIDComm = new UIDClass();
      pIDComm.Value = CommUID;
      ICommandItem pCommItem = pCommBars.Find(pIDComm);
      pCommItem.Execute();
    }
  
  }

}
