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

namespace ParcelEditHelper
{
  /// <summary>
  /// ParcelEditHelperExtension class implementing custom ESRI Editor Extension functionalities.
  /// </summary>
  public class ParcelEditHelperExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    private IEditEvents_Event m_editEvents;
    private static IDockableWindow s_dockWindow;

    public ParcelEditHelperExtension()
    {
    }

    protected override void OnStartup()
    {
      //AdjustmentDockWindow pDock=

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
        if(TheEditor!=null)
          AdjustmentDockWindow.SetEnabled(TheEditor.EditState==esriEditState.esriStateEditing);

      }

      return s_dockWindow;
    }

    void m_editEvents_OnStartEditing()
    {
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
