using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Editor;

//added
using System.Windows.Forms;
using ESRI.ArcGIS.Cadastral;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;

namespace SampleParcelFabricEvents
{
  /// <summary>
  /// FabricEventsExtension class implementing custom ESRI Editor Extension functionalities.
  /// </summary>
  public class FabricEventsExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    public FabricEventsExtension()
    {
    }
    IParcelEditManager m_pParcEditorMan;
    IParcelEditEvents_Event m_pParcelEditEvents;
    public static List<ESRI.ArcGIS.Geodatabase.IObjectClassEvents_Event> _objectClassEventList;
    public static ICadastralFabric m_TargetFabric;

    public static IArray _fabricLayers;
    public static IArray _fabricObjectClasses;
    public static List<int> _fabricObjectClassIds;
    public static Dictionary<int, ITable> _fabricInMemTablesLookUp;


    protected override void OnStartup()
    {
      IEditor theEditor = ArcMap.Editor;
      //get the cadastral extension
      ICadastralExtensionManager2 pCadExtMan = (ICadastralExtensionManager2)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      m_pParcEditorMan = (IParcelEditManager)pCadExtMan;
      m_pParcelEditEvents = pCadExtMan as IParcelEditEvents_Event;
      WireParcelFabricEvents();

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

    void WireParcelFabricEvents()
    {
      ParcelEditorEvents.OnGridCellEdit += delegate(int row, int col, object inValue)
      { return OnGridCellEdit(ref row, ref col, ref inValue); };

      #region other events for reference
      //ParcelEditorEvents.OnStartParcelEditing += delegate { OnStartParcelEditing(); };
      //ParcelEditorEvents.OnBeforeStopParcelEditing += delegate{OnBeforeStopParcelEditing();};
      //ParcelEditorEvents.OnAfterStopParcelEditing += delegate { OnAfterStopParcelEditing(); };
      #endregion
    }

    object OnGridCellEdit(ref int row, ref int col, ref object inValue)
    {
      object OutValue = inValue;
      IGSLine pGSLine = null;
      IParcelConstruction pCourses = (IParcelConstruction)m_pParcEditorMan.ParcelConstruction;

      if (col == 3)//this is the distance field
      {//this code uses a value of 0 on the distance field to over-ride 
        string sDistance = Convert.ToString(inValue);
        if (sDistance.Trim() == "0")
        {
          bool IsCompleteLine = (pCourses.GetLine(row, ref pGSLine));   
          //true means it's a complete line, false means it's a partial line
          if (!IsCompleteLine)
          {
            pGSLine.Radius = pGSLine.Distance; //default the radius to be the same as the default distance
            return String.Empty;
          }
          //note that the type of the inValue must be honoured when returning the value from this function.
        }
      }

      return OutValue;
    }
    
    void OnStartParcelEditing()
    {
      //MessageBox.Show("OnStartParcelEditing");
    }

    void OnAfterStopParcelEditing()
    {
      //MessageBox.Show("OnAfterStopParcelEditing");
    }

    void OnBeforeStopParcelEditing()
    {

    }

    public void WireFabricTableEditEvents()
    {

      if (_objectClassEventList == null)
        _objectClassEventList = new List<ESRI.ArcGIS.Geodatabase.IObjectClassEvents_Event>();

      //create event handler for each fabric class in the edit workspace
      try
      {
        _objectClassEventList.Clear();
        for (int i = 0; i < _fabricObjectClassIds.Count; i++)
        {
          IObjectClass pObjClass = (IObjectClass)_fabricObjectClasses.get_Element(i);
          //Create event handler.
          ESRI.ArcGIS.Geodatabase.IObjectClassEvents_Event ev = (ESRI.ArcGIS.Geodatabase.IObjectClassEvents_Event)pObjClass;
          ev.OnChange += new ESRI.ArcGIS.Geodatabase.IObjectClassEvents_OnChangeEventHandler(FabricRowChange);
          ev.OnChange += new ESRI.ArcGIS.Geodatabase.IObjectClassEvents_OnChangeEventHandler(FabricGeometryRowChange);
          //ev.OnCreate += new ESRI.ArcGIS.Geodatabase.IObjectClassEvents_OnCreateEventHandler(FabricRowCreate);
          _objectClassEventList.Add(ev);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message + " in Wire Fabric Events");
      }
    }

    public void UnWireFabricTableEditEvents()
    {
      if (_objectClassEventList == null)
        return;

      try
      {
        for (int i = _objectClassEventList.Count - 1; i >= 0; i--)
        {
          _objectClassEventList[i].OnChange -= FabricRowChange;
          _objectClassEventList[i].OnChange -= FabricGeometryRowChange;
          _objectClassEventList[i].OnCreate -= FabricRowCreate;
        }
        _objectClassEventList.Clear();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message + " in UnWire Fabric Events");
      }
    }

