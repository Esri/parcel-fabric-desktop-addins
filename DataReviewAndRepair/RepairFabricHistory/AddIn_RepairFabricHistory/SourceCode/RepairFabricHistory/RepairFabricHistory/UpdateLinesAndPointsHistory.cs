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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;

namespace RepairFabricHistory
{
  public class UpdateLinesAndPointsHistory : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private IFeatureLayer CFPointLayer = null;
    private IFeatureLayer CFLineLayer = null;
    private IFeatureLayer CFControlLayer = null;
    private IFeatureLayer CFLinePointLayer = null;
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSetParcels;

    public UpdateLinesAndPointsHistory()
    {
    }

    protected override void OnClick()
    {
      #region Prepare for editing
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadExtMan;
      if (pCadPacMan.PacketOpen)
      {
        MessageBox.Show("This command cannot be used when there is an open job.\r\nPlease finish or discard the open job, and try again.",
          "Delete Selected Parcels");
        return;
      }

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

      bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedDelete = false;

      IActiveView pActiveView = ArcMap.Document.ActiveView;
      IMap pMap = pActiveView.FocusMap;
      ICadastralFabric pCadFabric = null;
      clsFabricUtils FabricUTILS = new clsFabricUtils();
      IProgressDialog2 pProgressorDialog = null;

      //if we're in an edit session then grab the target fabric
      if (pEd.EditState == esriEditState.esriStateEditing)
        pCadFabric = pCadEd.CadastralFabric;
      else
      {
        MessageBox.Show("Please start editing and try again.");
        return;
      }

      if (pCadFabric == null)
      {//find the first fabric in the map
        if (!FabricUTILS.GetFabricFromMap(pMap, out pCadFabric))
        {
          MessageBox.Show
            ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }

      IArray CFParcelLayers = new ArrayClass();

      if (!(FabricUTILS.GetFabricSubLayersFromFabric(pMap, pCadFabric, out CFPointLayer, out CFLineLayer,
          out CFParcelLayers, out CFControlLayer, out CFLinePointLayer)))
      {
        return; //no fabric sublayers available for the targeted fabric
      }


      IWorkspace pWS = null;
      ITable pParcelsTable = null;
      ITable pLinesTable = null;
      ITable pLinePtsTable = null;
      ITable pPointsTable = null;
      pParcelsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
      pLinesTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
      pLinePtsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLinePoints);
      pPointsTable = (ITable)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);

      #endregion

      #region Get Selection
      //Get the selection of parcels
      IFeatureLayer pFL = (IFeatureLayer)CFParcelLayers.get_Element(0);

      IDataset pDS = (IDataset)pFL.FeatureClass;
      pWS = pDS.Workspace;

      ICadastralSelection pCadaSel = (ICadastralSelection)pCadEd;
      IEnumGSParcels pEnumGSParcels = pCadaSel.SelectedParcels;// need to get the parcels before trying to get the parcel count: BUG workaround

      IFeatureSelection pFeatSel = (IFeatureSelection)pFL;
      ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;

      try
      {
        int iParcelCount = pCadaSel.SelectedParcelCount;
        bool m_bShowProgressor = (iParcelCount > 10);

        if (m_bShowProgressor)
        {
          m_pProgressorDialogFact = new ProgressDialogFactoryClass();
          m_pTrackCancel = new CancelTrackerClass();
          m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, ArcMap.Application.hWnd);
          pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
          m_pStepProgressor.MinRange = 1;
          m_pStepProgressor.MaxRange = iParcelCount * 14; //(estimate 7 lines per parcel, 4 pts per parcel)
          m_pStepProgressor.StepValue = 1;
          pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        }

        if (m_bShowProgressor)
        {
          pProgressorDialog.ShowDialog();
          m_pStepProgressor.Message = "Initializing...";
        }

      #endregion

        #region parcel table history fields
        //Get the parcel table history fields
        //SystemStart, SystemEnd, LegalStart, LegalEnd, Historic
        int iParcSysStartDate = pParcelsTable.FindField("systemstartdate");
        int iParcSysEndDate = pParcelsTable.FindField("systemenddate");
        int iParcLegalStartDate = pParcelsTable.FindField("legalstartdate");
        int iParcLegalEndDate = pParcelsTable.FindField("legalenddate");
        int iParcHistorical = pParcelsTable.FindField("historical");

