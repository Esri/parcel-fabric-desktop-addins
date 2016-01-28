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
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geometry;

namespace SampleParcelFabricEdits
{
  public class ConvertStraightLineToCurve : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public ConvertStraightLineToCurve()
    {
    }

    protected override void OnClick()
    {
      //first check that we are currently editing
      if (ArcMap.Editor.EditState != esriEditState.esriStateEditing)
      {
        MessageBox.Show("Please start editing and try again.", "Sample Code");
        return;
      }

      //get the cadastral editor
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      
      //get the fabric line layers that belong to the target fabric.
      // **SAMPLE CODE NOTE**
      //This next function allows for more than 1 fabric lines sublayer in the map document, and uses a line layer array
      //However, this sample code assumes just one line fabric sub layer, and works with the first found
      //The function is provided for other expanded uses if needed elsewhere.
      IArray LineLayerArray;
      if (!GetFabricSubLayers(ArcMap.Document.ActiveView.FocusMap, esriCadastralFabricTable.esriCFTLines, 
         true, pCadEd.CadastralFabric, out LineLayerArray))
        return;

      // get the line selection; this code sample uses first line layer for the target fabric (first element)
      ISelectionSet2 LineSelection = 
        GetSelectionFromLayer(LineLayerArray.get_Element(0) as ICadastralFabricSubLayer);

      // check to see if there is only one parcel line selected
      // **SAMPLE CODE NOTE**
      //This sample code ensures one line feature, although it can be easily adapted for use on 
      //multiple line selection.
      if (LineSelection.Count != 1)
      {
        MessageBox.Show("Please select only one parcel line from the Target fabric.", "Sample Code");
        return;
      }
      //Get a search cursor from the line selection to get the parcel id
      //We need to get an edit lock on the parcel using the parcel id
      //An edit lock will guarantee the edit will persist in a multi-user environment after a reconcile

      ILongArray pParcelsToLock = new LongArrayClass();
      IFIDSet pFIDSetForParcelRegen = new FIDSet();
      ICursor pCur;
      LineSelection.Search(null,false, out pCur); 
      //this cursor returns the selected lines
      // **SAMPLE CODE NOTE**
      //setup for potential use for multiple line selection, even though this sample uses a single line selection

      //get the field indices for line attributes needed.
      int idxParcelID = pCur.FindField("parcelid");
      int idxToPointID = pCur.FindField("topointid");
      int idxFromPointID = pCur.FindField("frompointid");
      int idxCenterPtId = pCur.FindField("centerpointid");
      int idxDistance = pCur.FindField("distance");
      int idxRadius = pCur.FindField("radius");
      int idxCategory = pCur.FindField("category");
      
      //also need the fabric point table and fields
      IFeatureClass pFabricPointsFC = (IFeatureClass)pCadEd.CadastralFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
      int idxPointX=pFabricPointsFC.FindField("x");
      int idxPointY=pFabricPointsFC.FindField("y");
      int idxPointCtrPt = pFabricPointsFC.FindField("centerpoint");

      var ListFromToPairsForRadialLines = new List<int[]>();
      // **SAMPLE CODE NOTE**
      //setup for potential use for multiple line selection, even though this sample uses a single line selection
      //the list declared above is here for potential use in Add-ins that make use of multiple circular arc lines

      int[] ParcelIdCtrPtIdFromId1FromId2;
      IRow pRow = pCur.NextRow();
      while (pRow!=null)
      {
        int iParcelID = (int)pRow.get_Value(idxParcelID);
        pParcelsToLock.Add(iParcelID); //LongArray for the parcel locks
        pFIDSetForParcelRegen.Add(iParcelID); //FIDSet for the parcel regenerate
        //now check for a center point id on the line; this is for the case of changing the radius of an existing curve

        object value = pRow.get_Value(idxCenterPtId);
        if (value != DBNull.Value)
        {//collecting information to remove radial lines
          ParcelIdCtrPtIdFromId1FromId2 = new int[4];  // 4-element array
          ParcelIdCtrPtIdFromId1FromId2[0] = iParcelID;
          ParcelIdCtrPtIdFromId1FromId2[1] = (int)value; //center point is always the to point of the radial line
          ParcelIdCtrPtIdFromId1FromId2[2] = (int)pRow.get_Value(idxFromPointID);
          ParcelIdCtrPtIdFromId1FromId2[3] = (int)pRow.get_Value(idxToPointID);
          // **SAMPLE CODE NOTE**
          //now add the array, to the list to accomodate other add-ins that may use 
          //more than one selected circular arc line
          ListFromToPairsForRadialLines.Add(ParcelIdCtrPtIdFromId1FromId2);
        }
        Marshal.ReleaseComObject(pRow);
        pRow = pCur.NextRow();
      }
      Marshal.ReleaseComObject(pCur);

      bool IsFileBasedGDB = (ArcMap.Editor.EditWorkspace.WorkspaceFactory.WorkspaceType != 
        esriWorkspaceType.esriRemoteDatabaseWorkspace);

      if (!IsFileBasedGDB)
      {
        //for file geodatabase creating a job is optional
        //see if parcel locks can be obtained on the selected parcels. First create a job.
        string NewJobName = "";
        if (!CreateJob(pCadEd.CadastralFabric,"Sample Code change line to curve", out NewJobName))
          return;

        if (!TestForEditLocks(pCadEd.CadastralFabric, NewJobName, pParcelsToLock))
          return;
      }

      //if we get this far, an edit lock has been acquired, or this is file geodatabase (no lock required)
      //prompt the user for a new radius value

      string sRadius = Interaction.InputBox("Enter a new Radius:", "Radius");
      //**SAMPLE CODE NOTE** : 
      // using the Interaction class from the Microsfot Visual Basic library 
      // is a quick and easy way to provide an input dialog in a single line of code for sample purposes,
      // without neeing to add a windows form, dockable window, or other UI elements into this project.

      double dRadius = 0;
      if(!Double.TryParse(sRadius, out dRadius))
        return;
      //we have a valid double value, so we can get ready to edit

      IProgressDialogFactory pProgressorDialogFact = new ProgressDialogFactoryClass();
      ITrackCancel pTrackCancel = new CancelTracker();
      IStepProgressor pStepProgressor = pProgressorDialogFact.Create(pTrackCancel, ArcMap.Application.hWnd);
      IProgressDialog2 pProgressorDialog = (IProgressDialog2)pStepProgressor;
      
      ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadEd.CadastralFabric;
      try
      { //turn off the read-only flag on the lines table and points table
        pSchemaEd.ReleaseReadOnlyFields(LineSelection.Target, esriCadastralFabricTable.esriCFTLines); //release read-only
        pSchemaEd.ReleaseReadOnlyFields((ITable)pFabricPointsFC, esriCadastralFabricTable.esriCFTPoints); //release read-only

        //start an edit operation
        ArcMap.Editor.StartOperation();

        //get an update cursor to make the edit on the line(s)
        LineSelection.Update(null, false, out pCur);

        pRow = pCur.NextRow();
        int iChangeCount = 0;
        while (pRow != null)
        {
          double dChord = (double)pRow.get_Value(idxDistance);
          if (Math.Abs(dRadius) <= dChord/2 && dRadius!=0) //minimum allowable radius is half the chord
          {
            Marshal.ReleaseComObject(pRow);
            pRow = pCur.NextRow();
            continue;
          }

          //compute a center point location from new radius, unless it's 0
          int iNewCtrPtId = 0;
          if(dRadius!=0)
          {
            IFeature pFeat = pRow as IFeature;
            IPoint pCtrPt = ComputeCenterPointFromRadius(pFeat.Shape as IPolyline, dRadius, true);
            IFeature pNewPointFeat = pFabricPointsFC.CreateFeature();
            //**SAMPLE CODE NOTE** :
            //if adding a large number of points (more than 20) then createfeature is not the fastest approach,
            //Instead you would pre-allocate points using an insert cursor...
            //At this point in the code, the normal geodatabase performance considerations apply
            iNewCtrPtId = pNewPointFeat.OID;
            pNewPointFeat.set_Value(idxPointX,pCtrPt.X);
            pNewPointFeat.set_Value(idxPointY, pCtrPt.Y);
            pNewPointFeat.set_Value(idxPointCtrPt, 1); //1 = true boolean
            pNewPointFeat.Shape = pCtrPt;
            pNewPointFeat.Store();
          }
          //get the initial radius if the line is a curve (radius is being updated)
          object obj = pRow.get_Value(idxRadius);
          bool bIsChangingFromCurve = (obj != DBNull.Value); //there is a radius value
          obj = pRow.get_Value(idxCenterPtId);
          bIsChangingFromCurve = bIsChangingFromCurve && (obj != DBNull.Value); //radius value and Ctr Pt ID exist
          int iExistingCtrPtId = 0;
          if (bIsChangingFromCurve)
            iExistingCtrPtId = (int)obj;
          if (dRadius == 0) //user entered value is zero meaning convert to straight line
          {//changing to a straight line so set the center point an radius to null
            pRow.set_Value(idxRadius, DBNull.Value);
            pRow.set_Value(idxCenterPtId, DBNull.Value);
          }
          else if (!bIsChangingFromCurve) //user entered a new radius, and the existing line is not a curve
          {//changing to a circular arc so set the radius, and set the center point id to the new point's OID
            pRow.set_Value(idxRadius, dRadius);
            pRow.set_Value(idxCenterPtId, iNewCtrPtId);
          }
          else if (bIsChangingFromCurve) //user entered a radius, and the existing line is a curve
          
          pCur.UpdateRow(pRow);
          iChangeCount++;
          Marshal.ReleaseComObject(pRow);
          pRow = pCur.NextRow();
        }
        Marshal.ReleaseComObject(pCur);

        if (iChangeCount == 0)
        {//if there are no changes then don't add to the edit operation stack
          ArcMap.Editor.AbortOperation();
          return;
        }

        if (ListFromToPairsForRadialLines.Count > 0)
        {
          IQueryFilter pQuFilter = new QueryFilter();
          string sCat = LineSelection.Target.Fields.get_Field(idxCategory).Name;
          string sToPt = LineSelection.Target.Fields.get_Field(idxToPointID).Name;
          string sFromPt = LineSelection.Target.Fields.get_Field(idxFromPointID).Name;
          string sParcelID = LineSelection.Target.Fields.get_Field(idxParcelID).Name;
          string sInClauseToPts = "(";
          string sInClauseFromPts = "(";
          string sInClauseParcelIds = "(";
          //**SAMPLE CODE NOTE** : 
          //The following In Clause, when contructed for production environments
          //should take into account the token limit on Oracle database platforms. (<1000)
          // the processing of the in clause should be broekn into blocks with the in cluase has no more than 1000 elements
          foreach (int[] iParcelIdCtrPtIdFromId1FromId2 in ListFromToPairsForRadialLines)
          {
            if (sInClauseParcelIds.Length == 1)
              sInClauseParcelIds += iParcelIdCtrPtIdFromId1FromId2[0].ToString();
            else
              sInClauseParcelIds += "," + iParcelIdCtrPtIdFromId1FromId2[0].ToString();

            if (sInClauseToPts.Length == 1)
              sInClauseToPts += iParcelIdCtrPtIdFromId1FromId2[1].ToString();
            else
              sInClauseToPts += "," + iParcelIdCtrPtIdFromId1FromId2[1].ToString();

            if (sInClauseFromPts.Length == 1)
            {
              sInClauseFromPts += iParcelIdCtrPtIdFromId1FromId2[2].ToString();
              sInClauseFromPts += "," + iParcelIdCtrPtIdFromId1FromId2[3].ToString();
            }
            else
            {
              sInClauseFromPts += "," + iParcelIdCtrPtIdFromId1FromId2[2].ToString();
              sInClauseFromPts += "," + iParcelIdCtrPtIdFromId1FromId2[2].ToString();            
            }
          }

          pQuFilter.WhereClause = sCat + " = 4 AND " + sParcelID + " IN " + sInClauseParcelIds
            + ") AND " + sFromPt + " IN " + sInClauseFromPts
            + ") AND " + sToPt + " IN " + sInClauseToPts + ")";
          LineSelection.Target.DeleteSearchedRows(pQuFilter);
        }
        
        //with the new information added to the line, the rest of the parcel needs to be updated
        //regenerate the parcel using the parcel fidset

        ICadastralFabricRegeneration pRegenFabric = new CadastralFabricRegenerator();
        #region regenerator enum
        // enum esriCadastralRegeneratorSetting
        // esriCadastralRegenRegenerateGeometries         =   1
        // esriCadastralRegenRegenerateMissingRadials     =   2,
        // esriCadastralRegenRegenerateMissingPoints      =   4,
        // esriCadastralRegenRemoveOrphanPoints           =   8,
        // esriCadastralRegenRemoveInvalidLinePoints      =   16,
        // esriCadastralRegenSnapLinePoints               =   32,
        // esriCadastralRegenRepairLineSequencing         =   64,
        // esriCadastralRegenRepairPartConnectors         =   128

        // By default, the bitmask member is 0 which will only regenerate geometries.
        // (equivalent to passing in regeneratorBitmask = 1)
        #endregion

        pRegenFabric.CadastralFabric = pCadEd.CadastralFabric;
        pRegenFabric.RegeneratorBitmask = 7;
        pRegenFabric.RegenerateParcels(pFIDSetForParcelRegen, false, pTrackCancel);

        //15 (enum values of 8 means remove orphan points; this only works when doing entire fabric)
        //TODO: remove orphaned center points programmatically
        pStepProgressor.MinRange = 0;
        pStepProgressor.MaxRange = iChangeCount;
        pStepProgressor.StepValue = 1;
        pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        pRegenFabric.RegenerateParcels(pFIDSetForParcelRegen, false, pTrackCancel);

        ArcMap.Editor.StopOperation("Change line radius");

      }
      catch (Exception ex)
      {
        ArcMap.Editor.AbortOperation();
        MessageBox.Show(ex.Message);
      }
      finally
      {
        if (pSchemaEd != null)
        {
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);
        }
        pStepProgressor = null;
        if (!(pProgressorDialog == null))
          pProgressorDialog.HideDialog();
        pProgressorDialog = null;

        RefreshMap(LineLayerArray);
        ICadastralExtensionManager pCExMan = (ICadastralExtensionManager)pCadEd;
        IParcelPropertiesWindow2 pPropW = (IParcelPropertiesWindow2)pCExMan.ParcelPropertiesWindow;
        pPropW.RefreshAll();
        //update the TOC
        IMxDocument mxDocument = (ESRI.ArcGIS.ArcMapUI.IMxDocument)(ArcMap.Application.Document);
        for (int i = 0; i < mxDocument.ContentsViewCount; i++)
        {
          IContentsView pCV = (IContentsView)mxDocument.get_ContentsView(i);
          pCV.Refresh(null);
        }

      }

    }
    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }

    private bool GetFabricSubLayers(IMap Map, esriCadastralFabricTable FabricSubClass, bool ExcludeNonTargetFabrics, 
      ICadastralFabric TargetFabric, out IArray CFParcelFabSubLayers)
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
            ICadastralFabric ThisLayersFabric = pCFSubLyr.CadastralFabric;
            bool bIsTargetFabricLayer = ThisLayersFabric.Equals(TargetFabric);
            if (!ExcludeNonTargetFabrics || (ExcludeNonTargetFabrics && bIsTargetFabricLayer))
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

    private ISelectionSet2 GetSelectionFromLayer(ICadastralFabricSubLayer FabricSubLayer)
    {
      IFeatureSelection pFeatSel = (IFeatureSelection)FabricSubLayer;
      ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;
      return pSelSet;
    }

    private bool CreateJob(ICadastralFabric Fabric, string JobDescription, out string NewJobName)
    {
      DateTime localNow = DateTime.Now;
      string sTime = Convert.ToString(localNow);
      ICadastralJob pJob = new CadastralJob();
      pJob.Name = NewJobName = sTime;
      pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
      pJob.Description = JobDescription;
      try
      {
        Int32 jobId = Fabric.CreateJob(pJob);
        return true;
      }
      catch (COMException ex)
      {
        if (ex.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_ALREADY_EXISTS)
        {
          MessageBox.Show("Job named: '" + pJob.Name + "', already exists");
        }
        else
        {
          MessageBox.Show(ex.Message);
        }
        return false;
      }
    }

    private bool TestForEditLocks(ICadastralFabric Fabric, string NewJobName, ILongArray ParcelsToLock)
    {
      ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)Fabric;
      pFabLocks.LockingJob = NewJobName;
      
      ILongArray pLocksInConflict = null;
      ILongArray pSoftLcksInConflict = null;

      try
      {
        pFabLocks.AcquireLocks(ParcelsToLock, true, ref pLocksInConflict, ref pSoftLcksInConflict);
        return true;
      }
      catch (COMException pCOMEx)
      {
        if (pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_LOCK_ALREADY_EXISTS ||
          pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_CURRENTLY_EDITED)
        {
          MessageBox.Show("Edit Locks could not be acquired on all parcels of selected lines.");
          // since the operation is being aborted, release any locks that were acquired
          pFabLocks.UndoLastAcquiredLocks();
        }
        else
          MessageBox.Show(pCOMEx.Message + Environment.NewLine + Convert.ToString(pCOMEx.ErrorCode));

        return false;
      }
    }

    private IPoint ComputeCenterPointFromRadius(IPolyline ThePolyline, double NewRadius, bool IsMinorCurve)
    {
      IConstructCircularArc pCircArcConst = new CircularArc() as IConstructCircularArc;
      pCircArcConst.ConstructEndPointsRadius(ThePolyline.FromPoint, ThePolyline.ToPoint,
          (NewRadius < 0), Math.Abs(NewRadius), IsMinorCurve);
      ICircularArc pCircArc = pCircArcConst as ICircularArc;
      IPoint pCtrPoint = pCircArc.CenterPoint;
      IZAware pZAw = pCtrPoint as IZAware;
      pZAw.ZAware = true;
      pCtrPoint.Z = 0;
      return pCtrPoint;
    }

    private void RefreshMap(IArray LineLayers)
    {
      try
      {
        for (int z = 0; z <= LineLayers.Count - 1; z++)
        {
          if (LineLayers.get_Element(z) != null)
            ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection | esriViewDrawPhase.esriViewGeography, LineLayers.get_Element(z), ArcMap.Document.ActiveView.Extent);
        }
      }
      catch
      { }
    }

  }
}
