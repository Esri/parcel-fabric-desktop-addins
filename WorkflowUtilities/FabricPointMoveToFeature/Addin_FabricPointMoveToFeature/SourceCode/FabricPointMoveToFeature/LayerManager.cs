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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using Microsoft.Win32;

namespace FabricPointMoveToFeature
{
  public class LayerManager : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    private IMap m_map;
    private bool m_hasSelectableLayer;
    private double m_dReportTolerance;
    private double m_dMinimumMoveTolerance;
    private bool m_MergePoints;
    private double m_dMergePointTolerance;
    private static LayerManager s_extension;
    private bool m_useLines;
    private bool m_TestMinimumMove;
    private bool m_TransformationPrompt;
    private string m_PointFieldName;
    private bool m_SelectionIgnore;
    private bool m_SelectedReferenceFeatures;
    private bool m_SelectedParcels;
    private bool m_SelectPrompt;
    private bool m_ShowReport;
    private IEditor m_pEd = null;

    public LayerManager()
    {
    }
    
    public IEditor TheEditor
    {
      get
      {
        return m_pEd;
      }
    }

    public bool UseLines
    {
      get
      {
        return m_useLines;
      }
      set
      {
        m_useLines = value;
      }
    }

    public bool TestForMinimumMove
    {
      get
      {
        return m_TestMinimumMove;
      }
      set
      {
        m_TestMinimumMove = value;
      }
    }

    public double MinimumMoveTolerance
    {
      get
      {
        return m_dMinimumMoveTolerance;
      }
      set
      {
        m_dMinimumMoveTolerance = value;
      }
    }

    public bool PromptForDatumTransformation
    {
      get
      {
        return m_TransformationPrompt;
      }
      set
      {
        m_TransformationPrompt = value;
      }
    }

    public bool MergePoints
    {
      get
      {
        return m_MergePoints;
      }
      set
      {
        m_MergePoints = value;
      }
    }

    public double MergePointTolerance
    {
      get
      {
        return m_dMergePointTolerance;
      }
      set
      {
        m_dMergePointTolerance = value;
      }
    }

    public bool ShowReport
    {
      get
      {
        return m_ShowReport;
      }
      set
      {
        m_ShowReport = value;
      }
    }

    public double ReportTolerance
    {
      get
      {
        return m_dReportTolerance;
      }
      set
      {
        m_dReportTolerance = value;
      }
    }

    public string PointFieldName
    {
      get
      {
        return m_PointFieldName;
      }
      set
      {
        m_PointFieldName = value;
      }
    }

    public bool SelectionsIgnore
    {
      get
      {
        return m_SelectionIgnore;
      }
      set
      {
        m_SelectionIgnore = value;
      }
    }

    public bool SelectionsUseReferenceFeatures
    {
      get
      {
        return m_SelectedReferenceFeatures;
      }
      set
      {
        m_SelectedReferenceFeatures = value;
      }
    }

    public bool SelectionsUseParcels
    {
      get
      {
        return m_SelectedParcels;
      }
      set
      {
        m_SelectedParcels = value;
      }
    }

    public bool SelectionsPromptForChoicesWhenNoSelection
    {
      get
      {
        return m_SelectPrompt;
      }
      set
      {
        m_SelectPrompt = value;
      }
    }

    protected override void OnStartup()
    {
      try
      {
        s_extension = this;
        m_pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
        // Named event handler
        ArcMap.Events.NewDocument += delegate() { ArcMap_NewDocument(); };
        ArcMap.Events.OpenDocument += delegate() { ArcMap_NewDocument(); };
        
        Utilities Utils = new Utilities();
        string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
        if (sDesktopVers.Trim() == "")
          sDesktopVers = "Desktop10.0";
        else
          sDesktopVers = "Desktop" + sDesktopVers;
        string sValues =
        Utils.ReadFromRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral",
          "AddIn.FabricPointMoveToFeature");
        if (sValues.Trim() == "")
        {
          m_useLines = false;
          m_PointFieldName = "";
          m_TestMinimumMove = false;
          m_SelectionIgnore = true;
          m_SelectedReferenceFeatures=false;
          m_SelectedParcels=false;
          m_SelectPrompt=false;
          m_TransformationPrompt = false;
          m_dReportTolerance = 0;
          return;
        }
        try
        {
          string[] Values = sValues.Split(',');
          m_useLines = (Values[0].Trim() == "1");
          m_PointFieldName = Values[1].Trim();
          m_TestMinimumMove = (Values[2].Trim() == "1");

          if(!Double.TryParse(Values[3].Trim(), out m_dMinimumMoveTolerance))
            m_dMinimumMoveTolerance = 0;

          m_ShowReport = (Values[4].Trim() == "1");
          if (m_ShowReport)
          {
            m_dReportTolerance = 0;
            try
            { 
              m_dReportTolerance =Convert.ToDouble(Values[5]);
            }
            catch
            { }
          }

          m_SelectionIgnore = (Values[6].Trim() == "1");
          m_SelectedReferenceFeatures = (Values[7].Trim() == "1");
          m_SelectedParcels = (Values[8].Trim() == "1");
          m_SelectPrompt = (Values[9].Trim() == "1");
          m_MergePoints = (Values[10].Trim() == "1");

          if (m_MergePoints)
          {
            m_dMergePointTolerance= 0.003;
            try
            {
              m_dMergePointTolerance = Convert.ToDouble(Values[11]);
            }
            catch
            { }
          }
          m_TransformationPrompt= (Values[12].Trim() == "1");

        }
        catch
        { }
      }

