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

using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;

namespace ParcelFabricQualityControl
{
  public class CoordinateInverse : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public CoordinateInverse()
    {
    }

    protected override void OnClick()
    {
      IApplication pApp;
      ICadastralFabric m_pCadaFab;
      IQueryFilter m_pQF;

      #region Get Fabric

      pApp = (IApplication)ArcMap.Application;
      if (pApp == null)
        //if the app is null then could be running from ArcCatalog
        pApp = (IApplication)ArcCatalog.Application;

      if (pApp == null)
      {
        MessageBox.Show("Could not access the application.", "No Application found");
        return;
      }

      IGxApplication pGXApp = (IGxApplication)pApp;
      stdole.IUnknown pUnk = null;
      try
      {
        pUnk = (stdole.IUnknown)pGXApp.SelectedObject.InternalObjectName.Open();
      }
      catch (COMException ex)
      {
        if (ex.ErrorCode == (int)fdoError.FDO_E_DATASET_TYPE_NOT_SUPPORTED_IN_RELEASE ||
            ex.ErrorCode == -2147220944)
          MessageBox.Show("The dataset is not supported in this release.", "Could not open the dataset");
        else
          MessageBox.Show(ex.ErrorCode.ToString(), "Could not open the dataset");
        return;
      }

      if (pUnk is ICadastralFabric)
        m_pCadaFab = (ICadastralFabric)pUnk;
      else
      {
        MessageBox.Show("Please select a parcel fabric and try again.", "Not a parcel fabric");
        return;
      }
      #endregion

      IFeatureClass pFabricPointClass = (IFeatureClass)m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
      IDataset pDS = (IDataset)pFabricPointClass;
      IWorkspace pWS = pDS.Workspace;
      
      bool bIsFileBasedGDB = true;
      bool bIsUnVersioned = true;
      Utilities FabricUTILS = new Utilities();
      FabricUTILS.GetFabricPlatform(pWS, m_pCadaFab, out bIsFileBasedGDB,
        out bIsUnVersioned);


      if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
        return;
      m_pQF = new QueryFilterClass();
      m_pQF.WhereClause = "";
      int iChangePointCount = 0;
      try
      {
        //next need to use an in clause to update the points, ...
        ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
        pSchemaEd.ReleaseReadOnlyFields((ITable)pFabricPointClass, esriCadastralFabricTable.esriCFTPoints);
        if (!UpdatePointXYFromGeometry((ITable)pFabricPointClass, m_pQF, (bIsUnVersioned || bIsFileBasedGDB),0.0001, out iChangePointCount))
        {
          FabricUTILS.AbortEditing(pWS);
          return;
        }

        FabricUTILS.StopEditing(pWS);
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);

        MessageBox.Show("Updated " + iChangePointCount.ToString() + " points.","Coordinate Inverse");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        FabricUTILS.AbortEditing(pWS);
      }
      finally
      {
      }
    }

    protected override void OnUpdate()
    {
    }

    private bool UpdatePointXYFromGeometry(ITable PointTable, IQueryFilter QueryFilter, bool Unversioned, double UpdateIfMoreThanTolerance, out int ChangedPointCount)
    {
      IProgressDialogFactory pProgressorDialogFact = new ProgressDialogFactoryClass();
      ITrackCancel pTrackCancel = new CancelTrackerClass();
      IStepProgressor pStepProgressor = pProgressorDialogFact.Create(pTrackCancel, ArcMap.Application.hWnd);
      IProgressDialog2 pProgressorDialog = (IProgressDialog2)pStepProgressor;
      try
      {
        pStepProgressor.MinRange = 0;
        pStepProgressor.MaxRange = PointTable.RowCount(null);
        pStepProgressor.StepValue = 1;
        pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        bool bCont = true;

        ITableWrite pTableWr = (ITableWrite)PointTable;//used for unversioned table
        IRow pPointFeat = null;
        ICursor pPtCurs = null;
        ChangedPointCount = 0;
        if (Unversioned)
          pPtCurs = pTableWr.UpdateRows(QueryFilter, false);
        else
          pPtCurs = PointTable.Update(QueryFilter, false);

        pPointFeat = pPtCurs.NextRow();

        Int32 iPointIdx_X = pPtCurs.Fields.FindField("X");
        Int32 iPointIdx_Y = pPtCurs.Fields.FindField("Y");

        pProgressorDialog.ShowDialog();
        pStepProgressor.Message = "Updating point data...";

        while (pPointFeat != null)
        {//loop through all of the fabric points, and if any of the point id values are in the deleted set, 
          //then remove the control name from the point's NAME field

          bCont = pTrackCancel.Continue();
          if (!bCont)
            break;
          
          IFeature pFeat = (IFeature)pPointFeat;
          IPoint pPtSource = (IPoint)pFeat.ShapeCopy;

          if (pPtSource == null)
          {
            Marshal.ReleaseComObject(pPointFeat); //garbage collection
            pPointFeat = pPtCurs.NextRow();
            continue;
          }

          if (pPtSource.IsEmpty)
          {
            Marshal.ReleaseComObject(pPointFeat); //garbage collection
            pPointFeat = pPtCurs.NextRow();
            continue;
          }
          IPoint pPtTarget = new ESRI.ArcGIS.Geometry.PointClass();
          pPtTarget.X = Convert.ToDouble(pPointFeat.get_Value(iPointIdx_X));
          pPtTarget.Y = Convert.ToDouble(pPointFeat.get_Value(iPointIdx_Y));

          ILine pLine = new ESRI.ArcGIS.Geometry.LineClass();
          pLine.PutCoords(pPtSource, pPtTarget);

          if (pLine.Length > UpdateIfMoreThanTolerance)
          {
            pPointFeat.set_Value(iPointIdx_X, pPtSource.X);
            pPointFeat.set_Value(iPointIdx_Y, pPtSource.Y);

            if (Unversioned)
              pPtCurs.UpdateRow(pPointFeat);
            else
              pPointFeat.Store();
            ChangedPointCount++;
            string sCnt = ChangedPointCount.ToString() + " of " + pStepProgressor.MaxRange.ToString();
            pStepProgressor.Message = "Updating point data..." + sCnt;
          }

          Marshal.ReleaseComObject(pPointFeat); //garbage collection
          pPointFeat = pPtCurs.NextRow();

          if (pStepProgressor.Position < pStepProgressor.MaxRange)
            pStepProgressor.Step();

        }
        Marshal.ReleaseComObject(pPtCurs); //garbage collection
        return bCont;

      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating point XY from shape: " + Convert.ToString(ex.ErrorCode));
        ChangedPointCount = 0;
        return false;
      }
      finally
      {
        pStepProgressor = null;
        if (!(pProgressorDialog == null))
          pProgressorDialog.HideDialog();
        pProgressorDialog = null;
      }
    }
  }
}
