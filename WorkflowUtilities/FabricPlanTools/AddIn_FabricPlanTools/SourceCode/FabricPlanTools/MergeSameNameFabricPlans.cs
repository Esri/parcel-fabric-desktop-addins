/*
 Copyright 1995-2011 ESRI

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

using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CadastralUI;
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FabricPlanTools
{
  public class MergeSameNameFabricPlans : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    IApplication m_pApp;
    private ICadastralFabric m_pCadaFab;
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private ITime m_pEndTime;
    private ITime m_pStartTime;

    public MergeSameNameFabricPlans()
    {
    }

    protected override void OnClick()
    {
      m_pApp = (IApplication)ArcMap.Application;
      if (m_pApp == null)
        //if the app is null then could be running from ArcCatalog
        m_pApp = (IApplication)ArcCatalog.Application;

      if (m_pApp == null)
      {
        MessageBox.Show("Could not access the application.", "No Application found");
        return;
      }
      IGxApplication pGXApp = (IGxApplication)m_pApp;
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

      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);
      
      Utils FabricUTILS = new Utils();

      ITable pTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
      IDataset pDS = (IDataset)pTable;     
      IWorkspace pWS = pDS.Workspace;
      bool bIsFileBasedGDB = true;
      bool bIsUnVersioned = true;

      FabricUTILS.GetFabricPlatform(pWS, m_pCadaFab, out bIsFileBasedGDB,
        out bIsUnVersioned);

      //Do a Start and Stop editing to make sure we're not running in an edit session
      if (!FabricUTILS.StartEditing(pWS, true))
      {//if start editing fails then bail
        if (pUnk != null)
          Marshal.ReleaseComObject(pUnk);
        Cleanup(pMouseCursor, null, pTable, null, pWS, null);
        FabricUTILS = null;
        return;
      }
      FabricUTILS.StopEditing(pWS);
      IFIDSet pPlansToDelete = null;

      try
      {
        string[] SummaryNames = new string[0]; //define as dynamic array
        string[] RepeatPlans = new string[0]; //define as dynamic array
        ITable pPlansTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
        //load all the plan names into a string array
        m_pProgressorDialogFact = new ProgressDialogFactoryClass();
        m_pTrackCancel = new CancelTrackerClass();
        m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, m_pApp.hWnd);
        IProgressDialog2 pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
        int iRowCount = pPlansTable.RowCount(null);
        m_pStepProgressor.MinRange = 1;
        m_pStepProgressor.MaxRange = iRowCount * 2;
        m_pStepProgressor.StepValue = 1;
        pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
        pProgressorDialog.ShowDialog();
        m_pStepProgressor.Message = "Finding same-name plans to merge...";

        int iRepeatCnt = 0;

        if (!FindRepeatPlans(pPlansTable, out RepeatPlans, out SummaryNames, out iRepeatCnt))
        {
          pProgressorDialog.HideDialog();
          if (iRepeatCnt == 0)
          {
            MessageBox.Show("All plans in the fabric have unique names." +
               Environment.NewLine + "There are no plans to merge.", "Merge plans by name");
          }
          else
          {
            MessageBox.Show("There was a problem searching for repeat plans.", "Merge plans by name");
          }
          SummaryNames = null;
          RepeatPlans = null;
          Cleanup(pMouseCursor, null, pTable, pPlansTable, pWS, pProgressorDialog);
          return;
        }

        dlgMergeSameNamePlans TheSummaryDialog = new dlgMergeSameNamePlans();

        FillTheSummaryList(TheSummaryDialog, SummaryNames);
        
        DialogResult dResult = TheSummaryDialog.ShowDialog();

        if (dResult == DialogResult.Cancel)
        {
          pProgressorDialog.HideDialog();
          SummaryNames = null;
          RepeatPlans = null;
          Cleanup(pMouseCursor, null,pTable, pPlansTable, pWS, pProgressorDialog);
          pPlansTable = null;
          return;
        }

        //get the time now
        m_pStartTime = new TimeClass();
        m_pStartTime.SetFromCurrentLocalTime();

        Dictionary<int, int> Lookup = new Dictionary<int, int>();
        string[] InClause = new string[0]; //define as dynamic array

        m_pStepProgressor.Message = "Creating the merge query...";
        FabricUTILS.BuildSearchMapAndQuery(RepeatPlans, out Lookup, out InClause, out pPlansToDelete);

        ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
        ITable ParcelTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
        pSchemaEd.ReleaseReadOnlyFields(ParcelTable, esriCadastralFabricTable.esriCFTParcels); //release safety-catch

        if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
        {
          Cleanup(pMouseCursor, pPlansToDelete,pTable, pPlansTable, pWS, pProgressorDialog);
          InClause = null;
          Lookup.Clear();
          Lookup = null;
          return;
        }

        //setup progressor dialog for merge
        m_pStepProgressor.Message = "Moving parcels from source to target plans...";

        if (!FabricUTILS.MergePlans(ParcelTable, Lookup, InClause, bIsUnVersioned,m_pStepProgressor,m_pTrackCancel))
        {
          FabricUTILS.AbortEditing(pWS);
          pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);
          Cleanup(pMouseCursor, pPlansToDelete,pTable,pPlansTable, pWS,pProgressorDialog);
          InClause = null;
          Lookup.Clear();
          Lookup = null;
          return;
        }

        if (TheSummaryDialog.checkBox1.Checked)
        {
          //setup progressor dialog for Delete
          m_pStepProgressor.MaxRange = pPlansToDelete.Count();
          m_pStepProgressor.Message = "Deleting source plans...";        
          
          if (bIsUnVersioned)
          {
            if (!FabricUTILS.DeleteRowsUnversioned(pWS, pPlansTable, pPlansToDelete, m_pStepProgressor, m_pTrackCancel))
              Cleanup(pMouseCursor, pPlansToDelete,pTable,pPlansTable,pWS,pProgressorDialog);
          }
          else
          {
            if (!FabricUTILS.DeleteRowsByFIDSet(pPlansTable, pPlansToDelete, m_pStepProgressor, m_pTrackCancel))
              Cleanup(pMouseCursor, pPlansToDelete,pTable,pPlansTable, pWS,pProgressorDialog);
          }
        }

        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);

        m_pEndTime = new TimeClass();
        m_pEndTime.SetFromCurrentLocalTime();
        ITimeDuration HowLong = m_pEndTime.SubtractTime(m_pStartTime);

        m_pStepProgressor.Message = "["
          + HowLong.Hours.ToString("00") + "h "
          + HowLong.Minutes.ToString("00") + "m "
          + HowLong.Seconds.ToString("00") + "s]" + "  Saving changes...please wait.";
        
        FabricUTILS.StopEditing(pWS);
        Cleanup(pMouseCursor, pPlansToDelete,pTable,pPlansTable, pWS,pProgressorDialog);

      }
      catch (COMException ex)
      {
        MessageBox.Show(Convert.ToString(ex.ErrorCode));
        Cleanup(pMouseCursor, pPlansToDelete,pTable,null,pWS,null);
      }
    }

    void FillTheSummaryList(dlgMergeSameNamePlans TheSummary, string[] SummaryNames)
    {
      foreach (string s in SummaryNames)
      {
        string[] sItems = Regex.Split(s, "!_!");
        ListViewItem lstViewItm = new ListViewItem(sItems);  
        TheSummary.listView1.Items.Add(lstViewItm);
      }
    }

    bool FindRepeatPlans(ITable PlansTable, out string[] RepeatPlans, out string[] SummaryNames, out int RepeatCnt)
    {
      IFIDSet pNonEmptyPlansFIDSet = new FIDSetClass();
      ICursor pPlansCur = null;
      SummaryNames = null;
      RepeatPlans = null;
      RepeatCnt = 0;

      try
      {
        pPlansCur = PlansTable.Search(null, false);
        Int32 iPlanNameIDX = pPlansCur.Fields.FindField("NAME");
        IRow pPlanRow = pPlansCur.NextRow();
        //Create a collection of plan names

        if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
          m_pStepProgressor.Step();
        
        string[] sPlanNames = new string[0]; //define as dynamic array
        int iCount = 0;
        bool bCont = true;

        Utils FabricUTILS = new Utils();

        while (pPlanRow != null)
        {
          //Check if the cancel button was pressed. If so, stop process   
          bCont = m_pTrackCancel.Continue();
          if (!bCont)
          {
            RepeatCnt++;
            return false;
          }

          string sPlanNm = (string)pPlanRow.get_Value(iPlanNameIDX);
          if (sPlanNm.Trim() == "")
            sPlanNm = "<No Name>";
          FabricUTILS.RedimPreserveString(ref sPlanNames, 1);
          sPlanNames[iCount++] = sPlanNm.Trim() + ", OID:" + pPlanRow.OID;

          Marshal.ReleaseComObject(pPlanRow);
          pPlanRow = pPlansCur.NextRow();
          if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
            m_pStepProgressor.Step();
        }

        if (pPlanRow!=null)
          Marshal.ReleaseComObject(pPlanRow);
        if (pPlansCur != null)
          Marshal.ReleaseComObject(pPlansCur);

        System.Array.Sort<string>(sPlanNames);
        int m = -1;
        string sName_m;
        int k = -1;
        string sName_k;
        string sThisRepeat = "";
        bool bIsNewGroup = false;
        Dictionary<int, string> dictNameFromID = new Dictionary<int,string>();
        int iCnt = sPlanNames.GetLength(0) - 1;
        //Create a collection of repeat plan names
        string[] sRepeatPlans = new string[0]; //define as dynamic array
        FabricUTILS.RedimPreserveString(ref sRepeatPlans, 1);
        sRepeatPlans[0] = "";
        int idx = 0; // used with sRepeatPlans string array
        int iOID =-1;
        RepeatCnt = 0;
        for (int j = iCnt; j >= 0; j--)
        {
          //Check if the cancel button was pressed. If so, stop process   
          bCont = m_pTrackCancel.Continue();
          if (!bCont)
            return false;

          sName_m = "";
          sName_k = "";

          int l = sPlanNames[j].LastIndexOf(", OID:");
          string sName_l = sPlanNames[j].Trim().Substring(0, l);

          if (sName_l.ToUpper().Trim() == sThisRepeat.ToUpper().Trim())
            bIsNewGroup = false;
          else if (j < iCnt - 1)
            bIsNewGroup = true;

          if (j > 0)
          {
            k = sPlanNames[j - 1].LastIndexOf(", OID:");
            sName_k = sPlanNames[j - 1].Trim().Substring(0, k);
          }
          if (j < sPlanNames.GetLength(0)-1)
          {
            m = sPlanNames[j + 1].LastIndexOf(", OID:");
            sName_m = sPlanNames[j + 1].Trim().Substring(0, m);
          }

          sThisRepeat = sName_l;

          if (sName_l.ToUpper() == sName_k.ToUpper() ||
            sName_l.ToUpper() == sName_m.ToUpper())
          //If true then this is a repeated Plan
          {
            RepeatCnt++;
            int i = sPlanNames[j].LastIndexOf(", OID:");
            string sOID = sPlanNames[j].Trim().Substring(i + 6);
            iOID = Convert.ToInt32(sOID);
            try { dictNameFromID.Add(iOID, sName_l); }
            catch { }
            //test if we're in a new group and increment the array
            if (bIsNewGroup)
            {
              FabricUTILS.RedimPreserveString(ref sRepeatPlans, 1);
              idx++;
              sRepeatPlans[idx] = "";
            }

            if (!sRepeatPlans[idx].Contains("," + sOID + " "))
              sRepeatPlans[idx] += "," + sOID + " ";
            if (sName_l.ToUpper() == sName_k.ToUpper())
            {
              i = sPlanNames[j - 1].LastIndexOf(", OID:");
              sOID = sPlanNames[j - 1].Trim().Substring(i + 6);

              if (!sRepeatPlans[idx].Contains("," + sOID + " "))
                sRepeatPlans[idx] += "," + sOID + " ";
            }
            if (sName_l.ToUpper() == sName_m.ToUpper())
            {
              i = sPlanNames[j + 1].LastIndexOf(", OID:");
              sOID = sPlanNames[j + 1].Trim().Substring(i + 6);

              if (!sRepeatPlans[idx].Contains("," + sOID + " "))
                sRepeatPlans[idx] += "," + sOID + " ";
            }
          }
          else
            FabricUTILS.RemoveAt(ref sPlanNames, j);
          
          if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
            m_pStepProgressor.Step();
        }

        string[] sTemp = new string[sRepeatPlans.GetLength(0)];
        System.Array.Copy(sRepeatPlans, sTemp, sRepeatPlans.GetLength(0));

        if (sRepeatPlans.GetLength(0) ==1) //if there is only one repeat plan group
        {
          if (sRepeatPlans[0] == null) //test if there are no repeat plans
            return false;

          if (sRepeatPlans[0].Trim() == "") //test if there are no repeat plans
            return false;
        }
        
        //Define the summary array
        string[] sSummaryNames = new string[0];//create a string array for the summary list

        int z = 0;
        int h = 0;
        foreach (string s in sRepeatPlans)
        {
          if (s == null)
            continue;
          string sT2 = s.Replace(",", "").Trim();
          string[] sOID = sT2.Split(' ');
          int[] iOIDArr = new int[sOID.GetLength(0)];
          int zz = 0;
          foreach (string xx in sOID)
          {
            if (xx.Trim()!="")
              iOIDArr[zz++] = Convert.ToInt32(xx);
            else
              iOIDArr[zz++] = -1;
          }
          System.Array.Sort<int>(iOIDArr);
          zz = 0;
          foreach (int yy in iOIDArr)
            sOID[zz++] = Convert.ToString(yy);  

          string sFirstNameFound = "";
          int q =0;

          sTemp[z] = "";
          int iOID2 = Convert.ToInt32(sOID[0]);
          string ThisPlanName = "";
          if (iOID2 >= 0)
          {
            if (!dictNameFromID.TryGetValue(iOID2, out ThisPlanName))//if this is the first, it is the target plan
              return false;
          }
          sFirstNameFound = "!_!==>" + ThisPlanName + "   (id:" + sOID[0] + ")";          

          foreach (string s2 in sOID)
          {
            if (s2 == null)
              continue;
            if (s2.Trim() == "-1")
              continue;
            if (q > 0)
            {//if this is not the first plan then
              FabricUTILS.RedimPreserveString(ref sSummaryNames, 1); //makes space in the array
              if ( q < sOID.GetLength(0)-1)
                sSummaryNames[h] += ThisPlanName + "   (id:" + s2 + ")" + "!_!";
              else
                sSummaryNames[h] += ThisPlanName + "   (id:" + s2 + ")" + sFirstNameFound;
              h++;
            }
            sTemp[z] += "," + s2.Trim();
            q++;
          }
          FabricUTILS.RedimPreserveString(ref sSummaryNames, 1); //makes space in the array
          sSummaryNames[sSummaryNames.GetLength(0) - 1] = " !_! ";//add empty row as separator
          h++;

          sTemp[z] = sTemp[z].Replace(",", " ").Trim();
          sTemp[z] = sTemp[z].Replace(" ", ",").Trim();
          z++;
          sRepeatPlans = sTemp;
        }

        SummaryNames = sSummaryNames;
        RepeatPlans = sRepeatPlans;
        sTemp = null;
        FabricUTILS = null;

        dictNameFromID.Clear();
        dictNameFromID = null;

        if (pNonEmptyPlansFIDSet != null)
          Marshal.ReleaseComObject(pNonEmptyPlansFIDSet);

        return true;
      }

      catch (COMException ex)
      {
        if (pPlansCur != null)
          Marshal.ReleaseComObject(pPlansCur);
        if (pNonEmptyPlansFIDSet != null)
          Marshal.ReleaseComObject(pNonEmptyPlansFIDSet);
        MessageBox.Show(Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    private void Cleanup(IMouseCursor MouseCursor, IFIDSet PlansFIDSet, ITable pTable, 
      ITable PlansTable, IWorkspace Workspace,IProgressDialog2 pProgressorDialog)
    {
      if (pProgressorDialog!=null)
        pProgressorDialog.HideDialog();

      if (MouseCursor!=null)
        MouseCursor.SetCursor(0);
      if (m_pStepProgressor!=null)
        m_pStepProgressor.Hide();
      
      m_pStepProgressor = null;

      try
      {
        Marshal.ReleaseComObject(PlansFIDSet); //garbage collection
      }
      catch { }

      try
      {
        Marshal.ReleaseComObject(m_pProgressorDialogFact); //garbage collection
      }
      catch { }

      try
      {
        Marshal.ReleaseComObject(m_pTrackCancel); //garbage collection
      }
      catch { }

      if (PlansTable!= null)
        Marshal.ReleaseComObject(PlansTable);
      if (m_pCadaFab != null)
        Marshal.ReleaseComObject(m_pCadaFab);
      if (pTable != null)
        Marshal.ReleaseComObject(pTable);
      if (Workspace!= null)
        Marshal.ReleaseComObject(Workspace);
      if (m_pStartTime != null)
        Marshal.ReleaseComObject(m_pStartTime);
      if (m_pEndTime != null)
        Marshal.ReleaseComObject(m_pEndTime);
    }
  }

}