      catch (COMException ex)
      {
        MessageBox.Show(ex.Message);
      }
      Initialize();
    }

    //event handlers
    private void avEvent_ContentsChanged()
    {
      m_hasSelectableLayer = CheckForSelectableLayer();
      LayerDropdown.FillComboBox(m_map);
    }

    private void AvEvent_ItemAdded(object Item)
    {
      m_map = ArcMap.Document.FocusMap;
      if (m_map != null)
        LayerDropdown.FillComboBox(m_map);
      m_hasSelectableLayer = CheckForSelectableLayer();
    }

    private void AVEvents_FocusMapChanged()
    {
      Initialize();
    }

    // Privates
    private void Initialize()
    {
      // If the extension hasn't been started yet, bail
      if (s_extension == null)
        return;
      //// Reset event handlers
      IActiveViewEvents_Event avEvent = ArcMap.Document.FocusMap as IActiveViewEvents_Event;
      if (avEvent == null)
        return;
      avEvent.ItemAdded += AvEvent_ItemAdded;
      avEvent.ItemDeleted += AvEvent_ItemAdded;
      avEvent.ContentsChanged += avEvent_ContentsChanged;

      // Update the UI
      m_map = ArcMap.Document.FocusMap;
      LayerDropdown.FillComboBox(m_map);
      m_hasSelectableLayer = CheckForSelectableLayer();
    }

    private void Uninitialize()
    {
      if (s_extension == null)
        return;

      if (m_map == null)
        return;

      // Detach event handlers
      IActiveViewEvents_Event avEvent = m_map as IActiveViewEvents_Event;
      avEvent.ItemAdded -= AvEvent_ItemAdded;
      avEvent.ItemDeleted -= AvEvent_ItemAdded;
      //avEvent.SelectionChanged -= UpdateSelCountDockWin;
      avEvent.ContentsChanged -= avEvent_ContentsChanged;
      avEvent = null;

      // Update UI
      LayerDropdown selCombo = LayerDropdown.GetTheComboBox();
      if (selCombo != null)
        selCombo.ClearAll();
    }

    private bool CheckForSelectableLayer()
    {
      IMap map = ArcMap.Document.FocusMap;
      if (map == null)
        return false;
      // Bail if map has no layers
      if (map.LayerCount == 0)
        return false;

      // Get all the feature layers in the focus map
      // and see if at least one is selectable
      UID uid = new UID();
      uid.Value = "{40A9E885-5533-11d0-98BE-00805F7CED21}";
      IEnumLayer enumLayers = map.get_Layers(uid, true);
      IFeatureLayer featureLayer = enumLayers.Next() as IFeatureLayer;
      while (featureLayer != null)
      {
        if (featureLayer.Selectable == true)
          return true;

        featureLayer = enumLayers.Next() as IFeatureLayer;
      }
      return false;
    }
    internal static LayerManager GetExtension()
    {
      if (s_extension == null)
      {
        // Call FindExtension to load extension.
        UID id = new UID();
        id.Value = ThisAddIn.IDs.LayerManager;
        s_extension = (LayerManager)ArcMap.Application.FindExtensionByCLSID(id);
      }

      return s_extension;
    }

    void ArcMap_NewDocument()
    {
      IActiveViewEvents_Event pageLayoutEvent = ArcMap.Document.PageLayout as IActiveViewEvents_Event;
      pageLayoutEvent.FocusMapChanged += new IActiveViewEvents_FocusMapChangedEventHandler(AVEvents_FocusMapChanged);
      Initialize();
    }

  }
}
