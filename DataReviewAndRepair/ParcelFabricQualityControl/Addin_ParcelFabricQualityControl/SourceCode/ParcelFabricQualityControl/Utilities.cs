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

using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace ParcelFabricQualityControl
{
  class Utilities
  {
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

    public bool GetFabricSubLayers(IMap Map, esriCadastralFabricTable FabricSubClass, bool ExcludeNonTargetFabrics,
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

    public void FIDsetToLongArray(IFIDSet InFIDSet, ref ILongArray OutLongArray, ref int[] OutIntArray,IStepProgressor StepProgressor)
    {
      Int32 pfID = -1;
      InFIDSet.Reset();
      double dMax = InFIDSet.Count();
      int iMax = (int)(dMax);
      for (Int32 pCnt = 0; pCnt <= (InFIDSet.Count() - 1); pCnt++)
      {
        InFIDSet.Next(out pfID);
        OutLongArray.Add(pfID);
        OutIntArray[pCnt] = pfID;
        if (StepProgressor != null)
        {
          if (StepProgressor.Position < StepProgressor.MaxRange)
            StepProgressor.Step();
        }
      }
      return;
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

public bool UpdateTableByDictionaryLookup(ITable TheTable, IQueryFilter QueryFilter, string TargetField,
      bool Unversioned, Dictionary<int, double> Lookup)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
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
          double dVal = Lookup[pTheFeat.OID];
          pTheFeat.set_Value(iIDX, dVal);

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

    public bool UpdateCOGOByDictionaryLookups(ITable TheTable, IQueryFilter QueryFilter,
  bool Unversioned, Dictionary<int, double> LookupLines, Dictionary<int, List<double>> LookupCurves)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
        IRow pTheFeat = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeat = pUpdateCursor.NextRow();

        Int32 iDISTANCE_IDX = pUpdateCursor.Fields.FindField("DISTANCE");
        Int32 iRADIUS_IDX = pUpdateCursor.Fields.FindField("RADIUS");
        Int32 iARCLENGTH_IDX = pUpdateCursor.Fields.FindField("ARCLENGTH");
        Int32 iDELTA_IDX = pUpdateCursor.Fields.FindField("DELTA");

        while (pTheFeat != null)
        {//loop through all of the features, lookup the object id, then write the value to the 
          //feature's field
          bool bIsCurve=LookupCurves.ContainsKey(pTheFeat.OID);
          if (bIsCurve)
          {
            List<double> lstCurveParams = LookupCurves[pTheFeat.OID];
            pTheFeat.set_Value(iDISTANCE_IDX, lstCurveParams[0]);
            pTheFeat.set_Value(iRADIUS_IDX, lstCurveParams[1]);
            pTheFeat.set_Value(iARCLENGTH_IDX, lstCurveParams[2]);
            pTheFeat.set_Value(iDELTA_IDX, lstCurveParams[3]);
          }
          else
          {
            double dVal = LookupLines[pTheFeat.OID];
            pTheFeat.set_Value(iDISTANCE_IDX, dVal);
          }

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
        MessageBox.Show("Problem updating COGO feature: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public bool UpdateRadialLineBearingsByDictionaryLookups(ITable TheTable, IQueryFilter QueryFilter, bool Unversioned, 
      Dictionary<int, double> LookupLinesToComputedDirection, Dictionary<int, List<int>> LookupRadialIdentifiers, Dictionary<int, double> LookupCurveDelta)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
        IRow pTheFeat = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeat = pUpdateCursor.NextRow();

        Int32 iParcelID_IDX = pUpdateCursor.Fields.FindField("PARCELID");
        Int32 iFromPt_IDX = pUpdateCursor.Fields.FindField("FROMPOINTID");
        Int32 iToPt_IDX = pUpdateCursor.Fields.FindField("TOPOINTID");

        Int32 iBearing_IDX = pUpdateCursor.Fields.FindField("BEARING");

        while (pTheFeat != null)
        {//loop through all of the radial lines, lookup the match information, then compute and write the new bearing to the 
          //feature's field, using the central angle

          int iParcelIDSrc = Convert.ToInt32(pTheFeat.get_Value(iParcelID_IDX));
          int iFromPointIDSrc = Convert.ToInt32(pTheFeat.get_Value(iFromPt_IDX));
          int iCenterPointIDSrc = Convert.ToInt32(pTheFeat.get_Value(iToPt_IDX));

          IDictionaryEnumerator TheEnum= LookupRadialIdentifiers.GetEnumerator();
          TheEnum.Reset();
          bool bItsAMatch = false;
          while(TheEnum.MoveNext())
          {
            List<int> lstRadialIdentifiers = TheEnum.Value as List<int>;
            
            int iParcelID = lstRadialIdentifiers[0];
            int iToPointID = lstRadialIdentifiers[1];

            int iFromPointIDAtCurveStart = lstRadialIdentifiers[2];
            int iFromPointIDAtCurveEnd = lstRadialIdentifiers[3];

            bItsAMatch = (iParcelIDSrc == iParcelID && iCenterPointIDSrc == iToPointID &&
              (iFromPointIDSrc == iFromPointIDAtCurveStart || iFromPointIDSrc == iFromPointIDAtCurveEnd));

            if (bItsAMatch)
            {
              bool bCCW = Convert.ToBoolean(lstRadialIdentifiers[4]);
              double dCentralAngle= LookupCurveDelta[(int)TheEnum.Key];
              double d180 = 180;
              if (bCCW)
              {
                dCentralAngle = dCentralAngle * -1;
                d180 = 0;
              }

              if (!LookupLinesToComputedDirection.ContainsKey((int)TheEnum.Key))
                continue;

              double dNewAttributeChordBearing = LookupLinesToComputedDirection[(int)TheEnum.Key]; //decimal degrees north azimuth
              if (iFromPointIDSrc == iFromPointIDAtCurveStart)
              {
                double dRadialBearing = d180 + dNewAttributeChordBearing - ((dCentralAngle / 2) + 90);
                IAngularConverter pAngConv = new AngularConverterClass(); //next lines account for negative bearing
                if (pAngConv.SetAngle(dRadialBearing, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees))
                  dRadialBearing=pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth,esriDirectionUnits.esriDUDecimalDegrees);
                pTheFeat.set_Value(iBearing_IDX, dRadialBearing);
              }
              else if (iFromPointIDSrc == iFromPointIDAtCurveEnd)
              {
                double dRadialBearing = d180 + dNewAttributeChordBearing + ((dCentralAngle / 2) + 270);
                IAngularConverter pAngConv = new AngularConverterClass(); //next lines account for negative bearing
                if (pAngConv.SetAngle(dRadialBearing, esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees))
                  dRadialBearing = pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
                pTheFeat.set_Value(iBearing_IDX, dRadialBearing);
              }

              if (Unversioned)
                pUpdateCursor.UpdateRow(pTheFeat);
              else
                pTheFeat.Store();
             
              break;
            }
          }
          Marshal.ReleaseComObject(pTheFeat); //garbage collection
          pTheFeat = pUpdateCursor.NextRow();
        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating COGO feature: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public bool UpdateRadialLineDistancesByDictionaryLookups(ITable TheTable, IQueryFilter QueryFilter, bool Unversioned,
  Dictionary<int, List<double>> LookupLinesToCircularCurve, Dictionary<int, List<int>> LookupRadialIdentifiers)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)TheTable; //used for unversioned table
        IRow pTheFeat = null;
        ICursor pUpdateCursor = null;

        if (Unversioned)
          pUpdateCursor = pTableWr.UpdateRows(QueryFilter, false);
        else
          pUpdateCursor = TheTable.Update(QueryFilter, false);

        pTheFeat = pUpdateCursor.NextRow();

        Int32 iParcelID_IDX = pUpdateCursor.Fields.FindField("PARCELID");
        Int32 iFromPt_IDX = pUpdateCursor.Fields.FindField("FROMPOINTID");
        Int32 iToPt_IDX = pUpdateCursor.Fields.FindField("TOPOINTID");

        Int32 iDistance_IDX = pUpdateCursor.Fields.FindField("DISTANCE");

        while (pTheFeat != null)
        {//loop through all of the radial lines, lookup the match information, then write the new radius to the 
          //feature's distance field

          int iParcelIDSrc = Convert.ToInt32(pTheFeat.get_Value(iParcelID_IDX));
          int iFromPointIDSrc = Convert.ToInt32(pTheFeat.get_Value(iFromPt_IDX));
          int iCenterPointIDSrc = Convert.ToInt32(pTheFeat.get_Value(iToPt_IDX));

          IDictionaryEnumerator TheEnum = LookupRadialIdentifiers.GetEnumerator();
          TheEnum.Reset();
          bool bItsAMatch = false;
          while (TheEnum.MoveNext())
          {
            List<int> lstRadialIdentifiers = TheEnum.Value as List<int>;

            int iParcelID = lstRadialIdentifiers[0];
            int iToPointID = lstRadialIdentifiers[1];

            int iFromPointIDAtCurveStart = lstRadialIdentifiers[2];
            int iFromPointIDAtCurveEnd = lstRadialIdentifiers[3];

            bItsAMatch = (iParcelIDSrc == iParcelID && iCenterPointIDSrc == iToPointID &&
              (iFromPointIDSrc == iFromPointIDAtCurveStart || iFromPointIDSrc == iFromPointIDAtCurveEnd));

            if (bItsAMatch)
            {
              List<double> lstNewCOGOValues = LookupLinesToCircularCurve[(int)TheEnum.Key] as List<double>;
              double dNewRadius = lstNewCOGOValues[1];
              pTheFeat.set_Value(iDistance_IDX, Math.Abs(dNewRadius));
              if (Unversioned)
                pUpdateCursor.UpdateRow(pTheFeat);
              else
                pTheFeat.Store();

              break;
            }
          }
          Marshal.ReleaseComObject(pTheFeat); //garbage collection
          pTheFeat = pUpdateCursor.NextRow();
        }
        Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating COGO feature: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }

    public ISelectionSet2 GetSelectionFromLayer(ICadastralFabricSubLayer FabricSubLayer)
    {
      IFeatureSelection pFeatSel = (IFeatureSelection)FabricSubLayer;
      ISelectionSet2 pSelSet = (ISelectionSet2)pFeatSel.SelectionSet;
      return pSelSet;
    }

    public bool UpdateParcelSystemFieldsByLookup(ITable ParcelsTable, Dictionary<int, List<double>> UpdateSysFieldsLookup,bool IsUnversioned)
    {
      int tokenLimit = 995;
      try
      {
        List<int> lstOidsOfParcels = UpdateSysFieldsLookup.Keys.ToList<int>(); //linq
        List<string> sInClauseList = InClauseFromOIDsList(lstOidsOfParcels, tokenLimit);

        //SysValList.Add(dAz);
        //SysValList.Add(dDist);
        //SysValList.Add(dRatio);
        //SysValList.Add(dRotate);
        //SysValList.Add(dScale);
        //SysValList.Add(dShpErrX);
        //SysValList.Add(dShpErrY);

        Int32 iIDX0 = ParcelsTable.Fields.FindField("MISCLOSEBEARING"); //misclose direction
        string sIDX0 = ParcelsTable.Fields.get_Field(iIDX0).Name;
        Int32 iIDX1 = ParcelsTable.FindField("MISCLOSEDISTANCE"); //misclose distance
        string sIDX1 = ParcelsTable.Fields.get_Field(iIDX1).Name;
        Int32 iIDX2 = ParcelsTable.FindField("MISCLOSERATIO"); //misclose ratio
        string sIDX2 = ParcelsTable.Fields.get_Field(iIDX2).Name;
        Int32 iIDX3 = ParcelsTable.FindField("ROTATION"); //parcel rotation
        string sIDX3 = ParcelsTable.Fields.get_Field(iIDX3).Name;
        Int32 iIDX4 = ParcelsTable.FindField("SCALE"); //parcel scale
        string sIDX4 = ParcelsTable.Fields.get_Field(iIDX4).Name;
        Int32 iIDX5 = ParcelsTable.FindField("SHAPESTDERRORE"); //shape standard error X
        string sIDX5 = ParcelsTable.Fields.get_Field(iIDX5).Name;
        Int32 iIDX6 = ParcelsTable.FindField("SHAPESTDERRORN"); //shape standard error Y
        string sIDX6 = ParcelsTable.Fields.get_Field(iIDX6).Name;

        string sOIDName = ParcelsTable.OIDFieldName;
        ITableWrite pTableWr = (ITableWrite)ParcelsTable; //used for unversioned table
        IRow pTheParcel = null;
        ICursor pUpdateCursor = null;
        IQueryFilter pQuFilter = new QueryFilterClass();
        pQuFilter.SubFields = sOIDName + "," + sIDX0 + "," + sIDX1 + "," + sIDX2 + "," + sIDX3 + "," + sIDX4 + "," + sIDX5 + "," + sIDX6;
        foreach (string sInclause in sInClauseList)
        {
          string sQuery = sOIDName + " IN (" + sInclause + ")";
          pQuFilter.WhereClause = sQuery;
          if (IsUnversioned)
            pUpdateCursor = pTableWr.UpdateRows(pQuFilter, false);
          else
            pUpdateCursor = ParcelsTable.Update(pQuFilter, false);

          pTheParcel = pUpdateCursor.NextRow();

          while (pTheParcel != null)
          {//loop through all of the features, lookup the object id, then write the system value to the 
            //parcel's fields
            List<double> ThisParcelsInfo = UpdateSysFieldsLookup[pTheParcel.OID];
            double dVal0 = ThisParcelsInfo[0];
            double dVal1 = ThisParcelsInfo[1];
            double dVal2 = ThisParcelsInfo[2];
            double dVal3 = ThisParcelsInfo[3] < 0 ? ThisParcelsInfo[3] + 360 : ThisParcelsInfo[3];
            double dVal4 = ThisParcelsInfo[4];
            double dVal5 = ThisParcelsInfo[5];
            double dVal6 = ThisParcelsInfo[6];

            pTheParcel.set_Value(iIDX0, dVal0);
            pTheParcel.set_Value(iIDX1, dVal1);
            pTheParcel.set_Value(iIDX2, dVal2);
            pTheParcel.set_Value(iIDX3, dVal3);
            pTheParcel.set_Value(iIDX4, dVal4);
            pTheParcel.set_Value(iIDX5, dVal5);
            pTheParcel.set_Value(iIDX6, dVal6);

            if (IsUnversioned)
              pUpdateCursor.UpdateRow(pTheParcel);
            else
              pTheParcel.Store();

            Marshal.ReleaseComObject(pTheParcel); //garbage collection
            pTheParcel = pUpdateCursor.NextRow();
          }
          Marshal.ReleaseComObject(pUpdateCursor); //garbage collection
        }
        return true;
      }
      catch
      {
        return false;
      }
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
    public string GetDesktopBuildNumberFromRegistry()
    {
      string sBuild = "";
      try
      {
        string s = ReadFromRegistry(RegistryHive.LocalMachine, "Software\\ESRI\\ArcGIS", "RealVersion");
        string[] Values = s.Split('.');
        sBuild = Values[2];
        return sBuild;
      }
      catch (Exception)
      {
        return sBuild;
      }
    }
    public Dictionary<int, List<double>> ReComputeParcelSystemFieldsFromLines(ICadastralEditor pCadEd, ISpatialReference MapSpatialReference, 
      IFeatureClass pFabricParcelsClass, int[] IDsOfParcels)
    {
      IGeoDatabaseBridge2 IGDBBridge = new GeoDatabaseHelperClass();
      IFeatureCursor pFeatCurs = IGDBBridge.GetFeatures(pFabricParcelsClass, IDsOfParcels, false);
      IArray pParcelFeatArr = new ArrayClass();
      IGeoDataset pGeoDS = (IGeoDataset)pFabricParcelsClass.FeatureDataset;
      ISpatialReference pFabricSR = pGeoDS.SpatialReference;
      double dMetersPerUnit = 1;
      bool bFabricIsInGCS = !(pFabricSR is IProjectedCoordinateSystem);
      if (!bFabricIsInGCS)
      {
        IProjectedCoordinateSystem pPCS = (IProjectedCoordinateSystem)pFabricSR;
        dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
      }
      else
      {
        IProjectedCoordinateSystem pPCS = (IProjectedCoordinateSystem)MapSpatialReference;
        dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
      }
      IFeature pFeat = pFeatCurs.NextFeature();
      while (pFeat != null)
      {
        pParcelFeatArr.Add(pFeat);
        Marshal.ReleaseComObject(pFeat);
        pFeat = pFeatCurs.NextFeature();
      }
      Marshal.ReleaseComObject(pFeatCurs);

      ICadastralFeatureGenerator pFeatureGenerator = new CadastralFeatureGeneratorClass();
      IEnumGSParcels pEnumGSParcels = pFeatureGenerator.CreateParcelsFromFeatures(pCadEd, pParcelFeatArr, true);
      pEnumGSParcels.Reset();

      Dictionary<int, int> dict_ParcelAndStartPt = new Dictionary<int, int>();
      Dictionary<int, IPoint> dict_PointID2Point = new Dictionary<int, IPoint>();
      //dict_PointID2Point: this lookup makes an assumption that the fabric TO point geometry is at the same location as the line *geometry* endpoint
      IParcelLineFunctions3 ParcelLineFx = new ParcelFunctionsClass();
      Dictionary<int, List<double>> dict_SysFlds = new Dictionary<int, List<double>>();
      IAngularConverter pAngConv = new AngularConverterClass();

      IGSParcel pGSParcel = pEnumGSParcels.Next();
      int iFromPtIDX = -1;
      int iToPtIDX = -1;
      int iParcelIDX = -1;

      while (pGSParcel != null)
      {
        IEnumCELines pCELines = new EnumCELinesClass();
        IEnumGSLines pEnumGSLines = (IEnumGSLines)pCELines;

        IEnumGSLines pGSLinesInner = pGSParcel.GetParcelLines(null, false);
        pGSLinesInner.Reset();
        IGSParcel pTemp = null;
        IGSLine pGSLine = null;
        ICadastralFeature pCF = (ICadastralFeature)pGSParcel;
        int iParcID = pCF.Row.OID;
        pGSLinesInner.Next(ref pTemp, ref pGSLine);
        bool bStartPointAdded = false;
        int iFromPtID = -1;
        while (pGSLine != null)
        {
          if (pGSLine.Category == esriCadastralLineCategory.esriCadastralLineBoundary || 
            pGSLine.Category == esriCadastralLineCategory.esriCadastralLineRoad  ||
            pGSLine.Category == esriCadastralLineCategory.esriCadastralLinePartConnection)
          {
            pCELines.Add(pGSLine);

            ICadastralFeature pCadastralLineFeature = (ICadastralFeature)pGSLine;
            IFeature pLineFeat = (IFeature)pCadastralLineFeature.Row;
            if (iFromPtIDX == -1)
              iFromPtIDX = pLineFeat.Fields.FindField("FROMPOINTID");
            if (iToPtIDX == -1)
              iToPtIDX = pLineFeat.Fields.FindField("TOPOINTID");
            if (iParcelIDX == -1)
              iParcelIDX = pLineFeat.Fields.FindField("PARCELID");

            if (!bStartPointAdded)
            {
              iFromPtID = (int)pLineFeat.get_Value(iFromPtIDX);
              dict_ParcelAndStartPt.Add(iParcID, iFromPtID);
              bStartPointAdded = true;
            }
            IPolyline pPolyline = (IPolyline)pLineFeat.ShapeCopy;
            if (bFabricIsInGCS)
              pPolyline.Project(MapSpatialReference);
            //dict_PointID2Point: this lookup makes an assumption that the fabric TO point geometry is at the same location as the line *geometry* endpoint
            int iToPtID = (int)pLineFeat.get_Value(iToPtIDX);
            //first make sure the point is not already added
            if (!dict_PointID2Point.ContainsKey(iToPtID))
              dict_PointID2Point.Add(iToPtID, pPolyline.ToPoint);
          }
          pGSLinesInner.Next(ref pTemp, ref pGSLine);
        }
        IGSForwardStar pFwdStar = ParcelLineFx.CreateForwardStar(pEnumGSLines);
        //forward star is created for this parcel, now ready to find misclose for the parcel
        //for (int i = 0; i < IDsOfParcels.Count(); i++)
        //{
        List<int> LineIdsList = new List<int>();
        List<IVector3D> TraverseCourses = new List<IVector3D>();
        List<int> FabricPointIDList = new List<int>();
        bool bPass = false;
        if(!bFabricIsInGCS)
          bPass = GetParcelTraverse(ref pFwdStar, iFromPtID, dMetersPerUnit,
            ref LineIdsList, ref TraverseCourses, ref FabricPointIDList, 0, -1, -1, false);
        else
          bPass = GetParcelTraverse(ref pFwdStar, iFromPtID, dMetersPerUnit * dMetersPerUnit, 
            ref LineIdsList, ref TraverseCourses, ref FabricPointIDList, 0, -1, -1, false);
        List<double> SysValList = new List<double>();
        IVector3D MiscloseVector = null;
        IPoint[] FabricPoints = new IPoint[FabricPointIDList.Count];//from control

        int f = 0;
        foreach (int j in FabricPointIDList)
          FabricPoints[f++] = dict_PointID2Point[j];

        double dRatio = 10000;
        f = FabricPointIDList.Count - 1;
        IPoint[] AdjustedTraversePoints = BowditchAdjust(TraverseCourses, FabricPoints[f], FabricPoints[f], out MiscloseVector, out dRatio);//to control

        if (!pAngConv.SetAngle(MiscloseVector.Azimuth + Math.PI / 2, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians))
          continue;
        double dAz = pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
        SysValList.Add(dAz);
        double dDist = MiscloseVector.Magnitude;
        if (bFabricIsInGCS)
          dDist = dDist * dMetersPerUnit;
        SysValList.Add(dDist);
        double dRotate; double dScale; double dShpErrX; double dShpErrY;
        ComputeShapeDistortionParameters(FabricPoints, AdjustedTraversePoints, out dRotate, out dScale, out dShpErrX, out dShpErrY);
        dRotate = dRotate * 180 / Math.PI;
        SysValList.Add(dRatio);
        SysValList.Add(dRotate);
        SysValList.Add(dScale);
        SysValList.Add(dShpErrX);
        SysValList.Add(dShpErrY);
        dict_SysFlds.Add(pGSParcel.DatabaseId, SysValList);



        pGSParcel = pEnumGSParcels.Next();
      //}

      

      }
      return dict_SysFlds;
    }
    private bool ComputeShapeDistortionParameters(IPoint[] FabricPoints, IPoint[] AdjustedTraversePoints, out double RotationInRadians,
          out double Scale, out double ShpStdErrX, out double ShpStdErrY)
    {
      Scale = 1;
      RotationInRadians = 0;
      ShpStdErrX = 0;
      ShpStdErrY = 0;

      IAffineTransformation2D3GEN affineTransformation2D = new AffineTransformation2DClass();
      //affineTransformation2D.DefineFromControlPoints(ref FabricPoints, ref AdjustedTraversePoints);
      affineTransformation2D.DefineConformalFromControlPoints(ref FabricPoints, ref AdjustedTraversePoints);
      RotationInRadians = affineTransformation2D.Rotation;
      Scale = affineTransformation2D.XScale;
      Scale = affineTransformation2D.YScale;
      Scale = 1 / Scale; //the inverse of this computed scale is stored
      int iLen = FabricPoints.GetLength(0) * 2;
      double[] inPoints = new double[iLen];
      double[] outPoints = new double[iLen];
      double[] adjTrav = new double[iLen];

      int x = 0;
      int y = 0;
      for (int j = 0; j < FabricPoints.GetLength(0); j++)
      {
        x = j * 2;
        y = x + 1;
        inPoints[x] = FabricPoints[j].X;//x
        inPoints[y] = FabricPoints[j].Y;//y

        adjTrav[x] = AdjustedTraversePoints[j].X;//x
        adjTrav[y] = AdjustedTraversePoints[j].Y;//y
      }

      affineTransformation2D.TransformPointsFF(esriTransformDirection.esriTransformForward, ref inPoints, ref outPoints);

      //now build a list of the diffs for x and y between transformed and the AdjustedTraverse Results

      int iLen2 = FabricPoints.GetLength(0);
      double[] errX = new double[iLen2];
      double[] errY = new double[iLen2];
      x = 0;
      y = 0;
      double dSUMX = 0;
      double dSUMY = 0;

      for (int j = 0; j < iLen2; j++)
      {
        x = j * 2;
        y = x + 1;
        errX[j] = adjTrav[x] - outPoints[x] + 100000;//x
        errY[j] = adjTrav[y] - outPoints[y] + 100000;//y
        dSUMX += errX[j];
        dSUMY += errY[j];
      }
      double dMean = 0; double dStdDevX = 0; double dStdDevY = 0; double dRange; int iOutliers = 0;
      GetStatistics(errX, dSUMX, 1, out dMean, out dStdDevX, out dRange, out iOutliers);
      GetStatistics(errY, dSUMY, 1, out dMean, out dStdDevY, out dRange, out iOutliers);
      ShpStdErrX = dStdDevX;
      ShpStdErrY = dStdDevY;

      return true;
    }
    private IPoint[] BowditchAdjust(List<IVector3D> TraverseCourses, IPoint StartPoint, IPoint EndPoint, out IVector3D MiscloseVector, out double Ratio)
    {
      bool bLoop = (StartPoint.X == EndPoint.X && StartPoint.Y == EndPoint.Y);
      MiscloseVector = null;
      double dSUM = 0;
      Ratio = 10000;
      if (bLoop)
        MiscloseVector = GetClosingVector(TraverseCourses, out dSUM) as IVector3D; //assumes loop traverse
      else
      { ;}//TODO

      if (MiscloseVector.Magnitude > 0.001)
        Ratio = dSUM / MiscloseVector.Magnitude;

      if (Ratio > 10000)
        Ratio = 10000;

      double dRunningSum = 0;
      IPoint[] TraversePoints = new IPoint[TraverseCourses.Count]; //from control
      for (int i = 0; i < TraverseCourses.Count; i++)
      {
        IPoint toPoint = new PointClass();
        IVector3D vec = TraverseCourses[i];
        dRunningSum += vec.Magnitude;

        double dScale = (dRunningSum / dSUM);
        double dXCorrection = MiscloseVector.XComponent * dScale;
        double dYCorrection = MiscloseVector.YComponent * dScale;

        toPoint.PutCoords(StartPoint.X + vec.XComponent, StartPoint.Y + vec.YComponent);
        StartPoint.PutCoords(toPoint.X, toPoint.Y); //re-set the start point to the one just added

        IPoint pAdjustedPoint = new PointClass();
        pAdjustedPoint.PutCoords(toPoint.X - dXCorrection, toPoint.Y - dYCorrection);
        TraversePoints[i] = pAdjustedPoint;
      }
      return TraversePoints;
    }
    private IVector GetClosingVector(List<IVector3D> TraverseCourses, out double SUMofLengths)
    {
      IVector SumVec = null;
      SUMofLengths = 0;
      for (int i = 0; i < TraverseCourses.Count - 1; i++)
      {
        if (i == 0)
        {
          SUMofLengths = TraverseCourses[0].Magnitude + TraverseCourses[1].Magnitude;
          SumVec = TraverseCourses[0].AddVector(TraverseCourses[1]);
        }
        else
        {
          IVector3D SumVec3D = SumVec as IVector3D;
          SUMofLengths += TraverseCourses[i + 1].Magnitude;
          SumVec = SumVec3D.AddVector(TraverseCourses[i + 1]);
        }
      }
      return SumVec;
    }
    public double InverseDistanceByGroundToGrid(ISpatialReference SpatRef, IPoint FromPoint, IPoint ToPoint, double EllipsoidalHeight)
    {

      bool bSpatialRefInGCS = !(SpatRef is IProjectedCoordinateSystem2); 

      IZAware pZAw = (IZAware)FromPoint;
      pZAw.ZAware = true;
      FromPoint.Z = EllipsoidalHeight;

      pZAw = (IZAware)ToPoint;
      pZAw.ZAware = true;
      ToPoint.Z = EllipsoidalHeight;

      ILine pLine = new LineClass();
      pLine.PutCoords(FromPoint, ToPoint);

      ICadastralGroundToGridTools pG2G = new CadastralDataToolsClass();
      
      double dDist1 = pG2G.Inverse3D(SpatRef, false, FromPoint, ToPoint);
      double dDist2 = pG2G.Inverse3D(SpatRef, true, FromPoint, ToPoint); //use true if in GCS

      if (bSpatialRefInGCS)
        dDist1 = dDist2;

      return dDist1;
    }
    public void GetStatistics(double[] InDoubleArray, double InSum, int InSigma1or2or3,
  out double Mean, out double StandardDeviation, out double Range, out int NumberOfOutliers)
    {//SUM is assumed to be readily computed during construction of the array, and avoids another loop here
      Mean = InSum / InDoubleArray.Length;
      double SumSquares = 0;
      double Smallest = 0;
      double Largest = 0;
      NumberOfOutliers = 0;
      for (int i = 0; i < InDoubleArray.Length; i++)
      {
        if (i == 0)
        {
          Smallest = InDoubleArray[i];
          Largest = Smallest;
        }
        else
        {
          if (InDoubleArray[i] > Largest)
            Largest = InDoubleArray[i];
          if (InDoubleArray[i] < Smallest)
            Smallest = InDoubleArray[i];
        }

        double d = InDoubleArray[i] - Mean;
        d = d * d;
        SumSquares += d;
      }

      StandardDeviation = Math.Sqrt(SumSquares / InDoubleArray.Length);
      Range = Largest - Smallest;
      //look for and count outliers within 1, 2 or 3 sigma
      if (InSigma1or2or3 <= 0 || InSigma1or2or3 > 3)
        InSigma1or2or3 = 3;
      double TestValue = InSigma1or2or3 * StandardDeviation;
      for (int i = 0; i < InDoubleArray.Length; i++)
      {
        if ((Math.Abs(InDoubleArray[i] - Mean)) > (TestValue))
          NumberOfOutliers++;
      }


    }
    private bool GetParcelTraverse(ref IGSForwardStar FwdStar, int StartNodeId, double MetersPerUnit,
      ref List<int> LineIdList, ref List<IVector3D> TraverseCourses, ref List<int> PointIdList, int iInfinityChecker, int iPrevFrom, int iPrevTo, bool bBackSightOnPartConnector)
    {
      //forward star object expected to represent a single parcel
      iInfinityChecker++;
      if (iInfinityChecker > 20000)
        return false;
      //(This is a self-calling function.) Set an upper limit of 20000 downstream boundary lines, 
      //so exit gracefully, and avoid probable endless loop.
      //Possible cause of endless loop? Corrupted data; example, a line with the same from and to point id
      try
      {
        ILongArray iLngArr = FwdStar.get_ToNodes(StartNodeId);
        //get_ToNodes returns an array of radiated points, not "TO" points in the fabric data model sense
        int iCnt2 = 0;
        iCnt2 = iLngArr.Count;
        IGSLine pGSLine = null;
        //bool bPartConnector = false;
        for (int i = 0; i < iCnt2; i++)
        {
          int i2 = iLngArr.get_Element(i);
          bool bIsReversed = FwdStar.GetLine(StartNodeId, i2, ref pGSLine);
          
          int iFromPt = pGSLine.FromPoint;
          int iToPt = pGSLine.ToPoint;
          
          bBackSightOnPartConnector = ((iPrevFrom==iToPt && iPrevTo==iFromPt) 
              && pGSLine.Category == esriCadastralLineCategory.esriCadastralLinePartConnection);
          int iDBId = pGSLine.DatabaseId;
          //Debug.Print("Line:" + iDBId.ToString());
          //ICadastralFeature cf = (ICadastralFeature)pGSLine;
          //IRow rr = cf.Row;
          //int iRefParcel = (int)rr.get_Value(ParcelIdFldIdx);
          //if (iRefParcel != ParcelID)
          //  continue;
          //Debug.Print("Parcel:" + iRefParcel.ToString());

          if (!bIsReversed && !bBackSightOnPartConnector)
          {//if the line is running with same orientation as GetLine function
            //---OR---if the line is an origin connection and running with opposite orientation as GetLine function
            if (!LineIdList.Contains(iDBId) ) //&& iRefParcel == ParcelID)
            {
              LineIdList.Add(iDBId);
              IVector3D vec = new Vector3DClass();
              double dDistance = pGSLine.Distance / MetersPerUnit;
              double dBearing = pGSLine.Bearing;
              IPoint pToPt = new PointClass();
              vec.PolarSet(dBearing, 0, dDistance);
              TraverseCourses.Add(vec);
              PointIdList.Add(i2);
            }
            else
              return false;
            if (!GetParcelTraverse(ref FwdStar, i2, MetersPerUnit, ref LineIdList,
                  ref TraverseCourses, ref PointIdList, iInfinityChecker,iFromPt,iToPt, bBackSightOnPartConnector))
              return false;
          }
        }
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool GetAllFabricSubLayers(IEditor Editor, out IArray CFSubLayers)
    {
      ICadastralFabricSubLayer pCFSubLyr = null;
      IArray CFParcelFabricSubLayers2 = new ArrayClass();
      IFeatureLayer pParcelFabricSubLayer = null;
      UID pId = new UIDClass();
      pId.Value = "{BA381F2B-F621-4F45-8F78-101F65B5BBE6}"; //ICadastralFabricSubLayer
      IMap pMap = Editor.Map;
      IEnumLayer pEnumLayer = pMap.get_Layers(pId, true);
      pEnumLayer.Reset();
      ILayer pLayer = pEnumLayer.Next();
      while (pLayer != null)
      {
        pCFSubLyr = (ICadastralFabricSubLayer)pLayer;
        pParcelFabricSubLayer = (IFeatureLayer)pCFSubLyr;
        IDataset pDS = (IDataset)pParcelFabricSubLayer.FeatureClass;
        if (pDS.Workspace.Equals(Editor.EditWorkspace))
          CFParcelFabricSubLayers2.Add(pParcelFabricSubLayer);
        pLayer = pEnumLayer.Next();
      }
      CFSubLayers = CFParcelFabricSubLayers2;
      if (CFParcelFabricSubLayers2.Count > 0)
        return true;
      else
        return false;
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

  }
}
