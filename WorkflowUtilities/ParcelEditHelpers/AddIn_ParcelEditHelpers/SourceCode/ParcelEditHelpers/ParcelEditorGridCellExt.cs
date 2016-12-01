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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using ESRI.ArcGIS.Editor;
//using ESRI.ArcGIS.Cadastral;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;

namespace ParcelEditHelper
{
  /// <summary>
  /// ParcelEditorGridCellExt class implementing custom ESRI Editor Extension functionalities.
  /// </summary>
  public class ParcelEditorGridCellExt : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    public ParcelEditorGridCellExt()
    {
    }
    
    IParcelEditManager m_pParcEditorMan;
    IParcelEditEvents_Event m_pParcelEditEvents;
    ParcelEditHelperExtension m_ParcelEditHelperExt;

    protected override void OnStartup()
    {
      IEditor theEditor = ArcMap.Editor;
      UID pUID = new UIDClass();
      ICadastralExtensionManager2 pCadExtMan;
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";

      pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      m_pParcEditorMan = (IParcelEditManager)pCadExtMan;

      m_pParcelEditEvents = pCadExtMan as IParcelEditEvents_Event;
      m_ParcelEditHelperExt = ParcelEditHelperExtension.GetParcelEditHelperExtension();
      WireEditorEvents();
    }

    protected override void OnShutdown()
    {
    }

    #region Parcel Editor Events

    #region Shortcut properties to the parcel editor event interfaces

    private IParcelEditEvents_Event ParcelEditorEvents
    {
      get { return m_pParcelEditEvents; }
    }
    #endregion

    void WireEditorEvents()
    {
      #region other events for reference
      //    Dim pJoinEvents2 As IParcelJoinEvents2_Event = pCadExtMan.JoinParcelManager
      //    Dim pParcelEditEvents As IParcelEditEvents_Event = pCadExtMan
      //    Dim pCadastralEditEvents As ICadastralEditorEvents_Event = pCadExtMan

      //''    Dim pParcelEditManager As IParcelEditManager = pCadExtMan
      //    AddHandler Events.OnSelectionChanged, AddressOf OnSelectionChangedEvent

      //    AddHandler pJoinEvents2.OnJoin, AddressOf OnJoinEvent
      //    AddHandler pJoinEvents2.OnJoinLinkAdded, AddressOf OnJoinLinkAddEvent
      //    AddHandler pJoinEvents2.OnStartJoining, AddressOf OnStartJoiningEvent

      //    AddHandler pParcelEditEvents.OnBeforeStopParcelEditing, AddressOf OnBeforeStopParcelEditing
      //    AddHandler pParcelEditEvents.OnStartParcelEditing, AddressOf OnStartParcelEditing
      //    AddHandler pParcelEditEvents.OnAfterStopParcelEditing, AddressOf OnAfterStopParcelEditing
      //    AddHandler pParcelEditEvents.OnAfterSwitchParcelEditMode, AddressOf OnAfterSwitchParcelEditMode
      //    AddHandler pParcelEditEvents.OnBeforeSwitchParcelEditMode, AddressOf OnBeforeSwitchParcelEditMode
      //    AddHandler pParcelEditEvents.OnGridCellEdit, AddressOf OnGridCellEdit

      //    AddHandler pCadastralEditEvents.OnBeforeStopEditingFabric, AddressOf OnBeforeStopEditingFabric
      //    AddHandler pCadastralEditEvents.OnAfterStopEditingFabric, AddressOf OnAfterStopEditingFabric
      //    AddHandler pCadastralEditEvents.OnFabricLayerChanged, AddressOf OnFabricLayerChanged
      //    AddHandler pCadastralEditEvents.OnJobChanged, AddressOf OnBeforeStopEditingFabric
      //    AddHandler pCadastralEditEvents.OnStartEditingFabric, AddressOf OnStartEditingFabric
      #endregion

      ParcelEditorEvents.OnGridCellEdit += delegate(int row, int col, object inValue)
      { return OnGridCellEdit(ref row, ref col, ref inValue); };

      ParcelEditorEvents.OnStartParcelEditing += delegate { OnStartParcelEditing(); };
      ParcelEditorEvents.OnAfterStopParcelEditing += delegate { OnAfterStopParcelEditing(); };

    }

