using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Desktop.AddIns;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;


namespace CadastralXML
{
  public class CustomizeHelper : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    IApplicationStatusEvents_Event m_appStatusEvents;
    IApplication m_pApp = null;

    ICadastralExtensionManager2 m_pCadExtMan = null;
    private static IEditor m_pEd = null;

    private static CustomizeHelper s_extension;
    bool m_bIsMap = false;

    public CustomizeHelper()
    {
    }


    public ICadastralExtensionManager2 TheCadastralExtensionManager
    {
      get
      {
        return m_pCadExtMan;
      }
    }

    public static bool CommandIsEnabled
    {
      get
      {
        return (m_pEd.EditState==esriEditState.esriStateEditing);
      }
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

    void appStatusEvents_Initialized()
    {
      //The UID for the menu to add the custom button to
      string sMenuGuid = "{CFFCF318-533D-4806-95F0-7DFF28D87084}";//  "Parcel - Parcel Editor toolbar esriCadastralUI.CadastralEditorMenu"
      string sCommand1 = "";

      //Get the custom button from the Addin. This initializes the button
      //if it hasn't already been initialized.
      var AM_Cmd1 = AddIn.FromID<AppendCadastralXMLFiles>(ThisAddIn.IDs.AppendCadastralXMLFiles);
      sCommand1 = "Esri_CadastralXML_AppendCadastralXMLFiles";
      AddCommandToApplicationMenu(m_pApp, sCommand1, sMenuGuid, false, "{9FB04311-8CBC-4AFB-9F51-1C53658FB991}", false); //after "" command.

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


  }

}