    public void InitFabricState()
    {
      //get the fabric layers from the map
      IArray pFabricSubLayerArray;
      bool bHasFabricLayers = GetAllFabricSubLayers(out pFabricSubLayerArray);
      //bool bHasFabricLayers = GetFabricSubLayersByClass(esriCadastralFabricTable.esriCFTParcels, out pFabricSubLayerArray);
      //return the parcels fabric class as a feature class

      if (bHasFabricLayers)
      {
        _fabricLayers = pFabricSubLayerArray;
        for (int i = 0; i < pFabricSubLayerArray.Count; i++)
        {
          IFeatureLayer pFLyr = (IFeatureLayer)pFabricSubLayerArray.get_Element(i);
          IFeatureClass featureClass = (IFeatureClass)pFLyr.FeatureClass;
          int iObjClassID = featureClass.ObjectClassID;
          if (_fabricInMemTablesLookUp == null)
            _fabricInMemTablesLookUp = new Dictionary<int, ITable>();
          if (_fabricObjectClasses == null)
            _fabricObjectClasses = new ArrayClass();

          if (!_fabricInMemTablesLookUp.ContainsKey(iObjClassID))
          {
            _fabricObjectClasses.Add(featureClass);
            IWorkspace pWS = CreateInMemoryWorkspace();
            try
            {
              IFields NewFields = createFieldsFromSourceFields(featureClass.Fields);
              IDataset pDS = featureClass as IDataset;
              ITable InMemTable = createTableInMemory(pDS.Name, NewFields, pWS);
              if (InMemTable != null)
                _fabricInMemTablesLookUp.Add(iObjClassID, InMemTable);
            }
            catch (Exception ex)
            {
              MessageBox.Show("Error Creating In Memory Fabric Table" + Environment.NewLine + ex.Message);
            }
          }
          if (_fabricObjectClassIds == null)
            _fabricObjectClassIds = new List<int>();
          if (!_fabricObjectClassIds.Contains(iObjClassID))
            _fabricObjectClassIds.Add(iObjClassID);
        }

        WireFabricTableEditEvents();

      }
    }

    public void UnInitFabricState()
    {
      if (_fabricInMemTablesLookUp == null || _objectClassEventList == null)
        return;
      try
      {
        UnWireFabricTableEditEvents();
        _objectClassEventList = null;
        _fabricObjectClasses = null;
        _fabricObjectClassIds = null;
        _fabricInMemTablesLookUp = null;
        _fabricLayers = null;
      }
      catch
      { }
    }

    public bool GetAllFabricSubLayers(out IArray CFSubLayers)
    {
      ICadastralFabricSubLayer pCFSubLyr = null;
      IArray CFParcelFabricSubLayers2 = new ArrayClass();
      IFeatureLayer pParcelFabricSubLayer = null;
      UID pId = new UIDClass();
      pId.Value = "{BA381F2B-F621-4F45-8F78-101F65B5BBE6}"; //ICadastralFabricSubLayer
      
      IEnumLayer pEnumLayer = ArcMap.Editor.Map.get_Layers(pId, true);
      pEnumLayer.Reset();
      ILayer pLayer = pEnumLayer.Next();
      while (pLayer != null)
      {
        pCFSubLyr = (ICadastralFabricSubLayer)pLayer;
        pParcelFabricSubLayer = (IFeatureLayer)pCFSubLyr;
        IDataset pDS = (IDataset)pParcelFabricSubLayer.FeatureClass;
        if (pDS.Workspace.Equals(ArcMap.Editor.EditWorkspace))
          CFParcelFabricSubLayers2.Add(pParcelFabricSubLayer);
        pLayer = pEnumLayer.Next();
      }
      CFSubLayers = CFParcelFabricSubLayers2;
      if (CFParcelFabricSubLayers2.Count > 0)
        return true;
      else
        return false;
    }

