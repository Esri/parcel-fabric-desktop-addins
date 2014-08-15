/*
 Copyright 1995-2012 Esri

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

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.CadastralUI;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RepairFabricHistory
{
  class clsFabricUtils
  {
    int m_LastErrorCode = 0;

    public int LastErrorCode
    {
      get
      {
        return m_LastErrorCode;
      }
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
          "\r\n Click 'OK' to make changes permanent.",
          "Continue?", MessageBoxButtons.OKCancel);
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

    public void GetFabricPlatform(IWorkspace TheWorkspace, ICadastralFabric TheFabric,
      out bool IsFileBasedGDB, out bool IsUnVersioned)
    {
      IsFileBasedGDB = false;
      IsUnVersioned = false;

      ITable pTable = TheFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);

      IsFileBasedGDB = (!(TheWorkspace.WorkspaceFactory.WorkspaceType == esriWorkspaceType.esriRemoteDatabaseWorkspace));

      if (!(IsFileBasedGDB))
      {
        IVersionedObject pVersObj = (IVersionedObject)pTable;
        IsUnVersioned = (!(pVersObj.IsRegisteredAsVersioned));
        pTable = null;
        pVersObj = null;
      }
    }

    public void FIDsetToLongArray(IFIDSet InFIDSet, ref ILongArray OutLongArray, IStepProgressor StepProgressor)
    {
      Int32 pfID = -1;
      InFIDSet.Reset();
      double dMax = InFIDSet.Count();
      int iMax = (int)(dMax);
      for (Int32 pCnt = 0; pCnt <= (InFIDSet.Count() - 1); pCnt++)
      {
        InFIDSet.Next(out pfID);
        OutLongArray.Add(pfID);
        if (StepProgressor != null)
        {
          if (StepProgressor.Position < StepProgressor.MaxRange)
            StepProgressor.Step();
        }
      }
      return;
    }

    public bool GetFabricSubLayersFromFabric(IMap Map, ICadastralFabric Fabric, out IFeatureLayer CFPointLayer, out IFeatureLayer CFLineLayer,
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
            Debug.WriteLine(pParcelLayer.Name);
            CFControlLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRControlPoints);
            CFLinePointLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLinePoints);
          }
          
          if (CFLinePointLayer != null)
            CFParcelLayers2.Add(CFLinePointLayer);

          if (CFControlLayer != null)
            CFParcelLayers2.Add(CFControlLayer);

          if (CFLineLayer != null)
            CFParcelLayers2.Add(CFLineLayer);

          if (CFPointLayer != null)
            CFParcelLayers2.Add(CFPointLayer);

          CFParcelLayers = CFParcelLayers2;
          return true;
        }
      }
      //at the minimum, just need to make sure we have a parcel sublayer for the requested fabric
      if (pParcelLayer != null)
      {
        if (CFLinePointLayer != null)
          CFParcelLayers2.Add(CFLinePointLayer);

        if (CFControlLayer != null)
          CFParcelLayers2.Add(CFControlLayer);

        if (CFLineLayer != null)
          CFParcelLayers2.Add(CFLineLayer);

        if (CFPointLayer != null)
          CFParcelLayers2.Add(CFPointLayer);

        CFParcelLayers = CFParcelLayers2;
        return true;
      }
      else
      {
        CFParcelLayers = null;
        return false;
      }
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
          pCFLayer = (ICadastralFabricLayer)Layer;
          Fabric = pCFLayer.CadastralFabric;

          //if (pCFLayer != null)
          //{
          // do {}
          // while (Marshal.FinalReleaseComObject(pCFLayer)>0);
          //}

          return Fabric;

        }
        catch
        {
          //if it failed then try it as a fabric sublayer
          try
          {
            pCFSubLyr = (ICadastralFabricSubLayer)Layer;
            Fabric = pCFSubLyr.CadastralFabric;

            //if (pCFSubLyr != null)
            //{
            //  do { }
            //  while (Marshal.FinalReleaseComObject(pCFSubLyr) > 0);
            //}

            return Fabric;
          }
          catch
          {
            continue;//cast failed...not a fabric sublayer
          }
        }
      }
      return Fabric;
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
        else
        {
          if (pLayer != null)
            Marshal.FinalReleaseComObject(pLayer);
        }
      }
      return false;
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

    public bool StopEditing(IWorkspace TheWorkspace)
    {
      IWorkspaceEdit pWSEdit = (IWorkspaceEdit)TheWorkspace;
      pWSEdit.StopEditOperation();
      pWSEdit.EnableUndoRedo();
      pWSEdit.StopEditing(true);
      return true;
    }

    public bool AbortEditing(IWorkspace TheWorkspace)
    {
      IWorkspaceEdit pWSEdit = (IWorkspaceEdit)TheWorkspace;
      pWSEdit.AbortEditOperation();
      pWSEdit.EnableUndoRedo();
      if (pWSEdit.IsBeingEdited())
        pWSEdit.StopEditing(false);
      return true;
    }

    public IFeatureLayer[] RedimPreserveFLyr(ref IFeatureLayer[] x, long ResizeIncrement)
    {
      IFeatureLayer[] Temp1 = new FeatureLayerClass[x.GetLength(0) + ResizeIncrement];
      if (x != null)
        System.Array.Copy(x, Temp1, x.Length);
      x = Temp1;
      Temp1 = null;
      return x;
    }

    public bool GetJobAndLocks(ICadastralFabric Fabric, IFIDSet FIDSetParcels, IStepProgressor StepProgressor)
    {
      string sTime = "";
      bool bIsUnVersioned = false;
      if (!bIsUnVersioned)
      {
        //see if parcel locks can be obtained on the selected parcels. First create a job.
        DateTime localNow = DateTime.Now;
        sTime = Convert.ToString(localNow);
        ICadastralJob pJob = new CadastralJobClass();
        pJob.Name = sTime;
        pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
        pJob.Description = "Change selected parcels";
        try
        {
          Int32 jobId = Fabric.CreateJob(pJob);
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
          Marshal.ReleaseComObject(pJob);
          return false;
        }
        Marshal.ReleaseComObject(pJob);
      }

      //if we're in an enterprise then test for edit locks
      ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)Fabric;
      if (!bIsUnVersioned)
      {
        pFabLocks.LockingJob = sTime;
        ILongArray pLocksInConflict = null;
        ILongArray pSoftLcksInConflict = null;

        ILongArray pParcelsToLock = new LongArrayClass();

        FIDsetToLongArray(FIDSetParcels, ref pParcelsToLock, StepProgressor);
        if (StepProgressor != null)
          StepProgressor.Message = "Testing for edit locks on parcels...";

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
          }
          else
          {
            MessageBox.Show(Convert.ToString(pCOMEx.ErrorCode));
          }
          return false;
        }

        Marshal.ReleaseComObject(pSoftLcksInConflict);
        Marshal.ReleaseComObject(pParcelsToLock);
        Marshal.ReleaseComObject(pLocksInConflict);
        return true;
      }
      return false;
    }

    public void ExecuteCommand(string CommUID)
    {
      ICommandBars pCommBars = ArcMap.Application.Document.CommandBars;
      IUID pIDComm = new UIDClass();
      pIDComm.Value = CommUID;
      ICommandItem pCommItem = pCommBars.Find(pIDComm);
      pCommItem.Execute();
      Marshal.ReleaseComObject(pIDComm);
    }

    private void WriteToRegistry(string Path, string Name, string KeyValue)
    {
      RegistryKey regKeyAppRoot = Registry.CurrentUser.CreateSubKey(Path);
      regKeyAppRoot.SetValue(Name, KeyValue);
      return;
    }

    public string RegValue(RegistryHive Hive, string Key, string ValueName)
    {
      string sAns = "";
      RegistryKey objParent = null;
      if (Hive == RegistryHive.ClassesRoot)
        objParent = Registry.ClassesRoot;

      if (Hive == RegistryHive.CurrentConfig)
        objParent = Registry.CurrentConfig;

      if (Hive == RegistryHive.CurrentUser)
        objParent = Registry.CurrentUser;

      if (Hive == RegistryHive.DynData)
        objParent = Registry.DynData;

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
        if (objSubKey != null)
          sAns = (string)(objSubKey.GetValue(ValueName));
      }
      return sAns;
    }

    public bool UpdateInconsistentHistoryOnParcels(ITable pParcelsTable, int iParcelCount, 
      ICadastralFabric pCadFabric, List<string> sOIDUpdateList, 
      Dictionary<int, string> ParcelToHistory_DICT, 
      IStepProgressor m_pStepProgressor, ITrackCancel m_pTrackCancel)
    {
      bool m_bShowProgressor = (iParcelCount > 10);
      bool bCont = true;
      ICadastralFabricSchemaEdit2 pSchemaEd=null;
      try
      {
      #region update line table history
        //Get the line table history fields
        //SystemStart, SystemEnd, LegalStart, LegalEnd, Historic

        int iParcelSysEndDate = pParcelsTable.FindField("systemenddate");
        string sParcelSysEndDate = pParcelsTable.Fields.get_Field(iParcelSysEndDate).Name;

        int iParcelHistorical = pParcelsTable.FindField("historical");
        string sParcelHistorical = pParcelsTable.Fields.get_Field(iParcelHistorical).Name;

        IQueryFilter pQueryFilter = new QueryFilterClass();
        pQueryFilter.SubFields = pParcelsTable.OIDFieldName + "," + sParcelSysEndDate + ","
          + sParcelHistorical;

        pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        pSchemaEd.ReleaseReadOnlyFields(pParcelsTable, esriCadastralFabricTable.esriCFTParcels); //release safety-catch

        ICursor pCur = null;
        object obj = null;
        foreach (string sOIDSet in sOIDUpdateList)
        {
          if (sOIDSet.Trim() == "")
            continue;

          pQueryFilter.WhereClause = pParcelsTable.OIDFieldName + " IN (" + sOIDSet + ")";
          pCur = pParcelsTable.Update(pQueryFilter, false);
          IRow pParcel = pCur.NextRow();
          while (pParcel != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (m_bShowProgressor)
            {
              bCont = m_pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            string sParcHistory = "";
            if (ParcelToHistory_DICT.TryGetValue((int)pParcel.OID, out sParcHistory))
            {
              string[] sHistoryItems = sParcHistory.Split(',');

              if (sHistoryItems[1].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[1];
              pParcel.set_Value(iParcelSysEndDate, obj);

              if (sHistoryItems[4].Trim() == "")
                obj = DBNull.Value;
              else
              {
                bool x = (sHistoryItems[4].Trim().ToLower() == "true") ? true : false;
                if (x)
                  obj = 1;
                else
                  obj = 0;
              }
              pParcel.set_Value(iParcelHistorical, obj);
              pParcel.Store();

            }
            Marshal.ReleaseComObject(pParcel);
            pParcel = pCur.NextRow();
            if (m_bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
          }
          Marshal.FinalReleaseComObject(pCur);
        }
        if (!bCont)
          return false;
        else
          return true;
      #endregion
      }
      catch (Exception ex)
      { 
        MessageBox.Show(ex.Message);
        return false;
      }
      finally
      {
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);//set safety back on    
      }
    }

    public bool UpdateHistoryOnLines(ITable pLinesTable, ITable pPointsTable,
         int iParcelCount, ICadastralFabric pCadFabric, List<string> sOIDList, 
      Dictionary<int, string> ParcelToHistory_DICT, IStepProgressor m_pStepProgressor, ITrackCancel m_pTrackCancel)
    {
      bool m_bShowProgressor = (iParcelCount > 10);
      sOIDList.Add("");
      int tokenLimit = 995;
      bool bCont = true;
      int j = 0;
      int iCounter = 0;
      ICadastralFabricSchemaEdit2 pSchemaEd = null;

      try
      {
        #region update line table history
        //Get the line table history fields
        //SystemStart, SystemEnd, LegalStart, LegalEnd, Historic
        int iParcelID = pLinesTable.FindField("parcelid");
        string sParcelID = pLinesTable.Fields.get_Field(iParcelID).Name;

        int iLineSysStartDate = pLinesTable.FindField("systemstartdate");
        string sLineSysStartDate = pLinesTable.Fields.get_Field(iLineSysStartDate).Name;

        int iLineSysEndDate = pLinesTable.FindField("systemenddate");
        string sLineSysEndDate = pLinesTable.Fields.get_Field(iLineSysEndDate).Name;

        int iLineLegalStartDate = pLinesTable.FindField("legalstartdate");
        string sLineLegalStartDate = pLinesTable.Fields.get_Field(iLineLegalStartDate).Name;

        int iLineLegalEndDate = pLinesTable.FindField("legalenddate");
        string sLineLegalEndDate = pLinesTable.Fields.get_Field(iLineLegalEndDate).Name;

        int iLineHistorical = pLinesTable.FindField("historical");
        string sLineHistorical = pLinesTable.Fields.get_Field(iLineHistorical).Name;

        int iFromPoint = pLinesTable.FindField("frompointid");
        string sFromPoint = pLinesTable.Fields.get_Field(iFromPoint).Name;

        int iToPoint = pLinesTable.FindField("topointid");
        string sToPoint = pLinesTable.Fields.get_Field(iToPoint).Name;

        IQueryFilter pQueryFilter = new QueryFilterClass();
        pQueryFilter.SubFields = pLinesTable.OIDFieldName + ", parcelid," + sLineSysStartDate +
          "," + sLineSysEndDate + "," + sLineLegalStartDate + "," + sLineLegalEndDate +
          "," + sLineHistorical + "," + sFromPoint + "," + sToPoint;

        pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        pSchemaEd.ReleaseReadOnlyFields(pLinesTable, esriCadastralFabricTable.esriCFTLines); //release safety-catch

        Dictionary<int, string> PointToHistory_DICT = new Dictionary<int, string>(iParcelCount);

        List<string> sPointOIDList = new List<string>();
        List<int> iLinesOIDList = new List<int>();
        sPointOIDList.Add("");
        j = iCounter = 0;
        ICursor pCur = null;
        object obj = null;
        foreach (string sHistory in sOIDList)
        {
          if (sHistory.Trim() == "")
            continue;

          pQueryFilter.WhereClause = sParcelID + " IN (" + sHistory + ")";
          pCur = pLinesTable.Update(pQueryFilter, false);
          IRow pLine = pCur.NextRow();
          while (pLine != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (m_bShowProgressor)
            {
              bCont = m_pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            iLinesOIDList.Add(pLine.OID);
            string sParcHistory = "";
            if (ParcelToHistory_DICT.TryGetValue((int)pLine.get_Value(iParcelID), out sParcHistory))
            {
              string[] sHistoryItems = sParcHistory.Split(',');
              if (sHistoryItems[0].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[0];
              pLine.set_Value(iLineSysStartDate, obj);

              if (sHistoryItems[1].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[1];
              pLine.set_Value(iLineSysEndDate, obj);

              if (sHistoryItems[2].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[2];
              pLine.set_Value(iLineLegalStartDate, obj);

              if (sHistoryItems[3].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[3];
              pLine.set_Value(iLineLegalEndDate, obj);

              if (sHistoryItems[4].Trim() == "")
                obj = DBNull.Value;
              else
              {
                bool x = (sHistoryItems[4].Trim().ToLower() == "true") ? true : false;
                if (x)
                  obj = 1;
                else
                  obj = 0;
              }
              pLine.set_Value(iLineHistorical, obj);
              pLine.Store();

              int iVal = (int)pLine.get_Value(iToPoint);
              if (!PointToHistory_DICT.ContainsKey(iVal))
              {
                PointToHistory_DICT.Add(iVal, sParcHistory);

                if (iCounter <= tokenLimit)
                {
                  if (sPointOIDList[j].Trim() == "")
                    sPointOIDList[j] = Convert.ToString(iVal);
                  else
                    sPointOIDList[j] = sPointOIDList[j] + "," + Convert.ToString(iVal);
                  iCounter++;
                }
                else
                {//maximum tokens reached
                  iCounter = 0;
                  //set up the next OIDList
                  j++;
                  sPointOIDList.Add("");
                  sPointOIDList[j] = sPointOIDList[j] + Convert.ToString(iVal);
                }
              }
              else //if the point is here already
              {
                //Since the lines that have the shared points may have different
                //history these points need a different treatment. The approach in this code will make updates 
                //in favour of non-historic data.
                UpdateHistoryOnPoints(pLine, iVal, PointToHistory_DICT, iLineSysEndDate,
                  iLineLegalEndDate, iLineHistorical);
              }

              iVal = (int)pLine.get_Value(iFromPoint);
              if (!PointToHistory_DICT.ContainsKey(iVal))
              {
                PointToHistory_DICT.Add(iVal, sParcHistory);

                if (iCounter <= tokenLimit)
                {
                  if (sPointOIDList[j].Trim() == "")
                    sPointOIDList[j] = Convert.ToString(iVal);
                  else
                    sPointOIDList[j] = sPointOIDList[j] + "," + Convert.ToString(iVal);
                  iCounter++;
                }
                else
                {//maximum tokens reached
                  iCounter = 0;
                  //set up the next OIDList
                  j++;
                  sPointOIDList.Add("");
                  sPointOIDList[j] = sPointOIDList[j] + Convert.ToString(iVal);
                }
              }
              else //if the point is here already
              {
                //Since the lines that have the shared points may have different
                //history these points need a different treatment. The approach in this code will make updates 
                //in favour of non-historic data.
                UpdateHistoryOnPoints(pLine, iVal, PointToHistory_DICT, iLineSysEndDate,
                  iLineLegalEndDate, iLineHistorical);
              }
            }
            Marshal.ReleaseComObject(pLine);
            pLine = pCur.NextRow();
            if (m_bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
          }
          Marshal.FinalReleaseComObject(pCur);
        }
        if (!bCont)
          return false;
        #endregion
        #region Find other lines connected to these points and Update the dictionary values
        //search back on lines with points list
        iLinesOIDList.Sort();
        foreach (string sPointsQuery in sPointOIDList)
        {
          if (sPointsQuery.Trim() == "")
            continue;
          pQueryFilter.WhereClause = sToPoint + " IN (" + sPointsQuery + ")";
          pCur = pLinesTable.Search(pQueryFilter, false);
          IRow pLine = pCur.NextRow();
          while (pLine != null)
          {
            int iPos = iLinesOIDList.BinarySearch(pLine.OID);
            if (iPos < 0) //not found < 0
            {//if this line is not in the original line list, its points are shared outside of 
              //the original selection. Since the lines that have the shared points may have different
              //history these points need a different treatment. The approach in this code will make updates 
              //in favour of non-historic parcels.
              int iVal = (int)pLine.get_Value(iFromPoint);
              UpdateHistoryOnPoints(pLine, iVal, PointToHistory_DICT, iLineSysEndDate,
                  iLineLegalEndDate, iLineHistorical);
              iVal = (int)pLine.get_Value(iToPoint);
              UpdateHistoryOnPoints(pLine, iVal, PointToHistory_DICT, iLineSysEndDate,
                  iLineLegalEndDate, iLineHistorical);
            }
            Marshal.FinalReleaseComObject(pLine);
            pLine = pCur.NextRow();
          }
          Marshal.FinalReleaseComObject(pCur);

          //Now redo the same search with the From point. These are separate searches because using OR with 
          //2 separate in clauses is slow.

          pQueryFilter.WhereClause = sFromPoint + " IN (" + sPointsQuery + ")";
          pCur = pLinesTable.Search(pQueryFilter, false);
          pLine = pCur.NextRow();
          while (pLine != null)
          {
            int iPos = iLinesOIDList.BinarySearch(pLine.OID);
            if (iPos < 0) //not found < 0
            {//if this line is not in the original list, its points are shared outside of
              //the original selection and should be removed from the point update list
              int iVal = (int)pLine.get_Value(iFromPoint);
              UpdateHistoryOnPoints(pLine, iVal, PointToHistory_DICT, iLineSysEndDate,
                  iLineLegalEndDate, iLineHistorical);
              iVal = (int)pLine.get_Value(iToPoint);
              UpdateHistoryOnPoints(pLine, iVal, PointToHistory_DICT, iLineSysEndDate,
                  iLineLegalEndDate, iLineHistorical);
            }
            Marshal.FinalReleaseComObject(pLine);
            pLine = pCur.NextRow();
          }
          Marshal.FinalReleaseComObject(pCur);
        }
        #endregion
        #region Update the Points
        //update the points with the values in the dictionary.
        pSchemaEd.ReleaseReadOnlyFields(pPointsTable, esriCadastralFabricTable.esriCFTPoints);
        //declare the smaller points list
        List<string> sPointOIDSubsetList = new List<string>();
        sPointOIDSubsetList.Add("");
        iCounter = j = 0;

        foreach (KeyValuePair<int, String> entry in PointToHistory_DICT)
        {
          string s = entry.Key.ToString();
          if (iCounter <= tokenLimit)
          {
            if (sPointOIDSubsetList[j].Trim() == "")
              sPointOIDSubsetList[j] = s;
            else
              sPointOIDSubsetList[j] = sPointOIDSubsetList[j] + "," + s;
            iCounter++;
          }
          else
          {//maximum tokens reached
            iCounter = 0;
            //set up the next OIDList
            j++;
            sPointOIDSubsetList.Add("");
            sPointOIDSubsetList[j] = sPointOIDSubsetList[j] + s;
          }
        }

        //Get the point table history fields
        //SystemStart, SystemEnd, LegalStart, LegalEnd, Historic
        int iPointSysStartDate = pPointsTable.FindField("systemstartdate");
        string sPointSysStartDate = pPointsTable.Fields.get_Field(iPointSysStartDate).Name;

        int iPointSysEndDate = pPointsTable.FindField("systemenddate");
        string sPointSysEndDate = pPointsTable.Fields.get_Field(iPointSysEndDate).Name;

        int iPointLegalStartDate = pPointsTable.FindField("legalstartdate");
        string sPointLegalStartDate = pPointsTable.Fields.get_Field(iPointLegalStartDate).Name;

        int iPointLegalEndDate = pPointsTable.FindField("legalenddate");
        string sPointLegalEndDate = pPointsTable.Fields.get_Field(iPointLegalEndDate).Name;

        int iPointHistorical = pPointsTable.FindField("historical");
        string sPointHistorical = pPointsTable.Fields.get_Field(iPointHistorical).Name;

        string sOIDFld = pPointsTable.OIDFieldName;
        pQueryFilter.SubFields = sOIDFld + "," + sPointSysStartDate +
          "," + sPointSysEndDate + "," + sPointLegalStartDate + "," + iPointLegalEndDate +
          "," + sPointHistorical;

        foreach (string sPointsQuery in sPointOIDSubsetList)
        {
          if (sPointsQuery.Trim() == "")
            continue;
          pQueryFilter.WhereClause = sOIDFld + " IN (" + sPointsQuery + ")";
          pCur = pPointsTable.Update(pQueryFilter, false);
          IRow pPoint = pCur.NextRow();
          while (pPoint != null)
          {
            //Check if the cancel button was pressed. If so, stop process   
            if (m_bShowProgressor)
            {
              bCont = m_pTrackCancel.Continue();
              if (!bCont)
                break;
            }
            string sPointHistory = "";
            if (PointToHistory_DICT.TryGetValue((int)pPoint.OID, out sPointHistory))
            {
              string[] sHistoryItems = sPointHistory.Split(',');
              if (sHistoryItems[0].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[0];
              pPoint.set_Value(iPointSysStartDate, obj);

              if (sHistoryItems[1].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[1];
              pPoint.set_Value(iPointSysEndDate, obj);

              if (sHistoryItems[2].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[2];
              pPoint.set_Value(iPointLegalStartDate, obj);

              if (sHistoryItems[3].Trim() == "")
                obj = DBNull.Value;
              else
                obj = sHistoryItems[3];
              pPoint.set_Value(iPointLegalEndDate, obj);

              if (sHistoryItems[4].Trim() == "")
                obj = DBNull.Value;
              else
              {
                bool x = (sHistoryItems[4].Trim().ToLower() == "true") ? true : false;
                if (x)
                  obj = 1;
                else
                  obj = 0;
              }
              pPoint.set_Value(iPointHistorical, obj);
              pPoint.Store();
            }
            Marshal.ReleaseComObject(pPoint);
            pPoint = pCur.NextRow();
            if (m_bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
          }
          Marshal.FinalReleaseComObject(pCur);
          if (!bCont)
            return false;
          else
            return true;
        }
        #endregion
        if (!bCont)
          return false;
        else
          return true;
      }
      
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return false;
      }
      
      finally
      {
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPoints);//set safety back on    
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTLines);//set safety back on
      }   
    }

    private void UpdateHistoryOnPoints(IRow pLine, int PointOID, Dictionary<int, string> PointToHistory_DICT,
      int m_iLineSysEndDate, int m_iLineLegalEndDate, int m_iLineHistorical)
    {
      string sThisPointHistory = "";
      if (!PointToHistory_DICT.TryGetValue(PointOID, out sThisPointHistory))
        return;
      string[] sPointHistoryItems = sThisPointHistory.Split(',');

      //Compare SystemEndDates
      object obj2 = pLine.get_Value(m_iLineSysEndDate);
      if (obj2 == DBNull.Value && sPointHistoryItems[1].Trim() != "")
      {// the null on the line trumps the existing date value
        PointToHistory_DICT[PointOID] =
          sPointHistoryItems[0] + ",," + sPointHistoryItems[2] + "," +
          sPointHistoryItems[3] + "," + sPointHistoryItems[4];
      }

      //Compare LegalEndDates
      obj2 = pLine.get_Value(m_iLineLegalEndDate);
      if (obj2 == DBNull.Value && sPointHistoryItems[3].Trim() != "")
      {// the null on the line trumps the existing date value
        PointToHistory_DICT[PointOID] =
          sPointHistoryItems[0] + "," + sPointHistoryItems[1] + "," +
          sPointHistoryItems[2] + ",," + sPointHistoryItems[4];
      }

      //Compare Historical
      obj2 = pLine.get_Value(m_iLineHistorical);
      string sTrueFalse = "";
      if (obj2 != DBNull.Value)
      {
        sTrueFalse = obj2.ToString();
        ////if (sTrueFalse.Trim() == "0")
        ////  sTrueFalse = "false";
      }
      else
        sTrueFalse = "false";

      if ((sTrueFalse.ToLower() == "false") && (sPointHistoryItems[4].Trim() != "" ||
        sPointHistoryItems[4].Trim().ToLower() != "false"))
      {// the null on the line trumps the existing date value
        PointToHistory_DICT[PointOID] =
          sPointHistoryItems[0] + "," + sPointHistoryItems[1] + "," +
          sPointHistoryItems[2] + "," + sPointHistoryItems[3] + "," + "false";
      }

    }

    public bool ChangeDatesOnTable(ICursor pCursor, string FieldName, string sDate, bool Unversioned, 
      IStepProgressor m_pStepProgressor, ITrackCancel m_pTrackCancel)
    {
      bool bWriteNull = (sDate.Trim() == "");
      object dbNull = DBNull.Value;
      int FldIdx = pCursor.FindField(FieldName);
      int iSysEndDate = pCursor.FindField("systemenddate");
      int iHistorical = pCursor.FindField("historical");

      bool bIsSysEndDate = (FieldName.ToLower() == "systemenddate");
      bool bIsHistorical = (FieldName.ToLower() == "historical");
      bool bHasSysEndDateFld = (iSysEndDate > -1);
      bool bHasHistoricFld = (iHistorical > -1);

      bool bCont = true;
      bool m_bShowProgressor = (m_pStepProgressor != null);

      if (bWriteNull)
      {
        IField pfld = pCursor.Fields.get_Field(FldIdx);
        if (!pfld.IsNullable)
          return false;
      }

      DateTime localNow = DateTime.Now;

      IRow pRow = pCursor.NextRow();
      while (pRow != null)
      {
        //Check if the cancel button was pressed. If so, stop process   
        if (m_bShowProgressor)
        {
          bCont = m_pTrackCancel.Continue();
          if (!bCont)
            break;
        }

        if (bWriteNull)
        {
          if (bIsHistorical) //if writing null to Historical field, use 0 instead
            pRow.set_Value(FldIdx, 0);
          else
            pRow.set_Value(FldIdx, dbNull);

          if (bIsHistorical && bHasSysEndDateFld)   // if writing null to Historical field
            pRow.set_Value(iSysEndDate, dbNull);   // then also Null system end date
          if (bIsSysEndDate && bHasHistoricFld)     // if writing null to SystemEndDate field
            pRow.set_Value(iHistorical, 0);            //then set the Historical flag to false = 0
        }
        else
        {
          pRow.set_Value(FldIdx, sDate);
          if (bIsSysEndDate && bHasHistoricFld)     // if writing date to SystemEndDate field
            pRow.set_Value(iHistorical, 1);            //then set the Historical flag to true = 1
        }

        if (Unversioned)
          pCursor.UpdateRow(pRow);
        else
          pRow.Store();

        Marshal.ReleaseComObject(pRow);
        pRow = pCursor.NextRow();
        if (m_bShowProgressor)
        {
          if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
            m_pStepProgressor.Step();
        }
      }
      Marshal.ReleaseComObject(pCursor);
      if (!bCont)
        return false;
      return true;
    }

    public bool ChangeDatesOnTableMulti(ICursor pCursor, List<bool> HistorySettings, List<string> sDate, bool Unversioned, 
      Dictionary<int, string> ParcelToHistory_DICT,IStepProgressor m_pStepProgressor, ITrackCancel m_pTrackCancel)
    {
      bool bSystemEndDate_Clear = HistorySettings[0];
      bool bLegalStDate_Clear = HistorySettings[1];
      bool bLegalEndDate_Clear = HistorySettings[2];
      bool bSystemEndDate_Set = HistorySettings[3];
      bool bLegalStDate_Set = HistorySettings[4];
      bool bLegalEndDate_Set = HistorySettings[5];

      object dbNull = DBNull.Value;

      int FldIdxSystemEnd = pCursor.FindField("systemenddate");
      int iHistorical = pCursor.FindField("historical");
      int FldIdxLegalSt =pCursor.FindField("legalstartdate");
      int FldIdxLegalEnd = pCursor.FindField("legalenddate");
      bool bHasSysEndDateFld = (FldIdxSystemEnd > -1);
      bool bHasHistoricFld = (iHistorical > -1);
      bool bCont = true;
      bool m_bShowProgressor = (m_pStepProgressor!=null);

      IRow pRow = pCursor.NextRow();
      while (pRow != null)
      {
        //Check if the cancel button was pressed. If so, stop process   
        if (m_bShowProgressor)
        {
          bCont = m_pTrackCancel.Continue();
          if (!bCont)
            break;
        }

        string sHistoryInfo = "";
        string[] sUpdateDictionaryDates = null;
        bool bTryGetTrue = false;
        if (ParcelToHistory_DICT.TryGetValue(pRow.OID, out sHistoryInfo))
        {
          //update the strings in the dictionary
          sUpdateDictionaryDates = sHistoryInfo.Split(',');
          bTryGetTrue = true;
        } 

        if (bSystemEndDate_Set && bTryGetTrue)
        {
          pRow.set_Value(FldIdxSystemEnd, sDate[0]);
          if (bHasHistoricFld)                          // if writing date to SystemEndDate field
            pRow.set_Value(iHistorical, 1);             //then set the Historical flag to true = 1
          //update the dictionary
          //find the location of the System End Date
          string x= ParcelToHistory_DICT[pRow.OID];
          int i1 = x.IndexOf(",", 0);
          int i2 = x.IndexOf(",", i1+1);
          string s1= x.Remove(i1 + 1, i2 - i1);
          string s2=s1.Insert(i1 + 1, sDate[0] + ",");
          ParcelToHistory_DICT[pRow.OID] = s2;
        }

        if (bSystemEndDate_Clear && bTryGetTrue)
        {
          pRow.set_Value(FldIdxSystemEnd, dbNull);
          if (bHasHistoricFld)                          // if writing date to SystemEndDate field
            pRow.set_Value(iHistorical, 0);             //then set the Historical flag to true = 1

          //update the dictionary
          //find the location of the System End Date
          string x = ParcelToHistory_DICT[pRow.OID];
          int i1 = x.IndexOf(",", 0);
          int i2 = x.IndexOf(",", i1 + 1);
          string s1 = x.Remove(i1 + 1, i2 - i1);
          string s2 = s1.Insert(i1 + 1, ",");
          ParcelToHistory_DICT[pRow.OID] = s2;
        }

        if (bLegalStDate_Set && bTryGetTrue)
        {
          pRow.set_Value(FldIdxLegalSt, sDate[1]);
          //update the dictionary
          //find the location of the System End Date
          string x = ParcelToHistory_DICT[pRow.OID];
          int i1 = x.IndexOf(",", 0);
          int i2 = x.IndexOf(",", i1 + 1);
          int i3 = x.IndexOf(",", i2 + 1);
          string s1 = x.Remove(i2 + 1, i3 - i2);
          string s2 = s1.Insert(i2 + 1,sDate[1]  + ",");
          ParcelToHistory_DICT[pRow.OID] = s2;
        }

        if (bLegalStDate_Clear && bTryGetTrue)
        {
          pRow.set_Value(FldIdxLegalSt, dbNull);
          //update the dictionary
          //find the location of the System End Date
          string x = ParcelToHistory_DICT[pRow.OID];
          int i1 = x.IndexOf(",", 0);
          int i2 = x.IndexOf(",", i1 + 1);
          int i3 = x.IndexOf(",", i2 + 1);
          string s1 = x.Remove(i2 + 1, i3 - i2);
          string s2 = s1.Insert(i2 + 1, ",");
          ParcelToHistory_DICT[pRow.OID] = s2;
        }

        if (bLegalEndDate_Set && bTryGetTrue)
        {
          pRow.set_Value(FldIdxLegalEnd, sDate[2]);
          //update the dictionary
          //find the location of the System End Date
          string x = ParcelToHistory_DICT[pRow.OID];
          int i1 = x.IndexOf(",", 0);
          int i2 = x.IndexOf(",", i1 + 1);
          int i3 = x.IndexOf(",", i2 + 1);
          int i4 = x.IndexOf(",", i3 + 1);

          string s1 = x.Remove(i3 + 1, i4 - i3);
          string s2 = s1.Insert(i3 + 1, sDate[2] + ",");
          ParcelToHistory_DICT[pRow.OID] = s2;
        }

        if (bLegalEndDate_Clear && bTryGetTrue)
        {
          pRow.set_Value(FldIdxLegalEnd, dbNull);
          //update the dictionary
          //find the location of the System End Date
          string x = ParcelToHistory_DICT[pRow.OID];
          int i1 = x.IndexOf(",", 0);
          int i2 = x.IndexOf(",", i1 + 1);
          int i3 = x.IndexOf(",", i2 + 1);
          int i4 = x.IndexOf(",", i3 + 1);

          string s1 = x.Remove(i3 + 1, i4 - i3);
          string s2 = s1.Insert(i3 + 1, ",");
          ParcelToHistory_DICT[pRow.OID] = s2;
        }
       
        if (Unversioned)
          pCursor.UpdateRow(pRow);
        else
          pRow.Store();

        Marshal.ReleaseComObject(pRow);
        pRow = pCursor.NextRow();
        if (m_bShowProgressor)
        {
          if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
            m_pStepProgressor.Step();
        }
      }
      Marshal.ReleaseComObject(pCursor);
      if (!bCont)
        return false;
      return true;
    }
  }
}
