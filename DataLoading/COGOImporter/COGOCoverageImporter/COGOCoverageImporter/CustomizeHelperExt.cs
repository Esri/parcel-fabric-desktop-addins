/*
 Copyright 1995-2017 Esri

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
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.ArcCatalogUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Desktop.AddIns;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

namespace COGOCoverageImporter
{
  public class CustomizeHelperExt : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    IApplicationStatusEvents_Event m_appStatusEvents;
    IApplication m_pApp = null;

    //ICadastralFabric m_pCadFabric = null;
    //clsFabricUtils m_FabricUTILS;

    private static CustomizeHelperExt s_extension;

    bool m_bIsCatalog = false;
    bool m_bIsMap = false;
    public CustomizeHelperExt()
    { }
    protected override void OnStartup()
    {
      s_extension = this;
      m_bIsCatalog = false;
      m_bIsMap = false;

      m_pApp = (IApplication)ArcMap.Application;

      if (m_pApp == null)
        //if the app is null then could be running from ArcCatalog
        m_pApp = (IApplication)ArcCatalog.Application;
      else
        m_bIsMap = true;

      if (m_pApp == null)
        return;
      else if (!m_bIsMap)
        m_bIsCatalog = true;

      if (m_bIsMap)
      {
        ArcMap.Events.NewDocument += ArcMap_NewOpenDocument;
        ArcMap.Events.OpenDocument += ArcMap_NewOpenDocument;
      }

      m_appStatusEvents = m_pApp as IApplicationStatusEvents_Event;
      m_appStatusEvents.Initialized += new IApplicationStatusEvents_InitializedEventHandler(appStatusEvents_Initialized);

    }

    private void WireDocumentEvents()
    {
      //
      // TODO: Sample document event wiring code. Change as needed
      //

      // Named event handler
      ArcMap.Events.NewDocument += delegate () { ArcMap_NewDocument(); };

      // Anonymous event handler
      ArcMap.Events.BeforeCloseDocument += delegate ()
      {
        // Return true to stop document from closing
        ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
        return msgBox.DoModal("BeforeCloseDocument Event", "Abort closing?", "Yes", "No", ArcMap.Application.hWnd);
      };

    }

    void ArcMap_NewOpenDocument()
    {
      appStatusEvents_Initialized();
    }

    void ArcMap_NewDocument()
    {
      // TODO: Handle new document event
    }

    void appStatusEvents_Initialized()
    {
      //The UID for the menu to add the custom button to
      string sMenuGuid = "{E6087790-BEBC-4de8-8221-BAEB12A60A58}";//  "Cadastral Fabric Context Menu."

      //FindGUIDOfCommandItem(m_pApp, "{E6087790-BEBC-4de8-8221-BAEB12A60A58}");
      //List<string> tbs = new List<string>();
      //List<string> tbs = ListArcMapVisibleToolbars(m_pApp);

      List<string> tbs = ListArcCatalogToolbars(m_pApp);

      string sCommand1 = "";

      if (m_bIsCatalog)
      {
        //Get the custom button from the Addin. This initializes the button
        //if it hasn't already been initialized.
        var AC_Cmd1 = AddIn.FromID<LoadCOGOData>(ThisAddIn.IDs.LoadCOGOData2);
        sCommand1 = ThisAddIn.IDs.LoadCOGOData2.ToString(); // the ICommand to add
      }

      if (m_bIsMap)
      {
        //Get the custom button from the Addin. This initializes the button
        //if it hasn't already been initialized.
        var AM_Cmd1 = AddIn.FromID<LoadCOGOData>(ThisAddIn.IDs.LoadCOGOData);
        sCommand1 = ThisAddIn.IDs.LoadCOGOData.ToString(); // the ICommand to add
      }

      AddCommandToApplicationMenu(m_pApp, sCommand1, sMenuGuid, false,
        "", "{AEA2FE42-ADC8-4F2A-88C1-185CF9BA4EA6}", false); //after check fabric command.

      // If the extension hasn't been started yet, bail
      if (s_extension == null)
        return;
      if (m_bIsMap)
      {
        //// Reset event handlers
        ESRI.ArcGIS.Carto.IActiveViewEvents_Event avEvent =
          ArcMap.Document.FocusMap as ESRI.ArcGIS.Carto.IActiveViewEvents_Event;
        if (avEvent == null)
          return;
      }
    }

    private void AddCommandToApplicationMenu(IApplication TheApplication, string AddThisCommandGUID,
  string ToThisMenuGUID, bool StartGroup, string PositionAfterThisCommandGUID, string PositionBeforeThisCommandGUID, bool EndGroup)
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

      if (iPos == pCmdBar.Count && PositionBeforeThisCommandGUID.Trim() != "") //prioritize the "after" case
      {
        UID pPositionCommandUID = new UIDClass();
        pPositionCommandUID.Value = PositionBeforeThisCommandGUID;
        iPos = pCmdBar.Find(pPositionCommandUID).Index;
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


    private void FindGUIDOfCommandItem(IApplication TheApplication, string ParentMenuGUID)
    {
      if (ParentMenuGUID.Trim() == "")
        return;

      //Assign the UID for the menu we want to add the custom button to
      UID MenuUID = new UIDClass();
      MenuUID.Value = ParentMenuGUID;
      //Get the target menu as a ICommandBar
      ICommandBar pCmdBar = TheApplication.Document.CommandBars.Find(MenuUID) as ICommandBar;
      
      IToolBarDef pToolBarDef = pCmdBar as IToolBarDef;
      
      int iCnt = pCmdBar.Count;
      for (int i = 0; i < iCnt; i++)
      {
        //ScrapClass itemdef = new ScrapClass();
        //try{pToolBarDef.GetItemInfo(i, itemdef);} catch { continue; }
        ////print the UID of commandItem
        //Debug.Print(itemdef.ID.ToString());
        //UID uid = new UIDClass();
        //uid.Value = itemdef.ID.ToString();
        ICommandItem commandItem = pCmdBar.Find("GxCadastralFabricContextMenu");
        Debug.Print(": " + commandItem.Name);
      }

    }

    public List<string> ListArcMapVisibleToolbars(IApplication application)
    {
      List<string> toolbarList = new List<string>();

      //Only work with ArcMap application types, not ArcCatalog, ArcGlobe, ArcToolBox, etc.
      if (!(application is ESRI.ArcGIS.ArcMapUI.IMxApplication))
      {
        return null;
      }

      // Set up GUID object for 'ESRI Mx Command Bars' component category
      UID uid_MxCommandBars = new UIDClass();
      //uid_MxCommandBars.Value = (System.Object)("{B56A7C4A-83D4-11d2-A2E9-080009B6F22B}"); // Explict Cast
      //HKEY_CLASSES_ROOT\Component Categories\{ 56C205F9 - E53A - 11D1 - 9496 - 080009EEBECB} //esri gx command bars
      uid_MxCommandBars.Value = (System.Object)("{56C205F9-E53A-11D1-9496-080009EEBECB}");
      // Set up the category factory.
      ICategoryFactory categoryFactory = new CategoryFactoryClass();
      categoryFactory.CategoryID = uid_MxCommandBars;

      IDocument document = application.Document;

      // Go through each member of the category, and if it is a toolbar try to find it in the document
      object object_ComponentCategory = categoryFactory.CreateNext();
      ICommandBars commandBars = document.CommandBars;

      while (object_ComponentCategory != null)
      {
        if (object_ComponentCategory is IToolBarDef)
        {
          IToolBarDef toolbarDef = (IToolBarDef)object_ComponentCategory; //Explicit Cast

          toolbarList.Add(toolbarDef.Name);
          Debug.Print("Toolbar - " + toolbarDef.Name);

          if (toolbarDef.Name == "ArcGIS4LocalGovernment_AttributeAssistantToolbar")
            MessageBox.Show("here");

          for (int i = 0; i < toolbarDef.ItemCount; i++)
          {
            ScrapClass itemdef = new ScrapClass();
            toolbarDef.GetItemInfo(i, itemdef);
            //print the UID of commandItem

            if (itemdef.ID != null && itemdef.ID != "")
            {
              Debug.Print(itemdef.ID.ToString());
              UID uid = new UIDClass();
              uid.Value = itemdef.ID.ToString();
              ICommandItem commandItem = commandBars.Find(uid, false, false);
              if (commandItem != null)
              {
                Debug.Print(i + "," + commandItem.Name + "," + commandItem.Category);
              }
            }
          }
        }

        object_ComponentCategory = categoryFactory.CreateNext();
      }
      return toolbarList;



    }

    public List<string> ListArcCatalogToolbars(IApplication application)
    {
      List<string> toolbarList = new List<string>();

      //Only work with ArcMap application types, not ArcCatalog, ArcGlobe, ArcToolBox, etc.
      if (!(application is ESRI.ArcGIS.ArcCatalog.Application))
      {
        return null;
      }

      // Set up GUID object for 'ESRI Mx Command Bars' component category
      UID uid_GxCommandBars = new UIDClass();
      uid_GxCommandBars.Value = (System.Object)("{56C205F9-E53A-11D1-9496-080009EEBECB}");//esri gx command bars
      // Set up the category factory.
      ICategoryFactory categoryFactory = new CategoryFactoryClass();
      categoryFactory.CategoryID = uid_GxCommandBars;

      IDocument document = application.Document;

      // Go through each member of the category, and if it is a toolbar try to find it in the document
      object object_ComponentCategory = categoryFactory.CreateNext();
      ICommandBars commandBars = document.CommandBars;

      while (object_ComponentCategory != null)
      {
        if (object_ComponentCategory is IToolBarDef)
        {
          IToolBarDef toolbarDef = (IToolBarDef)object_ComponentCategory; //Explicit Cast

          toolbarList.Add(toolbarDef.Name);
          Debug.Print("Toolbar - " + toolbarDef.Name);

          if (toolbarDef.Name == "ArcGIS4LocalGovernment_AttributeAssistantToolbar")
            MessageBox.Show("here");

          for (int i = 0; i < toolbarDef.ItemCount; i++)
          {
            ScrapClass itemdef = new ScrapClass();
            toolbarDef.GetItemInfo(i, itemdef);
            //print the UID of commandItem

            if (itemdef.ID != null && itemdef.ID != "")
            {
              Debug.Print(itemdef.ID.ToString());
              UID uid = new UIDClass();
              uid.Value = itemdef.ID.ToString();
              ICommandItem commandItem = commandBars.Find(uid, false, false);
              if (commandItem != null)
              {
                Debug.Print(i + "," + commandItem.Name + "," + commandItem.Category);
              }
            }
          }
        }

        object_ComponentCategory = categoryFactory.CreateNext();
      }
      return toolbarList;



    }


  }

}
public class ScrapClass : IItemDef
{
  #region IItemDef Members

  public bool Group { get; set; }
  public string ID { get; set; }
  public int SubType { get; set; }

  #endregion
}