    private bool GetFabricSubLayers(IMap Map, esriCadastralFabricTable FabricSubClass, bool ExcludeNonTargetFabrics,
  ICadastralFabric TargetFabric, out IArray CFParcelFabSubLayers)
    {
      ICadastralFabricSubLayer pCFSubLyr = null;
      IArray CFParcelFabricSubLayers2 = new ArrayClass();
      IFeatureLayer pParcelFabricSubLayer = null;
      UID pId = new UIDClass();
      pId.Value = "{E156D7E5-22AF-11D3-9F99-00C04F6BC78E}";
      IEnumLayer pEnumLayer = Map.get_Layers(pId, true);
      pEnumLayer.Reset();
      ILayer pLayer = pEnumLayer.Next();
      while (pLayer != null)
      {
        if (pLayer is ICadastralFabricSubLayer)
        {
          pCFSubLyr = (ICadastralFabricSubLayer)pLayer;
          if (pCFSubLyr.CadastralTableType == FabricSubClass)
          {
            pParcelFabricSubLayer = (IFeatureLayer)pCFSubLyr;
            ICadastralFabric ThisLayersFabric = pCFSubLyr.CadastralFabric;
            bool bIsTargetFabricLayer = ThisLayersFabric.Equals(TargetFabric);
            if (!ExcludeNonTargetFabrics || (ExcludeNonTargetFabrics && bIsTargetFabricLayer))
              CFParcelFabricSubLayers2.Add(pParcelFabricSubLayer);
          }
        }
        pLayer = pEnumLayer.Next();
      }
      CFParcelFabSubLayers = CFParcelFabricSubLayers2;
      if (CFParcelFabricSubLayers2.Count > 0)
        return true;
      else
        return false;
    }

    public bool GetFabricSubLayersByClass(esriCadastralFabricTable FabricSubClass, out IArray CFParcelFabSubLayers)
    {
      ICadastralFabricSubLayer pCFSubLyr = null;
      IArray CFParcelFabricSubLayers2 = new ArrayClass();
      IFeatureLayer pParcelFabricSubLayer = null;
      UID pId = new UIDClass();
      //pId.Value = "{E156D7E5-22AF-11D3-9F99-00C04F6BC78E}";//IGeoFeatureLayer
      pId.Value = "{BA381F2B-F621-4F45-8F78-101F65B5BBE6}"; //ICadastralFabricSubLayer

      IEnumLayer pEnumLayer = ArcMap.Editor.Map.get_Layers(pId, true);
      pEnumLayer.Reset();
      ILayer pLayer = pEnumLayer.Next();
      while (pLayer != null)
      {
        pCFSubLyr = (ICadastralFabricSubLayer)pLayer;
        if (pCFSubLyr.CadastralTableType == FabricSubClass)
        {
          pParcelFabricSubLayer = (IFeatureLayer)pCFSubLyr;
          IDataset pDS = (IDataset)pParcelFabricSubLayer.FeatureClass;
          if (pDS.Workspace.Equals(ArcMap.Editor.EditWorkspace))
            CFParcelFabricSubLayers2.Add(pParcelFabricSubLayer);
        }
        pLayer = pEnumLayer.Next();
      }
      CFParcelFabSubLayers = CFParcelFabricSubLayers2;
      if (CFParcelFabricSubLayers2.Count > 0)
        return true;
      else
        return false;
    }