    object OnGridCellEdit(ref int row, ref int col, ref object inValue)
    {
      string m_sFieldName = m_ParcelEditHelperExt.FieldName;
      bool m_bRecordFieldName = m_ParcelEditHelperExt.RecordToField;
      object OutValue = inValue;
      if (col == 2 && m_bRecordFieldName)//this is the bearing field
      {
        IParcelConstruction pTrav = (IParcelConstruction)m_pParcEditorMan.ParcelConstruction;
        IGSLine pGSLine = null;
        bool IsCompleteLine = (pTrav.GetLine(row, ref pGSLine));
        //true means it's a complete line
        //false means it's a partial line
        //note that the type of the inValue must be honoured when returning the value from this function, in this case we are rebuilding the string for the same value, unaltered.
        //however thev other relevant work done is to set the record field to the same value to capture what was entered prior to resequencing and potential bearing changes.
        //this happens either for a complete line or a partial one
        //The logic could be changed to be more conservative and only capture the record for a partial line (IsCompleteLine==false), since that would only record the first time it's cogo'd, 
        //compared to this code that records edits of completed lines *as well as* partial lines
        IAngularConverter pAngConv = new AngularConverterClass();
        string sBear = Convert.ToString(inValue);
        sBear = sBear.Replace("°", "-");
        sBear = sBear.Replace("'", "-");
        sBear = sBear.Replace("\"", "");

        if (!pAngConv.SetString(sBear, esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds)) //TODO: base this on Plan properties
          return OutValue;
        double brgRecord = pAngConv.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);

        sBear = pAngConv.GetString(esriDirectionType.esriDTQuadrantBearing, esriDirectionUnits.esriDUDegreesMinutesSeconds, 0);

        IGSAttributes pLineAtts = (IGSAttributes)pGSLine;

        IParcelConstruction4 pTrav4 = pTrav as IParcelConstruction4;
        pTrav4.UpdateGridFromGSLines(true, false);
        sBear = sBear.Replace(" ", "");
        sBear = sBear.Insert(sBear.Length - 1, "\"");
        int i = sBear.LastIndexOf('-');
        sBear = sBear.Insert(i, "'");
        i = sBear.IndexOf('-');
        sBear = sBear.Insert(i, "°");
        sBear = sBear.Replace("-", "");
        pLineAtts.SetProperty(m_sFieldName, sBear);
        return sBear;
        //note that the type of the inValue must be honoured when returning the value from this function.
      }
      return OutValue;
    }

    void OnStartParcelEditing()
    {  
      m_ParcelEditHelperExt.IsParcelOpen = true;
    }

    void OnAfterStopParcelEditing()
    {
      m_ParcelEditHelperExt.IsParcelOpen = false;
    }

    void OnBeforeStopParcelEditing()
    {
      if (m_ParcelEditHelperExt.RecordToField)
        return;

      IParcelConstruction4 pTrav = (IParcelConstruction4)m_pParcEditorMan.ParcelConstruction;
      IEnumGSLines pGSLines = pTrav.GetLines(false, false);

      pGSLines.Reset();
      IGSLine pGSLine = null;
      IGSParcel pGSParcel = null;
      pGSLines.Next(ref pGSParcel, ref pGSLine);
      while (pGSLine != null)
      {
        IGSAttributes pLineAtts = (IGSAttributes)pGSLine;
        double brgRecord = Convert.ToDouble(pGSLine.Bearing);
        brgRecord = brgRecord * 180 / Math.PI;
        pLineAtts.SetProperty(m_ParcelEditHelperExt.FieldName, brgRecord);
        //pGSParcel.Modified();
        pGSLines.Next(ref pGSParcel, ref pGSLine);
      }

      pTrav.UpdateGridFromGSLines(true, false);
    }

    #endregion

  }

}
