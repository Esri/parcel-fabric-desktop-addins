//Add-in provided import library references
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Added Esri references
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

//Added non-Esri references
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SampleParcelTraceTool
{

  /// <summary>
  /// A construction tool for ArcMap Editor, using shape constructors
  /// </summary>
  public partial class SampleTraceTool1 : ESRI.ArcGIS.Desktop.AddIns.Tool, IShapeConstructorTool, ISketchTool
  {
    //Add-in wizard added these member variables
    private IEditor3 m_editor;
    private IEditEvents_Event m_editEvents;
    private IEditEvents5_Event m_editEvents5;
    private IEditSketch3 m_edSketch;
    private IShapeConstructor m_csc;

    //Added fabric member variables
    private ICadastralEditor m_pCadEd;
    private ICadastralFabric m_pCadFab;
    private IFeatureClass m_pFabricLines;

    public SampleTraceTool1()
    {
      // Get the editor
      m_editor = ArcMap.Editor as IEditor3;
      m_editEvents = m_editor as IEditEvents_Event;
      m_editEvents5 = m_editor as IEditEvents5_Event;
    }

    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }

    protected override void OnActivate()
    {
      //get the cadastral editor and target fabric
      m_pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      m_pCadFab = m_pCadEd.CadastralFabric;

      if (m_pCadFab == null)
      {
        MessageBox.Show("No target fabric found. Please add a fabric to the map start editing, and try again.");
        return;
      }

      m_pFabricLines = (IFeatureClass)m_pCadFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);

      m_editor.CurrentTask = null;
      m_edSketch = m_editor as IEditSketch3;
      m_edSketch.GeometryType = esriGeometryType.esriGeometryPolyline;
      m_csc = new TraceConstructorClass();
      m_csc.Initialize(m_editor);
      m_edSketch.ShapeConstructor = m_csc;
      m_csc.Activate();

      // Setup events
      m_editEvents.OnSketchModified += OnSketchModified;
      m_editEvents5.OnShapeConstructorChanged += OnShapeConstructorChanged;
      m_editEvents.OnSketchFinished += OnSketchFinished;
    }

    protected override bool OnDeactivate()
    {
      m_editEvents.OnSketchModified -= OnSketchModified;
      m_editEvents5.OnShapeConstructorChanged -= OnShapeConstructorChanged;
      m_editEvents.OnSketchFinished -= OnSketchFinished;
      return true;
    }

    protected override void OnDoubleClick()
    {
      if (m_edSketch.Geometry == null)
        return;

      if (Control.ModifierKeys == Keys.Shift)
      {
        // Finish part
        ISketchOperation pso = new SketchOperation();
        pso.MenuString_2 = "Finish Sketch Part";
        pso.Start(m_editor);
        m_edSketch.FinishSketchPart();
        pso.Finish(null);
      }
      else
        m_edSketch.FinishSketch();
    }

    private void OnSketchModified()
    {
      if (IsShapeConstructorOkay(m_csc))
        m_csc.SketchModified();
    }

    private void OnShapeConstructorChanged()
    {
      // Activate a new constructor
      if (m_csc != null)
        m_csc.Deactivate();
      m_csc = null;
      m_csc = m_edSketch.ShapeConstructor;
      if (m_csc != null)
        m_csc.Activate();
    }

    private void OnSketchFinished()
    {
      //Use the geometry from the sketch and make a narrow buffer to search for fabric lines contained within the buffer
      IBufferConstruction pBuffConst = new BufferConstruction();
      IGeometry pGeom = pBuffConst.Buffer(m_edSketch.Geometry, 0.1);
      ISpatialFilter pSpatFilter = new SpatialFilter();
      pSpatFilter.Geometry = pGeom;
      pSpatFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;

      //set up for reporting information about the lines
      string sReportString = "";
      int iTotalLineCount = 0;
      int iCurveCount = 0;
      int iMultiSegmentCount = 0;

      //Create a feature cursor to query the fabric lines table
      IFeatureCursor pFeatCurs = m_pFabricLines.Search(pSpatFilter, false);
      IFeature pFeat = pFeatCurs.NextFeature();

      while (pFeat != null)
      {
        //loop through the found lines
        //count all the lines
        iTotalLineCount++;
        ISegmentCollection pSegColl = (ISegmentCollection)pFeat.Shape;
        if (pSegColl.SegmentCount > 1) //count lines that have more than 1 segment
          iMultiSegmentCount++;
        else
        {
          if (pSegColl.get_Segment(0) is ICircularArc)
            iCurveCount++; //count lines that have a single circular arc geometry segment
        }
        Marshal.ReleaseComObject(pFeat);
        pFeat = pFeatCurs.NextFeature();
      }
      Marshal.ReleaseComObject(pFeatCurs);

      //report the results
      sReportString += "\nTotal parcel lines: " + iTotalLineCount.ToString();
      sReportString += "\nCurve parcel lines: " + iCurveCount.ToString();
      sReportString += "\nMulti segment parcel lines: " + iMultiSegmentCount.ToString();

      MessageBox.Show(sReportString, "Sample: Trace Report");
    }

  }

}
