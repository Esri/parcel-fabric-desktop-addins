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
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;

namespace ParcelFabricQualityControl
{
  public class AddQCLayers : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    static public string AssemblyDirectory
    {
      get
      {
        string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return System.IO.Path.GetDirectoryName(path);
      }
    }

    public AddQCLayers()
    {
    }

    protected override void OnClick()
    {
      IArray LineLyrArr;
      IArray PolygonLyrArr;
      IActiveView pActiveView = ArcMap.Document.ActiveView;
      IMap pMap = pActiveView.FocusMap;

      Utilities Utils = new Utilities();

      if (!Utils.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTLines, out LineLyrArr))
        return;

      if (!Utils.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTParcels, out PolygonLyrArr))
        return;

      IFeatureLayer pFlyr = (IFeatureLayer)LineLyrArr.get_Element(0);
      IFeatureClass pFabricLinesFC = pFlyr.FeatureClass;

      pFlyr = (IFeatureLayer)PolygonLyrArr.get_Element(0);
      IFeatureClass pFabricParcelsFC = pFlyr.FeatureClass;

      IProjectedCoordinateSystem2 pPCS = null;

      IGeoDataset pGeoDS = (IGeoDataset)pFabricLinesFC;
      ISpatialReference pFabricSpatRef = pGeoDS.SpatialReference;

      double dMetersPerUnit = 1;
      bool bFabricIsInGCS = !(pFabricSpatRef is IProjectedCoordinateSystem2);
      if (pFabricSpatRef != null)
      {
        if (!bFabricIsInGCS)
        {
          pPCS = (IProjectedCoordinateSystem2)pFabricSpatRef;
          dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
        }
      }

      string fileName ="";
      if (dMetersPerUnit!=1)
        fileName = AssemblyDirectory + "\\QualityControlLayers\\QA Symbology Feet.lyr";
      else
        fileName = AssemblyDirectory + "\\QualityControlLayers\\QA Symbology Meters.lyr";
      bool bIsBefore1022 = false;
      string sBuild=Utils.GetDesktopBuildNumberFromRegistry();
      int iBuildNumber = 0;
      if (Int32.TryParse(sBuild, out iBuildNumber))
        bIsBefore1022 = iBuildNumber<3542; //CR278039 was fixed 10.2.2.3542

      AddQALayerToActiveView(pMap, pFabricLinesFC, pFabricParcelsFC, fileName, dMetersPerUnit, bIsBefore1022, bFabricIsInGCS);
    }

    protected override void OnUpdate()
    {
    }

    private void AddQALayerToActiveView(IMap map, IFeatureClass SourceLineFeatureClass, IFeatureClass SourcePolygonFeatureClass,
      string layerPathFile, double metersPerUnit, bool bIsBefore1022, bool bFabricIsInGCS)
    {
      if (map == null || layerPathFile == null || !layerPathFile.EndsWith(".lyr"))
      {
        return;
      }

      // Create a new GxLayer
      IGxLayer gxLayer = new GxLayerClass();
      IGxFile gxFile = (IGxFile)gxLayer;

      // Set the path for where the layerfile is located on disk
      gxFile.Path = layerPathFile;

      // Test if we have a valid layer and add it to the map
      if (!(gxLayer.Layer == null))
      {
        if (!(gxLayer.Layer is ICompositeLayer))
          return;
        ICompositeLayer pCompLyr = (ICompositeLayer)gxLayer.Layer;
        for (int i=0; i<pCompLyr.Count; i++)
        {
          ILayer pLyr = pCompLyr.get_Layer(i);
          //
          if (pLyr is IFeatureLayer)
          {
            IFeatureLayer pFlyr = (IFeatureLayer)pLyr;
            //now update the definition query
            IFeatureLayerDefinition pFeatLyrDef = (IFeatureLayerDefinition)pFlyr;
            string sLyrName = pFlyr.Name;
            bool bExc = false;
            if (sLyrName.Contains("Minus"))
            {
              pFlyr.FeatureClass = SourceLineFeatureClass;
              bExc = false;

              //ILayerFields pLayerFields = pFlyr as ILayerFields;
              //First turn off all layers
              //for (int kk = 0; kk < pLayerFields.FieldCount; kk++)
              //{
              //  IFieldInfo pFieldInfo = pLayerFields.get_FieldInfo(kk);
              //  pFieldInfo.Visible = false;
              //}

              SetLabelExpressionOnFeatureLayer(pFlyr, out bExc);
              if (bExc || bFabricIsInGCS)
              {
                int jj=pFlyr.FeatureClass.FindField("ComputedMinusObserved");
                if (jj>-1)
                {
                  string sVal = (0.1 / metersPerUnit).ToString("0.00");
                  string sCminusO=pFlyr.FeatureClass.Fields.get_Field(jj).Name;
                  pFeatLyrDef.DefinitionExpression = sCminusO + " > "+sVal+" AND " + sCminusO + " < "+sVal;

                  //IFieldInfo pFieldInfo = pLayerFields.get_Field(jj) as IFieldInfo;
                  //pFieldInfo.Visible = true;

                }
                continue;
              }
              string s = pFeatLyrDef.DefinitionExpression;
              int iField = SourceLineFeatureClass.FindField("ArcLength");
              if (iField > -1)
              {

                //IFieldInfo pFieldInfo = pLayerFields.get_Field(iField) as IFieldInfo;
                //pFieldInfo.Visible = true;

                string s2 = SourceLineFeatureClass.Fields.get_Field(iField).Name;
                pFeatLyrDef.DefinitionExpression = s.Replace("\"ArcLength\"", s2);
                pFeatLyrDef.DefinitionExpression = pFeatLyrDef.DefinitionExpression.Replace("ArcLength", s2);
              }

              s = pFeatLyrDef.DefinitionExpression;
              iField = SourceLineFeatureClass.FindField("Distance");
              if (iField > -1)
              {

                //IFieldInfo pFieldInfo = pLayerFields.get_Field(iField) as IFieldInfo;
                //pFieldInfo.Visible = true;

                string s2 = SourceLineFeatureClass.Fields.get_Field(iField).Name;
                pFeatLyrDef.DefinitionExpression = s.Replace("\"Distance\"", s2);
                pFeatLyrDef.DefinitionExpression = pFeatLyrDef.DefinitionExpression.Replace("Distance", s2);
              }

              s = pFeatLyrDef.DefinitionExpression;
              iField = SourceLineFeatureClass.FindField("DensifyType");
              if (iField > -1)
              {
                string s2 = SourceLineFeatureClass.Fields.get_Field(iField).Name;
                pFeatLyrDef.DefinitionExpression = s.Replace("\"DensifyType\"", s2);
                pFeatLyrDef.DefinitionExpression = pFeatLyrDef.DefinitionExpression.Replace("DensifyType", s2);
              }


              s = pFeatLyrDef.DefinitionExpression;
              pFeatLyrDef.DefinitionExpression = s.Replace("\"Shape_Length\"", SourceLineFeatureClass.LengthField.Name);
              pFeatLyrDef.DefinitionExpression = pFeatLyrDef.DefinitionExpression.Replace("Shape_Length", SourceLineFeatureClass.LengthField.Name);

            }
            else if (sLyrName.Contains("Parcel"))
            { //In 10.1 start editing crashes if the definition query in these layers that use POWER function is present.
              //Can test if the release is 10.1 and knock out the def query, or else exclude the layers.
              
              string s = pFeatLyrDef.DefinitionExpression;
              int iField=SourcePolygonFeatureClass.FindField("MiscloseDistance");
              string s2 = SourcePolygonFeatureClass.Fields.get_Field(iField).Name;
              pFeatLyrDef.DefinitionExpression = s.Replace("\"MiscloseDistance\"", s2);
              pFeatLyrDef.DefinitionExpression = pFeatLyrDef.DefinitionExpression.Replace("MiscloseDistance", s2);

              s = pFeatLyrDef.DefinitionExpression;
              iField = SourcePolygonFeatureClass.FindField("ShapeStdErrorE");
              s2 = SourcePolygonFeatureClass.Fields.get_Field(iField).Name;
              pFeatLyrDef.DefinitionExpression = s.Replace("\"ShapeStdErrorE\"", s2);
              pFeatLyrDef.DefinitionExpression = pFeatLyrDef.DefinitionExpression.Replace("ShapeStdErrorE", s2);

              s = pFeatLyrDef.DefinitionExpression;
              iField = SourcePolygonFeatureClass.FindField("ShapeStdErrorN");
              s2 = SourcePolygonFeatureClass.Fields.get_Field(iField).Name;
              pFeatLyrDef.DefinitionExpression = s.Replace("\"ShapeStdErrorN\"", s2);
              pFeatLyrDef.DefinitionExpression = pFeatLyrDef.DefinitionExpression.Replace("ShapeStdErrorN", s2);

              pFlyr.FeatureClass = SourcePolygonFeatureClass;
              SetLabelExpressionOnFeatureLayer(pFlyr, out bExc);

              if (s.ToLower().Contains("power") && bIsBefore1022)
              {//remove the def query CR278039
                pFeatLyrDef.DefinitionExpression = "";
                continue;
              }

            }
          }
        }
        gxLayer.Layer.Name = "QA Symbology";

        IMxDocument pMXDoc = ArcMap.Document;
        IOperationStack pOpSt = pMXDoc.OperationStack;
        IAddLayersOperation pAddLyrOp = new AddLayersOperationClass();
        pAddLyrOp.SetDestinationInfo(0,map,null);
        pAddLyrOp.Name = "Add Fabric QA Layer";
        pAddLyrOp.AddLayer(gxLayer.Layer);
        IOperation pOp=(IOperation)pAddLyrOp;
        pOpSt.Do(pOp);
      }
    }

    private void SetLabelExpressionOnFeatureLayer(IFeatureLayer FeatLayer, out bool LabelExcluded)
    {
      string s = "";
      LabelExcluded = false;
      if (FeatLayer is IGeoFeatureLayer)
      {
        IGeoFeatureLayer pGeoFeatLyr = (IGeoFeatureLayer)FeatLayer;
        IAnnotateLayerPropertiesCollection pAnnoLyrPropsColl = pGeoFeatLyr.AnnotationProperties;
        IAnnotateLayerProperties pAnnoLyrProps; IElementCollection obj1; IElementCollection obj2;
        pAnnoLyrPropsColl.QueryItem(0, out pAnnoLyrProps, out obj1, out obj2);
        ILabelEngineLayerProperties LblEngProp = (ILabelEngineLayerProperties)pAnnoLyrProps;
        
        s = LblEngProp.Expression.ToLower();
        if (FeatLayer.FeatureClass.LengthField.Name.ToLower().Contains("st_length"))
        { //exclude the label on function generated field shape_length

          if (s.Contains("shape_length"))
          {
            LblEngProp.IsExpressionSimple = true;
            LblEngProp.Expression = "";
            LabelExcluded=true;
            return;
          }
        }

        LblEngProp.Expression = s.Replace("shape_length", FeatLayer.FeatureClass.LengthField.Name);

        IFeatureClass pFC = FeatLayer.FeatureClass;
        int iFld1 = pFC.Fields.FindField("shapestderrore");
        if (iFld1 > -1)
        {
          string s1 = pFC.Fields.get_Field(iFld1).Name;
          s = LblEngProp.Expression;
          LblEngProp.Expression = s.Replace("shapestderrore", s1);
        }

        int iFld2 = pFC.Fields.FindField("shapestderrorn");
        if (iFld2 > -1)
        {
          s = LblEngProp.Expression;
          string s2 = pFC.Fields.get_Field(iFld2).Name;
          LblEngProp.Expression = s.Replace("shapestderrorn", s2);
        }

      }
    }

  }
}