    public static IFields createFieldsFromSourceFields(IFields SourceFields)
    {
      // create fields
      IFields pFields;
      IFieldsEdit pFieldsEdit;
      IField pField;
      IFieldEdit pFieldEdit;

      pFields = new Fields();
      pFieldsEdit = (IFieldsEdit)pFields;
      int iFldCnt = SourceFields.FieldCount;
      pFieldsEdit.FieldCount_2 = iFldCnt;

      for (int i = 0; i < iFldCnt; i++)
      {
        IField SourceField = SourceFields.get_Field(i);

        if (SourceField.Editable || SourceField.Type == esriFieldType.esriFieldTypeOID || SourceField.Type == esriFieldType.esriFieldTypeGeometry)
        {
          IClone clone = SourceField as IClone;
          pField = clone.Clone() as IField;
        }
        else
        {
          pField = new Field();
          pFieldEdit = (IFieldEdit)pField;
          pFieldEdit.Editable_2 = true;
          pFieldEdit.Name_2 = SourceField.Name;
          pFieldEdit.IsNullable_2 = SourceField.IsNullable;
          pFieldEdit.Length_2 = SourceField.Length;
          pFieldEdit.Precision_2 = SourceField.Precision;
          pFieldEdit.Type_2 = SourceField.Type;
        }
        pFieldsEdit.set_Field(i, pField);
      }
      return pFields;
    }

    public static IWorkspace CreateInMemoryWorkspace()
    {
      IWorkspaceFactory workspaceFactory = null;
      IWorkspaceName workspaceName = null;
      IName name = null;
      IWorkspace workspace = null;
      try
      {
        // Create an InMemory workspace factory.
        workspaceFactory = new InMemoryWorkspaceFactory();

        // Create an InMemory geodatabase.
        workspaceName = workspaceFactory.Create("", "MyWorkspace",
         null, 0);

        // Cast for IName.
        name = (IName)workspaceName;

        //Open a reference to the InMemory workspace through the name object.
        workspace = (IWorkspace)name.Open();
        return workspace;
      }
      catch
      {
        return null;

      }
      finally
      {
        workspaceFactory = null;
        workspaceName = null;
        name = null;
      }
    }

    public static ITable createTableInMemory(string strName, IFields TableFields, IWorkspace pWS)
    {
      IFeatureWorkspace pFWS = (IFeatureWorkspace)pWS;
      return pFWS.CreateTable(strName, TableFields, null, null, "");
    }

    private static bool InMemTableExistsForRow(ESRI.ArcGIS.Geodatabase.IObject obj)
    {
      if (_fabricInMemTablesLookUp == null)
        return false;

      int iObjClassID = obj.Class.ObjectClassID;

      if (_fabricInMemTablesLookUp[iObjClassID] != null)
        return true;
      else
        return false;
    }

    public static void FabricGeometryRowChange(ESRI.ArcGIS.Geodatabase.IObject obj)
    {
      if (obj is IFeature)
      {
        IFeatureChanges pFeatChanges = obj as IFeatureChanges;
        if (!pFeatChanges.ShapeChanged)
          return;
        if (pFeatChanges.OriginalShape.IsEmpty) //means new fabric parcel
          return;
      }

      if (InMemTableExistsForRow(obj))
      {
        // code for fabric geometry change here
        MessageBox.Show("Fabric geometry change");
      }
    }

    public static void FabricRowChange(ESRI.ArcGIS.Geodatabase.IObject obj)
    {
      if (InMemTableExistsForRow(obj))
      {
        // code for fabric row change here
        string sOID = obj.OID.ToString();
        string sTable = obj.Class.AliasName;
        MessageBox.Show("Fabric row change in table " + sTable + Environment.NewLine +
          "OID: " +  sOID);
      }

    }

    public static void FabricRowCreate(ESRI.ArcGIS.Geodatabase.IObject obj)
    {
      //if (InMemTableExistsForRow(obj))
      //{
      //  // code for fabric row create here
      //  string sOID = obj.OID.ToString();
      //  string sTable = obj.Class.AliasName;
      //  MessageBox.Show("Fabric row created in table " + sTable + Environment.NewLine +
      //    "OID: " + sOID);
      //}
    }

    #endregion


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
      Events.OnStartEditing += delegate { OnStartEditing(); };
      Events.OnStopEditing += delegate { OnStopEditing(); };

      Events2.BeforeStopEditing += delegate(bool save) { OnBeforeStopEditing(save); };
    }

    void OnBeforeStopEditing(bool save)
    {
    }

    void OnStartEditing()
    {
      ICadastralEditor pCadEd = m_pParcEditorMan as ICadastralEditor;
      m_TargetFabric = pCadEd.CadastralFabric;

      InitFabricState();
    }

    void OnStopEditing()
    {
      UnInitFabricState();
    }
    #endregion




  }

}
