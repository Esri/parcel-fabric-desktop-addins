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

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Desktop.AddIns;
using System.Runtime.InteropServices;

namespace ParcelEditHelper
{
  public class CustomizeHelper : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    IApplicationStatusEvents_Event m_appStatusEvents;
    IApplication m_pApp = null;

    ICadastralExtensionManager2 m_pCadExtMan = null;
    IEditor m_pEd = null;

    private static CustomizeHelper s_extension;
    bool m_bIsMap = false;
    public CustomizeHelper()
    {
    }

    protected override void OnStartup()
    {
      s_extension = this;
      m_pApp = (IApplication)ArcMap.Application;

      if (m_pApp == null)
        return;

      ArcMap.Events.NewDocument += ArcMap_NewOpenDocument;
      ArcMap.Events.OpenDocument += ArcMap_NewOpenDocument;

      m_appStatusEvents = m_pApp as IApplicationStatusEvents_Event;
      m_appStatusEvents.Initialized += new IApplicationStatusEvents_InitializedEventHandler(appStatusEvents_Initialized);

      m_pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

      //get the extension
      UID pUID = new UIDClass();
      pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
      m_pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
    }

    public IEditor TheEditor
    {
      get
      {
        return m_pEd;
      }
    }

    public ICadastralExtensionManager2 TheCadastralExtensionManager
    {
      get
      {
        return m_pCadExtMan;
      }
    }

    public bool CommandIsEnabled
    {
      get;
      set;
    }

    void appStatusEvents_Initialized()
    {
      //The UID for the menu to add the custom button to
      string sMenuGuid = "{E45FE607-8E26-44D3-A851-86496FF031C9}";//  "Parcel - Parcel Lines Context Menu."
      string sCommand1 = "";

      //Get the custom button from the Addin. This initializes the button
      //if it hasn't already been initialized.
      var AM_Cmd1 = AddIn.FromID<ConstructionTraverse>(ThisAddIn.IDs.ConstructionTraverse);
      sCommand1 = "Esri_ParcelEditHelper_ConstructionTraverse";
      AddCommandToApplicationMenu(m_pApp, sCommand1, sMenuGuid, false, "", false); //after "" command.

      var AM_Cmd2 = AddIn.FromID<SaveLinesGridToFile>(ThisAddIn.IDs.SaveLinesGridToFile);
      sCommand1 = "Esri_ParcelEditHelper_SaveLinesGridToFile";
      AddCommandToApplicationMenu(m_pApp, sCommand1, sMenuGuid, true, "", false); //after "" command.

      var AM_Cmd3 = AddIn.FromID<LoadFileToLinesGrid>(ThisAddIn.IDs.LoadFileToLinesGrid);
      sCommand1 = "Esri_ParcelEditHelper_LoadFileToLinesGrid";
      AddCommandToApplicationMenu(m_pApp, sCommand1, sMenuGuid, false, "", false); //after "" command.
      
      var AM_Cmd4 = AddIn.FromID<BreaklineAddNewLines>(ThisAddIn.IDs.BreaklineAddNewLines);
      sMenuGuid = "{4598F676-8CEB-4fe1-8E4F-5ADB93379793}";//  "Parcel - Construction Lines Context Menu."
      sCommand1 = "Esri_ParcelEditHelper_BreaklineAddNewLines";
      AddCommandToApplicationMenu(m_pApp, sCommand1, sMenuGuid, false, "{9987F18B-8CC4-4548-8C41-7DB51F289BB3}", false); //after "" command.

      // If the extension hasn't been started yet, bail
      if (s_extension == null)
        return;

      //// Reset event handlers
      ESRI.ArcGIS.Carto.IActiveViewEvents_Event avEvent =
        ArcMap.Document.FocusMap as ESRI.ArcGIS.Carto.IActiveViewEvents_Event;
      if (avEvent == null)
        return;
      avEvent.ItemAdded += AvEvent_ItemAdded;
      avEvent.ItemDeleted += AvEvent_ItemAdded;
      avEvent.ContentsChanged += AvEvent_ContentsChanged;

    }

    //event handlers
    private void AvEvent_ContentsChanged()
    {
      //
    }

    private void AvEvent_ItemAdded(object Item)
    {
      //
    }

    private void AddCommandToApplicationMenu(IApplication TheApplication, string AddThisCommandGUID,
      string ToThisMenuGUID, bool StartGroup, string PositionAfterThisCommandGUID, bool EndGroup)
    {
      if (AddThisCommandGUID.Trim() == "")
        return;

      if (ToThisMenuGUID.Trim() == "")
        return;

      //Then add items to the command bar:
      UID pCommandUID = new UIDClass(); // this references an ICommand from my extension
      pCommandUID.Value = AddThisCommandGUID; // the ICommand to add

      //Assign the UID for the menu we want to add the custom button to
      UID MenuUID = new UIDClass();
      MenuUID.Value = ToThisMenuGUID;
      //Get the target menu as a ICommandBar
      ICommandBar pCmdBar = TheApplication.Document.CommandBars.Find(MenuUID) as ICommandBar;

      int iPos = pCmdBar.Count;

      //for the position string, if it's an empty string then default to end of menu.
      if (PositionAfterThisCommandGUID.Trim() != "")
      {
        UID pPositionCommandUID = new UIDClass();
        pPositionCommandUID.Value = PositionAfterThisCommandGUID;
        iPos = pCmdBar.Find(pPositionCommandUID).Index + 1;
      }

      ICommandItem pCmdItem = pCmdBar.Find(pCommandUID);

      //Check if it is already present on the context menu...
      if (pCmdItem == null)
      {
        pCmdItem = pCmdBar.Add(pCommandUID, iPos);
        pCmdItem.Group = StartGroup;
        pCmdItem.Refresh();
      }

    }

    protected override void OnShutdown()
    {
      if (m_bIsMap)
      {
        ArcMap.Events.NewDocument -= ArcMap_NewOpenDocument;
        ArcMap.Events.OpenDocument -= ArcMap_NewOpenDocument;
      }

      s_extension = null;
      base.OnShutdown();
    }

    void ArcMap_NewOpenDocument()
    {
      appStatusEvents_Initialized();
    }

    internal static CustomizeHelper GetExtension()
    {
      if (s_extension == null)
      {
        // Call FindExtension to load extension.
        UID id = new UIDClass();
        id.Value = ThisAddIn.IDs.CustomizeHelper;
        s_extension = (CustomizeHelper)ArcMap.Application.FindExtensionByCLSID(id);
      }

      return s_extension;
    }
  }

}
