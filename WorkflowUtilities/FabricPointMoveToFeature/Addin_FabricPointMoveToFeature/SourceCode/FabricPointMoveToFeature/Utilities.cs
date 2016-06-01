﻿/*
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;
using Microsoft.Win32;


namespace FabricPointMoveToFeature
{
  class Utilities
  {
    public List<string> InClauseFromOIDsList(List<int> ListOfOids, int TokenMax)
    {
      List<string> InClause = new List<string>();
      int iCnt = 0;
      int iIdx = 0;
      InClause.Add("");
      foreach (int i in ListOfOids)
      {
        if (iCnt == TokenMax)
        {
          InClause.Add("");
          iCnt = 0;
          iIdx++;
        }
        if (InClause[iIdx].Trim() == "")
          InClause[iIdx] = i.ToString();
        else
          InClause[iIdx] += "," + i.ToString();
        iCnt++;
      }
      return InClause;
    }
    public bool GetFabricSubLayers(IMap Map, esriCadastralFabricTable FabricSubClass, out IArray CFParcelFabSubLayers)
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
    public bool WriteToRegistry(RegistryHive Hive, string Path, string Name, string KeyValue)
    {
      RegistryKey objParent = null;
      if (Hive == RegistryHive.ClassesRoot)
        objParent = Registry.ClassesRoot;

      if (Hive == RegistryHive.CurrentConfig)
        objParent = Registry.CurrentConfig;

      if (Hive == RegistryHive.CurrentUser)
        objParent = Registry.CurrentUser;

      if (Hive == RegistryHive.LocalMachine)
        objParent = Registry.LocalMachine;

      if (Hive == RegistryHive.PerformanceData)
        objParent = Registry.PerformanceData;

      if (Hive == RegistryHive.Users)
        objParent = Registry.Users;

      if (objParent != null)
      {
        RegistryKey regKeyAppRoot = objParent.CreateSubKey(Path);
        regKeyAppRoot.SetValue(Name, KeyValue);
        return true;
      }
      else
        return false;
    }
    public string ReadFromRegistry(RegistryHive Hive, string Key, string ValueName)
    {
      string sAns = "";
      RegistryKey objParent = null;

      if (Hive == RegistryHive.ClassesRoot)
        objParent = Registry.ClassesRoot;

      if (Hive == RegistryHive.CurrentConfig)
        objParent = Registry.CurrentConfig;

      if (Hive == RegistryHive.CurrentUser)
        objParent = Registry.CurrentUser;

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
        object xx = null;
        if (objSubKey != null)
          xx = (objSubKey.GetValue(ValueName));
        if (xx != null)
          sAns = xx.ToString();
      }
      return sAns;
    }
    public string GetDesktopVersionFromRegistry()
    {
      string sVersion = "";
      try
      {
        string s = ReadFromRegistry(RegistryHive.LocalMachine, "Software\\ESRI\\ArcGIS", "RealVersion");
        string[] Values = s.Split('.');
        sVersion = Values[0] + "." + Values[1];
        return sVersion;
      }
      catch (Exception)
      {
        return sVersion;
      }
    }
    public bool DeleteByInClause(IWorkspace TheWorkSpace, ITable inTable, IField QueryIntegerField,
      List<string> InClauseIDs, bool IsVersioned, IStepProgressor StepProgressor, ITrackCancel TrackCancel)
    {
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      IQueryFilter pQF = new QueryFilterClass();

      ISQLSyntax pSQLSyntax = (ISQLSyntax)TheWorkSpace;
      string sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
      string sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

      ICursor ipCursor = null;
      IRow pRow = null;
      //make sure that there are no more then 999 tokens for the in clause(ORA- query will otherwise error on an Oracle database)
      //this code assumes that InClauseIDs holds an arraylist of comma separated OIDs with no more than 995 id's per list item
      string sWhereClauseLHS = sPref + QueryIntegerField.Name + sSuff + " in (";

      try
      {
        ITableWrite pTableWr = (ITableWrite)inTable;
        bool bCont = true;

        Int32 count = InClauseIDs.Count - 1;
        for (int k = 0; k <= count; k++)
        {
          pQF.WhereClause = sWhereClauseLHS + InClauseIDs[k] + ")"; //left-hand side of the where clause
          if (pQF.WhereClause.Contains("()"))
            continue;
          if (!IsVersioned)
            ipCursor = pTableWr.UpdateRows(pQF, false);
          else
            ipCursor = inTable.Update(pQF, false);

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
            return false;
          }
          Marshal.ReleaseComObject(ipCursor);
        }
        return true;
      }

      catch (Exception ex)
      {
        if (ipCursor != null)
          Marshal.ReleaseComObject(ipCursor);
        if (pRow != null)
          Marshal.ReleaseComObject(pRow);
        MessageBox.Show(Convert.ToString(ex.Message));
        return false;
      }
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

  }
}