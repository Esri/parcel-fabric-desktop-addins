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

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.CadastralUI;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FabricPlanTools
{
  class Utils
  {
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
          return Fabric;
        }
        catch
        {
          //if it failed then try it as a fabric sublayer
          try
          {
            pCFSubLyr = (ICadastralFabricSubLayer)Layer;
            Fabric = pCFSubLyr.CadastralFabric;
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
      }
      return false;
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
          "\r\n Click 'OK' to delete permanently.",
          "Continue with delete?", MessageBoxButtons.OKCancel);
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
        pVersObj = null;
      }
      if (pTable != null)
        Marshal.ReleaseComObject(pTable);
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
          MessageBox.Show(ex.Message + "  " + Convert.ToString(ex.ErrorCode),"Start Editing");
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

    public void BuildSearchMapAndQuery(string[] PlanItems, out Dictionary<int, int> Lookup, 
      out string[] InClause, out IFIDSet FIDSetOfPlansToDelete)
    {
      //first build a dictionary for a lookup
      Dictionary<int, int> d = new Dictionary<int, int>();
      //Create a collection for the InClause
      string[] sInClause = new string[0]; //define as dynamic array
      IFIDSet pFIDSet = new FIDSetClass();
      RedimPreserveString(ref sInClause, 1);
      sInClause[0] = "";
      int iTokenLimit = 995;
      int iTokenCnt = 0;
      int iInClauseIdx = 0;
      foreach (string s in PlanItems)
      {
        if (s == null)
          continue;
        string[] sOID = s.Split(',');
        int idx = 0;
        foreach (string s2 in sOID)
        {
          if (idx > 0)
          {
            int lKey = Convert.ToInt32(sOID[idx]);
            pFIDSet.Add(lKey);
            d.Add(lKey, Convert.ToInt32(sOID[0]));
            if (iTokenCnt >= iTokenLimit)
            {
              RedimPreserveString(ref sInClause, 1);
              iTokenCnt = 0;
              iInClauseIdx++;
            }
            sInClause[iInClauseIdx] += "," + sOID[idx];
            iTokenCnt++;
          }
          idx++;
        }
      }
      Lookup = d;
      FIDSetOfPlansToDelete = pFIDSet;
      d = null;
      InClause = sInClause;
      sInClause = null;
    }

    public bool MergePlans(ITable ParcelTable, Dictionary<int, int> Lookup, string[] sInClause, bool Unversioned,
      IStepProgressor StepProgressor, ITrackCancel TrackCancel)
    {
      IDataset pDS = (IDataset)ParcelTable;
      IWorkspace pWS = pDS.Workspace;

      Int32 iPlanIDX = ParcelTable.Fields.FindField("PLANID");
      ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
      string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
      string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

      ITableWrite pTableWr = (ITableWrite)ParcelTable;
      bool bCont = true;
      int zz = 0;
      int iCnt= sInClause.GetLength(0);
      try
      {
        foreach (string ss in sInClause)
        {
          if (ss == null)
            continue;
          IQueryFilter pQF = new QueryFilterClass();
          string InClause = ss.Replace(",", " ").Trim();
          InClause = InClause.Replace(" ", ",");
          string FieldName = "PLANID";
          pQF.WhereClause = sPref + FieldName + sSuff + " IN (" + InClause + ")";
          if (!ParcelTable.HasOID)
            MessageBox.Show("Has no OID");

          int iRowCnt = ParcelTable.RowCount(pQF);
          ICursor pCur = null;
          if (Unversioned)
            pCur = pTableWr.UpdateRows(pQF, false);
          else
            pCur = ParcelTable.Update(pQF, false);
          
          //Check if the cancel button was pressed. If so, stop process
          if (TrackCancel != null)
            bCont = TrackCancel.Continue();
          if (!bCont)
            break;

          //now for each of these parcels, use the dictionary to re-assign the plan id value
          IRow pParcel = pCur.NextRow();

          if (StepProgressor != null)
          {
            StepProgressor.MinRange = StepProgressor.Position; //reset the progress bar position
            StepProgressor.MaxRange = StepProgressor.Position + iRowCnt;
          
            if (StepProgressor.Position < StepProgressor.MaxRange)
              StepProgressor.Step();
            StepProgressor.Message = "Moving parcels to target plans...step " + Convert.ToString(++zz) + " of " + Convert.ToString(iCnt);
          }

          if (pQF != null)
            Marshal.ReleaseComObject(pQF);

          while (pParcel != null)
          {
          //Check if the cancel button was pressed. If so, stop process
            if (TrackCancel != null)
              bCont = TrackCancel.Continue();
            if (!bCont)
              break;
            
            int MergePlanID = (int)pParcel.get_Value(iPlanIDX);
            int TargetPlanID = -1;
            if (Lookup.TryGetValue(MergePlanID, out TargetPlanID))
            {
              pParcel.set_Value(iPlanIDX, TargetPlanID);
              pCur.UpdateRow(pParcel);
            }
            Marshal.ReleaseComObject(pParcel);
            pParcel = pCur.NextRow();
            if (StepProgressor != null)
            {
              if (StepProgressor.Position < StepProgressor.MaxRange)
                StepProgressor.Step();
            }
          }
          if (pCur != null)
            Marshal.ReleaseComObject(pCur);

          if (!bCont)
            break;
        }

        if(pTableWr!=null)
          Marshal.ReleaseComObject(pTableWr);
        
        Marshal.ReleaseComObject(pWS);
        if (!bCont)
          return false;
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public string[] RedimPreserveString(ref string[] x, int ResizeIncrement)
    {
      string[] Temp1 = new string[x.GetLength(0) + ResizeIncrement];
      if (x != null)
        System.Array.Copy(x, Temp1, x.Length);
      x = Temp1;
      Temp1 = null;
      return x;
    }

    public int[] RedimPreserveInt(ref int[] x, int ResizeIncrement)
    {
      int[] Temp1 = new int[x.GetLength(0) + ResizeIncrement];
      if (x != null)
        System.Array.Copy(x, Temp1, x.Length);
      x = Temp1;
      Temp1 = null;
      return x;
    }

    public void RemoveAt(ref string[] source, int index)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      if (0 > index || index >= source.Length)
        throw new ArgumentOutOfRangeException("index", index, "index is outside the bounds of source array");

      System.Array dest = System.Array.CreateInstance(source.GetType().GetElementType(), source.Length - 1);
      System.Array.Copy(source, 0, dest, 0, index);
      System.Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
      System.Array.Copy(dest, 0, source, 0, index);
      string[] Temp1 = new string[dest.GetLength(0)];
      System.Array.Copy(dest, Temp1, dest.Length);
      source = Temp1;
      Temp1 = null;
    }
    
   public bool DeleteRowsByFIDSet(ITable inTable, IFIDSet pFIDSet,
    IStepProgressor StepProgressor, ITrackCancel TrackCancel)
      {//this routine uses the GetRows method, avoids the need to break up the InClause.
        IMouseCursor pMouseCursor = new MouseCursorClass();
        pMouseCursor.SetCursor(2);
        try
        {
          pFIDSet.Reset();
          int[] iID = { };
          bool bCont = true;
          iID = RedimPreserveInt(ref iID, pFIDSet.Count());
          for (int iCount = 0; iCount <= pFIDSet.Count() - 1; iCount++)
            pFIDSet.Next(out iID[iCount]);
          ICursor pCursor = inTable.GetRows(iID, false);
          IRow row = pCursor.NextRow();
          if (StepProgressor != null)
          {
            StepProgressor.MinRange = StepProgressor.Position; //reset the progress bar position
            StepProgressor.MaxRange = StepProgressor.Position + pFIDSet.Count();
            if (StepProgressor.Position < StepProgressor.MaxRange)
              StepProgressor.Step();
          }
          while (row != null)
          {
            //Check if the cancel button was pressed. If so, stop process
            if (StepProgressor != null)
            {
              if (TrackCancel != null)
                bCont = TrackCancel.Continue();
              if (!bCont)
                break;
            }
            row.Delete();
            Marshal.ReleaseComObject(row);
            row = pCursor.NextRow();
            if (StepProgressor != null)
            {
              if (StepProgressor.Position < StepProgressor.MaxRange)
                StepProgressor.Step();
            }
          }
          Marshal.ReleaseComObject(pCursor);
          inTable = null;
          iID = null;
          if (!bCont)
            return false;
          return true;
        }
        catch (COMException ex)
        {
          StepProgressor = null;
          MessageBox.Show(ex.Message);
          return false;
        }
      }

    public bool DeleteRowsUnversioned(IWorkspace TheWorkSpace, ITable inTable,
      IFIDSet pFIDSet, IStepProgressor StepProgressor, ITrackCancel TrackCancel)
    {
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      if (StepProgressor != null)
      {
        StepProgressor.MinRange = StepProgressor.Position; //reset the progress bar position
        StepProgressor.MaxRange = StepProgressor.Position + pFIDSet.Count();
        if (StepProgressor.Position < StepProgressor.MaxRange)
          StepProgressor.Step();
      }

      IQueryFilter pQF = new QueryFilterClass();

      ISQLSyntax pSQLSyntax = (ISQLSyntax)TheWorkSpace;
      string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
      string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

      ICursor ipCursor = null;
      IRow pRow = null;
      //make sure that there are no more then 999 tokens for the in clause(ORA- query will otherwise error on an Oracle database)
      int iTokenLimit = 995;
      int iTokenSet = 0; //the index of the set of 995 tokens
      string sWhereClauseLHS = sPref + inTable.OIDFieldName + sSuff + " in (";
      string[] ids = { sWhereClauseLHS };

      try
      {
        ITableWrite pTableWr = (ITableWrite)inTable;
        pFIDSet.Reset();
        bool bCont = true;
        Int32 iID;

        Int32 count = pFIDSet.Count();
        int j = 0; //inner count for each set of IDs
        for (int k = 0; k < count; k++)
        {
          if (j > iTokenLimit)
          {//over the limit for this Token set, time to create a new set
            ids[iTokenSet] += ")";//close the previous set
            RedimPreserveString(ref ids, 1);//make space in the string array for the next token set
            iTokenSet++;//increment the index
            ids[iTokenSet] = sWhereClauseLHS; //left-hand side of the where clause
            j = 0;//reset the inner count back to zero
          }

          pFIDSet.Next(out iID);
          if (j > 0) //write a comma if this is not the first ID
            ids[iTokenSet] += ",";
          ids[iTokenSet] += iID.ToString();
          j++; //increment the inner count
        }
        ids[iTokenSet] += ")";

        if (count > 0)
        {
          for (int k = 0; k <= iTokenSet; k++)
          {
            pQF.WhereClause = ids[k];
            ipCursor = pTableWr.UpdateRows(pQF, false);
            pRow = ipCursor.NextRow();
            while (pRow != null)
            {
              ipCursor.DeleteRow();
              Marshal.ReleaseComObject(pRow);
              if (StepProgressor != null)
              {
                //Check if the cancel button was pressed. If so, stop process
                if (TrackCancel != null)
                  bCont = TrackCancel.Continue();
                if (!bCont)
                  break;
                if (StepProgressor.Position < StepProgressor.MaxRange)
                  StepProgressor.Step();
              }
              pRow = ipCursor.NextRow();
            }

            if (!bCont)
            {
              AbortEditing(TheWorkSpace);
              if (ipCursor != null)
                Marshal.ReleaseComObject(ipCursor);
              if (pRow != null)
                Marshal.ReleaseComObject(pRow);
              if (pQF != null)
                Marshal.ReleaseComObject(pQF);
              if (pMouseCursor != null)
                Marshal.ReleaseComObject(pMouseCursor);
              return false;
            }
            Marshal.ReleaseComObject(ipCursor);
          }
          Marshal.ReleaseComObject(pQF);
        }

        Marshal.ReleaseComObject(pMouseCursor);
        return true;
      }

      catch (COMException ex)
      {
        if (ipCursor != null)
          Marshal.ReleaseComObject(ipCursor);
        if (pRow != null)
          Marshal.ReleaseComObject(pRow);
        if (pQF != null)
          Marshal.ReleaseComObject(pQF);
        if (pMouseCursor != null)
          Marshal.ReleaseComObject(pMouseCursor);
        MessageBox.Show(Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public bool InsertPlanRecords(ITable pPlansTable, ArrayList sPlanInserts, ArrayList UnitsAndFormat,
      ArrayList ExistingPlans, bool bUseNonVersionedEdit, IStepProgressor m_pStepProgressor, ITrackCancel TrackCancel,
      ref Dictionary<string, int> LookUp)
    {
      IRowBuffer pPlanRowBuff = null;
      ICursor pPlanCur = null;
      try
      {
        Dictionary<string, int> ExistingPlanLookup = new Dictionary<string, int>();
        ArrayList ExistingPlans2 = new ArrayList();

        foreach (string s in ExistingPlans)
        {
          string[] sItems = Regex.Split(s, "oid:");
          string sPart1 = sItems[0].Substring(0, s.IndexOf("(")).Trim();
          ExistingPlans2.Add(sPart1);
          string sPart2 = sItems[1].Substring(0, sItems[1].LastIndexOf(")")).Trim();
          int jj = Convert.ToInt32(sPart2);
          if (!ExistingPlanLookup.ContainsKey(sPart1))
            ExistingPlanLookup.Add((string)sPart1, jj);
        }

        ITableWrite pPlansTableWr = (ITableWrite)pPlansTable;

        bool bShowProgressor = (m_pStepProgressor != null && TrackCancel != null);

        if (bShowProgressor)
          m_pStepProgressor.Message = "Inserting new Plans...";

        if (bUseNonVersionedEdit)
          pPlanCur = pPlansTableWr.InsertRows(true);
        else
          pPlanCur = pPlansTable.Insert(true);

        pPlanRowBuff = pPlansTable.CreateRowBuffer();
        bool bCont = true;
        foreach (string s in sPlanInserts)
        {
          //Check if the cancel button was pressed. If so, stop process   
          if (bShowProgressor)
          {
            bCont = TrackCancel.Continue();
            if (!bCont)
              break;
          }

          if (ExistingPlanLookup.ContainsKey(s))
          {
            int iVal = -1;
            if (ExistingPlanLookup.TryGetValue(s, out iVal))
              LookUp.Add(s, iVal);
          }
          else
          {
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("Name"), s);
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("Description"), "Recovered missing plan.");

            DateTime mDate = DateTime.Now;
            var SystemDate = mDate.Date;
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("SystemStartDate"), SystemDate);

            int DistanceUnits = (int)UnitsAndFormat[2];//9001;//units should be set based on fabric projection
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("DistanceUnits"), DistanceUnits);

            int DirectionFormat = (int)UnitsAndFormat[3];//4;
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("DirectionFormat"), DirectionFormat);

            int AreaUnits = (int)UnitsAndFormat[1]; //2;
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("AreaUnits"), AreaUnits);

            int AngleUnits = (int)UnitsAndFormat[0]; //3;
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("AngleUnits"), AngleUnits);

            int LineParameters = (int)UnitsAndFormat[4];//4;
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("LineParameters"), LineParameters);

            int Accuracy = 4;
            pPlanRowBuff.set_Value(pPlanRowBuff.Fields.FindField("Accuracy"), Accuracy);

            object P_Oid = pPlanCur.InsertRow(pPlanRowBuff);

            LookUp.Add(s, (int)P_Oid);
          }

          if (bShowProgressor)
          {
            if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
              m_pStepProgressor.Step();
          }
        }

      if (pPlanRowBuff!=null)
      {
        do { }
        while (Marshal.ReleaseComObject(pPlanRowBuff) > 0);
      }

      if (pPlanCur != null)
      {
        do { }
        while (Marshal.ReleaseComObject(pPlanCur) > 0);
      }

      return bCont; //return the result from the Cancel tracker...true unless cancel was clicked.
      }

      catch (COMException ex)
      {
        MessageBox.Show(ex.Message);
        if (pPlanRowBuff != null)
        {
          do { }
          while (Marshal.ReleaseComObject(pPlanRowBuff) > 0);
        }

        if (pPlanCur != null)
        {
          do { }
          while (Marshal.ReleaseComObject(pPlanCur) > 0);
        }        
        return false;
      }
    }

    public bool UpdateParcelRecords(ITable pParcelsTable, ArrayList sParcelUpdates, int TheNewPlanID,bool bUseNonVersionedEdit,
                IStepProgressor m_pStepProgressor, ITrackCancel TrackCancel)
    {
      bool bShowProgressor = (m_pStepProgressor != null && TrackCancel != null);

      if (bShowProgressor)
        m_pStepProgressor.Message = "Updating Parcels...";

      Int32 iPlanID = pParcelsTable.Fields.FindField("PLANID");

      IDataset pDS = (IDataset)pParcelsTable;
      ISQLSyntax pSQLSyntax = (ISQLSyntax)pDS.Workspace;
      string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
      string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

      IQueryFilter pQF = new QueryFilterClass();
      string FieldName = pParcelsTable.OIDFieldName;
      bool bCont = true;
      try
      {
        foreach (string InClause in sParcelUpdates)
        {
          if (InClause.Trim() == "")
            continue;
          pQF.WhereClause = sPref + FieldName + sSuff + " IN (" + InClause + ")";

          ITableWrite2 pParcelsTableWr = (ITableWrite2)pParcelsTable;
          ICursor pUpdateCur = null;
          if (bUseNonVersionedEdit)
            pUpdateCur = pParcelsTableWr.UpdateRows(pQF, false);
          else
            pUpdateCur = pParcelsTable.Update(pQF, false);

          IRow pRow = pUpdateCur.NextRow();
          //now update the parcel planid 
          while (pRow != null)
          {
            pRow.set_Value(iPlanID, TheNewPlanID);
            pUpdateCur.UpdateRow(pRow);
            if (bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
            Marshal.ReleaseComObject(pRow);
            //after garbage collection, and before gettng the next row,
            //check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
              bCont = TrackCancel.Continue();
            if (!bCont)
              break;
            pRow = pUpdateCur.NextRow();
          }
          if (pUpdateCur != null)
            Marshal.ReleaseComObject(pUpdateCur);

          if (!bCont)
            break;
        }
        //
      }
      catch (COMException Ex)
      {
        MessageBox.Show("Problem encountered:" + Ex.ErrorCode.ToString()+ ":" + Ex.Message, 
          "Update Parcel Records");
        if (pQF != null)
          Marshal.ReleaseComObject(pQF);
        return false;
      }

      if (pQF != null)
        Marshal.ReleaseComObject(pQF);

      return bCont; //return the result from the Cancel tracker...true unless cancel was clicked.
    }

    public bool UpdateParcelRecordsByPlanGroup(ITable pParcelsTable, ArrayList sParcelUpdates, 
      Dictionary<string, int> TheNewPlanIDs, Dictionary<int,int> ParcelLookup, bool bUseNonVersionedEdit, 
      IStepProgressor m_pStepProgressor, ITrackCancel TrackCancel)
    {
      bool bShowProgressor = (m_pStepProgressor != null && TrackCancel !=null);

      if (bShowProgressor)
        m_pStepProgressor.Message = "Updating Parcels...";

      Int32 iPlanID = pParcelsTable.Fields.FindField("PLANID");

      IDataset pDS = (IDataset)pParcelsTable;
      ISQLSyntax pSQLSyntax = (ISQLSyntax)pDS.Workspace;
      string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
      string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

      IQueryFilter pQF = new QueryFilterClass();
      string FieldName = pParcelsTable.OIDFieldName;
      
      bool bCont = true;
      try
      {
        foreach (string InClause in sParcelUpdates)
        {
          if (InClause.Trim() == "")
            continue;
          pQF.WhereClause = sPref + FieldName + sSuff + " IN (" + InClause + ")";

          ITableWrite2 pParcelsTableWr = (ITableWrite2)pParcelsTable;
          ICursor pUpdateCur = null;
          if (bUseNonVersionedEdit)
            pUpdateCur = pParcelsTableWr.UpdateRows(pQF, false);
          else
            pUpdateCur = pParcelsTable.Update(pQF, false);

          IRow pRow = pUpdateCur.NextRow();
          //now update the parcel planid 

          int iOldPlanID = -1;
          int iNewPlanID = -1;

          while (pRow != null)
          {
            int iParcelID = pRow.OID;

            if (!ParcelLookup.TryGetValue(iParcelID, out iOldPlanID))
              continue;

            string sNewPlanName = "[" + Convert.ToString(iOldPlanID) + "]";

            if (!TheNewPlanIDs.TryGetValue(sNewPlanName, out iNewPlanID))
              continue;

            pRow.set_Value(iPlanID, iNewPlanID);
            pUpdateCur.UpdateRow(pRow);
            if (bShowProgressor)
            {
              if (m_pStepProgressor.Position < m_pStepProgressor.MaxRange)
                m_pStepProgressor.Step();
            }
            Marshal.ReleaseComObject(pRow);
            //after garbage collection, and before gettng the next row,
            //check if the cancel button was pressed. If so, stop process   
            if (bShowProgressor)
              bCont = TrackCancel.Continue();
            if (!bCont)
              break;
            pRow = pUpdateCur.NextRow();
          }
          if (pUpdateCur != null)
            Marshal.ReleaseComObject(pUpdateCur);

          if (!bCont)
            break;
        }
      }
      catch (COMException Ex)
      {
        MessageBox.Show("Problem encountered:" + Ex.ErrorCode.ToString() + ":" + Ex.Message,
        "Update Parcel Records");
        if (pQF != null)
          Marshal.ReleaseComObject(pQF);
        return false;
      }

      if (pQF != null)
        Marshal.ReleaseComObject(pQF);

      return bCont; //return the result from the Cancel tracker...true unless cancel was clicked.
    }

    public int GetFabricVersion(ICadastralFabric2 pFab)
  {
    IDECadastralFabric2 pDECadaFab = null;
    IDEDataset pDEDS = null;
    int iVersion = -1;
    try
    {  
      IDatasetComponent pDSComponent=(IDatasetComponent)pFab;
      pDEDS = pDSComponent.DataElement;
      pDECadaFab = (IDECadastralFabric2)pDEDS;
      iVersion = pDECadaFab.Version;
    }
    catch (COMException ex)
    {
      MessageBox.Show(ex.Message);
    }

    if (pDEDS!=null)
      Marshal.ReleaseComObject(pDEDS);
    
    return iVersion;
  }

    public bool CadastralTableAddField(ICadastralFabric pCadaFab, esriCadastralFabricTable eTable, esriFieldType FieldType,
          string FieldName, string FieldAlias, int FieldLength)
    {
      ITable pTable = pCadaFab.get_CadastralTable(eTable);

      // First check to see if a field with this name already exists
      if(pTable.FindField(FieldName) > -1)
      {
        if (pTable != null)
          Marshal.ReleaseComObject(pTable);
        return false;
      }

      IField2 pField = null; 
      try
      {
        //Create a new Field
        pField = new FieldClass();
        //QI for IFieldEdit
        IFieldEdit2 pFieldEdit = (IFieldEdit2)pField;
        pFieldEdit.Type_2 = FieldType;
        pFieldEdit.Editable_2 = true;
        pFieldEdit.IsNullable_2 = true;
        pFieldEdit.Name_2 = FieldName;
        pFieldEdit.AliasName_2 = FieldAlias;
        pFieldEdit.Length_2 = FieldLength;
        //'.RasterDef_2 = pRasterDef
        pTable.AddField(pField);

        if (pField != null)
          Marshal.ReleaseComObject(pField);
      }
      catch (COMException ex)
      {
        if (pField != null)
          Marshal.ReleaseComObject(pField);
        MessageBox.Show(ex.Message);
        return false;
      }
      return true;
    }

    public bool CadastralTableAddFieldV1(ICadastralFabric pCadaFab, esriCadastralFabricTable eTable, esriFieldType FieldType,
          string FieldName, string FieldAlias, int FieldLength)
    { 
      ITable pTable = pCadaFab.get_CadastralTable(eTable);

      // First check to see if a field with this name already exists
      if (pTable.FindField(FieldName) > -1)
      {
        if (pTable != null)
          Marshal.ReleaseComObject(pTable);
        return false;
      }

      IDatasetComponent pDSComponent = (IDatasetComponent)pCadaFab;
      IDEDataset pDEDS = pDSComponent.DataElement;
      IDECadastralFabric pDECadaFab = (IDECadastralFabric)pDEDS;
      
      IField2 pField = null;
      try
      {
        //Create a new Field
        pField = new FieldClass();
        //QI for IFieldEdit
        IFieldEdit2 pFieldEdit = (IFieldEdit2)pField;
        pFieldEdit.Type_2 = FieldType;
        pFieldEdit.Editable_2 = true;
        pFieldEdit.IsNullable_2 = true;
        pFieldEdit.Name_2 = FieldName;
        pFieldEdit.AliasName_2 = FieldAlias;
        pFieldEdit.Length_2 = FieldLength;
        //'.RasterDef_2 = pRasterDef
      }
      catch (COMException ex)
      {
        if (pField != null)
          Marshal.ReleaseComObject(pField);
        MessageBox.Show(ex.Message);
        return false;
      }

      IArray pArr = pDECadaFab.CadastralTableFieldEdits;

      bool found = false;
      int cnt = pArr.Count;
      ICadastralTableFieldEdits pCadaTableFldEdits=null;
      IFields pFields=null;

      for (int i = 0; i<=(cnt - 1);i++)
      {
        pCadaTableFldEdits = (ICadastralTableFieldEdits)pArr.get_Element(i);
        IFieldsEdit pNewFields = new FieldsClass();
        int fldCnt =0;
        if(pCadaTableFldEdits.CadastralTable == eTable )
        {
          pFields = pCadaTableFldEdits.ExtendedAttributeFields;
          //Copy existing fields
          if (pFields != null)
          {
            fldCnt = pFields.FieldCount;
            pNewFields.FieldCount_2 = fldCnt + 1;
            for (int j = 0; j <= (fldCnt - 1); j++)
              pNewFields.Field_2[j] = pFields.get_Field(j);
          }
          else
            pNewFields.FieldCount_2 = 1;

          //Add the new field
          pNewFields.Field_2[fldCnt] = pField;
          //reset extended attribute fields
          pCadaTableFldEdits.ExtendedAttributeFields = pNewFields;
          found = true;
          break;
        }
      }
    
      if(!found)
      {
        pCadaTableFldEdits = new CadastralTableFieldEditsClass();
        pCadaTableFldEdits.CadastralTable = eTable; //add the field to the table
        pFields = new FieldsClass();
        int fldCnt = pFields.FieldCount;
        IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;
        pFieldsEdit.FieldCount_2 = fldCnt + 1;
        pFieldsEdit.Field_2[fldCnt]= pField;
        pCadaTableFldEdits.ExtendedAttributeFields = pFields;
        pArr.Add(pCadaTableFldEdits);
        Marshal.ReleaseComObject(pFields);
      }

      //Set the CadastralTableFieldEdits property on the DE to the array
        pDECadaFab.CadastralTableFieldEdits = pArr;

      // Update the schema
        ICadastralFabricSchemaEdit pSchemaEd =(ICadastralFabricSchemaEdit)pCadaFab;
        pSchemaEd.UpdateSchema(pDECadaFab);

        Marshal.ReleaseComObject(pField);
        return true;
      }
    }
  }