        //Add the OIDs of all the selected parcels into a new feature IDSet
        //Need a Lookup for the History information
        Dictionary<int, string> ParcelToHistory_DICT = new Dictionary<int, string>(iParcelCount);
        int tokenLimit = 995;
        int iParcelCount2 = 0;
        int iListSize = (int)Math.Truncate(Convert.ToDouble(iParcelCount / tokenLimit));

        List<string> sOIDList = new List<string>(iListSize);
        sOIDList.Add("");

        bool bCont = true;
        int j = 0;
        int iCounter = 0;

        List<string> sOIDUpdateList = new List<string>(iListSize);
        sOIDUpdateList.Add("");
        int j2=0;
        int iCounter2 = 0;

        m_pFIDSetParcels = new FIDSetClass();

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
          if (iCounter <= tokenLimit)
          {
            if (sOIDList[j].Trim()=="")
              sOIDList[j] = Convert.ToString(pGSParcel.DatabaseId);
            else
              sOIDList[j] = sOIDList[j] + "," + Convert.ToString(pGSParcel.DatabaseId);
            iCounter++;
          }
          else
          {//maximum tokens reached
            iCounter = 0;
            //set up the next OIDList
            j++;
            sOIDList.Add("");
            sOIDList[j] = sOIDList[j] + Convert.ToString(pGSParcel.DatabaseId);
          }

          //add to the lookup
          IGSAttributes pGSParcelAttributes = (IGSAttributes)pGSParcel;
          object pObj = pGSParcelAttributes.GetProperty("systemstartdate");
          string sSystemStartParcel = "";
          if (pObj != null)
            sSystemStartParcel = pObj.ToString();

          pObj = pGSParcelAttributes.GetProperty("systemenddate");
          string sSystemEndParcel = "";
          if (pObj != null)
            sSystemEndParcel = pObj.ToString();

          string sLegalStartParcel = pGSParcel.LegalStartDate.ToString();
          string sLegalEndParcel = pGSParcel.LegalEndDate.ToString();
          string sHistorical = pGSParcel.Historical.ToString();

          DateTime localNow = DateTime.Now;
          string sDate = Convert.ToString(localNow);

          //need to do a consistency check on the systemenddate and historical flags.
          if (sHistorical.ToLower() == "true" && sSystemEndParcel == "")
          {
            sSystemEndParcel = sDate;
            if (iCounter2 <= tokenLimit)
            {
              if (sOIDUpdateList[j2].Trim() == "")
                sOIDUpdateList[j2] = Convert.ToString(pGSParcel.DatabaseId);
              else
                sOIDUpdateList[j2] = sOIDUpdateList[j2] + "," + Convert.ToString(pGSParcel.DatabaseId);
              iCounter2++;
            }
            else
            {//maximum tokens reached
              iCounter2 = 0;
              //set up the next OIDUpdateList
              j2++;
              sOIDUpdateList.Add("");
              sOIDUpdateList[j2] = sOIDUpdateList[j2] + Convert.ToString(pGSParcel.DatabaseId);
            }
            iParcelCount2++;
          }
          if (sHistorical.ToLower() == "false" && sSystemEndParcel != "")
          {
            sHistorical = "True";
            if (iCounter2 <= tokenLimit)
            {
              if (sOIDUpdateList[j2].Trim() == "")
                sOIDUpdateList[j2] = Convert.ToString(pGSParcel.DatabaseId);
              else
                sOIDUpdateList[j2] = sOIDUpdateList[j2] + "," + Convert.ToString(pGSParcel.DatabaseId);
              iCounter2++;
            }
            else
            {//maximum tokens reached
              iCounter2 = 0;
              //set up the next OIDUpdateList
              j2++;
              sOIDUpdateList.Add("");
              sOIDUpdateList[j2] = sOIDUpdateList[j2] + Convert.ToString(pGSParcel.DatabaseId);
            }
            iParcelCount2++;
          }

          ParcelToHistory_DICT.Add(pGSParcel.DatabaseId, sSystemStartParcel + "," +
            sSystemEndParcel + "," + sLegalStartParcel + "," + sLegalEndParcel + "," +
            sHistorical);

