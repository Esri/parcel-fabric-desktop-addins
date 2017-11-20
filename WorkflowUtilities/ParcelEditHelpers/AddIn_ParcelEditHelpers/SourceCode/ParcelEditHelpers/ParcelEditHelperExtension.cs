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
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ParcelEditHelper
{
  /// <summary>
  /// ParcelEditHelperExtension class implementing custom ESRI Editor Extension functionalities.
  /// </summary>
  public class ParcelEditHelperExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    private IEditEvents_Event m_editEvents;
    private IEditEvents2_Event m_editEvents2;
    private static IDockableWindow s_dockWindow;
    private static ParcelEditHelperExtension s_extension;
    private bool m_RecordToField;
    private bool m_ParcelIsOpen;
    private bool m_SpiralIsPending;
    private bool m_SpiralIsLengthByDistance; // as opposed to by Angle (=FALSE)
    private bool m_SpiralIsCounterClockwise;
    private double m_SpiralRadius1;
    private double m_SpiralRadius2;
    private double m_SpiralArcLength;
    private double m_SpiralDeltaAngle;


    private string m_FieldName;
    private IFeatureClass m_pLinesFC;

    public ParcelEditHelperExtension()
    {
    }

    public bool RecordToField
    {
      get
      {
        return m_RecordToField;
      }
      set
      {
        m_RecordToField = value;
      }
    }
    
    public bool IsParcelOpen
    {
      get
      {
        return m_ParcelIsOpen;
      }
      set
      {
        m_ParcelIsOpen = value;
      }
    }

    public string FieldName
    {
      get
      {
        return m_FieldName;
      }
      set
      {
        m_FieldName = value;
      }
    }

    public bool HasSpiralConstructionPending
    {
      get
      {
        return m_SpiralIsPending;
      }
      set
      {
        m_SpiralIsPending = value;
      }
    }

    public bool SpiralLengthParameterIsByDistance
    {
      get
      {
        return m_SpiralIsLengthByDistance;
      }
      set
      {
        m_SpiralIsLengthByDistance = value;
      }
    }

    public bool SpiralIsCounterClockwise
    {
      get
      {
        return m_SpiralIsCounterClockwise;
      }
      set
      {
        m_SpiralIsCounterClockwise = value;
      }
    }

    public double SpiralRadiusOne
    {
      get
      {
        return m_SpiralRadius1;
      }
      set
      {
        m_SpiralRadius1 = value;
      }
    }

    public double SpiralRadiusTwo
    {
      get
      {
        return m_SpiralRadius2;
      }
      set
      {
        m_SpiralRadius2 = value;
      }
    }

    public double SpiralArcLength
    {
      get
      {
        return m_SpiralArcLength;
      }
      set
      {
        m_SpiralArcLength = value;
      }
    }

    public double SpiralDeltaAngle
    {
      get
      {
        return m_SpiralDeltaAngle;
      }
      set
      {
        m_SpiralDeltaAngle = value;
      }
    }


    protected override void OnStartup()
    {
      //AdjustmentDockWindow pDock=

      Utilities UTIL = new Utilities();
      s_extension = this;
      m_SpiralIsPending = false; //initialize false
      try
      {
        string sDesktopVers = UTIL.GetDesktopVersionFromRegistry();
        if (sDesktopVers.Trim() == "")
          sDesktopVers = "Desktop10.0";
        else
          sDesktopVers = "Desktop" + sDesktopVers;
        
        string sValues = UTIL.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
          "Options", false);

        string[] Values = sValues.Split(',');
        string sVal = Values[0];
        string sFieldName = Values[1];
        bool bUseFieldRecord = false;

        if (sVal.Trim() != "")
          bUseFieldRecord = ((sVal.ToUpper().Trim()) == "1");

        s_extension.FieldName = sFieldName;
        s_extension.RecordToField = bUseFieldRecord;

      }
      catch
      {}

      m_editEvents = ArcMap.Editor as IEditEvents_Event;
      m_editEvents2 = ArcMap.Editor as IEditEvents2_Event;
      m_editEvents.OnStartEditing += new IEditEvents_OnStartEditingEventHandler(m_editEvents_OnStartEditing);
      m_editEvents.OnStopEditing += new IEditEvents_OnStopEditingEventHandler(m_editEvents_OnStopEditing);
      m_editEvents2.OnVertexAdded += new IEditEvents2_OnVertexAddedEventHandler(OnVertexAdded);
      //      m_editEvents2.OnVertexAdded += OnVertexAdded;

    }

    internal static IDockableWindow GetFabricAdjustmentWindow()
    {
      // Only get/create the dockable window if needed
      if (s_dockWindow == null)
      {
        // Use GetDockableWindow directly intead of FromID as we want the client IDockableWindow not the internal class
        UID dockWinID = new UIDClass();
        dockWinID.Value = ThisAddIn.IDs.AdjustmentDockWindow;
        s_dockWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
//      make enabled
        IEditor TheEditor = ArcMap.Editor;
        if(TheEditor!=null)
          AdjustmentDockWindow.SetEnabled(TheEditor.EditState==esriEditState.esriStateEditing);

      }

      return s_dockWindow;
    }

    void m_editEvents_OnStartEditing()
    {
      AdjustmentDockWindow.SetEnabled(true);
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
  
      string m_sFieldName = "";
      m_sFieldName = s_extension.FieldName;

      ICadastralFabric pFabric = pCadEd.CadastralFabric;
      IFeatureClass pLinesFC = (IFeatureClass)pFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
      if (pLinesFC.FindField(m_sFieldName) == -1)
      {
        m_RecordToField = false;
        return;
      }
    }

    void m_editEvents_OnStopEditing(bool bSave)
    {
      AdjustmentDockWindow.SetEnabled(false);
    }

    internal static ParcelEditHelperExtension GetParcelEditHelperExtension()
    {
      if (s_extension == null)
      {
        // Call FindExtension to load extension.
        UID id = new UID();
        id.Value = ThisAddIn.IDs.ParcelEditHelperExtension;
        s_extension = (ParcelEditHelperExtension)ArcMap.Application.FindExtensionByCLSID(id);
      }

      return s_extension;
    }

    protected override void OnShutdown()
    {
       s_dockWindow = null;
       m_editEvents.OnStartEditing -= m_editEvents_OnStartEditing;
       m_editEvents2.OnVertexAdded -= OnVertexAdded;
    }

    #region Editor Events

    #region Shortcut properties to the various editor event interfaces
    private IEditEvents_Event Events
    {
      get { return ArcMap.Editor as IEditEvents_Event; }
    }
    private IEditEvents2_Event Events2
    {
      get { return ArcMap.Editor as IEditEvents2_Event; }
    }
    private IEditEvents3_Event Events3
    {
      get { return ArcMap.Editor as IEditEvents3_Event; }
    }
    private IEditEvents4_Event Events4
    {
      get { return ArcMap.Editor as IEditEvents4_Event; }
    }
    #endregion

    void WireEditorEvents()
    {
      //
      //  TODO: Sample code demonstrating editor event wiring
      //
      Events.OnCurrentTaskChanged += delegate
      {
        if (ArcMap.Editor.CurrentTask != null)
          System.Diagnostics.Debug.WriteLine(ArcMap.Editor.CurrentTask.Name);
      };
      Events2.BeforeStopEditing += delegate(bool save) { OnBeforeStopEditing(save); };
    }

    private void OnVertexAdded(IPoint VertexPoint)
    {
      if (m_SpiralIsPending)
      {
//        IEditSketch pSketch = ArcMap.Editor as IEditSketch;
//        ISegmentCollection pSegColl = pSketch.Geometry as ISegmentCollection;
//        int iSegCnt = pSegColl.SegmentCount;

//        if (m_SpiralIsLengthByDistance)
//        {
//          // https://en.wikipedia.org/wiki/Degree_of_curvature#Formulas_for_radius_of_curvature

//          //the parameter name "curvature" is actually the inverse of the radius. This makes setting infinity easier as it is just 0.
//          //double dFromCurvature = (m_SpiralRadius1 * Math.PI)/(180 * m_SpiralArcLength);
//          //double dToCurvature = (m_SpiralRadius2 * Math.PI) / (180 * m_SpiralArcLength);

//          double dFromCurvature = 1/m_SpiralRadius1;
//          double dToCurvature = 1/m_SpiralRadius2;

//          IPolyline6 theSpiralPolyLine = ConstructSpiralbyLength(pSegColl.Segment[iSegCnt - 1].FromPoint, VertexPoint, dFromCurvature, dToCurvature, !m_SpiralIsCounterClockwise, m_SpiralArcLength, 10);
          
//          if (theSpiralPolyLine == null)
//            MessageBox.Show("A spiral could not be created with the entered parameters.");

//          ISegmentCollection pSpiralSegCollection = theSpiralPolyLine as ISegmentCollection;

//          pSegColl.ReplaceSegmentCollection(iSegCnt-1,1,pSpiralSegCollection);

////          pSegColl.AddSegmentCollection(pSpiralSegCollection);

//          IGeometry geom = pSegColl as IGeometry;
//          pSketch.Geometry = geom;
//          pSketch.RefreshSketch();

//        }



      }
      m_SpiralIsPending=false;
    }
    void OnBeforeStopEditing(bool save)
    {
    }
    #endregion

  }

}
