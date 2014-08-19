/*
 Copyright 1995-2014 Esri

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

namespace DeleteSelectedParcels
{
  public class CustomizelHelperExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    IApplicationStatusEvents_Event m_appStatusEvents;
    IApplication m_pApp = null;

    ICadastralExtensionManager2 m_pCadExtMan = null;
    IEditor m_pEd = null;
    ICadastralFabric m_pCadFabric=null;
    clsFabricUtils m_FabricUTILS;

    private static CustomizelHelperExtension s_extension;

    bool m_bIsCatalog = false;
    bool m_bIsMap = false;
    bool m_IsUnversionedFabric = false;

    public CustomizelHelperExtension()
    {
    }

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

      if (m_bIsMap)
      {
        m_pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");

        //get the extension
        UID pUID = new UIDClass();
        pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
        m_pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByCLSID(pUID);
      }
      m_FabricUTILS = new clsFabricUtils();
    }

    public bool MapHasUnversionedFabric
    {
      get
      {
        return m_IsUnversionedFabric;
      }
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

    public bool IsUnVersionedFabric(ICadastralFabric TheFabric)
    {
      bool IsFileBasedGDB = false;
      bool IsUnVersioned = false;

      ITable pTable = TheFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
      IDataset pDS=(IDataset)TheFabric;
      IWorkspace pWS = pDS.Workspace;

      IsFileBasedGDB = (!(pWS.WorkspaceFactory.WorkspaceType == esriWorkspaceType.esriRemoteDatabaseWorkspace));

      if (!(IsFileBasedGDB))
      {
        IVersionedObject pVersObj = (IVersionedObject)pTable;
        IsUnVersioned = (!(pVersObj.IsRegisteredAsVersioned));
        pTable = null;
        pVersObj = null;
      }
      if (IsUnVersioned && !IsFileBasedGDB)
        return true;
      else
        return false;
    }

    void appStatusEvents_Initialized()
    {
      //The UID for the menu to add the custom button to
      string sMenuGuid = "{E6087790-BEBC-4de8-8221-BAEB12A60A58}";//  "Cadastral Fabric Context Menu."
      string sCommand1 = "";
      string sCommand2 = "";

      if (m_bIsCatalog)
      {
        //Get the custom button from the Addin. This initializes the button
        //if it hasn't already been initialized.
        var AC_Cmd1 = AddIn.FromID<clsDeleteInconsistentRecords>(ThisAddIn.IDs.clsDeleteInconsistentRecords2);
        // sCommand1 = ThisAddIn.IDs.clsDeleteInconsistentRecords2.ToString(); // the ICommand to add
        sCommand1 = "Esri_DeleteSelectedParcels_Inconsistent_Records";

        var AC_Cmd2 = AddIn.FromID<TruncateFabricTables>(ThisAddIn.IDs.TruncateFabricTables2);
        sCommand2 = ThisAddIn.IDs.TruncateFabricTables2.ToString(); // the ICommand to add
      }

      if (m_bIsMap)
      {
        //Get the custom button from the Addin. This initializes the button
        //if it hasn't already been initialized.
        var AM_Cmd1 = AddIn.FromID<clsDeleteInconsistentRecords>(ThisAddIn.IDs.clsDeleteInconsistentRecords);

       // sCommand1 = ThisAddIn.IDs.clsDeleteInconsistentRecords.ToString(); // the ICommand to add
        sCommand1="Esri_DeleteSelectedParcels_Inconsistent_Records";

        var AM_Cmd2 = AddIn.FromID<TruncateFabricTables>(ThisAddIn.IDs.TruncateFabricTables);
        sCommand2 = ThisAddIn.IDs.TruncateFabricTables.ToString(); // the ICommand to add
      }

      AddCommandToApplicationMenu(m_pApp, sCommand1, sMenuGuid, false, "{3BFD71DE-024E-43EA-8A37-562324D839ED}", false); //after check fabric command.
      AddCommandToApplicationMenu(m_pApp, sCommand2, sMenuGuid, false, sCommand1, true);

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
        avEvent.ItemAdded += AvEvent_ItemAdded;
        avEvent.ItemDeleted += AvEvent_ItemAdded;
        avEvent.ContentsChanged += AvEvent_ContentsChanged;
      }
    }

    private void SetIsUnversionedFlag()
    {
      IActiveView pActiveView = ArcMap.Document.ActiveView;
      IMap pMap = pActiveView.FocusMap;
      if (m_FabricUTILS.GetFabricFromMap(pMap, out m_pCadFabric))
      {
        if (IsUnVersionedFabric(m_pCadFabric))
          m_IsUnversionedFabric = true;
        else
          m_IsUnversionedFabric = false;
      }
    }

    //event handlers
    private void AvEvent_ContentsChanged()
    {
      SetIsUnversionedFlag();
    }

    private void AvEvent_ItemAdded(object Item)
    {
      SetIsUnversionedFlag();
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

      //if (EndGroup)
      //{
      //  if (pCmdBar.Count > iPos)
      //  { pCmdItem=pCmdBar.Find("")}
      //}
    }
    
    protected override void OnShutdown()
    {
      if (m_bIsMap)
      {
        ArcMap.Events.NewDocument -= ArcMap_NewOpenDocument;
        ArcMap.Events.OpenDocument -= ArcMap_NewOpenDocument;
      }
      m_FabricUTILS = null;
      s_extension = null;
      base.OnShutdown();
    }

    void ArcMap_NewOpenDocument()
    {
      appStatusEvents_Initialized();
    }

    internal static CustomizelHelperExtension GetExtension()
    {
      if (s_extension == null)
      {
        // Call FindExtension to load extension.
        UID id = new UIDClass();
        id.Value = ThisAddIn.IDs.CustomizelHelperExtension;
        s_extension = (CustomizelHelperExtension)ArcMap.Application.FindExtensionByCLSID(id);
      }

      return s_extension;
    }

  }

}
