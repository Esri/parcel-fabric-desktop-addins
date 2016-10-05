/*
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

using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using ESRI.ArcGIS.ArcMapUI;
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


namespace ParcelEditHelper
{
  public class LineToCircularCurve : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public LineToCircularCurve()
    {
    }
    private IStepProgressor m_pStepProgressor;
    //Create a CancelTracker.
    private ITrackCancel m_pTrackCancel;
    private IProgressDialogFactory m_pProgressorDialogFact;
    private IFIDSet m_pFIDSetParcels;
    private IQueryFilter m_pQF;
    private string m_sReport;
    private string sUnderline = Environment.NewLine + "---------------------------------------------------------------------" + Environment.NewLine;
    private string m_sLineCount;
    private string m_sParcelCount;
    private bool m_bShowReport = false;
    private bool m_bNoUpdates = false;
    private bool m_bShowProgressor = false;
    protected override void OnClick()
    {
      m_bNoUpdates = false;
      m_sReport = "Direction Inverse Report:";
      IEditor m_pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

      if (m_pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing first, and try again.", "Start Editing");
        return;
      }

      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);

      //check if there is a Manual Mode "modify" job active ===========
      ICadastralPacketManager pCadPacMan = (ICadastralPacketManager)pCadExtMan;
      if (pCadPacMan.PacketOpen)
      {
        MessageBox.Show("The Delete Parcels command cannot be used when there is an open job.\r\nPlease finish or discard the open job, and try again.",
          "Delete Selected Parcels");
        return;
      }

      try
      {

        IEditProperties2 pEditorProps2 = (IEditProperties2)m_pEd;

        IArray LineLyrArr;
        IMap pMap = m_pEd.Map;
        ICadastralFabric pCadFabric = null;
        //ISpatialReference pSpatRef = m_pEd.Map.SpatialReference;
        //IProjectedCoordinateSystem2 pPCS = null;
        IActiveView pActiveView = ArcMap.Document.ActiveView;

        //double dMetersPerUnit = 1;

        //if (pSpatRef == null)
        //  ;
        //else if (pSpatRef is IProjectedCoordinateSystem2)
        //{
        //  pPCS = (IProjectedCoordinateSystem2)pSpatRef;
        //  string sUnit = pPCS.CoordinateUnit.Name;
        //  if (sUnit.Contains("Foot") && sUnit.Contains("US"))
        //    sUnit = "U.S. Feet";

        //  dMetersPerUnit = pPCS.CoordinateUnit.MetersPerUnit;
        //}

        IAngularConverter pAngConv = new AngularConverterClass();
        Utilities Utils = new Utilities();

        if (!Utils.GetFabricSubLayers(pMap, esriCadastralFabricTable.esriCFTLines, out LineLyrArr))
          return;

        //if we're in an edit session then grab the target fabric
        if (m_pEd.EditState == esriEditState.esriStateEditing)
          pCadFabric = pCadEd.CadastralFabric;

        if (pCadFabric == null)
        {//find the first fabric in the map
          if (!Utils.GetFabricFromMap(pMap, out pCadFabric))
          {
            MessageBox.Show
              ("No Parcel Fabric found in the map.\r\nPlease add a single fabric to the map, and try again.");
            return;
          }
        }
        List<int> lstLineIds = new List<int>();

        IFeatureClass pFabricLinesFC = (IFeatureClass)pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
        int idxParcelIDFld = pFabricLinesFC.Fields.FindField("ParcelID");
        int idxCENTERPTID = pFabricLinesFC.Fields.FindField("CenterPointID");
        int idxRADIUS = pFabricLinesFC.Fields.FindField("Radius");
        bool bFieldsPresent = true;
        if (idxParcelIDFld == -1)
          bFieldsPresent = false;
        if (idxCENTERPTID == -1)
          bFieldsPresent = false;
        if (idxRADIUS == -1)
          bFieldsPresent = false;

        if (!bFieldsPresent)
        {
          MessageBox.Show("Fields missing.");
          return;
        }

        Dictionary<int, List<string>> dictLineToCurveNeighbourData = new Dictionary<int, List<string>>();
        m_pFIDSetParcels = new FIDSet();
        for (int i = 0; i < LineLyrArr.Count; i++)
        {
          IFeatureSelection pFeatSel = LineLyrArr.Element[i] as IFeatureSelection;
          ISelectionSet pSelSet = pFeatSel.SelectionSet;
          ICursor pCursor = null;
          pSelSet.Search(null, false, out pCursor);
          IFeature pLineFeat = pCursor.NextRow() as IFeature;

          while (pLineFeat != null)
          {
            if (!lstLineIds.Contains(pLineFeat.OID))
            {
              IGeometry pGeom = pLineFeat.ShapeCopy;
              ISegmentCollection pSegColl = pGeom as ISegmentCollection;
              ISegment pSeg = null;
              if (pSegColl.SegmentCount == 1)
                pSeg = pSegColl.get_Segment(0);
              else
              {
                //todo: but for now, only deals with single segment short segments
                Marshal.ReleaseComObject(pLineFeat);
                pLineFeat = pCursor.NextRow() as IFeature;
                continue;
              }

              //check geometry for circular arc
              if (pSeg is ICircularArc)
              {
                object dVal1 = pLineFeat.get_Value(idxRADIUS);
                object dVal2 = pLineFeat.get_Value(idxCENTERPTID);
                ICircularArc pCircArc = pSeg as ICircularArc;
                if (dVal1 != DBNull.Value && dVal2 != DBNull.Value)
                {
                  Marshal.ReleaseComObject(pLineFeat);
                  pLineFeat = pCursor.NextRow() as IFeature;
                  continue;
                }
              }
              
              //query near lines
              int iFoundTangent = 0;
              List<string> sCurveInfoFromNeighbours = new List<string>();

              if (Utils.HasTangentCurveMatchFeatures(pFabricLinesFC, (IPolycurve)pGeom,"", 1.5,0.033,1,(pSeg.Length*1.1),
                 out iFoundTangent, ref sCurveInfoFromNeighbours))
              {
                lstLineIds.Add(pLineFeat.OID);
                int j = (int)pLineFeat.get_Value(idxParcelIDFld);
                m_pFIDSetParcels.Add(j);
                dictLineToCurveNeighbourData.Add(pLineFeat.OID, sCurveInfoFromNeighbours);
              }
              if (iFoundTangent == 1) //if there's only one tangent look further afield
              {
                int iFoundLinesCount = 0;
                int iFoundParallel = 0;
                if (Utils.HasParallelCurveMatchFeatures(pFabricLinesFC, (IPolycurve)pGeom, "", 1.5, 70,
                    out iFoundLinesCount, out iFoundParallel, ref sCurveInfoFromNeighbours))
                {
                  if (!dictLineToCurveNeighbourData.ContainsKey(pLineFeat.OID))
                    dictLineToCurveNeighbourData.Add(pLineFeat.OID, sCurveInfoFromNeighbours);
                }
              }
            }
            Marshal.ReleaseComObject(pLineFeat);
            pLineFeat = pCursor.NextRow() as IFeature;
          }
          Marshal.ReleaseComObject(pCursor);
        }

        #region line to curve candidate analysis
        if (lstLineIds.Count == 0)
          return;

        RefineToBestRadiusAndCenterPoint(dictLineToCurveNeighbourData);

        #endregion

        if (dictLineToCurveNeighbourData.Count == 0)
          return;

        bool bIsFileBasedGDB = false; bool bIsUnVersioned = false; bool bUseNonVersionedDelete = false;
        IWorkspace pWS = m_pEd.EditWorkspace;
        IProgressDialog2 pProgressorDialog = null;
        IMouseCursor pMouseCursor = new MouseCursorClass();
        pMouseCursor.SetCursor(2);
        if (!Utils.SetupEditEnvironment(pWS, pCadFabric, m_pEd, out bIsFileBasedGDB,
    out bIsUnVersioned, out bUseNonVersionedDelete))
        {
          return;
        }


        #region Create Cadastral Job
        string sTime = "";
        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          //see if parcel locks can be obtained on the selected parcels. First create a job.
          DateTime localNow = DateTime.Now;
          sTime = Convert.ToString(localNow);
          ICadastralJob pJob = new CadastralJobClass();
          pJob.Name = sTime;
          pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
          pJob.Description = "Convert lines to curves";
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
            return;
          }
        }
        #endregion

        #region Test for Edit Locks
        ICadastralFabricLocks pFabLocks = (ICadastralFabricLocks)pCadFabric;

        //only need to get locks for parcels that have lines that are to be changed


        int[] pParcelIds = new int[m_pFIDSetParcels.Count()];
        ILongArray pParcelsToLock = new LongArrayClass();
        Utils.FIDsetToLongArray(m_pFIDSetParcels, ref pParcelsToLock, ref pParcelIds, m_pStepProgressor);

        if (!bIsUnVersioned && !bIsFileBasedGDB)
        {
          pFabLocks.LockingJob = sTime;
          ILongArray pLocksInConflict = null;
          ILongArray pSoftLcksInConflict = null;

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

            return;
          }
        }
        #endregion

        if (m_pEd.EditState == esriEditState.esriStateEditing)
        {
          try
          {
            m_pEd.StartOperation();
          }
          catch
          {
            m_pEd.AbortOperation();//abort any open edit operations and try again
            m_pEd.StartOperation();
          }
        }
        if (bUseNonVersionedDelete)
        {
          if (!Utils.StartEditing(pWS, bIsUnVersioned))
            return;
        }

        ICadastralFabricSchemaEdit2 pSchemaEd = (ICadastralFabricSchemaEdit2)pCadFabric;
        pSchemaEd.ReleaseReadOnlyFields((ITable)pFabricLinesFC, esriCadastralFabricTable.esriCFTLines); //release for edits

        m_pQF = new QueryFilter();

       // m_pEd.StartOperation();

        List<string> sInClauseList = Utils.InClauseFromOIDsList(lstLineIds, 995);
        foreach (string InClause in sInClauseList)
        {
          m_pQF.WhereClause = pFabricLinesFC.OIDFieldName + " IN (" + InClause + ")";
          if (!UpdateCircularArcValues((ITable)pFabricLinesFC, m_pQF, bIsUnVersioned, dictLineToCurveNeighbourData))
            ;
        }
        m_pEd.StopOperation("Insert missing circular arc information.");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        m_pEd.AbortOperation();
      }
      finally
      { 
      
      }
    }

    void RefineToBestRadiusAndCenterPoint(Dictionary<int, List<string>> dictLineToCurveNeighbourLookUP)
    {
      IDictionaryEnumerator TheEnum = dictLineToCurveNeighbourLookUP.GetEnumerator();
      TheEnum.Reset();
      while (TheEnum.MoveNext())
      {//each list item for each line feature
        List<string> lstCurveNeighbors = TheEnum.Value as List<string>;
        //first check how many tangent matches there are
        List<double> tangentRadii = new List<double>();
        List<string> tangentRadiiAndCenterPoint = new List<string>();
        List<double> parallelRadii = new List<double>();
        List<string> parallelRadiiAndCenterPoint = new List<string>();

        //adding some test data for testing
        //lstCurveNeighbors.Add("x,192.5,339,t");
        //lstCurveNeighbors.Add("x,192.497,339,t");
        //lstCurveNeighbors.Add("x,192.401,341,t");
        //lstCurveNeighbors.Add("x,192.402,341,t");


        foreach (string sCurveNeighbourInfo in lstCurveNeighbors)
        {//line feature
          if (sCurveNeighbourInfo.Trim() == "")
            continue;
          string[] Values = sCurveNeighbourInfo.Split(',');

          if (Values[3].Trim() == "t")
          {
            double dRadius = Convert.ToDouble(Values[1]);
            tangentRadii.Add(dRadius);
            tangentRadiiAndCenterPoint.Add(Values[1] +","+ Values[2]);
          }
          else
          {
            double dRadius = Convert.ToDouble(Values[1]);
            parallelRadii.Add(dRadius);
          }
        }

        if(tangentRadii.Count==1 && parallelRadii.Count>0)
        {//only one radius found, so search the parallel offsets for one confirmer
          bool bHasConfirmer = false;
          foreach (double dd in parallelRadii)
          { 
            if(Math.Abs(dd-tangentRadii[0])<0.5)
            {
              bHasConfirmer = true;
              break;
            }
          }
          if (bHasConfirmer)
          {
            string s = lstCurveNeighbors[0];
            lstCurveNeighbors.Clear();
            lstCurveNeighbors.Add(tangentRadiiAndCenterPoint[0]);
          }
          continue;
        }

        var groupsTangent = tangentRadii.GroupBy(item => Math.Round(item,2)).Where(group => group.Skip(1).Any());
        var groupsTangentAndCP = tangentRadiiAndCenterPoint.GroupBy(item => item).Where(group => group.Skip(1).Any());

        Debug.Print(groupsTangent.Count().ToString());
        Debug.Print(groupsTangentAndCP.Count().ToString());

        if (groupsTangent.Count() == 1 && groupsTangentAndCP.Count() == 1)
        { //if there is only 1 of each group, then there are no ambiguities for the tangent or the center point
          IGrouping<string, string> d1 = groupsTangentAndCP.ElementAt(0);
          lstCurveNeighbors.Clear();
          lstCurveNeighbors.Add(d1.Key);
          continue;
        }
        else if (groupsTangent.Count() == 1 && groupsTangentAndCP.Count() > 1)
        {//if there is only 1 tangent, but more than one center point then there are center points to merge
          lstCurveNeighbors.Clear();
          lstCurveNeighbors.Add("cp merge");
          lstCurveNeighbors.AddRange(groupsTangentAndCP as IEnumerable<string>);       
        }

        if (groupsTangent.Count() > 1)
        { //if there is more than 1 tangent, then ...code stub if needed
          foreach (var value in groupsTangentAndCP)
          {
            Debug.Print(value.Key.ToString());
          }
        }
      }

    }
    protected override void OnUpdate()
    {
    }

    public bool UpdateCircularArcValues(ITable LineTable, IQueryFilter QueryFilter, bool Unversioned, Dictionary<int, List<string>> CurveLookup)
    {
      try
      {
        ITableWrite pTableWr = (ITableWrite)LineTable;//used for unversioned table
        IRow pLineFeat = null;
        ICursor pLineCurs = null;

        if (Unversioned)
          pLineCurs = pTableWr.UpdateRows(QueryFilter, false);
        else
          pLineCurs = LineTable.Update(QueryFilter, false);

        pLineFeat = pLineCurs.NextRow();

        Int32 iCtrPointIDX = pLineCurs.Fields.FindField("CENTERPOINTID");
        Int32 iRadiusIDX = pLineCurs.Fields.FindField("RADIUS");

        while (pLineFeat != null)
        {//loop through all of the given lines, and update centerpoint ids and the Radius values
          List<string> CurveInfoList = CurveLookup[pLineFeat.OID];
          string[] sCurveInfo = CurveInfoList[0].Split(','); //should only be one element in the list at this point
          if (sCurveInfo.Length > 2)
          {
            Marshal.ReleaseComObject(pLineFeat); //garbage collection
            pLineFeat = pLineCurs.NextRow();
            continue;
          }
          double dRadius = Convert.ToDouble(sCurveInfo[0]);
          int iCtrPointID = Convert.ToInt32(sCurveInfo[1]);
          pLineFeat.set_Value(iRadiusIDX, dRadius);
          pLineFeat.set_Value(iCtrPointIDX, iCtrPointID);

          if (Unversioned)
            pLineCurs.UpdateRow(pLineFeat);
          else
            pLineFeat.Store();

          Marshal.ReleaseComObject(pLineFeat); //garbage collection
          pLineFeat = pLineCurs.NextRow();
        }
        Marshal.ReleaseComObject(pLineCurs); //garbage collection
        return true;
      }
      catch (COMException ex)
      {
        MessageBox.Show("Problem updating circular arc: " + Convert.ToString(ex.ErrorCode));
        return false;
      }
    }


  }
}
