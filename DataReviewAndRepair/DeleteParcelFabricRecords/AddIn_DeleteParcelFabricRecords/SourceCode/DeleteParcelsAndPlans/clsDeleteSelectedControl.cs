/*
 Copyright 1995-2015 Esri

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

using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;


namespace DeleteSelectedParcels
{
  public class clsDeleteSelectedControl : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private IFIDSet m_pFIDSetControl;
    private IQueryFilter m_pQF;

    public clsDeleteSelectedControl()
    {
    }

    protected override void OnClick()
    {
      bool bShowProgressor=false;
      IStepProgressor pStepProgressor = null;
      //Create a CancelTracker.
      ITrackCancel pTrackCancel = null;
      IProgressDialogFactory pProgressorDialogFact;

      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      //first get the selected parcel features
      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadExtMan;
      if (pCadPacMan.PacketOpen)
      {
        MessageBox.Show("The Delete Control command cannot be used when there is an open job.\r\nPlease finish or discard the open job, and try again.",
          "Delete Selected Control");
        return;
      }

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

      IActiveView pActiveView = ArcMap.Document.ActiveView;
      IMap pMap = pActiveView.FocusMap;
      ICadastralFabric pCadFabric = null;
      clsFabricUtils FabricUTILS = new clsFabricUtils();
      IProgressDialog2 pProgressorDialog = null;

      //if we're in an edit session then grab the target fabric
      if (pEd.EditState == esriEditState.esriStateEditing)
        pCadFabric = pCadEd.CadastralFabric;

      if (pCadFabric == null)
      {//find the first fabric in the map
        if (!FabricUTILS.GetFabricFromMap(pMap, out pCadFabric))
        {
          MessageBox.Show
            ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }

      IArray CFControlLayers = new ArrayClass();

      if (!(FabricUTILS.GetControlLayersFromFabric(pMap, pCadFabric, out CFControlLayers)))
        return; //no fabric sublayers available for the targeted fabric

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedDelete = false;
      IWorkspace pWS = null;
      ITable pPointsTable = null;
      ITable pControlTable = null;

      try
      {
        if (pEd.EditState == esriEditState.esriStateEditing)
        {
          try
          {
            pEd.StartOperation();
          }
          catch
          {
            pEd.AbortOperation();//abort any open edit operations and try again
            pEd.StartOperation();
          }
        }

        IFeatureLayer pFL = (IFeatureLayer)CFControlLayers.get_Element(0);
        IDataset pDS = (IDataset)pFL.FeatureClass;
        pWS = pDS.Workspace;

        if (!FabricUTILS.SetupEditEnvironment(pWS, pCadFabric, pEd, out bIsFileBasedGDB,
          out bIsUnVersioned, out bUseNonVersionedDelete))
          return;

        //loop through each control layer and
        //Get the selection of control
         int iCnt=0;
         int iTotalSelectionCount = 0;
         for (; iCnt < CFControlLayers.Count; iCnt++)
         {
           pFL = (IFeatureLayer)CFControlLayers.get_Element(iCnt);
           IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
           ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;
           iTotalSelectionCount += pSelSet.Count;
         }

         if (iTotalSelectionCount == 0)
         {
           MessageBox.Show("Please select some fabric control points and try again.", "No Selection",
             MessageBoxButtons.OK, MessageBoxIcon.Information);
           if (bUseNonVersionedDelete)
           {
             pCadEd.CadastralFabricLayer = null;
             CFControlLayers = null;
           }
           return;
         }

         bShowProgressor = (iTotalSelectionCount > 10);

         if (bShowProgressor)
         {
           pProgressorDialogFact = new ProgressDialogFactoryClass();
           pTrackCancel = new CancelTrackerClass();
           pStepProgressor = pProgressorDialogFact.Create(pTrackCancel, ArcMap.Application.hWnd);
           pProgressorDialog = (IProgressDialog2)pStepProgressor;
           pStepProgressor.MinRange = 1;
           pStepProgressor.MaxRange = iTotalSelectionCount * 2; //(runs through selection twice)
           pStepProgressor.StepValue = 1;
           pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
         }

        //loop through each control layer and
        //delete from its selection
        m_pQF = new QueryFilterClass();
        iCnt=0;
        for (; iCnt < CFControlLayers.Count; iCnt++)
        {
          pFL = (IFeatureLayer)CFControlLayers.get_Element(iCnt);
          IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
          ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;

          ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
          string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
          string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

          if (bShowProgressor)
          {
            pProgressorDialog.ShowDialog();
            pStepProgressor.Message = "Collecting Control point data...";
          }

          //Add the OIDs of all the selected control points into a new feature IDSet
          string[] sOIDListPoints = { "(" };
          int tokenLimit = 995;
          //int tokenLimit = 5; //temp for testing
          bool bCont = true;
          int j = 0;
          int iCounter = 0;

          m_pFIDSetControl = new FIDSetClass();

          ICursor pCursor = null;
          pSelSet.Search(null, false, out pCursor);//code deletes all selected control points
          IFeatureCursor pControlFeatCurs = (IFeatureCursor)pCursor;
          IFeature pControlFeat = pControlFeatCurs.NextFeature();
          int iPointID = pControlFeatCurs.FindField("POINTID");

          while (pControlFeat != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
            {
              bCont = pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            bool bExists = false;
            m_pFIDSetControl.Find(pControlFeat.OID, out bExists);
            if (!bExists)
            {
              m_pFIDSetControl.Add(pControlFeat.OID);
              object obj = pControlFeat.get_Value(iPointID);

              if (iCounter <= tokenLimit)
              {
                //if the PointID is not null add it to a query string as well
                if (obj != DBNull.Value)
                  sOIDListPoints[j] += Convert.ToString(obj) + ",";
                iCounter++;
              }
              else
              {//maximum tokens reached
                //set up the next OIDList
                sOIDListPoints[j] = sOIDListPoints[j].Trim();
                iCounter = 0;
                j++;
                FabricUTILS.RedimPreserveString(ref sOIDListPoints, 1);
                sOIDListPoints[j] = "(";
                if (obj != DBNull.Value)
                  sOIDListPoints[j] += Convert.ToString(obj) + ",";
              }
            }
            Marshal.ReleaseComObject(pControlFeat); //garbage collection
            pControlFeat = pControlFeatCurs.NextFeature();

            if (bShowProgressor)
            {
              if (pStepProgressor.Position < pStepProgressor.MaxRange)
                pStepProgressor.Step();
            }
          }
          Marshal.ReleaseComObject(pCursor); //garbage collection

          if (!bCont)
          {
            AbortEdits(bUseNonVersionedDelete, pEd, pWS);
            return;
          }

          if (bUseNonVersionedDelete)
          {
            if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
            {
              if (bUseNonVersionedDelete)
                pCadEd.CadastralFabricLayer = null;
              return;
            }
          }

          //first delete all the control point records
          if (bShowProgressor)
            pStepProgressor.Message = "Deleting control points...";

          bool bSuccess = true;
          pPointsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
          pControlTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);

          if (!bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsByFIDSet(pControlTable, m_pFIDSetControl, pStepProgressor, pTrackCancel);
          if (bUseNonVersionedDelete)
            bSuccess = FabricUTILS.DeleteRowsUnversioned(pWS, pControlTable,
                m_pFIDSetControl, pStepProgressor, pTrackCancel);
          if (!bSuccess)
          {
            AbortEdits(bUseNonVersionedDelete, pEd, pWS);
            return;
          }
          //next need to use an in clause to update the points, ...
          ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
          //...for each item in the sOIDListPoints array
          foreach (string inClause in sOIDListPoints)
          {
            string sClause = inClause.Trim().TrimEnd(',');
            if (sClause.EndsWith("("))
              continue;
            if (sClause.Length < 3)
              continue;
            pSchemaEd.ReleaseReadOnlyFields(pPointsTable, esriCadastralFabricTable.esriCFTPoints);
            m_pQF.WhereClause = (sPref + pPointsTable.OIDFieldName + sSuff).Trim() + " IN " + sClause + ")";

            if (!FabricUTILS.ResetPointAssociations(pPointsTable, m_pQF, bIsUnVersioned))
            {
              pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);
              return;
            }
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);
          }
        }

        if (bUseNonVersionedDelete)
          FabricUTILS.StopEditing(pWS);

        if (pEd.EditState == esriEditState.esriStateEditing)
          pEd.StopOperation("Delete Control Points");
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }
      finally
      {
        RefreshMap(pActiveView, CFControlLayers);

        //update the TOC
        IMxDocument mxDocument = (ESRI.ArcGIS.ArcMapUI.IMxDocument)(ArcMap.Application.Document);
        for (int i = 0; i < mxDocument.ContentsViewCount; i++)
        {
          IContentsView pCV = (IContentsView)mxDocument.get_ContentsView(i);
          pCV.Refresh(null);
        }

        if (pProgressorDialog != null)
          pProgressorDialog.HideDialog();

        if (bUseNonVersionedDelete)
        {
          pCadEd.CadastralFabricLayer = null;
          CFControlLayers = null;
        }

        if (pMouseCursor != null)
          pMouseCursor.SetCursor(0);
      }
    }

    private void AbortEdits(bool bUseNonVersionedDelete, IEditor pEd, IWorkspace pWS)
    {
      clsFabricUtils FabricUTILS = new clsFabricUtils();
      if (bUseNonVersionedDelete)
        FabricUTILS.AbortEditing(pWS);
      else
      {
        if (pEd != null)
        {
          if (pEd.EditState == esriEditState.esriStateEditing)
            pEd.AbortOperation();
        }        
      }
    }

    protected override void OnUpdate()
    {
      CustomizelHelperExtension v = CustomizelHelperExtension.GetExtension();
      this.Enabled = v.CommandIsEnabled;
      
      if(!this.Enabled)
        this.Enabled = v.MapHasUnversionedFabric;
    }

    private void RefreshMap(IActiveView ActiveView, IArray ControlLayers)
    {
      try
      {
        for (int z = 0; z <= ControlLayers.Count - 1; z++)
        {
          if (ControlLayers.get_Element(z) != null)
          {
            IFeatureSelection pFeatSel = (IFeatureSelection)ControlLayers.get_Element(z);
            pFeatSel.Clear();//refreshes the parcel explorer
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, ControlLayers.get_Element(z), ActiveView.Extent);
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ControlLayers.get_Element(z), ActiveView.Extent);
          }
        }
      }
      catch
      { }
    }

  }
}