          Marshal.ReleaseComObject(pGSParcel); //garbage collection
          pGSParcel = pEnumGSParcels.Next();
          if (m_bShowProgressor)
          {
            if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
              m_pStepProgressor.Step();
          }
        }
        Marshal.ReleaseComObject(pEnumGSParcels); //garbage collection
        if (!bCont)
          return;
        #endregion

        if (!FabricUTILS.SetupEditEnvironment(pWS, pCadFabric, pEd, out bIsFileBasedGDB,
              out bIsUnVersioned, out bUseNonVersionedDelete))
          return;
        
        #region Confirm Edit Locks
        //if we're in an enterprise then test for edit locks
        string sTime = "";
        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          //see if parcel locks can be obtained on the selected parcels. First create a job.
          DateTime localNow = DateTime.Now;
          sTime = Convert.ToString(localNow);
          ICadastralJob pJob = new CadastralJobClass();
          pJob.Name = sTime;
          pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
          pJob.Description = "Update history on selected parcels";
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
            Marshal.ReleaseComObject(pJob);
            if (bUseNonVersionedDelete)
              pCadEd.CadastralFabricLayer = null;
            return;
          }
          Marshal.ReleaseComObject(pJob);
        }
        ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)pCadFabric;
        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          pFabLocks.LockingJob = sTime;
          ILongArray pLocksInConflict = null;
          ILongArray pSoftLcksInConflict = null;

          ILongArray pParcelsToLock = new LongArrayClass();

          FabricUTILS.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, m_pStepProgressor);
          if (m_bShowProgressor && !bIsFileBasedGDB)
            m_pStepProgressor.Message = "Testing for edit locks on parcels...";

          try
          {
            pFabLocks.AcquireLocks(pParcelsToLock, true, ref pLocksInConflict, ref pSoftLcksInConflict);
          }
          catch (COMException pCOMEx)
          {
            if (pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_LOCK_ALREADY_EXISTS ||
              pCOMEx.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_CURRENTLY_EDITED)
            {
              MessageBox.Show("Edit Locks could not be acquired on all selected parcels.");
              // since the operation is being aborted, release any locks that were acquired
              pFabLocks.UndoLastAcquiredLocks();
            }
            else
              MessageBox.Show(pCOMEx.Message + Environment.NewLine + Convert.ToString(pCOMEx.ErrorCode));

            if (bUseNonVersionedDelete)
              pCadEd.CadastralFabricLayer = null;
            return;
          }
          Marshal.ReleaseComObject(pSoftLcksInConflict);
          Marshal.ReleaseComObject(pParcelsToLock);
          Marshal.ReleaseComObject(pLocksInConflict);
        }
        #endregion

        if (m_bShowProgressor)
        {
          pProgressorDialog.ShowDialog();
          m_pStepProgressor.Message = "Updating parcel history...";
        }

        pEd.StartOperation();

        #region The Edit
        //update the parcels that have inconsistencies in Historic/SystemEndDateFields
        if (!FabricUTILS.UpdateInconsistentHistoryOnParcels(pParcelsTable, iParcelCount2, pCadFabric,
          sOIDUpdateList, ParcelToHistory_DICT, m_pStepProgressor, m_pTrackCancel))
        {
          pEd.AbortOperation();
          return;
        }
        
        //update the lines and points
        if (!FabricUTILS.UpdateHistoryOnLines(pLinesTable, pPointsTable, iParcelCount,
          pCadFabric, sOIDList, ParcelToHistory_DICT, m_pStepProgressor, m_pTrackCancel))
        {
          pEd.AbortOperation();
          return;
        }

        #endregion

        pEd.StopOperation("Update Line and Point History");
      }
      
      catch (Exception ex)
      {
        pEd.AbortOperation();
        MessageBox.Show("Error:" + ex.Message);
      }
      finally
      {
        RefreshMap(pActiveView, CFParcelLayers, CFPointLayer, CFLineLayer, CFControlLayer, CFLinePointLayer);
        if (!(pProgressorDialog == null))
        {
          pProgressorDialog.HideDialog();
        }
      }
    }

    protected override void OnUpdate()
    {
      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      this.Enabled = pEd.EditState!=esriEditState.esriStateNotEditing;
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

  }
}
