/*
 Copyright 1995-2012 ESRI

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
//using System.Collections.Generic;
using System.Collections;

namespace RepairFabricHistory
{
  public class DateFieldChanger : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSetParcels;
    private ICursor m_pCursor;

    private IQueryFilter m_pQF;
    private bool m_bShowProgressor;
    public DateFieldChanger()
    {
    }

    protected override void OnClick()
    {
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      //first get the selected parcel features
      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);
      Marshal.ReleaseComObject(pUID);

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

      IFeatureLayer CFPointLayer = null; IFeatureLayer CFLineLayer = null;
      IFeatureLayer CFControlLayer = null;
      IFeatureLayer CFLinePointLayer = null;

      IActiveView pActiveView = ArcMap.Document.ActiveView;
      IMap pMap = pActiveView.FocusMap;
      ICadastralFabric pCadFabric = null;
      IProgressDialog2 pProgressorDialog = null;

      clsFabricUtils UTILS = new clsFabricUtils();

      //if we're in an edit session then grab the target fabric
      if (pEd.EditState == esriEditState.esriStateEditing)
        pCadFabric = pCadEd.CadastralFabric;

      if (pCadFabric == null)
      {//find the first fabric in the map
        if (!UTILS.GetFabricFromMap(pMap, out pCadFabric))
        {
          MessageBox.Show
            ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }

      IArray CFParcelLayers = new ArrayClass();

      if (!(UTILS.GetFabricSubLayersFromFabric(pMap, pCadFabric, out CFPointLayer, out CFLineLayer,
          out CFParcelLayers, out CFControlLayer, out CFLinePointLayer)))
        return; //no fabric sublayers available for the targeted fabric

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersioned = false;
      IWorkspace pWS = null;
      try
      {
        //Get the selection of parcels
        IFeatureLayer pFL = (IFeatureLayer)CFParcelLayers.get_Element(0);
        IDataset pDS = (IDataset)pFL.FeatureClass;
        pWS = pDS.Workspace;

        if (!UTILS.SetupEditEnvironment(pWS, pCadFabric, pEd, out bIsFileBasedGDB,
          out bIsUnVersioned, out bUseNonVersioned))
          return;

        if (bUseNonVersioned)
        {
          ICadastralFabricLayer pCFLayer = new CadastralFabricLayerClass();
          pCFLayer.CadastralFabric = pCadFabric;
          pCadEd.CadastralFabricLayer = pCFLayer;//NOTE: Need to set this back to NULL when done.
        }

        Hashtable FabLyrToFieldMap = new Hashtable();
        DateChanger pDateChangerDialog = new DateChanger();
        pDateChangerDialog.cboBoxFabricClasses.Items.Clear();
        string[] FieldStrArr = new string[CFParcelLayers.Count];
        for (int i = 0; i < CFParcelLayers.Count; i++)
        {
          FieldStrArr[i] = "";
          IFeatureLayer lyr = (IFeatureLayer)CFParcelLayers.get_Element(i);
       //   ICadastralFabricLayer cflyr = CFParcelLayers.get_Element(i);
          
          pDateChangerDialog.cboBoxFabricClasses.Items.Add(lyr.Name);
          IFields pFlds = lyr.FeatureClass.Fields;
          for (int j = 0; j < lyr.FeatureClass.Fields.FieldCount; j++)
          {
            IField pFld = lyr.FeatureClass.Fields.get_Field(j);
            if (pFld.Type == esriFieldType.esriFieldTypeDate)
            {
              if (FieldStrArr[i].Trim() == "")
                FieldStrArr[i] = pFld.Name;
              else
                FieldStrArr[i] += "," + pFld.Name;
            }
          }
          FabLyrToFieldMap.Add(i, FieldStrArr[i]);
        }
        pDateChangerDialog.FieldMap = FabLyrToFieldMap;
        pDateChangerDialog.cboBoxFabricClasses.SelectedIndex = 0;



        // ********  Display the dialog  *********
        DialogResult pDialogResult = pDateChangerDialog.ShowDialog();
        if (pDialogResult != DialogResult.OK)
          return;
        //************************

        //*** get the choices from the dialog
        IFeatureLayer flyr = (IFeatureLayer)CFParcelLayers.get_Element(pDateChangerDialog.cboBoxFabricClasses.SelectedIndex);
        int iDateFld = flyr.FeatureClass.Fields.FindField(pDateChangerDialog.cboBoxFields.Text);

        if (pDateChangerDialog.radioButton2.Checked)
        {
          IField pFld = flyr.FeatureClass.Fields.get_Field(iDateFld);
          if (!pFld.IsNullable)
          {
            MessageBox.Show("The field you selected does not allow NULL values, and must have a date." + Environment.NewLine +
            "Please try again using the date option, or using a different date field.", "Field does not Allow Null", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;                   
          }
        }
        
        ICadastralFabricSubLayer pSubLyr = (ICadastralFabricSubLayer)flyr;
        bool bLines = false;
        bool bParcels = false;
        if (pSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLines)
          bLines = true;
        if (pSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTParcels)
          bParcels = true;

        //find out if there is a selection for the chosen layer
        bool ChosenLayerHasSelection = false;

        IFeatureSelection pFeatSel = null;
        ISelectionSet2 pSelSet = null;
        ICadastralSelection pCadaSel = null;
        IEnumGSParcels pEnumGSParcels = null;

        int iFeatureCnt = 0;
        pFeatSel = (IFeatureSelection)flyr;
        if (pFeatSel != null)
        {
          pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;
          ChosenLayerHasSelection = (pSelSet.Count > 0);
          iFeatureCnt = pSelSet.Count;
        }
        //****

        if (iFeatureCnt == 0)
        {
          if (MessageBox.Show("There are no features selected in the " + flyr.Name + " layer." + Environment.NewLine + "Click OK to Change dates for ALL features in the layer.", "No Selection",
            MessageBoxButtons.OKCancel) != DialogResult.OK)
            return;
        }
        else
        {
          pCadaSel = (ICadastralSelection)pCadEd;
          //** TODO: this enum should be based on the selected points, lines, or line-points
          pEnumGSParcels = pCadaSel.SelectedParcels;// need to get the parcels before trying to get the parcel count: BUG workaround
          //***
        }

        if (iFeatureCnt == 0)
        {
          m_pCursor = (ICursor)flyr.FeatureClass.Search(null, false);
          ITable pTable = (ITable)flyr.FeatureClass;
          iFeatureCnt = pTable.RowCount(null);
        }

        m_bShowProgressor = (iFeatureCnt > 10);
        if (m_bShowProgressor)
        {
          m_pProgressorDialogFact = new ProgressDialogFactoryClass();
          m_pTrackCancel = new CancelTrackerClass();
          m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
          m_pStepProgressor.MinRange = 1;
          m_pStepProgressor.MaxRange = iFeatureCnt;
          m_pStepProgressor.StepValue = 1;
          pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        }
        m_pQF = new QueryFilterClass();
        string sPref; string sSuff;

        ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
        sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
        sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

        //====== need to do this for all the parcel sublayers in the map that are part of the target fabric
        if (m_bShowProgressor)
        {
          pProgressorDialog.ShowDialog();
          m_pStepProgressor.Message = "Collecting data...";
        }

        bool bCont = true;
        m_pFIDSetParcels = new FIDSetClass();

        if (ChosenLayerHasSelection && bParcels && !bIsUnVersioned)
        {
          //if there is a selection add the OIDs of all the selected parcels into a new feature IDSet
          pEnumGSParcels.Reset();
          IGSParcel pGSParcel = pEnumGSParcels.Next();

          while (pGSParcel != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (m_bShowProgressor)
            {
              bCont = m_pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            m_pFIDSetParcels.Add(pGSParcel.DatabaseId);
            Marshal.ReleaseComObject(pGSParcel); //garbage collection
            pGSParcel = pEnumGSParcels.Next();
            //}
            if (m_bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
          }
        }

        if ((!ChosenLayerHasSelection && bParcels && !bIsUnVersioned) ||
            (!ChosenLayerHasSelection && bLines && !bIsUnVersioned))
        {
          IRow pRow = m_pCursor.NextRow();
          while (pRow != null)
          {
            m_pFIDSetParcels.Add(pRow.OID);
            Marshal.ReleaseComObject(pRow);
            pRow = m_pCursor.NextRow();
          }
          Marshal.ReleaseComObject(m_pCursor);
        }

        if (bLines && ChosenLayerHasSelection && !bIsUnVersioned)
        {
          pSelSet.Search(null, false, out m_pCursor);
          IRow pRow = m_pCursor.NextRow();
          int iFld = m_pCursor.FindField("PARCELID");
          while (pRow != null)
          {
            m_pFIDSetParcels.Add((int)pRow.get_Value(iFld));
            Marshal.ReleaseComObject(pRow);
            pRow = m_pCursor.NextRow();
          }
          Marshal.ReleaseComObject(m_pCursor);
        }

        //=========================================================
        if (!bCont)
        {
          //Since I'm using update cursor need to clear the cadastral selection
          pCadaSel.SelectedParcels = null;
          //clear selection, to make sure the parcel explorer is updated and refreshed properly
          return;
        }

        string sTime = "";
        if (!bIsUnVersioned)
        {
          //see if parcel locks can be obtained on the selected parcels. First create a job.
          DateTime localNow = DateTime.Now;
          sTime = Convert.ToString(localNow);
          ICadastralJob pJob = new CadastralJobClass();
          pJob.Name = sTime;
          pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
          pJob.Description = "Change Date on selected parcels";
          try
          {
            Int32 jobId = pCadFabric.CreateJob(pJob);
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
            m_pStepProgressor = null;
            if (!(pProgressorDialog == null))
              pProgressorDialog.HideDialog();
            pProgressorDialog = null;
            if (bUseNonVersioned)
              pCadEd.CadastralFabricLayer = null;
            return;
          }
        }

        //if we're in an enterprise then test for edit locks
        ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)pCadFabric;
        if (!bIsUnVersioned)
        {
          pFabLocks.LockingJob = sTime;
          ILongArray pLocksInConflict = null;
          ILongArray pSoftLcksInConflict = null;

          ILongArray pParcelsToLock = new LongArrayClass();

          UTILS.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, m_pStepProgressor);
          if (m_pStepProgressor != null && !bIsFileBasedGDB)
            m_pStepProgressor.Message = "Testing for edit locks on parcels...";

          try
          {
            pFabLocks.AcquireLocks(pParcelsToLock, true, ref pLocksInConflict, ref pSoftLcksInConflict);
          }
          catch (COMException pCOMEx)
          {
            if (pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_LOCK_ALREADY_EXISTS)
            {
              MessageBox.Show("Edit Locks could not be acquired on all selected parcels.");
              // since the operation is being aborted, release any locks that were acquired
              pFabLocks.UndoLastAcquiredLocks();
              //clear selection, to make sure the parcel explorer is updated and refreshed properly
              RefreshMap(pActiveView, CFParcelLayers, CFPointLayer, CFLineLayer, CFControlLayer, CFLinePointLayer);
            }
            else
            {
              MessageBox.Show(Convert.ToString(pCOMEx.ErrorCode));
            }

            if (bUseNonVersioned)
              pCadEd.CadastralFabricLayer = null;
            return;
          }
        }

        //Now... start the edit. Start an edit operation.
        if (pEd.EditState == esriEditState.esriStateEditing)
          pEd.StartOperation();

        if (bUseNonVersioned)
        {
          if (!UTILS.StartEditing(pWS, bUseNonVersioned))
          {
            if (bUseNonVersioned)
              pCadEd.CadastralFabricLayer = null;
            return;
          }
        }

        //Change all the date records
        if (m_pStepProgressor != null)
          m_pStepProgressor.Message = "Changing dates...";

        bool bSuccess = true;

        ITable Table2Edit = (ITable)flyr.FeatureClass;
        ITableWrite pTableWr = (ITableWrite)Table2Edit;

        if (ChosenLayerHasSelection)
          //TODO: Selection based update does not work on unversioned tables
          //need to change this code to create an update cursor from the selection, 
          //including code for tokens > 995
          pSelSet.Update(null, false, out m_pCursor);
        else
        {
          if (bUseNonVersioned)
          {
            m_pCursor = pTableWr.UpdateRows(null, false);
          }
          else
            m_pCursor = Table2Edit.Update(null, false);
        }
        ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        if (bLines)
          pSchemaEd.ReleaseReadOnlyFields(Table2Edit,
            esriCadastralFabricTable.esriCFTLines); //release safety-catch
        else if (bParcels)
          pSchemaEd.ReleaseReadOnlyFields(Table2Edit,
            esriCadastralFabricTable.esriCFTParcels); //release safety-catch
        else
        {
          pSchemaEd.ReleaseReadOnlyFields(Table2Edit,
            esriCadastralFabricTable.esriCFTPoints); //release safety-catch
          pSchemaEd.ReleaseReadOnlyFields(Table2Edit,
            esriCadastralFabricTable.esriCFTControl); //release safety-catch
          pSchemaEd.ReleaseReadOnlyFields(Table2Edit,
            esriCadastralFabricTable.esriCFTLinePoints); //release safety-catch
        }

        if (pDateChangerDialog.radioButton2.Checked)
          bSuccess = UTILS.ChangeDatesOnTable(m_pCursor, pDateChangerDialog.cboBoxFields.Text, "",
            bUseNonVersioned, m_pStepProgressor, m_pTrackCancel);
        else
          bSuccess = UTILS.ChangeDatesOnTable(m_pCursor, pDateChangerDialog.cboBoxFields.Text,
            pDateChangerDialog.dateTimePicker1.Text, bUseNonVersioned, m_pStepProgressor, m_pTrackCancel);

        if (!bSuccess)
        {
          if (!bIsUnVersioned)
            pFabLocks.UndoLastAcquiredLocks();
          if (bUseNonVersioned)
            UTILS.AbortEditing(pWS);
          else
            pEd.AbortOperation();
          //clear selection, to make sure the parcel explorer is updated and refreshed properly

          return;
        }

        if (pEd.EditState == esriEditState.esriStateEditing)
          pEd.StopOperation("Change Date");

        if (bUseNonVersioned)
          UTILS.StopEditing(pWS);

        if (bParcels)
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);
        else if (bLines)
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);
        else
        {
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints); //release safety-catch
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTControl); //release safety-catch
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLinePoints); //release safety-catch
        }

      }
      catch (Exception ex)
      {
        MessageBox.Show("Error:" + ex.Message);
        if (bUseNonVersioned)
          UTILS.AbortEditing(pWS);
        else
          pEd.AbortOperation();
      }
      finally
      {
        RefreshMap(pActiveView, CFParcelLayers, CFPointLayer, CFLineLayer, CFControlLayer, CFLinePointLayer);

        if (bUseNonVersioned)
        {
          pCadEd.CadastralFabricLayer = null;
          CFParcelLayers = null;
          CFPointLayer = null;
          CFLineLayer = null;
          CFControlLayer = null;
          CFLinePointLayer = null;
        }
        
        m_pStepProgressor = null;
        if (!(pProgressorDialog == null))
          pProgressorDialog.HideDialog();
        pProgressorDialog = null;
      }
    }

    //private bool ChangeDatesOnTable(ICursor pCursor, string FieldName, string sDate, bool Unversioned)
    //{
    //  bool bWriteNull = (sDate.Trim() == "");
    //  object dbNull = DBNull.Value;
    //  int FldIdx = pCursor.FindField(FieldName);
    //  int iSysEndDate = pCursor.FindField("systemenddate");
    //  int iHistorical = pCursor.FindField("historical");

    //  bool bIsSysEndDate=(FieldName.ToLower()=="systemenddate");
    //  bool bIsHistorical=(FieldName.ToLower()=="historical");
    //  bool bHasSysEndDateFld = (iSysEndDate > -1);
    //  bool bHasHistoricFld = (iHistorical > -1 );

    //  if (bWriteNull)
    //  {
    //    IField pfld=pCursor.Fields.get_Field(FldIdx);
    //    if (!pfld.IsNullable)
    //      return false;
    //  }
      
    //  DateTime localNow = DateTime.Now;

    //  IRow pRow = pCursor.NextRow();
    //  while (pRow != null)
    //  {
    //    if (bWriteNull)
    //    {
    //      if (bIsHistorical) //if writing null to Historical field, use 0 instead
    //        pRow.set_Value(FldIdx, 0);
    //      else
    //        pRow.set_Value(FldIdx, dbNull);

    //      if(bIsHistorical && bHasSysEndDateFld)   // if writing null to Historical field
    //        pRow.set_Value(iSysEndDate, dbNull);   // then also Null system end date
    //      if (bIsSysEndDate && bHasHistoricFld)     // if writing null to SystemEndDate field
    //        pRow.set_Value(iHistorical, 0);            //then set the Historical flag to false = 0
    //    }
    //    else
    //    {
    //      pRow.set_Value(FldIdx, sDate);
    //      if (bIsSysEndDate && bHasHistoricFld)     // if writing date to SystemEndDate field
    //        pRow.set_Value(iHistorical, 1);            //then set the Historical flag to true = 1
    //    }

    //    if (Unversioned)
    //      pCursor.UpdateRow(pRow);
    //    else
    //      pRow.Store();

    //    Marshal.ReleaseComObject(pRow);
    //    pRow = pCursor.NextRow();
    //  }
    //  Marshal.ReleaseComObject(pCursor);
    //  return true;
    //}

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

    protected override void OnUpdate()
    {
      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      this.Enabled = pEd.EditState != esriEditState.esriStateNotEditing;
    }

  }
}
