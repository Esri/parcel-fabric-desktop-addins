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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FabricPlanTools
{
  public class FixParcelsWithNoPlan : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    IApplication m_pApp;
    private ICadastralFabric m_pCadaFab;
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;

    private int m_AngleUnits = 3;
    private int m_AreaUnits = 4;
    private int m_DistanceUnits = 9001;
    private int m_DirectionFormat = 4;
    private int m_LineParams = 4;

    public FixParcelsWithNoPlan()
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
        Cleanup(pMouseCursor, null, pTable, null, pWS, null, true);
        FabricUTILS = null;
        return;
      }
      FabricUTILS.StopEditing(pWS);

      ITable pPlansTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
      ITable pParcelsTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);

      m_pProgressorDialogFact = new ProgressDialogFactoryClass();
      m_pTrackCancel = new CancelTrackerClass();
      m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, m_pApp.hWnd);
      IProgressDialog2 pProgressorDialog = (IProgressDialog2)m_pStepProgressor;
      int iRowCount = pPlansTable.RowCount(null);
      int iRowCount2 = pParcelsTable.RowCount(null);
      m_pStepProgressor.MinRange = 1;
      m_pStepProgressor.MaxRange =iRowCount  + iRowCount2;
      m_pStepProgressor.StepValue = 1;
      pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
      pProgressorDialog.ShowDialog();
      m_pStepProgressor.Message = "Searching " + iRowCount2.ToString() + " parcel records...";

      int iFixCnt = 0;
      //load all the plan names into a string array
      
      ArrayList NoPlanParcels = new ArrayList();
      ArrayList NoPlanParcelsGrouped = new ArrayList();
      ArrayList PlansList = new ArrayList();
      Dictionary<int, int> ParcelLookup = new Dictionary<int, int>();

      if (!FindNoPlanParcels(pPlansTable, pParcelsTable, ref PlansList, ref NoPlanParcels, 
        ref NoPlanParcelsGrouped, ref ParcelLookup, out iFixCnt))
      {
        pProgressorDialog.HideDialog();
        if (iFixCnt > 0)
          MessageBox.Show("Canceled searching for parcels with no plan.", "Fix Parcels With No Name");
        
        NoPlanParcels = null;

        Cleanup(pMouseCursor, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
        return;
      }

      m_pStepProgressor.Message = "Search complete.";

      NoPlanParcels.Sort();

      dlgFixParcelsWithNoPlan MissingPlansDialog = new dlgFixParcelsWithNoPlan();
      FillTheList(MissingPlansDialog.listView1, NoPlanParcels);
      FillTheList(MissingPlansDialog.listViewByGroup, NoPlanParcelsGrouped);
      FillTheListBox(MissingPlansDialog.listPlans, PlansList);

      MissingPlansDialog.ThePlansList = PlansList;

      MissingPlansDialog.label1.Text = "Found " + iFixCnt.ToString() + 
        " parcels that have a missing plan record.";
      MissingPlansDialog.lblSelectionCount.Text = "(" + iFixCnt.ToString() + " of " 
        + iFixCnt.ToString() + " selected to fix)";

      //cleanup
      Cleanup(null, null, null, null, null, pProgressorDialog, false);
    

      DialogResult dResult = MissingPlansDialog.ShowDialog();

      if (dResult == DialogResult.Cancel)
      {
        Cleanup(null, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
        return;
      }

      //re-initilize the progressor
      m_pProgressorDialogFact = new ProgressDialogFactoryClass();
      m_pTrackCancel = new CancelTrackerClass();
      m_pStepProgressor = m_pProgressorDialogFact.Create(m_pTrackCancel, m_pApp.hWnd);
      pProgressorDialog = (IProgressDialog2)m_pStepProgressor;

      m_pStepProgressor.MinRange = 1;
      m_pStepProgressor.MaxRange = MissingPlansDialog.listView1.CheckedItems.Count;
      m_pStepProgressor.StepValue = 1;
      pProgressorDialog.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;
      pProgressorDialog.ShowDialog();
      m_pStepProgressor.Message = "Fixing parcels without a plan...";


      if (!FabricUTILS.StartEditing(pWS, bIsUnVersioned))
      {
        pProgressorDialog.HideDialog();
        Cleanup(null, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
        FabricUTILS = null;
        return;
      }
      int iNewPlanID = 0;

      //Need to collect the choices from the UI and write the results to the DB
      ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)m_pCadaFab;
      
      if (MissingPlansDialog.radioBtnExistingPlan.Checked)
      {//Get the id of the EXISTING Plan
        string sPlanString = MissingPlansDialog.listPlans.SelectedItem.ToString();
        string[] sPlanOID = Regex.Split(sPlanString, "oid:");
        sPlanOID[1].Trim();
        iNewPlanID = Convert.ToInt32(sPlanOID[1].Remove(sPlanOID[1].LastIndexOf(")")));
      }

      Dictionary<string, int> PlanLookUp= new Dictionary<string,int>();
      ArrayList iUnitsAndFormat = new ArrayList();
      iUnitsAndFormat.Add(m_AngleUnits);
      iUnitsAndFormat.Add(m_AreaUnits);
      iUnitsAndFormat.Add(m_DistanceUnits);
      iUnitsAndFormat.Add(m_DirectionFormat);
      iUnitsAndFormat.Add(m_LineParams);

      if (MissingPlansDialog.radioBtnUserDef.Checked)
      { //create a NEW plan named with user's entered text
        ArrayList sPlanInserts = new ArrayList();
        pSchemaEd.ReleaseReadOnlyFields(pPlansTable, esriCadastralFabricTable.esriCFTPlans); //release safety-catch
        sPlanInserts.Add(MissingPlansDialog.txtPlanName.Text);
        FabricUTILS.InsertPlanRecords(pPlansTable, sPlanInserts,iUnitsAndFormat,PlansList, bIsUnVersioned, 
          null, null, ref PlanLookUp);
        if (!PlanLookUp.TryGetValue(MissingPlansDialog.txtPlanName.Text, out iNewPlanID))
        {
          Cleanup(null, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
          FabricUTILS = null;
          return;
        }
      }

      if (MissingPlansDialog.radioBtnPlanID.Checked)
      {//create multiple new plans for each PlanID
        ArrayList sPlanInserts = new ArrayList();
        foreach (ListViewItem listItem in MissingPlansDialog.listViewByGroup.CheckedItems)
          sPlanInserts.Add("[" + listItem.SubItems[0].Text + "]");
        
        pSchemaEd.ReleaseReadOnlyFields(pPlansTable, esriCadastralFabricTable.esriCFTPlans); //release safety-catch
        FabricUTILS.InsertPlanRecords(pPlansTable, sPlanInserts, iUnitsAndFormat, PlansList, bIsUnVersioned, 
          null, null, ref PlanLookUp);
      }

      ArrayList sParcelUpdates = new ArrayList();
      sParcelUpdates.Add("");
      int i = 0;
      int iCnt = 0;
      int iTokenLimit = 995;
      foreach (ListViewItem listItem in MissingPlansDialog.listView1.CheckedItems)
      {
        string s = listItem.SubItems[1].Text;
        string[] sItems = Regex.Split(s, "id:");
        if (iCnt >= iTokenLimit)//time to start a new row
        {
          sParcelUpdates.Add("");//add a new item to the arraylist
          iCnt = 0;//reset token counter
          i++;//increment array index
        }
        sItems[1] = sItems[1].Remove(sItems[1].LastIndexOf(")"));

        if (iCnt == 0)
          sParcelUpdates[i] += sItems[1];
        else
          sParcelUpdates[i] += "," + sItems[1];
        iCnt++;
      }

      //============edit block==========
      try
      {
        pSchemaEd.ReleaseReadOnlyFields(pTable, esriCadastralFabricTable.esriCFTParcels); //release safety-catch

        if (MissingPlansDialog.radioBtnUserDef.Checked || MissingPlansDialog.radioBtnExistingPlan.Checked)
        {
          if (!FabricUTILS.UpdateParcelRecords(pTable, sParcelUpdates, iNewPlanID, bIsUnVersioned,
            m_pStepProgressor, m_pTrackCancel))
          {
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPlans);
            FabricUTILS.AbortEditing(pWS);
            Cleanup(null, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
            FabricUTILS = null;
            return;
          }
        }
        if (MissingPlansDialog.radioBtnPlanID.Checked)
        {
          if (!FabricUTILS.UpdateParcelRecordsByPlanGroup(pTable, sParcelUpdates, PlanLookUp,
            ParcelLookup, bIsUnVersioned, m_pStepProgressor, m_pTrackCancel))
          {
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);
            pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPlans);
            FabricUTILS.AbortEditing(pWS);
            Cleanup(null, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
            FabricUTILS = null;
            return;
          }
        }

        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTParcels);
        pSchemaEd.ResetReadOnlyFields(esriCadastralFabricTable.esriCFTPlans);

        FabricUTILS.StopEditing(pWS);

        //Cleanup(null, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
        //FabricUTILS = null;
      }
      catch (COMException Ex)
      {
        MessageBox.Show(Ex.ErrorCode.ToString() +":" + Ex.Message,"Fix Missing Plans");
      }
      finally
      {
        Cleanup(null, null, pTable, pPlansTable, pWS, pProgressorDialog, true);
        FabricUTILS = null;
      }
    }

    protected override void OnUpdate()
    {
    }

    private void Cleanup(IMouseCursor MouseCursor, IFIDSet PlansFIDSet, ITable pTable,
      ITable PlansTable,  IWorkspace Workspace, IProgressDialog2 pProgressorDialog, bool CleanFabricVar)
    {

      if (pProgressorDialog != null)
        pProgressorDialog.HideDialog();

      if (MouseCursor != null)
        MouseCursor.SetCursor(0);
      if (m_pStepProgressor != null)
        m_pStepProgressor.Hide();

      m_pStepProgressor = null;

      if (PlansFIDSet != null)
      {
        do { }
        while (Marshal.ReleaseComObject(PlansFIDSet) > 0);
      }
      if (m_pProgressorDialogFact != null)
      {
        do { }
        while (Marshal.ReleaseComObject(m_pProgressorDialogFact) > 0);
      }
      if (m_pTrackCancel != null)
      {
        do { }
        while (Marshal.ReleaseComObject(m_pTrackCancel) > 0);
      }

      if (PlansTable != null)
      {
        do { }
        while (Marshal.ReleaseComObject(PlansTable) > 0);
      }

      if (m_pCadaFab != null && CleanFabricVar)
      {
        do { }
        while (Marshal.ReleaseComObject(m_pCadaFab) > 0);
      }
      
      if (pTable != null)
      {
        do { }
        while (Marshal.ReleaseComObject(pTable) > 0);
      }
      
      if (Workspace != null)
      {
        do { }
        while (Marshal.ReleaseComObject(Workspace) > 0);
      }

    }

    bool FindNoPlanParcels(ITable PlansTable, ITable ParcelsTable, ref ArrayList PlansList, 
      ref ArrayList NoPlanParcels, ref ArrayList NoPlansGroup, ref Dictionary<int, int> ParcelLookup, out int FixCnt)
    {
      List<int> pPlansFIDSet = new List<int>();
      IQueryFilter pQuFilter = new QueryFilterClass();
      ICursor pPlansCur = null;
      FixCnt = 0;
      int iPlanNameIdx = PlansTable.FindField("NAME");
      int iAngleUnits = PlansTable.FindField("ANGLEUNITS");
      int iAreaUnits = PlansTable.FindField("AREAUNITS");
      int iDistanceUnits = PlansTable.FindField("DISTANCEUNITS");
      int iDirectionFormat = PlansTable.FindField("DIRECTIONFORMAT");
      int iLineParams= PlansTable.FindField("LINEPARAMETERS");

      try
      {
        pPlansCur = PlansTable.Search(null, false);
        IRow pPlanRow = pPlansCur.NextRow();
        //Create a collection of PlanIDs
        if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
          m_pStepProgressor.Step();
        
        bool bCont = true;
        Utils FabricUTILS = new Utils();

        while (pPlanRow != null)
        {
          pPlansFIDSet.Add(pPlanRow.OID);

          object Attr_val = pPlanRow.get_Value(iPlanNameIdx);
          string sPlanName = "<null>";
          if (Attr_val != DBNull.Value)
            sPlanName = Attr_val.ToString();

          if (sPlanName.ToUpper().Trim() == "<MAP>")
          {
            m_AngleUnits = (int)pPlanRow.get_Value(iAngleUnits);
            m_AreaUnits = (int)pPlanRow.get_Value(iAreaUnits);
            m_DistanceUnits = (int)pPlanRow.get_Value(iDistanceUnits);
            m_DirectionFormat = (int)pPlanRow.get_Value(iDirectionFormat);
            m_LineParams = (int)pPlanRow.get_Value(iLineParams);
          }

          PlansList.Add(sPlanName + "     (oid: " + pPlanRow.OID.ToString() + ")");
          Marshal.ReleaseComObject(pPlanRow);
          //Check if the cancel button was pressed. If so, stop process before getting next row
          bCont = m_pTrackCancel.Continue();
          if (!bCont)
          {
            FixCnt = 0;
            Marshal.ReleaseComObject(pPlansCur);
            return false;
          }
          pPlanRow = pPlansCur.NextRow();
          if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
            m_pStepProgressor.Step();
        }

        Marshal.ReleaseComObject(pPlansCur);

        ICursor pParcelsCur = null;

        pQuFilter.SubFields = ParcelsTable.OIDFieldName + ",PLANID,NAME";
        pParcelsCur = ParcelsTable.Search(pQuFilter, false);

        Int32 iPlanIDFld = pParcelsCur.Fields.FindField("PLANID");
        Int32 iParcelNameFld = pParcelsCur.Fields.FindField("NAME");

        IRow pParcelRow = pParcelsCur.NextRow();
        //Create a collection of PlanID references
        if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
          m_pStepProgressor.Step();

        while (pParcelRow != null)
        {
          Int32 iPlanID=(Int32)pParcelRow.get_Value(iPlanIDFld);
          if (!pPlansFIDSet.Contains(iPlanID) && iPlanID>-1) //this means that a parcel has been orphaned from its Plan
          {
            try
            {
              int iVal = pParcelRow.OID;
              if (!ParcelLookup.ContainsKey(iVal))
              {
                ParcelLookup.Add(iVal, iPlanID);
                object Attr_val = pParcelRow.get_Value(iParcelNameFld);
                string sName = "<null>";
                if (Attr_val != DBNull.Value)
                  sName = Attr_val.ToString();
                NoPlanParcels.Add(iPlanID.ToString() + "!_!" + sName + "    (id:" + pParcelRow.OID.ToString() + ")");
                NoPlansGroup.Add(iPlanID.ToString());
                FixCnt++;
              }
              else
              { 
              //this means a repeated OID within the same cursor??
                Debug.Print(iVal.ToString());
                Debug.Print(ParcelLookup[iVal].ToString());
              }
            }
            catch (Exception ex)
            {
              Debug.Print(ex.Message);
              Debug.Print(ParcelLookup.Count.ToString());
              Marshal.ReleaseComObject(pParcelRow);
              break;
            }
          }
          Marshal.ReleaseComObject(pParcelRow);

          //Check if the cancel button was pressed. If so, stop process   
          bCont = m_pTrackCancel.Continue();
          if (!bCont)
          {
            FixCnt = 0;
            Marshal.ReleaseComObject(pParcelsCur);
            return false;
          }

          pParcelRow = pParcelsCur.NextRow();

          if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
            m_pStepProgressor.Step();
        }
        Marshal.ReleaseComObject(pParcelsCur);

        RemoveRedundantIndexItems(ref NoPlansGroup, true);
        return true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message.ToString()  + " " + ex.Message);
        return false;
      }
      finally
      {
        if (pQuFilter != null)
        {
          do { }
          while (Marshal.ReleaseComObject(pQuFilter) > 0);
        }
      }
    }

    private void RemoveRedundantIndexItems(ref ArrayList values, bool AddCountTag)
    {
      values.Sort();
      int iRepeat = 1;
      for (int i = values.Count - 1; i > 0; i--)
      {
        string s1 = values[i].ToString().Trim();
        string s2 = values[i-1].ToString().Trim();
        
        if (iRepeat == 1 && AddCountTag)
          values[i] += "!_!1 of 1".Trim();

        if (i == 1 && AddCountTag)
        {
          if (!values[i - 1].ToString().Contains(" of "))
            values[i - 1] += "!_!1 of 1".Trim();
        }

        if (s1 == s2 || s1.Contains(s2 + "!_!"))
        {
          iRepeat++;
          s1 = iRepeat.ToString();//re-using
          if (AddCountTag)
          {
            if (i==1)
              values[i - 1]=values[i - 1].ToString().Substring(0, values[i - 1].ToString().LastIndexOf("!_!")); 
            values[i - 1] += "!_!" + s1 + " of " + s1;
          }
        }
        else
          iRepeat = 1;//reset repeat tracker

        if (iRepeat > 1)
          //remove the item [i] from the array
          values.RemoveAt(i);
      }
    }

    void FillTheList(ListView TheList, ArrayList SummaryNames)
    {
      foreach (string s in SummaryNames)
      {
        string[] sItems = Regex.Split(s, "!_!");
        ListViewItem lstViewItm = new ListViewItem(sItems);
        lstViewItm.Checked = true; //check them all by default
        TheList.Items.Add(lstViewItm);
      }
    }

    void FillTheListBox(ListBox TheList, ArrayList PlansList)
    {
      PlansList.Sort();
      foreach (object s in PlansList)
        TheList.Items.Add(s);
    }

  }
}
