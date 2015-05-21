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

namespace DeleteSelectedParcels
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

    public void EmptyGeometries(IFeatureClass inFeatureClass, IFIDSet pFIDSet)
    {
      try
      {
        if (pFIDSet.Count() < 1)
          return;
        pFIDSet.Reset();
        int[] iID = { };
        iID = RedimPreserveInt(ref iID, pFIDSet.Count());
        for (int iCount = 0; iCount <= pFIDSet.Count() - 1; iCount++)
          pFIDSet.Next(out iID[iCount]);
        IFeatureCursor pFeatCursor = inFeatureClass.GetFeatures(iID, false);
        IFeature pFeat = pFeatCursor.NextFeature();
        while (pFeat != null)
        {
          IGeometry pGeo = pFeat.ShapeCopy;
          pGeo.SetEmpty();
          pFeat.Shape = pGeo;
          pFeat.Store();
          Marshal.ReleaseComObject(pFeat);
          pFeat = pFeatCursor.NextFeature();
        }
        Marshal.ReleaseComObject(pFeatCursor);
        iID = null;
        return;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }
    }

    public void EmptyGeometriesUnversioned(IWorkspace TheWorkSpace, IFeatureClass inTable, IFIDSet pFIDSet)
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
          IFeature pFeat = null;
          ISet pRowSet = new SetClass();
          for (int k = 0; k <= iTokenSet; k++)
          {
            pRowSet.RemoveAll();
            pQF.WhereClause = ids[k];
            ipCursor = pTableWr.UpdateRows(pQF, false);
            pRow = ipCursor.NextRow();
            while (pRow != null)
            {
              pFeat = (IFeature)pRow;
              IGeometry pGeo = pFeat.ShapeCopy;
              pGeo.SetEmpty();
              pFeat.Shape = pGeo;
              ipCursor.UpdateRow(pRow);
              Marshal.ReleaseComObject(pRow);
              pRow = ipCursor.NextRow();
            }
            if (!bCont)
            {
              AbortEditing(TheWorkSpace);
              if (ipCursor != null)
                Marshal.ReleaseComObject(ipCursor);
              if (pRow != null)
                Marshal.ReleaseComObject(pRow);
              //if (pQF != null)
              //  Marshal.ReleaseComObject(pQF);
              return;
            }
          }
          Marshal.ReleaseComObject(ipCursor);
          //Marshal.ReleaseComObject(pQF);
        }
        return;
      }

      catch (COMException ex)
      {
        if (ipCursor != null)
          Marshal.ReleaseComObject(ipCursor);
        if (pRow != null)
          Marshal.ReleaseComObject(pRow);
        //if (pQF != null)
        //  Marshal.ReleaseComObject(pQF);
        MessageBox.Show(Convert.ToString(ex.ErrorCode));
        return;
      }
    }

    public void IntersectFIDSetCommonIDs(IFIDSet InFIDSet1, IFIDSet InFIDSet2, out IFIDSet OutFIDSet)
    {
      IFIDSet OutFIDSet2 = new FIDSetClass();
      Int32 pfID = -1;
      bool bExists = false;
      InFIDSet1.Reset();
      for (Int32 i = 0; i <= (InFIDSet1.Count() - 1); i++)
      {
        InFIDSet1.Next(out pfID);
        InFIDSet2.Find(pfID, out bExists);
        if (bExists)
          OutFIDSet2.Add(pfID);
      }
      OutFIDSet = OutFIDSet2;
      return;
    }

    public bool GetFabricSubLayersFromFabric(IMap Map, ICadastralFabric Fabric, out IFeatureLayer CFPointLayer, out IFeatureLayer CFLineLayer,
          out IArray CFParcelLayers, out IFeatureLayer CFControlLayer, out IFeatureLayer CFLinePointLayer)
    {
      ICadastralFabricLayer pCFLayer = null;
      ICadastralFabricSubLayer pCFSubLyr = null;
      ICompositeLayer pCompLyr = null;
      IArray CFParcelLayers2 = new ArrayClass();
     
      long layerCount = Map.LayerCount;
      CFPointLayer = null; CFLineLayer = null; CFControlLayer = null; CFLinePointLayer = null;
      IFeatureLayer pParcelLayer = null;
      try
      {
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
              ICadastralFabric pCadFab2 = null;
              try
              {
                pCadFab2 = pCFSubLyr.CadastralFabric; //this fails when the layer is created from a selection
              }
              catch(Exception)
              {
                continue;
              }
              if (Fabric.Equals(pCadFab2) && pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTParcels)
              {
                pParcelLayer = (IFeatureLayer)pCFSubLyr;
                CFParcelLayers2.Add(pParcelLayer);
              }
              if (Fabric.Equals(pCadFab2) && pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLines)
                CFLineLayer = (IFeatureLayer)pCFSubLyr;

              if (Fabric.Equals(pCadFab2) && pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTPoints)
                CFPointLayer = (IFeatureLayer)pCFSubLyr;

              if (Fabric.Equals(pCadFab2) && pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLinePoints)
                CFLinePointLayer = (IFeatureLayer)pCFSubLyr;
              
              if (Fabric.Equals(pCadFab2) && pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTControl)
                CFControlLayer = (IFeatureLayer)pCFSubLyr;
            }
          }
          if (pCFLayer != null)
          {
            if (pCFLayer.CadastralFabric.Equals(Fabric))
            {
              CFPointLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRPoints);
              CFLineLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLines);
              pParcelLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRParcels);
              CFParcelLayers2.Add(pParcelLayer);
              CFControlLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRControlPoints);
              CFLinePointLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLinePoints);
            }
            CFParcelLayers = CFParcelLayers2;
            return true;
          }
        }
        //at the minimum, just need to make sure we have a parcel sublayer for the requested fabric
        if (pParcelLayer != null)
        {
          CFParcelLayers = CFParcelLayers2;
          return true;
        }
        else
        {
          CFParcelLayers = null;
          return false;
        }
      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message);
        CFParcelLayers = null;
        return false;
      }
    }

    public bool GetControlLayersFromFabric(IMap Map, ICadastralFabric Fabric, out IArray CFControlLayers)
    {
      ICadastralFabricLayer pCFLayer = null;
      ICadastralFabricSubLayer pCFSubLyr = null;
      ICompositeLayer pCompLyr = null;
      IArray CFControlLayers2 = new ArrayClass();

      long layerCount = Map.LayerCount;

      IFeatureLayer pControllLayer = null;
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
            ICadastralFabric pCadFab2 = null;
            try
            {
              pCadFab2 = pCFSubLyr.CadastralFabric; //this fails when the layer is created from a selection
            }
            catch (Exception)
            {
              continue;
            }
            if (Fabric.Equals(pCadFab2) &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTControl)
            {
              pControllLayer = (IFeatureLayer)pCFSubLyr;
              CFControlLayers2.Add(pControllLayer);
            }
          }
        }

        //Check that the fabric layer belongs to the requested fabric
        if (pCFLayer != null)
        {
          if (pCFLayer.CadastralFabric.Equals(Fabric))
          {
            pControllLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRControlPoints);
            CFControlLayers2.Add(pControllLayer);
            Debug.WriteLine(pControllLayer.Name);
          }
          CFControlLayers = CFControlLayers2;
          return true;
        }
      }
      //at the minimum, just need to make sure we have a control sublayer for the requested fabric
      if (pControllLayer != null)
      {
        CFControlLayers = CFControlLayers2;
        return true;
      }
      else
      {
        CFControlLayers = null;
        return false;
      }
    }

    public bool GetLinePointLayersFromFabric(IMap Map, ICadastralFabric Fabric, out IArray CFLinepointLayers)
    {
      ICadastralFabricLayer pCFLayer = null;
      ICadastralFabricSubLayer pCFSubLyr = null;
      ICompositeLayer pCompLyr = null;
      IArray CFLinePointLayers2 = new ArrayClass();

      long layerCount = Map.LayerCount;

      IFeatureLayer pLinePointlLayer = null;
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
            ICadastralFabric pCadFab2 = null;
            try
            {
              pCadFab2 = pCFSubLyr.CadastralFabric; //this fails when the layer is created from a selection
            }
            catch (Exception)
            {
              continue;
            }
            if (Fabric.Equals(pCadFab2) &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLinePoints)
            {
              pLinePointlLayer = (IFeatureLayer)pCFSubLyr;
              CFLinePointLayers2.Add(pLinePointlLayer);
            }
          }
        }

        //Check that the fabric layer belongs to the requested fabric
        if (pCFLayer != null)
        {
          if (pCFLayer.CadastralFabric.Equals(Fabric))
          {
            pLinePointlLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLinePoints);
            CFLinePointLayers2.Add(pLinePointlLayer);
            Debug.WriteLine(pLinePointlLayer.Name);
          }
          CFLinepointLayers = CFLinePointLayers2;
          return true;
        }
      }
      //at the minimum, just need to make sure we have a line-point sublayer for the requested fabric
      if (pLinePointlLayer != null)
      {
        CFLinepointLayers = CFLinePointLayers2;
        return true;
      }
      else
      {
        CFLinepointLayers = null;
        return false;
      }
    }

    public bool GetLineLayersFromFabric(IMap Map, ICadastralFabric Fabric, out IArray CFLineLayers)
    {
      ICadastralFabricLayer pCFLayer = null;
      ICadastralFabricSubLayer pCFSubLyr = null;
      ICompositeLayer pCompLyr = null;
      IArray CFLineLayers2 = new ArrayClass();

      long layerCount = Map.LayerCount;

      IFeatureLayer pLineLayer = null;
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
            ICadastralFabric pCadFab2 = null;
            try
            {
              pCadFab2 = pCFSubLyr.CadastralFabric; //this fails when the layer is created from a selection
            }
            catch (Exception)
            {
              continue;
            }
            if (Fabric.Equals(pCadFab2) &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLines)
            {
              pLineLayer = (IFeatureLayer)pCFSubLyr;
              CFLineLayers2.Add(pLineLayer);
            }
          }
        }

        //Check that the fabric layer belongs to the requested fabric
        if (pCFLayer != null)
        {
          if (pCFLayer.CadastralFabric.Equals(Fabric))
          {
            pLineLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLines);
            CFLineLayers2.Add(pLineLayer);
            Debug.WriteLine(pLineLayer.Name);
          }
          CFLineLayers = CFLineLayers2;
          return true;
        }
      }
      //at the minimum, just need to make sure we have a control sublayer for the requested fabric
      if (pLineLayer != null)
      {
        CFLineLayers = CFLineLayers2;
        return true;
      }
      else
      {
        CFLineLayers = null;
        return false;
      }
    }

    public ISymbol GetSymbolFromFeature(IFeatureLayer InFeatureLayer, IFeature InFeature)
    {
      IGeoFeatureLayer pGeoLyr = (IGeoFeatureLayer)InFeatureLayer;
      IFeatureRenderer pFeatRend = pGeoLyr.Renderer;
      ISymbol pSymb = pFeatRend.get_SymbolByFeature(InFeature);
      return pSymb;
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
          if (pCFLayer is ICadastralFabricLayer)
          {
            pCFLayer = (ICadastralFabricLayer)Layer;
            Fabric = pCFLayer.CadastralFabric;
            return Fabric;
          }
          else
          {
            pCFSubLyr = (ICadastralFabricSubLayer)Layer;
            Fabric = pCFSubLyr.CadastralFabric;
            return Fabric;
          }
        }
        catch
        {
          continue;//cast failed...not a fabric sublayer
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

    public ICursor GetCursorFromCommaSeparatedOIDList(ITable inTable, string OIDList, string FieldName)
    {
      try
      {
        if (OIDList.Trim() == "")
          return null;
        string sPref; string sSuff;
        IDataset pDS = (IDataset)inTable;
        IWorkspace pWS = pDS.Workspace;
        ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
        sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
        sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);

        int iFld=inTable.FindField(FieldName);
        string sFldName = inTable.Fields.get_Field(iFld).Name;

        //if there is no open parenthesis then add one
        if (!OIDList.Trim().StartsWith("("))
        {
          string sOIDList = ("(" + OIDList).Trim();
        }
        // remove the last comma if there is one
        if ((OIDList.Substring(OIDList.Length - 1, 1)) == ",")
          OIDList = OIDList.Substring(0, OIDList.Length - 1);
        // add a closing parenthesis if it is missing
        if (!OIDList.Trim().EndsWith(")")) OIDList = OIDList + ")";
        OIDList = OIDList.Trim();

        IQueryFilter pQueryFilter = new QueryFilterClass();
        pQueryFilter.WhereClause = (sPref + sFldName + sSuff).Trim() + " IN " + OIDList;
        ICursor pCursor = inTable.Search(pQueryFilter, false);
        //Marshal.ReleaseComObject(pQueryFilter);//garbage collection
        return pCursor;
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        return null;
      }
    }

    public List<string> BuildInClauseList(List<int> theList)
    {
      List<string> InClauseList = new List<string>();
      int iToken = 995;
      int iCounter = 0;
      try
      {
        if (theList.Count() > 0)
        {
          string sFileDump = "(";

          foreach (int i in theList)
          {
            sFileDump += i.ToString() + ",";
            iCounter++;
            if (iCounter >= iToken)
            {
              int iPos = sFileDump.LastIndexOf(",");
              sFileDump = sFileDump.Remove(iPos);
              sFileDump += ")";
              InClauseList.Add(sFileDump);
              sFileDump = "(";
              iCounter = 0;
            }
          }
          int iPos2 = sFileDump.LastIndexOf(",");
          sFileDump = sFileDump.Remove(iPos2);
          sFileDump += ")";
          InClauseList.Add(sFileDump);
        }

        return InClauseList;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
    }

    public bool ResetControlAssociations(ITable ControlTable, IQueryFilter QueryFilter, bool Unversioned)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)ControlTable;//used for unversioned table
        IRow pControlPointFeat = null;
        ICursor pControlPtCurs = null;

        if (Unversioned)
          pControlPtCurs = pTableWr.UpdateRows(QueryFilter, false);
        else
          pControlPtCurs = ControlTable.Update(QueryFilter, false);

        pControlPointFeat = pControlPtCurs.NextRow();

        Int32 iPointIDX = pControlPtCurs.Fields.FindField("POINTID");
        Int32 iActiveIDX = pControlPtCurs.Fields.FindField("ACTIVE");

        while (pControlPointFeat != null)
        {//loop through all of the control points, and if any of the point id values are in the deleted set, then remove the ID from the 
          //control point's Point id field
          pControlPointFeat.set_Value(iPointIDX, DBNull.Value);
          pControlPointFeat.set_Value(iActiveIDX, 0);
          if (Unversioned)
            pControlPtCurs.UpdateRow(pControlPointFeat);
          else
            pControlPointFeat.Store();

          Marshal.ReleaseComObject(pControlPointFeat); //garbage collection
          pControlPointFeat = pControlPtCurs.NextRow();
        }
        Marshal.ReleaseComObject(pControlPtCurs); //garbage collection
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem resetting control point association: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public bool ResetPointAssociations(ITable PointTable, IQueryFilter QueryFilter, bool Unversioned)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)PointTable;//used for unversioned table
        IRow pPointFeat = null;
        ICursor pPtCurs = null;

        if (Unversioned)
          pPtCurs = pTableWr.UpdateRows(QueryFilter, false);
        else
          pPtCurs = PointTable.Update(QueryFilter, false);

        pPointFeat = pPtCurs.NextRow();

        Int32 iPointIDX = pPtCurs.Fields.FindField("NAME");

        while (pPointFeat != null)
        {//loop through all of the fabric points, and if any of the point id values are in the deleted set, 
          //then remove the control name from the point's NAME field
          pPointFeat.set_Value(iPointIDX, DBNull.Value);
          if (Unversioned)
            pPtCurs.UpdateRow(pPointFeat);
          else
            pPointFeat.Store();

          Marshal.ReleaseComObject(pPointFeat); //garbage collection
          pPointFeat = pPtCurs.NextRow();
        }
        Marshal.ReleaseComObject(pPtCurs); //garbage collection
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem resetting point table's control association: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public bool UpdateTableByDictionaryLookup(ITable TheTable, IQueryFilter QueryFilter, string TargetField,
      bool Unversioned, Dictionary<int, int> Lookup)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)TheTable;//used for unversioned table
        IRow pTheFeat = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeat = pUpdateCursor.NextRow();

        Int32 iIDX = pUpdateCursor.Fields.FindField(TargetField.ToUpper());

        while (pTheFeat != null)
        {//loop through all of the features, lookup the object id, then write the value to the 
          //feature's field
          int iVal = Lookup[pTheFeat.OID];
          pTheFeat.set_Value(iIDX, iVal);

          if (Unversioned)
            pUpdateCursor.UpdateRow(pTheFeat);
          else
            pTheFeat.Store();

          Marshal.ReleaseComObject(pTheFeat); //garbage collection
          pTheFeat = pUpdateCursor.NextRow();
        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating feature: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public bool DeleteRowsByFIDSet(ITable inTable, IFIDSet pFIDSet,
      IStepProgressor StepProgressor, ITrackCancel TrackCancel)
    {//this routine uses the GetRows method, avoids the need to break up the InClause.
      if (pFIDSet == null)
        return false;
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
        if (ex.ErrorCode==-2147217400)
          //MessageBox.Show(ex.ErrorCode + Environment.NewLine + ex.Message + 
          //  Environment.NewLine + "This error indicates that the fabric may not have been correctly upgraded.");
          //TODO: need to confirm this.
          m_LastErrorCode = ex.ErrorCode;
        else
          MessageBox.Show(ex.Message + Environment.NewLine + ex.ErrorCode);
        
        m_LastErrorCode = ex.ErrorCode;
        return false;
      }
    }

    public bool DeleteRowsByFIDSetReturnGeomCollection(ITable inTable, IFIDSet pFIDSet,
  IStepProgressor StepProgressor, ITrackCancel TrackCancel, ref IGeometryCollection GeomCollection)
    {//this routine uses the GetRows method, avoids the need to break up the InClause.
      if (pFIDSet == null)
        return false;
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
          if (StepProgressor.Position < StepProgressor.MaxRange)
            StepProgressor.Step();
        }
        while (row != null)
        {
          IFeature pFeat = row as IFeature;
          IGeometry pGeom = pFeat.ShapeCopy;
          if (pGeom != null)
          {
            if (!pGeom.IsEmpty)
            {
              object obj = Type.Missing;
              IEnvelope2 pEnv = (IEnvelope2)pGeom.Envelope;
              pEnv.Expand(0.1, 0.1, false);
              GeomCollection.AddGeometry(pEnv, ref obj, ref obj);
            }
          }
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
        if (ex.ErrorCode == -2147217400)
          //MessageBox.Show(ex.ErrorCode + Environment.NewLine + ex.Message + 
          //  Environment.NewLine + "This error indicates that the fabric may not have been correctly upgraded.");
          //TODO: need to confirm this.
          m_LastErrorCode = ex.ErrorCode;
        else
          MessageBox.Show(ex.Message + Environment.NewLine + ex.ErrorCode);

        m_LastErrorCode = ex.ErrorCode;
        return false;
      }
    }


    public bool DeleteRowsUnversioned(IWorkspace TheWorkSpace, ITable inTable,
      IFIDSet pFIDSet, IStepProgressor StepProgressor, ITrackCancel TrackCancel)
    {
      IMouseCursor pMouseCursor = new MouseCursorClass();
      pMouseCursor.SetCursor(2);

      Debug.WriteLine(StepProgressor.Position);
      Debug.WriteLine(StepProgressor.MaxRange);
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
              //if (pQF != null)
              //  Marshal.ReleaseComObject(pQF);
              return false;
            }
            Marshal.ReleaseComObject(ipCursor);
          }
          //Marshal.ReleaseComObject(pQF);
        }
        Debug.WriteLine(StepProgressor.Position);
        return true;
      }

      catch (Exception ex)
      {
        if (ipCursor != null)
          Marshal.ReleaseComObject(ipCursor);
        if (pRow != null)
          Marshal.ReleaseComObject(pRow);
        //if (pQF != null)
        //  Marshal.ReleaseComObject(pQF);
        MessageBox.Show(Convert.ToString(ex.Message));
        return false;
      }
    }

    public bool DeleteByQuery(IWorkspace TheWorkSpace, ITable inTable, IField QueryIntegerField,
      string[] QueryIDs, bool IsVersioned, IStepProgressor StepProgressor, ITrackCancel TrackCancel)
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
      //this code assumes that QueryIDs holds an arraylist of comma separated OIDs with no more than 995 id's per list item
      string sWhereClauseLHS = sPref + QueryIntegerField.Name + sSuff + " in (";
      string[] ids = { sWhereClauseLHS };

      try
      {
        ITableWrite pTableWr = (ITableWrite)inTable;
        bool bCont = true;

        Int32 count = QueryIDs.GetLength(0) - 1;
        for (int k = 0; k <= count; k++)
        {
          pQF.WhereClause = sWhereClauseLHS + QueryIDs[k] + ")"; //left-hand side of the where clause
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

    public bool CanEditFabric(IWorkspace workspace, ITable fabricTable, out string ReturnMessage)
    {
      bool bWorkspaceIsBeingEdited = false;
      bool bHasEditPrivileges = true;
      bool bCanEdit = true;

      string sMessage = "";
      if (workspace.WorkspaceFactory.WorkspaceType == esriWorkspaceType.esriRemoteDatabaseWorkspace)
      {
        bHasEditPrivileges = HasEditPrivileges(fabricTable);
        if (!bHasEditPrivileges)
        {
          ReturnMessage = "You do not have edit privileges.";
          bCanEdit = false;
          return bCanEdit;
        }
        bool bProtected = false;
        bool bIsOwner = IsConnectedAsOwner(workspace, out bProtected);
        if (!bIsOwner && bProtected)
        {
          sMessage = "The version is protected and you are not connected as the owner.";
          bCanEdit = false;
        }
        else
          bCanEdit = true;
      }
      else
      {
        bWorkspaceIsBeingEdited = IsWorkspaceBeingEdited(workspace);
        if (bWorkspaceIsBeingEdited)
        {
          sMessage = "The fabric is currently being edited.";
          bCanEdit = false;
        }
      }
      ReturnMessage = sMessage;
      return bCanEdit;
    
    }

    public string GetVersionName(IWorkspace workspace)
    {
      if (workspace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
      {
        IPropertySet pPropSet = workspace.ConnectionProperties;
        object propertyvalue = pPropSet.GetProperty("VERSION");
        string sVersionName = (string)propertyvalue;
        return sVersionName;
      }
      else
        return "";
    }

    public bool IsConnectedAsOwner(IWorkspace workspace, out bool IsProtectedVersion)
    {
      IsProtectedVersion = false;
      if (workspace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
      {
        IPropertySet pPropSet = workspace.ConnectionProperties;
        object propertyvalue = pPropSet.GetProperty("VERSION");
        string sVersionName = (string)propertyvalue;
        IVersionedWorkspace versionedWorkspace = (IVersionedWorkspace)workspace;
        IVersion pVersion = versionedWorkspace.FindVersion(sVersionName);
        IVersionInfo pVInfo = pVersion.VersionInfo;
        IsProtectedVersion=(pVInfo.Access==esriVersionAccess.esriVersionAccessProtected);
        return pVInfo.IsOwner();
      }
      else
        return true;
    }

    private bool IsWorkspaceBeingEdited(IWorkspace workspace)
    {
      //Get a reference to the editor.
      bool bIsBeingEdited = true;
      UID uid = new UIDClass();
      uid.Value = "esriEditor.Editor";

      IApplication pApp = (IApplication)ArcMap.Application;
      if (pApp == null)
        //if the app is null then assume running from ArcCatalog, and return false
        return false;

      IEditor editor = pApp.FindExtensionByCLSID(uid) as IEditor;
      if (editor.EditState != esriEditState.esriStateNotEditing)
      {
        if (editor.EditWorkspace.Equals(workspace))
          bIsBeingEdited = true;
        else
          bIsBeingEdited = false;
      }
      else 
      {
        bIsBeingEdited = false;
      }
      return bIsBeingEdited;
    }

    private bool HasEditPrivileges(ITable TheTable)
    {
      string sPrivs = PrivilegesOnTable(TheTable);
      if (sPrivs.Contains("Delete"))
        return true;
      else
        return false;
    }

    private string PrivilegesOnTable(ITable TheTable)
    {
      string sReport; 
      IDataset pDS2 = (IDataset)TheTable;
      IDatasetName pDSName = (IDatasetName)pDS2.FullName;
      if (pDSName is ISQLPrivilege)
      {
        ISQLPrivilege pSQLPriv = (ISQLPrivilege)pDSName;
        int ThePrivileges = pSQLPriv.SQLPrivileges;
        string sPrivileges = PrivilegesBitWiseParser((esriSQLPrivilege)pSQLPriv.SQLPrivileges);
        sReport = sPrivileges;
      }
      else
      {
        sReport = "";
      }
      return sReport;
    }

    private string PrivilegesBitWiseParser(esriSQLPrivilege val)
    {
      // esriSelectPrivilege    1
      // esriUpdatePrivilege    2
      // esriInsertPrivilege    4
      // esriDeletePrivilege    8

      string sPrivileges = "";
      //test for
      if (val == esriSQLPrivilege.esriSelectPrivilege)
      {
        sPrivileges += " Select ";
      }
      if ((int)val == (int)esriSQLPrivilege.esriSelectPrivilege + (int)esriSQLPrivilege.esriUpdatePrivilege)
      {
        sPrivileges += " Select, Update";
      }

      if ((int)val == (int)esriSQLPrivilege.esriSelectPrivilege + (int)esriSQLPrivilege.esriDeletePrivilege)
      {
        sPrivileges += " Select, Delete";
      }

      if ((int)val == (int)esriSQLPrivilege.esriSelectPrivilege + (int)esriSQLPrivilege.esriInsertPrivilege)
      {
        sPrivileges += " Select, Insert";
      }

      if ((int)val == (int)esriSQLPrivilege.esriSelectPrivilege + (int)esriSQLPrivilege.esriUpdatePrivilege
        + (int)esriSQLPrivilege.esriInsertPrivilege)
      {
        sPrivileges += " Select, Update, Insert";
      }

      if ((int)val == (int)esriSQLPrivilege.esriSelectPrivilege + (int)esriSQLPrivilege.esriInsertPrivilege
        + (int)esriSQLPrivilege.esriDeletePrivilege)
      {
        sPrivileges += " Select, Insert, Delete";
      }

      if ((int)val == (int)esriSQLPrivilege.esriSelectPrivilege + (int)esriSQLPrivilege.esriUpdatePrivilege
        + (int)esriSQLPrivilege.esriDeletePrivilege)
      {
        sPrivileges += " Select, Update, Delete";
      }

      if ((int)val == (int)esriSQLPrivilege.esriSelectPrivilege + (int)esriSQLPrivilege.esriUpdatePrivilege
        + (int)esriSQLPrivilege.esriInsertPrivilege + (int)esriSQLPrivilege.esriDeletePrivilege)
      {
        sPrivileges += " Select, Update, Insert, Delete";
      }
      return sPrivileges;
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

    public bool GetListFromQuery(ITable inTable, IQueryFilter theQueryFilter, out List<int> ResultList, bool ShowProgressor, ITrackCancel TrackCancel)
    {
      List<int> TheReturnList = new List<int>();
      ICursor pCur = inTable.Search(theQueryFilter, false);
      int iCursorCnt = 0;
      IRow pRow = pCur.NextRow();
      bool bCont = true;
      while (pRow != null)
      {
        iCursorCnt++;
        int iThisID = (int)pRow.OID;
        TheReturnList.Add(iThisID);
        Marshal.FinalReleaseComObject(pRow);
        //after garbage collection, and before getting the next row,
        //check if the cancel button was pressed. If so, stop process

        //Check if the cancel button was pressed. If so, stop process
        if (ShowProgressor)
          bCont = TrackCancel.Continue();
        if (!bCont)
          break;
       
        pRow = pCur.NextRow();
      }
      if (!bCont)
        TheReturnList = null;

      ResultList = TheReturnList;
      return bCont;
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

    public long[] RedimPreserveLng(ref long[] x, long ResizeIncrement)
    {
      long[] Temp1 = new long[x.GetLength(0) + ResizeIncrement];
      if (x != null)
        System.Array.Copy(x, Temp1, x.Length);
      x = Temp1;
      Temp1 = null;
      return x;
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
        pJob.Description = "Delete selected parcels";
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
          return false;
        }
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

        return true;
      }
      return false;
    }

    public double ConvertMetersToFabricUnits(double InDistance, ICadastralFabric Fabric)
    {
      IProjectedCoordinateSystem2 pPCS;
      ILinearUnit pMapLU;
      double dMetersPerMapUnit = 1;

      IGeoDataset pGDS = (IGeoDataset)Fabric;

      ISpatialReference2 pSpatRef = (ISpatialReference2)pGDS.SpatialReference;
      if (pSpatRef == null)
        return InDistance;

      if (pSpatRef is IProjectedCoordinateSystem2)
      {
        pPCS = (IProjectedCoordinateSystem2)pSpatRef;
        pMapLU = pPCS.CoordinateUnit;
        dMetersPerMapUnit = pMapLU.MetersPerUnit;
        double dRes = InDistance / dMetersPerMapUnit;
        return dRes;
      }
      return InDistance;
    }

    public bool InfoFromCurrentTool(out string CommBarUID, out string CommUID, out string Name)
    {
      try
      {
        ICommandItem pCommItem = ArcMap.Application.CurrentTool;
        Name = pCommItem.Name;
        CommUID = pCommItem.ID.Value.ToString();
        ICommandBar pCommBar = pCommItem.Parent;
        ICommandItem pCommItem2 = (ICommandItem)pCommBar;
        CommBarUID = pCommItem2.ID.Value.ToString();
        return true;
      }
      catch 
      {
        Name = "";
        CommUID = "";
        CommBarUID = "";
        return false;
      }
    }

    public void ExecuteCommand(string CommUID)
    {
      if (CommUID.Trim() == "")
        return;
      ICommandBars pCommBars = ArcMap.Application.Document.CommandBars;
      IUID pIDComm = new UIDClass();
      pIDComm.Value = CommUID;
      ICommandItem pCommItem = pCommBars.Find(pIDComm);
      pCommItem.Execute();
    }

    public void FlashGeometry(IGeometry Geom, IScreenDisplay Display, IColor Color, int Size, int Interval)
    {
      if (Geom == null)
        return;
      short Cache = Display.ActiveCache;
      Display.ActiveCache = (short)esriScreenCache.esriNoScreenCache;
      Display.StartDrawing(0, Cache);

      if (Geom.GeometryType == esriGeometryType.esriGeometryLine || Geom.GeometryType == esriGeometryType.esriGeometryCircularArc)
      {
        ILineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
        ISymbol pSymbol = (ISymbol)pSimpleLineSymbol;
        pSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen; //erase itself when drawn twice
        pSimpleLineSymbol.Width = Size;
        pSimpleLineSymbol.Color = Color;
        Display.SetSymbol((ISymbol)pSimpleLineSymbol);
        ISegmentCollection pPath = new PathClass();
        pPath.AddSegment((ISegment)Geom);
        IGeometryCollection pPolyL = new PolylineClass();
        pPolyL.AddGeometry((IGeometry)pPath);
        Display.DrawPolyline((IGeometry)pPolyL);
        System.Threading.Thread.Sleep(Interval);
        Display.DrawPolyline((IGeometry)pPolyL);
       }
      else if (Geom.GeometryType == esriGeometryType.esriGeometryPolyline)
      {
        ILineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
        ISymbol pSymbol = (ISymbol)pSimpleLineSymbol; //'QI
        pSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen; //erase itself when drawn twice
        pSimpleLineSymbol.Width = Size;
        pSimpleLineSymbol.Color = Color;
        Display.SetSymbol((ISymbol)pSimpleLineSymbol);
        Display.DrawPolyline(Geom);
        System.Threading.Thread.Sleep(Interval);
        Display.DrawPolyline(Geom);
      }
      else if (Geom.GeometryType == esriGeometryType.esriGeometryPolygon)
      {
        ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
        ISymbol pSymbol = (ISymbol)pSimpleFillSymbol;
        pSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen; //erase itself when drawn twice
        pSimpleFillSymbol.Color = Color;
        Display.SetSymbol((ISymbol)pSimpleFillSymbol);
        Display.DrawPolygon(Geom);
        System.Threading.Thread.Sleep(Interval);
        Display.DrawPolygon(Geom);
      }
      else if (Geom.GeometryType == esriGeometryType.esriGeometryPoint)
      {
        ISimpleMarkerSymbol pSimpleMarkersymbol = new SimpleMarkerSymbolClass();
        ISymbol pSymbol = (ISymbol)pSimpleMarkersymbol;
        pSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
        pSimpleMarkersymbol.Color = Color;
        pSimpleMarkersymbol.Size = Size;
        Display.SetSymbol((ISymbol)pSimpleMarkersymbol);
        Display.DrawPoint(Geom);
        System.Threading.Thread.Sleep(Interval);
        Display.DrawPoint(Geom);
      }
      Display.FinishDrawing();
      //reset the cache
      Display.ActiveCache = Cache;
    }

    public List<int> DifferenceBetweenLists(List<int> List1, List<int> List2)
    {
      //List1 minus List2
      List<int> differences = new List<int>();
      try
      {
        List1.Sort();
        List2.Sort();
        //List<int> newList = List1.Intersect(List2).ToList<int>(); //Linq method..memory hog on large datasets
        //List1.RemoveAll(a => newList.Contains(a)); //this is too slow on large datasets
        foreach (int i in List1)
        {
          int iFoundItem = List2.BinarySearch(i);
          if (iFoundItem < 0)
            differences.Add(i);
        }
        return differences;
      }
      catch
      {
        return differences;
      }
    }

    public List<int> IntersectLists(List<int> List1, List<int> List2)
    {
      var firstCount = List1.Count;
      var secondCount = List2.Count;
      int firstIndex = 0, secondIndex = 0;
      var intersection = new List<int>();
      while (firstIndex < firstCount && secondIndex < secondCount)
      {
        var comp = List1[firstIndex].CompareTo(List2[secondIndex]);
        if (comp < 0)
          ++firstIndex;
        else if (comp > 0)
          ++secondIndex;
        else
        {
          intersection.Add(List1[firstIndex]);
          ++firstIndex;
          ++secondIndex;
        }
      }
      return intersection;
    }

    //public RegistryFindKey()
    //{

    ////    '------------4/2/2012 ---------------
    ////'first get the service pack info to see if this code needs to run
    ////Dim sAns As String
    ////Dim bIsSP4 As Boolean = False
    ////Dim sErr As String = ""
    ////sAns = RegValue(RegistryHive.LocalMachine, "SOFTWARE\ESRI\Desktop10.0\CoreRuntime", "InstallVersion", sErr)
    ////bIsSP4 = (sAns = "10.0.4000")

    ////'also this workaround does not work in Manual Mode
    ////'check if there is a Manual Mode "modify" job active ===========
    //}

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
        objParent=Registry.ClassesRoot;    

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

  }
}