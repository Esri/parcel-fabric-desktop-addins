//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DeleteSelectedParcels {
    using ESRI.ArcGIS.Desktop.AddIns;
    using ESRI.ArcGIS.CatalogUI;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.ArcCatalog;
    using ESRI.ArcGIS.ArcMapUI;
    using System;
    using System.Collections.Generic;
    
    
    /// <summary>
    /// A class for looking up declarative information in the associated configuration xml file (.esriaddinx).
    /// </summary>
    internal class ThisAddIn {
        
        internal static string Name {
            get {
                return "Delete Fabric Records";
            }
        }
        
        internal static string AddInID {
            get {
                return "{10da9251-6137-4b99-a213-93fa62247f65}";
            }
        }
        
        internal static string Company {
            get {
                return "Esri";
            }
        }
        
        internal static string Version {
            get {
                return "3.2";
            }
        }
        
        internal static string Description {
            get {
                return "Tools to delete parcels, control, connections, and line points. Repair tools to r" +
                    "eport or delete inconsistent fabric records.";
            }
        }
        
        internal static string Author {
            get {
                return "Tim Hodson";
            }
        }
        
        internal static string Date {
            get {
                return "1/15/2016";
            }
        }
        
        /// <summary>
        /// A class for looking up Add-in id strings declared in the associated configuration xml file (.esriaddinx).
        /// </summary>
        internal class IDs {
            
            /// <summary>
            /// Returns 'ESRI_DeleteSelectedParcels_clsDeleteSelectedParcels', the id declared for Add-in Button class 'clsDeleteSelectedParcels'
            /// </summary>
            internal static string clsDeleteSelectedParcels {
                get {
                    return "ESRI_DeleteSelectedParcels_clsDeleteSelectedParcels";
                }
            }
            
            /// <summary>
            /// Returns 'ESRI_DeleteSelectedParcels_clsDeleteEmptyPlans', the id declared for Add-in Button class 'clsDeleteEmptyPlans'
            /// </summary>
            internal static string clsDeleteEmptyPlans {
                get {
                    return "ESRI_DeleteSelectedParcels_clsDeleteEmptyPlans";
                }
            }
            
            /// <summary>
            /// Returns 'ESRI_DeleteSelectedParcels_TruncateFabricTables', the id declared for Add-in Button class 'TruncateFabricTables'
            /// </summary>
            internal static string TruncateFabricTables {
                get {
                    return "ESRI_DeleteSelectedParcels_TruncateFabricTables";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_clsDeleteSelectedControl', the id declared for Add-in Button class 'clsDeleteSelectedControl'
            /// </summary>
            internal static string clsDeleteSelectedControl {
                get {
                    return "Esri_DeleteSelectedParcels_clsDeleteSelectedControl";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_clsDeleteSelectedLinePts', the id declared for Add-in Button class 'clsDeleteSelectedLinePts'
            /// </summary>
            internal static string clsDeleteSelectedLinePts {
                get {
                    return "Esri_DeleteSelectedParcels_clsDeleteSelectedLinePts";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_clsDeleteSelectedConnxLines', the id declared for Add-in Button class 'clsDeleteSelectedConnxLines'
            /// </summary>
            internal static string clsDeleteSelectedConnxLines {
                get {
                    return "Esri_DeleteSelectedParcels_clsDeleteSelectedConnxLines";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_clsDeleteInconsistentRecords', the id declared for Add-in Button class 'clsDeleteInconsistentRecords'
            /// </summary>
            internal static string clsDeleteInconsistentRecords {
                get {
                    return "Esri_DeleteSelectedParcels_clsDeleteInconsistentRecords";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_clsDeleteOrphansTool', the id declared for Add-in Tool class 'clsDeleteOrphansTool'
            /// </summary>
            internal static string clsDeleteOrphansTool {
                get {
                    return "Esri_DeleteSelectedParcels_clsDeleteOrphansTool";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_clsPartialScanInconsistentRecords', the id declared for Add-in Button class 'clsPartialScanInconsistentRecords'
            /// </summary>
            internal static string clsPartialScanInconsistentRecords {
                get {
                    return "Esri_DeleteSelectedParcels_clsPartialScanInconsistentRecords";
                }
            }
            
            /// <summary>
            /// Returns 'ESRI_DeleteSelectedParcels_ToolbarHelp', the id declared for Add-in Button class 'ToolbarHelp'
            /// </summary>
            internal static string ToolbarHelp {
                get {
                    return "ESRI_DeleteSelectedParcels_ToolbarHelp";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_CustomizelHelperExtension', the id declared for Add-in Extension class 'CustomizelHelperExtension'
            /// </summary>
            internal static string CustomizelHelperExtension {
                get {
                    return "Esri_DeleteSelectedParcels_CustomizelHelperExtension";
                }
            }
            
            /// <summary>
            /// Returns 'ESRI_DeleteSelectedParcels_TruncateFabricTables2', the id declared for Add-in Button class 'TruncateFabricTables'
            /// </summary>
            internal static string TruncateFabricTables2 {
                get {
                    return "ESRI_DeleteSelectedParcels_TruncateFabricTables2";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_clsDeleteInconsistentRecords2', the id declared for Add-in Button class 'clsDeleteInconsistentRecords'
            /// </summary>
            internal static string clsDeleteInconsistentRecords2 {
                get {
                    return "Esri_DeleteSelectedParcels_clsDeleteInconsistentRecords2";
                }
            }
            
            /// <summary>
            /// Returns 'Esri_DeleteSelectedParcels_CustomizelHelperExtension2', the id declared for Add-in Extension class 'CustomizelHelperExtension'
            /// </summary>
            internal static string CustomizelHelperExtension2 {
                get {
                    return "Esri_DeleteSelectedParcels_CustomizelHelperExtension2";
                }
            }
        }
    }
    
internal static class ArcCatalog
{
  private static IApplication s_app;
  private static IGxDocumentEvents_Event s_docEvent;
  public static IApplication Application
  {
    get
    {
      if (s_app == null)
        s_app = Internal.AddInStartupObject.GetHook<IGxApplication>() as IApplication;

      return s_app;
    }
  }

  public static IDocument Document
  {
    get
    {
      if (Application != null)
        return Application.Document;

      return null;
    }
  }

  public static IGxApplication ThisApplication
  {
    get { return Application as IGxApplication; }
  }

  public static IDockableWindowManager DockableWindowManager
  {
    get { return Application as IDockableWindowManager; }
  }

  public static IGxDocumentEvents_Event Events
  {
    get 
    {
      s_docEvent = Document as IGxDocumentEvents_Event;
      return s_docEvent; 
    }
  }
}

internal static class ArcMap
{
  private static IApplication s_app = null;
  private static IDocumentEvents_Event s_docEvent;

  public static IApplication Application
  {
    get
    {
      if (s_app == null)
        s_app = Internal.AddInStartupObject.GetHook<IMxApplication>() as IApplication;

      return s_app;
    }
  }

  public static IMxDocument Document
  {
    get
    {
      if (Application != null)
        return Application.Document as IMxDocument;

      return null;
    }
  }
  public static IMxApplication ThisApplication
  {
    get { return Application as IMxApplication; }
  }
  public static IDockableWindowManager DockableWindowManager
  {
    get { return Application as IDockableWindowManager; }
  }
  public static IDocumentEvents_Event Events
  {
    get
    {
      s_docEvent = Document as IDocumentEvents_Event;
      return s_docEvent;
    }
  }
}

namespace Internal
{
  [StartupObjectAttribute()]
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
  public sealed partial class AddInStartupObject : AddInEntryPoint
  {
    private static AddInStartupObject _sAddInHostManager;
    private List<object> m_addinHooks = null;

    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public AddInStartupObject()
    {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool Initialize(object hook)
    {
      bool createSingleton = _sAddInHostManager == null;
      if (createSingleton)
      {
        _sAddInHostManager = this;
        m_addinHooks = new List<object>();
        m_addinHooks.Add(hook);
      }
      else if (!_sAddInHostManager.m_addinHooks.Contains(hook))
        _sAddInHostManager.m_addinHooks.Add(hook);

      return createSingleton;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void Shutdown()
    {
      _sAddInHostManager = null;
      m_addinHooks = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static T GetHook<T>() where T : class
    {
      if (_sAddInHostManager != null)
      {
        foreach (object o in _sAddInHostManager.m_addinHooks)
        {
          if (o is T)
            return o as T;
        }
      }

      return null;
    }

    // Expose this instance of Add-in class externally
    public static AddInStartupObject GetThis()
    {
      return _sAddInHostManager;
    }
  }
}
}
