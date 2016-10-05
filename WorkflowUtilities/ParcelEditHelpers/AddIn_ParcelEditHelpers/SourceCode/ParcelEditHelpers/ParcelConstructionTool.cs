using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace ParcelEditHelper
{

  /// <summary>
  /// A construction tool for ArcMap Editor, using shape constructors
  /// </summary>
  public partial class ParcelConstructionTool : ESRI.ArcGIS.Desktop.AddIns.Tool, IShapeConstructorTool, ISketchTool
  {
    private IEditor3 m_editor;
    private IEditEvents_Event m_editEvents;
    private IEditEvents5_Event m_editEvents5;
    private IEditSketch3 m_edSketch;
    private IShapeConstructor m_csc;

    public ParcelConstructionTool()
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
      m_edSketch = m_editor as IEditSketch3;

      // Activate a shape constructor based on the current sketch geometry
      if (m_edSketch.GeometryType == esriGeometryType.esriGeometryPoint | m_edSketch.GeometryType == esriGeometryType.esriGeometryMultipoint)
        m_csc = new PointConstructorClass();
      else
        m_csc = new StraightConstructorClass();

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
      m_csc.SketchModified();
    }

    private void OnShapeConstructorChanged()
    {
      // Activate a new constructor
      m_csc.Deactivate();
      m_csc = null;
      m_csc = m_edSketch.ShapeConstructor;
      if (m_csc != null)
        m_csc.Activate();
    }

    private void OnSketchFinished()
    {
      //TODO: Custom code
    }


  }

}
