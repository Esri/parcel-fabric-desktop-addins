/*
 Copyright 1995-2013 Esri

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
using System.Linq;
using System.Text;

namespace DeleteSelectedParcels
{
  public class clsDeleteEmptyPlans : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public clsDeleteEmptyPlans()
    {

    }

    protected override void OnClick()
    {

      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      //get the plans
      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      dlgEmptyPlansList pPlansListDialog = new dlgEmptyPlansList();

      IActiveView pActiveView = ArcMap.Document.ActiveView;
      IMap pMap = pActiveView.FocusMap;
      ICadastralFabric pCadFabric = null;
      clsFabricUtils FabricUTILS = new clsFabricUtils();

      //if we're in an edit session then grab the target fabric
      if (pEd.EditState == esriEditState.esriStateEditing)
      {
        pCadFabric = pCadEd.CadastralFabric;
      }
      if(pCadFabric==null)
      {
      //find the first fabric in the map
        if (!FabricUTILS.GetFabricFromMap(pMap, out pCadFabric))
        {
          MessageBox.Show
            ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
          return;
        }
      }
      ITable pPlansTable =null;
      if (pCadFabric != null)
        pPlansTable = pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
      else
        return;
      IDataset pDS = (IDataset)pPlansTable;
      IWorkspace pWS = pDS.Workspace;

      bool bIsFileBasedGDB; 
      bool bIsUnVersioned;
      bool bUseNonVersionedEdit;
      if (!(FabricUTILS.SetupEditEnvironment(pWS, pCadFabric, pEd, 
        out bIsFileBasedGDB, out bIsUnVersioned, out bUseNonVersionedEdit)))
        return;

      IFIDSet pEmptyPlans = null;
      if (!FindEmptyPlans(pCadFabric, null, null, out pEmptyPlans))
        return;
      
      //Fill the list on the dialog
      AddEmptyPlansToList(pCadFabric, pEmptyPlans, pPlansListDialog);

      //Display the dialog
      DialogResult pDialogResult = pPlansListDialog.ShowDialog();
      if (pDialogResult != DialogResult.OK)
        return;
      IArray array = (IArray)pPlansListDialog.checkedListBox1.Tag;

      IFIDSet pPlansToDelete = new FIDSetClass();

      foreach (int checkedItemIndex in pPlansListDialog.checkedListBox1.CheckedIndices)
      {
        Int32 iPlansID = (Int32)array.get_Element(checkedItemIndex);
        if (iPlansID>-1)
          pPlansToDelete.Add(iPlansID);
      }

      if (bUseNonVersionedEdit)
      {
        FabricUTILS.DeleteRowsUnversioned(pWS, pPlansTable, pPlansToDelete, null, null);
      } 
      else
      {
        try
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
          FabricUTILS.DeleteRowsByFIDSet(pPlansTable, pPlansToDelete, null, null);
          pEd.StopOperation("Delete Empty Plans");
        }
        catch (COMException ex)
        {
          MessageBox.Show(Convert.ToString(ex.ErrorCode));
          pEd.AbortOperation();
        }
      }
    }

    protected override void OnUpdate()
    {
      CustomizelHelperExtension v = CustomizelHelperExtension.GetExtension();
      this.Enabled=v.CommandIsEnabled;
      if (!this.Enabled)
        this.Enabled = v.MapHasUnversionedFabric;
    }

    bool FindEmptyPlans(ICadastralFabric Fabric, IStepProgressor StepProgressor,
      ITrackCancel TrackCancel, out IFIDSet EmptyPlans)
    {
      ICursor pPlansCur = null;
      ICursor pParcelCur = null;
      IFIDSet pEmptyPlansFIDSet = new FIDSetClass();
      List<int> pNonEmptyPlansList = new List<int>();
      IDataset pDS=(IDataset)Fabric;
      IWorkspace pWS = pDS.Workspace;

      ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
      string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
      string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

      try
      {
        ITable pPlansTable = Fabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
        ITable pParcelsTable = Fabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);

        ////build a list of ids for all the plans found via parcels
        //if a Personal GDB then don't try to use DISTINCT
        if (pWS.Type == esriWorkspaceType.esriLocalDatabaseWorkspace && pWS.PathName.ToLower().EndsWith(".mdb"))
        {
          pParcelCur = pParcelsTable.Search(null, false);
        }
        else
        {
          IQueryFilter pQuF = new QueryFilterClass();
          pQuF.SubFields = "PLANID";
          IQueryFilterDefinition2 queryFilterDef = (IQueryFilterDefinition2)pQuF;
          queryFilterDef.PrefixClause = "DISTINCT PLANID";
          pParcelCur = pParcelsTable.Search(pQuF, true); //Recycling set to true

        }

        Int32 iPlanIDX = pParcelCur.Fields.FindField("PLANID");
        IRow pParcRow = pParcelCur.NextRow();

        while (pParcRow != null)
        {
          //Create a collection of planIDs from Parcels table that we know are not empty
          Int32 iPlanID = -1;
          object Attr_val = pParcRow.get_Value(iPlanIDX);
          if (Attr_val != DBNull.Value)
          {
            iPlanID = (Int32)Attr_val;
            if (iPlanID > -1)
            {
              if (!pNonEmptyPlansList.Contains(iPlanID))
                pNonEmptyPlansList.Add(iPlanID);
            }
          }
          Marshal.ReleaseComObject(pParcRow);
          pParcRow = pParcelCur.NextRow();
        }

        if (pParcelCur != null)
          Marshal.FinalReleaseComObject(pParcelCur);

        pPlansCur = pPlansTable.Search(null, false);

        IRow pPlanRow = pPlansCur.NextRow();
        while (pPlanRow != null)
        {
          bool bFound = false;
          bFound = pNonEmptyPlansList.Contains(pPlanRow.OID);
          if (!bFound) //This plan was not found in our parcel-referenced plans
          {
            //check if this is the default map plan, so it can be excluded from deletion
            Int32 iPlanNameIDX = pPlanRow.Fields.FindField("NAME");
            string sPlanName = (string)pPlanRow.get_Value(iPlanNameIDX);
            if (sPlanName.ToUpper() != "<MAP>")
              pEmptyPlansFIDSet.Add(pPlanRow.OID);
          }
          Marshal.ReleaseComObject(pPlanRow);
          pPlanRow = pPlansCur.NextRow();
        }
        EmptyPlans = pEmptyPlansFIDSet;

        return true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(Convert.ToString(ex.Message), "Find Empty Plans");
        EmptyPlans = null;
        return false;
      }
      finally
      {
        if (pParcelCur != null)
        {
          do { }
          while (Marshal.ReleaseComObject(pParcelCur) > 0);
        }

        if (pNonEmptyPlansList != null)
        {
          pNonEmptyPlansList.Clear();
          pNonEmptyPlansList = null;
        }

        if (pParcelCur != null)
        {
          do { }
          while (Marshal.ReleaseComObject(pParcelCur) > 0);
        }
      }
    }

    void AddEmptyPlansToList(ICadastralFabric Fabric, IFIDSet EmptyPlansFIDSet, dlgEmptyPlansList EmptyPlansList)
    {
      ITable pPlansTable = Fabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
      CheckedListBox list = EmptyPlansList.checkedListBox1;
      IArray array = new ArrayClass();
      for (int idx = 0; idx <= (EmptyPlansFIDSet.Count() - 1); idx++)
      {
        // Add the name of the plan to the list
        Int32 i_x;
        Int32 iPlanName;
        iPlanName = pPlansTable.FindField("NAME");
        EmptyPlansFIDSet.Next(out i_x);
        array.Add(i_x);
        string sPlanName = (string)pPlansTable.GetRow(i_x).get_Value(iPlanName);
        list.Items.Add(sPlanName, true);
      }
      // Bind array of plan ids with the list
      list.Tag = (object)array;
      if(list.Items.Count>0)
        EmptyPlansList.checkedListBox1_SelectedValueChanged(null, null);
    }
  }
  }

