using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;

namespace ParcelEditHelper
{
  /// <summary>
  /// AdjustmentExt class implementing custom ESRI Editor Extension functionalities.
  /// </summary>
  public class AdjustmentExt : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    private IEditEvents_Event m_editEvents;
    private static IDockableWindow s_dockWindow;
    public AdjustmentExt()
    {
    }

    protected override void OnStartup()
    {
      IEditor theEditor = ArcMap.Editor;

      m_editEvents = ArcMap.Editor as IEditEvents_Event;
      m_editEvents.OnStartEditing += new IEditEvents_OnStartEditingEventHandler(m_editEvents_OnStartEditing);
      m_editEvents.OnStopEditing += new IEditEvents_OnStopEditingEventHandler(m_editEvents_OnStopEditing);
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
        if (TheEditor != null)
          AdjustmentDockWindow.SetEnabled(TheEditor.EditState == esriEditState.esriStateEditing);

      }

      return s_dockWindow;
    }

    void m_editEvents_OnStartEditing()
    {
      //set the units
      AdjustmentDockWindow.SetEnabled(true);

    }

    void m_editEvents_OnStopEditing(bool bSave)
    {
      AdjustmentDockWindow.SetEnabled(false);
    }

    protected override void OnShutdown()
    {
      s_dockWindow = null;
      m_editEvents.OnStartEditing -= m_editEvents_OnStartEditing;
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

    void OnBeforeStopEditing(bool save)
    {
    }
    #endregion

  }

}
